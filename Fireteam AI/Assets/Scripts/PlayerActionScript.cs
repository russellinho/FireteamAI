using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Characters.FirstPerson;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using SpawnMode = GameControllerScript.SpawnMode;
using NpcActionState = NpcScript.ActionStates;
using FlightMode = BlackHawkScript.FlightMode;
using UnityEngine.Rendering.PostProcessing;
using Koobando.AntiCheat;
using StatBoosts = EquipmentScript.StatBoosts;
using Random = UnityEngine.Random;

public class PlayerActionScript : MonoBehaviourPunCallbacks
{
    private const int WATER_LAYER = 4;
    const float UNDERWATER_TIMER = 30f;
    const float MAX_AVOID = 0.9f;
    public static int MIN_DETECTION_LEVEL = 1;
    public static int MAX_DETECTION_LEVEL = 80;
    private const float EXPLOSION_FORCE = 75f;
	private const float BULLET_FORCE = 50f;
    const float INTERACTION_DISTANCE = 4.5f;
    const float BOMB_DEFUSE_TIME = 8f;
    const float DEPLOY_USE_TIME = 4f;
    const float NPC_INTERACT_TIME = 5f;
    const float HEAL_TIME = 5f;
    const float REVIVE_TIME = 6f;
    private const float ENV_DAMAGE_DELAY = 0.5f;
    private const float MINIMUM_FALL_DMG_VELOCITY = 25f;
    private const float MINIMUM_FALL_DMG = 10f;
    private const float FALL_DMG_MULTIPLIER = 2f;
    private const float FALL_DMG_DIVISOR = 9f;
    private const int ENEMY_LAYER = 14;

    // Object references
    public PhotonView pView;
    public SkillController skillController;
    public GameControllerScript gameController;
    public AudioControllerScript audioController;
    public CharacterController charController;
    public Rigidbody mainRigid;
    public WeaponActionScript wepActionScript;
    public CameraShakeScript cameraShakeScript;
    public PhotonTransformViewKoobando photonTransformView;
    public AudioSource aud;
    public AudioSource radioAud;
    public Camera viewCam;
    public GameObject spectatorCam;
    public GameObject thisSpectatorCam;
    public PlayerHUDScript hud;
    public EquipmentScript equipmentScript;
    public WeaponScript weaponScript;
    public CameraShakeScript camShakeScript;
    public InGameMessengerHUD inGameMessengerHud;
    public Animator animator;
    public PlayerScript playerScript;
    public ParticleSystem healParticleEffect;
    public ParticleSystem boostParticleEffect;
    public ParticleSystem overshieldBreakEffect;
    public ParticleSystem overshieldRecoverParticleEffect;
    public ParticleSystem jetpackParticleEffect;
    public SpriteRenderer hudMarker;
    public SpriteRenderer hudMarker2;
    public GameObject fpcBodyRef;
    public GameObject[] objectsToDisable;
    private BetaEnemyScript enemySeenBy;
    public AudioClip ammoPickupSound;
    public AudioClip healthPickupSound;
    public Transform carryingSlot;
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
    public Rigidbody[] ragdollBodies;

    // Player variables
    public EncryptedInt health;
    public EncryptedFloat overshield;
    public bool activeCamo;
    public float sprintTime;
    private EncryptedBool spawnInvincibilityActive;
    // public bool godMode;
    public bool canShoot;
    private float charHeightOriginal;
    private float charCenterYOriginal;
    public bool escapeValueSent;
    public bool isRespawning;
    public float respawnTimer;
    private bool escapeAvailablePopup;
    private bool isInteracting;
    private string interactingWith;
    private int interactedOnById = -1;
    private GameObject activeInteractable;
    private float interactionTimer;
    private bool interactionLock;
    private float enterSpectatorModeTimer;
    private bool unlimitedStamina;
    public bool insideBubbleShield;
    private EncryptedFloat originalSpeed;
    public EncryptedFloat totalSpeedBoost;
    private float itemSpeedModifier;
    public float weaponSpeedModifier;
    private float originalFpcBodyPosY;
    public float verticalVelocityBeforeLanding;
    private bool onMyMap;
    public GameObject objectCarrying;

    // Game logic helper variables
    public FirstPersonController fpc;
    public bool isNotOnTeamMap;

    private float crouchPosY;
    private float crouchBodyPosY;
    private float crouchBodyScaleY;

    private Vector3 alivePosition;
    private Vector3 deadPosition;
    private float fraction;
    private float deathCameraLerpVar;
    private Vector3 deathCameraLerpPos;
    private bool rotationSaved;

    public float hitTimer;
    public bool skipHitDir;
    public float healTimer;
    public float boostTimer;
    private float overshieldRecoverTimer;
    private float overshieldRecoverAmount;
    private float envDamageTimer;
    public Transform cameraParent;

    // Mission references
    private float detectionLevel;

    private bool initialized;
    public bool waitingOnAccept;
    public Vector3 lastHitFromPos;
	private int lastHitBy;
	private int lastBodyPartHit;
    private float underwaterTimer;
    private float underwaterTakeDamageTimer;
    private bool isInWater;
    public float fightingSpiritTimer;
    public float lastStandTimer;
    private float activeCamoTimer;

    public void PreInitialize()
    {
        if (!IsInGame()) {
            gameObject.SetActive(false);
            return;
        }

        InitPlayer();
    }

    void InitPlayer() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.reviveWindowTimer = -100f;
        if (pView.IsMine) {
            if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
                string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
                // pView.RPC("RpcDisablePlayerForVersus", RpcTarget.AllBuffered, myTeam);
                SetTeamHost();
            }
        }
    }

    public void SyncDataOnJoin(bool init) {
        pView.RPC("RpcAskServerForDataPlayer", RpcTarget.Others, init);
    }

    public void Initialize()
    {
        // Load the in-game necessities
        //DontDestroyOnLoad(gameObject);
        //AddMyselfToPlayerList();

        // Setting original positions for returning from crouching
        charHeightOriginal = charController.height;
        charCenterYOriginal = charController.center.y;

        escapeValueSent = false;
        SetInteracting(false, null);
        originalFpcBodyPosY = fpcBodyRef.transform.localPosition.y;

        health = 100;
        detectionLevel = 0;
        sprintTime = playerScript.stamina;

        // // If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
         if (!pView.IsMine)
         {
             Destroy(viewCam.GetComponent<AudioReverbFilter>());
             Destroy(viewCam.GetComponent<AudioLowPassFilter>());
             Destroy(viewCam.GetComponent<AudioListener>());
             viewCam.enabled = false;
             initialized = true;
             //enabled = false;
             return;
         }

        // gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();

        // Initialize variables
        SetOvershield(skillController.GetOvershield(), false);
        canShoot = true;

        crouchPosY = 0.3f;
        crouchBodyPosY = 0.25f;
        crouchBodyScaleY = 0.66f;

        fraction = 0f;
        deathCameraLerpVar = 0f;
        rotationSaved = false;

        hitTimer = 1f;
        healTimer = 1f;
        boostTimer = 1f;
        underwaterTimer = UNDERWATER_TIMER;
        isRespawning = false;
        respawnTimer = 0f;
        escapeAvailablePopup = false;
        enterSpectatorModeTimer = 0f;
        unlimitedStamina = false;
        itemSpeedModifier = 1f;
        originalSpeed = playerScript.speed;
        totalSpeedBoost = originalSpeed;
        ToggleRagdoll(false);
        InitializeGuardianAngel();
        skillController.SetLastStand();

        StartCoroutine(SpawnInvincibilityRoutine());
        StartCoroutine("RegeneratorRecover");
        StartCoroutine("PainkillerCompound");
        StartCoroutine("UpdateContingencyTimeBoost");
        initialized = true;
    }

    void Start() {
        if (PhotonNetwork.IsMasterClient) {
            Hashtable h = new Hashtable();
            h.Add("inGame", 1);
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        } else {
            if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) != 1) {
                PlayerData.playerdata.DestroyMyself();
            }
        }
        string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
        if (gameMode == "versus") {
            if (PhotonNetwork.IsMasterClient) {
                SetTeamHost(true);
            } else {
                string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
                string thisMap = SceneManager.GetActiveScene().name;
                if (thisMap.EndsWith("_Red") && myTeam == "red") {
                    int redLeader = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
                    if (!GameControllerScript.playerList.ContainsKey(redLeader) || GameControllerScript.playerList[redLeader].objRef == null || !PlayerStillInRoom(redLeader)) {
                        SetTeamHost(true);
                    }
                } else if (thisMap.EndsWith("_Blue") && myTeam == "blue") {
                    int blueLeader = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
                    if (!GameControllerScript.playerList.ContainsKey(blueLeader) || GameControllerScript.playerList[blueLeader].objRef == null || !PlayerStillInRoom(blueLeader)) {
                        SetTeamHost(true);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!initialized) {
            return;
        }
        if (gameController == null)
        {
            GameObject gc = GameObject.FindWithTag("GameController");
            if (gc == null)
            {
                return;
            }
            gameController = gc.GetComponent<GameControllerScript>();
        }

        if (!pView.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.J)) {
            Debug.LogError("height: " + charController.height + ", center: " + charController.center.y);
        }

        UpdateFightingSpirit();
        UpdateLastStand();
        UpdateActiveCamouflage();
        UpdateRegeneration();
        UnlockInteractionLock();
        UpdateEnvDamageTimer();
        UpdateUnderwaterTimer();
        updatePlayerSpeed();
        // Instant respawn hack
        // if (Input.GetKeyDown (KeyCode.P)) {
        //     BeginRespawn ();
        // }
        // Physics sky drop test hack
        // if (Input.GetKeyDown(KeyCode.O)) {
        //     transform.position = new Vector3(transform.position.x, transform.position.y + 20f, transform.position.z);
        // }

         if (enterSpectatorModeTimer > 0f)
         {
             enterSpectatorModeTimer -= Time.deltaTime;
             if (enterSpectatorModeTimer <= 0f)
             {
                 EnterSpectatorMode();
             }
         }

        HandleMissionEvents();
        HandleAlertMessages();

        if (health > 0 && fpc.enabled && fpc.m_IsRunning)
        {
            audioController.PlaySprintSound(true);
            if (!skillController.HasRunNGun()) {
                canShoot = false;
            }
            //animator.SetBool("isSprinting", true);
            fpc.SetSprintingInAnimator(true);
            if (sprintTime > 0f && !unlimitedStamina)
            {
                sprintTime -= Time.deltaTime;
            }
            if (fpc.m_IsRunning && sprintTime <= 0f)
            {
                fpc.sprintLock = true;
            }
        }
        else
        {
            audioController.PlaySprintSound(false);
            // animator.SetBool("isSprinting", false);
            fpc.SetSprintingInAnimator(false);
            if (sprintTime < playerScript.stamina && !unlimitedStamina)
            {
                sprintTime += Time.deltaTime;
            }
            if (!isInteracting)
            {
                canShoot = true;
            }
            else
            {
                canShoot = false;
            }
        }

        if (!PlayerPreferences.playerPreferences.KeyWasPressed("Sprint", true) && fpc.sprintLock)
        {
            fpc.sprintLock = false;
        }

        UpdateOvershieldRecovery();
        DeathCheck();
        if (health <= 0 && fightingSpiritTimer <= 0f && lastStandTimer <= 0f)
        {
            hud.ToggleHintText(null);
            SetInteracting(false, null);
            if (!escapeValueSent)
            {
                escapeValueSent = true;
            }
        }
        else
        {
            CheckForInteractables();
            InteractCheck();
            DeployUseCheck();
        }

        HandleInteracting();

        if (fpc.enabled && fpc.canMove && !hud.container.pauseMenuGUI.pauseActive)
        {
            HandleCrouch();
            if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact")) {
                MarkEnemy();
            }
        }
        
        DetermineEscaped();
        RespawnRoutine();

        UpdateDetectionLevel();
        UpdateDetectionHUD();
        
        FallOffMapProtection();
    }

    void FixedUpdate() {
        if (!initialized) {
            return;
        }
        if (!pView.IsMine) {
            return;
        }
        if (!fpc.m_CharacterController.isGrounded) {
            UpdateVerticalVelocityBeforeLanding();
        }
    }

    void HandleAlertMessages() {
        if (gameController.alertMessage != null) {
            hud.MessagePopup(gameController.alertMessage);
            gameController.alertMessage = null;
        }
    }

    void HandleMissionEvents() {
        if (gameController.currentMap == 1) {
            if (gameController.sectorsCleared == 0 && gameController.objectives.itemsRemaining == 2)
            {
                gameController.sectorsCleared++;
                hud.OnScreenEffect("SECTOR CLEARED!", false);
                gameController.ClearDeadPlayersList();
                BeginRespawn(false);
            }

            if (gameController.objectives.itemsRemaining == 0 && !escapeAvailablePopup)
            {
                escapeAvailablePopup = true;
                hud.MessagePopup("Escape available! Head to the waypoint!");
                hud.ComBoxPopup(2f, "Democko", "Well done. There's an extraction waiting for you on the top of the construction site. Democko signing out.", "HUD/democko");
            }

            // Update assault mode
            hud.UpdateAssaultModeIndHud(gameController.assaultMode);

            // On assault mode changed
            bool h = gameController.assaultMode;
            if (h != gameController.assaultModeChangedIndicator)
            {
                gameController.assaultModeChangedIndicator = h;
                hud.MessagePopup("Your cover is blown!");
                hud.ComBoxPopup(2f, "Democko", "They know you're here! Slot the bastards!", "HUD/democko");
                hud.ComBoxPopup(20f, "Democko", "Cicadas on the rooftops! Watch the rooftops!", "HUD/democko");
            }
        } else if (gameController.currentMap == 2) {
            if (gameController.gameOver) {
                if (gameController.objectives.stepsLeftToCompletion == 1) {
                    gameController.UpdateObjectives();
                    hud.UpdateObjectives();
                    hud.ComBoxPopup(1f, "Democko", "Alright, let's get the hell out of here!", "HUD/democko");
                }
                return;
            }
            // When the initial timer runs out, start the Cicada spawn
            if (gameController.objectives.missionTimer1 <= 0f) {
                if (gameController.spawnMode != SpawnMode.Routine) {
                    gameController.spawnMode = SpawnMode.Routine;
                    hud.MessagePopup("Survive until evac arrives!");
                    hud.ComBoxPopup(3f, "Democko", "You guys have trouble inbound! My NAV scans show Cicadas closing in on you from all over the place!", "HUD/democko");
                    hud.ComBoxPopup(240f, "Democko", "Guys, avoid going outside! This is their territory and they know it well!", "HUD/democko");
                    gameController.objectives.missionTimer2 = 720f;
                    // gameController.objectives.missionTimer2 = 130f;
                }
            } else {
                gameController.objectives.missionTimer1 -= Time.deltaTime;
                return;
            }

            // Halfway through waiting period, trigger a checkpoint to respawn/recover everyone
            if (gameController.sectorsCleared == 0 && gameController.objectives.missionTimer2 <= 360f) {
                gameController.sectorsCleared++;
                hud.OnScreenEffect("SECTOR CLEARED!", false);
                gameController.ClearDeadPlayersList();
                BeginRespawn(false);
                hud.ComBoxPopup(1f, "Red Ruby", "There are Cicadas all over the damn place!", "HUD/redruby");
                hud.ComBoxPopup(4f, "Democko", "We’re about half way there; just hang in there!", "HUD/democko");
            }

            // When two minutes left, have player go select evac point if one isn't chosen yet
            if (gameController.objectives.selectedEvacIndex == -2 && gameController.objectives.missionTimer2 <= 120f) {
                gameController.objectives.selectedEvacIndex = -1;
                if (gameController.objectives.stepsLeftToCompletion != 2) {
                    hud.ComBoxPopup(2f, "Democko", "The chopper’s about two minutes out! These landing zones aren’t clear; you guys need to go out there and mark one with a flare so we can know where to land!", "HUD/democko");
                    hud.MessagePopup("Designate a landing zone for the evac team!");
                    gameController.UpdateObjectives();
                    hud.UpdateObjectives();
                    foreach (GameObject o in gameController.items) {
                        FlareScript f = o.GetComponentInChildren<FlareScript>(true);
                        f.ToggleFlareTemplate(true);
                        f.gameObject.SetActive(true);
                    }
                }
            }

            // When the wait time for evac runs out, choose a random evac spot and make the chopper land there
            if (gameController.objectives.missionTimer2 <= 0f) {
                // If the player hasn't chosen an evac spot yet, reset the timer
                if (gameController.objectives.selectedEvacIndex == -1) {
                    gameController.objectives.selectedEvacIndex = -2;
                    gameController.objectives.missionTimer2 = 120f;
                    hud.ComBoxPopup(0f, "Democko", "You guys didn’t plant the flare down! We’re circling back around!", "HUD/democko");
                    hud.MessagePopup("Designate a landing zone for the evac team!");
                    return;
                } else {
                    // Land chopper in chosen evac spot and alert the team
                    if (gameController.objectives.stepsLeftToCompletion == 1 && gameController.objectives.missionTimer3 <= 0f) {
                        hud.ComBoxPopup(2f, "Democko", "The chopper is here! There’s a lot of heat out here so we can’t stay long, so move quick!", "HUD/democko");
                        hud.MessagePopup("Escape available! Head to the waypoint with the pilot!");
                        gameController.objectives.missionTimer3 = 90f;
                    }
                }
            } else {
                gameController.objectives.missionTimer2 -= Time.deltaTime;
                return;
            }

            if (gameController.objectives.missionTimer3 > 0f) {
                gameController.objectives.missionTimer3 -= Time.deltaTime;
            }

            // Run another timer for everyone being able to escape
            if (gameController.objectives.missionTimer3 <= 0f) {
                gameController.objectives.missionTimer2 = 90f;
                hud.ComBoxPopup(1f, "Democko", "We had to wave off! We'll circle around and come back!", "HUD/democko");
                hud.MessagePopup("Survive until evac returns!");
                Vector3 n = new Vector3(120f, 150f, -1340f);
                Vector3 n2 = new Vector3(gameController.exitPoint.transform.position.x, gameController.exitPoint.transform.position.y + 30f, gameController.exitPoint.transform.position.z - 8f);
                Vector3 n3 = new Vector3(gameController.exitPoint.transform.position.x, gameController.exitPoint.transform.position.y + 1.35f, gameController.exitPoint.transform.position.z - 8f);
                gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().SetDestination(n, false, 30f, FlightMode.Travel);
                gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().SetDestination(n2, false, 50f, FlightMode.Travel);
                gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().SetDestination(n3, false, 5f, FlightMode.Descend);
            }
        }
    }

    [PunRPC]
    void RpcUpdateObjectives() {
        gameController.UpdateObjectives();
        hud.UpdateObjectives();
    }

    [PunRPC]
    void RpcPlayTakeDamageGrunt()
    {
        audioController.PlayGruntSound();
    }

    public void TakeDamage(int d, bool useArmor, bool useOvershield, Vector3 hitFromPos, int hitBy, int bodyPartHit)
    {
        if (d <= 0) return;
        if (fightingSpiritTimer > 0f) return;
        if (lastStandTimer > 0f) return;
        if (health <= 0f) return;
        // If registering a gun shot (hit by = 0), then the server must register it.
        if (!pView.IsMine && hitBy != 0) return;

        // Send over network
        pView.RPC("RpcTakeDamage", RpcTarget.All, d, useArmor, useOvershield, hitFromPos.x, hitFromPos.y, hitFromPos.z, hitBy, bodyPartHit);
    }

    [PunRPC]
    void RpcTakeDamage(int d, bool useArmor, bool useOvershield, float hitFromX, float hitFromY, float hitFromZ, int hitBy, int bodyPartHit)
    {
        if (gameObject.layer == 0) return;
        if (pView.IsMine)
        {
            if (d <= 0) return;
            if (fightingSpiritTimer > 0f) return;
            if (lastStandTimer > 0f) return;
            if (health <= 0f) return;
            // See if you dodged the bullet first (avoidability). Return if you did
            if (hitBy == 0) {
                int avoidChance = (int)(Mathf.Clamp((playerScript.avoidability - 1f) + skillController.GetAvoidabilityBoost() + skillController.GetIntimidationBoost() + skillController.GetDdosAccuracyReduction(), 0f, MAX_AVOID) * 100f);
                if (Random.Range(0, 100) < avoidChance) {
                    return;
                }
            }
            bool usingOvershield = skillController.GetOvershield() > 0 && (int)overshield > 0;
            ResetHitTimer();
            skillController.RegenerationReset();
            if (!skillController.HasRusticCowboy() && !usingOvershield) {
                wepActionScript.mouseLook.ActivateFlinch();
            }
            // Calculate damage done including armor
            // Apply Rampage skill effect
            if (useArmor) {
                // See if you absorbed the gunshot through Bullet Sponge skill
                if (hitBy == 0 && skillController.BulletSpongeAbsorbed()) {
                    d = 0;
                } else {
                    if (skillController.GetRampageBoost()) {
                        // 5% chance the damage isn't fully absorbed - will do 1 damage if not
                        int r = Random.Range(0, 100);
                        if (r < 5) {
                            d = 1;
                        } else {
                            d = 0;
                        }
                    } else {
                        if (hitBy == 2) {
                            d = (int)((float)d * (1f - skillController.GetMeleeResistance()) * (1f - skillController.GetMartialArtsDefenseBoost()));
                        } else {
                            float damageReduction = ((playerScript.armor + skillController.GetHeadstrongBoost()) * skillController.GetArmorBoost()) - 1f;
                            damageReduction = Mathf.Clamp(damageReduction, 0f, 1f);
                            d = Mathf.RoundToInt((float)d * (1f - damageReduction));
                        }
                    }
                }
            }
            // Painkiller skill damage dampening
            d = (int)((float)d * (1f - skillController.GetPainkillerTotalAmount()));

            if (usingOvershield) {
                if (overshield - d <= 0f) {
                    audioController.PlayOvershieldPopSound();
                } else {
                    audioController.PlayHitSound(true);
                }
            } else {
                audioController.PlayHitSound(false);
                pView.RPC("RpcPlayTakeDamageGrunt", RpcTarget.All);
            }

            // if (godMode)
            // {
            //     d = 0;
            // }
            lastHitFromPos = new Vector3(hitFromX, hitFromY, hitFromZ);
            lastHitBy = hitBy;
            lastBodyPartHit = bodyPartHit;

            if (usingOvershield) {
                SetOvershield(Mathf.Max(0, overshield - d), true);
                overshieldRecoverTimer = skillController.GetOvershieldRecoverTime();
                overshieldRecoverAmount = 0f;
            } else {
                SetHealth(health - d, false);
            }
        }
    }

    [PunRPC]
    void RpcDisablePlayerForVersus(string myTeam) {
        int isRedMap = 0;
        if (SceneManager.GetActiveScene().name.EndsWith("_Red"))
        {
            isRedMap = 1;
        }
        else if (SceneManager.GetActiveScene().name.EndsWith("_Blue"))
        {
            isRedMap = -1;
        }
        onMyMap = ((myTeam == "red" && isRedMap == 1) || (myTeam == "blue" && isRedMap == -1));
        if (!onMyMap)
        {
            DisablePlayerForVersus();
        }
    }

    void UnlockInteractionLock() {
        if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", false, true)) {
            interactionLock = false;
        }
    }

    public void HandleCrouch()
    {
        if (PlayerPreferences.playerPreferences.KeyWasPressed("Crouch") && !fpc.m_IsRunning && !fpc.GetIsSwimming() && !fpc.GetIsIncapacitated())
        {
            fpc.m_IsCrouching = !fpc.m_IsCrouching;
            if (!fpc.IsFullyMobile()) {
                fpc.m_IsCrouching = false;
            }

            FpcCrouch(fpc.m_IsCrouching ? 'c' : 's');
            fpc.SetCrouchingInAnimator(fpc.m_IsCrouching);

            // Collect the original y position of the FPS controller since we're going to move it downwards to crouch
            if (fpc.m_IsCrouching) {
                charController.height = 1f;
                charController.center = new Vector3(0f, 0.54f, 0f);
                // Network it
                pView.RPC("RpcCrouch", RpcTarget.Others, 1f, 0.54f);
            } else {
                charController.height = charHeightOriginal;
                charController.center = new Vector3(0f, charCenterYOriginal, 0f);
                // Network it
                pView.RPC("RpcCrouch", RpcTarget.Others, charHeightOriginal, charCenterYOriginal);
            }
        }
    }

    public void HandleJumpAfterCrouch() {
        fpc.m_IsCrouching = false;

        FpcCrouch(fpc.m_IsCrouching ? 'c' : 's');
        fpc.SetCrouchingInAnimator(fpc.m_IsCrouching);

        charController.height = charHeightOriginal;
        charController.center = new Vector3(0f, charCenterYOriginal, 0f);
        // Network it
        pView.RPC("RpcCrouch", RpcTarget.Others, charHeightOriginal, charCenterYOriginal);
    }

    void FpcCrouch(char mode) {
        if (mode == 'c') {
            fpcBodyRef.transform.localPosition = new Vector3(fpcBodyRef.transform.localPosition.x, -0.5f, fpcBodyRef.transform.localPosition.z);
        } else if (mode == 'p') {
            fpcBodyRef.transform.localPosition = new Vector3(fpcBodyRef.transform.localPosition.x, -1f, fpcBodyRef.transform.localPosition.z);
        } else {
            fpcBodyRef.transform.localPosition = new Vector3(fpcBodyRef.transform.localPosition.x, originalFpcBodyPosY, fpcBodyRef.transform.localPosition.z);
        }
    }

    [PunRPC]
    public void RpcCrouch(float height, float center)
    {
        if (gameObject.layer == 0) return;
        charController.height = height;
        charController.center = new Vector3(0f, center, 0f);   
    }

    void DeathCheck()
    {
        if (health <= 0 && fightingSpiritTimer <= 0f && lastStandTimer <= 0f)
        {
            if (fpc.enabled) {
                if (activeCamo) {
                    ToggleActiveCamo(false, 0f);
                }
                equipmentScript.ToggleFirstPersonBody(false);
                equipmentScript.ToggleFullBody(true);
                equipmentScript.ToggleMesh(true);
                //weaponScript.SwitchWeaponToFullBody();
                fpc.SetIsDeadInAnimator(true);
                StartCoroutine(DelayToggleRagdoll(0.2f, true));
                SetInteracting(false, null);
                DropCarrying();
                fpc.SetIsIncapacitated(false);
                hud.SetCarryingText(null);
                TriggerPlayerDownAlert();
                DeactivateFightingSpirit();
                DeactivateLastStand();
                hud.container.voiceCommandsPanel.SetActive(false);
                hud.container.skillPanel.SetActive(false);
                hud.container.revivePlayerPanel.SetActive(false);
                skipHitDir = false;
            }
            fpc.enabled = false;
            if (!rotationSaved)
            {
                if (escapeValueSent)
                {
                    gameController.ConvertCounts(-1);
                }
                hud.ToggleHUD(false);
                hud.ToggleSpectatorMessage(true);
                // deathCameraLerpPos = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y + 2.5f, headTransform.localPosition.z - 4.5f);
                enterSpectatorModeTimer = 6f;
                viewCam.transform.SetParent(transform);
                viewCam.fieldOfView = 60;
                rotationSaved = true;
                AddToTotalDeaths();
            }

            deathCameraLerpPos = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y + 2.5f, headTransform.localPosition.z - 4.5f);
            DeathCameraEffect();
        }
    }

    void AddToTotalDeaths()
    {
        gameController.AddToTotalDeaths(PhotonNetwork.LocalPlayer.ActorNumber);
    }

    void HandleInteracting() {
        if (isInteracting) {
            // Disable movement
            fpc.canMove = false;
            // Set interaction HUD
            hud.ToggleChatText(false);
            // Handle interaction timer
            if (interactingWith == "Bomb") {
                hud.ToggleActionBar(true, "DEFUSING...");
                interactionTimer += (Time.deltaTime / (BOMB_DEFUSE_TIME * (1f - skillController.GetDexterityBoost())));
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f)
                {
                    BombScript b = activeInteractable.GetComponent<BombScript>();
                    interactionTimer = 0f;
                    pView.RPC("RpcDefuseBomb", RpcTarget.AllBufferedViaServer, b.bombId);
                    activeInteractable = null;
                    interactionLock = true;
                }
            } else if (interactingWith == "Deploy") {
                hud.ToggleActionBar(true, "USING...");
                interactionTimer += (Time.deltaTime / (DEPLOY_USE_TIME * (1f - skillController.GetDexterityBoost())));
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f) {
                    DeployableScript d = activeInteractable.GetComponent<DeployableScript>();
                    interactionTimer = 0f;
                    if (d.deployableName == "Ammo Bag") {
                        wepActionScript.UseAmmoBag(d.deployableId);
                    } else if (d.deployableName == "First Aid Kit") {
                        wepActionScript.UseFirstAidKit(d.deployableId);
                    }
                    interactionLock = true;
                }
            } else if (interactingWith == "Flare") {
                hud.ToggleActionBar(true, "POPPING FLARE...");
                interactionTimer += (Time.deltaTime / (DEPLOY_USE_TIME * (1f - skillController.GetDexterityBoost())));
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f)
                {
                    FlareScript f = activeInteractable.GetComponent<FlareScript>();
                    interactionTimer = 0f;
                    pView.RPC("RpcPopFlare", RpcTarget.AllBufferedViaServer, f.flareId);
                    activeInteractable = null;
                    interactionLock = true;
                }
            } else if (interactingWith == "Npc") {
                hud.ToggleActionBar(true, "INTERACTING...");
                interactionTimer += (Time.deltaTime / (NPC_INTERACT_TIME * (1f - skillController.GetDexterityBoost())));
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f)
                {
                    NpcScript n = activeInteractable.GetComponent<NpcScript>();
                    interactionTimer = 0f;
                    pView.RPC("RpcCarryNpc", RpcTarget.All, pView.Owner.ActorNumber);
                    fpc.m_IsCrouching = false;
                    activeInteractable = null;
                    interactionLock = true;
                }
            } else if (interactingWith == "PlayerHeal") {
                hud.ToggleActionBar(true, "HEALING TEAMMATE...");
                bool startedHealing = (interactionTimer == 0f);
                interactionTimer += (Time.deltaTime / HEAL_TIME);
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f)
                {
                    PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
                    interactionTimer = 0f;
                    p.SetHealth(p.health + skillController.GetFlatlineHealAmount(), true);
                    SetHealth(health - skillController.GetFlatlineSacrificeAmount(), false);
                    activeInteractable = null;
                    interactionLock = true;
                } else {
                    if (startedHealing) {
                        PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
                        p.ToggleProceduralInfo("PLEASE STAND STILL WHILE YOUR TEAMMATE HEALS YOU", true, PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                }
            } else if (interactingWith == "PlayerRevive") {
                hud.ToggleActionBar(true, "REVIVING TEAMMATE...");
                bool startedReviving = (interactionTimer == 0f);
                interactionTimer += (Time.deltaTime / (REVIVE_TIME * (1f - skillController.GetDexterityBoost())));
                hud.SetActionBarSlider(interactionTimer);
                if (interactionTimer >= 1f)
                {
                    PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
                    interactionTimer = 0f;
                    p.LastStandRevive();
                    activeInteractable = null;
                    interactionLock = true;
                } else {
                    if (startedReviving) {
                        PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
                        p.ToggleProceduralInfo("PLEASE STAY STILL WHILE YOUR TEAMMATE REVIVES YOU", true, PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                }
            }
        } else {
            if (!hud.container.inGameMessenger.inputText.enabled) {
                fpc.canMove = true;
            }
            if (!wepActionScript.deployInProgress) {
                hud.ToggleActionBar(false, null);
                // hud.ToggleChatText(true);
            }
            if (interactionTimer > 0f) {
                Debug.Log("Clearing interacting on");
                pView.RPC("RpcClearInteractingOn", RpcTarget.All);
            }
            interactionTimer = 0f;
        }
    }

    [PunRPC]
    void RpcClearInteractingOn()
    {
        PlayerActionScript p = PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>();
        if (pView.Owner.ActorNumber == p.interactedOnById) {
            p.ToggleProceduralInfo(null, true, -1);
        }
    }

    void InteractCheck() {
        HealPlayerCheck();
        RevivePlayerCheck();
        if (gameController.currentMap == 1) {
            BombDefuseCheck();
        } else if (gameController.currentMap == 2) {
            CarryNpcCheck();
            DropOffNpcCheck();
            PopFlareCheck();
        }
    }

    // If map objective is defusing bombs, this method checks if the player is near any bombs
    void BombDefuseCheck()
    {
        if (gameController == null || gameController.items == null)
        {
            return;
        }

        // Is near and looking at a bomb that can be defused
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0 && lastStandTimer <= 0f) {
            BombScript b = activeInteractable.GetComponent<BombScript>();
            if (b != null) {
                if (b.defused) {
                    SetInteracting(false, null);
                    return;
                }
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "Bomb");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO DEFUSE");
                }
            }
        } else {
            // Stop using the deployable
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void HealPlayerCheck()
    {
        if (skillController.CanHealPlayers() && activeInteractable != null && !hud.PauseIsActive() && health > 0 && skillController.GetFlatlineSacrificeAmount() < health && lastStandTimer <= 0f) {
            PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
            if (p != null && skillController.GetFlatlineHealAmount() > 0) {
                if (p.health <= 0 || p.health >= 100 || p.fightingSpiritTimer > 0f || p.lastStandTimer > 0f) {
                    SetInteracting(false, null);
                    return;
                }
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "PlayerHeal");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO HEAL TEAMMATE");
                }
            }
        } else {
            // Stop using the deployable
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void RevivePlayerCheck()
    {
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0 && lastStandTimer <= 0f) {
            PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
            if (p != null && p.lastStandTimer > 0f) {
                if (p.lastStandTimer <= 0f) {
                    SetInteracting(false, null);
                    return;
                }
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "PlayerRevive");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO REVIVE TEAMMATE");
                }
            }
        } else {
            // Stop using the deployable
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void PopFlareCheck() {
        if (gameController == null || gameController.objectives.selectedEvacIndex >= 0) {
            return;
        }

        // Is near and looking at a flare
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0 && lastStandTimer <= 0f) {
            FlareScript f = activeInteractable.GetComponent<FlareScript>();
            if (f != null) {
                if (gameController.objectives.selectedEvacIndex >= 0) {
                    SetInteracting(false, null);
                    return;
                }
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "Flare");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO POP FLARE");
                }
            }
        } else {
            // Stop using the deployable
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void CarryNpcCheck() {
        if (gameController == null || gameController.vipRef == null)
        {
            return;
        }

        if (activeInteractable != null && !hud.PauseIsActive() && health > 0 && lastStandTimer <= 0f) {
            NpcScript n = activeInteractable.GetComponent<NpcScript>();
            if (n != null) {
                if (n.actionState == NpcActionState.Carried || n.actionState == NpcActionState.Dead) {
                    SetInteracting(false, null);
                    return;
                }
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "Npc");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO INTERACT");
                }
            }
        } else {
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void DropOffNpcCheck() {
        if (gameController == null || gameController.vipRef == null)
        {
            return;
        }

        if (objectCarrying != null && objectCarrying.GetComponent<NpcScript>().carriedByPlayerId == pView.Owner.ActorNumber && !hud.PauseIsActive()) {
            NpcScript n = objectCarrying.GetComponent<NpcScript>();
            if (n != null) {
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Drop") && !interactionLock) {
                    // Drop off the NPC
                    DropCarrying();
                    hud.SetCarryingText(null);
                }
            }
        }
    }

    void DeployUseCheck() {
        // If we are looking at the deploy item and near it, we can use it
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0 && lastStandTimer <= 0f) {
            DeployableScript d = activeInteractable.GetComponent<DeployableScript>();
            if (d != null) {
                if (PlayerPreferences.playerPreferences.KeyWasPressed("Interact", true) && !interactionLock) {
                    // Use the deployable
                    SetInteracting(true, "Deploy");
                    hud.ToggleHintText(null);
                } else {
                    // Stop using the deployable
                    SetInteracting(false, null);
                    hud.ToggleHintText("HOLD [" + PlayerPreferences.playerPreferences.keyMappings["Interact"].key.ToString() + "] TO USE [" + d.deployableName + "]");
                }
            }
        } else {
            // Stop using the deployable
            SetInteracting(false, null);
            hud.ToggleHintText(null);
        }
    }

    void CheckForInteractables() {
        RaycastHit hit;
        int interactableMask = (1 << 18 | 1 << 15 | 1 << 9);
        if (Physics.Raycast(viewCam.transform.position, viewCam.transform.forward, out hit, INTERACTION_DISTANCE, interactableMask)) {
            if (hit.transform.gameObject.tag == "Human") {
                NpcScript n = hit.transform.GetComponentInParent<NpcScript>();
                if (n != null) {
                    if (n.actionState == NpcActionState.Incapacitated) {
                        activeInteractable = n.transform.gameObject;
                    }
                }
            } else if (hit.transform.gameObject.tag == "Player") {
                activeInteractable = hit.transform.gameObject;
            } else {
                activeInteractable = hit.transform.gameObject;
            }
        } else {
            if (activeInteractable != null) {
                PlayerActionScript p = activeInteractable.GetComponent<PlayerActionScript>();
                if (p != null) {
                    p.ToggleProceduralInfo(null, true, -1);
                }
            }
            activeInteractable = null;
        }
    }

    public void ResetHitTimer()
    {
        hitTimer = 0f;
    }

    public void ResetHealTimer()
    {
        healTimer = 0f;
    }

    public void ResetBoostTimer() {
        boostTimer = 0f;
    }

    [PunRPC]
    void RpcKillMyself()
    {
        health = 0;
        lastHitFromPos = transform.position;
		lastHitBy = 2;
		lastBodyPartHit = 0;
    }

    public void SetHealth(int h, bool useParticleEffect)
    {
        pView.RPC("RpcSetHealth", RpcTarget.All, h, useParticleEffect);
    }

    [PunRPC]
    void RpcSetHealth(int h, bool useParticleEffect)
    {
        if (gameObject.layer == 0) return;
        if (fightingSpiritTimer > 0f) return;
        if (lastStandTimer > 0f) return;
        h = Mathf.Clamp(h, 0, 100);
        int prevHealth = health;
        if (pView.IsMine && h <= 0) {
            if (skillController.UseLastStand()) {
                ActivateLastStand();
            } else {
                if (fightingSpiritTimer <= 0f) {
                    ActivateFightingSpirit();
                }
            }
        }
        health = h;
        if (useParticleEffect) {
            PlayHealParticleEffect();
            // ResetHealTimer();
        }
        if (pView.IsMine) {
            skillController.HandleHealthChangeEvent(h);
            ToggleProceduralInfo(null, false, -1);
            // Motivate boost
            // If the player is dead, remove the boost from everyone
            // If health went down and is now less than the trigger when it previously wasn't, then send the boost to everyone
            // Else if health went up and is now above the trigger when it previously wasn't, then remove the boost from everyone
            int trigger = skillController.GetMotivateHealthTrigger();
            if (health <= 0) {
                pView.RPC("RpcDeactivateMotivateSkillFromThisPlayer", RpcTarget.All);
            } else if (prevHealth > trigger && health <= trigger) {
                pView.RPC("RpcActivateMotivateSkillFromThisPlayer", RpcTarget.All, skillController.GetMotivateDamageBoost());
            } else if (prevHealth <= trigger && health > trigger) {
                pView.RPC("RpcDeactivateMotivateSkillFromThisPlayer", RpcTarget.All);
            }
        }
    }

    void SetOvershield(float o, bool playPopEffect)
    {
        pView.RPC("RpcSetOvershield", RpcTarget.All, o, playPopEffect);
    }

    [PunRPC]
    void RpcSetOvershield(float o, bool playPopEffect)
    {
        if (gameObject.layer == 0) return;
        overshield = o;
        if (pView.IsMine) {
            // If overshield is less than 20% of full overshield, then start playing the warning color flashes
            if (overshield < (skillController.GetOvershield() / 5f)) {
                if (!hud.overshieldFlashActive) {
                    hud.ToggleOvershieldWarningFlash(true);
                    audioController.PlayOvershieldWarningSound(true);
                }
            }
        }
        if (o <= 0f && playPopEffect) {
            overshieldBreakEffect.Play();
        }
    }

    [PunRPC]
    void RpcActivateMotivateSkillFromThisPlayer(float damageBoost)
    {
        if (gameObject.layer == 0) return;
        int actorNo = pView.Owner.ActorNumber;
        SkillController mySkillController = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>();
        mySkillController.AddMotivateBoost(actorNo, damageBoost);
        mySkillController.AddToMotivateDamageBoost(damageBoost);
    }

    [PunRPC]
    void RpcDeactivateMotivateSkillFromThisPlayer()
    {
        if (gameObject.layer == 0) return;
        int actorNo = pView.Owner.ActorNumber;
        SkillController mySkillController = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>();
        float dmgRemoval = mySkillController.RemoveMotivateBoost(actorNo);
        mySkillController.RemoveFromMotivateDamageBoost(dmgRemoval);
    }

    void DetermineEscaped()
    {
        if (gameController.objectives.escapeAvailable)
        {
            if (!escapeValueSent)
            {
                if (health > 0 && Vector3.Distance(gameController.exitPoint.transform.position, transform.position) <= 10f && transform.position.y >= (gameController.exitPoint.transform.position.y - 1f))
                {
                    gameController.IncrementEscapeCount();
                    escapeValueSent = true;
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
        bool obstructed = Physics.Linecast(a, b, out hitInfo, ignoreLayers, QueryTriggerInteraction.Ignore);
        if (obstructed) {
            if (hitInfo.transform.gameObject.layer == 18) {
				if (hitInfo.transform.gameObject.GetComponent<BombScript>() == null) {
					obstructed = false;
				}
			}
        }
		return obstructed;
	}

    void HandleExplosiveEffects(Collider other)
    {
        // Handle explosive damage
        if (other.gameObject.name.Contains("M67"))
        {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            // If a ray casted from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID))
            {
                // Determine how far from the explosion the enemy was
                float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
                float blastRadius = other.gameObject.GetComponent<ThrowableScript>().blastRadius;
                distanceFromGrenade = Mathf.Min(distanceFromGrenade, blastRadius);
                float scale = 1f - (distanceFromGrenade / blastRadius);

                // Scale damage done to enemy by the distance from the explosion
                Weapon grenadeStats = InventoryScript.itemData.weaponCatalog[t.rootWeapon];
                int damageReceived = (int)(grenadeStats.damage * scale);

                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
                // Deal damage to the player
                TakeDamage(damageReceived, false, true, other.gameObject.transform.position, 1, 0);
                //ResetHitTimer();
            }
        }
        else if (other.gameObject.name.Contains("XM84"))
        {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID))
            {
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

                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);

                totalDisorientationTime *= distanceMultiplier * rotationMultiplier;
                hud.FlashbangEffect(totalDisorientationTime);
                audioController.PlayFlashbangEarRingSound(totalDisorientationTime);
            }
        } else if (other.gameObject.name.Contains("RPG-7")) {
            LauncherScript l = other.gameObject.GetComponent<LauncherScript>();
            // If a ray casted from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !l.isLive && !l.PlayerHasBeenAffected(pView.ViewID))
            {
                // Determine how far from the explosion the enemy was
                float distanceFromExplosion = Vector3.Distance(transform.position, other.gameObject.transform.position);
                float blastRadius = other.gameObject.GetComponent<LauncherScript>().blastRadius;
                distanceFromExplosion = Mathf.Min(distanceFromExplosion, blastRadius);
                float scale = 1f - (distanceFromExplosion / blastRadius);

                // Scale damage done to enemy by the distance from the explosion
                Weapon launcherStats = InventoryScript.itemData.weaponCatalog[l.rootWeapon];
                int damageReceived = (int)(launcherStats.damage * scale);

                // Validate that this enemy has already been affected
                l.AddHitPlayer(pView.ViewID);
                // Deal damage to the player
                TakeDamage(damageReceived, false, true, other.gameObject.transform.position, 1, 0);
                //ResetHitTimer();
            }
        }
    }

    public void TriggerEcmFeedback(int level, float duration)
    {
        string s = "";
        int aliveCount = 0;
        int[] aliveIds = new int[gameController.enemyList.Count];
        foreach (KeyValuePair<int, GameObject> p in gameController.enemyList) {
            if (p.Value.GetComponent<BetaEnemyScript>().health > 0) {
                aliveIds[aliveCount++] = p.Key;
            }
        }
        if (level == 1) {
            int aliveToTrigger = aliveCount / 3;
            if (aliveCount > 0 && aliveCount <= 2) {
                aliveCount = 1;
            }
        } else if (level == 2) {
            int aliveToTrigger = aliveCount / 2;
            if (aliveCount > 0 && aliveCount <= 1) {
                aliveCount = 1;
            }
        } else if (level == 3) {
            int aliveToTrigger = 7 * aliveCount / 10;
            if (aliveCount > 0 && aliveCount <= 7) {
                aliveCount = 1;
            }
        }
        for (int i = 0; i < aliveCount; i++) {
            if (i != 0) {
                s += ",";
            }
            s += aliveIds[i];
        }
        pView.RPC("RpcSyncEcmFeedback", RpcTarget.All, s, duration);
    }

    [PunRPC]
    void RpcSyncEcmFeedback(string enemyIds, float duration)
    {
        if (gameObject.layer == 0) return;
        string[] ss = enemyIds.Split(',');
        Debug.LogError("enemies disorint: " + enemyIds);
        foreach (string s in ss) {
            int i = int.Parse(s);
            GameObject o = gameController.enemyList[i];
            BetaEnemyScript b = o.GetComponent<BetaEnemyScript>();
            if (b.health > 0) {
                b.SetThisEnemyDisoriented(duration);
            }
        }
    }
    
    public void TriggerInfraredScan(float duration)
    {
        string s = "";
        int aliveCount = 0;
        int[] aliveIds = new int[gameController.enemyList.Count];
        foreach (KeyValuePair<int, GameObject> p in gameController.enemyList) {
            if (p.Value.GetComponent<BetaEnemyScript>().health > 0) {
                aliveIds[aliveCount++] = p.Key;
            }
        }
        for (int i = 0; i < aliveCount; i++) {
            if (i != 0) {
                s += ",";
            }
            s += aliveIds[i];
        }
        pView.RPC("RpcSyncInfraredScan", RpcTarget.All, s, duration);
    }

    [PunRPC]
    void RpcSyncInfraredScan(string enemyIds, float duration)
    {
        if (gameObject.layer == 0) return;
        string[] ss = enemyIds.Split(',');
        foreach (string s in ss) {
            int i = int.Parse(s);
            GameObject o = gameController.enemyList[i];
            BetaEnemyScript b = o.GetComponent<BetaEnemyScript>();
            if (b.health > 0) {
                b.MarkEnemyOutlineInstant(duration);
            }
        }
    }

    void HandleEnvironmentEffects(Collider other) {
		if (health <= 0 || envDamageTimer < ENV_DAMAGE_DELAY || fightingSpiritTimer > 0f || lastStandTimer > 0f) {
			return;
		}

		if (other.gameObject.tag.Equals("Fire")) {
			FireScript f = other.gameObject.GetComponent<FireScript>();
			int damageReceived = (int)(f.damage);
			TakeDamage(damageReceived, false, true, other.gameObject.transform.position, 2, 0);
			ResetEnvDamageTimer();
		}
	}

    void HandlePickups(Collider other)
    {
        if (other.gameObject.tag.Equals("AmmoBox"))
        {
            aud.clip = ammoPickupSound;
            aud.Play();
            int loadedMaxAmmo = InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].maxAmmo - InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].clipCapacity;
            int restoreAmt = Mathf.Max(1, (int)((float)(loadedMaxAmmo / 10) * (1f + skillController.GetResourcefulBoost())));
            if (weaponScript.currentlyEquippedType == 1) {
                wepActionScript.totalAmmoLeft += restoreAmt;
                wepActionScript.totalAmmoLeft = Mathf.Min(wepActionScript.totalAmmoLeft, (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].maxAmmo + (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].clipCapacity * skillController.GetProviderBoost())) - wepActionScript.currentAmmo);
                weaponScript.SyncAmmoCounts();
            } else {
                weaponScript.RefillAmmoOnPrimary(restoreAmt);
            }
            pView.RPC("RpcDestroyPickup", RpcTarget.All, other.gameObject.GetComponent<PickupScript>().pickupId, gameController.teamMap);
        }
        else if (other.gameObject.tag.Equals("HealthBox"))
        {
            aud.clip = healthPickupSound;
            aud.Play();
            ResetHealTimer();
            SetHealth(100, false);
            pView.RPC("RpcDestroyPickup", RpcTarget.All, other.gameObject.GetComponent<PickupScript>().pickupId, gameController.teamMap);
        }
    }

    [PunRPC]
    void RpcDestroyPickup(int pickupId, string team) {
        if (gameObject.layer == 0) return;
        GameObject o = gameController.GetPickup(pickupId);
        o.GetComponent<PickupScript>().DestroyPickup();
        gameController.DestroyPickup(pickupId);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        LauncherScript l = hit.collider.gameObject.GetComponent<LauncherScript>();
        if (l != null) {
            l.Explode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (pView.IsMine)
        {
            if (health <= 0 || fightingSpiritTimer > 0f || lastStandTimer > 0f) {
                return;
            }
            HandleExplosiveEffects(other);
            HandlePickups(other);
        }
        if (other.GetComponentInParent<BubbleShieldScript>() != null) {
            insideBubbleShield = true;
        }
        // else
        // {
        //     if (other.gameObject.tag.Equals("AmmoBox"))
        //     {
        //         other.gameObject.GetComponent<PickupScript>().DestroyPickup();
        //     }
        //     else if (other.gameObject.tag.Equals("HealthBox"))
        //     {
        //         other.gameObject.GetComponent<PickupScript>().DestroyPickup();
        //     }
        // }
        if (other.gameObject.layer == WATER_LAYER) {
            isInWater = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<BubbleShieldScript>() != null) {
            insideBubbleShield = false;
        }
        if (other.gameObject.layer == WATER_LAYER) {
            isInWater = false;
        }
    }

    void OnTriggerStay(Collider other) {
        if (pView.IsMine)
        {
            if (health <= 0 || fightingSpiritTimer > 0f || lastStandTimer > 0f) {
                return;
            }
            HandleEnvironmentEffects(other);
        }
    }

    void DeathCameraEffect()
    {
        deathCameraLerpVar += (Time.deltaTime / 4f);
        viewCam.transform.LookAt(headTransform);
        viewCam.transform.localPosition = Vector3.Lerp(viewCam.transform.localPosition, deathCameraLerpPos, deathCameraLerpVar);
    }

    void EnterSpectatorMode()
    {
        if (thisSpectatorCam == null) {
            pView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, false);
            thisSpectatorCam = Instantiate(spectatorCam, Vector3.zero, Quaternion.Euler(Vector3.zero));
            thisSpectatorCam.transform.SetParent(null);
        }
    }

    void LeaveSpectatorMode()
    {
        Destroy(thisSpectatorCam);
        thisSpectatorCam = null;
        pView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, true);
    }

    [PunRPC]
    void RpcChangePlayerDisableStatus(bool status)
    {
        ChangePlayerDisableStatus(status);
    }

    void ChangePlayerDisableStatus(bool status) {
        if (gameObject.layer == 0) return;
        if (!status) {
            equipmentScript.DespawnPlayer();
            weaponScript.DespawnPlayer();
        } else {
            charController.height = 1f;
            charController.center = new Vector3(0f, charCenterYOriginal, 0f);
        }
        hudMarker.enabled = status;
        hudMarker2.enabled = status;
        // charController.enabled = status;
        ToggleRagdoll(!status);
        if (pView.IsMine)
        {
            fpc.enabled = status;
            fpc.m_MouseLook.ResetRot();
            viewCam.GetComponent<AudioListener>().enabled = status;
            viewCam.transform.localPosition = new Vector3(0.001179763f, 0.003319679f, -0.000299095f);
            viewCam.transform.localRotation = Quaternion.Euler(-11.903f, 90f, 0f);
            viewCam.enabled = status;
            wepActionScript.enabled = status;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        escapeValueSent = false;
        if (gameController.matchType == 'V' && onMyMap && otherPlayer != null) {
            RemovePlayerAsHost(otherPlayer.ActorNumber);
            SetTeamHost();
        }
        if (pView.IsMine) {
            if (otherPlayer.ActorNumber == interactedOnById) {
                ToggleProceduralInfo(null, false, -1);
            }
            SkillController mySkillController = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>();
            float dmgRemoval = mySkillController.RemoveMotivateBoost(otherPlayer.ActorNumber);
            mySkillController.RemoveFromMotivateDamageBoost(dmgRemoval);
        }
    }
    
    void RemovePlayerAsHost(int pId) {
        string t = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"] + "Host";
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(t)) return;
        if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties[t]) == pId) {
            Hashtable h = new Hashtable();
            h[t] = null;
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Destroy(gameObject);
    }

    void BeginRespawn(bool wasRevive)
    {
        enterSpectatorModeTimer = 0f;
        if (health <= 0)
        {
            gameController.ConvertCounts(0);
            gameController.gameOver = false;
            // Flash the respawn time bar on the screen
            hud.RespawnBar();
            // Then, actually start the respawn process
            respawnTimer = 4f;
            isRespawning = true;
            if (wasRevive) {
                gameController.ClearReviveWindowTimer();
            }
        } else {
            SetHealth(100, false);
        }
    }

    void RespawnRoutine()
    {
        if (isRespawning)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                isRespawning = false;
                Respawn();
            }
        }
    }

    // Reset character health, scale, rotation, position, ammo, disabled HUD components, disabled scripts, death variables, etc.
    public void Respawn()
    {
        waitingOnAccept = false;
        SetHealth(100, false);
        viewCam.transform.SetParent(cameraParent);
        viewCam.transform.GetComponent<Camera>().fieldOfView = 60;
        hud.ToggleHUD(true);
        hud.ToggleSpectatorMessage(false);
        fpc.m_IsCrouching = false;
        fpc.m_IsWalking = false;
        FpcCrouch('s');
        fpc.SetIsIncapacitated(false);
        escapeValueSent = false;
        canShoot = true;
        fpc.canMove = true;
        fpc.SetMouseDynamicsForMelee(false);
        fraction = 0f;
        deathCameraLerpVar = 0f;
        rotationSaved = false;
        hitTimer = 1f;
        healTimer = 1f;
        boostTimer = 1f;
        insideBubbleShield = false;
        SetInteracting(false, null);
        interactionTimer = 0f;
        wepActionScript.deployInProgress = false;
        wepActionScript.deployTimer = 0f;
        wepActionScript.totalAmmoLeft = wepActionScript.weaponStats.maxAmmo;
        wepActionScript.currentAmmo = wepActionScript.weaponStats.clipCapacity;
        equipmentScript.ToggleFullBody(false);
        equipmentScript.fullBodyRef.transform.localPosition = Vector3.zero;
        equipmentScript.fullBodyRef.transform.localRotation = Quaternion.identity;
        equipmentScript.ToggleFirstPersonBody(true);
        equipmentScript.ToggleFpcMesh(true);
        //weaponScript.SwitchWeaponToFpcBody();
        equipmentScript.RespawnPlayer();
        weaponScript.RespawnPlayer();
        wepActionScript.ResetMyActionStates();
        fpc.ResetAnimationState();
        fpc.ResetFPCAnimator(weaponScript.currentlyEquippedType);
        skillController.SetLastStand();

        // Send player back to spawn position, reset rotation, leave spectator mode
        //transform.rotation = Quaternion.Euler(Vector3.zero);
        // transform.position = gameController.spawnLocation.position;
        fpc.m_MouseLook.Init(fpc.charTransform, fpc.spineTransform, fpc.fpcTransformSpine, fpc.fpcTransformBody);
        LeaveSpectatorMode();
        transform.position = gameController.spawnLocation.position;
        if (gameController.reviveWindowTimer > -50f) {
            gameController.ClearReviveWindowTimer();
        }
        //weaponScript.DrawWeapon(1);
        StartCoroutine(SpawnInvincibilityRoutine());
    }

    [PunRPC]
    void RpcPopFlare(int index) {
        if (gameObject.layer == 0) return;
        for (int i = 0; i < gameController.items.Length; i++) {
            FlareScript f = gameController.items[i].GetComponentInChildren<FlareScript>();
            if (f.flareId == index) {
                f.PopFlare();
                gameController.exitPoint = f.gameObject;
                HandlePopFlareForMission(gameController.currentMap, i);
                break;
            }
        }
    }

    [PunRPC]
    void RpcCarryNpc(int playerId) {
        if (gameObject.layer == 0) return;
        NpcScript n = gameController.vipRef.GetComponent<NpcScript>();
        n.ToggleIsCarrying(true, playerId);
        // If is local player, set to is carrying
        if (playerId == PhotonNetwork.LocalPlayer.ActorNumber) {
            objectCarrying = gameController.vipRef;
            hud.SetCarryingText("PERSON");
        }
    }

    [PunRPC]
    void RpcDropOffNpc() {
        if (gameObject.layer == 0) return;
        NpcScript n = gameController.vipRef.GetComponent<NpcScript>();
        int droppedOffBy = n.carriedByPlayerId;
        n.ToggleIsCarrying(false, -1);
        if (droppedOffBy == pView.Owner.ActorNumber) {
            objectCarrying = null;
        }
    }

    void HandlePopFlareForMission(int mission, int i) {
        if (mission == 2) {
            gameController.UpdateObjectives();
            hud.UpdateObjectives();
            gameController.objectives.missionTimer2 = 120f;
            gameController.objectives.selectedEvacIndex = i;
            foreach (GameObject o in gameController.items) {
                FlareScript s = o.GetComponentInChildren<FlareScript>();
                if (s.flareId != i) {
                    s.gameObject.SetActive(false);
                }
            }
            hud.ComBoxPopup(1f, "Democko", "We see you! We’re incoming!", "HUD/democko");
            gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().ToggleEnabled(true, false);
            Vector3 n = new Vector3(gameController.exitPoint.transform.position.x, gameController.exitPoint.transform.position.y + 30f, gameController.exitPoint.transform.position.z - 8f);
            Vector3 n2 = new Vector3(gameController.exitPoint.transform.position.x, gameController.exitPoint.transform.position.y + 1.35f, gameController.exitPoint.transform.position.z - 8f);
            gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().SetDestination(n, false, 110f, FlightMode.Travel);
            gameController.escapeVehicleRef.GetComponent<BlackHawkScript>().SetDestination(n2, false, 5f, FlightMode.Descend);
        }
    }

    [PunRPC]
    void RpcDefuseBomb(int index)
    {
        if (gameObject.layer == 0) return;
        for (int i = 0; i < gameController.items.Length; i++) {
            BombScript b = gameController.items[i].GetComponent<BombScript>();
            if (b.bombId == index) {
                b.Defuse();
                gameController.UpdateObjectives();
                hud.UpdateObjectives();
                break;
            }
        }
    }

    public void HandleGameOverBanner()
    {
        if (fpc.enabled)
        {
            EnterSpectatorMode();
            equipmentScript.ToggleFirstPersonBody(false);
        }
        if (thisSpectatorCam != null) {
            thisSpectatorCam.GetComponent<SpectatorScript>().GameOverCam();
        }
    }

    public void PlayHealParticleEffect() {
        healParticleEffect.Play();
    }

    public void PlayBoostParticleEffect(bool sendOverNetwork) {
        boostParticleEffect.Play();
        if (sendOverNetwork) {
            pView.RPC("RpcBoostParticleEffect", RpcTarget.Others);
        }
    }

    [PunRPC]
    void RpcBoostParticleEffect()
    {
        boostParticleEffect.Play();
    }

    [PunRPC]
    void RpcOvershieldRegenParticleEffect()
    {
        overshieldRecoverParticleEffect.Play();
    }

    public void ToggleJetpackParticleEffect(bool b)
    {
        if (b && jetpackParticleEffect.isPlaying) return;
        if (!b && !jetpackParticleEffect.IsAlive()) return;
        pView.RPC("RpcToggleJetpackParticleEffect", RpcTarget.All, b);
    }

    [PunRPC]
    void RpcToggleJetpackParticleEffect(bool b)
    {
        if (b) {
            jetpackParticleEffect.Play();
        } else {
            jetpackParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void InjectMedkit() {
        StartCoroutine(HealthBoostEffect());
    }

    public IEnumerator HealthBoostEffect(){
        float totalHealPortion = 0.6f;
        int healAmounts = 5;
        if (skillController.GetBoosterEffectLevel() == 1) {
            totalHealPortion *= 1f + SkillController.BOOSTER_LVL1_EFFECT;
        } else if (skillController.GetBoosterEffectLevel() == 2) {
            totalHealPortion = 1 + SkillController.BOOSTER_LVL2_EFFECT;
            healAmounts = 6;
        } else if (skillController.GetBoosterEffectLevel() == 3) {
            totalHealPortion = 1 + SkillController.BOOSTER_LVL3_EFFECT;
            healAmounts = 7;
        }
        int healthIncrement = (int)(playerScript.health * totalHealPortion / 5f);
        if (this.health < playerScript.health && this.health > 0 && fightingSpiritTimer <= 0f && lastStandTimer <= 0f) {
          for (int i = 0; i < healAmounts; i++) {
            int newHealth = 0;
            if (this.health + healthIncrement > playerScript.health){
              newHealth = playerScript.health;
            } else {
              newHealth = this.health + healthIncrement;
            }
            SetHealth(newHealth, true);
            yield return new WaitForSeconds(2);
          }
         } else {
           yield return null;
         }
    }

    public void InjectAdrenaphine() {
        float skillBoost = 1f;
        if (skillController.GetBoosterEffectLevel() == 1) {
            skillBoost += SkillController.BOOSTER_LVL1_EFFECT;
        } else if (skillController.GetBoosterEffectLevel() == 2) {
            skillBoost = SkillController.BOOSTER_LVL2_EFFECT;
        } else if (skillController.GetBoosterEffectLevel() == 3) {
            skillBoost = SkillController.BOOSTER_LVL3_EFFECT;
        }
        StartCoroutine(StaminaBoostEffect(10f * skillBoost, 1.5f * skillBoost));
    }

    public IEnumerator StaminaBoostEffect(float staminaBoost, float speedBoost){
        itemSpeedModifier = speedBoost;
        unlimitedStamina = true;

        yield return new WaitForSeconds(staminaBoost);
        unlimitedStamina = false;
        itemSpeedModifier = 1f;
    }

    public void updatePlayerSpeed(){
        originalSpeed = playerScript.speed;
        totalSpeedBoost = originalSpeed * skillController.GetSpeedBoost() * itemSpeedModifier * weaponSpeedModifier;
    }

    public int GetDetectionRate() {
        int baseDetection = playerScript.detection;
        return (int)Mathf.Clamp((((float)baseDetection * (1f - skillController.GetThisPlayerAvoidabilityBoost())) - skillController.GetDdosDetectionBoost()), MIN_DETECTION_LEVEL, MAX_DETECTION_LEVEL);
    }

    public void SetEnemySeenBy(int enemyPViewId) {
        if (enemySeenBy != null && enemySeenBy.pView.ViewID == enemyPViewId) return;
        pView.RPC("RpcSetEnemySeenBy", RpcTarget.All, enemyPViewId);
    }

    [PunRPC]
    void RpcSetEnemySeenBy(int enemyPViewId) {
        if (gameObject.layer == 0) return;
        GameObject enemyRef = gameController.enemyList[enemyPViewId];
        BetaEnemyScript b = enemyRef.GetComponent<BetaEnemyScript>();
        if (enemySeenBy == null || (b.suspicionMeter > detectionLevel)) {
            enemySeenBy = enemyRef.GetComponent<BetaEnemyScript>();
        }
    }

    public void ClearEnemySeenBy() {
        if (enemySeenBy == null) return;
        pView.RPC("RpcClearEnemySeenBy", RpcTarget.All);
    }

    [PunRPC]
    void RpcClearEnemySeenBy() {
        if (gameObject.layer == 0) return;
        enemySeenBy = null;
    }

    void UpdateDetectionLevel() {
        if (enemySeenBy == null) {
            detectionLevel = 0f;
        } else {
            detectionLevel = enemySeenBy.suspicionMeter;
            if (detectionLevel <= 0f) {
                ClearEnemySeenBy();
            }
        }
    }

    void UpdateDetectionHUD() {
        if (gameController.assaultMode) {
            hud.ToggleDetectionHUD(false);
            hud.SetDetectionMeter(0f);
            return;
        }

        if (detectionLevel > 0f) {
            // Show the detection meter if it isn't currently shown
            if (!hud.container.detectionMeter.enabled) {
                hud.ToggleDetectionHUD(true);
                audioController.PlayCautionSound();
            }
            // Update the detection meter
            float d = detectionLevel / 100f;
            hud.SetDetectionMeter(d);
            if (d >= 1f) {
                // Display the detected text
                if (!hud.container.detectionText.enabled) {
                    hud.ToggleDetectedText(true);
                    audioController.PlayDetectedSound();
                }
            }
        } else {
            // Hide the detection HUD
            hud.ToggleDetectionHUD(false);
            // Reset its value to 0
            hud.SetDetectionMeter(0f);
        }
    }

    public void DetermineFallDamage() {
        // if (godMode) {
        //     return;
        // }
        float totalFallDamage = 0f;
        //Debug.Log("Vert velocity was: " + verticalVelocityBeforeLanding);
        if (verticalVelocityBeforeLanding <= -MINIMUM_FALL_DMG_VELOCITY) {
            totalFallDamage = MINIMUM_FALL_DMG * Mathf.Pow(FALL_DMG_MULTIPLIER, Mathf.Abs(verticalVelocityBeforeLanding) / FALL_DMG_DIVISOR);
        }
        // Debug.Log("total fall damage: " + totalFallDamage);
        totalFallDamage = Mathf.Clamp(totalFallDamage, 0f, 100f);
        TakeDamage((int)(totalFallDamage * (1f - skillController.GetFallDamageReduction())), false, false, transform.position, 2, 0);
    }

    public void UpdateVerticalVelocityBeforeLanding() {
        //Debug.Log("current vert velocity: " + currentVerticalVelocity + ",vert velocity before land: " + verticalVelocityBeforeLanding);
        verticalVelocityBeforeLanding = charController.velocity.y;
        //Debug.Log("v: " + verticalVelocityBeforeLanding);
    }

    public void ResetVerticalVelocityBeforeLanding() {
        verticalVelocityBeforeLanding = 0f;
    }

    void MarkEnemy() {
        if (!isInteracting && !gameController.assaultMode) {
            RaycastHit hit;
            if (Physics.SphereCast(wepActionScript.fpcShootPoint.position, 3f, wepActionScript.fpcShootPoint.transform.forward, out hit, Mathf.Infinity)) {
                if (hit.transform.gameObject.layer == ENEMY_LAYER) {
                    BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                    if (b != null) {
                        b.MarkEnemyOutline(skillController.GetKeenEyesMultiplier());
                    }
                }
            }
        }
    }

    IEnumerator SpawnInvincibilityRoutine() {
        spawnInvincibilityActive = true;
        yield return new WaitForSeconds(8f);
        spawnInvincibilityActive = false;
    }

    // Disables player in current scene since they shouldn't exist in this scene
    void DisablePlayerForVersus()
    {
        isNotOnTeamMap = true;
        gameObject.tag = "Untagged";
        gameObject.layer = 0;

        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            objectsToDisable[i].SetActive(false);
        }

        equipmentScript.enabled = false;
        weaponScript.enabled = false;
        playerScript.enabled = false;
        charController.enabled = false;
        aud.enabled = false;
        fpc.enabled = false;
        wepActionScript.enabled = false;
        cameraShakeScript.enabled = false;
        audioController.enabled = false;
        inGameMessengerHud.enabled = false;
        hud.enabled = false;
        photonTransformView.enabled = false;
        GetComponent<Rigidbody>().detectCollisions = false;
        this.enabled = false;
    }

    void SetTeamHost(bool force = false) {
        string t = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"] + "Host";
        if (force) {
            Hashtable h = new Hashtable();
            h.Add(t, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        } else {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(t)) {
                Hashtable h = new Hashtable();
                h.Add(t, PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.CurrentRoom.SetCustomProperties(h);
            }
        }
    }

    void SetInteracting(bool b, string objectName) {
        isInteracting = b;
        interactingWith = objectName;
    }

    void DropCarrying() {
        if (objectCarrying != null) {
            pView.RPC("RpcDropOffNpc", RpcTarget.All);
        }
    }

    void TriggerPlayerDownAlert() {
        pView.RPC("RpcTriggerPlayerDownAlert", RpcTarget.Others, PhotonNetwork.NickName);
    }

    [PunRPC]
    void RpcTriggerPlayerDownAlert(string playerDownName) {
        if (gameObject.layer == 0) return;
        PlayerActionScript p = PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>();
        if (p.health > 0) {
            hud.MessagePopup(playerDownName + " is down!");
        }
    }

    void TriggerPlayerIncapacitatedAlert(string playerName)
    {
        PlayerActionScript p = PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>();
        if (p.health > 0) {
            hud.MessagePopup(playerName + " needs to be helped up!");
        }
    }

    void ResetEnvDamageTimer() {
		envDamageTimer = 0f;
	}

    void UpdateEnvDamageTimer() {
		if (envDamageTimer < ENV_DAMAGE_DELAY) {
			envDamageTimer += Time.deltaTime;
		}
	}

    void UpdateOvershieldRecovery()
    {
        if (overshieldRecoverTimer > 0f) {
            overshieldRecoverTimer -= Time.deltaTime;
            if (overshieldRecoverTimer <= 0f) {
                overshieldRecoverTimer = 0f;
                hud.ToggleOvershieldWarningFlash(false);
                audioController.PlayOvershieldWarningSound(false);
                audioController.PlayOvershieldRecoverSound(true);
                pView.RPC("RpcOvershieldRegenParticleEffect", RpcTarget.All);
            }
        }

        if (overshieldRecoverTimer <= 0f && overshield < skillController.GetOvershield()) {
            overshield += skillController.GetOvershield() * Time.deltaTime / 3f;
            if (overshield >= skillController.GetOvershield()) {
                SetOvershield(skillController.GetOvershield(), false);
                audioController.PlayOvershieldRecoverSound(false);
            }
        }
    }

    void UpdateUnderwaterTimer()
    {
        if (fpc.GetIsSwimming())
        {
            if (underwaterTimer > 0f) {
                underwaterTimer -= Time.deltaTime;
            } else {
                if (underwaterTakeDamageTimer <= 0f) {
                    TakeDamage(2, false, false, Vector3.zero, 2, 0);
                    underwaterTakeDamageTimer = 1.5f;
                } else {
                    underwaterTakeDamageTimer -= Time.deltaTime;
                }
            }
        } else {
            underwaterTimer = UNDERWATER_TIMER;
        }
    }

    void FallOffMapProtection() {
        if (transform.position.y <= gameController.outOfBoundsPoint.position.y) {
            transform.position = gameController.spawnLocation.position;
        }
    }

    bool IsInGame() {
        string thisScene = SceneManager.GetActiveScene().name;
        if (thisScene == "Title") {
            return false;
        }
        return true;
    }

    void SetPlayerDead() 
    {
        health = 0;
        activeCamoTimer = 0f;
        lastStandTimer = 0f;
        fightingSpiritTimer = 0f;
        ToggleActiveCamo(false, 0f);
        equipmentScript.ToggleFirstPersonBody(false);
        equipmentScript.ToggleFullBody(false);
        equipmentScript.ToggleMesh(false);
        SetInteracting(false, null);
        DropCarrying();
        fpc.enabled = false;
        if (pView.IsMine) {
            hud.SetCarryingText(null);
            if (!rotationSaved)
            {
                // if (escapeValueSent)
                // {
                //     gameController.ConvertCounts(1, -1);
                // }
                hud.ToggleHUD(false);
                hud.ToggleSpectatorMessage(true);
                rotationSaved = true;
            }
            EnterSpectatorMode();
        }
    }

    [PunRPC]
	void RpcAskServerForDataPlayer(bool init) {
        if (!pView.IsMine) return;
        int healthToSend = health;
        float overshieldToSend = overshield;
        string playersDead = (string)PhotonNetwork.CurrentRoom.CustomProperties["deads"];
        string[] playersDeadList = null;
        if (playersDead != null) {
            playersDeadList = playersDead.Split(',');
            foreach (string p in playersDeadList) {
                if (p == PhotonNetwork.LocalPlayer.NickName) {
                    healthToSend = 0;
                    break;
                }
            }
        }
        int joinMode = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
        bool waitForAccept = false;
        if (init && !gameController.isVersusHostForThisTeam() && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("waitPeriod")) {
            if (joinMode == 1) {
                waitForAccept = true;
            } else if (joinMode == 2) {
                if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(gameController.teamMap + "Assault")) {
                    waitForAccept = true;
                }
            }
        }
        float motivateDmg = -1f;
        string motivates = null;
        if (gameController.isVersusHostForThisTeam()) {
            motivateDmg = skillController.GetMyMotivateDamageBoost();
            motivates = skillController.SerializeMotivateBoosts();
        }
		pView.RPC("RpcSyncDataPlayer", RpcTarget.All, healthToSend, overshieldToSend, escapeValueSent, GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills, GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].deaths, escapeAvailablePopup, waitForAccept,
                    skillController.GetMyHackerBoost(), skillController.GetMyHeadstrongBoost(), skillController.GetMyResourcefulBoost(), skillController.GetMyInspireBoost(), skillController.GetMyAvoidabilityBoost(), skillController.GetMyIntimidationBoost(), skillController.GetMyProviderBoost(), skillController.GetMyDdosLevel(),
                    skillController.GetMyMartialArtsAttackBoost(), skillController.GetMyMartialArtsDefenseBoost(), skillController.GetMyFireteamBoost(), skillController.GetSilhouetteBoost(), skillController.GetRegeneratorLevel(), skillController.GetPainkillerLevel(),
                    motivateDmg, motivates, fightingSpiritTimer, lastStandTimer, activeCamo, activeCamoTimer, interactedOnById);
	}

	[PunRPC]
	void RpcSyncDataPlayer(int health, float overshield, bool escapeValueSent, int kills, int deaths, bool escapeAvailablePopup, bool waitForAccept,
        int myHackerBoost, float myHeadstrongBoost, float myResourcefulBoost, float myInspireBoost, float myAvoidabilityBoost, float myIntimidationBoost, int myProviderBoost, int myDdosLevel, float myMartialArtsAttackBoost, float myMartialArtsDefenseBoost,
        float myFireteamBoost, int silhouetteBoost, int regeneratorLevel, int painkillerLevel, float motivateDamageBoost, string serializedMotivateBoosts, float fightingSpiritTimer, float lastStandTimer, bool activeCamo, float activeCamoTimer, int interactedOnById) {
        this.health = health;
        this.overshield = overshield;
        this.fightingSpiritTimer = fightingSpiritTimer;
        if (fightingSpiritTimer > 0f) {
            this.health = 0;
        }
        this.lastStandTimer = lastStandTimer;
        if (lastStandTimer > 0f) {
            this.health = 0;
            fpc.SetIsIncapacitated(true);
            fpc.SetIncapacitatedInAnimator(true);
            charController.height = 0.85f;
            charController.center = new Vector3(0f, 0.27f, 0f);
        }
        this.interactedOnById = interactedOnById;
        this.escapeValueSent = escapeValueSent;
        GameControllerScript.playerList[pView.OwnerActorNr].kills = kills;
        GameControllerScript.playerList[pView.OwnerActorNr].deaths = deaths;
        this.escapeAvailablePopup = escapeAvailablePopup;

        // Sync skill boosts
        SkillController mySkillController = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>();
        if (skillController.GetThisPlayerHackerBoost() == 0) {
            skillController.SetThisPlayerHackerBoost(myHackerBoost);
            mySkillController.AddHackerBoost(myHackerBoost);
        }
        if (skillController.GetThisPlayerHeadstrongBoost() == 0f) {
            skillController.SetThisPlayerHeadstrongBoost(myHeadstrongBoost);
            mySkillController.AddHeadstrongBoost(myHeadstrongBoost);
        }
        if (skillController.GetThisPlayerResourcefulBoost() == 0f) {
            skillController.SetThisPlayerResourcefulBoost(myResourcefulBoost);
            mySkillController.AddResourcefulBoost(myResourcefulBoost);
        }
        if (skillController.GetThisPlayerInspireBoost() == 0f) {
            skillController.SetThisPlayerInspireBoost(myInspireBoost);
            mySkillController.AddInspireBoost(myInspireBoost);
        }
        if (skillController.GetThisPlayerIntimidationBoost() == 0f) {
            skillController.SetThisPlayerIntimidationBoost(myIntimidationBoost);
            mySkillController.AddIntimidationBoost(myIntimidationBoost);
        }
        if (skillController.GetThisPlayerProviderBoost() == 0) {
            skillController.SetThisPlayerProviderBoost(myProviderBoost);
            mySkillController.AddProviderBoost(myProviderBoost);
        }
        if (skillController.GetThisPlayerDdosLevel() == 0) {
            skillController.SetThisPlayerDdosLevel(myDdosLevel);
            mySkillController.AddDdosBoost(myDdosLevel);
        }
        if (skillController.GetThisPlayerFireteamBoost() == 0) {
            skillController.SetThisPlayerFireteamBoost(myFireteamBoost);
            mySkillController.AddFireteamBoost(myFireteamBoost);
        }
        if (skillController.GetThisPlayerMartialArtsAttackBoost() == 0f) {
            skillController.SetThisPlayerMartialArtsAttackBoost(myMartialArtsAttackBoost);
            skillController.SetThisPlayerMartialArtsDefenseBoost(myMartialArtsDefenseBoost);
            mySkillController.AddMartialArtsBoost(myMartialArtsAttackBoost, myMartialArtsDefenseBoost);
        }
        if (skillController.GetThisSilhouetteBoost() == 0) {
            skillController.SetThisSilhouetteBoost(silhouetteBoost);
        }
        if (skillController.GetThisRegeneratorLevel() == 0) {
            skillController.SetThisRegeneratorLevel(regeneratorLevel);
            if (regeneratorLevel > 0) {
                mySkillController.AddRegenerator(pView.Owner.ActorNumber);
            }
        }
        if (skillController.GetThisPainkillerLevel() == 0) {
            skillController.SetThisPainkillerLevel(painkillerLevel);
            if (painkillerLevel > 0) {
                mySkillController.AddPainkiller(pView.Owner.ActorNumber);
            }
        }
        if (skillController.GetThisPlayerAvoidabilityBoost() == 0f) {
            skillController.SetThisPlayerAvoidabilityBoost(myAvoidabilityBoost);
        }
        if (mySkillController.GetMyMotivateDamageBoost() == 0f && motivateDamageBoost != -1f) {
            try {
                ArrayList newMotivateBoosts = new ArrayList();
                string[] boosts = serializedMotivateBoosts.Split(',');
                foreach (string boost in boosts) {
                    string[] thisBoost = boost.Split('|');
                    int thisActorNo = int.Parse(thisBoost[0]);
                    float thisDmgBoost = float.Parse(thisBoost[1]);
                    SkillController.MotivateNode n = new SkillController.MotivateNode();
                    n.actorNo = thisActorNo;
                    n.damageBoost = thisDmgBoost;
                    newMotivateBoosts.Add(n);
                }
                mySkillController.SyncMotivateBoost(newMotivateBoosts, motivateDamageBoost);
            } catch (Exception e) {
                Debug.LogError("Exception caught while syncing motivate boosts: " + e.Message);
            }
        }

        this.activeCamo = activeCamo;
        this.activeCamoTimer = activeCamoTimer;
        if (this.activeCamo) {
            ToggleActiveCamoSync();
        }

        if (health <= 0 && fightingSpiritTimer <= 0f && lastStandTimer <= 0f) {
            SetPlayerDead();
        } else if (waitForAccept) {
            waitingOnAccept = true;
            SetPlayerDead();
            if (pView.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber) {
                gameController.PingMasterForAcceptance();
                hud.container.spectatorText.text = "PLEASE WAIT FOR THE HOST TO ACCEPT YOU INTO THE GAME.";
            }
        }
	}

    public void NetworkComBoxMessage(string speaker, string message, string picPath) {
        pView.RPC("RpcNetworkComBoxMessage", RpcTarget.All, speaker, message, picPath);
    }

    [PunRPC]
    void RpcNetworkComBoxMessage(string speaker, string message, string picPath) {
        PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().DisplayComBox(speaker, message, picPath);
    }

    bool PlayerStillInRoom(int actorNo)
    {
        foreach (Player p in PhotonNetwork.PlayerList) {
            if (p.ActorNumber == actorNo) return true;
        }
        return false;
    }

    public void SendVoiceCommand(char type, int i)
	{
		pView.RPC("RpcSendVoiceCommand", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, (int)type, i, (int)InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender, gameController.teamMap);
	}

	[PunRPC]
	void RpcSendVoiceCommand(string playerName, int type, int i, int gender, string team)
	{
		if (team != gameController.teamMap) return;
		char typeChar = (char)type;
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().PlayVoiceCommand(playerName, typeChar, i);
		PlayVoiceCommand(typeChar, i, (char)gender);
	}

    public void PlayVoiceCommand(char type, int i, char gender)
	{
		radioAud.clip = hud.GetVoiceCommandAudio(type, i, gender);
		radioAud.Play();
	}

    void ToggleRagdoll(bool b) {
		animator.enabled = !b;
		charController.enabled = !b;
		mainRigid.useGravity = !b;

		foreach (Rigidbody rb in ragdollBodies)
		{
			rb.isKinematic = !b;
			rb.useGravity = b;
		}

		headTransform.GetComponent<Collider>().enabled = b;
		torsoTransform.GetComponent<Collider>().enabled = b;
		leftArmTransform.GetComponent<Collider>().enabled = b;
		leftForeArmTransform.GetComponent<Collider>().enabled = b;
		rightArmTransform.GetComponent<Collider>().enabled = b;
		rightForeArmTransform.GetComponent<Collider>().enabled = b;
		pelvisTransform.GetComponent<Collider>().enabled = b;
		leftUpperLegTransform.GetComponent<Collider>().enabled = b;
		leftLowerLegTransform.GetComponent<Collider>().enabled = b;
		rightUpperLegTransform.GetComponent<Collider>().enabled = b;
		rightLowerLegTransform.GetComponent<Collider>().enabled = b;

        equipmentScript.ToggleUpdateWhenOffscreen(b);
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

    public void OnPlayerLeftMatch()
    {
        pView.RPC("RpcOnPlayerLeftMatch", RpcTarget.Others);
    }

    [PunRPC]
    void RpcOnPlayerLeftMatch()
    {
        if (gameObject.layer == 0) return;
        PlayerActionScript p = PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>();
        if (pView.Owner.ActorNumber == p.interactedOnById) {
            p.ToggleProceduralInfo(null, false, -1);
        }
        SkillController mySkillController = PlayerData.playerdata.inGamePlayerReference.GetComponent<SkillController>();
        float dmgRemoval = mySkillController.RemoveMotivateBoost(pView.Owner.ActorNumber);
        mySkillController.RemoveFromMotivateDamageBoost(dmgRemoval);
    }

    // Called when a player dies, leaves the game, or this player respawns
    void UpdateSpeedBoostFromSkills()
    {
        float skillSpeedBoost = skillController.HandleAllyDeath();
        StatBoosts newTotalStatBoosts = equipmentScript.CalculateStatBoostsWithCurrentEquips();
        playerScript.stats.setSpeed(newTotalStatBoosts.speedBoost + skillSpeedBoost);
        playerScript.updateStats();
    }

    void ToggleHumanCollision(bool b)
	{
		if (!b) {
			headTransform.gameObject.layer = 17;
			torsoTransform.gameObject.layer = 17;
			leftArmTransform.gameObject.layer = 17;
			leftForeArmTransform.gameObject.layer = 17;
			rightArmTransform.gameObject.layer = 17;
			rightForeArmTransform.gameObject.layer = 17;
			pelvisTransform.gameObject.layer = 17;
			leftUpperLegTransform.gameObject.layer = 17;
			leftLowerLegTransform.gameObject.layer = 17;
			rightUpperLegTransform.gameObject.layer = 17;
			rightLowerLegTransform.gameObject.layer = 17;
		} else {
			headTransform.gameObject.layer = 15;
			torsoTransform.gameObject.layer = 15;
			leftArmTransform.gameObject.layer = 15;
			leftForeArmTransform.gameObject.layer = 15;
			rightArmTransform.gameObject.layer = 15;
			rightForeArmTransform.gameObject.layer = 15;
			pelvisTransform.gameObject.layer = 15;
			leftUpperLegTransform.gameObject.layer = 15;
			leftLowerLegTransform.gameObject.layer = 15;
			rightUpperLegTransform.gameObject.layer = 15;
			rightLowerLegTransform.gameObject.layer = 15;
		}
	}

    IEnumerator DelayToggleRagdoll(float seconds, bool b)
    {
        yield return new WaitForSeconds(seconds);
        pView.RPC("RpcToggleRagdollPlayer", RpcTarget.All, b);
    }

    [PunRPC]
    void RpcToggleRagdollPlayer(bool b)
    {
        if (gameObject.layer == 0) return;
        ToggleRagdoll(b);
        ToggleHumanCollision(!b);
        if (b) {
            ApplyForceModifiers();
        }
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    void UpdateRegeneration()
    {
        if (skillController.RegenerationFlag()) {
            int lvl = skillController.GetRegenerationLevel();
            if (lvl == 1) {
                RegenerateHealth(2);
            } else if (lvl == 2) {
                RegenerateHealth(4);
            } else if (lvl == 3) {
                RegenerateHealth(6);
            } else if (lvl == 4) {
                RegenerateHealth(8);
            } else if (lvl == 5) {
                RegenerateHealth(10);
            }
            skillController.RegenerationReset();
        }
    }

    void RegenerateHealth(int health)
    {
        if (this.health < 100 && this.health > 0 && fightingSpiritTimer <= 0f && lastStandTimer <= 0f) {
            int newHealth = this.health + health;
            SetHealth(newHealth, false);
        }
    }

    public void ActivateSkill(int skill)
    {
        if (skill == 1) {
            if (skillController.ActivateFirmGrip()) {
                // Skill effect
                PlayBoostParticleEffect(true);
            }
        } else if (skill == 2) {
            if (skillController.ActivateRampage()) {
                // Skill effect
                PlayBoostParticleEffect(true);
            }
        } else if (skill == 3) {
            if (skillController.ActivateActiveCamouflage()) {
                ToggleActiveCamo(true, skillController.GetActiveCamoTime());
                PlayBoostParticleEffect(true);
            }
        } else if (skill == 4) {
            if (skillController.ActivateSnipersDel()) {
                // Skill effect
                PlayBoostParticleEffect(true);
            }
        } else if (skill == 5) {
            if (skillController.ActivateBulletStream()) {
                // Skill effect
                PlayBoostParticleEffect(true);
            }
        } else if (skill == 6) {
            if (skillController.HasSkill(6)) {
                weaponScript.DrawEcmFeedbackSkill();
            }
        } else if (skill == 7) {
            if (skillController.HasSkill(7)) {
                weaponScript.DrawBubbleShieldSkill();
            }
        } else if (skill == 8) {
            if (skillController.HasSkill(8)) {
                weaponScript.DrawInfraredScanSkill();
            }
        } else if (skill == 9) {
            if (skillController.CanCallGuardianAngel()) {
                hud.ActivateGuardianAngel();
            }
        }
    }

    IEnumerator RegeneratorRecover()
    {
        if (fightingSpiritTimer <= 0f && lastStandTimer <= 0f) {
            // For every regenerator on your team, determine if they're within proper range
            LinkedList<int>.Enumerator ids = skillController.regeneratorPlayerIds.GetEnumerator();
            try {
                while (ids.MoveNext()) {
                    int thisPlayerId = ids.Current;
                    GameObject regenerator = GameControllerScript.playerList[thisPlayerId].objRef;
                    SkillController regeneratorSkillController = regenerator.GetComponent<SkillController>();
                    if (Vector3.Distance(regenerator.transform.position, transform.position) <= SkillController.REGENERATOR_MAX_DISTANCE) {
                        regeneratorSkillController.ActivateRegenerator(true);
                        int recoverAmt = regeneratorSkillController.GetRegeneratorRecoveryAmount();
                        if (recoverAmt > 0 && health < 100 && health > 0) {
                            SetHealth(health + recoverAmt, true);
                        }
                    } else {
                        regeneratorSkillController.ActivateRegenerator(false);
                    }
                }
            } catch (Exception e) {
                Debug.LogError("Caught error in [RegeneratorRecover]: " + e.Message);
            }
        }
        yield return new WaitForSeconds(2f);
        StartCoroutine("RegeneratorRecover");
    }

    IEnumerator PainkillerCompound()
    {
        // For every painkiller on your team, determine if they're within proper range
        LinkedList<int>.Enumerator ids = skillController.painkillerPlayerIds.GetEnumerator();
        try {
            while (ids.MoveNext()) {
                int thisPlayerId = ids.Current;
                GameObject painkiller = GameControllerScript.playerList[thisPlayerId].objRef;
                SkillController painkillerSkillController = painkiller.GetComponent<SkillController>();
                if (Vector3.Distance(painkiller.transform.position, transform.position) <= (SkillController.REGENERATOR_MAX_DISTANCE + 5f)) {
                    painkillerSkillController.ActivatePainkiller(true);
                } else {
                    painkillerSkillController.ActivatePainkiller(false);
                }
            }
        } catch (Exception e) {
            Debug.LogError("Caught error in [PainkillerCompound]: " + e.Message);
        }
        yield return new WaitForSeconds(3f);
        StartCoroutine("PainkillerCompound");
    }

    void InitializeGuardianAngel()
    {
        string key = PhotonNetwork.LocalPlayer.NickName + "GA";
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key)) {
            Hashtable h = new Hashtable();
            h.Add(key, skillController.GetMaxGuardianAngels());
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        }
    }

    public int GetGuardianAngelsRemaining()
    {
        return Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.NickName + "GA"]);
    }

    public void CallGuardianAngel(int actorNo)
    {
        if (!skillController.CanCallGuardianAngel()) return;
        if (health <= 0 || fightingSpiritTimer > 0f || lastStandTimer > 0f) return;
        if (!GameControllerScript.playerList.ContainsKey(actorNo)) return;
        if (GameControllerScript.playerList[actorNo].objRef == null) return;
        PlayerActionScript thisPlayerActionScript = GameControllerScript.playerList[actorNo].objRef.GetComponent<PlayerActionScript>();
        if (thisPlayerActionScript.health > 0) return;
        thisPlayerActionScript.CallRevive(PhotonNetwork.LocalPlayer.NickName + " HAS REVIVED YOU!", true);
        hud.MessagePopup("YOU HAVE REVIVED " + GameControllerScript.playerList[actorNo].name);
        pView.RPC("RpcKillMyself", RpcTarget.All);
        int guardianAngelsRemaining = GetGuardianAngelsRemaining() - 1;
        Hashtable h = new Hashtable();
        h.Add(PhotonNetwork.LocalPlayer.NickName + "GA", guardianAngelsRemaining);
        PhotonNetwork.CurrentRoom.SetCustomProperties(h);
    }

    void Revive(string reason)
    {
        hud.MessagePopup(reason);
        BeginRespawn(true);
    }

    public void CallRevive(string reason, bool sendOverNetwork)
    {
        if (sendOverNetwork) {
            pView.RPC("RpcCallRevive", RpcTarget.All, reason);
        } else {
            Revive(reason);
        }
    }

    [PunRPC]
    void RpcCallRevive(string reason)
    {
        if (pView.IsMine) {
            Revive(reason);
        }
    }

    void ToggleProceduralInfo(string s, bool sendOverNetwork, int interactedOnById)
    {
        if (sendOverNetwork) {
            pView.RPC("RpcToggleProceduralInfo", RpcTarget.All, s, interactedOnById);
        } else {
            hud.SetProceduralInfo(s);
            this.interactedOnById = interactedOnById;
        }
    }

    [PunRPC]
    void RpcToggleProceduralInfo(string s, int interactedOnById)
    {
        if (pView.IsMine) {
            hud.SetProceduralInfo(s);
        }
        this.interactedOnById = interactedOnById;
    }

    IEnumerator UpdateContingencyTimeBoost()
    {
        UpdateSpeedBoostFromSkills();
        yield return new WaitForSeconds(4f);
        StartCoroutine("UpdateContingencyTimeBoost");
    }

    void ActivateFightingSpirit()
    {
        fightingSpiritTimer = skillController.GetFightingSpiritTime();
        if (fightingSpiritTimer > 0f) {
            hud.MessagePopup("Fighting Spirit!");
            PlayBoostParticleEffect(false);
            skipHitDir = true;
            pView.RPC("RpcActivateFightingSpirit", RpcTarget.Others, fightingSpiritTimer);
        }
    }

    void ActivateLastStand()
    {
        lastStandTimer = skillController.GetLastStandTime();
        if (lastStandTimer > 0f) {
            interactedOnById = -1;
            DropCarrying();
            // Set FPC controller to be in last stand mode
            charController.height = 0.85f;
            charController.center = new Vector3(0f, 0.27f, 0f);
            FpcCrouch('p');
            pView.RPC("RpcCrouch", RpcTarget.Others, 0.5f, 0.27f);
            fpc.SetIsIncapacitated(true);
            fpc.SetIncapacitatedInAnimator(true);
            hud.MessagePopup("Last Stand!");
            skipHitDir = true;
            pView.RPC("RpcActivateLastStand", RpcTarget.Others, lastStandTimer, PhotonNetwork.NickName);
        }
    }

    public void LastStandRevive()
    {
        pView.RPC("RpcLastStandRevive", RpcTarget.All);
        fpc.SetIncapacitatedInAnimator(false);
    }

    [PunRPC]
    void RpcLastStandRevive()
    {
        lastStandTimer = 0f;
        interactedOnById = -1;
        if (pView.IsMine) {
            SetHealth(80, false);
            FpcCrouch('s');
        }
        equipmentScript.fullBodyRef.transform.localPosition = Vector3.zero;
        charController.height = charHeightOriginal;
        charController.center = new Vector3(0f, charCenterYOriginal, 0f);
        fpc.SetIsIncapacitated(false);
        skipHitDir = false;
    }

    [PunRPC]
    void RpcActivateFightingSpirit(float t)
    {
        fightingSpiritTimer = t;
        PlayBoostParticleEffect(false);
    }

    [PunRPC]
    void RpcActivateLastStand(float t, string playerName)
    {
        lastStandTimer = t;
        equipmentScript.fullBodyRef.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        TriggerPlayerIncapacitatedAlert(playerName);
    }

    void DeactivateFightingSpirit()
    {
        pView.RPC("RpcDeactivateFightingSpirit", RpcTarget.All);
    }

    [PunRPC]
    void RpcDeactivateFightingSpirit()
    {
        fightingSpiritTimer = 0f;
    }

    void DeactivateLastStand()
    {
        FpcCrouch('s');
        pView.RPC("RpcDeactivateLastStand", RpcTarget.All);
    }

    [PunRPC]
    void RpcDeactivateLastStand()
    {
        lastStandTimer = 0f;
        equipmentScript.fullBodyRef.transform.localPosition = Vector3.zero;
    }

    void UpdateFightingSpirit()
    {
        if (fightingSpiritTimer > 0f) {
            fightingSpiritTimer -= Time.deltaTime;
            ResetHitTimer();
            health = (int)(100f * (fightingSpiritTimer / skillController.GetFightingSpiritTime()));
            if (health < 0) health = 0;
        }
    }

    void UpdateLastStand()
    {
        if (lastStandTimer > 0f) {
            ResetHitTimer();
            if (interactedOnById == -1) {
                lastStandTimer -= Time.deltaTime;
                health = (int)(100f * (lastStandTimer / skillController.GetLastStandTime()));
                if (health < 0) health = 0;
            }
        }
    }

    void UpdateActiveCamouflage()
    {
        if (activeCamoTimer > 0f) {
            activeCamoTimer -= Time.deltaTime;
            if (activeCamoTimer <= 0f) {
                activeCamoTimer = 0f;
                ToggleActiveCamo(false, 0f);
            }
        }
    }

    void ToggleActiveCamo(bool b, float duration)
    {
        if (b) {
            activeCamoTimer = duration;
            audioController.PlayCamouflageSound();
        } else {
            audioController.PlayCamouflageOffSound();
        }
        activeCamo = b;
        equipmentScript.CamouflageFpcMesh(b);
        weaponScript.CamouflageFpcMesh(b);
        pView.RPC("RpcToggleActiveCamo", RpcTarget.Others, b);
    }

    [PunRPC]
    void RpcToggleActiveCamo(bool b)
    {
        activeCamo = b;
        equipmentScript.CamouflageMesh(b);
        weaponScript.CamouflageMesh(b);
    }

    void ToggleActiveCamoSync()
    {
        equipmentScript.CamouflageMesh(true);
        weaponScript.CamouflageMesh(true);
    }

}
