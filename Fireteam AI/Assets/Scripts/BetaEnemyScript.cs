using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

public class BetaEnemyScript : MonoBehaviour {

	private const float MELEE_DISTANCE = 2.3f;
	private const float PLAYER_HEIGHT_OFFSET = 1f;

	// Prefab references
	public GameObject ammoBoxPickup;
	public GameObject healthBoxPickup;
	public AudioClip[] voiceClips;
	public AudioClip[] gruntSounds;
	public GameObject hitParticles;
	public GameObject bulletImpact;
	public GameObject bloodEffect;
	public GameObject bloodEffectHeadshot;

	// Body/Component references
	private AudioSource audioSource;
	private AudioSource gunAudio;
	private PhotonView pView;
	public Transform headTransform;
	public Transform shootPoint;
	public LineRenderer sniperTracer;
	private Animator animator;
	private Rigidbody rigid;
	private SpriteRenderer marker;
	public EnemyModelCreator modeler;
	public NavMeshAgent navMesh;
	public NavMeshObstacle navMeshObstacle;
	private CapsuleCollider myCollider;
	private CapsuleCollider headCollider;
	private MeshRenderer gunRef;
	private BoxCollider meleeTrigger;
	private Vector3 prevNavDestination;
	private bool prevWasStopped;

	// Enemy variables
	public EnemyType enemyType;
	public ActionStates actionState;
	private FiringStates firingState;
	private bool isCrouching;
	public int aggression;
	private float accuracyOffset;
	public bool sniper;
	public float rotationSpeed = 6f;
	public int health;
	public int deathBy;
	public float disorientationTime;
	private Vector3 spawnPos;
	private Vector3 spawnRot;
	public bool alerted = false;
	private bool suspicious = false;
	private bool wasMasterClient;
	private GameObject gameController;
	private ArrayList enemyAlertMarkers;
	public int alertStatus;
	// Responsible for displaying the correct alert symbol. If equals 0, then the alert display is inactive
	public int alertDisplay;

	// Finite state machine states
	public enum ActionStates {Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking, Disoriented};
	// FSM used for determining movement while attacking and not in cover
	enum FiringStates {StandingStill, StrafeLeft, StrafeRight, Backpedal, Forward};

	// Type of enemy
	public enum EnemyType {Patrol, Scout};

	// Gun/weapon stuff
	public float range;
	public int bulletsPerMag = 30;
	public int currentBullets;
	public AudioClip shootSound;
	public ParticleSystem muzzleFlash;
	private bool isReloading = false;
	public float fireRate = 0.4f;
	public float damage = 10f;
	float fireTimer = 0.0f; // Once it equals fireRate, it will allow us to shoot

	// Target references
	public GameObject player;
	private GameObject playerToHit;
	public Vector3 lastSeenPlayerPos = Vector3.negativeInfinity;

	// All patrol pathfinding points for an enemy
	public GameObject[] navPoints;

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
	private float originalColliderHeight = 0f;
	private float originalColliderRadius = 0f;
	private Vector3 originalColliderCenter;

	// Testing mode - set in inspector
	//public bool testingMode;

	// Use this for initialization
	void Start () {
		alertDisplay = 0;
		alertTimer = -100f;
		coverWaitTimer = Random.Range (2f, 7f);
		coverSwitchPositionsTimer = Random.Range (12f, 18f);

		player = null;
		spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		spawnRot = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
		animator = GetComponent<Animator> ();
		health = 100;
		disorientationTime = 0f;
		currentBullets = bulletsPerMag;
		audioSource = GetComponent<AudioSource> ();
		audioSource.maxDistance = 100f;
		gunAudio = GetComponentsInChildren<AudioSource> () [1];
		rigid = GetComponent<Rigidbody> ();
		rigid.freezeRotation = true;
		isCrouching = false;

		coverTimer = 0f;
		inCover = false;
		pView = GetComponent<PhotonView> ();

		myCollider = GetComponent<CapsuleCollider> ();
		headCollider = GetComponentsInChildren<CapsuleCollider> () [1];
		originalColliderHeight = myCollider.height;
		originalColliderRadius = myCollider.radius;
		originalColliderCenter = new Vector3 (myCollider.center.x, myCollider.center.y, myCollider.center.z);
		marker = GetComponentInChildren<SpriteRenderer> ();
		gunRef = GetComponentInChildren<MeshRenderer> ();
		meleeTrigger = GetComponent<BoxCollider> ();
		gameController = GameObject.Find("GameController");
		gameController.GetComponent<GameControllerScript>().enemyList.Add(pView.ViewID, gameObject);
		enemyAlertMarkers = gameController.GetComponent<GameControllerScript>().enemyAlertMarkers;

		if (enemyType == EnemyType.Patrol) {
			range = 20f;
			accuracyOffset = 0.5f;
			fireRate = 0.4f;
			damage = 20f;
			shootSound = (AudioClip)Resources.Load ("Gun Sounds/M16A3");
			gunAudio.minDistance = 9f;
			aggression = 7;
		} else {
			if (sniper) {
				range = 35f;
				accuracyOffset = 1.5f;
				fireRate = 20f;
				damage = 35f;
				shootSound = (AudioClip)Resources.Load ("Gun Sounds/L96A1");
				gunAudio.minDistance = 18f;
			} else {
				range = 27f;
				accuracyOffset = 0.5f;
				fireRate = 0.4f;
				damage = 20f;
				shootSound = (AudioClip)Resources.Load ("Gun Sounds/M16A3");
				gunAudio.minDistance = 9f;
			}
		}

		prevWasStopped = true;
		prevNavDestination = Vector3.negativeInfinity;

		if (!PhotonNetwork.IsMasterClient) {
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
			wasMasterClient = false;
		} else {
			wasMasterClient = true;
		}

	}

	// Update is called once per frame
	void Update () {
		if (PhotonNetwork.IsMasterClient) {
			if (!wasMasterClient) {
				wasMasterClient = true;
				if (enemyType == EnemyType.Patrol) {
					navMesh.enabled = true;
					navMeshObstacle.enabled = false;
					if (!prevWasStopped) {
						if (navMesh.isOnNavMesh) {
							navMesh.isStopped = prevWasStopped;
							navMesh.SetDestination (prevNavDestination);
						}
					}
				} else {
					navMeshObstacle.enabled = true;
					navMesh.enabled = false;
				}
			}
			if (actionState == ActionStates.Disoriented && disorientationTime <= 0f) {
				pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Idle);
			}
		} else {
			wasMasterClient = false;
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
		}

		UpdateDisorientationTime();
		ReplenishFireRate ();
		DecreaseAlertTime ();
		UpdateFiringModeTimer ();
		EnsureNotSuspiciousAndAlerted();
		HandleEnemyAlerts();

		if (!PhotonNetwork.IsMasterClient || animator.GetCurrentAnimatorStateInfo(0).IsName("Die") || animator.GetCurrentAnimatorStateInfo(0).IsName("DieHeadshot")) {
			if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
				StopVoices();
			}
			return;
		}

		CheckAlerted ();
		CheckTargetDead ();

		// If disoriented, don't have the ability to do anything else except die
		if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
			StopVoices();
			return;
		}

		HandleCrouching ();

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

	}

	void FixedUpdate() {
		// Hot fix for death animation not working on client
		if (!PhotonNetwork.IsMasterClient && health <= 0) {
			//removeFromMarkerList();
			actionState = ActionStates.Dead;
		}

		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Die") || animator.GetCurrentAnimatorStateInfo (0).IsName ("DieHeadshot")) {
			if (PhotonNetwork.IsMasterClient && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			}
			return;
		}
		// Handle animations independent of frame rate
		DecideAnimation ();
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Disoriented")) {
			if (PhotonNetwork.IsMasterClient && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			}
			return;
		}
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void LateUpdate() {
		if (!PhotonNetwork.IsMasterClient || health <= 0)
			return;
		// If the enemy sees the player, rotate the enemy towards the player only if the enemy is aiming at the player
		if (player != null && ShouldRotateTowardsPlayerTarget()) {
			RotateTowardsPlayer();
		}

	}

	bool ShouldRotateTowardsPlayerTarget() {
		if (actionState == ActionStates.Firing || actionState == ActionStates.Melee || actionState == ActionStates.Reloading) {
			return true;
		}
		return false;
	}



	void ReplenishFireRate() {
		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}
	}

	void DecreaseAlertTime() {
		if (alertTimer > 0f) {
			alertTimer -= Time.deltaTime;
		}
	}

	void UpdateFiringModeTimer() {
		if (firingModeTimer > 0f) {
			firingModeTimer -= Time.deltaTime;
		}
	}

	void UpdateDisorientationTime() {
		if (disorientationTime > 0f) {
			disorientationTime -= Time.deltaTime;
		}
	}

	void HandleCrouching() {
		if (health > 0) {
			if (isCrouching) {
				myCollider.height = 0.97f;
				myCollider.radius = 0.32f;
				myCollider.center = new Vector3 (-0.05f, 0.43f, -0.03f);
			} else {
				myCollider.height = originalColliderHeight;
				myCollider.radius = originalColliderRadius;
				myCollider.center = new Vector3 (originalColliderCenter.x, originalColliderCenter.y, originalColliderCenter.z);
			}
		}
	}

	void CheckAlerted() {
		if (!Vector3.Equals (GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
			if (!alerted) {
				SetAlerted(true);
				SetAlertTimer (12f);
			}
		}
	}

	public void SetAlerted(bool b) {
		pView.RPC ("RpcSetAlerted", RpcTarget.All, b);
	}

	public void SetAlertTimer(float t) {
		pView.RPC ("RpcSetAlertTimer", RpcTarget.All, t);
	}

	[PunRPC]
	void RpcSetAlertTimer(float t) {
		alertTimer = t;
	}

	// What happens when the enemy is alerted
	[PunRPC]
	void RpcSetAlerted(bool b) {
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
		isCrouching = b;
	}

	void RotateTowardsPlayer() {
		Vector3 rotDir = (player.transform.position - transform.position).normalized;
		Quaternion lookRot = Quaternion.LookRotation (rotDir);
		Quaternion tempQuat = Quaternion.Slerp (transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
		Vector3 tempRot = tempQuat.eulerAngles;
		transform.rotation = Quaternion.Euler (new Vector3 (0f, tempRot.y, 0f));
	}

	[PunRPC]
	void RpcSetWanderStallDelay(float f) {
		wanderStallDelay = f;
	}

	[PunRPC]
	void RpcSetNavMeshDestination(float x, float y, float z) {
		if (PhotonNetwork.IsMasterClient) {
			if (navMesh.isOnNavMesh) {
				navMesh.SetDestination (new Vector3 (x, y, z));
				navMesh.isStopped = false;
			}
		} else {
			prevNavDestination = new Vector3 (x,y,z);
			prevWasStopped = false;
		}
	}

	[PunRPC]
	void RpcSetInCover(bool n) {
		inCover = n;
	}

	[PunRPC]
	void RpcSetFiringModeTimer(float t) {
		firingModeTimer = t;
	}

	void HandleMovementPatrol() {
		// Melee attack trumps all
		if (actionState == ActionStates.Melee || actionState == ActionStates.Dead || actionState == ActionStates.Disoriented) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			}
			return;
		}
		// Handle movement for wandering
		if (actionState == ActionStates.Wander) {
			if (PhotonNetwork.IsMasterClient) {
				if (navMesh.speed != 1.5f) {
					pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 1.5f);
				}
				// Only server should be updating the delays and they should sync across the network
				// Initial spawn value
				if (wanderStallDelay == -1f) {
					wanderStallDelay = Random.Range (0f, 7f);
					pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay);
				}
				// Take away from the stall delay if the enemy is standing still
				if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
					wanderStallDelay -= Time.deltaTime;
					//pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay);
				} else {
					// Else, check if the enemy has reached its destination
					if (navMeshReachedDestination (0.3f)) {
						pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
					}
				}
				// If the stall delay is done, the enemy needs to move to a wander point
				if (wanderStallDelay < 0f && navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
					int r = Random.Range (0, navPoints.Length);
					RotateTowards (navPoints [r].transform.position);
					pView.RPC ("RpcSetNavMeshDestination", RpcTarget.All, navPoints [r].transform.position.x, navPoints [r].transform.position.y, navPoints [r].transform.position.z);
					wanderStallDelay = Random.Range (0f, 7f);
					//pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay);
				}
			}
		}

		if (actionState == ActionStates.Idle) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
				pView.RPC ("RpcSetWanderStallDelay", RpcTarget.All, -1f);
			}
		}

		if (actionState == ActionStates.Dead || actionState == ActionStates.InCover) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			}
		}

		if (actionState == ActionStates.Pursue && !lastSeenPlayerPos.Equals(Vector3.negativeInfinity)) {
			if (navMesh.speed != 6f) {
				pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 6f);
				pView.RPC ("RpcSetNavMeshDestination", RpcTarget.All, lastSeenPlayerPos.x, lastSeenPlayerPos.y, lastSeenPlayerPos.z);
				pView.RPC ("RpcSetLastSeenPlayerPos", RpcTarget.All, false, 0f, 0f, 0f);
			}
			return;
		}

		if (actionState == ActionStates.Seeking) {
			// Seek behavior: use navMesh to move towards the last area of gunshot. If the enemy moves towards that location
			// and there's nobody there, go back to wandering the area

			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
				pView.RPC ("RpcSetNavMeshDestination", RpcTarget.All, GameControllerScript.lastGunshotHeardPos.x, GameControllerScript.lastGunshotHeardPos.y, GameControllerScript.lastGunshotHeardPos.z);
				if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint")) {
					if (navMesh.speed != 6f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 6f);
					}
				} else {
					if (navMesh.speed != 4f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 4f);
					}
				}
			}
		}

		if (actionState == ActionStates.TakingCover) {
			// If the enemy is not near the cover spot, run towards it
			if (!coverPos.Equals (Vector3.negativeInfinity)) {
				pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 6f);
				pView.RPC ("RpcSetNavMeshDestination", RpcTarget.All, coverPos.x, coverPos.y, coverPos.z);
				pView.RPC ("RpcSetCoverPos", RpcTarget.All, false, 0f, 0f, 0f);
			} else {
				// If the enemy has finally reached cover, then he will get into cover mode
				if (navMeshReachedDestination(0f)) {
					// Done
					pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
					if (actionState != ActionStates.InCover) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.InCover);
					}
				}
			}
		}

		if (actionState == ActionStates.Firing) {
			if (navMesh.speed != 4f) {
				pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 4f);
			}
			if (firingModeTimer <= 0f) {
				int r = Random.Range (0, 5);
				if (r == 0) {
					if (firingState != FiringStates.StandingStill) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StandingStill);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer);
				} else if (r == 1) {
					if (firingState != FiringStates.Forward) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.Forward);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer);
					if (navMesh.speed != 4f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 4f);
					}
				} else if (r == 2) {
					if (firingState != FiringStates.Backpedal) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.Backpedal);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer);
					if (navMesh.speed != 3f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 3f);
					}
				} else if (r == 3) {
					if (firingState != FiringStates.StrafeLeft) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StrafeLeft);
					}
					firingModeTimer = 1.7f;
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer);
					if (navMesh.speed != 2.5f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 2.5f);
					}
				} else if (r == 4) {
					if (firingState != FiringStates.StrafeRight) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StrafeRight);
					}
					firingModeTimer = 1.7f;
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer);
					if (navMesh.speed != 2.5f) {
						pView.RPC ("RpcUpdateNavMeshSpeed", RpcTarget.All, 2.5f);
					}
				}
			}

			if (firingState == FiringStates.StandingStill) {
				pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			}

			if (player != null) {
				if (firingState == FiringStates.Forward) {
					navMesh.isStopped = true;
					navMesh.ResetPath ();
					navMesh.isStopped = false;
					navMesh.Move (transform.forward * Time.deltaTime);
					//navMesh.SetDestination (player.transform.position);
				}

				if (firingState == FiringStates.Backpedal) {
					navMesh.isStopped = true;
					navMesh.ResetPath ();
					navMesh.isStopped = false;
					navMesh.Move (transform.forward * -Time.deltaTime);
					//navMesh.SetDestination (new Vector3(transform.position.x, transform.position.y, transform.position.z - 5f));
					//navMesh.SetDestination (new Vector3 (-oppositeDirVector.x, oppositeDirVector.y, -oppositeDirVector.z));
				}

				if (firingState == FiringStates.StrafeLeft) {
					navMesh.isStopped = true;
					navMesh.ResetPath ();
					navMesh.isStopped = false;
					Vector3 dest = new Vector3 (transform.right.x * Time.deltaTime, transform.right.y * Time.deltaTime, transform.right.z * Time.deltaTime);
					navMesh.Move (dest);
					//navMesh.SetDestination (new Vector3(transform.position.x + dest.x, transform.position.y + dest.y, transform.position.z + dest.z));
				}

				if (firingState == FiringStates.StrafeRight) {
					navMesh.isStopped = true;
					navMesh.ResetPath ();
					navMesh.isStopped = false;
					Vector3 dest = new Vector3 (transform.right.x * -Time.deltaTime, transform.right.y * -Time.deltaTime, transform.right.z * -Time.deltaTime);
					navMesh.Move (dest);
				}
			}
		}
	}

	[PunRPC]
	void RpcSetCoverWaitTimer(float t) {
		coverWaitTimer = t;
	}

	[PunRPC]
	void RpcSetCoverSwitchPositionsTimer(float t) {
		coverSwitchPositionsTimer = t;
	}

	// Action Decision while in combat
	void DecideActionPatrolInCombat() {
		if (actionState == ActionStates.InCover || actionState == ActionStates.Firing) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
				coverWaitTimer -= Time.deltaTime;
				//pView.RPC ("RpcSetCoverWaitTimer", 	RpcTarget.Others, coverWaitTimer);
			}

			// Three modes in cover - defensive, offensive, maneuvering; only used when engaging a player
			if (player != null) {
				// If the cover wait timer has ran out, switch from defensive to offensive and vice versa
				if (coverWaitTimer <= 0f && !isReloading) {
					pView.RPC ("RpcSetIsCrouching", RpcTarget.All, !isCrouching);
					coverWaitTimer = Random.Range (2f, 7f);
					//pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer);
				}
				// Maneuvering through cover; if the maneuver timer runs out, it's time to move to another cover position
				// TODO: Broken - coverswitch timer is never reset
				/**if (coverSwitchPositionsTimer <= 0f) {
					bool coverFound = DynamicTakeCover ();
					if (coverFound) {
						inCover = false;
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.TakingCover);
					} else {
						coverPos = Vector3.negativeInfinity;
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.InCover);
					}
				}*/
			}
		}
	}

	void PlayVoiceClip(int n) {
		if (!audioSource.isPlaying && health > 0 && disorientationTime <= 0f) {
			audioSource.clip = voiceClips [n - 1];
			audioSource.Play ();
		}
	}

	public void PlayGruntSound() {
		if (gruntSounds.Length == 0) return;
		int r = Random.Range(0, gruntSounds.Length);
		audioSource.clip = voiceClips [r];
		audioSource.Play ();
	}

	IEnumerator PlayVoiceClipDelayed(int n, float t) {
		yield return new WaitForSeconds (t);
		if (actionState != ActionStates.Dead) {
			PlayVoiceClip (n);
		}
	}

	[PunRPC]
	void RpcDie() {
		marker.enabled = false;
	}

	[PunRPC]
	void RpcSetCrouchMode(int n) {
		crouchMode = n;
	}

	// Decision tree for scout type enemy
	void DecideActionScout() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead) {
			// Spawn a drop box
			int r = Random.Range (1,12);
			if (r == 6) {
				// 1/6 chance of getting a health box
				PhotonNetwork.Instantiate(healthBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			} else if (r >= 1 && r < 5) {
				// 1/3 chance of getting ammo box
				PhotonNetwork.Instantiate(ammoBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			}

			pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Dead);

			pView.RPC ("StartDespawn", RpcTarget.All);
			return;
		}

		// Melee attack trumps all
		if (actionState == ActionStates.Melee || actionState == ActionStates.Disoriented) {
			return;
		}

		// Continue with decision tree
		PlayerScan();
		// Sees a player?
		if (player != null) {
			alertTimer = 10f;
			player.GetComponent<PlayerActionScript>().IncreaseDetectionLevel();
			if (Vector3.Distance (player.transform.position, transform.position) <= MELEE_DISTANCE) {
				if (actionState != ActionStates.Melee) {
					pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Melee);
				}
			} else {
				if (currentBullets > 0) {
					if (actionState != ActionStates.Firing) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Firing);
					}
					if (crouchMode == 0) {
						pView.RPC ("RpcSetCrouchMode", RpcTarget.All, 1);
					} else if (crouchMode == 1) {
						pView.RPC ("RpcSetCrouchMode", RpcTarget.All, 2);
					}
					TakeCoverScout ();
				} else {
					if (actionState != ActionStates.Reloading) {
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Reloading);
					}
					pView.RPC ("RpcSetCrouchMode", RpcTarget.All, 0);
					TakeCoverScout ();
				}
			}
		} else {
			if (alertTimer > 0f) {
				if (crouchMode != 0) {
					pView.RPC ("RpcSetCrouchMode", RpcTarget.All, 0);
				}
				TakeCoverScout ();
			} else {
				if (crouchMode != 1) {
					pView.RPC ("RpcSetCrouchMode", RpcTarget.All, 1);
				}
				TakeCoverScout ();
			}
			if (actionState != ActionStates.Idle) {
				pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Idle);
			}
		}
	}

	[PunRPC]
	void RpcSetPlayerToHit(int id) {
		if (id == -1) {
			playerToHit = null;
		} else {
			playerToHit = GameControllerScript.playerList [id];
		}
	}

	bool EnvObstructionExists(Vector3 a, Vector3 b) {
		// Ignore other enemy/player colliders
		// Layer mask (layers/objects to ignore in explosion that don't count as defensive)
		int ignoreLayers = (1 << 9) & (1 << 11) & (1 << 12) & (1 << 13) & (1 << 14) & (1 << 15);
		ignoreLayers = ~ignoreLayers;
		return Physics.Linecast(a, b, ignoreLayers);
	}

	void KilledByGrenade(int killedByViewId) {
		pView.RPC("RpcRegisterGrenadeKill", RpcTarget.All, killedByViewId);
	}

	void HandleExplosiveEffectTriggers(Collider other) {
		// First priority is to handle possible explosion damage
		if (health <= 0) {
			return;
		}
		if (other.gameObject.tag.Equals("Explosive")) {
            // If the grenade is still active or if the grenade has already affected the enemy, ignore it
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            // If a ray casted from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID)) {
				// Determine how far from the explosion the enemy was
				float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
				float blastRadius = other.gameObject.GetComponent<ThrowableScript>().blastRadius;
				distanceFromGrenade = Mathf.Min(distanceFromGrenade, blastRadius);
				float scale = 1f - (distanceFromGrenade / blastRadius);

				// Scale damage done to enemy by the distance from the explosion
				WeaponStats grenadeStats = other.gameObject.GetComponent<WeaponStats>();
				int damageReceived = (int)(grenadeStats.damage * scale);

				// Deal damage to the enemy
				TakeDamage(damageReceived);
                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
				if (health <= 0) {
					deathBy = 1;
					KilledByGrenade(t.playerThrownByReference);
				}
			}

			// Make enemy alerted by the explosion if he's not dead
			if (!alerted && health > 0) {
				SetAlerted(true);
			}
			return;
		}

		if (other.gameObject.tag.Equals("Flashbang")) {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID)) {
				float totalDisorientationTime = ThrowableScript.MAX_FLASHBANG_TIME;

				// Determine how far from the explosion the enemy was
				float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
				float blastRadius = t.blastRadius;

				// Determine rotation away from the flashbang - if more pointed away, less the duration
				Vector3 toPosition = Vector3.Normalize(other.gameObject.transform.position - transform.position);
				float angleToPosition = Vector3.Angle(transform.forward, toPosition);

				// Modify total disorientation time dependent on distance from grenade and rotation away from grenade
				float distanceMultiplier = Mathf.Clamp(1f - (distanceFromGrenade / blastRadius) + 0.6f, 0f, 1f);
				float rotationMultiplier = Mathf.Clamp(1f - (angleToPosition / 180f) + 0.1f, 0f, 1f);

				// Set enemy disorientation time
				totalDisorientationTime *= distanceMultiplier * rotationMultiplier;
				disorientationTime += totalDisorientationTime;
				disorientationTime = Mathf.Min(disorientationTime, ThrowableScript.MAX_FLASHBANG_TIME);
				if (disorientationTime > 0f) {
					pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Disoriented);
				}
                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
                // Make enemy alerted by the disorientation if he's not dead
                if (!alerted && health > 0) {
					SetAlerted(true);
				}
				return;
			}
		}
	}

	void HandleMeleeEffectTriggers(Collider other) {
		float dist = Vector3.Distance(transform.position, other.transform.position);
		if (dist <= MELEE_DISTANCE) {
			if (!alerted) {
				pView.RPC ("RpcSetAlerted", RpcTarget.All, true);
			}

			if (actionState != ActionStates.Melee) {
				pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Melee);
			}
			playerToHit = other.gameObject;
		}
	}

	[PunRPC]
	void RpcRegisterGrenadeKill(int playerNetworkId) {
		// If the player id of the person who killed the enemy matches my player id
		if (playerNetworkId == PlayerData.playerdata.inGamePlayerReference.GetComponent<PhotonView>().ViewID) {
			// Increment my kill score and show the kill popup for myself
			PlayerData.playerdata.inGamePlayerReference.GetComponent<WeaponActionScript>().RewardKill(false);
		}
	}

	void OnTriggerEnter(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (PhotonNetwork.IsMasterClient) {
			HandleExplosiveEffectTriggers(other);
		}

		/** Melee trigger functionality below */
		if (!PhotonNetwork.LocalPlayer.IsLocal) {
			return;
		}

		if (!other.gameObject.tag.Equals ("Player")) {
			return;
		}
		// Don't consider dead players
		if (other.gameObject.GetComponent<PlayerActionScript> ().health <= 0f) {
			return;
		}

		// If the player enters the enemy's sight range, determine if the player is in the right angle. If he is and there is no current player to target, then
		// assign the player and stop searching
		if (actionState != ActionStates.Disoriented && health > 0f) {
			HandleMeleeEffectTriggers(other);
		}
	}

	[PunRPC]
	void RpcUpdateNavMesh(bool stopped) {
		if (PhotonNetwork.IsMasterClient) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
				navMesh.isStopped = stopped;
			}
		} else {
			prevWasStopped = stopped;
		}
	}

	[PunRPC]
	void RpcUpdateNavMeshSpeed(float speed) {
		navMesh.speed = speed;
	}

	[PunRPC]
	void StartDespawn() {
		StartCoroutine(Despawn());
	}

	// Decision tree for patrol type enemy
	void DecideActionPatrol() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead)
		{
			int r = Random.Range (1,12);
			if (r == 6) {
				// 1/6 chance of getting a health box
				PhotonNetwork.Instantiate(healthBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			} else if (r >= 1 && r < 5) {
				// 1/3 chance of getting ammo box
				PhotonNetwork.Instantiate(ammoBoxPickup.name, transform.position, Quaternion.Euler(Vector3.zero));
			}

			pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
			pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Dead);

			pView.RPC ("StartDespawn", RpcTarget.All);
			return;
		}

		// Melee attack trumps all
		if (actionState == ActionStates.Melee || actionState == ActionStates.Disoriented) {
			return;
		}

		PlayerScan ();
		// Root - is the enemy alerted by any type of player presence (gunshots, sight, getting shot, other enemies alerted nearby)
		if (alerted) {
			if (player != null) {
				// If the enemy has seen a player
				if (!alerted) {
					player.GetComponent<PlayerActionScript>().IncreaseDetectionLevel();
				}
				alertTimer = 12f;
				if (actionState != ActionStates.Firing && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Pursue && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, aggression - 2);
					if (r <= 1) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.InCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.InCover);
							}
						}
					} else {
						if (actionState != ActionStates.Firing) {
							pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Firing);
						}
					}
				}
			} else {
				// If the enemy has not seen a player
				if (alertTimer <= 0f && alertTimer != -100f && alerted) {
					pView.RPC ("RpcUpdateAlertedStatus", RpcTarget.All);
				}
				if (actionState != ActionStates.Seeking && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Firing && actionState != ActionStates.Reloading) {
					int r = Random.Range (1, aggression - 1);
					if (r <= 4) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.InCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.InCover);
							}
						}
					} else {
						if (actionState != ActionStates.Seeking) {
							pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Seeking);
						}
					}
				}

				if (actionState == ActionStates.Seeking) {
					if (navMeshReachedDestination (20f) && player == null) {
						pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Wander);
					}
				}

				// If the enemy has seen a player before but no longer does, then possibly (60% chance) pursue the player or take cover (40% chance)
				if (actionState == ActionStates.Firing) {
					if (!Vector3.Equals (lastSeenPlayerPos, Vector3.negativeInfinity)) {
						int r = Random.Range (1, aggression);
						if (r <= 2) {
							if (actionState != ActionStates.TakingCover) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.TakingCover);
							}
						} else {
							if (actionState != ActionStates.Pursue) {
								pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Pursue);
							}
						}
					} else {
						if (actionState != ActionStates.Idle) {
							pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.Idle);
						}
					}
				}

				// If the enemy was in pursuit of a player but has lost track of him, then go back to wandering
				if (actionState == ActionStates.Pursue && Vector3.Equals(lastSeenPlayerPos, Vector3.negativeInfinity)) {
					if (navMeshReachedDestination(0.5f)) {
						pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Wander);
					}
				}

				// If the enemy is in cover, stay there for a while and then go back to seeking the last gunshot position, or wandering if there isn't one
				if (actionState == ActionStates.InCover) {
					coverSwitchPositionsTimer -= Time.deltaTime;
					if (coverSwitchPositionsTimer <= 0f) {
						coverSwitchPositionsTimer = Random.Range (6f, 10f);
						//pView.RPC ("RpcSetCoverSwitchPositionsTimer", RpcTarget.Others, coverSwitchPositionsTimer);
						if (GameControllerScript.lastGunshotHeardPos != Vector3.negativeInfinity) {
							pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Seeking);
						} else {
							pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Wander);
						}
					}
				}
			}
		} else {
			// Else, wander around the patrol points until alerted or enemy seen
			if (actionState != ActionStates.Wander) {
				pView.RPC("RpcUpdateActionState", RpcTarget.All, ActionStates.Wander);
			}
			if (player != null && !alerted) {
				pView.RPC("RpcSetAlerted", RpcTarget.All, true);
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
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
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
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Die") && !animator.GetCurrentAnimatorStateInfo (0).IsName ("DieHeadshot")
			&& !animator.GetCurrentAnimatorStateInfo (0).IsName ("Die2") && !animator.GetCurrentAnimatorStateInfo (0).IsName ("DieExplosion")) {
				// If killed by an explosion, play the explosion death animation. Else, play a random regular death animation
				if (deathBy == 1) {
					animator.Play("DieExplosion");
				} else {
					int r = Random.Range (1, 4);
					if (r == 1) {
						animator.Play ("Die");
					} else if (r == 2) {
						animator.Play ("DieHeadshot");
					} else {
						animator.Play("Die2");
					}
				}
			}
		}

		if (actionState == ActionStates.Pursue) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint"))
				animator.Play ("Sprint");
		}

		if (actionState == ActionStates.Idle) {
			if (alerted) {
				if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Firing")) {
					animator.Play ("Firing");
				}
			} else {
				if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
					animator.Play ("Idle");
			}
		}

		if (actionState == ActionStates.Firing || actionState == ActionStates.Reloading || actionState == ActionStates.InCover) {
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
				if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
					navMesh.isStopped = true;
				}
				if (isCrouching) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Crouching"))
						animator.Play ("Crouching");
				} else {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Aim"))
						animator.Play ("Aim");
				}
			} else if (currentBullets <= 0) {
				if (enemyType != EnemyType.Scout) {
					if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
						pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, true);
					}
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

		if (actionState == ActionStates.Disoriented) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Disoriented")) {
				animator.Play ("Disoriented");
			}
		}
	}

	float ScaleOffset(float dist) {
		float scaledOffset = (1f / accuracyOffset) * dist;
		if (scaledOffset > accuracyOffset) {
			return accuracyOffset;
		}
		return scaledOffset;
	}

	private void Fire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading)
			return;

		GameControllerScript.lastGunshotHeardPos = transform.position;
		if (player != null) {
			RaycastHit hit;
			// Locks onto the player and shoots at him
			Vector3 playerPos = player.GetComponent<FirstPersonController>().fpcTransformSpine.position;
			playerPos = new Vector3(playerPos.x, playerPos.y - 0.1f, playerPos.z);
			Vector3 dir = playerPos - shootPoint.position;

			// Adding artificial stupidity - ensures that the player isn't hit every time by offsetting
			// the shooting direction in x and y by two random numbers
			float scaledOffset = ScaleOffset(Vector3.Distance(playerPos, shootPoint.position));
			float xOffset = Random.Range (-scaledOffset, scaledOffset);
			float yOffset = Random.Range (-scaledOffset, scaledOffset);
			dir = new Vector3 (dir.x + xOffset, dir.y + yOffset, dir.z);
			//Debug.DrawRay (shootPoint.position, dir * range, Color.red);
			if (Physics.Raycast (shootPoint.position, dir, out hit)) {
				if (hit.transform.tag.Equals ("Player") || hit.transform.tag.Equals ("Human")) {
					pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal);
					if (hit.transform.tag.Equals ("Player")) {
						PlayerActionScript ps = hit.transform.GetComponent<PlayerActionScript> ();
						ps.TakeDamage((int)damage);
						//ps.ResetHitTimer ();
						ps.SetHitLocation (transform.position);
					} else {
						hit.transform.GetComponent<BetaEnemyScript>().TakeDamage((int)damage);
					}
				} else {
					pView.RPC ("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
					pView.RPC ("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
				}
			}
		}

		pView.RPC("RpcShootAction", RpcTarget.All);
	}

	[PunRPC]
	void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal) {
		GameObject bloodSpill = Instantiate(bloodEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bloodSpill.transform.Rotate (180f, 0f, 0f);
		Destroy (bloodSpill, 1.5f);
	}

	[PunRPC]
	void RpcInstantiateBulletHole(Vector3 point, Vector3 normal, string parentName) {
		GameObject attachToObject = GameObject.Find(parentName);
		if (!attachToObject) {
			return;
		}
		GameObject bulletHoleEffect = Instantiate (bulletImpact, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bulletHoleEffect.transform.SetParent (attachToObject.transform);
		Destroy (bulletHoleEffect, 3f);
	}


	[PunRPC]
	void RpcInstantiateHitParticleEffect(Vector3 point, Vector3 normal) {
		GameObject hitParticleEffect = Instantiate (hitParticles, point, Quaternion.FromToRotation (Vector3.up, normal));
		Destroy (hitParticleEffect, 1f);
	}

	[PunRPC]
	void RpcShootAction() {
		muzzleFlash.Play();
		PlayShootSound();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
		if (sniper && player != null) {
			SniperTracerScript s = sniperTracer.gameObject.GetComponent<SniperTracerScript> ();
			s.enabled = true;
			s.SetDistance (Vector3.Distance(shootPoint.position, player.transform.position));
			sniperTracer.enabled = true;
		}
	}

	private void PlayShootSound() {
		gunAudio.PlayOneShot (shootSound);
	}

	public void Reload() {
		currentBullets += bulletsPerMag;
	}

	[PunRPC]
	void RpcReload(int bullets) {
		currentBullets = bullets;
	}

	public void MeleeAttack() {
		if (playerToHit != null) {
			int r = Random.Range (0, 2);
			if (r == 0) {
				PlayVoiceClip (5);
			} else {
				PlayVoiceClip (13);
			}
			PlayerActionScript ps = playerToHit.GetComponent<PlayerActionScript> ();
			ps.TakeDamage (50);
			//ps.ResetHitTimer();
			ps.SetHitLocation (transform.position);
			if (Vector3.Distance(transform.position, playerToHit.transform.position) > MELEE_DISTANCE) {
				playerToHit = null;
			}
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
		headCollider.gameObject.layer = 15;
		RemoveHitboxes ();
		yield return new WaitForSeconds(5f);
		DespawnAction ();
		if (!sniper) {
			StartCoroutine ("Respawn");
		}
	}

	void DespawnAction() {
		if (enemyType == EnemyType.Patrol) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
				navMesh.ResetPath ();
				navMesh.isStopped = true;
				navMesh.enabled = false;
			}
		} else {
			navMeshObstacle.enabled = false;
		}
		modeler.DespawnPlayer();
		marker.enabled = false;
		gunRef.enabled = false;
		myCollider.enabled = false;
	}

	void RemoveHitboxes() {
		myCollider.height = 0f;
		myCollider.center = new Vector3 (0f, 0f, 0f);
		meleeTrigger.enabled = false;
	}

	// b is the mode the AI is in. 0 means override everything and take cover, 1 is override everything and leave cover
	// 2 is use the natural timer to decide
	// Used for Scout AI
	void TakeCoverScout() {
		if (crouchMode == 0) {
			coverWaitTimer = 0f;
			if (!inCover) {
				inCover = true;
				//pView.RPC ("RpcSetInCover", RpcTarget.All, true);
			}
		} else if (crouchMode == 1) {
			if (coverWaitTimer <= 0f) {
				coverWaitTimer = Random.Range (4f, 15f);
				//pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer);
			}
			if (inCover) {
				inCover = false;
				//pView.RPC ("RpcSetInCover", RpcTarget.All, false);
			}
		} else {
			if (coverWaitTimer <= 0f && !inCover) {
				coverTimer = Random.Range (3f, 7f);
				inCover = true;
				//pView.RPC ("RpcSetCoverTimer", RpcTarget.Others, coverTimer);
				//pView.RPC ("RpcSetInCover", RpcTarget.All, true);
			} else if (coverTimer <= 0f && inCover) {
				coverWaitTimer = Random.Range (4f,15f);
				inCover = false;
				//pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer);
				//pView.RPC ("RpcSetInCover", RpcTarget.All, false);
			}
		}
	}

	[PunRPC]
	void RpcSetCoverTimer(float f) {
		coverTimer = f;
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
			Vector3 spot = coverSpots [Random.Range (1, coverSpots.Length)].position;
			coverPos = spot;
			//pView.RPC ("RpcSetCoverPos", RpcTarget.All, true, spot.x, spot.y, spot.z);
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
					// Find objects near cover spot, and search the array for AI teammates
					Collider[] objectsNearCover = Physics.OverlapSphere(coverPos, 5f);
					bool coverIsGood = true;
					foreach(Collider go in objectsNearCover) {
						if (go.name.Contains("Cicada")) {
							coverIsGood = false;
							break;
						}
					}
					if (coverIsGood) {
						return true;
					}

					//pView.RPC ("RpcSetCoverPos", RpcTarget.All, true, coverSpots[i].position.x, coverSpots[i].position.y, coverSpots[i].position.z);
				}
			}

		}
		return false;
	}

	[PunRPC]
	void RpcSetCoverPos(bool n, float x, float y, float z) {
		if (!n) {
			coverPos = Vector3.negativeInfinity;
		} else {
			coverPos = new Vector3 (x, y, z);
		}
	}

	private bool navMeshReachedDestination(float bufferRange) {
		if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.pathPending && !navMesh.isStopped) {
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
		if (player == null || player.GetComponent<PlayerActionScript>().health <= 0) {
			ArrayList keysNearBy = new ArrayList ();
			foreach (GameObject p in GameControllerScript.playerList.Values) {
				if (!p || p.GetComponent<PlayerActionScript>().health <= 0)
					continue;
				if (Vector3.Distance (transform.position, p.transform.position) < range + 20f) {
					Vector3 toPlayer = p.transform.position - transform.position;
					float angleBetween = Vector3.Angle (transform.forward, toPlayer);
					if (angleBetween <= 90f) {
						// Cast a ray to make sure there's nothing in between the player and the enemy
						Debug.DrawRay (headTransform.position, toPlayer, Color.blue);
						Transform playerHead = p.GetComponent<FirstPersonController>().headTransform;
						RaycastHit hit1;
						RaycastHit hit2;
						// Vector3 middleHalfCheck = new Vector3 (p.transform.position.x, p.transform.position.y + PLAYER_HEIGHT_OFFSET, p.transform.position.z);
						Vector3 middleHalfCheck = new Vector3 (playerHead.position.x, playerHead.position.y - 0.1f, playerHead.position.z);
						Vector3 topHalfCheck = new Vector3 (playerHead.position.x, playerHead.position.y, playerHead.position.z);
						if (!Physics.Linecast (headTransform.position, middleHalfCheck, out hit2))
						{
								continue;
						}
						if (!Physics.Linecast (headTransform.position, topHalfCheck, out hit1))
						{
								continue;
						}
						
						if (hit1.transform.gameObject == null || hit2.transform.gameObject == null)
						{
							continue;
						}
						if (!hit1.transform.gameObject.tag.Equals("Player") && !hit2.transform.gameObject.tag.Equals("Player")) {
							// If we don't see a player, check if player is in close range.
							// Check objects within a certain distance for a player
							if (!alerted) {
								if (Vector3.Distance(p.transform.position, headTransform.position) < 8f) {
									suspicious = true;
									// Debug.Log("I hear sum body");
								}
								else {
									suspicious = false;
									// Debug.Log("Guess it was my imagination");
								}
							}
							continue;
						}
						keysNearBy.Add (p.GetComponent<PhotonView>().OwnerActorNr);
					}
				}
			}

			if (keysNearBy.Count != 0) {
				pView.RPC ("RpcSetTarget", RpcTarget.All, (int)keysNearBy [Random.Range (0, keysNearBy.Count)]);
			}
		} else {
			// If we do, check if it's still in range
			if (Vector3.Distance (transform.position, player.transform.position) >= range + 20f) {
				pView.RPC ("RpcSetLastSeenPlayerPos", RpcTarget.All, true, player.transform.position.x, player.transform.position.y, player.transform.position.z);
				pView.RPC ("RpcSetTarget", RpcTarget.All, -1);
			}
		}
	}

	[PunRPC]
	void RpcSetLastSeenPlayerPos(bool n, float x, float y, float z) {
		if (!n) {
			lastSeenPlayerPos = Vector3.negativeInfinity;
		} else {
			lastSeenPlayerPos = new Vector3 (x, y, z);
		}
	}

	public void TakeDamage(int d) {
		pView.RPC ("RpcTakeDamage", RpcTarget.All, d);
	}

	[PunRPC]
	public void RpcTakeDamage(int d) {
		health -= d;
	}

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action) {
		//{Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking}
		if (action == ActionStates.Firing || action == ActionStates.Moving || action == ActionStates.Reloading || action == ActionStates.Pursue || action == ActionStates.TakingCover || action == ActionStates.InCover) {
			int r = Random.Range (0, 3);
			if (r == 1) {
				StartCoroutine (PlayVoiceClipDelayed(Random.Range (1, 5), Random.Range(2f, 50f)));
			} else if (r != 0) {
				StartCoroutine (PlayVoiceClipDelayed(Random.Range (6, 13), Random.Range(2f, 50f)));
			}
		}
		// Play grunt when enemy dies or hit by flashbang
		if (action == ActionStates.Dead) {
			PlayGruntSound();
			removeFromMarkerList();
			AddToMarkerRemovalQueue();
		}
		if (action == ActionStates.Disoriented) {
			PlayGruntSound();
		}
		actionState = action;
	}

	[PunRPC]
	private void RpcUpdateFiringState(FiringStates firing) {
		firingState = firing;
	}

	void CheckTargetDead() {
		if (player != null && player.GetComponent<PlayerActionScript> ().health <= 0f) {
			pView.RPC ("RpcSetTarget", RpcTarget.All, -1);
		}
	}

	[PunRPC]
	void RpcSetTarget(int id) {
		if (id == -1) {
			player = null;
		} else {
			player = (GameObject)GameControllerScript.playerList [id];
		}
	}

	[PunRPC]
	void RpcUpdateAlertedStatus() {
		alerted = false;
		GameControllerScript.lastGunshotHeardPos = Vector3.negativeInfinity;
		player = null;
		lastSeenPlayerPos = Vector3.negativeInfinity;
	}

	// Reset values to respawn
	IEnumerator Respawn() {
		yield return new WaitForSeconds (44f);
		if (gameController.GetComponent<GameControllerScript>().assaultMode) {
			RespawnAction ();
		} else {
			StartCoroutine ("Respawn");
		}
	}

	void RespawnAction () {
		myCollider.height = originalColliderHeight;
		myCollider.center = new Vector3 (originalColliderCenter.x, originalColliderCenter.y, originalColliderCenter.z);
		myCollider.enabled = true;
		gameObject.layer = 14;
		headCollider.gameObject.layer = 13;
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
		animator.Play ("Idle");
		firingState = FiringStates.StandingStill;
		firingModeTimer = 0f;

		wanderStallDelay = -1f;
		coverPos = Vector3.negativeInfinity;
		crouchMode = 2;
		coverScanRange = 50f;

		meleeTrigger.enabled = true;
		modeler.RespawnPlayer();
		marker.enabled = true;
		gunRef.enabled = true;

		if (enemyType == EnemyType.Patrol) {
			navMesh.enabled = true;
		} else {
			navMeshObstacle.enabled = true;
		}
	}

	void StopVoices() {
		audioSource.Stop();
	}

	void HandleEnemyAlerts() {
		if (health <= 0 || gameController.GetComponent<GameControllerScript>().assaultMode) {
			return;
		}

		// Activate exclamation sign, and disable question mark
		if (alerted && alertStatus != 2) {
			gameController.GetComponent<GameControllerScript>().enemyAlertMarkers.Add(pView.ViewID);
			alertStatus = 2;
		} else if (suspicious && alertStatus != 1) {
			gameController.GetComponent<GameControllerScript>().enemyAlertMarkers.Add(pView.ViewID);
			alertStatus = 1;
		} else {
			removeFromMarkerList();
		}

	}

	void removeFromMarkerList() {
		if (enemyAlertMarkers == null || alertStatus == 0) {
			return;
		}
		foreach (int item in enemyAlertMarkers) {
			if ((int)item == pView.ViewID) {
				enemyAlertMarkers.Remove(item);
				break;
			}
		}
		alertStatus = 0;
	}

	void AddToMarkerRemovalQueue() {
		gameController.GetComponent<GameControllerScript>().enemyMarkerRemovalQueue.Enqueue(pView.ViewID);
	}

	void EnsureNotSuspiciousAndAlerted() {
		if (alerted) {
			suspicious = false;
		}
	}

	// Draw a sphere to see effective range for stealth
	// void OnDrawGizmos() {
	// 		Gizmos.color = Color.yellow;
	// 		Gizmos.DrawSphere(headTransform.position, 8f);
	// }

}
