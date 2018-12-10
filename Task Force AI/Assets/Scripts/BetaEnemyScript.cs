using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class BetaEnemyScript : MonoBehaviour {

	public GameObject ammoBoxPickup;
	public GameObject healthBoxPickup;

	// Enemy variables
	public int aggression;

	// Finite state machine states
	public enum ActionStates {Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking};
	// FSM used for determining movement while attacking and not in cover
	enum FiringStates {StandingStill, StrafeLeft, StrafeRight, Backpedal, Forward};

	// Enemy combat style
	public enum EnemyType {Patrol, Scout};

	// Gun stuff
	private AudioSource audioSource;

	public float range;
	public int bulletsPerMag = 30;
	public int currentBullets;
	public AudioClip shootSound;

	public Transform shootPoint;
	public ParticleSystem muzzleFlash;
	private bool isReloading = false;

	public GameObject hitParticles;
	public GameObject bulletImpact;
	public GameObject bloodEffect;
	public GameObject bloodEffectHeadshot;

	public float fireRate = 0.4f;
	public float damage = 10f;

	public EnemyType enemyType;

	// Once it equals fireRate, it will allow us to shoot
	float fireTimer = 0.0f;

	//-----------------------------------------------------

	private GameObject player;
	private GameObject playerToHit;
	public Vector3 lastSeenPlayerPos = Vector3.negativeInfinity;
	private Animator animator;
	public int health;
	private Rigidbody rigid;

	private float rotationSpeed = 6f;

	// All patrol pathfinding points for an enemy
	public GameObject[] navPoints;
	private Vector3 spawnPos;
	private Quaternion spawnRot;

	public ActionStates actionState;

	private FiringStates firingState;
	private bool isCrouching;

	private Transform spine;

	private NavMeshAgent navMesh;

	// Timers
	private float alertTimer;
	// Time in cover
	private float coverTimer = 0f;
	// Time to wait to be in cover again
	private float coverWaitTimer = 0f;
	// Time to wait to maneuver to another cover position
	private float coverSwitchPositionsTimer = 0f;
	// Time to change firing positions
	private float firingModeTimer = 0f;

	private float wanderStallDelay = -1f;
	private bool inCover;
	private Vector3 coverPos;
	private int crouchMode = 2;
	private float coverScanRange = 50f;

	// Collision
	private CapsuleCollider myCollider;
	private float originalColliderHeight = 0f;
	private float originalColliderRadius = 0f;
	private Vector3 originalColliderCenter;

	public bool alerted = false;
	private GameObject[] players;

	// Testing mode - set in inspector
	//public bool testingMode;

	private PhotonView pView;
	public Transform headTransform;

	// Use this for initialization
	void Start () {
		if (PhotonNetwork.IsMasterClient) {
			alertTimer = -100f;
			coverWaitTimer = Random.Range (2f, 7f);
			coverSwitchPositionsTimer = Random.Range (12f, 18f);
		}

		player = null;
		spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		spawnRot = Quaternion.Euler (transform.rotation.x, transform.rotation.y, transform.rotation.z);
		spine = GetComponentInChildren<SpineScript> ().gameObject.transform;
		animator = GetComponent<Animator> ();
		players = new GameObject[8];
		health = 100;
		currentBullets = bulletsPerMag;
		audioSource = GetComponent<AudioSource> ();
		rigid = GetComponent<Rigidbody> ();
		rigid.freezeRotation = true;
		isCrouching = false;
		// Get nav points
		if (enemyType != EnemyType.Scout) {
			navMesh = GetComponent<NavMeshAgent>();
			if (!navMesh.isOnNavMesh) {
				enemyType = EnemyType.Scout;
			}
		}
		//navPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
		coverTimer = 0f;
		inCover = false;
		pView = GetComponent<PhotonView> ();

		myCollider = GetComponent<CapsuleCollider> ();
		originalColliderHeight = myCollider.height;
		originalColliderRadius = myCollider.radius;
		originalColliderCenter = new Vector3 (myCollider.center.x, myCollider.center.y, myCollider.center.z);

		if (enemyType == EnemyType.Patrol) {
			range = 10f;
		} else {
			range = 27f;
		}

	}

	// Update is called once per frame
	void Update () {
		if (isCrouching) {
			myCollider.height = 0.97f;
			myCollider.radius = 0.32f;
			myCollider.center = new Vector3 (-0.05f, 0.43f, -0.03f);
		} else {
			myCollider.height = originalColliderHeight;
			myCollider.radius = originalColliderRadius;
			myCollider.center = originalColliderCenter;
		}

		if (!PhotonNetwork.IsMasterClient || animator.GetCurrentAnimatorStateInfo(0).IsName("Die") || animator.GetCurrentAnimatorStateInfo(0).IsName("DieHeadshot")) {
			if (enemyType != EnemyType.Scout) {
				navMesh.isStopped = true;
			}
			return;
		}
		if (!Vector3.Equals (GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
			if (!alerted) {
				pView.RPC ("RpcSetAlerted", RpcTarget.All, true);
				alertTimer = 12f;
			}
		}

		CheckTargetDead ();

		/**Debug.Log (navMesh.destination);
		Debug.Log (actionState);
		Debug.Log ("Pos: " + transform.position);
		Debug.Log (navMesh.speed);
		Debug.Log (navMesh.isStopped);*/

		//Debug.DrawRay (transform.position, transform.forward * range, Color.blue);

		if (enemyType == EnemyType.Patrol) {
			DecideActionPatrolInCombat();
			DecideActionPatrol ();
			HandleMovementPatrol ();
		} else {
			DecideActionScout ();
		}

		// Shoot at player
		// Add !isCrouching if you don't want the AI to fire while crouched behind cover
		if (actionState == ActionStates.Firing || (actionState == ActionStates.InCover && player != null)) {
			if (currentBullets > 0) {
				Fire ();
			}
		}

		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}

		//Debug.Log ("Spine: " + spine.transform.rotation.x + "," + spine.transform.rotation.y + "," + spine.transform.rotation.z);

		if (alertTimer > 0f) {
			alertTimer -= Time.deltaTime;
		}

		if (firingModeTimer > 0f) {
			firingModeTimer -= Time.deltaTime;
		}

		//Debug.Log (isReloading);
		//Debug.Log ("Firing State: " + firingState);
		//Debug.Log ("Action State: " + actionState);

		// Scout stuff
		/**if (coverTimer > 0f && actionState != ActionStates.Idle) {
			coverTimer -= Time.deltaTime;
		}

		if (coverWaitTimer > 0f && actionState != ActionStates.Idle) {
			coverWaitTimer -= Time.deltaTime;
		}*/

		//Debug.Log (player == null);
		//Debug.Log (lastSeenPlayerPos);
		//Debug.Log (coverWaitTimer + " " + inCover);
	}

	void FixedUpdate() {
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die") || animator.GetCurrentAnimatorStateInfo(0).IsName("DieHeadshot"))
			return;
		// Handle animations independent of frame rate
		DecideAnimation ();
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void LateUpdate() {
		if (!PhotonNetwork.IsMasterClient || health <= 0)
			return;
		// If the enemy sees the player, rotate the enemy towards the player
		RotateTowardsPlayer();

	}

	// What happens when the enemy is alerted
	[PunRPC]
	void RpcSetAlerted(bool b) {
		//Debug.Log ("h");
		alerted = b;
		if (range == 10f) {
			range *= 2.5f;
		} else if ((range / 2.5f) == 10f) {
			range = 10f;
		}
	}

	[PunRPC]
	void RpcSetIsCrouching(bool b)
	{
		//Debug.Log ("i");
		isCrouching = b;
	}

	void RotateTowardsPlayer() {
		if (player != null) {
			Vector3 rotDir = (player.transform.position - transform.position).normalized;
			Quaternion lookRot = Quaternion.LookRotation (rotDir);
			Quaternion tempQuat = Quaternion.Slerp (transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
			Vector3 tempRot = tempQuat.eulerAngles;
			//tempRot = new Vector3 (0f, tempRot.y, 0f);
			transform.rotation = Quaternion.Euler (new Vector3 (0f, tempRot.y, 0f));
			//spine.transform.localRotation = Quaternion.Euler (new Vector3 (tempRot.x, 0f, 0f));
			spine.transform.forward = new Vector3(spine.transform.forward.x, player.transform.position.y - spine.transform.position.y + 0.3f, spine.transform.forward.z);
		}
	}

	void HandleMovementPatrol() {
		// Melee attack trumps all
		if (actionState == ActionStates.Melee) {
			navMesh.isStopped = true;
			return;
		}
		// Handle movement for wandering
		if (actionState == ActionStates.Wander) {
			if (PhotonNetwork.IsMasterClient) {
				navMesh.speed = 1.5f;
				// Only server should be updating the delays and they should sync across the network
				// Initial spawn value
				if (wanderStallDelay == -1f) {
					wanderStallDelay = Random.Range (0f, 7f);
				}
				// Take away from the stall delay if the enemy is standing still
				if (navMesh.isStopped) {
					//					Debug.Log ("enemy stalled");
					wanderStallDelay -= Time.deltaTime;
				} else {
					// Else, check if the enemy has reached its destination
					if (navMeshReachedDestination (0.3f)) {
						//						Debug.Log ("enemy reached dest");
						navMesh.isStopped = true;
					}
				}
				// If the stall delay is done, the enemy needs to move to a wander point
				if (wanderStallDelay < 0f && navMesh.isStopped) {
					int r = Random.Range (0, navPoints.Length);
					RotateTowards (navPoints [r].transform.position);
					navMesh.SetDestination (navPoints [r].transform.position);
					navMesh.isStopped = false;
					wanderStallDelay = Random.Range (0f, 7f);
				}
			}
		}

		if (actionState == ActionStates.Idle) {
			navMesh.isStopped = true;
			wanderStallDelay = -1f;
		}

		if (actionState == ActionStates.Dead || actionState == ActionStates.InCover) {
			navMesh.isStopped = true;
		}

		if (actionState == ActionStates.Pursue && !lastSeenPlayerPos.Equals(Vector3.negativeInfinity)) {
			navMesh.speed = 6f;
			navMesh.isStopped = false;
			navMesh.SetDestination (lastSeenPlayerPos);
			lastSeenPlayerPos = Vector3.negativeInfinity;
			return;
		}

		if (actionState == ActionStates.Seeking) {
			// Seek behavior: use navMesh to move towards the last area of gunshot. If the enemy moves towards that location
			// and there's nobody there, go back to wandering the area

			if (navMesh.isStopped) {
				//RotateTowards (GameControllerTestScript.lastGunshotHeardPos);
				navMesh.SetDestination (GameControllerScript.lastGunshotHeardPos);
				navMesh.isStopped = false;
				if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint")) {
					navMesh.speed = 6f;
				} else {
					navMesh.speed = 4f;
				}
			}
		}

		if (actionState == ActionStates.TakingCover) {
			// If the enemy is not near the cover spot, run towards it
			if (!coverPos.Equals (Vector3.negativeInfinity)) {
				//navMesh.isStopped = true;
				navMesh.speed = 6f;
				navMesh.isStopped = false;
				navMesh.SetDestination (coverPos);
				coverPos = Vector3.negativeInfinity;
				//navMesh.stoppingDistance = 0.5f;
			} else {
				// If the enemy has finally reached cover, then he will get into cover mode
				if (navMeshReachedDestination(0f)) {
					// Done
					navMesh.isStopped = true;
					inCover = true;
					if (actionState != ActionStates.InCover) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.InCover);
					}
				}
			}
		}

		if (actionState == ActionStates.Firing && !inCover) {
			navMesh.speed = 4f;
			if (firingModeTimer <= 0f) {
				int r = Random.Range (0, 5);
				if (r == 0) {
					pView.RPC ("RpcUpdateFiringState", RpcTarget.AllBuffered, FiringStates.StandingStill);
					firingModeTimer = Random.Range (2f, 3.2f);
				} else if (r == 1) {
					pView.RPC ("RpcUpdateFiringState", RpcTarget.AllBuffered, FiringStates.Forward);
					firingModeTimer = Random.Range (2f, 3.2f);
					navMesh.speed = 4f;
				} else if (r == 2) {
					pView.RPC ("RpcUpdateFiringState", RpcTarget.AllBuffered, FiringStates.Backpedal);
					navMesh.speed = 3f;
				} else if (r == 3) {
					pView.RPC ("RpcUpdateFiringState", RpcTarget.AllBuffered, FiringStates.StrafeLeft);
					firingModeTimer = 1.7f;
					navMesh.speed = 2.5f;
				} else if (r == 4) {
					pView.RPC ("RpcUpdateFiringState", RpcTarget.AllBuffered, FiringStates.StrafeRight);
					firingModeTimer = 1.7f;
					navMesh.speed = 2.5f;
				}
			}

			if (firingState == FiringStates.StandingStill) {
				navMesh.isStopped = true;
			}

			if (player != null) {
				RotateTowardsPlayer ();
				if (firingState == FiringStates.Forward) {
					navMesh.Move (transform.forward * 2f);
					//navMesh.SetDestination (player.transform.position);
					//navMesh.isStopped = false;
				}

				if (firingState == FiringStates.Backpedal) {
					//Vector3 oppositeDirVector = player.transform.position - transform.position;
					navMesh.Move (transform.forward * -2f);
					//navMesh.SetDestination (new Vector3(transform.position.x, transform.position.y, transform.position.z - 5f));
					//navMesh.SetDestination (new Vector3 (-oppositeDirVector.x, oppositeDirVector.y, -oppositeDirVector.z));
					//navMesh.isStopped = false;
				}

				if (firingState == FiringStates.StrafeLeft) {
					Vector3 dest = new Vector3 (transform.right.x * navMesh.speed * 2f, transform.right.y * navMesh.speed * 2f, transform.right.z * navMesh.speed * 2f);
					navMesh.Move (dest);
					//navMesh.SetDestination (new Vector3(transform.position.x + dest.x, transform.position.y + dest.y, transform.position.z + dest.z));
					//navMesh.isStopped = false;
				}

				if (firingState == FiringStates.StrafeRight) {
					Vector3 dest = new Vector3 (transform.right.x * -navMesh.speed * 2f, transform.right.y * -navMesh.speed * 2f, transform.right.z * -navMesh.speed * 2f);
					navMesh.Move (dest);
					//navMesh.SetDestination (new Vector3(transform.position.x + dest.x, transform.position.y + dest.y, transform.position.z + dest.z));
					//navMesh.isStopped = false;
				}
			}
		}
	}

	// Action Decision while in combat
	void DecideActionPatrolInCombat() {
		// Action Decision while in combat
		//Debug.Log("Cover wait timer: " + coverWaitTimer);
		//Debug.Log ("Cover switch positions timer: " + coverSwitchPositionsTimer);
		if (actionState == ActionStates.InCover || actionState == ActionStates.Firing) {
			if (navMesh.isStopped) {
				coverWaitTimer -= Time.deltaTime;
			}
			if (inCover) {
				coverSwitchPositionsTimer -= Time.deltaTime;
			}

			// Three modes in cover - defensive, offensive, maneuvering; only used when engaging a player
			if (player != null) {
				// If the cover wait timer has ran out, switch from defensive to offensive and vice versa
				if (coverWaitTimer <= 0f && !isReloading) {
					pView.RPC ("RpcSetIsCrouching", RpcTarget.All, !isCrouching);
					coverWaitTimer = Random.Range (2f, 7f);
				}
				// Maneuvering through cover; if the maneuver timer runs out, it's time to move to another cover position
				// TODO: Broken - coverswitch timer is never reset
				/**if (coverSwitchPositionsTimer <= 0f) {
					bool coverFound = DynamicTakeCover ();
					if (coverFound) {
						inCover = false;
						pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.TakingCover);
					} else {
						coverPos = Vector3.negativeInfinity;
						pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.InCover);
					}
				}*/
			}

			//Debug.Log ("inCover: " + inCover);
		}
	}

	[PunRPC]
	void RpcPlaySound(string s) {
		//Debug.Log ("j");
		AudioClip a = (AudioClip)Resources.Load(s);
		audioSource.clip = a;
		audioSource.Play ();
	}

	[PunRPC]
	void RpcDie() {
		//Debug.Log ("k");
		GetComponentInChildren<SpriteRenderer> ().enabled = false;
		//GetComponent<CapsuleCollider> ().isTrigger = true;
	}

	// Decision tree for scout type enemy
	void DecideActionScout() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead) {
			// Spawn a drop box
			int r = Random.Range (1,12);
			if (r == 6) {
				// 1/6 chance of getting a health box
				Debug.Log("health pickup spawned");
				PhotonNetwork.Instantiate(healthBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			} else if (r > 1 && r < 6) {
				// 1/3 chance of getting ammo box
				Debug.Log("ammo pickup spawned");
				PhotonNetwork.Instantiate(ammoBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			}

			pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Dead);
			// Choose a death sound
			r = Random.Range (0, 3);
			if (r == 0) {
				pView.RPC ("RpcPlaySound", RpcTarget.All, "Grunts/grunt1");
			} else if (r == 1) {
				pView.RPC ("RpcPlaySound", RpcTarget.All, "Grunts/grunt2");
			} else {
				pView.RPC ("RpcPlaySound", RpcTarget.All, "Grunts/grunt4");
			}

			//pView.RPC ("RpcDie", RpcTarget.All);

			StartCoroutine(Despawn ());
			return;
		}

		// Melee attack trumps all
		if (actionState == ActionStates.Melee) {
			return;
		}

		// Continue with decision tree
		PlayerScan();
		// Sees a player?
		if (player != null) {
			alertTimer = 10f;

			if (Vector3.Distance (player.transform.position, transform.position) <= 2.3f) {
				if (actionState != ActionStates.Melee) {
					pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Melee);
				}
			} else {
				if (currentBullets > 0) {
					if (actionState != ActionStates.Firing) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Firing);
					}
					if (crouchMode == 0)
						crouchMode = 1;
					else if (crouchMode == 1)
						crouchMode = 2;
					TakeCoverScout ();
				} else {
					if (actionState != ActionStates.Reloading) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Reloading);
					}
					crouchMode = 0;
					TakeCoverScout ();
				}
			}
		} else {
			if (alertTimer > 0f) {
				crouchMode = 0;
				TakeCoverScout ();
			} else {
				crouchMode = 1;
				TakeCoverScout ();
			}
			if (actionState != ActionStates.Idle) {
				pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Idle);
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (!PhotonNetwork.IsMasterClient) {
			return;
		}
		if (!other.gameObject.tag.Equals ("Player")) {
			return;
		}
		// Don't consider dead players
		if (other.gameObject.GetComponent<PlayerScript> ().health <= 0f) {
			return;
		}

		// If the player enters the enemy's sight range, determine if the player is in the right angle. If he is and there is no current player to target, then
		// assign the player and stop searching
		float dist = Vector3.Distance(transform.position, other.transform.position);
		if (dist < 2.3f) {
			if (!alerted) {
				pView.RPC ("RpcSetAlerted", RpcTarget.AllBuffered, true);
			}

			if (actionState != ActionStates.Melee) {
				pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Melee);
			}
			playerToHit = other.gameObject;

		}
	}

	// Decision tree for patrol type enemy
	void DecideActionPatrol() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead)
		{
			int r = Random.Range (1,12);
			if (r == 6) {
				// 1/6 chance of getting a health box
				Debug.Log("health pickup spawned");
				PhotonNetwork.Instantiate(healthBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			} else if (r > 1 && r < 6) {
				// 1/3 chance of getting ammo box
				Debug.Log("ammo pickup spawned");
				PhotonNetwork.Instantiate(ammoBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			}

			navMesh.isStopped = true;
			if (actionState != ActionStates.Dead) {
				pView.RPC ("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Dead);
			}
			// Choose a death sound
			r = Random.Range(0, 3);
			if (r == 0)
			{
				pView.RPC("RpcPlaySound", RpcTarget.All, "Grunts/grunt1");
			}
			else if (r == 1)
			{
				pView.RPC("RpcPlaySound", RpcTarget.All, "Grunts/grunt2");
			}
			else
			{
				pView.RPC("RpcPlaySound", RpcTarget.All, "Grunts/grunt4");
			}

			//pView.RPC("RpcDie", RpcTarget.AllBuffered);

			StartCoroutine(Despawn());
			return;
		}

		// Melee attack trumps all
		if (actionState == ActionStates.Melee) {
			return;
		}

		PlayerScan ();
		// Root - is the enemy alerted by any type of player presence (gunshots, sight, getting shot, other enemies alerted nearby)
		if (alerted) {
			if (player != null) {
				// If the enemy has seen a player
				alertTimer = 12f;
				if (actionState != ActionStates.Firing && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Pursue && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, aggression - 2);
					if (r <= 2) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.InCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.InCover);
							}
						}
					} else {
						if (actionState != ActionStates.Firing) {
							pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Firing);
						}
					}
				}
			} else {
				// If the enemy has not seen a player
				UpdateAlertedStatus();
				if (actionState != ActionStates.Seeking && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Firing && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, aggression - 1);
					if (r <= 2) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.InCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.InCover);
							}
						}
					} else {
						if (actionState != ActionStates.Seeking) {
							pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Seeking);
						}
					}
				}

				if (actionState == ActionStates.Seeking) {
					if (navMeshReachedDestination (0.5f) && player == null) {
						pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Wander);
					}
				}

				// If the enemy has seen a player before but no longer does, then possibly (60% chance) pursue the player or take cover (40% chance)
				if (actionState == ActionStates.Firing) {
					if (!Vector3.Equals (lastSeenPlayerPos, Vector3.negativeInfinity)) {
						int r = Random.Range (1, aggression);
						if (r <= 2) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.Pursue) {
								pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Pursue);
							}
						}
					} else {
						pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Idle);
					}
				}

				// If the enemy was in pursuit of a player but has lost track of him, then go back to wandering
				if (actionState == ActionStates.Pursue && Vector3.Equals(lastSeenPlayerPos, Vector3.negativeInfinity)) {
					if (navMeshReachedDestination(0.5f)) {
						pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Wander);
					}
				}

				// If the enemy is in cover, stay there for a while and then go back to seeking the last gunshot position, or wandering if there isn't one
				if (actionState == ActionStates.InCover) {
					coverSwitchPositionsTimer -= Time.deltaTime;
					if (coverSwitchPositionsTimer <= 0f) {
						coverSwitchPositionsTimer = Random.Range (6f, 10f);
						if (GameControllerScript.lastGunshotHeardPos != Vector3.negativeInfinity) {
							pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Seeking);
						} else {
							pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Wander);
						}
					}
				}
			}
		} else {
			// Else, wander around the patrol points until alerted or enemy seen
			if (actionState != ActionStates.Wander) {
				pView.RPC("RpcUpdateActionState", RpcTarget.AllBuffered, ActionStates.Wander);
			}
			if (player != null && !alerted) {
				pView.RPC("RpcSetAlerted", RpcTarget.AllBuffered, true);
			}
		}
	}

	void DecideAnimation() {
		if (actionState == ActionStates.Seeking) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving") && !animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint")) {
				int r = Random.Range (1, 4);
				if (r >= 1 && r <= 2) {
					animator.Play ("Moving");
				} else {
					animator.Play ("Sprint");
				}
			}
		}

		if (actionState == ActionStates.Wander) {
			if (navMesh.isStopped) {
				if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
					animator.Play ("Idle");
			} else {
				if (!alerted && !animator.GetCurrentAnimatorStateInfo (0).IsName ("Walk")) {
					animator.Play ("Walk");
				} else if (alerted && !animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving")) {
					animator.Play ("Moving");
				}
			}

		}

		if (actionState == ActionStates.TakingCover) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint"))
				animator.Play ("Sprint");
		}

		if (actionState == ActionStates.Dead) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Die") && !animator.GetCurrentAnimatorStateInfo (0).IsName ("DieHeadshot")) {
				// TODO: Later change headshot death to when hit in the head
				int r = Random.Range (1, 3);
				if (r == 1) {
					animator.Play ("Die");
				} else {
					animator.Play ("DieHeadshot");
				}
			}
		}

		if (actionState == ActionStates.Pursue) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint"))
				animator.Play ("Sprint");
		}

		if (actionState == ActionStates.Idle) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
				animator.Play ("Idle");
		}

		if (actionState == ActionStates.Firing || actionState == ActionStates.Reloading || actionState == ActionStates.InCover) {
			//Debug.Log ("isCrouching: " + isCrouching);
			// Set proper animation
			if (actionState == ActionStates.Firing && currentBullets > 0) {
				if (firingState == FiringStates.StandingStill) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Firing"))
						animator.Play ("Firing");
				} else if (firingState == FiringStates.Forward) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving"))
						animator.Play ("Moving");
				} else if (firingState == FiringStates.Backpedal) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Backpedal"))
						animator.Play ("Backpedal");
				} else if (firingState == FiringStates.StrafeLeft) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("StrafeLeft"))
						animator.Play ("StrafeLeft");
				} else if (firingState == FiringStates.StrafeRight) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("StrafeRight"))
						animator.Play ("StrafeRight");
				}
			} else if (actionState == ActionStates.InCover && currentBullets > 0) {
				navMesh.isStopped = true;
				if (isCrouching) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Crouching"))
						animator.Play ("Crouching");
				} else {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Aim"))
						animator.Play ("Aim");
				}
			} else if (currentBullets <= 0) {
				if (enemyType != EnemyType.Scout) {
					navMesh.isStopped = true;
				}
				if (isCrouching) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("CrouchReload"))
						animator.Play ("CrouchReload");
				} else {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Reloading"))
						animator.Play ("Reloading");
				}
			}
		}

		if (actionState == ActionStates.Melee) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Melee")) {
				animator.Play ("Melee");
			}
		}
	}

	private void Fire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading)
			return;

		GameControllerScript.lastGunshotHeardPos = transform.position;
		if (player != null) {
			RaycastHit hit;
			// Locks onto the player and shoots at him
			Vector3 dir = player.GetComponentsInChildren<Transform>()[0].position - shootPoint.position;

			// Adding artificial stupidity - ensures that the player isn't hit every time by offsetting
			// the shooting direction in x and y by two random numbers
			float xOffset = Random.Range (-3f, 3f);
			float yOffset = Random.Range (-3f, 3f);
			dir = new Vector3 (dir.x + xOffset, dir.y + yOffset, dir.z);
			//Debug.DrawRay (shootPoint.position, dir * range, Color.red);
			if (Physics.Raycast (shootPoint.position, dir, out hit)) {
				GameObject bloodSpill = null;
				if (hit.transform.tag.Equals ("Player") || hit.transform.tag.Equals ("Human")) {
					pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal);

					//Debug.Log (transform.name + " has hit you");
					if (hit.transform.tag.Equals ("Player")) {
						hit.transform.GetComponent<PlayerScript>().TakeDamage((int)damage);
						hit.transform.GetComponent<PlayerScript> ().ResetHitTimer ();
						hit.transform.GetComponent<PlayerScript> ().SetHitLocation (transform.position);
					} else {
						hit.transform.GetComponent<BetaEnemyScript>().TakeDamage((int)damage);
					}
				} else {
					pView.RPC ("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
					pView.RPC ("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
				}
			}
		}

		//animator.CrossFadeInFixedTime ("Firing", 0.01f);
		pView.RPC("RpcShootAction", RpcTarget.All);
	}

	[PunRPC]
	void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal) {
		//Debug.Log ("a");
		GameObject bloodSpill = Instantiate(bloodEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bloodSpill.transform.Rotate (180f, 0f, 0f);
		Destroy (bloodSpill, 1.5f);
	}

	[PunRPC]
	void RpcInstantiateBulletHole(Vector3 point, Vector3 normal, string parentName) {
		//Debug.Log ("b");
		GameObject bulletHoleEffect = Instantiate (bulletImpact, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bulletHoleEffect.transform.SetParent (GameObject.Find(parentName).transform);
		Destroy (bulletHoleEffect, 3f);
	}


	[PunRPC]
	void RpcInstantiateHitParticleEffect(Vector3 point, Vector3 normal) {
		//Debug.Log ("c");
		GameObject hitParticleEffect = Instantiate (hitParticles, point, Quaternion.FromToRotation (Vector3.up, normal));
		Destroy (hitParticleEffect, 1f);
	}

	[PunRPC]
	void RpcShootAction() {
		//Debug.Log ("d");
		muzzleFlash.Play();
		PlayShootSound();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}

	private void PlayShootSound() {
		GetComponentInChildren<AudioSource>().PlayOneShot (shootSound);
	}

	public void Reload() {
		int bulletsToLoad = bulletsPerMag - currentBullets;
		currentBullets += bulletsPerMag;
	}

	public void MeleeAttack() {
		if (playerToHit != null) {
			playerToHit.GetComponent<PlayerScript> ().health -= 50;
			playerToHit.GetComponent<PlayerScript> ().hitTimer = 0f;
			playerToHit = null;
		}
	}

	private void RotateTowards(Vector3 r) {
		Vector3 rotDir = (r - transform.position).normalized;
		Quaternion lookRot = Quaternion.LookRotation (rotDir);
		Quaternion tempQuat = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
		Vector3 tempRot = tempQuat.eulerAngles;
		tempRot = new Vector3 (0f, tempRot.y, 0f);
		transform.rotation = Quaternion.Euler(tempRot);
	}

	IEnumerator Despawn() {
		gameObject.layer = 12;
		RpcRemoveHitboxes ();
		pView.RPC ("RpcRemoveHitboxes", RpcTarget.All);
		yield return new WaitForSeconds(5f);
		pView.RPC ("RpcDespawn", RpcTarget.All);
		StartCoroutine ("Respawn");
	}

	[PunRPC]
	void RpcDespawn() {
		SkinnedMeshRenderer[] s = GetComponentsInChildren<SkinnedMeshRenderer> ();
		for (int i = 0; i < s.Length; i++) {
			s [i].enabled = false;
		}
		GetComponentInChildren<SpriteRenderer> ().enabled = false;
		GetComponentInChildren<MeshRenderer> ().enabled = false;
	}

	[PunRPC]
	void RpcRemoveHitboxes() {
		GetComponent<CapsuleCollider>().enabled = false;
		GetComponentInChildren<CapsuleCollider> ().enabled = false;
		GetComponent<BoxCollider>().enabled = false;
	}

	// b is the mode the AI is in. 0 means override everything and take cover, 1 is override everything and leave cover
	// 2 is use the natural timer to decide
	// Used for Scout AI
	void TakeCoverScout() {
		if (crouchMode == 0) {
			coverWaitTimer = 0f;
			inCover = true;
		} else if (crouchMode == 1) {
			if (coverWaitTimer <= 0f) coverWaitTimer = Random.Range (4f, 15f);
			inCover = false;
		} else {
			if (coverWaitTimer <= 0f && !inCover) {
				coverTimer = Random.Range (3f, 7f);
				inCover = true;
			} else if (coverTimer <= 0f && inCover) {
				coverWaitTimer = Random.Range (4f,15f);
				inCover = false;
			}
		}
	}

	// Take cover algorithm for moving enemies - returns true if cover was found, false if not
	bool DynamicTakeCover() {
		// Scan for cover first
		Collider[] nearbyCover = Physics.OverlapBox(transform.position, new Vector3(coverScanRange, 20f, coverScanRange));
		if (nearbyCover == null || nearbyCover.Length == 0) {
			return false;
		}
		// If cover is nearby, find the closest one
		if (nearbyCover.Length == 0) {
			return false;
		}
		//ArrayList coverIndices = new ArrayList ();
		int minCoverIndex = -1;
		for (int i = 0; i < nearbyCover.Length; i++) {
			if (nearbyCover [i].gameObject.tag.Equals ("Cover")) {
				if (minCoverIndex == -1) {
					minCoverIndex = i;
				} else {
					if (Vector3.Distance (transform.position, nearbyCover [i].transform.position) < Vector3.Distance (transform.position, nearbyCover [minCoverIndex].transform.position)) {
						minCoverIndex = i;
					}
				}
				//coverIndices.Add (i);
			}
		}
		/**if (coverIndices.Count == 0) {
			return false;
		}*/

		//int minCoverIndex = (int)coverIndices [Random.Range (0, coverIndices.Count - 1)];
		if (minCoverIndex == -1) {
			return false;
		}

		// Once the closest cover is found, set the AI to be in cover, pick a cover side opposite of the player and run to it
		// If there is no target player, just choose a random cover
		Transform[] coverSpots = nearbyCover [minCoverIndex].gameObject.GetComponentsInChildren<Transform>();
		if (player == null) {
			coverPos = coverSpots [Random.Range (0, coverSpots.Length)].position;
		} else {
			// Determines if a unique cover spot was found or not
			bool foundOne = false;
			for (int i = 0; i < coverSpots.Length; i++) {
				// Don't want to hide in the same place again
				if (Vector3.Distance (transform.position, coverSpots[i].position) <= 0.5f) {
					continue;
				}
				// If there's something blocking the player and the enemy, then the enemy wants to hide behind it
				if (Physics.Linecast (coverSpots[i].position, player.transform.position)) {
					coverPos = coverSpots [i].position;
					foundOne = true;
					break;
				}
			}
			// If a unique cover spot wasn't found, then just choose a random spot
			if (!foundOne) {
				coverPos = coverSpots [Random.Range (0, coverSpots.Length)].position;
			}
		}
		return true;
	}

	private bool navMeshReachedDestination(float bufferRange) {
		if (!navMesh.pathPending && !navMesh.isStopped) {
			if (navMesh.remainingDistance <= (navMesh.stoppingDistance + bufferRange)) {
				if (!navMesh.hasPath || navMesh.velocity.sqrMagnitude == 0f) {
					// Done
					return true;
				}
			}
		}
		return false;
	}

	void PlayerScan() {
		// If we do not have a target player, try to find one
		if (player == null || player.GetComponent<PlayerScript>().health <= 0) {
			ArrayList keysNearBy = new ArrayList ();
			foreach (GameObject p in GameControllerScript.playerList.Values) {
				if (!p || p.GetComponent<PlayerScript>().health <= 0)
					continue;
				if (Vector3.Distance (transform.position, p.transform.position) < range + 20f) {
					Vector3 toPlayer = p.transform.position - transform.position;
					float angleBetween = Vector3.Angle (transform.forward, toPlayer);
					if (angleBetween <= 60f) {
						// Cast a ray to make sure there's nothing in between the player and the enemy
						Debug.DrawRay (headTransform.position, toPlayer, Color.blue);
						RaycastHit hit1;
						RaycastHit hit2;
						Vector3 middleHalfCheck = new Vector3 (p.transform.position.x, p.transform.position.y, p.transform.position.z);
						Vector3 topHalfCheck = new Vector3 (p.transform.position.x, p.transform.position.y + 0.4f, p.transform.position.z);
						Physics.Linecast (headTransform.position, middleHalfCheck, out hit2);
						Physics.Linecast (headTransform.position, topHalfCheck, out hit1);
						if (!hit1.transform.gameObject.tag.Equals ("Player") && !hit2.transform.gameObject.tag.Equals ("Player")) {
							continue;
						}
						keysNearBy.Add (p.GetComponent<PhotonView>().OwnerActorNr);
					}
				}
			}
			if (keysNearBy.Count != 0)
				player = (GameObject)GameControllerScript.playerList [(int)keysNearBy[Random.Range (0, keysNearBy.Count)]];
		} else {
			// If we do, check if it's still in range
			if (Vector3.Distance (transform.position, player.transform.position) >= range + 20f) {
				lastSeenPlayerPos = player.transform.position;
				player = null;
			}
		}
	}

	public void TakeDamage(int d) {
		pView.RPC ("RpcTakeDamage", RpcTarget.AllBuffered, d);
	}

	[PunRPC]
	public void RpcTakeDamage(int d) {
		//Debug.Log ("e");
		health -= d;
	}

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action) {
		//Debug.Log ("f");
		actionState = action;
	}

	[PunRPC]
	private void RpcUpdateFiringState(FiringStates firing) {
		//Debug.Log ("g");
		firingState = firing;
	}

	void CheckTargetDead() {
		if (player != null && player.GetComponent<PlayerScript> ().health <= 0f) {
			player = null;
		}
	}

	void UpdateAlertedStatus() {
		if (alertTimer <= 0f && alertTimer != -100f && alerted) {
			pView.RPC ("RpcSetAlerted", RpcTarget.All, false);
			GameControllerScript.lastGunshotHeardPos = Vector3.negativeInfinity;
			player = null;
			lastSeenPlayerPos = Vector3.negativeInfinity;
		}
	}

	// Reset values to respawn
	IEnumerator Respawn() {
		yield return new WaitForSeconds (25f);
		pView.RPC ("RpcRespawn", RpcTarget.All);
	}

	[PunRPC]
	void RpcRespawn() {
		gameObject.layer = 0;
		health = 100;
		transform.position = new Vector3 (spawnPos.x, spawnPos.y, spawnPos.z);
		transform.rotation = Quaternion.Euler (spawnRot.x, spawnRot.y, spawnRot.z);
		coverWaitTimer = Random.Range (2f, 7f);
		coverSwitchPositionsTimer = Random.Range (6f, 10f);
		player = null;
		currentBullets = bulletsPerMag;
		isCrouching = false;

		coverTimer = 0f;
		inCover = false;
		isReloading = false;
		fireTimer = 0.0f;

		playerToHit = null;
		lastSeenPlayerPos = Vector3.negativeInfinity;

		actionState = ActionStates.Idle;
		firingState = FiringStates.StandingStill;
		firingModeTimer = 0f;

		wanderStallDelay = -1f;
		coverPos = Vector3.negativeInfinity;
		crouchMode = 2;
		coverScanRange = 50f;

		GetComponent<CapsuleCollider>().enabled = true;
		GetComponent<BoxCollider>().enabled = true;
		GetComponentInChildren<CapsuleCollider>().enabled = true;
		SkinnedMeshRenderer[] s = GetComponentsInChildren<SkinnedMeshRenderer> ();
		for (int i = 0; i < s.Length; i++) {
			s [i].enabled = true;
		}
		GetComponentInChildren<SpriteRenderer> ().enabled = true;
		GetComponentInChildren<MeshRenderer> ().enabled = true;
	}

}
