using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class BetaEnemyScript : MonoBehaviour {
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

	public float fireRate = 0.4f;
	public float damage = 10f;

	public EnemyType enemyType;

	// Once it equals fireRate, it will allow us to shoot
	float fireTimer = 0.0f;

	//-----------------------------------------------------

	private GameObject player;
	private GameObject playerToHit;
	private Transform lastSeenPlayerPos;
	private Animator animator;
	public int health;
	private Rigidbody rigid;

	private float rotationSpeed = 6f;

	// All patrol pathfinding points for an enemy
	public GameObject[] navPoints;
	// TODO: SyncVar
	public ActionStates actionState;
	// TODO: SyncVar
	private FiringStates firingState;
	private bool isCrouching;

	private Transform spine;

	private GameObject[] players;

	private NavMeshAgent navMesh;
	private AudioSource aud;

	// Timers
	private float alertTimer = 0f;
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

	private bool alerted = false;

	// Testing mode - set in inspector
	public bool testingMode;

	private PhotonView pView;

	// Use this for initialization
	void Start () {

		player = null;
		spine = GetComponentInChildren<SpineScript> ().gameObject.transform;
		animator = GetComponent<Animator> ();
		players = new GameObject[8];
		health = 100;
		currentBullets = bulletsPerMag;
		audioSource = GetComponent<AudioSource> ();
		rigid = GetComponent<Rigidbody> ();
		isCrouching = false;
		// Get nav points
		navMesh = GetComponent<NavMeshAgent>();
		navPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
		GetComponent<Rigidbody> ().freezeRotation = true;
		aud = GetComponent<AudioSource> ();
		coverTimer = 0f;
		coverWaitTimer = Random.Range (2f, 7f);
		coverSwitchPositionsTimer = Random.Range (12f, 18f);
		inCover = false;
		pView = GetComponent<PhotonView> ();
	}

	// Update is called once per frame
	void Update () {
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Die")) {
			return;
		}
		if (!Vector3.Equals (GameControllerTestScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
			alerted = true;
		}
		/**Debug.Log (navMesh.destination);
		Debug.Log (actionState);
		Debug.Log ("Pos: " + transform.position);
		Debug.Log (navMesh.speed);
		Debug.Log (navMesh.isStopped);*/

		DecideActionPatrolInCombat ();

		//Debug.DrawRay (transform.position, transform.forward * range, Color.blue);

		if (alerted && (range == 10f)) {
			range *= 1.5f;
		}

		if (enemyType == EnemyType.Patrol) {
			DecideActionPatrol ();
			HandleMovementPatrol ();
		} else {
			DecideActionScout ();
		}

		// Shoot at player
		if (actionState == ActionStates.Firing) {
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
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
			return;
		DecideAnimation ();
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void LateUpdate() {
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
			return;
		// If the enemy sees the player, rotate the enemy towards the player
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
					wanderStallDelay -= Time.deltaTime;
				} else {
					// Else, check if the enemy has reached its destination
					if (navMeshReachedDestination (0f)) {
						navMesh.isStopped = true;
					}
				}
				// If the stall delay is done, the enemy needs to move to a wander point
				if (wanderStallDelay < 0f) {
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

		if (actionState == ActionStates.Pursue && lastSeenPlayerPos != null) {
			navMesh.speed = 6f;
			navMesh.isStopped = false;
			navMesh.SetDestination (lastSeenPlayerPos.position);
			lastSeenPlayerPos = null;
			return;
		}

		if (actionState == ActionStates.Seeking) {
			navMesh.isStopped = false;
			navMesh.speed = 4f;

			// Seek behavior: use navMesh to move towards the last area of gunshot. If the enemy moves towards that location
			// and there's nobody there, take cover in that area

			if (testingMode) {
				Vector3 currDest = navMesh.destination;
				if (!Vector3.Equals (currDest, GameControllerTestScript.lastGunshotHeardPos)) {
					//RotateTowards (GameControllerTestScript.lastGunshotHeardPos);
					navMesh.SetDestination (GameControllerTestScript.lastGunshotHeardPos);
				}

				if (navMeshReachedDestination (0.5f)) {
					bool coverFound = DynamicTakeCover ();
					if (coverFound) {
						actionState = ActionStates.TakingCover;
					} else {
						actionState = ActionStates.InCover;
					}
				}
			}
		}

		if (actionState == ActionStates.TakingCover) {
			// If the enemy is not near the cover spot, run towards it
			if (!coverPos.Equals (Vector3.negativeInfinity)) {
				navMesh.isStopped = true;
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
					actionState = ActionStates.InCover;
				}
			}
		}

		if (actionState == ActionStates.Firing && !inCover) {
			navMesh.speed = 4f;
			if (firingModeTimer <= 0f) {
				firingModeTimer = Random.Range (2f, 3.2f);
				int r = Random.Range (0, 4);
				if (r == 0) {
					firingState = FiringStates.StandingStill;
				} else if (r == 1) {
					firingState = FiringStates.Forward;
					navMesh.speed = 4f;
				} else if (r == 2) {
					firingState = FiringStates.Backpedal;
					navMesh.speed = 3f;
				} else if (r == 3) {
					firingState = FiringStates.StrafeLeft;
					navMesh.speed = 2.5f;
				} else if (r == 4) {
					firingState = FiringStates.StrafeRight;
					navMesh.speed = 2.5f;
				}
			}

			if (firingState == FiringStates.StandingStill) {
				navMesh.isStopped = true;
			}

			if (firingState == FiringStates.Forward) {
				navMesh.SetDestination (player.transform.position);
				navMesh.isStopped = false;
			}

			if (firingState == FiringStates.Backpedal) {
				RotateTowards (player.transform.position);
				Vector3 oppositeDirVector = player.transform.position - transform.position;
				//navMesh.SetDestination (new Vector3(transform.position.x, transform.position.y, transform.position.z - 5f));
				navMesh.SetDestination (new Vector3(-oppositeDirVector.x, oppositeDirVector.y, -oppositeDirVector.z));
				navMesh.isStopped = false;
			}

			if (firingState == FiringStates.StrafeLeft) {
				RotateTowards (player.transform.position);
				//Vector3 rotatedVector = Quaternion.AngleAxis(-90, Vector3.up) * (player.transform.position - transform.position);
				//navMesh.SetDestination (new Vector3(transform.position.x - 5f, transform.position.y, transform.position.z));
				//navMesh.SetDestination (rotatedVector);
				navMesh.SetDestination (transform.right * -2f);
				navMesh.isStopped = false;
			}

			if (firingState == FiringStates.StrafeRight) {
				RotateTowards (player.transform.position);
				//Vector3 rotatedVector = Quaternion.AngleAxis(90, Vector3.up) * (player.transform.position - transform.position);
				//navMesh.SetDestination (new Vector3(transform.position.x + 5f, transform.position.y, transform.position.z));
				navMesh.SetDestination (transform.right * 2f);
				navMesh.isStopped = false;
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
				if (coverWaitTimer <= 0f) {
					isCrouching = !isCrouching;
					coverWaitTimer = Random.Range (2f, 7f);
				}
				// Maneuvering through cover; if the maneuver timer runs out, it's time to move to another cover position
				// TODO: Broken - coverswitch timer is never reset
				/**if (coverSwitchPositionsTimer <= 0f) {
					bool coverFound = DynamicTakeCover ();
					if (coverFound) {
						inCover = false;
						actionState = ActionStates.TakingCover;
					} else {
						coverPos = Vector3.negativeInfinity;
						actionState = ActionStates.InCover;
					}
				}*/
			}

			//Debug.Log ("inCover: " + inCover);
		}
	}

	// Decision tree for scout type enemy
	void DecideActionScout() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead) {
			actionState = ActionStates.Dead;
			// Choose a death sound
			int r = Random.Range (0, 3);
			if (r == 0) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt1");
			} else if (r == 1) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt2");
			} else {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt4");
			}
			aud.Play ();

			GetComponentInChildren<SpriteRenderer> ().enabled = false;
			//GetComponent<CapsuleCollider> ().isTrigger = true;
			StartCoroutine(Despawn ());
			return;
		}

		// Continue with decision tree
		// Scan for a target player
		PlayerScan();
		// Sees a player?
		if (player != null) {
			alertTimer = 10f;

			if (Vector3.Distance (player.transform.position, transform.position) <= 2.3f) {
				actionState = ActionStates.Melee;
			} else {
				if (currentBullets > 0) {
					actionState = ActionStates.Firing;
					if (crouchMode == 0)
						crouchMode = 1;
					else if (crouchMode == 1)
						crouchMode = 2;
					TakeCoverScout ();
				} else {
					actionState = ActionStates.Reloading;
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
			actionState = ActionStates.Idle;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (!other.gameObject.tag.Equals ("Player") && !other.gameObject.tag.Equals ("PlayerTesting")) {
			return;
		}
		if (!alerted) {
			alerted = true;
		}

		actionState = ActionStates.Melee;
		playerToHit = other.gameObject;
	}

	// Decision tree for patrol type enemy
	void DecideActionPatrol() {
		// Melee attack trumps all
		if (actionState == ActionStates.Melee) {
			return;
		}
		// Root - is the enemy alerted by any type of player presence (gunshots, sight, getting shot, other enemies alerted nearby)
		if (alerted) {
			// Scan for enemies
			PlayerScan();
			if (player != null) {
				// If the enemy has seen a player
				if (actionState != ActionStates.Firing && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Pursue && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, 2);
					if (r == 1) {
						actionState = ActionStates.Firing;
					} else {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							actionState = ActionStates.TakingCover;
						} else {
							actionState = ActionStates.InCover;
						}
					}
				}
			} else {
				// If the enemy has not seen a player
				if (actionState != ActionStates.Seeking && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Firing && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, 6);
					if (r > 2) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							actionState = ActionStates.TakingCover;
						} else {
							actionState = ActionStates.InCover;
						}
					} else {
						actionState = ActionStates.Seeking;
					}
				}

				// If the enemy has seen a player before but no longer does, then possibly (60% chance) pursue the player or take cover (40% chance)
				if (actionState == ActionStates.Firing && lastSeenPlayerPos != null) {
					int r = Random.Range (1, 5);
					if (r <= 2) {
						actionState = ActionStates.TakingCover;
					} else {
						actionState = ActionStates.Pursue;
					}
				}
			}
		} else {
			// Else, wander around the patrol points until alerted or enemy seen
			actionState = ActionStates.Wander;
			PlayerScan ();
			if (player != null) {
				alerted = true;
			}
		}
	}

	void DecideAnimation() {
		if (actionState == ActionStates.Seeking) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving"))
				animator.Play ("Moving");
		}

		if (actionState == ActionStates.Wander) {
			if (navMesh.isStopped) {
				if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
					animator.Play ("Idle");
			} else {
				if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Walk"))
					animator.Play ("Walk");
			}

		}

		if (actionState == ActionStates.TakingCover) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint"))
				animator.Play ("Sprint");
		}

		if (actionState == ActionStates.Dead) {
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
				animator.Play ("Die");
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
			} else {
				navMesh.isStopped = true;
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

		if (player != null) {
			RaycastHit hit;
			// Locks onto the player and shoots at him
			Vector3 dir = player.GetComponentsInChildren<Transform>()[0].position - shootPoint.position;

			// Adding artificial stupidity - ensures that the player isn't hit every time by offsetting
			// the shooting direction in x and y by two random numbers
			float xOffset = Random.Range (-2.5f, 2.5f);
			float yOffset = Random.Range (-2.5f, 2.5f);
			dir = new Vector3 (dir.x + xOffset, dir.y + yOffset, dir.z);
			Debug.DrawRay (shootPoint.position, dir * range, Color.red);
			if (Physics.Raycast (shootPoint.position, dir, out hit)) {
				GameObject bloodSpill = null;
				if (hit.transform.tag.Equals ("Player") || hit.transform.tag.Equals ("Human")) {
					bloodSpill = Instantiate (bloodEffect, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
					bloodSpill.transform.Rotate (180f, 0f, 0f);
					Debug.Log (transform.name + " has hit you");
					if (hit.transform.tag.Equals ("Player")) {
						hit.transform.GetComponent<PlayerScript> ().health -= (int)damage;
						hit.transform.GetComponent<PlayerScript> ().hitTimer = 0f;
						hit.transform.GetComponent<PlayerScript> ().hitLocation = transform.position;
					} else {
						hit.transform.GetComponent<BetaEnemyScript> ().health -= (int)damage;
					}
				} else {
					GameObject hitParticleEffect = Instantiate (hitParticles, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
					GameObject bulletHoleEffect = Instantiate (bulletImpact, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
					bulletHoleEffect.transform.SetParent (hit.transform);
					Destroy (hitParticleEffect, 1f);
					Destroy (bulletHoleEffect, 3f);
				}
				if (bloodSpill != null)
					Destroy (bloodSpill, 1.5f);
			}
		}

		//animator.CrossFadeInFixedTime ("Firing", 0.01f);
		muzzleFlash.Play ();
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}

	private void PlayShootSound() {
		audioSource.PlayOneShot (shootSound);
	}

	public void Reload() {
		int bulletsToLoad = bulletsPerMag - currentBullets;
		currentBullets += bulletsPerMag;
	}

	public void MeleeAttack() {
		if (playerToHit != null && !testingMode) {
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
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
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
		ArrayList coverIndices = new ArrayList ();
		for (int i = 0; i < nearbyCover.Length; i++) {
			if (nearbyCover [i].gameObject.tag.Equals ("Cover")) {
				coverIndices.Add (i);
			}
		}
		if (coverIndices.Count == 0) {
			return false;
		}

		int minCoverIndex = (int)coverIndices [Random.Range (0, coverIndices.Count - 1)];

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
		if (testingMode) {
			if (player == null) {
				ArrayList indicesNearBy = new ArrayList ();
				for (int i = 0; i < GameControllerTestScript.playerList.Count; i++) {
					GameObject p = (GameObject)GameControllerTestScript.playerList [i];
					if (Vector3.Distance (transform.position, p.transform.position) < range + 12f) {
						Vector3 toPlayer = p.transform.position - transform.position;
						float angleBetween = Vector3.Angle (transform.forward, toPlayer);
						if (angleBetween <= 60f) {
							indicesNearBy.Add (i);
						}
					}
				}
				if (indicesNearBy.Count != 0)
					player = (GameObject)GameControllerTestScript.playerList [Random.Range (0, indicesNearBy.Count)];
			} else {
				// If we do, check if it's still in range
				//Debug.Log("Dist: " + Vector3.Distance(transform.position, player.transform.position) + " Range:" + (range + 12f));
				if (Vector3.Distance (transform.position, player.transform.position) >= range + 12f) {
					lastSeenPlayerPos = player.transform;
					player = null;

				}
			}
		} else {
			if (player == null) {
				ArrayList indicesNearBy = new ArrayList ();
				for (int i = 0; i < GameControllerScript.playerList.Count; i++) {
					GameObject p = (GameObject)GameControllerScript.playerList [i];
					if (Vector3.Distance (transform.position, p.transform.position) < range + 12f) {
						Vector3 toPlayer = p.transform.position - transform.position;
						float angleBetween = Vector3.Angle (transform.forward, toPlayer);
						if (angleBetween <= 60f) {
							indicesNearBy.Add (i);
						}
					}
				}
				if (indicesNearBy.Count != 0)
					player = (GameObject)GameControllerScript.playerList [Random.Range (0, indicesNearBy.Count)];
			} else {
				// If we do, check if it's still in range
				if (Vector3.Distance (transform.position, player.transform.position) >= range + 12f) {
					lastSeenPlayerPos = player.transform;
					player = null;

				}
			}
		}
	}

	public void TakeDamage(int d) {
		pView.RPC ("RpcTakeDamage", RpcTarget.AllBuffered, d);
	}

	[PunRPC]
	public void RpcTakeDamage(int d) {
		health -= d;
	}

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action) {
		
	}

	[PunRPC]
	private void RpcUpdateFiringState(FiringStates firing) {
		
	}

}
