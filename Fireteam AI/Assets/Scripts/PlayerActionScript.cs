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
using Koobando.AntiCheat;

public class PlayerActionScript : MonoBehaviourPunCallbacks
{
    const float MAX_DETECTION_LEVEL = 100f;
    const float BASE_DETECTION_RATE = 20f;
    const float INTERACTION_DISTANCE = 4.5f;
    const float BOMB_DEFUSE_TIME = 8f;
    const float DEPLOY_USE_TIME = 3f;
    const float NPC_INTERACT_TIME = 5f;
    private const float ENV_DAMAGE_DELAY = 0.5f;

    // Object references
    public PhotonView pView;
    public GameControllerScript gameController;
    public AudioControllerScript audioController;
    public CharacterController charController;
    public WeaponActionScript wepActionScript;
    public CameraShakeScript cameraShakeScript;
    public PhotonTransformViewKoobando photonTransformView;
    public AudioSource aud;
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
    public SpriteRenderer hudMarker;
    public SpriteRenderer hudMarker2;
    public GameObject fpcBodyRef;
    public GameObject[] objectsToDisable;
    private BetaEnemyScript enemySeenBy;
    public AudioClip ammoPickupSound;
    public AudioClip healthPickupSound;
    public Transform carryingSlot;

    // Player variables
    public EncryptedInt health;
    public float sprintTime;
    private EncryptedBool spawnInvincibilityActive;
    // public bool godMode;
    public bool canShoot;
    private float charHeightOriginal;
    private float charCenterYOriginal;
    public bool escapeValueSent;
    private bool assaultModeChangedIndicator;
    public int kills;
    private int deaths;
    public bool isRespawning;
    public float respawnTimer;
    private bool escapeAvailablePopup;
    private bool isInteracting;
    private string interactingWith;
    private GameObject activeInteractable;
    private float interactionTimer;
    private bool interactionLock;
    private float enterSpectatorModeTimer;
    private bool unlimitedStamina;
    private EncryptedFloat originalSpeed;
    public EncryptedFloat totalSpeedBoost;
    private float itemSpeedModifier;
    public float weaponSpeedModifier;
    private float originalFpcBodyPosY;
    public float verticalVelocityBeforeLanding;
    private Rigidbody rBody;
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
    public float healTimer;
    public float boostTimer;
    private float envDamageTimer;
    public Vector3 hitLocation;
    public Transform headTransform;
    public Transform cameraParent;

    // Mission references
    private float detectionLevel;

    void Awake()
    {
        if (!IsInGame()) {
            gameObject.SetActive(false);
            return;
        }

        InitPlayer();
    }

    void InitPlayer() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
        if (pView.IsMine) {
            if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
                string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
                pView.RPC("RpcDisablePlayerForVersus", RpcTarget.AllBuffered, myTeam);
                SetTeamHost();
            }
        }
    }

    public void SyncDataOnJoin() {
        if (!PhotonNetwork.IsMasterClient && !pView.IsMine) {
            pView.RPC("RpcAskServerForData", RpcTarget.MasterClient);
        }
    }

    void Start()
    {
        // Load the in-game necessities
        //DontDestroyOnLoad(gameObject);
        //AddMyselfToPlayerList();

        // Setting original positions for returning from crouching
        charHeightOriginal = charController.height;
        charCenterYOriginal = charController.center.y;

        escapeValueSent = false;
        assaultModeChangedIndicator = false;
        SetInteracting(false, null);
        originalFpcBodyPosY = fpcBodyRef.transform.localPosition.y;

        health = 100;
        kills = 0;
        deaths = 0;
        detectionLevel = 0;
        sprintTime = playerScript.stamina;

        rBody = GetComponent<Rigidbody>();

        // // If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
         if (!pView.IsMine)
         {
             Destroy(viewCam.GetComponent<AudioReverbFilter>());
             Destroy(viewCam.GetComponent<AudioLowPassFilter>());
             Destroy(viewCam.GetComponent<AudioListener>());
             viewCam.enabled = false;
             //enabled = false;
             return;
         }

        // gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();

        // Initialize variables
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
        isRespawning = false;
        respawnTimer = 0f;
        escapeAvailablePopup = false;
        enterSpectatorModeTimer = 0f;
        unlimitedStamina = false;
        itemSpeedModifier = 1f;
        originalSpeed = playerScript.speed;
        totalSpeedBoost = originalSpeed;

        StartCoroutine(SpawnInvincibilityRoutine());
    }

    void Update()
    {
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

        UnlockInteractionLock();
        UpdateEnvDamageTimer();
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
            canShoot = false;
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

        DeathCheck();
        if (health <= 0)
        {
            hud.ToggleHintText(null);
            SetInteracting(false, null);
            if (!escapeValueSent)
            {
                escapeValueSent = true;
                gameController.IncrementDeathCount();
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
                BeginRespawn();
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
            if (h != assaultModeChangedIndicator)
            {
                assaultModeChangedIndicator = h;
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
                BeginRespawn();
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

    public void TakeDamage(int d, bool useArmor)
    {
        if (d <= 0) return;
        // Calculate damage done including armor
        if (useArmor) {
            float damageReduction = playerScript.armor - 1f;
            d = Mathf.RoundToInt((float)d * (1f - damageReduction));
        }

        // Send over network
        pView.RPC("RpcTakeDamage", RpcTarget.All, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d)
    {
        if (gameObject.layer == 0) return;
        ResetHitTimer();
        audioController.PlayGruntSound();
        if (pView.IsMine)
        {
            audioController.PlayHitSound();
        }
        // if (!godMode)
        // {
        //     health -= d;
        // }
        health -= d;
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
        if (PlayerPreferences.playerPreferences.KeyWasPressed("Crouch") && !fpc.m_IsRunning)
        {
            fpc.m_IsCrouching = !fpc.m_IsCrouching;
            if (!fpc.IsFullyMobile()) {
                fpc.m_IsCrouching = false;
            }
        }

        FpcCrouch(fpc.m_IsCrouching);
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

    void FpcCrouch(bool crouch) {
        if (crouch) {
            fpcBodyRef.transform.localPosition = new Vector3(fpcBodyRef.transform.localPosition.x, -0.5f, fpcBodyRef.transform.localPosition.z);
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
        if (health <= 0)
        {
            if (fpc.enabled) {
                equipmentScript.ToggleFirstPersonBody(false);
                equipmentScript.ToggleFullBody(true);
                equipmentScript.ToggleMesh(true);
                //weaponScript.SwitchWeaponToFullBody();
                fpc.SetIsDeadInAnimator(true);
                SetInteracting(false, null);
                DropCarrying();
                hud.SetCarryingText(null);
                TriggerPlayerDownAlert();
            }
            fpc.enabled = false;
            if (!rotationSaved)
            {
                if (escapeValueSent)
                {
                    gameController.ConvertCounts(1, -1);
                }
                hud.ToggleHUD(false);
                hud.ToggleSpectatorMessage(true);
                // deathCameraLerpPos = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y + 2.5f, headTransform.localPosition.z - 4.5f);
                enterSpectatorModeTimer = 6f;
                viewCam.transform.SetParent(transform);
                viewCam.fieldOfView = 60;
                rotationSaved = true;
                pView.RPC("RpcAddToTotalDeaths", RpcTarget.All);
            }

            deathCameraLerpPos = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y + 2.5f, headTransform.localPosition.z - 4.5f);
            DeathCameraEffect();
        }
    }

    [PunRPC]
    void RpcAddToTotalDeaths()
    {
        GameControllerScript.playerList[pView.Owner.ActorNumber].deaths++;
        if (gameObject.layer == 0) return;
        if (deaths != int.MaxValue) {
            deaths++;
        }
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
                interactionTimer += (Time.deltaTime / BOMB_DEFUSE_TIME);
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
                interactionTimer += (Time.deltaTime / DEPLOY_USE_TIME);
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
                interactionTimer += (Time.deltaTime / DEPLOY_USE_TIME);
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
                interactionTimer += (Time.deltaTime / NPC_INTERACT_TIME);
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
            }
        } else {
            fpc.canMove = true;
            if (!wepActionScript.deployInProgress) {
                hud.ToggleActionBar(false, null);
                hud.ToggleChatText(true);
            }
            interactionTimer = 0f;
        }
    }

    void InteractCheck() {
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
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0) {
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

    void PopFlareCheck() {
        if (gameController == null || gameController.objectives.selectedEvacIndex >= 0) {
            return;
        }

        // Is near and looking at a flare
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0) {
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

        if (activeInteractable != null && !hud.PauseIsActive() && health > 0) {
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
        if (activeInteractable != null && !hud.PauseIsActive() && health > 0) {
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
        int interactableMask = (1 << 18);
        if (Physics.Raycast(viewCam.transform.position, viewCam.transform.forward, out hit, INTERACTION_DISTANCE, interactableMask)) {
            activeInteractable = hit.transform.gameObject;
        } else {
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
    void RpcSetHealth(int h)
    {
        if (gameObject.layer == 0) return;
        health = h;
    }

    public void SetHitLocation(Vector3 pos)
    {
        hitLocation = pos;
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
                Weapon grenadeStats = InventoryScript.itemData.weaponCatalog[other.gameObject.GetComponent<WeaponMeta>().weaponName];
                int damageReceived = (int)(grenadeStats.damage * scale);

                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
                // Deal damage to the player
                TakeDamage(damageReceived, false);
                //ResetHitTimer();
                SetHitLocation(other.transform.position);
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
                Weapon launcherStats = InventoryScript.itemData.weaponCatalog[other.gameObject.GetComponent<WeaponMeta>().weaponName];
                int damageReceived = (int)(launcherStats.damage * scale);

                // Validate that this enemy has already been affected
                l.AddHitPlayer(pView.ViewID);
                // Deal damage to the player
                TakeDamage(damageReceived, false);
                //ResetHitTimer();
                SetHitLocation(other.transform.position);
            }
        }
    }

    void HandleEnvironmentEffects(Collider other) {
		if (health <= 0 || envDamageTimer < ENV_DAMAGE_DELAY) {
			return;
		}

		if (other.gameObject.tag.Equals("Fire")) {
			FireScript f = other.gameObject.GetComponent<FireScript>();
			int damageReceived = (int)(f.damage);
			TakeDamage(damageReceived, false);
			ResetEnvDamageTimer();
            SetHitLocation(other.transform.position);
		}
	}

    void HandlePickups(Collider other)
    {
        if (other.gameObject.tag.Equals("AmmoBox"))
        {
            aud.clip = ammoPickupSound;
            aud.Play();
            if (weaponScript.currentlyEquippedType == 1) {
                wepActionScript.totalAmmoLeft = wepActionScript.weaponStats.maxAmmo - wepActionScript.currentAmmo;
            } else {
                weaponScript.MaxRefillAmmoOnPrimary();
            }
            pView.RPC("RpcDestroyPickup", RpcTarget.All, other.gameObject.GetComponent<PickupScript>().pickupId, gameController.teamMap);
        }
        else if (other.gameObject.tag.Equals("HealthBox"))
        {
            aud.clip = healthPickupSound;
            aud.Play();
            ResetHealTimer();
            pView.RPC("RpcSetHealth", RpcTarget.All, 100);
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
            if (health <= 0) {
                return;
            }
            HandleExplosiveEffects(other);
            HandlePickups(other);
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
    }

    void OnTriggerStay(Collider other) {
        if (pView.IsMine)
        {
            if (health <= 0) {
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
        pView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, false);
        thisSpectatorCam = Instantiate(spectatorCam, Vector3.zero, Quaternion.Euler(Vector3.zero));
        thisSpectatorCam.transform.SetParent(null);
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
        }
        hudMarker.enabled = status;
        hudMarker2.enabled = status;
        charController.enabled = status;
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
        if (gameController.matchType == 'V' && onMyMap) {
            RemovePlayerAsHost(otherPlayer.ActorNumber);
            SetTeamHost();
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

    void BeginRespawn()
    {
        enterSpectatorModeTimer = 0f;
        if (health <= 0)
        {
            gameController.ConvertCounts(-1, 0);
            gameController.gameOver = false;
            // Flash the respawn time bar on the screen
            hud.RespawnBar();
            // Then, actually start the respawn process
            respawnTimer = 5f;
            isRespawning = true;
        } else {
            health = 100;
            pView.RPC("RpcSetHealth", RpcTarget.Others, 100);
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
    void Respawn()
    {
        health = 100;
        pView.RPC("RpcSetHealth", RpcTarget.Others, 100);
        viewCam.transform.SetParent(cameraParent);
        viewCam.transform.GetComponent<Camera>().fieldOfView = 60;
        hud.ToggleHUD(true);
        hud.ToggleSpectatorMessage(false);
        fpc.m_IsCrouching = false;
        fpc.m_IsWalking = false;
        FpcCrouch(false);
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
        SetInteracting(false, null);
        interactionTimer = 0f;
        wepActionScript.deployInProgress = false;
        wepActionScript.deployTimer = 0f;
        wepActionScript.totalAmmoLeft = wepActionScript.weaponStats.maxAmmo;
        wepActionScript.currentAmmo = wepActionScript.weaponStats.clipCapacity;
        equipmentScript.ToggleFullBody(false);
        equipmentScript.ToggleFirstPersonBody(true);
        equipmentScript.ToggleFpcMesh(true);
        //weaponScript.SwitchWeaponToFpcBody();
        equipmentScript.RespawnPlayer();
        weaponScript.RespawnPlayer();
        wepActionScript.ResetMyActionStates();
        fpc.ResetAnimationState();
        fpc.ResetFPCAnimator(weaponScript.currentlyEquippedType);

        // Send player back to spawn position, reset rotation, leave spectator mode
        //transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.position = gameController.spawnLocation.position;
        fpc.m_MouseLook.Init(fpc.charTransform, fpc.spineTransform, fpc.fpcTransformSpine, fpc.fpcTransformBody);
        LeaveSpectatorMode();
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
                    o.SetActive(false);
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

    public void PlayBoostParticleEffect() {
        boostParticleEffect.Play();
    }

    public void InjectMedkit() {
        StartCoroutine(HealthBoostEffect());
    }

    public IEnumerator HealthBoostEffect(){
        int healthIncrement = (int)(playerScript.health*.6f/5f);
        if (this.health < playerScript.health && this.health > 0){
          for (int i = 0; i < 5; i++) {
            if (this.health + healthIncrement > playerScript.health){
              this.health = playerScript.health;
            } else {
              this.health += healthIncrement;
            }
            pView.RPC("RpcSetHealth", RpcTarget.Others, this.health);
            yield return new WaitForSeconds(2);

          }

         } else {
           yield return null;
         }
    }

    public void InjectAdrenaphine() {
        StartCoroutine(StaminaBoostEffect(10f, 1.5f));
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
        totalSpeedBoost = originalSpeed * itemSpeedModifier * weaponSpeedModifier;
    }

    public float GetDetectionRate() {
        // TODO: Later this will need to be updated to account for armor and weapons carrying
        return BASE_DETECTION_RATE;
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
            float d = detectionLevel / MAX_DETECTION_LEVEL;
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
        if (verticalVelocityBeforeLanding <= -25f) {
            //totalFallDamage = 40f * (Mathf.Abs(verticalVelocityBeforeLanding) / 20f);
            totalFallDamage = 10f * Mathf.Pow(2, Mathf.Abs(verticalVelocityBeforeLanding) / 14f);
        }
        // Debug.Log("total fall damage: " + totalFallDamage);
        totalFallDamage = Mathf.Clamp(totalFallDamage, 0f, 100f);
        TakeDamage((int)totalFallDamage, false);
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
            if (Physics.Raycast(wepActionScript.fpcShootPoint.position, wepActionScript.fpcShootPoint.transform.forward, out hit, 300f)) {
                if (hit.transform.tag.Equals("Human")) {
                    BetaEnemyScript b = hit.transform.gameObject.GetComponent<BetaEnemyScript>();
                    if (b != null) {
                        b.MarkEnemyOutline();
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

    void SetTeamHost() {
        string t = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"] + "Host";
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(t)) {
            Hashtable h = new Hashtable();
            h.Add(t, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
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
        hud.MessagePopup(playerDownName + " is down!");
    }

    void ResetEnvDamageTimer() {
		envDamageTimer = 0f;
	}

    void UpdateEnvDamageTimer() {
		if (envDamageTimer < ENV_DAMAGE_DELAY) {
			envDamageTimer += Time.deltaTime;
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

    [PunRPC]
	void RpcAskServerForData() {
        int healthToSend = health;
		pView.RPC("RpcSyncData", RpcTarget.All, healthToSend, escapeValueSent, assaultModeChangedIndicator, kills, deaths, escapeAvailablePopup);
	}

	[PunRPC]
	void RpcSyncData(int health, bool escapeValueSent, bool assaultModeChangedIndicator, int kills, int deaths, bool escapeAvailablePopup) {
        this.health = health;
        this.escapeValueSent = escapeValueSent;
        this.assaultModeChangedIndicator = assaultModeChangedIndicator;
        this.kills = kills;
        this.deaths = deaths;
        this.escapeAvailablePopup = escapeAvailablePopup;
	}

}
