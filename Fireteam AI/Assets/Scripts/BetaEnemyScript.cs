using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;
using SpawnMode = GameControllerScript.SpawnMode;
using UnityEngine.SceneManagement;

public class BetaEnemyScript : MonoBehaviour, IPunObservable {
	public static int NUMBER_KILLED;
	private const float MELEE_DISTANCE = 1.7f;
	private const float PLAYER_HEIGHT_OFFSET = 1f;
	private const float DETECTION_OUTLINE_MAX_TIME = 10f;
	private const float MAX_SUSPICION_LEVEL = 100f;
	// Scan for players every 0.8 of a second instead of every frame
	private const float PLAYER_SCAN_DELAY = 0.8f;
	private const float ENV_DAMAGE_DELAY = 0.5f;
	private const int ENEMY_FIRE_IGNORE = ~(1 << 14 | 1 << 13);
	private const int OBSCURE_IGNORE = ~(1 << 14 | 1 << 15 | 1 << 16 | 1 << 17 | 1 << 22);
	private const float EXPLOSION_FORCE = 75;
	private const float BULLET_FORCE = 50f;
	private int HEALTH_KIT_DROP_CHANCE = 33;
	private int AMMO_KIT_DROP_CHANCE = 17;
	private const int POISONED_DMG = 5;
	private const float POISONED_INTERVAL = 1f;

	// Prefab references
	public GameObject ammoBoxPickup;
	public GameObject healthBoxPickup;
	public AudioClip[] voiceClips;
	public AudioClip[] gruntSounds;
	public GameObject bloodEffect;
	public GameObject bloodEffectHeadshot;
	public GameObject overshieldHitEffect;

	// Body/Component references
	public AudioSource audioSource;
	public PhotonView pView;
	public Collider mainCol;
	public Rigidbody mainRigid;
	public Transform headTransform;
	public Transform torsoTransform;
	public Transform leftArmTransform;
	public Transform leftForeArmTransform;
	public Transform rightArmTransform;
	public Transform rightForeArmTransform;
	public Transform pelvisTransform;
	public Transform leftUpperLegTransform;
	public Transform leftLowerLegTransform;
	public Transform rightUpperLegTransform;
	public Transform rightLowerLegTransform;
	public LineRenderer sniperTracer;
	public Animator animator;
	public Rigidbody[] ragdollBodies;
	public SpriteRenderer marker;
	public EnemyModelCreator modeler;
	public NavMeshAgent navMesh;
	public NavMeshObstacle navMeshObstacle;
	public WeaponMeta gunRef;
	private Vector3 prevNavDestination;
	private bool prevWasStopped;
	private bool insideBubbleShield;

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
	private int thisHealthDropChanceBoost;
	private int thisAmmoDropChanceBoost;
	private int lastHitBy;
	private int lastBodyPartHit;
	public float disorientationTime;
	private Vector3 spawnPos;
	// The alert state for the enemy. None = enemy is neutral; Suspicious = enemy is suspicious (?); alert = enemy is alerted (!)
	public enum AlertStatus {Neutral, Suspicious, Alert};
	public AlertStatus alertStatus;
	private enum HealthStatus {Neutral, Poisoned};
	private HealthStatus healthStatus;
	private bool wasMasterClient;
	public GameObject gameController;
	public GameControllerScript gameControllerScript;
	public bool isOutlined;
	public float initialSpawnTime;

	// Finite state machine states
	public enum ActionStates {Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking, Disoriented};
	// FSM used for determining movement while attacking and not in cover
	enum FiringStates {StandingStill, StrafeLeft, StrafeRight, Backpedal, Forward};

	// Type of enemy
	public enum EnemyType {Patrol, Scout};

	// Gun/weapon stuff
	public float range;
	private float alertRange;
	private float neutralRange;
	public int bulletsPerMag = 30;
	public int currentBullets;
	private bool isReloading = false;
	public float fireRate = 0.4f;
	float fireTimer = 0.0f; // Once it equals fireRate, it will allow us to shoot

	// Target references
	public GameObject playerTargeting;
	public Vector3 lastSeenPlayerPos = Vector3.negativeInfinity;

	// All patrol pathfinding points for an enemy
	public GameObject[] navPoints;

	// Timers
	// Amount of time remaining before next player scan check
	private float playerScanTimer = 0f;
	// Time in cover
	private float coverTimer = 0f;
	// Time to wait to be in cover again
	private float coverWaitTimer = 0f;
	// Time to wait to maneuver to another cover position
	public float coverSwitchPositionsTimer = 0f;
	// Time to change firing positions
	private float firingModeTimer = 0f;
	private float detectionOutlineTimer = 0f;
	// Gauage for how suspicious the enemy is during stealth mode
	public float suspicionMeter = 0f;
	// Time to wait before the suspicion meter starts cooling down
	private float suspicionCoolDownDelay = 0f;
	// Time to wait before the enemy can start becoming suspicious again
    private float increaseSuspicionDelay = 0f;
    private float alertTeamAfterAlertedTimer = 6f;
	private float actionTransitionDelay = 0f;
	// Responsible for putting a delay between damage done by the environment like fire, gas, etc.
	private float envDamageTimer;
	private bool syncSuspicionValuesSemiphore = false;

	public float wanderStallDelay = -1f;
	private bool inCover;
	private Transform coverPos;
	// Crouching status of the enemy. Tells the enemy what to do in regard to crouching. 
	// 0 means override everything and take cover; 1 is override everything and leave cover; 2 is use the natural timer to decide
	private enum CrouchMode {ForceCover, ForceLeaveCover, Natural};
	private CrouchMode crouchMode;
	private float coverScanRange = 22f;
	private Vector3 lastHitFromPos;
	private float poisonTimer;
	private int poisonedById;
	// public Transform pNav;

    // Testing mode - set in inspector
    //public bool testingMode;

	void Awake() {
		ToggleRagdoll(false);
		if (gameControllerScript.matchType == 'C')
        {
            StartForCampaign();
        } else if (gameControllerScript.matchType == 'V')
        {
            StartForVersus();
        }
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
		if (!PhotonNetwork.IsMasterClient && !gameControllerScript.isVersusHostForThisTeam() && !pView.IsMine) {
			pView.RPC("RpcAskServerForDataEnemies", RpcTarget.All);
		}
	}

    // void Start()
    // {
    //     if (gameControllerScript.matchType == 'C')
    //     {
    //         StartForCampaign();
    //     } else if (gameControllerScript.matchType == 'V')
    //     {
    //         StartForVersus();
    //     }
    // }

    // Use this for initialization
    void StartForCampaign () {
		playerScanTimer = PLAYER_SCAN_DELAY;
		alertStatus = AlertStatus.Neutral;
		healthStatus = HealthStatus.Neutral;
		crouchMode = CrouchMode.Natural;
		coverWaitTimer = Random.Range (2f, 7f);
		coverSwitchPositionsTimer = Random.Range (12f, 18f);

		playerTargeting = null;
		spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		health = 100;
		disorientationTime = 0f;
		currentBullets = bulletsPerMag;
		audioSource.maxDistance = 100f;
		isCrouching = false;
		isOutlined = false;

		coverTimer = 0f;
		inCover = false;

		gameControllerScript.enemyList.Add(pView.ViewID, gameObject);

		if (enemyType == EnemyType.Patrol) {
			range = 20f;
			accuracyOffset = 1.05f;
			fireRate = 0.4f;
			aggression = 14;
		} else {
			if (sniper) {
				range = 35f;
				accuracyOffset = 1.25f;
				fireRate = 20f;
			} else {
				range = 27f;
				accuracyOffset = 1.05f;
				fireRate = 0.4f;
			}
		}

		alertRange = range * 2.5f;
		neutralRange = range;

		prevWasStopped = true;
		prevNavDestination = Vector3.negativeInfinity;

		if (!PhotonNetwork.IsMasterClient) {
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
			wasMasterClient = false;
		} else {
			wasMasterClient = true;
		}

		if (gameControllerScript.spawnMode == SpawnMode.Paused) {
			StartCoroutine (Respawn(initialSpawnTime, false));
		}

	}

    void StartForVersus()
    {
		playerScanTimer = PLAYER_SCAN_DELAY;
        alertStatus = AlertStatus.Neutral;
		healthStatus = HealthStatus.Neutral;
		crouchMode = CrouchMode.Natural;
        coverWaitTimer = Random.Range(2f, 7f);
        coverSwitchPositionsTimer = Random.Range(12f, 18f);

        playerTargeting = null;
        spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        health = 100;
        disorientationTime = 0f;
        currentBullets = bulletsPerMag;
        audioSource.maxDistance = 100f;
        isCrouching = false;
        isOutlined = false;

        coverTimer = 0f;
        inCover = false;

        gameControllerScript.enemyList.Add(pView.ViewID, gameObject);

        if (enemyType == EnemyType.Patrol)
        {
            range = 20f;
            accuracyOffset = 1.05f;
            fireRate = 0.4f;
            aggression = 14;
        }
        else
        {
            if (sniper)
            {
                range = 35f;
                accuracyOffset = 1.25f;
                fireRate = 20f;
            }
            else
            {
                range = 27f;
                accuracyOffset = 1.05f;
                fireRate = 0.4f;
            }
        }

		alertRange = range * 2.5f;
		neutralRange = range;

        prevWasStopped = true;
        prevNavDestination = Vector3.negativeInfinity;

        if (!gameControllerScript.isVersusHostForThisTeam())
        {
            navMesh.enabled = false;
            navMeshObstacle.enabled = false;
            wasMasterClient = false;
        }
        else
        {
            wasMasterClient = true;
        }

		if (gameControllerScript.spawnMode == SpawnMode.Paused) {
			StartCoroutine (Respawn(initialSpawnTime, false));
		}

    }

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (gameControllerScript != null && gameControllerScript.matchType == 'V') {
			SerializeViewVersus(stream, info);
		} else {
			SerializeViewCampaign(stream, info);
		}
	}

	void SerializeViewCampaign(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.IsWriting)
		{
			stream.SendNext(this.suspicionMeter);
		}
		else
		{
			this.suspicionMeter = (float)stream.ReceiveNext();
		}
	}

	void SerializeViewVersus(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.IsWriting)
		{
			stream.SendNext(gameControllerScript.teamMap);
			stream.SendNext(this.suspicionMeter);
		}
		else
		{
			string team = stream.ReceiveNext().ToString();

            if (team != gameControllerScript.teamMap) return;

			this.suspicionMeter = (float)stream.ReceiveNext();
		}
	}

    void Update()
    {
        if (gameControllerScript.matchType == 'C')
        {
            UpdateForCampaign();
        } else if (gameControllerScript.matchType == 'V')
        {
            UpdateForVersus();
        }
		// PrintNavMeshAgentStats();
    }

    // Update is called once per frame
    void UpdateForCampaign () {
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
				UpdateActionState(ActionStates.Idle);
			}
		} else {
			wasMasterClient = false;
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
		}

		UpdateActionTransitionDelay();
		UpdateEnvDamageTimer();
		UpdateDisorientationTime();
		ReplenishFireRate ();
		UpdateFiringModeTimer ();

		if (!PhotonNetwork.IsMasterClient || animator.GetCurrentAnimatorStateInfo(0).IsName("Die") || animator.GetCurrentAnimatorStateInfo(0).IsName("DieHeadshot")) {
			// if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
			// 	StopVoices();
			// }
			return;
		}

		HandleHealthStatus();
		CheckForGunfireSounds ();
		CheckTargetDead ();

		if (enemyType == EnemyType.Patrol) {
			DecideActionPatrolInCombat();
			DecideActionPatrol ();
			HandleMovementPatrol ();
		} else {
			DecideActionScout ();
		}

		// If disoriented, don't have the ability to do anything else except die
		if (actionState == ActionStates.Dead || actionState == ActionStates.Disoriented) return;

		// Shoot at player
		// Add !isCrouching if you don't want the AI to fire while crouched behind cover
		if (actionState == ActionStates.Firing || (actionState == ActionStates.InCover && playerTargeting != null)) {
			if (currentBullets > 0) {
				Fire ();
			}
		}

	}

    void UpdateForVersus()
    {
		if (gameControllerScript.isVersusHostForThisTeam()) {
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
				UpdateActionState(ActionStates.Idle);
			}
		} else {
			wasMasterClient = false;
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
		}

		UpdateActionTransitionDelay();
		UpdateEnvDamageTimer();
		UpdateDisorientationTime();
		ReplenishFireRate ();
		UpdateFiringModeTimer ();

		if (!gameControllerScript.isVersusHostForThisTeam() || animator.GetCurrentAnimatorStateInfo(0).IsName("Die") || animator.GetCurrentAnimatorStateInfo(0).IsName("DieHeadshot")) {
			if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
				// StopVoices();
			}
			return;
		}

		CheckForGunfireSounds ();
		CheckTargetDead ();

		if (enemyType == EnemyType.Patrol) {
			DecideActionPatrolInCombat();
			DecideActionPatrol ();
			HandleMovementPatrol ();
		} else {
			DecideActionScout ();
		}
		
		// If disoriented, don't have the ability to do anything else except die
		if (actionState == ActionStates.Dead || actionState == ActionStates.Disoriented) return;

		// Shoot at player
		// Add !isCrouching if you don't want the AI to fire while crouched behind cover
		if (actionState == ActionStates.Firing || (actionState == ActionStates.InCover && playerTargeting != null)) {
			if (currentBullets > 0 && actionTransitionDelay <= 0f) {
				Fire ();
			}
		}
    }

	// Alert other enemy team members to sound the alarm if alerted and timer runs out
	void CheckAlertTeamAfterAlerted() {
		if (!gameControllerScript.assaultMode && alertStatus == AlertStatus.Alert) {
			if (actionState != ActionStates.Disoriented) {
				alertTeamAfterAlertedTimer -= Time.deltaTime;
				if (actionState != ActionStates.Dead && alertTeamAfterAlertedTimer <= 0f) {
					if (!gameControllerScript.assaultMode) {
						gameControllerScript.UpdateAssaultMode();
					}
				}
			}
		}
	}

	void FixedUpdate() {
		if (gameControllerScript.matchType == 'C') {
			FixedUpdateForCampaign();
		} else if (gameControllerScript.matchType == 'V') {
			FixedUpdateForVersus();
		}
	}

	void FixedUpdateForCampaign() {
		// Test for detection outline
		// if (Input.GetKeyDown(KeyCode.M)) {
		// 	isOutlined = !isOutlined;
		// 	ToggleDetectionOutline(isOutlined);
		// }
		if (health <= 0) {
			if (isOutlined) {
				isOutlined = false;
				detectionOutlineTimer = 0f;
				ToggleDetectionOutline(false);
			}
			//removeFromMarkerList();
			if (!PhotonNetwork.IsMasterClient) {
				actionState = ActionStates.Dead;
			}
		}

		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Die") || animator.GetCurrentAnimatorStateInfo (0).IsName ("DieHeadshot")) {
			if (PhotonNetwork.IsMasterClient && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
			return;
		}
		// Handle animations and detection outline independent of frame rate
		DecideAnimation ();
		HandleDetectionOutline();
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Disoriented")) {
			if (PhotonNetwork.IsMasterClient && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
			return;
		}
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void FixedUpdateForVersus() {
		// Test for detection outline
		// if (Input.GetKeyDown(KeyCode.M)) {
		// 	isOutlined = !isOutlined;
		// 	ToggleDetectionOutline(isOutlined);
		// }
		if (health <= 0) {
			if (isOutlined) {
				isOutlined = false;
				detectionOutlineTimer = 0f;
				ToggleDetectionOutline(false);
			}
			//removeFromMarkerList();
			if (!gameControllerScript.isVersusHostForThisTeam()) {
				actionState = ActionStates.Dead;
			}
		}

		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Die") || animator.GetCurrentAnimatorStateInfo (0).IsName ("DieHeadshot")) {
			if (gameControllerScript.isVersusHostForThisTeam() && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
			return;
		}
		// Handle animations and detection outline independent of frame rate
		DecideAnimation ();
		HandleDetectionOutline();
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Disoriented")) {
			if (gameControllerScript.isVersusHostForThisTeam() && navMesh && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
			return;
		}
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void LateUpdate() {
		if (gameControllerScript.matchType == 'C') {
			LateUpdateForCampaign();
		} else if (gameControllerScript.matchType == 'V') {
			LateUpdateForVersus();
		}
	}

	void LateUpdateForCampaign() {
		if (!PhotonNetwork.IsMasterClient || health <= 0)
			return;
		// If the enemy sees the player, rotate the enemy towards the player only if the enemy is aiming at the player
		if (playerTargeting != null && ShouldRotateTowardsPlayerTarget()) {
			RotateTowardsPlayer();
		}

	}

	void LateUpdateForVersus() {
		if (!gameControllerScript.isVersusHostForThisTeam() || health <= 0)
			return;
		// If the enemy sees the player, rotate the enemy towards the player only if the enemy is aiming at the player
		if (playerTargeting != null && ShouldRotateTowardsPlayerTarget()) {
			RotateTowardsPlayer();
		}
	}

	bool ShouldRotateTowardsPlayerTarget() {
		if (actionState == ActionStates.Firing || actionState == ActionStates.Melee || actionState == ActionStates.Reloading || actionState == ActionStates.InCover) {
			return true;
		}
		return false;
	}

	void ReplenishFireRate() {
		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
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

	void UpdateActionTransitionDelay()
	{
		if (actionTransitionDelay > 0f) {
			actionTransitionDelay -= Time.deltaTime;
		}
	}

	void ToggleRagdoll(bool b)
	{
		animator.enabled = !b;
		mainCol.enabled = !b;
		mainRigid.isKinematic = b;
		mainRigid.useGravity = !b;

		foreach (Rigidbody rb in ragdollBodies)
		{
			rb.isKinematic = !b;
			rb.useGravity = b;
		}

		// headTransform.GetComponent<Collider>().enabled = b;
		// torsoTransform.GetComponent<Collider>().enabled = b;
		// leftArmTransform.GetComponent<Collider>().enabled = b;
		// leftForeArmTransform.GetComponent<Collider>().enabled = b;
		// rightArmTransform.GetComponent<Collider>().enabled = b;
		// rightForeArmTransform.GetComponent<Collider>().enabled = b;
		// pelvisTransform.GetComponent<Collider>().enabled = b;
		// leftUpperLegTransform.GetComponent<Collider>().enabled = b;
		// leftLowerLegTransform.GetComponent<Collider>().enabled = b;
		// rightUpperLegTransform.GetComponent<Collider>().enabled = b;
		// rightLowerLegTransform.GetComponent<Collider>().enabled = b;

		ToggleUpdateWhenOffscreen(b);
	}

	void CheckForGunfireSounds() {
		if (!Vector3.Equals (GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
			SetAlertStatus(AlertStatus.Alert);
		}
	}

	public void SetAlerted() {
		SetAlertStatus(AlertStatus.Alert);
	}

	// Sets the alert status on the enemy (neutral, alert, suspicious)
	// Number passed in can be 0 (AlertStatus.Neutral), 1 (AlertStatus.Suspicious), or 2 (AlertStatus.Alert) as in accordance with the AlertStatus enum
	void SetAlertStatus(AlertStatus a) {
		if (alertStatus != a) {
			pView.RPC ("RpcSetAlertStatus", RpcTarget.All, (int)a, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	void RpcSetAlertStatus(int statusNumber, string team) {
        if (team != gameControllerScript.teamMap) return;
		alertStatus = (AlertStatus)statusNumber;
		AdjustRangeForAlertStatus();
		actionTransitionDelay = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>().GetDdosDelayTime();
	}

	void AdjustRangeForAlertStatus() {
		if (alertStatus == AlertStatus.Alert) {
			range = alertRange;
		} else {
			range = neutralRange;
		}
	}

	[PunRPC]
	void RpcSetIsCrouching(bool b, string team)
	{
        if (team != gameControllerScript.teamMap) return;
        isCrouching = b;
	}

	void RotateTowardsPlayer() {
		Vector3 rotDir = (playerTargeting.transform.position - transform.position).normalized;
		Quaternion lookRot = Quaternion.LookRotation (rotDir);
		Quaternion tempQuat = Quaternion.Slerp (transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
		Vector3 tempRot = tempQuat.eulerAngles;
		transform.rotation = Quaternion.Euler (new Vector3 (0f, tempRot.y, 0f));
	}

	[PunRPC]
	void RpcSetWanderStallDelay(float f, string team) {
        if (team != gameControllerScript.teamMap) return;
        wanderStallDelay = f;
	}

	void SetNavMeshDestination(Vector3 dest) {
		if (!Vector3.Equals(navMesh.destination, dest)) {
			pView.RPC("RpcSetNavMeshDestination", RpcTarget.All, dest.x, dest.y, dest.z, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	void RpcSetNavMeshDestination(float x, float y, float z, string team) {
        if (team != gameControllerScript.teamMap) return;
		if (gameControllerScript.matchType == 'V') {
			SetNavMeshDestinationForVersus(x, y, z);
		} else {
			SetNavMeshDestinationForCampaign(x, y, z);
		}
	}

	void SetNavMeshDestinationForCampaign(float x, float y, float z) {
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

	void SetNavMeshDestinationForVersus(float x, float y, float z) {
		if (gameControllerScript.isVersusHostForThisTeam()) {
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
	void RpcSetInCover(bool n, string team) {
        if (team != gameControllerScript.teamMap) return;
        inCover = n;
	}

	[PunRPC]
	void RpcSetFiringModeTimer(float t, string team) {
        if (team != gameControllerScript.teamMap) return;
        firingModeTimer = t;
	}

	void HandleMovementPatrol() {
		bool isMaster = (gameControllerScript.matchType == 'C' && PhotonNetwork.IsMasterClient) || (gameControllerScript.matchType == 'V' && gameControllerScript.isVersusHostForThisTeam());
		// Melee attack trumps all
		if (actionState == ActionStates.Melee || actionState == ActionStates.Dead || actionState == ActionStates.Disoriented) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
			return;
		}
		// Handle movement for wandering
		if (actionState == ActionStates.Wander) {
			if (isMaster) {
				if (alertStatus == AlertStatus.Alert) {
						UpdateNavMeshSpeed(4.5f);
				} else {
					UpdateNavMeshSpeed(1.5f);
				}
				// Only server should be updating the delays and they should sync across the network
				// Initial spawn value
				if (wanderStallDelay == -1f) {
					wanderStallDelay = Random.Range (0f, 7f);
					pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay, gameControllerScript.teamMap);
				}
				// Take away from the stall delay if the enemy is standing still
				if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
					wanderStallDelay -= Time.deltaTime;
					//pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay, gameControllerScript.teamMap);
				} else {
					// Else, check if the enemy has reached its destination
					if (navMeshReachedDestination (2f)) {
						SetNavMeshStopped(true);
					}
				}
				// If the stall delay is done, the enemy needs to move to a wander point
				if (wanderStallDelay <= 0f && navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
					int r = Random.Range (0, navPoints.Length);
					RotateTowards (navPoints [r].transform.position);
					SetNavMeshDestination(navPoints [r].transform.position);
					wanderStallDelay = Random.Range (0f, 7f);
                    //pView.RPC ("RpcSetWanderStallDelay", RpcTarget.Others, wanderStallDelay, gameControllerScript.teamMap);
                }
            }
		}

		if (actionState == ActionStates.Idle) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
				pView.RPC ("RpcSetWanderStallDelay", RpcTarget.All, -1f, gameControllerScript.teamMap);
			}
		}

		if (actionState == ActionStates.Dead || actionState == ActionStates.InCover) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && !navMesh.isStopped) {
				SetNavMeshStopped(true);
			}
		}

		if (actionState == ActionStates.Pursue && !lastSeenPlayerPos.Equals(Vector3.negativeInfinity)) {
			if (!Vector3.Equals(navMesh.destination, lastSeenPlayerPos)) {
				UpdateNavMeshSpeed(6f);
				SetNavMeshDestination(lastSeenPlayerPos);
				pView.RPC ("RpcSetLastSeenPlayerPos", RpcTarget.All, false, 0f, 0f, 0f, gameControllerScript.teamMap);
			}
		}

		if (actionState == ActionStates.Seeking) {
			// Seek behavior: use navMesh to move towards the last area of gunshot. If the enemy moves towards that location
			// and there's nobody there, go back to wandering the area

			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
				SetNavMeshDestination(GameControllerScript.lastGunshotHeardPos);
				if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint")) {
					UpdateNavMeshSpeed(6f);
				} else {
					UpdateNavMeshSpeed(4f);
				}
			}
		}

		if (actionState == ActionStates.TakingCover) {
			// If the enemy is not near the cover spot, run towards it
			if (coverPos != null) {
				UpdateNavMeshSpeed(6f);
				SetNavMeshDestination(coverPos.position);
				pView.RPC ("RpcSetCoverPos", RpcTarget.All, coverPos.GetComponent<CoverSpotScript>().coverId, false, 0f, 0f, 0f, gameControllerScript.teamMap);
			} else {
				// If the enemy has finally reached cover, then he will get into cover mode
				if (navMeshReachedDestination(1.5f)) {
					// Done
					SetNavMeshStopped(true);
					UpdateActionState(ActionStates.InCover);
				}
			}
		}

		if (actionState == ActionStates.Firing) {
			UpdateNavMeshSpeed(4f);
			if (firingModeTimer <= 0f) {
				int r = Random.Range (0, 5);
				if (r == 0) {
					if (firingState != FiringStates.StandingStill) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StandingStill, gameControllerScript.teamMap);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
					//pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer, gameControllerScript.teamMap, gameControllerScript.teamMap);
				} else if (r == 1) {
					if (firingState != FiringStates.Forward) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.Forward, gameControllerScript.teamMap);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
                    //pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer, gameControllerScript.teamMap);
                    UpdateNavMeshSpeed(4f);
				} else if (r == 2) {
					if (firingState != FiringStates.Backpedal) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.Backpedal, gameControllerScript.teamMap);
					}
					firingModeTimer = Random.Range (2f, 3.2f);
                    //pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer, gameControllerScript.teamMap);
                    UpdateNavMeshSpeed(3f);
				} else if (r == 3) {
					if (firingState != FiringStates.StrafeLeft) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StrafeLeft, gameControllerScript.teamMap);
					}
					firingModeTimer = 1.7f;
                    //pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer, gameControllerScript.teamMap);
                    UpdateNavMeshSpeed(2.5f);
				} else if (r == 4) {
					if (firingState != FiringStates.StrafeRight) {
						pView.RPC ("RpcUpdateFiringState", RpcTarget.All, FiringStates.StrafeRight, gameControllerScript.teamMap);
					}
					firingModeTimer = 1.7f;
                    //pView.RPC ("RpcSetFiringModeTimer", RpcTarget.Others, firingModeTimer, gameControllerScript.teamMap);
                    UpdateNavMeshSpeed(2.5f);
				}
			}

			if (firingState == FiringStates.StandingStill) {
				SetNavMeshStopped(true);
			}

			if (playerTargeting != null && navMesh.isOnNavMesh) {
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
	void RpcSetCoverWaitTimer(float t, string team) {
        if (team != gameControllerScript.teamMap) return;
        coverWaitTimer = t;
	}

	[PunRPC]
	void RpcSetCoverSwitchPositionsTimer(float t, string team) {
        if (team != gameControllerScript.teamMap) return;
        coverSwitchPositionsTimer = t;
	}

	// Action Decision while in combat
	void DecideActionPatrolInCombat() {
		if (actionState == ActionStates.InCover || actionState == ActionStates.Firing) {
            // Three modes in cover - defensive, offensive, maneuvering; only used when engaging a player
            if (playerTargeting != null) {
				if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh && navMesh.isStopped) {
					coverWaitTimer -= Time.deltaTime;
					//pView.RPC ("RpcSetCoverWaitTimer", 	RpcTarget.Others, coverWaitTimer, gameControllerScript.teamMap);
				}
				// If the cover wait timer has ran out, switch from defensive to offensive and vice versa
				if (coverWaitTimer <= 0f && !isReloading) {
					pView.RPC ("RpcSetIsCrouching", RpcTarget.All, !isCrouching, gameControllerScript.teamMap);
					coverWaitTimer = Random.Range (2f, 7f);
                    //pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer, gameControllerScript.teamMap);
                }
                // Maneuvering through cover; if the maneuver timer runs out, it's time to move to another cover position
                // TODO: Broken - coverswitch timer is never reset
                /**if (coverSwitchPositionsTimer <= 0f) {
					bool coverFound = DynamicTakeCover ();
					if (coverFound) {
						inCover = false;
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.TakingCover, gameControllerScript.teamMap);
					} else {
						coverPos = Vector3.negativeInfinity;
						pView.RPC ("RpcUpdateActionState", RpcTarget.All, ActionStates.InCover, gameControllerScript.teamMap);
					}
				}*/
            }
        }
	}

	float CalculateSuspicionLevelForPos(Vector3 pos) {
		// Suspicion will depend on distance from the position and degree turned towards it
		float maxRange = range + 20f;
		float distanceFromTarget = Vector3.Distance (transform.position, pos);
		// How far away you are relative to max detection distance - the lower, the closer
		float percentOfRange = maxRange / distanceFromTarget;
		// Calculate distance multiplier
		float d = Mathf.Clamp(percentOfRange, 0.05f, 10f);
		// Calculate rotation multiplier
		Vector3 toPlayer = pos - transform.position;
		float angleBetween = Vector3.Angle (transform.forward, toPlayer);
		float r = 1f;
		if (angleBetween <= 90f) {
			float ang = 90f - angleBetween;
			r = Mathf.Clamp(Mathf.Deg2Rad * ang, 0.5f, 1f);
		}
		// Get base detection rate for player
		float total = 10f;
		if (playerTargeting.GetComponent<PlayerActionScript>() != null) {
			total = playerTargeting.GetComponent<PlayerActionScript>().GetDetectionRate();
		}
		// Calculate total suspicion increase
		// Debug.Log("dist: " + d + " rot: " + r);
		return Time.deltaTime * total * d * r / 2f;
	}

	// void PlayVoiceClip(int n) {
	// 	if (!audioSource.isPlaying && health > 0 && disorientationTime <= 0f) {
	// 		audioSource.clip = voiceClips [n - 1];
	// 		audioSource.Play ();
	// 	}
	// }

	public void PlayGruntSound() {
		if (gruntSounds.Length == 0) return;
		int r = Random.Range(0, gruntSounds.Length);
		audioSource.clip = gruntSounds [r];
		audioSource.Play ();
	}

	// IEnumerator PlayVoiceClipDelayed(int n, float t) {
	// 	yield return new WaitForSeconds (t);
	// 	if (actionState != ActionStates.Dead) {
	// 		PlayVoiceClip (n);
	// 	}
	// }

	[PunRPC]
	void RpcDie(string team) {
        if (team != gameControllerScript.teamMap) return;
        marker.enabled = false;
	}

	[PunRPC]
	void RpcSetCrouchMode(int n, string team) {
        if (team != gameControllerScript.teamMap) return;
        crouchMode = (CrouchMode)n;
	}

	void SetCrouchMode(CrouchMode c) {
		if (crouchMode != c) {
			pView.RPC("RpcSetCrouchMode", RpcTarget.All, (int)c, gameControllerScript.teamMap);
		}
	}

	void ApplyForceModifiers()
	{
		// 0 = death from bullet, 1 = death from explosion, 2 = death from fire/etc.
		if (lastHitBy == 0) {
			Rigidbody rb = ragdollBodies[lastBodyPartHit - 1];
			if (lastBodyPartHit == WeaponActionScript.HEAD_TARGET) {
				Vector3 forceDir = Vector3.Normalize(headTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.TORSO_TARGET) {
				Vector3 forceDir = Vector3.Normalize(torsoTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftForeArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightForeArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.PELVIS_TARGET) {
				Vector3 forceDir = Vector3.Normalize(pelvisTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftUpperLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftLowerLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightUpperLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightLowerLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			}
		} else if (lastHitBy == 1) {
			foreach (Rigidbody rb in ragdollBodies) {
				rb.AddExplosionForce(EXPLOSION_FORCE, lastHitFromPos, 7f, 0f, ForceMode.Impulse);
			}
		}
	}

	// Decision tree for scout type enemy
	void DecideActionScout() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead) {
			// Spawn a drop box
			int r = Random.Range(0, 100);
			int healthKitDropChance = HEALTH_KIT_DROP_CHANCE + thisHealthDropChanceBoost;
			int ammoKitDropChance = AMMO_KIT_DROP_CHANCE + thisAmmoDropChanceBoost;
			if (r >= 0 && r <= healthKitDropChance) {
				// 1/6 chance of getting a health box
				DropHealthPickup();
			} else if (r >= (healthKitDropChance + 1) && r <= (ammoKitDropChance + healthKitDropChance)) {
				// 1/3 chance of getting ammo box
				DropAmmoPickup();
			}

			if (playerTargeting != null) {
				PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript>();
				if (a != null) {
					playerTargeting.GetComponent<PlayerActionScript>().ClearEnemySeenBy();
				}
				playerTargeting = null;
			}

			SetSuspicionLevel(0f, 0f, 0f);
			SetAlertStatus(AlertStatus.Neutral);

			UpdateActionState(ActionStates.Dead);
			UpdateHealthStatus(HealthStatus.Neutral, 0);

			float respawnTime = Random.Range(0f, gameControllerScript.aIController.enemyRespawnSecs);
			pView.RPC ("StartDespawn", RpcTarget.All, respawnTime, gameControllerScript.teamMap);
			return;
		}

		// Melee attack, disorientation, death trumps all. Also check if action transition is delayed because of DDoS Attack skill
		if (actionState == ActionStates.Melee || actionState == ActionStates.Disoriented || actionState == ActionStates.Dead || actionTransitionDelay > 0f) {
			return;
		}

		CheckAlertTeamAfterAlerted();

		// Continue with decision tree
		PlayerScan();
		if (alertStatus == AlertStatus.Alert) {
			// Sees a player?
			if (playerTargeting != null) {
				// Else, proceed with regular behavior
				// Handle a melee attack
				WeaponActionScript was = playerTargeting.GetComponent<WeaponActionScript>();
				NpcScript n = playerTargeting.GetComponent<NpcScript>();
				if ((was != null || n != null) && TargetIsWithinMeleeDistance()) {
					SetAlertStatus(AlertStatus.Alert);
					UpdateActionState(ActionStates.Melee);
				} else {
					if (currentBullets > 0) {
						UpdateActionState(ActionStates.Firing);
						if (crouchMode == CrouchMode.ForceCover) {
							SetCrouchMode(CrouchMode.ForceLeaveCover);
						} else if (crouchMode == CrouchMode.ForceLeaveCover) {
							SetCrouchMode(CrouchMode.Natural);
						}
						TakeCoverScout ();
					} else {
						UpdateActionState(ActionStates.Reloading);
						SetCrouchMode(CrouchMode.ForceCover);
						TakeCoverScout ();
					}
				}
			} else {
				SetCrouchMode(CrouchMode.ForceLeaveCover);
				TakeCoverScout ();
				UpdateActionState(ActionStates.Idle);
			}
		} else {
			// Else, remain on lookout
			UpdateActionState(ActionStates.Idle);
			if (playerTargeting != null) {
				if (!gameControllerScript.assaultMode) {
					if (suspicionMeter < MAX_SUSPICION_LEVEL) {
						SetAlertStatus(AlertStatus.Suspicious);
						// Increase suspicion level
						float suspicionIncrease = CalculateSuspicionLevelForPos(playerTargeting.transform.position);
						IncreaseSuspicionLevel(suspicionIncrease);
						// Alert the local player if he's the one being seen only if this enemy has the greatest suspicion level
						if (playerTargeting.GetComponent<PlayerActionScript>() != null) {
							playerTargeting.GetComponent<PlayerActionScript>().SetEnemySeenBy(pView.ViewID);
						}
					} else {
						SetAlertStatus(AlertStatus.Alert);
					}
				} else {
					SetAlertStatus(AlertStatus.Alert);
				}
			} else {
				if (!gameControllerScript.assaultMode) {
					if (suspicionMeter > 0f) {
						DecreaseSuspicionLevel();
						SetAlertStatus(AlertStatus.Suspicious);
					} else {
						SetAlertStatus(AlertStatus.Neutral);
					}
				} else {
					SetAlertStatus(AlertStatus.Alert);
				}
			}
		}
	}

	bool EnvObstructionExists(Vector3 a, Vector3 b) {
		// Ignore other enemy/player colliders
		// Layer mask (layers/objects to ignore in explosion that don't count as defensive)
		if (insideBubbleShield) {
			return true;
		}
		int ignoreLayers = (1 << 9) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15) | (1 << 17) | (1 << 18);
		ignoreLayers = ~ignoreLayers;
		RaycastHit hitInfo;
		bool t = Physics.Linecast(a, b, out hitInfo, ignoreLayers, QueryTriggerInteraction.Ignore);
		if (t) {
			t = (hitInfo.transform.tag == "Human") ? false : true;
			if (hitInfo.transform.gameObject.layer == 18) {
				if (hitInfo.transform.gameObject.GetComponent<BombScript>() == null) {
					t = false;
				}
			}
		}
		return t;
	}

	void KilledByGrenade(int killedByViewId) {
		pView.RPC("RpcRegisterGrenadeKill", RpcTarget.All, killedByViewId, gameControllerScript.teamMap);
	}

	void HandleEnvironmentEffectTriggers(Collider other) {
		if (health <= 0 || envDamageTimer < ENV_DAMAGE_DELAY) {
			return;
		}

		if (other.gameObject.tag.Equals("Fire")) {
			FireScript f = other.gameObject.GetComponent<FireScript>();
			int damageReceived = (int)(f.damage);
			TakeDamage(damageReceived, other.gameObject.transform.position, 2, 0, 0, 0);
			ResetEnvDamageTimer();
		}
	}

	void HandleExplosiveEffectTriggers(Collider other) {
		// First priority is to handle possible explosion damage
		if (health <= 0) {
			return;
		}
		if (other.gameObject.tag.Equals("Explosive")) {
            // If the grenade is still active or if the grenade has already affected the enemy, ignore it
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            // If a ray caszed from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (t != null) {
				if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID)) {
					// Determine how far from the explosion the enemy was
					float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
					float blastRadius = t.blastRadius;
					distanceFromGrenade = Mathf.Min(distanceFromGrenade, blastRadius);
					float scale = 1f - (distanceFromGrenade / blastRadius);

					// Scale damage done to enemy by the distance from the explosion
					Weapon grenadeStats = InventoryScript.itemData.weaponCatalog[t.rootWeapon];
					int damageReceived = (int)(grenadeStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived, other.gameObject.transform.position, 1, 0, 0, 0);
					// Validate that this enemy has already been affected
					t.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
						KilledByGrenade(t.fromPlayerId);
					}
				}
			} else {
				LauncherScript l = other.gameObject.GetComponent<LauncherScript>();
				if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !l.isLive && !l.PlayerHasBeenAffected(pView.ViewID)) {
					// Determine how far from the explosion the enemy was
					float distanceFromProjectile = Vector3.Distance(transform.position, other.gameObject.transform.position);
					float blastRadius = other.gameObject.GetComponent<LauncherScript>().blastRadius;
					distanceFromProjectile = Mathf.Min(distanceFromProjectile, blastRadius);
					float scale = 1f - (distanceFromProjectile / blastRadius);

					// Scale damage done to enemy by the distance from the explosion
					Weapon projectileStats = InventoryScript.itemData.weaponCatalog[l.rootWeapon];
					int damageReceived = (int)(projectileStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived, other.gameObject.transform.position, 1, 0, 0, 0);
					// Validate that this enemy has already been affected
					l.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
						KilledByGrenade(l.fromPlayerId);
					}
				}
			}

			// Make enemy alerted by the explosion if he's not dead
			if (health > 0) {
				SetAlertStatus(AlertStatus.Alert);
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
					UpdateActionState(ActionStates.Disoriented);
					pView.RPC ("RpcSetIsCrouching", RpcTarget.All, false, gameControllerScript.teamMap);
				}
                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
                // Make enemy alerted by the disorientation if he's not dead
                if (health > 0) {
					SetAlertStatus(AlertStatus.Alert);
				}
				return;
			}
		}
	}

	bool TargetIsWithinMeleeDistance() {
		RaycastHit hit;
		Vector3 meleeTargetPos = new Vector3(playerTargeting.transform.position.x, playerTargeting.transform.position.y + 0.5f, playerTargeting.transform.position.z);
		// if (playerTargeting.GetComponent<PlayerActionScript>() != null) {
		// 	meleeTargetPos = playerTargeting.GetComponent<PlayerActionScript>().headTransform.position;
		// } else {
		// 	meleeTargetPos = new Vector3(playerTargeting.transform.position.x, playerTargeting.transform.position.y + 0.5f, playerTargeting.transform.position.z);
		// }
		if (Physics.Linecast (headTransform.position, meleeTargetPos, out hit, (1 << 9))) {
			if (hit.transform.gameObject.tag == "Player") {
				if (hit.distance <= MELEE_DISTANCE) {
					return true;
				}
			}
		}
		return false;
		// return (Vector3.Distance (playerTargeting.transform.position, transform.position) <= MELEE_DISTANCE);
	}

	[PunRPC]
	void RpcRegisterGrenadeKill(int playerNetworkId, string team) {
        if (team != gameControllerScript.teamMap) return;
        // If the player id of the person who killed the enemy matches my player id
        if (playerNetworkId == PlayerData.playerdata.inGamePlayerReference.GetComponent<PhotonView>().ViewID) {
			// Increment my kill score and show the kill popup for myself
			PlayerData.playerdata.inGamePlayerReference.GetComponent<WeaponActionScript>().RewardKill(false);
			PlayerData.playerdata.inGamePlayerReference.GetComponent<AudioControllerScript>().PlayKillSound();
		}
	}

	void OnTriggerEnter(Collider other) {
		if (gameControllerScript.matchType == 'V') {
			OnTriggerEnterForVersus(other);
		} else {
			OnTriggerEnterForCampaign(other);
		}
		if (other.GetComponentInParent<BubbleShieldScript>() != null) {
            insideBubbleShield = true;
        }
	}

	void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<BubbleShieldScript>() != null) {
            insideBubbleShield = false;
        }
	}

	void OnTriggerEnterForCampaign(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (PhotonNetwork.IsMasterClient) {
			HandleExplosiveEffectTriggers(other);
		}
	}

	void OnTriggerEnterForVersus(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (gameControllerScript.isVersusHostForThisTeam()) {
			HandleExplosiveEffectTriggers(other);
		}
	}

	void OnTriggerStay(Collider other) {
		if (gameControllerScript.matchType == 'V') {
			OnTriggerStayForVersus(other);
		} else {
			OnTriggerStayForCampaign(other);
		}
	}

	void OnTriggerStayForCampaign(Collider other) {
		if (PhotonNetwork.IsMasterClient) {
			HandleEnvironmentEffectTriggers(other);
		}
	}

	void OnTriggerStayForVersus(Collider other) {
		if (gameControllerScript.isVersusHostForThisTeam()) {
			HandleEnvironmentEffectTriggers(other);
		}
	}

	void SetNavMeshStopped(bool stopped) {
		if (navMesh.isStopped != stopped) {
			pView.RPC ("RpcUpdateNavMesh", RpcTarget.All, stopped, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	void RpcUpdateNavMesh(bool stopped, string team) {
        if (team != gameControllerScript.teamMap) return;
        if (gameControllerScript.matchType == 'V') {
			UpdateNavMeshForVersus(stopped);
		} else {
			UpdateNavMeshForCampaign(stopped);
		}
	}

	void UpdateNavMeshForCampaign(bool stopped) {
		if (PhotonNetwork.IsMasterClient) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
				navMesh.isStopped = stopped;
			}
		}
		prevWasStopped = stopped;
		if (prevWasStopped) {
			prevNavDestination = Vector3.negativeInfinity;
		}
	}

	void UpdateNavMeshForVersus(bool stopped) {
		if (gameControllerScript.isVersusHostForThisTeam()) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
				navMesh.isStopped = stopped;
			}
		}
		prevWasStopped = stopped;
		if (prevWasStopped) {
			prevNavDestination = Vector3.negativeInfinity;
		}
	}

	void UpdateNavMeshSpeed(float speed) {
		if (speed != navMesh.speed) {
			pView.RPC("RpcUpdateNavMeshSpeed", RpcTarget.All, speed, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	void RpcUpdateNavMeshSpeed(float speed, string team) {
        if (team != gameControllerScript.teamMap) return;
        navMesh.speed = speed;
	}

	[PunRPC]
	void StartDespawn(float respawnTime, string team) {
        if (team != gameControllerScript.teamMap) return;
        StartCoroutine(Despawn(respawnTime));
	}

	// Decision tree for patrol type enemy
	void DecideActionPatrol() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead)
		{
			// Spawn a drop box
			int r = Random.Range(0, 100);
			int healthKitDropChance = HEALTH_KIT_DROP_CHANCE + thisHealthDropChanceBoost;
			int ammoKitDropChance = AMMO_KIT_DROP_CHANCE + thisAmmoDropChanceBoost;
			if (r >= 0 && r <= healthKitDropChance) {
				// 1/6 chance of getting a health box
				DropHealthPickup();
			} else if (r >= (healthKitDropChance + 1) && r <= (ammoKitDropChance + healthKitDropChance)) {
				// 1/3 chance of getting ammo box
				DropAmmoPickup();
			}
			
			if (playerTargeting != null) {
				PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript>();
				if (a != null) {
					playerTargeting.GetComponent<PlayerActionScript>().ClearEnemySeenBy();
				}
				playerTargeting = null;
			}

			SetSuspicionLevel(0f, 0f, 0f);
			SetAlertStatus(AlertStatus.Neutral);

			SetNavMeshStopped(true);
			UpdateActionState(ActionStates.Dead);
			UpdateHealthStatus(HealthStatus.Neutral, 0);

			float respawnTime = Random.Range(0f, gameControllerScript.aIController.enemyRespawnSecs);
			pView.RPC ("StartDespawn", RpcTarget.All, respawnTime, gameControllerScript.teamMap);
			return;
		}

		// Melee attack, death, disorientation trumps all. Also check if action transition is delayed because of DDoS Attack skill
		if (actionState == ActionStates.Melee || actionState == ActionStates.Disoriented || actionState == ActionStates.Dead || actionTransitionDelay > 0f) {
			return;
		}

		CheckAlertTeamAfterAlerted();

		PlayerScan ();

		// Root - is the enemy alerted by any type of player presence (gunshots, sight, getting shot, other enemies alerted nearby)
		if (alertStatus == AlertStatus.Alert) {
			if (playerTargeting != null) {
				WeaponActionScript was = playerTargeting.GetComponent<WeaponActionScript>();
				NpcScript n = playerTargeting.GetComponent<NpcScript>();
				if ((was != null || n != null) && TargetIsWithinMeleeDistance()) {
					SetAlertStatus(AlertStatus.Alert);
					UpdateActionState(ActionStates.Melee);
				} else {
					if (actionState != ActionStates.Firing && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Pursue && actionState != ActionStates.Reloading) {
						int r = Random.Range (1, aggression - 2);
						if (r <= 1) {
							bool coverFound = DynamicTakeCover ();
							if (coverFound) {
								UpdateActionState(ActionStates.TakingCover);
							} else {
								UpdateActionState(ActionStates.InCover);
							}
						} else {
							UpdateActionState(ActionStates.Firing);
						}
					}
				}
			} else {
                // If the enemy has not seen a player
                if (actionState != ActionStates.Seeking && actionState != ActionStates.TakingCover && actionState != ActionStates.InCover && actionState != ActionStates.Firing && actionState != ActionStates.Reloading && actionState != ActionStates.Wander) {
					int r = Random.Range (1, aggression - 1);
					if (r <= 4) {
						bool coverFound = DynamicTakeCover ();
						if (coverFound) {
							UpdateActionState(ActionStates.TakingCover);
						} else {
							UpdateActionState(ActionStates.InCover);
						}
					} else {
						if (!Vector3.Equals(GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
							UpdateActionState(ActionStates.Seeking);
						} else {
							UpdateActionState(ActionStates.Wander);
						}
					}
				}

				if (actionState == ActionStates.Wander) {
					if (!Vector3.Equals(GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
						UpdateActionState(ActionStates.Seeking);
					}
				}

				if (actionState == ActionStates.Seeking) {
					if (navMeshReachedDestination (20f) && playerTargeting == null) {
						UpdateActionState(ActionStates.Wander);
					}
				}

				// If the enemy has seen a player before but no longer does, then possibly (60% chance) pursue the player or take cover (40% chance)
				if (actionState == ActionStates.Firing) {
					if (!Vector3.Equals (lastSeenPlayerPos, Vector3.negativeInfinity)) {
						int r = Random.Range (1, aggression);
						if (r <= 2) {
							UpdateActionState(ActionStates.TakingCover);
						} else {
							UpdateActionState(ActionStates.Pursue);
						}
					} else {
						UpdateActionState(ActionStates.Idle);
					}
				}

				// If the enemy was in pursuit of a player but has lost track of him, then go back to wandering
				if (actionState == ActionStates.Pursue && Vector3.Equals(lastSeenPlayerPos, Vector3.negativeInfinity)) {
					if (navMeshReachedDestination(2f)) {
						UpdateActionState(ActionStates.Wander);
					}
				}

				// If the enemy is in cover, stay there for a while and then go back to seeking the last gunshot position, or wandering if there isn't one
				if (actionState == ActionStates.InCover) {
					coverSwitchPositionsTimer -= Time.deltaTime;
					if (coverSwitchPositionsTimer <= 0f) {
						coverSwitchPositionsTimer = Random.Range (6f, 10f);
						//pView.RPC ("RpcSetCoverSwitchPositionsTimer", RpcTarget.Others, coverSwitchPositionsTimer);
						if (!Vector3.Equals(GameControllerScript.lastGunshotHeardPos, Vector3.negativeInfinity)) {
							UpdateActionState(ActionStates.Seeking);
						} else {
							UpdateActionState(ActionStates.Wander);
						}
					}
				}
			}
		} else {
			// Else, wander around the patrol points until alerted or enemy seen
			UpdateActionState(ActionStates.Wander);
			if (playerTargeting != null) {
				if (!gameControllerScript.assaultMode) {
					if (suspicionMeter < MAX_SUSPICION_LEVEL) {
						SetAlertStatus(AlertStatus.Suspicious);
						// Increase suspicion level
						float suspicionIncrease = CalculateSuspicionLevelForPos(playerTargeting.transform.position);
						IncreaseSuspicionLevel(suspicionIncrease);
						// Alert the local player if he's the one being seen only if this enemy has the greatest suspicion level
						if (playerTargeting.GetComponent<PlayerActionScript>() != null) {
							playerTargeting.GetComponent<PlayerActionScript>().SetEnemySeenBy(pView.ViewID);
						}
					} else {
						SetAlertStatus(AlertStatus.Alert);
					}
				} else {
					SetAlertStatus(AlertStatus.Alert);
				}
			} else {
				if (!gameControllerScript.assaultMode) {
					if (suspicionMeter > 0f) {
						DecreaseSuspicionLevel();
						SetAlertStatus(AlertStatus.Suspicious);
					} else {
						SetAlertStatus(AlertStatus.Neutral);
					}
				} else {
					SetAlertStatus(AlertStatus.Alert);
				}
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
				if (alertStatus != AlertStatus.Alert && !animator.GetCurrentAnimatorStateInfo (0).IsName ("Walk")) {
					animator.Play ("Walk");
				} else if (alertStatus == AlertStatus.Alert && !animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving")) {
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
				if (lastHitBy == 1) {
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
			if (alertStatus == AlertStatus.Alert) {
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
						SetNavMeshStopped(true);
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
		if (playerTargeting != null) {
			RaycastHit hit;
			// Locks onto the player and shoots at him
			FirstPersonController targetingFpc = playerTargeting.GetComponent<FirstPersonController>();
			Vector3 playerPos = Vector3.negativeInfinity;
			if (targetingFpc != null) {
				playerPos = targetingFpc.fpcTransformSpine.position;
				playerPos = new Vector3(playerPos.x, playerPos.y - 0.1f, playerPos.z);
			} else {
				playerPos = playerTargeting.transform.position;
				playerPos = new Vector3(playerPos.x, playerPos.y + 0.5f, playerPos.z);
			}
			
			Vector3 dir = playerPos - headTransform.position;

			// Adding artificial stupidity - ensures that the player isn't hit every time by offsetting
			// the shooting direction in x and y by two random numbers
			float scaledOffset = ScaleOffset(Vector3.Distance(playerPos, headTransform.position));
			float xOffset = Random.Range (-scaledOffset, scaledOffset);
			float yOffset = Random.Range (-scaledOffset, scaledOffset);
			dir = new Vector3 (dir.x + xOffset, dir.y + yOffset, dir.z);
			//Debug.DrawRay (gunRef.weaponShootPoint.position, dir * range, Color.red);
			if (Physics.Raycast (headTransform.position, dir, out hit, Mathf.Infinity, ENEMY_FIRE_IGNORE)) {
				if (hit.transform.tag.Equals ("Player")) {
					PlayerActionScript ps = hit.transform.GetComponent<PlayerActionScript> ();
					pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, ps.overshield > 0f, gameControllerScript.teamMap);
					ps.TakeDamage(CalculateDamageDealt(InventoryScript.itemData.weaponCatalog[gunRef.weaponName].damage / 2f, hit.transform.position.y, hit.point.y, hit.transform.gameObject.GetComponent<CharacterController>().height), true, true, transform.position, 0, Random.Range(1, 12));
					//ps.ResetHitTimer ();
				} else if (hit.transform.tag.Equals ("Human")) {
					pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false, gameControllerScript.teamMap);
					// BetaEnemyScript b = hit.transform.GetComponent<BetaEnemyScript>();
					NpcScript n = hit.transform.GetComponentInParent<NpcScript>();
					if (n != null) {
						int bodyPartIdHit = hit.transform.gameObject.GetComponent<BodyPartId>().bodyPartId;
						n.TakeDamage(CalculateDamageDealtToNpc(InventoryScript.itemData.weaponCatalog[gunRef.weaponName].damage / 2f, bodyPartIdHit), headTransform.position, 0, bodyPartIdHit);
					}
					// if (b != null) {
					// 	b.TakeDamage(CalculateDamageDealtAgainstEnemyAlly(damage, hit.transform.position.y, hit.point.y, hit.transform.gameObject.GetComponent<CapsuleCollider>().height));
					// }
				} else {
					Terrain t = hit.transform.gameObject.GetComponent<Terrain>();
                	pView.RPC("RpcHandleBulletVfxEnemy", RpcTarget.All, hit.point, -hit.normal, (t == null ? -1 : t.index), gameControllerScript.teamMap);
				}
			}
		}

		pView.RPC("RpcShootAction", RpcTarget.All, gameControllerScript.teamMap);
	}

	[PunRPC]
	void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal, bool overshield, string team) {
        if (team != gameControllerScript.teamMap) return;
		GameObject hitEffect = null;
		if (overshield) {
			hitEffect = Instantiate(overshieldHitEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
		} else {
			hitEffect = Instantiate(bloodEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
		}
		hitEffect.transform.Rotate (180f, 0f, 0f);
		Destroy (hitEffect, 1.5f);
	}

	[PunRPC]
	void RpcHandleBulletVfxEnemy(Vector3 point, Vector3 normal, int terrainId, string team) {
        if (team != gameControllerScript.teamMap) return;
		if (gameObject.layer == 0) return;
        if (terrainId == -1) {
			GameObject bulletHoleEffect = Instantiate(overshieldHitEffect, point, Quaternion.FromToRotation(Vector3.forward, normal));
			bulletHoleEffect.GetComponent<AudioSource>().Play();
			Destroy(bulletHoleEffect, 1.5f);
		} else {
			Terrain terrainHit = gameControllerScript.terrainMetaData[terrainId];
			GameObject bulletHoleEffect = Instantiate(terrainHit.GetRandomBulletHole(), point, Quaternion.FromToRotation(Vector3.forward, normal));
			bulletHoleEffect.transform.SetParent(terrainHit.gameObject.transform);
			Destroy(bulletHoleEffect, 4f);
		}
	}

	[PunRPC]
	void RpcShootAction(string team) {
        if (team != gameControllerScript.teamMap) return;
        PlayMuzzleFlash();
		PlayShootSound();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
		if (sniper && playerTargeting != null) {
			SniperTracerScript s = sniperTracer.gameObject.GetComponent<SniperTracerScript> ();
			s.enabled = true;
			s.SetDistance (Vector3.Distance(gunRef.weaponShootPoint.position, playerTargeting.transform.position));
			sniperTracer.enabled = true;
		}
	}

	void PlayMuzzleFlash() {
        if (gunRef.muzzleFlash != null) {
            gunRef.muzzleFlash.Play();
        }
    }

	private void PlayShootSound() {
		gunRef.fireSound.Play();
	}

	public void Reload() {
		currentBullets += bulletsPerMag;
	}

	[PunRPC]
	void RpcReload(int bullets, string team) {
        if (team != gameControllerScript.teamMap) return;
        currentBullets = bullets;
	}

	public void MeleeAttack() {
		if (playerTargeting != null && actionState != ActionStates.Disoriented && health > 0f) {
			// int r = Random.Range (0, 2);
			// if (r == 0) {
			// 	PlayVoiceClip (5);
			// } else {
			// 	PlayVoiceClip (13);
			// }
			PlayerActionScript ps = playerTargeting.GetComponent<PlayerActionScript> ();
			NpcScript n = playerTargeting.GetComponent<NpcScript> ();
			if (n != null) {
				n.TakeDamage(50, transform.position, 2, 0);
			}
			if (ps != null) {
				ps.TakeDamage (50, true, true, transform.position, 2, 0);
				//ps.ResetHitTimer();
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

	IEnumerator Despawn(float respawnTime) {
		if (actionState != ActionStates.Dead) yield return null;
		DespawnAction();
		StartCoroutine(DelayToggleRagdoll(0.2f, true));
		// RemoveHitboxes ();
		yield return new WaitForSeconds(5f);
		DespawnRenderers();
		if (!sniper) {
			StartCoroutine (Respawn(respawnTime, false));
		}
	}

	void DespawnAction() {
		if (enemyType == EnemyType.Patrol) {
			if (navMesh.isActiveAndEnabled && navMesh.isOnNavMesh) {
				navMesh.ResetPath ();
				navMesh.isStopped = true;
			}
			navMesh.enabled = false;
		} else {
			navMeshObstacle.enabled = false;
		}
	}

	void ToggleUpdateWhenOffscreen(bool b)
	{
		modeler.ToggleUpdateWhenOffscreen(b);
	}

	void DespawnRenderers()
	{
		modeler.DespawnPlayer();
		marker.enabled = false;
		ToggleWeaponMesh(false);
	}

	void RemoveHitboxes() {
		headTransform.GetComponent<Collider>().enabled = false;
		torsoTransform.GetComponent<Collider>().enabled = false;
		leftArmTransform.GetComponent<Collider>().enabled = false;
		leftForeArmTransform.GetComponent<Collider>().enabled = false;
		rightArmTransform.GetComponent<Collider>().enabled = false;
		rightForeArmTransform.GetComponent<Collider>().enabled = false;
		pelvisTransform.GetComponent<Collider>().enabled = false;
		leftUpperLegTransform.GetComponent<Collider>().enabled = false;
		leftLowerLegTransform.GetComponent<Collider>().enabled = false;
		rightUpperLegTransform.GetComponent<Collider>().enabled = false;
		rightLowerLegTransform.GetComponent<Collider>().enabled = false;
	}

	// Used for Scout AI
	void TakeCoverScout() {
		if (crouchMode == 0) {
			coverWaitTimer = 0f;
			if (!inCover) {
				inCover = true;
                //pView.RPC ("RpcSetInCover", RpcTarget.All, true, gameControllerScript.teamMap);
            }
        } else if (crouchMode == CrouchMode.ForceLeaveCover) {
			if (coverWaitTimer <= 0f) {
				coverWaitTimer = Random.Range (4f, 15f);
                //pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer, gameControllerScript.teamMap);
            }
            if (inCover) {
				inCover = false;
                //pView.RPC ("RpcSetInCover", RpcTarget.All, false, gameControllerScript.teamMap);
            }
        } else {
			if (coverWaitTimer <= 0f && !inCover) {
				coverTimer = Random.Range (3f, 7f);
				inCover = true;
				//pView.RPC ("RpcSetCoverTimer", RpcTarget.Others, coverTimer, gameControllerScript.teamMap);
				//pView.RPC ("RpcSetInCover", RpcTarget.All, true, gameControllerScript.teamMap);
			} else if (coverTimer <= 0f && inCover) {
				coverWaitTimer = Random.Range (4f,15f);
				inCover = false;
                //pView.RPC ("RpcSetCoverWaitTimer", RpcTarget.Others, coverWaitTimer, gameControllerScript.teamMap);
                //pView.RPC ("RpcSetInCover", RpcTarget.All, false, gameControllerScript.teamMap);
            }
        }
	}

	[PunRPC]
	void RpcSetCoverTimer(float f, string team) {
        if (team != gameControllerScript.teamMap) return;
        coverTimer = f;
	}

	// Take cover algorithm for moving enemies - returns true if cover was found, false if not
	bool DynamicTakeCover() {
		// Scan for cover first
		Collider[] nearbyCover = Physics.OverlapBox(transform.position, new Vector3(coverScanRange, 25f, coverScanRange));
		if (nearbyCover == null || nearbyCover.Length == 0) {
			return false;
		}
		// If cover is nearby, find the closest one
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
		CoverSpotScript[] coverSpots = nearbyCover [minCoverIndex].gameObject.GetComponentsInChildren<CoverSpotScript>();
		if (playerTargeting == null) {
			CoverSpotScript spot = coverSpots [Random.Range (0, coverSpots.Length)];
			pView.RPC ("RpcSetCoverPos", RpcTarget.All, spot.coverId, true, spot.transform.position.x, spot.transform.position.y, spot.transform.position.z, gameControllerScript.teamMap);
		} else {
			Transform bestFoundCoverSpot = null;
			for (int i = 0; i < coverSpots.Length; i++) {
				// Don't want to hide in the same place again
				if (Vector3.Distance (transform.position, coverSpots[i].transform.position) <= 0.5f) {
					continue;
				}
				// If a teammate is already taking that cover spot, find another
				if (coverSpots[i].GetComponent<CoverSpotScript>().IsTaken()) {
					continue;
				}
				// If there's something blocking the player and the enemy, then the enemy wants to hide behind it. This is priority
				if (Physics.Linecast (coverSpots[i].transform.position, playerTargeting.transform.position)) {
					bestFoundCoverSpot = coverSpots [i].transform;
					break;
				} else {
					// Else if there's nothing blocking the player and the enemy, then try to rush to cover anyways. Second priority
					bestFoundCoverSpot = coverSpots[i].transform;
				}
			}

			// If a cover spot was found, then take it
			if (bestFoundCoverSpot != null) {
				pView.RPC ("RpcSetCoverPos", RpcTarget.All, bestFoundCoverSpot.GetComponent<CoverSpotScript>().coverId, true, bestFoundCoverSpot.position.x, bestFoundCoverSpot.position.y, bestFoundCoverSpot.position.z, gameControllerScript.teamMap);
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	void RpcSetCoverPos(short id, bool n, float x, float y, float z, string team) {
        if (team != gameControllerScript.teamMap) return;
        if (!n) {
			gameControllerScript.LeaveCoverSpot(id);
			coverPos = null;
		} else {
			coverPos = gameControllerScript.coverSpots[id].transform;
			gameControllerScript.TakeCoverSpot(id);
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

	int GetHealthOfCurrentTarget() {
		if (playerTargeting != null) {
			PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript>();
			NpcScript n = playerTargeting.GetComponent<NpcScript>();
			if (a != null) {
				return a.health;
			} else {
				return n.health;
			}
		}

		return 0;
	}

	float GetFightingSpiritTimerOfCurrentTarget()
	{
		if (playerTargeting != null) {
			PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript>();
			if (a != null) {
				return a.fightingSpiritTimer;
			}
		}
		return 0f;
	}

	void PlayerScan() {
		if (playerScanTimer <= 0f) {
			playerScanTimer = PLAYER_SCAN_DELAY;
			// If we do not have a target player, try to find one
			int entityTargetingHealth = GetHealthOfCurrentTarget();
			if (playerTargeting == null || (entityTargetingHealth <= 0 && GetFightingSpiritTimerOfCurrentTarget() <= 0f)) {
				ArrayList keysNearBy = new ArrayList ();
				foreach (PlayerStat playerStat in GameControllerScript.playerList.Values) {
					GameObject p = playerStat.objRef;
					if (p == null || (p.GetComponent<PlayerActionScript>().health <= 0 && p.GetComponent<PlayerActionScript>().fightingSpiritTimer <= 0f))
						continue;
					// Silhouette skill boost
					if (gameControllerScript.assaultMode) {
						int silhouette = p.GetComponent<SkillController>().GetThisSilhouetteBoost();
						if (Random.Range(0, 100) < silhouette) {
							continue;
						}
					}
					if (Vector3.Distance (transform.position, p.transform.position) < range + 20f) {
						Vector3 toPlayer = p.transform.position - transform.position;
						float angleBetween = Vector3.Angle (transform.forward, toPlayer);
						if (angleBetween <= 90f) {
							if (TargetIsObscured(p)) {
								continue;
							}
							keysNearBy.Add (p.GetComponent<PhotonView>().Owner.ActorNumber);
						}
					}
				}

				// Scan for friendly NPCs/VIPs too
				// TODO: Fill in here once NPC allies are released
				GameObject npcP = gameControllerScript.vipRef;
				if (npcP != null && npcP.GetComponent<NpcScript>().health > 0) {
					if (Vector3.Distance (transform.position, npcP.transform.position) < range + 20f) {
						Vector3 toNpc = npcP.transform.position - transform.position;
						float angleBetween = Vector3.Angle (transform.forward, toNpc);
						if (angleBetween <= 90f) {
							if (!TargetIsObscured(npcP)) {
								keysNearBy.Add (-2);
							}
						}
					}
				}

				if (keysNearBy.Count != 0) {
					pView.RPC ("RpcSetTarget", RpcTarget.All, (int)keysNearBy [Random.Range (0, keysNearBy.Count)], gameControllerScript.teamMap);
				}
			} else {
				// If we do, check if it's still in range
				if (Vector3.Distance (transform.position, playerTargeting.transform.position) >= range + 20f) {
					UnseeTarget();
				} else {
					// If still in range, check if still in eye view
					Vector3 toPlayer = playerTargeting.transform.position - transform.position;
					float angleBetween = Vector3.Angle (transform.forward, toPlayer);
					if (angleBetween <= 90f) {
						// If still in eye view, check if there's something obscuring the player
						if (TargetIsObscured(playerTargeting)) {
							UnseeTarget();
						}
					} else {
						UnseeTarget();
					}
				}
			}
		} else {
			playerScanTimer -= Time.deltaTime;
		}
	}

	bool TargetIsObscured(GameObject targetRef) {
		// Cast a ray to make sure there's nothing in between the player and the enemy
		// Debug.DrawRay (headTransform.position, toPlayer, Color.blue);
		FirstPersonController targetPlayerCheck = targetRef.GetComponent<FirstPersonController>();
		Transform playerHead = null;
		Vector3 middleHalfCheck = Vector3.zero;
		Vector3 topHalfCheck = Vector3.zero;
		if (targetPlayerCheck != null) {
			playerHead = targetPlayerCheck.headTransform;
			middleHalfCheck = new Vector3 (playerHead.position.x, playerHead.position.y - 0.1f, playerHead.position.z);
			topHalfCheck = new Vector3 (playerHead.position.x, playerHead.position.y, playerHead.position.z);
		} else {
			middleHalfCheck = new Vector3(targetRef.transform.position.x, targetRef.transform.position.y + 0.1f, targetRef.transform.position.z);
			topHalfCheck = new Vector3(targetRef.transform.position.x, targetRef.transform.position.y + 0.2f, targetRef.transform.position.z);
		}
		RaycastHit hit1;
		RaycastHit hit2;

		if (!Physics.Linecast (headTransform.position, middleHalfCheck, out hit2, OBSCURE_IGNORE))
		{
			return true;
		}
		if (!Physics.Linecast (headTransform.position, topHalfCheck, out hit1, OBSCURE_IGNORE))
		{
			return true;
		}
		
		if (hit1.transform.gameObject == null || hit2.transform.gameObject == null)
		{
			return true;
		}
		if (targetPlayerCheck != null) {
			if (!hit1.transform.gameObject.tag.Equals("Player") && !hit2.transform.gameObject.tag.Equals("Player")) {
				return true;
			}
		} else {
			if (!hit1.transform.gameObject.tag.Equals("Human") && !hit2.transform.gameObject.tag.Equals("Human")) {
				return true;
			}
		}
		return false;
	}
	
	void UnseeTarget() {
		pView.RPC ("RpcSetLastSeenPlayerPos", RpcTarget.All, true, playerTargeting.transform.position.x, playerTargeting.transform.position.y, playerTargeting.transform.position.z, gameControllerScript.teamMap);
		pView.RPC ("RpcSetTarget", RpcTarget.All, -1, gameControllerScript.teamMap);
	}

	[PunRPC]
	void RpcSetLastSeenPlayerPos(bool n, float x, float y, float z, string team) {
        if (team != gameControllerScript.teamMap) return;
        if (!n) {
			lastSeenPlayerPos = Vector3.negativeInfinity;
		} else {
			lastSeenPlayerPos = new Vector3 (x, y, z);
		}
	}

	public void TakeDamage(int d, Vector3 hitFromPos, int hitBy, int bodyPartHit, int healthDropChanceBoost, int ammoDropChanceBoost) {
		if (health > 0) {
			pView.RPC ("RpcTakeDamage", RpcTarget.All, d, hitFromPos.x, hitFromPos.y, hitFromPos.z, hitBy, bodyPartHit, healthDropChanceBoost, ammoDropChanceBoost, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	public void RpcTakeDamage(int d, float hitFromX, float hitFromY, float hitFromZ, int hitBy, int bodyPartHit, int healthDropChanceBoost, int ammoDropChanceBoost, string team) {
        if (team != gameControllerScript.teamMap) return;
        health -= d;
		lastHitFromPos = new Vector3(hitFromX, hitFromY, hitFromZ);
		lastHitBy = hitBy;
		lastBodyPartHit = bodyPartHit;
		thisHealthDropChanceBoost = healthDropChanceBoost;
		thisAmmoDropChanceBoost = ammoDropChanceBoost;
		if (healthStatus == HealthStatus.Poisoned && hitBy == 2) {
			PlayGruntSound();
			// Put hitmarker on poisoner's screen to let the player know this is poisoned
			GameObject poisoner = null;
			if (GameControllerScript.playerList.ContainsKey(poisonedById)) {
				poisoner = GameControllerScript.playerList[poisonedById].objRef;
				poisoner.GetComponent<WeaponActionScript>().ExternalInstantiateHitmarker();
			}
			if (health <= 0) {
				// Death from poison - award kill to whoever poisoned this
				if (poisoner != null) {
					poisoner.GetComponent<WeaponActionScript>().ExternalRewardKill();
				}
			}
		}
	}

	void UpdateActionState(ActionStates action) {
		if (actionState != action) {
			pView.RPC("RpcUpdateActionState", RpcTarget.All, action, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action, string team) {
        if (team != gameControllerScript.teamMap) return;
        //{Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking}
        // if (action == ActionStates.Firing || action == ActionStates.Moving || action == ActionStates.Reloading || action == ActionStates.Pursue || action == ActionStates.TakingCover || action == ActionStates.InCover) {
		// 	int r = Random.Range (0, 3);
		// 	if (r == 1) {
		// 		StartCoroutine (PlayVoiceClipDelayed(Random.Range (1, 5), Random.Range(2f, 50f)));
		// 	} else if (r != 0) {
		// 		StartCoroutine (PlayVoiceClipDelayed(Random.Range (6, 13), Random.Range(2f, 50f)));
		// 	}
		// }
		// Play grunt when enemy dies or hit by flashbang
		if (action == ActionStates.Dead || action == ActionStates.Disoriented) {
			PlayGruntSound();
		} else {
			actionTransitionDelay = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>().GetDdosDelayTime();
		}
		actionState = action;
	}

	[PunRPC]
	private void RpcUpdateFiringState(FiringStates firing, string team) {
        if (team != gameControllerScript.teamMap) return;
        firingState = firing;
	}

	void CheckTargetDead() {
		if (playerTargeting != null) {
			PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript> ();
			NpcScript n = playerTargeting.GetComponent<NpcScript> ();
			if (a != null && a.health <= 0f && a.fightingSpiritTimer <= 0f) {
				pView.RPC ("RpcSetTarget", RpcTarget.All, -1, gameControllerScript.teamMap);
			}
			if (n != null && n.health <= 0f) {
				pView.RPC ("RpcSetTarget", RpcTarget.All, -1, gameControllerScript.teamMap);
			}
		}
	}

	[PunRPC]
	void RpcSetTarget(int id, string team) {
        if (team != gameControllerScript.teamMap) return;
        if (id == -1) {
			playerTargeting = null;
		} else if (id == -2) {
			playerTargeting = gameControllerScript.vipRef;
		} else {
			playerTargeting = (GameObject)GameControllerScript.playerList [id].objRef;
		}
	}

	// Reset values to respawn
	IEnumerator Respawn(float respawnTime, bool syncWithClientsAgain) {
		if (actionState != ActionStates.Dead) yield return null;
		yield return new WaitForSeconds (respawnTime + PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>().GetHackerBoost());
		if (gameControllerScript.assaultMode && gameControllerScript.spawnMode != SpawnMode.Paused) {
			RespawnAction (syncWithClientsAgain);
		} else {
			StartCoroutine (Respawn(respawnTime, syncWithClientsAgain));
		}
	}

	void RespawnAction (bool syncWithClientsAgain) {
		if (gameControllerScript.spawnMode == SpawnMode.Routine) {
			gameControllerScript.MarkAIReadyForRespawn(pView.ViewID, syncWithClientsAgain);
		} else if (gameControllerScript.spawnMode == SpawnMode.Fixed) {
			RespawnAtPosition(spawnPos, syncWithClientsAgain);
		}
	}

	public void RespawnAtPosition(Vector3 pos, bool syncWithClientsAgain) {
		if (syncWithClientsAgain) {
			pView.RPC("RpcRespawnAtPosition", RpcTarget.All, gameControllerScript.teamMap, pos.x, pos.y, pos.z);
		} else {
			ToggleRagdoll(false);
			navMesh.enabled = false;
			navMeshObstacle.enabled = false;
			insideBubbleShield = false;
			transform.position = pos;
			transform.rotation = Quaternion.identity;
			ToggleHumanCollision(true);
			health = 100;
			lastHitBy = 0;
			coverWaitTimer = Random.Range (2f, 7f);
			coverSwitchPositionsTimer = Random.Range (6f, 10f);
			playerTargeting = null;
			currentBullets = bulletsPerMag;
			isCrouching = false;

			coverTimer = 0f;
			inCover = false;
			isReloading = false;
			fireTimer = 0.0f;

			lastSeenPlayerPos = Vector3.negativeInfinity;

			actionState = ActionStates.Idle;
			animator.Play ("Idle");
			firingState = FiringStates.StandingStill;
			firingModeTimer = 0f;

			wanderStallDelay = -1f;
			coverPos = null;
			crouchMode = CrouchMode.Natural;
			coverScanRange = 50f;

			modeler.RespawnPlayer();
			marker.enabled = true;
			ToggleWeaponMesh(true);

			thisAmmoDropChanceBoost = 0;
			thisHealthDropChanceBoost = 0;

			if (enemyType == EnemyType.Patrol) {
				navMesh.enabled = true;
				navMeshObstacle.enabled = false;
			} else {
				navMesh.enabled = false;
				navMeshObstacle.enabled = true;
			}
		}
	}

	[PunRPC]
	void RpcRespawnAtPosition(string team, float respawnPosX, float respawnPosY, float respawnPosZ) {
		if (team != gameControllerScript.teamMap) return;
		ToggleRagdoll(false);
		navMesh.enabled = false;
		navMeshObstacle.enabled = false;
		insideBubbleShield = false;
		transform.position = new Vector3(respawnPosX, respawnPosY, respawnPosZ);
		transform.rotation = Quaternion.identity;
		ToggleHumanCollision(true);
		health = 100;
		lastHitBy = 0;
		coverWaitTimer = Random.Range (2f, 7f);
		coverSwitchPositionsTimer = Random.Range (6f, 10f);
		playerTargeting = null;
		currentBullets = bulletsPerMag;
		isCrouching = false;

		coverTimer = 0f;
		inCover = false;
		isReloading = false;
		fireTimer = 0.0f;

		lastSeenPlayerPos = Vector3.negativeInfinity;

		actionState = ActionStates.Idle;
		animator.Play ("Idle");
		firingState = FiringStates.StandingStill;
		firingModeTimer = 0f;

		wanderStallDelay = -1f;
		coverPos = null;
		crouchMode = CrouchMode.Natural;
		coverScanRange = 50f;

		modeler.RespawnPlayer();
		marker.enabled = true;
		ToggleWeaponMesh(true);

		thisHealthDropChanceBoost = 0;
		thisAmmoDropChanceBoost = 0;

		if (enemyType == EnemyType.Patrol) {
			navMesh.enabled = true;
			navMeshObstacle.enabled = false;
		} else {
			navMesh.enabled = false;
			navMeshObstacle.enabled = true;
		}
	}

	void StopVoices() {
		audioSource.Stop();
	}
	
	void ToggleDetectionOutline(bool b) {
		modeler.ToggleDetectionOutline(b);
	}

	void HandleDetectionOutline() {
		if (gameControllerScript.assaultMode) {
			if (isOutlined) {
				detectionOutlineTimer = 0f;
				isOutlined = false;
				ToggleDetectionOutline(false);
			}
			return;
		}
		if (detectionOutlineTimer <= 0f) {
			if (isOutlined) {
				isOutlined = false;
				ToggleDetectionOutline(false);
			}
		} else {
			if (!isOutlined) {
				isOutlined = true;
				ToggleDetectionOutline(true);
			}
			detectionOutlineTimer -= Time.deltaTime;
		}
	}

	// Use when a player marks an enemy in stealth mode
	public void MarkEnemyOutline(int timeMultiplier) {
		pView.RPC("RpcMarkEnemyOutline", RpcTarget.All, gameControllerScript.teamMap, timeMultiplier);
	}

	[PunRPC]
	void RpcMarkEnemyOutline(string team, int timeMultiplier) {
        if (team != gameControllerScript.teamMap) return;
        detectionOutlineTimer = DETECTION_OUTLINE_MAX_TIME * timeMultiplier;
	}

	void DropAmmoPickup() {
		GameObject o = GameObject.Instantiate(ammoBoxPickup, transform.position, Quaternion.Euler(Vector3.zero));
		o.GetComponent<PickupScript>().pickupId = o.GetInstanceID();
		gameControllerScript.DropPickup(o.GetInstanceID(), o);
		pView.RPC("RpcDropAmmoPickup", RpcTarget.Others, o.GetInstanceID(), gameControllerScript.teamMap);
	}

	void DropHealthPickup() {
		GameObject o = GameObject.Instantiate(healthBoxPickup, transform.position, Quaternion.Euler(Vector3.zero));
		o.GetComponent<PickupScript>().pickupId = o.GetInstanceID();
		gameControllerScript.DropPickup(o.GetInstanceID(), o);
		pView.RPC("RpcDropHealthPickup", RpcTarget.Others, o.GetInstanceID(), gameControllerScript.teamMap);
	}

	[PunRPC]
	void RpcDropAmmoPickup(int pickupId, string team) {
		if (team != gameControllerScript.teamMap) return;
		GameObject o = GameObject.Instantiate(ammoBoxPickup, transform.position, Quaternion.Euler(Vector3.zero));
		o.GetComponent<PickupScript>().pickupId = pickupId;
		gameControllerScript.DropPickup(pickupId, o);
	}

	[PunRPC]
	void RpcDropHealthPickup(int pickupId, string team) {
		if (team != gameControllerScript.teamMap) return;
		GameObject o = GameObject.Instantiate(healthBoxPickup, transform.position, Quaternion.Euler(Vector3.zero));
		o.GetComponent<PickupScript>().pickupId = pickupId;
		gameControllerScript.DropPickup(pickupId, o);
	}

	int CalculateDamageDealt(float initialDamage, float baseY, float hitY, float height) {
        float total = initialDamage;
		// Determine how high/low on the body was hit. The closer to 1, the closer to shoulders; closer to 0, closer to feet
		float bodyHeightHit = Mathf.Abs(hitY - baseY) / height;
		// Higher the height, the more damage dealt
		if (bodyHeightHit <= 0.25f) {
			total *= 0.25f;
		} else if (bodyHeightHit < 0.8f) {
			total *= bodyHeightHit;
		}
        return (int)total;
    }

	int CalculateDamageDealtToNpc(float initialDamage, int bodyPartHit, int divisor = 1) {
        if (bodyPartHit == WeaponActionScript.HEAD_TARGET) {
            return 100;
        } else if (bodyPartHit == WeaponActionScript.TORSO_TARGET) {
            return (int)(initialDamage / (float)divisor);
        } else if (bodyPartHit == WeaponActionScript.LEFT_ARM_TARGET) {
            return (int)((initialDamage / (float)divisor) / 2f);
        } else if (bodyPartHit == WeaponActionScript.LEFT_FOREARM_TARGET) {
            return (int)((initialDamage / (float)divisor) / 3f);
        } else if (bodyPartHit == WeaponActionScript.RIGHT_ARM_TARGET) {
            return (int)((initialDamage / (float)divisor) / 2f);
        } else if (bodyPartHit == WeaponActionScript.RIGHT_FOREARM_TARGET) {
            return (int)((initialDamage / (float)divisor) / 3f);
        } else if (bodyPartHit == WeaponActionScript.PELVIS_TARGET) {
            return (int)(initialDamage / (float)divisor);
        } else if (bodyPartHit == WeaponActionScript.LEFT_UPPER_LEG_TARGET) {
            return (int)((initialDamage / (float)divisor) / 1.5f);
        } else if (bodyPartHit == WeaponActionScript.LEFT_LOWER_LEG_TARGET) {
            return (int)((initialDamage / (float)divisor) / 2f);
        } else if (bodyPartHit == WeaponActionScript.RIGHT_UPPER_LEG_TARGET) {
            return (int)((initialDamage / (float)divisor) / 1.5f);
        } else if (bodyPartHit == WeaponActionScript.RIGHT_LOWER_LEG_TARGET) {
            return (int)((initialDamage / (float)divisor) / 2f);
        }
        return 0;
    }

	public void IncreaseSuspicionLevel(float amount) {
        if (gameControllerScript.assaultMode) return;

        bool somethingChanged = false;
        float previousSuspicionMeter = suspicionMeter;
        float previousSuspicionCoolDownDelay = suspicionCoolDownDelay;
        float previousIncreaseSuspicionDelay = increaseSuspicionDelay;

        if (increaseSuspicionDelay > 0f) {
            increaseSuspicionDelay -= Time.deltaTime;
            if (increaseSuspicionDelay != previousIncreaseSuspicionDelay) {
                somethingChanged = true;
            }
        } else {
            suspicionMeter += amount;
			if (suspicionMeter > MAX_SUSPICION_LEVEL) {
				suspicionMeter = MAX_SUSPICION_LEVEL;
			}
            suspicionCoolDownDelay = 4f;
			// Determine if suspicion amount or suspicion cool down has changed to sync over the network
            if ((previousSuspicionMeter != suspicionMeter) || (previousSuspicionCoolDownDelay != suspicionCoolDownDelay)) {
                somethingChanged = true;
            }

            if (suspicionMeter == MAX_SUSPICION_LEVEL) {
                increaseSuspicionDelay = 6f;
                if (previousIncreaseSuspicionDelay != increaseSuspicionDelay) {
                    somethingChanged = true;
                }
            }
        }

        // Sync values to all other players
        if (somethingChanged && !syncSuspicionValuesSemiphore) {
            StartCoroutine("SyncSuspicionValuesProcessor");
        }
    }

	public void DecreaseSuspicionLevel() {
		bool somethingChanged = false;
        if (suspicionCoolDownDelay <= 0f) {
            if (suspicionMeter > 0f) {
                float suspicionDeduction = suspicionMeter - (15 * Time.deltaTime);
                suspicionMeter = (suspicionDeduction < 0f ? 0f : suspicionDeduction);
				somethingChanged = true;
            }
        } else {
            suspicionCoolDownDelay -= Time.deltaTime;
			somethingChanged = true;
        }

		// Sync values to all other players
        if (somethingChanged && !syncSuspicionValuesSemiphore) {
            StartCoroutine("SyncSuspicionValuesProcessor");
        }
    }

	void SetSuspicionLevel(float suspicionMeter, float increaseSuspicionDelay, float suspicionCoolDownDelay) {
		pView.RPC("RpcSyncSuspicionValues", RpcTarget.All, gameControllerScript.teamMap, suspicionMeter, increaseSuspicionDelay, suspicionCoolDownDelay);
	}

	[PunRPC]
    void RpcSyncSuspicionValues(string team, float suspicionMeter, float increaseSuspicionDelay, float suspicionCoolDownDelay) {
		if (team != gameControllerScript.teamMap) return;
        this.suspicionMeter = suspicionMeter;
        this.increaseSuspicionDelay = increaseSuspicionDelay;
        this.suspicionCoolDownDelay = suspicionCoolDownDelay;
    }

	IEnumerator SyncSuspicionValuesProcessor() {
        syncSuspicionValuesSemiphore = true;
        yield return new WaitForSeconds(1.5f);
        pView.RPC("RpcSyncSuspicionValues", RpcTarget.Others, gameControllerScript.teamMap, suspicionMeter, increaseSuspicionDelay, suspicionCoolDownDelay);
        syncSuspicionValuesSemiphore = false;
    }

	public void EditorKillAi() {
		if (playerTargeting != null) {
			PlayerActionScript a = playerTargeting.GetComponent<PlayerActionScript>();
			if (a != null) {
				playerTargeting.GetComponent<PlayerActionScript>().ClearEnemySeenBy();
			}
			playerTargeting = null;
		}

		suspicionMeter = 0f;
        increaseSuspicionDelay = 0f;
        suspicionCoolDownDelay = 0f;
		alertStatus = AlertStatus.Neutral;
		actionState = ActionStates.Dead;

		ToggleHumanCollision(false);

		// RemoveHitboxes ();
		DespawnRenderers();
		
	}

	void ToggleHumanCollision(bool b)
	{
		if (!b) {
			headTransform.gameObject.layer = 12;
			torsoTransform.gameObject.layer = 12;
			leftArmTransform.gameObject.layer = 12;
			leftForeArmTransform.gameObject.layer = 12;
			rightArmTransform.gameObject.layer = 12;
			rightForeArmTransform.gameObject.layer = 12;
			pelvisTransform.gameObject.layer = 12;
			leftUpperLegTransform.gameObject.layer = 12;
			leftLowerLegTransform.gameObject.layer = 12;
			rightUpperLegTransform.gameObject.layer = 12;
			rightLowerLegTransform.gameObject.layer = 12;
		} else {
			headTransform.gameObject.layer = 14;
			torsoTransform.gameObject.layer = 14;
			leftArmTransform.gameObject.layer = 14;
			leftForeArmTransform.gameObject.layer = 14;
			rightArmTransform.gameObject.layer = 14;
			rightForeArmTransform.gameObject.layer = 14;
			pelvisTransform.gameObject.layer = 14;
			leftUpperLegTransform.gameObject.layer = 14;
			leftLowerLegTransform.gameObject.layer = 14;
			rightUpperLegTransform.gameObject.layer = 14;
			rightLowerLegTransform.gameObject.layer = 14;
		}
	}

	void PrintNavMeshAgentStats() {
		if (Input.GetKeyDown(KeyCode.L)) {
			Debug.Log("AI Name: " + gameObject.name);
			Debug.Log("isActiveAndEnabled: " + navMesh.isActiveAndEnabled);
			Debug.Log("isPathStale: " + navMesh.isPathStale);
			Debug.Log("isOnNavMesh" + navMesh.isOnNavMesh);
			Debug.Log("isStopped: " + navMesh.isStopped);
			Debug.Log("---------------------------------------------------------------------");
		}
	}

	// public void EditorAssignNavPts() {
	// 	Transform[] navChilds = pNav.GetComponentsInChildren<Transform>();

	// 	for (int i = 1; i < navChilds.Length; i++) {
	// 		navPoints[i - 1] = navChilds[i].gameObject;
	// 	}
	// }

	void ResetEnvDamageTimer() {
		envDamageTimer = 0f;
	}

	void UpdateEnvDamageTimer() {
		if (envDamageTimer < ENV_DAMAGE_DELAY) {
			envDamageTimer += Time.deltaTime;
		}
	}

	[PunRPC]
	void RpcAskServerForDataEnemies() {
		if (PhotonNetwork.IsMasterClient || gameControllerScript.isVersusHostForThisTeam()) {
			int playerTargetingId = 0;
			
			if (playerTargeting == null) {
				playerTargetingId = -1;
			} else if (playerTargeting == gameControllerScript.vipRef) {
				playerTargetingId = -2;
			} else {
				playerTargetingId = playerTargeting.GetComponent<PhotonView>().Owner.ActorNumber;
			}

			pView.RPC("RpcSyncDataEnemies", RpcTarget.Others, marker.enabled,
					navMesh.enabled, navMesh.speed, navMeshObstacle.enabled, 
					gunRef.weaponParts[0].enabled, prevNavDestination.x, prevNavDestination.y, prevNavDestination.z, prevWasStopped, actionState, firingState, isCrouching, health, disorientationTime,
					spawnPos.x, spawnPos.y, spawnPos.z, alertStatus, wasMasterClient, currentBullets, fireTimer, playerTargetingId, lastSeenPlayerPos.x, lastSeenPlayerPos.y, lastSeenPlayerPos.z,
					suspicionMeter, suspicionCoolDownDelay, increaseSuspicionDelay, alertTeamAfterAlertedTimer, inCover, crouchMode, detectionOutlineTimer, gameControllerScript.teamMap);
		}
	}

	[PunRPC]
	void RpcSyncDataEnemies(bool markerEnabled,
					bool navMeshEnabled, float navMeshSpeed, bool navMeshObstacleEnabled, bool gunRefEnabled, float preNavDestX, float preNavDestY,
					float preNavDestZ, bool prevWasStopped, ActionStates acState, FiringStates fiState, bool isCrouching, int health, float disorientationTime,
					float spawnPosX, float spawnPosY, float spawnPosZ, AlertStatus alertStatus, bool wasMasterClient, int currentBullets, float fireTimer,
					int playerTargetingId, float lastSeenPlayerPosX, float lastSeenPlayerPosY, float lastSeenPlayerPosZ, float suspicionMeter, float suspicionCoolDownDelay,
					float increaseSuspicionDelay, float alertTeamAfterAlertedTimer, bool inCover, CrouchMode crouchMode, float detectionOutlineTimer, string team) {
		if (team != gameControllerScript.teamMap) return;
		// if (playerDespawned) {
		// 	modeler.DespawnPlayer();
		// } else {
		// 	modeler.RespawnPlayer();
		// }
		marker.enabled = markerEnabled;
		// navMesh.isStopped = isStopped;
		// navMesh.destination = new Vector3(destinationX, destinationY, destinationZ);
		navMesh.speed = navMeshSpeed;
		navMesh.enabled = navMeshEnabled;
		navMeshObstacle.enabled = navMeshObstacleEnabled;
		ToggleWeaponMesh(gunRefEnabled);
		prevNavDestination = new Vector3(preNavDestX, preNavDestY, preNavDestZ);
		this.prevWasStopped = prevWasStopped;
		actionState = acState;
		if (actionState == ActionStates.Dead) {
			ToggleHumanCollision(false);
		} else {
			ToggleHumanCollision(true);
		}
		firingState = fiState;
		this.isCrouching = isCrouching;
		this.health = health;
		this.disorientationTime = disorientationTime;
		this.spawnPos = new Vector3(spawnPosX, spawnPosY, spawnPosZ);
		this.alertStatus = alertStatus;
		this.wasMasterClient = wasMasterClient;
		this.currentBullets = currentBullets;
		this.fireTimer = fireTimer;
		this.detectionOutlineTimer = detectionOutlineTimer;
		if (playerTargetingId == -1) {
			playerTargeting = null;
		} else if (playerTargetingId == -2) {
			playerTargeting = gameControllerScript.vipRef;
		} else {
			playerTargeting = (GameObject)GameControllerScript.playerList [playerTargetingId].objRef;
		}
		lastSeenPlayerPos = new Vector3(lastSeenPlayerPosX, lastSeenPlayerPosY, lastSeenPlayerPosZ);
		this.suspicionMeter = suspicionMeter;
		this.suspicionCoolDownDelay = suspicionCoolDownDelay;
		this.increaseSuspicionDelay = increaseSuspicionDelay;
		this.alertTeamAfterAlertedTimer = alertTeamAfterAlertedTimer;
		this.inCover = inCover;
		this.crouchMode = crouchMode;
	}

	void ToggleWeaponMesh(bool b)
	{
		foreach (MeshRenderer m in gunRef.weaponParts) {
			m.enabled = b;
		}
	}

	IEnumerator DelayToggleRagdoll(float seconds, bool b)
    {
        yield return new WaitForSeconds(seconds);
        pView.RPC("RpcToggleRagdollEnemy", RpcTarget.All, b, gameControllerScript.teamMap);
    }

    [PunRPC]
    void RpcToggleRagdollEnemy(bool b, string team)
    {
		if (team != gameControllerScript.teamMap) return;
        ToggleRagdoll(b);
		ToggleHumanCollision(!b);
        if (b) {
            ApplyForceModifiers();
        }
    }

	private int CalculateDamageDropoff(float damage, float distance, float range)
    {
        float dropoffRange = range / 3f;
        float sustainRange = 2f * range / 3f;
        if (distance <= sustainRange) {
            return 0;
        }
        float maxDropoffAmount = damage / 3f;
        return (int)(((distance - sustainRange) / dropoffRange) * maxDropoffAmount);
    }

	public void SetPoisoned(int fromPlayerId)
	{
		UpdateHealthStatus(HealthStatus.Poisoned, fromPlayerId);
	}

	void UpdateHealthStatus(HealthStatus h, int fromPlayerId)
	{
		if (healthStatus == h) return;
		pView.RPC("RpcUpdateHealthStatus", RpcTarget.All, h, fromPlayerId, gameControllerScript.teamMap);
	}

	[PunRPC]
	void RpcUpdateHealthStatus(HealthStatus h, int fromPlayerId, string team)
	{
		if (team != gameControllerScript.teamMap) return;
		healthStatus = h;
		if (h == HealthStatus.Poisoned) {
			poisonTimer = POISONED_INTERVAL;
			poisonedById = fromPlayerId;
		}
	}

	void HandleHealthStatus()
	{
		if (healthStatus == HealthStatus.Poisoned) {
			poisonTimer -= Time.deltaTime;
			if (poisonTimer <= 0f) {
				poisonTimer = POISONED_INTERVAL;
				TakeDamage(POISONED_DMG, transform.position, 2, 0, 0, 0);
			}
		}
	}

}
