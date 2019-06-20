using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActionScript : MonoBehaviourPunCallbacks
{
    // Object references
    public GameControllerScript gameController;
    public AudioControllerScript audioController;
    public CharacterController charController;
    public WeaponActionScript wepActionScript;
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

    // Player variables
    public int health;
    public float sprintTime;
    public bool godMode;
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
    private bool isDefusing;
    private float enterSpectatorModeTimer;
    private bool unlimitedStamina;
    private float originalSpeed;
    public float totalSpeedBoost;
    private float itemSpeedModifier;
    public float weaponSpeedModifier;

    // Game logic helper variables
    public FirstPersonController fpc;

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

    // Mission references
    private GameObject currentBomb;
    private int currentBombIndex;
    private int bombIterator;
    private float bombDefuseCounter = 0f;

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

        health = 100;
        kills = 0;
        deaths = 0;
        sprintTime = playerScript.stamina;

         currentBombIndex = 0;
         bombIterator = 0;

        // // If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
         if (!GetComponent<PhotonView>().IsMine)
         {
             Destroy(GetComponentInChildren<AudioListener>());
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
        /**if (Input.GetKeyDown (KeyCode.P)) {
            BeginRespawn ();
        }*/

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
                equipmentScript.ToggleEquipVisibility(true);
            }
        }
        else
        {
            BombCheck();
        }

        if (fpc.enabled && fpc.canMove)
        {
            Crouch();
        }
        DetermineEscaped();
        RespawnRoutine();


    }

    void AddMyselfToPlayerList()
    {
        GameControllerScript.playerList.Add(photonView.OwnerActorNr, gameObject);
        GameControllerScript.totalKills.Add(photonView.Owner.NickName, kills);
        GameControllerScript.totalDeaths.Add(photonView.Owner.NickName, deaths);
    }

    public void TakeDamage(int d)
    {
        // Calculate damage done including armor
        float damageReduction = playerScript.armor - 1f;
        d = Mathf.RoundToInt((float)d * (1f - damageReduction));

        // Send over network
        photonView.RPC("RpcTakeDamage", RpcTarget.All, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d)
    {
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

    public void Crouch()
    {
        bool originalCrouch = fpc.m_IsCrouching;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (charController.isGrounded) {
                fpc.m_IsCrouching = !fpc.m_IsCrouching;
            }
        }

        // Collect the original y position of the FPS controller since we're going to move it downwards to crouch
        if (fpc.m_IsCrouching) {
            charController.height = 1f;
            charController.center = new Vector3(0f, 0.54f, 0f);
        } else {
            charController.height = charHeightOriginal;
            charController.center = new Vector3(0f, charCenterYOriginal, 0f);
        }

        // Set the animation to crouching
        // animator.SetBool("Crouching", fpc.m_IsCrouching);
        fpc.SetCrouchingInAnimator(fpc.m_IsCrouching);

        // Network it
        // if (fpc.m_IsCrouching != originalCrouch)
        // {
        //     photonView.RPC("RpcCrouch", RpcTarget.Others, fpc.m_IsCrouching);
        // }
    }

    [PunRPC]
    public void RpcCrouch(bool crouch)
    {
        fpc.m_IsCrouching = crouch;
        float h = charHeightOriginal;
        //float viewH = fpcPositionYOriginal;
        //float speed = charController.velocity;

        // if (fpc.m_IsCrouching)
        // {
        //     h = charHeightOriginal * .65f;
        //     viewH = .55f;
        //     bodyScale = .7f;
        // }
        // else
        // {
        //     viewH = .8f;
        //     bodyScale = bodyScaleOriginal;
        // }

        float lastHeight = charController.height;
        // float lastCameraHeight = fpcPosition.position.y;
        // charController.height = Mathf.Lerp(charController.height, h, 10 * Time.deltaTime);
        // fpcPosition.localPosition = new Vector3(fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
        transform.position = new Vector3(transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
    }

    void DeathCheck()
    {
        if (health <= 0)
        {
            if (fpc.enabled) {
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
            // Check if the player is still near the bomb
            if (Vector3.Distance(gameObject.transform.position, currentBomb.transform.position) > 4.5f || currentBomb.GetComponent<BombScript>().defused)
            {
                currentBomb = null;
                hud.container.hintText.enabled = false;
                return;
            }

            if (Input.GetKey(KeyCode.E) && health > 0)
            {
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
            }
            else
            {
                // Enable movement again
                if (!fpc.canMove)
                {
                    fpc.canMove = true;
                    isDefusing = false;
                    hud.ToggleActionBar(false);
                    hud.container.defusingText.enabled = false;
                    hud.container.hintText.enabled = true;
                    bombDefuseCounter = 0f;
                }
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
        Debug.Log("Env obstruction: " + Physics.Linecast(a, b, ignoreLayers));
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
                TakeDamage(damageReceived);
                ResetHitTimer();
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
            other.gameObject.GetComponent<PickupScript>().PlayPickupSound();
            other.gameObject.GetComponent<PickupScript>().DestroyPickup();
        }
        else if (other.gameObject.tag.Equals("HealthBox"))
        {
            ResetHealTimer();
            photonView.RPC("RpcSetHealth", RpcTarget.All, 100);
            other.gameObject.GetComponent<PickupScript>().PlayPickupSound();
            other.gameObject.GetComponent<PickupScript>().DestroyPickup();
        }
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
        else
        {
            if (other.gameObject.tag.Equals("AmmoBox"))
            {
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
            else if (other.gameObject.tag.Equals("HealthBox"))
            {
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
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
        equipmentScript.DespawnPlayer();
        weaponScript.DespawnPlayer();
        hudMarker.enabled = status;
        if (photonView.IsMine)
        {
            charController.enabled = status;
            fpc.enabled = status;
            viewCam.GetComponent<AudioListener>().enabled = status;
            viewCam.enabled = status;
            wepActionScript.enabled = status;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        escapeValueSent = false;
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
        photonView.RPC("RpcSetHealth", RpcTarget.All, 100);
        viewCam.transform.SetParent(headTransform);
        hud.ToggleHUD(true);
        hud.ToggleSpectatorMessage(false);
        fpc.m_IsCrouching = false;
        fpc.m_IsWalking = true;
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
        equipmentScript.RespawnPlayer();
        weaponScript.RespawnPlayer();
        fpc.ResetAnimationState();

        // Send player back to spawn position, reset rotation, leave spectator mode
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.position = new Vector3(gameController.spawnLocation.position.x, gameController.spawnLocation.position.y, gameController.spawnLocation.position.z);
        LeaveSpectatorMode();
        wepActionScript.CockingAction();
    }

    [PunRPC]
    void RpcDefuseBomb(int index)
    {
        gameController.bombs[index].GetComponent<BombScript>().Defuse();
    }

    public void HandleGameOverBanner()
    {
        if (fpc.enabled)
        {
            EnterSpectatorMode();
        }
        thisSpectatorCam.GetComponent<SpectatorScript>().GameOverCam();
    }

    public void PlayHealParticleEffect() {
        healParticleEffect.Play();
    }

    public void PlayBoostParticleEffect() {
        boostParticleEffect.Play();
    }

    public IEnumerator addHealth(){
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


    public IEnumerator useStaminaBoost(float staminaBoost, float speedBoost){
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

}
