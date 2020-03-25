using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActionScript : MonoBehaviourPunCallbacks
{
    const float MAX_DETECTION_LEVEL = 100f;

    // Object references
    public GameControllerScript gameController;
    public AudioControllerScript audioController;
    public CharacterController charController;
    public WeaponActionScript wepActionScript;
    public CameraShakeScript cameraShakeScript;
    public PhotonTransformView photonTransformView;
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
    public GameObject fpcBodyRef;
    public GameObject[] objectsToDisable;

    // Player variables
    public int health;
    public float sprintTime;
    public bool godMode;
    public bool canShoot;
    private bool syncDetectionValuesSemiphore;
    private float charHeightOriginal;
    private float charCenterYOriginal;
    public bool escapeValueSent;
    private bool assaultModeChangedIndicator;
    public int kills;
    private int deaths;
    public bool isRespawning;
    public float respawnTimer;
    private bool escapeAvailablePopup;
    private bool isDefusing;
    private float enterSpectatorModeTimer;
    private bool unlimitedStamina;
    private float originalSpeed;
    public float totalSpeedBoost;
    private float itemSpeedModifier;
    public float weaponSpeedModifier;
    private float originalFpcBodyPosY;
    public float verticalVelocityBeforeLanding;
    private Rigidbody rBody;
    private bool onMyMap;

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
    public Vector3 hitLocation;
    public Transform headTransform;
    public Transform cameraParent;

    // Mission references
    private GameObject currentBomb;
    private int currentBombIndex;
    private int bombIterator;
    private float bombDefuseCounter = 0f;
    private float detectionLevel;
    private float detectionCoolDownDelay;
    private float increaseDetectionDelay;
    private bool detectionResetUnderway;

    void Awake()
    {
        if (photonView.IsMine) {
            if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
                string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
                photonView.RPC("RpcDisablePlayerForVersus", RpcTarget.AllBuffered, myTeam);
                SetTeamHost();
            }
        }
    }

    void Start()
    {
        // Load the in-game necessities
        DontDestroyOnLoad(gameObject);
        AddMyselfToPlayerList();

        // Setting original positions for returning from crouching
        charHeightOriginal = charController.height;
        charCenterYOriginal = charController.center.y;

         escapeValueSent = false;
         assaultModeChangedIndicator = false;
         isDefusing = false;
         syncDetectionValuesSemiphore = false;
         originalFpcBodyPosY = fpcBodyRef.transform.localPosition.y;

        health = 100;
        kills = 0;
        deaths = 0;
        detectionLevel = 0;
        detectionCoolDownDelay = 0f;
        increaseDetectionDelay = 0f;
        detectionResetUnderway = false;
        sprintTime = playerScript.stamina;

         currentBombIndex = 0;
         bombIterator = 0;
         rBody = GetComponent<Rigidbody>();

        // // If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
         if (!GetComponent<PhotonView>().IsMine)
         {
             Destroy(viewCam.GetComponent<AudioReverbFilter>());
             Destroy(viewCam.GetComponent<AudioLowPassFilter>());
             Destroy(viewCam.GetComponent<AudioListener>());
             viewCam.enabled = false;
             //enabled = false;
             return;
         }

        gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();

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

        if (!photonView.IsMine)
        {
            return;
        }

        updatePlayerSpeed();
        // Instant respawn hack
        // if (Input.GetKeyDown (KeyCode.P)) {
        //     BeginRespawn ();
        // }
        // Physics sky drop test hack
        if (Input.GetKeyDown(KeyCode.O)) {
            transform.position = new Vector3(transform.position.x, transform.position.y + 20f, transform.position.z);
        }

         if (enterSpectatorModeTimer > 0f)
         {
             enterSpectatorModeTimer -= Time.deltaTime;
             if (enterSpectatorModeTimer <= 0f)
             {
                 EnterSpectatorMode();
             }
         }

        if (gameController.sectorsCleared == 0 && gameController.bombsRemaining == 2)
        {
            gameController.sectorsCleared++;
            hud.OnScreenEffect("SECTOR CLEARED!", false);
            BeginRespawn();
        }

        if (gameController.bombsRemaining == 0 && !escapeAvailablePopup)
        {
            escapeAvailablePopup = true;
            hud.MessagePopup("Escape available! Head to the waypoint!");
            hud.ComBoxPopup(2f, "Well done. There's an extraction waiting for you on the top of the construction site. Democko signing out.");
        }

        // Update assault mode
        hud.UpdateAssaultModeIndHud(gameController.assaultMode);

        // On assault mode changed
        bool h = gameController.assaultMode;
        if (h != assaultModeChangedIndicator)
        {
            assaultModeChangedIndicator = h;
            hud.MessagePopup("Your cover is blown!");
            hud.ComBoxPopup(2f, "They know you're here! Slot the bastards!");
            hud.ComBoxPopup(20f, "Cicadas on the rooftops! Watch the rooftops!");
        }

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
            if (!isDefusing)
            {
                canShoot = true;
            }
            else
            {
                canShoot = false;
            }
        }

        if (!Input.GetKey(KeyCode.LeftShift) && fpc.sprintLock)
        {
            fpc.sprintLock = false;
        }

        DeathCheck();
        if (health <= 0)
        {
            if (!escapeValueSent)
            {
                escapeValueSent = true;
                gameController.IncrementDeathCount();
            }
        }
        else
        {
            BombCheck();
        }

        if (fpc.enabled && fpc.canMove && !hud.container.pauseMenuGUI.activeInHierarchy)
        {
            HandleCrouch();
            if (Input.GetKeyDown(KeyCode.F)) {
                MarkEnemy();
            }
        }
        
        DetermineEscaped();
        RespawnRoutine();

        DecreaseDetectionLevel();
        UpdateDetectionHUD();
    }

    void FixedUpdate() {
        if (!photonView.IsMine) {
            return;
        }
        if (!fpc.m_CharacterController.isGrounded) {
            UpdateVerticalVelocityBeforeLanding();
        }
    }

    void AddMyselfToPlayerList()
    {
        GameControllerScript.playerList.Add(photonView.OwnerActorNr, gameObject);
        GameControllerScript.totalKills.Add(photonView.Owner.NickName, kills);
        GameControllerScript.totalDeaths.Add(photonView.Owner.NickName, deaths);
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
        photonView.RPC("RpcTakeDamage", RpcTarget.All, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d)
    {
        if (gameObject.layer == 0) return;
        ResetHitTimer();
        audioController.PlayGruntSound();
        if (photonView.IsMine)
        {
            audioController.PlayHitSound();
        }
        if (!godMode)
        {
            health -= d;
        }
    }

    [PunRPC]
    void RpcDisablePlayerForVersus(string myTeam) {
        int isRedMap = 0;
        if (SceneManager.GetActiveScene().name.EndsWith("Red"))
        {
            isRedMap = 1;
        }
        else if (SceneManager.GetActiveScene().name.EndsWith("Blue"))
        {
            isRedMap = -1;
        }
        onMyMap = ((myTeam == "red" && isRedMap == 1) || (myTeam == "blue" && isRedMap == -1));
        if (!onMyMap)
        {
            DisablePlayerForVersus();
        }
    }

    public void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            fpc.m_IsCrouching = !fpc.m_IsCrouching;
        }

        FpcCrouch(fpc.m_IsCrouching);
        fpc.SetCrouchingInAnimator(fpc.m_IsCrouching);

        // Collect the original y position of the FPS controller since we're going to move it downwards to crouch
        if (fpc.m_IsCrouching) {
            charController.height = 1f;
            charController.center = new Vector3(0f, 0.54f, 0f);
            // Network it
            photonView.RPC("RpcCrouch", RpcTarget.Others, 1f, 0.54f);
        } else {
            charController.height = charHeightOriginal;
            charController.center = new Vector3(0f, charCenterYOriginal, 0f);
            // Network it
            photonView.RPC("RpcCrouch", RpcTarget.Others, charHeightOriginal, charCenterYOriginal);
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
                photonView.RPC("RpcAddToTotalDeaths", RpcTarget.All);
            }

            deathCameraLerpPos = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y + 2.5f, headTransform.localPosition.z - 4.5f);
            DeathCameraEffect();
        }
    }

    [PunRPC]
    void RpcAddToTotalDeaths()
    {
        if (gameObject.layer == 0) return;
        deaths++;
        GameControllerScript.totalDeaths[photonView.Owner.NickName]++;
    }

    // If map objective is defusing bombs, this method checks if the player is near any bombs
    void BombCheck()
    {
        if (gameController == null || gameController.bombs == null)
        {
            return;
        }

        if (!currentBomb) {
            bool found = false;
            int count = 0;
            foreach (GameObject i in gameController.bombs) {
                BombScript b = i.GetComponent<BombScript> ();
                if (!b.defused) {
                    if (Vector3.Distance (gameObject.transform.position, i.transform.position) <= 4.5f) {
                        currentBomb = i;
                        currentBombIndex = count;
                        found = true;
                        break;
                    }
                }
                count++;
            }
            if (!found) {
                currentBomb = null;
            }
        }

        if (!currentBomb)
        {
            // Enable movement again
            if (!fpc.canMove && !hud.container.inGameMessenger.inputText.enabled)
            {
                fpc.canMove = true;
                isDefusing = false;
                hud.ToggleActionBar(false);
                hud.container.defusingText.enabled = false;
                hud.container.hintText.enabled = false;
                bombDefuseCounter = 0f;
            }

            if (bombIterator >= gameController.bombs.Length)
            {
                currentBomb = null;
                bombIterator = 0;
            }
            BombScript b = gameController.bombs[bombIterator].GetComponent<BombScript>();
            if (!b.defused)
            {
                if (Vector3.Distance(gameObject.transform.position, gameController.bombs[bombIterator].transform.position) <= 4.5f)
                {
                    currentBombIndex = bombIterator;
                    currentBomb = gameController.bombs[currentBombIndex];
                    bombIterator = -1;
                }
            }
            bombIterator++;
        }

        if (currentBomb != null)
        {
            // Check if the player is still near the bomb or if it's already defused
            if (Vector3.Distance(gameObject.transform.position, currentBomb.transform.position) > 4.5f || currentBomb.GetComponent<BombScript>().defused)
            {
                currentBomb = null;
                hud.container.hintText.enabled = false;
                return;
            }

            if (health > 0 && !hud.container.pauseMenuGUI.activeInHierarchy) {
                if (Input.GetKey(KeyCode.F)) {
                    fpc.canMove = false;
                    isDefusing = true;
                    hud.container.hintText.enabled = false;
                    hud.ToggleActionBar(true);
                    hud.container.defusingText.enabled = true;
                    bombDefuseCounter += (Time.deltaTime / 8f);
                    hud.SetActionBarSlider(bombDefuseCounter);
                    if (bombDefuseCounter >= 1f)
                    {
                        bombDefuseCounter = 0f;

                        photonView.RPC("RpcDefuseBomb", RpcTarget.All, currentBombIndex);
                        gameController.DecrementBombsRemaining();
                        currentBomb = null;

                        hud.ToggleActionBar(false);
                        hud.container.defusingText.enabled = false;
                        hud.container.hintText.enabled = false;
                        // Enable movement again
                        fpc.canMove = true;
                        isDefusing = false;
                    }
                } else {
                    // Enable movement again
                    if (!fpc.canMove)
                    {
                        fpc.canMove = true;
                        isDefusing = false;
                        hud.ToggleActionBar(false);
                        hud.container.defusingText.enabled = false;
                        bombDefuseCounter = 0f;
                    }
                    hud.container.hintText.enabled = true;
                }
            } else {
                fpc.canMove = false;
                isDefusing = false;
                hud.ToggleActionBar(false);
                hud.container.defusingText.enabled = false;
                hud.container.hintText.enabled = false;
                bombDefuseCounter = 0f;
            }
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
        if (gameController.escapeAvailable)
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
		int ignoreLayers = (1 << 9) & (1 << 11) & (1 << 12) & (1 << 13) & (1 << 14) & (1 << 15);
        ignoreLayers = ~ignoreLayers;
		return Physics.Linecast(a, b, ignoreLayers);
	}

    void HandleExplosiveEffects(Collider other)
    {
        // Handle explosive damage
        if (other.gameObject.name.Contains("M67"))
        {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            // If a ray casted from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(photonView.ViewID))
            {
                // Determine how far from the explosion the enemy was
                float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
                float blastRadius = other.gameObject.GetComponent<ThrowableScript>().blastRadius;
                distanceFromGrenade = Mathf.Min(distanceFromGrenade, blastRadius);
                float scale = 1f - (distanceFromGrenade / blastRadius);

                // Scale damage done to enemy by the distance from the explosion
                WeaponStats grenadeStats = other.gameObject.GetComponent<WeaponStats>();
                int damageReceived = (int)(grenadeStats.damage * scale);

                // Validate that this enemy has already been affected
                t.AddHitPlayer(photonView.ViewID);
                // Deal damage to the player
                TakeDamage(damageReceived, false);
                //ResetHitTimer();
                SetHitLocation(other.transform.position);
            }
        }
        else if (other.gameObject.name.Contains("XM84"))
        {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(photonView.ViewID))
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
                t.AddHitPlayer(photonView.ViewID);

                totalDisorientationTime *= distanceMultiplier * rotationMultiplier;
                hud.FlashbangEffect(totalDisorientationTime);
                audioController.PlayFlashbangEarRingSound(totalDisorientationTime);
            }
        }
    }

    void HandlePickups(Collider other)
    {
        if (other.gameObject.tag.Equals("AmmoBox"))
        {
            wepActionScript.totalAmmoLeft = wepActionScript.GetWeaponStats().maxAmmo + (wepActionScript.GetWeaponStats().clipCapacity - wepActionScript.currentAmmo);
            photonView.RPC("RpcDestroyPickup", RpcTarget.All, other.gameObject.GetComponent<PickupScript>().pickupId, gameController.teamMap);
        }
        else if (other.gameObject.tag.Equals("HealthBox"))
        {
            ResetHealTimer();
            photonView.RPC("RpcSetHealth", RpcTarget.All, 100);
            photonView.RPC("RpcDestroyPickup", RpcTarget.All, other.gameObject.GetComponent<PickupScript>().pickupId, gameController.teamMap);
        }
    }

    [PunRPC]
    void RpcDestroyPickup(int pickupId, string team) {
        if (team != gameController.teamMap) return;
        GameObject o = gameController.GetPickup(pickupId);
        o.GetComponent<PickupScript>().PlayPickupSound();
        o.GetComponent<PickupScript>().DestroyPickup();
        gameController.DestroyPickup(pickupId);
    }

    void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
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

    void DeathCameraEffect()
    {
        deathCameraLerpVar += (Time.deltaTime / 4f);
        viewCam.transform.LookAt(headTransform);
        viewCam.transform.localPosition = Vector3.Lerp(viewCam.transform.localPosition, deathCameraLerpPos, deathCameraLerpVar);
    }

    void EnterSpectatorMode()
    {
        photonView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, false);
        thisSpectatorCam = Instantiate(spectatorCam, Vector3.zero, Quaternion.Euler(Vector3.zero));
        thisSpectatorCam.transform.SetParent(null);
    }

    void LeaveSpectatorMode()
    {
        Destroy(thisSpectatorCam);
        thisSpectatorCam = null;
        photonView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, true);
    }

    [PunRPC]
    void RpcChangePlayerDisableStatus(bool status)
    {
        if (gameObject.layer == 0) return;
        if (!status) {
            equipmentScript.DespawnPlayer();
            weaponScript.DespawnPlayer();
        }
        hudMarker.enabled = status;
        charController.enabled = status;
        if (photonView.IsMine)
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
            PhotonNetwork.CurrentRoom.CustomProperties.Remove(t);
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
        photonView.RPC("RpcSetHealth", RpcTarget.Others, 100);
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
        fraction = 0f;
        deathCameraLerpVar = 0f;
        rotationSaved = false;
        hitTimer = 1f;
        healTimer = 1f;
        boostTimer = 1f;
        currentBomb = null;
        bombDefuseCounter = 0f;
        wepActionScript.totalAmmoLeft = wepActionScript.GetWeaponStats().maxAmmo;
        wepActionScript.currentAmmo = wepActionScript.GetWeaponStats().clipCapacity;
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
        transform.position = new Vector3(gameController.spawnLocation.position.x, gameController.spawnLocation.position.y, gameController.spawnLocation.position.z);
        fpc.m_MouseLook.Init(fpc.charTransform, fpc.spineTransform, fpc.fpcTransformSpine, fpc.fpcTransformBody);
        LeaveSpectatorMode();
        //weaponScript.DrawWeapon(1);
        StartCoroutine(SpawnInvincibilityRoutine());
    }

    [PunRPC]
    void RpcDefuseBomb(int index)
    {
        if (gameObject.layer == 0) return;
        gameController.bombs[index].GetComponent<BombScript>().Defuse();
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

    public void IncreaseDetectionLevel() {
        if (gameController.assaultMode) return;

        bool somethingChanged = false;
        float previousDetectionLevel = detectionLevel;
        float previousDetectionCoolDownDelay = detectionCoolDownDelay;
        float previousIncreaseDetectionDelay = increaseDetectionDelay;

        if (increaseDetectionDelay > 0f) {
            increaseDetectionDelay -= Time.deltaTime;
            if (increaseDetectionDelay != previousIncreaseDetectionDelay) {
                somethingChanged = true;
            }
        }

        if (increaseDetectionDelay <= 0f) {
            // TODO: Eventually update this to increase detection level gradually based on distance, time, equipment, weapons
            detectionLevel = MAX_DETECTION_LEVEL;
            detectionCoolDownDelay = 4f;
            if ((previousDetectionLevel != detectionLevel) || (previousDetectionCoolDownDelay != detectionCoolDownDelay)) {
                somethingChanged = true;
            }

            if (detectionLevel == MAX_DETECTION_LEVEL) {
                increaseDetectionDelay = 6f;
                if (previousIncreaseDetectionDelay != increaseDetectionDelay) {
                    somethingChanged = true;
                }
            }
        }

        // Sync values to all other players
        if (somethingChanged && !syncDetectionValuesSemiphore) {
            StartCoroutine("SyncDetectionValuesProcessor");
        }
    }

    [PunRPC]
    void RpcSyncDetectionValues(float detectionLevel, float increaseDetectionDelay, float detectionCoolDownDelay) {
        if (gameObject.layer == 0) return;
        this.detectionLevel = detectionLevel;
        this.increaseDetectionDelay = increaseDetectionDelay;
        this.detectionCoolDownDelay = detectionCoolDownDelay;
    }

    IEnumerator SyncDetectionValuesProcessor() {
        syncDetectionValuesSemiphore = true;
        yield return new WaitForSeconds(0.5f);
        photonView.RPC("RpcSyncDetectionValues", RpcTarget.Others, detectionLevel, increaseDetectionDelay, detectionCoolDownDelay);
        syncDetectionValuesSemiphore = false;
    }

    public void DecreaseDetectionLevel() {
        if (detectionCoolDownDelay <= 0f) {
            if (detectionLevel > 0f) {
                float detectionDeduction = detectionLevel - (5 * Time.deltaTime);
                detectionLevel = (detectionDeduction < 0f ? 0f : detectionDeduction);
            }
        } else {
            detectionCoolDownDelay -= Time.deltaTime;
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
            hud.SetDetectionMeter(detectionLevel / MAX_DETECTION_LEVEL);
            if (detectionLevel == MAX_DETECTION_LEVEL) {
                // Display the detected text
                if (!hud.container.detectionText.enabled) {
                    hud.ToggleDetectedText(true);
                }
                // Begin the detection HUD reset if one currently hasn't begun
                if (!detectionResetUnderway) {
                    audioController.PlayAlertSound();
                    StartCoroutine("DetectionHUDReset");
                }
            }
        } else {
            // Hide the detection HUD
            //Debug.Log("hero");
            hud.ToggleDetectionHUD(false);
            // Reset its value to 0
            hud.SetDetectionMeter(0f);
        }
    }

    IEnumerator DetectionHUDReset() {
        detectionResetUnderway = true;
        yield return new WaitForSeconds(4f);
        detectionLevel = 0f;
        detectionResetUnderway = false;
    }

    public void DetermineFallDamage() {
        if (godMode) {
            return;
        }
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
        if (!isDefusing && !gameController.assaultMode) {
            RaycastHit hit;
            if (Physics.Raycast(wepActionScript.fpcShootPoint.position, wepActionScript.fpcShootPoint.transform.forward, out hit, 300f)) {
                if (hit.transform.tag.Equals("Human")) {
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().MarkEnemyOutline();
                }
            }
        }
    }

    IEnumerator SpawnInvincibilityRoutine() {
        godMode = true;
        yield return new WaitForSeconds(3f);
        godMode = false;
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
            PhotonNetwork.CurrentRoom.CustomProperties[t] = PhotonNetwork.LocalPlayer.ActorNumber;
        }
    }

}
