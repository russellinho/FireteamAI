using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;
using Koobando.AntiCheat;
using ExitGames.Client.Photon;

public class WeaponActionScript : MonoBehaviour, IOnEventCallback
{
    private const int WATER_LAYER = 4;
    private const byte LAUNCHER_SPAWN_CODE = 126;
    private const byte THROWABLE_SPAWN_CODE = 127;
    private const float SHELL_SPEED = 3f;
    private const float SHELL_TUMBLE = 4f;
    private const float DEPLOY_BASE_TIME = 4f;
    private const short DEPLOY_OFFSET = 2;
    private const float LUNGE_SPEED = 20f;
    private const float UNPAUSE_DELAY = 0.5f;
    public static int HEAD_TARGET = 1;
	public static int TORSO_TARGET = 2;
	public static int LEFT_ARM_TARGET = 3;
	public static int LEFT_FOREARM_TARGET = 4;
	public static int RIGHT_ARM_TARGET = 5;
	public static int RIGHT_FOREARM_TARGET = 6;
	public static int PELVIS_TARGET = 7;
	public static int LEFT_UPPER_LEG_TARGET = 8;
	public static int LEFT_LOWER_LEG_TARGET = 9;
	public static int RIGHT_UPPER_LEG_TARGET = 10;
	public static int RIGHT_LOWER_LEG_TARGET = 11;
    private const int FIRE_IGNORE_MASK = ~(1 << 13);

    public MouseLook mouseLook;
    public PlayerActionScript playerActionScript;
    public CameraShakeScript cameraShakeScript;
    public PlayerHUDScript hudScript;
    public AudioControllerScript audioController;
    public FirstPersonController fpc;
    public GameObject weaponHolder;
    public GameObject weaponHolderFpc;
    private AudioSource weaponSound;
    private AudioSource reloadSound;
    public Animator animator;
    public Animator animatorFpc;
    public WeaponMeta weaponMetaData;
    public WeaponMeta meleeMetaData;
    public Weapon weaponStats;
    public Weapon meleeStats;
    private WeaponMods weaponMods;
    public GameObject WaterBulletEffect;
    public GameObject BloodEffect;
    public GameObject BloodEffectHeadshot;
    public GameObject OvershieldHitEffect;

    // Projectile recoil constants
    public const float MAX_RECOIL_TIME = 1.4f;
    public const float RECOIL_ACCELERATION = 4.2f;
    public const float RECOIL_DECELERATION = 4.2f;
    private const float SWAY_ACCELERATION = 1.5f;

    // Projectile variables
    private EncryptedFloat spread = 0f;
    public EncryptedFloat maxSpread = 0f;
    public EncryptedFloat spreadAcceleration = 0f;
    public EncryptedFloat spreadDeceleration = 0f;
    private EncryptedFloat recoilTime = 0f;
    private EncryptedFloat swayGauge = 0f;
    private bool voidRecoilRecover = true;
    private bool throwGrenade;
    //private float recoilSlerp = 0f;

    public EncryptedInt totalAmmoLeft;
    public EncryptedInt currentAmmo;

    public Transform shootPoint;
    public Transform fpcShootPoint;
    public Vector3 meleeTargetPos = Vector3.negativeInfinity;
    private Vector3 meleeStartingPos;
    public bool isLunging = false;
    public bool isReloading = false;
    public bool isCocking = false;
    public bool isFiring = false;
    public bool isMeleeing = false;
    public bool isAiming = false;
    public bool isDrawing = false;
    public bool isWieldingThrowable = false;
    public bool isWieldingBooster = false;
    public bool isWieldingDeployable = false;
    public bool isCockingGrenade = false;
    public bool isUsingBooster = false;
    public bool isUsingDeployable = false;
    // Used for allowing arms to move during aim down sight movement
    private bool aimDownSightsLock;
    private float aimDownSightsTimer;
    public Vector3 currentAimDownSightPos;
    public Vector3 currentAimStableHandPos;
    
    private GameObject bloodEffect;

    public enum FireMode { Auto, Semi }
    public enum ShotMode { Single, Burst }
    public FireMode firingMode;
    private int currentFiringModeIndex;
    public ShotMode shotMode;
    private bool shootInput;
    private bool meleeInput;

    // Once it equals fireRate, it will allow us to shoot
    float fireTimer = 0.0f;

    // Aiming down sights
    public Transform camTransform;
    public Camera weaponCam;
    private Vector3 originalPosCam;
    private Vector3 originalPosCamSecondary;
    // Aiming speed
    public PhotonView pView;
    public Transform rightCollar;
    public Transform leftCollar;
    public Vector3 leftCollarCurrentPos;
    public Vector3 rightCollarCurrentPos;

    // Zoom variables
    private int zoom = 3;
    private int defaultFov = 60;
    // Other variables
    public DeployMeshScript deployPlanMesh;
    private Vector3 deployPos;
    private Quaternion deployRot;
    public float deployTimer;
    public bool deployInProgress;
    public bool switchWeaponBackToRight;
    private EncryptedInt headshotCount;
    // Timer that prevents player from accidentally firing right after unpausing
    private float unpauseDelay;

    // Use this for initialization
    private bool initialized;
    public float qq;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void Initialize()
    {
        deployTimer = 0f;
        aimDownSightsLock = false;
        aimDownSightsTimer = 0f;
        throwGrenade = false;
        if (pView != null && !pView.IsMine)
        {
            initialized = true;
            return;
        }
        currentAmmo = weaponStats.clipCapacity;

        originalPosCam = camTransform.localPosition;

        originalPosCamSecondary = new Vector3(-0.13f, 0.11f, 0.04f);

        mouseLook = fpc.m_MouseLook;

        // Create animation event for shotgun reload

        // CreateAnimEvents();
        initialized = true;
    }

    // void CreateAnimEvents() {
    //     foreach (AnimationClip a in animatorFpc.runtimeAnimatorController.animationClips) {
    //         if (a.name.Equals("Loading_R870")) {
    //             AnimationEvent ae = new AnimationEvent();
    //             ae.time = 0.7f;
    //             ae.functionName = "ReloadShotgun";
    //             a.AddEvent(ae);
    //         }
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        if (!initialized) {
            return;
        }
        if (pView != null && !pView.IsMine)
        {
            return;
        }

        mouseLook.UpdateFlinch();

        if (playerActionScript.health <= 0 || playerActionScript.gameController.gameOver) {
            hudScript.toggleSniperOverlay(false);
            hudScript.ToggleSightCrosshair(false);
            return;
        }

        if (!deployInProgress && deployPlanMesh != null) {
            DestroyDeployPlanMesh();
        }

        if (PlayerPreferences.playerPreferences.KeyWasPressed("FireMode"))
        {
            if (weaponStats.firingModes != null) {
                currentFiringModeIndex++;
                if (currentFiringModeIndex >= weaponStats.firingModes.Length) {
                    currentFiringModeIndex = 0;
                }
                firingMode = (FireMode)weaponStats.firingModes[currentFiringModeIndex];
                hudScript.SetFireMode(firingMode.ToString().ToUpper());
            }
        }

        meleeInput = PlayerPreferences.playerPreferences.KeyWasPressed("Melee");
        
        switch (firingMode)
        {
            case FireMode.Auto:
                shootInput = PlayerPreferences.playerPreferences.KeyWasPressed("Fire", true);
                break;
            case FireMode.Semi:
                shootInput = PlayerPreferences.playerPreferences.KeyWasPressed("Fire");
                break;
        }

        if (hudScript.container.pauseMenuGUI.pauseActive) {
            return;
        }

        HandleAttack();

        if (unpauseDelay > 0f) {
            unpauseDelay -= Time.deltaTime;
        }

        if (!playerActionScript.canShoot || fpc.GetIsSwimming() || isWieldingThrowable || isWieldingBooster || isWieldingDeployable)
        {
            return;
        }
        
        if (PlayerPreferences.playerPreferences.KeyWasPressed("Reload")) {
            if (CanInitiateReload())
            {
                ReloadAction();
            }
        }

        // Automatically reload if no ammo
        if (currentAmmo <= 0 && totalAmmoLeft > 0 && AutoReloadCheck() && playerActionScript.canShoot) {
            //Debug.Log("current ammo: " + currentAmmo + " isFiring: " + isFiring + " isReloading: " + isReloading);
            cameraShakeScript.SetShake(false);
            ReloadAction();
        }

        if (switchWeaponBackToRight) {
            switchWeaponBackToRight = false;
            playerActionScript.weaponScript.weaponHolderFpc.SwitchWeaponToRightHand();
        }

        AimDownSights();
    }

    bool AutoReloadCheck() {
        if (isDrawing || isFiring || isMeleeing || isReloading || isCockingGrenade || isUsingBooster || isUsingDeployable || deployInProgress || isCocking || (fpc.m_IsRunning && !playerActionScript.skillController.HasRunNGun())) {
            return false;
        }
        return true;
    }

    public void RefillFireTimer()
    {
        if (fireTimer < weaponStats.fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    void HandleAttack()
    {
         if (!pView.IsMine || playerActionScript.health <= 0)
         {
             return;
         }
         RefillFireTimer();
        //  if (animator.gameObject.activeSelf)
        //  {
        //      AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(1);
        //      isReloading = info.IsName("Reload") || info.IsName("ReloadCrouch");
        //  }
        // Shooting mechanics

        if (meleeInput) {
            Melee();
            return;
        }

        if (weaponStats.category.Equals("Explosive")) {
            FireGrenades();
            return;
        }
        if (weaponStats.category.Equals("Booster")) {
            FireBooster();
            return;
        }
        if (weaponStats.category.Equals("Etc")) {
            FireEtc();
            return;
        }
        if (weaponStats.category.Equals("Deployable")) {
            FireDeployable();
            return;
        }
        
        if (shootInput && !meleeInput && !isMeleeing && !isDrawing && !isReloading && playerActionScript.canShoot && !hudScript.container.pauseMenuGUI.pauseActive && unpauseDelay <= 0f)
        {
            if (currentAmmo > 0)
            {
                if (shotMode == ShotMode.Single) {
                    if (weaponStats.category.Equals("Launcher")) {
                        FireLauncher();
                    } else {
                        Fire();
                    }
                } else {
                    FireShotgun();
                }
                voidRecoilRecover = false;
            }
            // else if (totalAmmoLeft > 0)
            // {
            //     cameraShakeScript.SetShake(false);
            //     ReloadAction();
            // }
        }
        else
        {
            DecreaseSpread();
            // DecreaseRecoil();
            // UpdateRecoil(false);
            cameraShakeScript.SetShake(false);
            if (CrossPlatformInputManager.GetAxis ("Mouse X") == 0 && CrossPlatformInputManager.GetAxis ("Mouse Y") == 0 && !voidRecoilRecover) {
                UpdateRecoil (false);
                DecreaseRecoil ();
            } else {
                voidRecoilRecover = true;
                recoilTime = 0f;
            }
        }
    }

    public void SetSpread(float accuracy)
    {
        // Add accuracy boost from skills
        maxSpread = (1f - Mathf.Clamp((accuracy / 100f), 0f, 1f)) * (1f - playerActionScript.skillController.accuracyBoost) * (1f - playerActionScript.skillController.GetInspireBoost());
        spreadAcceleration = maxSpread;
        spreadDeceleration = maxSpread / 2f;
    }
    
    // If aim down sights lock is enabled, arms have free range movement apart from their animations
    void UpdateAimDownSightsArms() {
        if (aimDownSightsLock) {
            if (aimDownSightsTimer < 1f) {
                aimDownSightsTimer += (Time.deltaTime * weaponMetaData.aimDownSightSpeed);
            }
            // If going to center
            if (isAiming) {
                if (fpc.equipmentScript.GetGender() == 'M') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, currentAimStableHandPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, currentAimDownSightPos, aimDownSightsTimer);
                } else if (fpc.equipmentScript.GetGender() == 'F') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, currentAimStableHandPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, currentAimDownSightPos, aimDownSightsTimer);
                }
            // If coming back to normal
            } else {
                // If the player is back in the normal position, then disable the lock
                if (fpc.equipmentScript.GetGender() == 'M') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, weaponMetaData.defaultLeftCollarPosMale, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, weaponMetaData.defaultRightCollarPosMale, aimDownSightsTimer);
                    if (aimDownSightsLock && aimDownSightsTimer >= 1f) {
                        aimDownSightsLock = false;
                    }
                } else if (fpc.equipmentScript.GetGender() == 'F') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, weaponMetaData.defaultLeftCollarPosFemale, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, weaponMetaData.defaultRightCollarPosFemale, aimDownSightsTimer);
                    if (aimDownSightsLock && aimDownSightsTimer >= 1f) {
                        aimDownSightsLock = false;
                    }
                }
            }
        }
    }

    void LateUpdate() {
        if (!initialized) {
            return;
        }
        UpdateAimDownSightsArms();
        if (deployPlanMesh != null) {
            UpdateDeployPlanMesh();
        }
    }

    void ToggleSniper(bool b) {
        if (!weaponStats.isSniper) return;
        if (weaponMetaData.weaponParts[0].enabled == b) return;
        foreach (MeshRenderer weaponPart in weaponMetaData.weaponParts) {
            weaponPart.enabled = b;
        }
        if (weaponMetaData.suppressorSlot != null) {
            MeshRenderer suppressorRenderer = weaponMetaData.suppressorSlot.GetComponentInChildren<MeshRenderer>();
            if (suppressorRenderer != null) {
                suppressorRenderer.enabled = b;
            }
        }
    }

    bool IsPumpActionCocking() {
        if (isCocking) {
            if (fpc.fpcAnimator.GetBool("isShotgun")) {
                return true;
            }
        }
        return false;
    }

    bool IsBoltActionCocking() {
        if (isCocking) {
            if (fpc.fpcAnimator.GetBool("isBoltAction")) {
                return true;
            }
        }
        return false;
    }

    public void AimDownSights()
    {
        if (!playerActionScript.fpc.m_IsRunning)
        {
            // Logic for toggle aim rather than hold down aim
            /**if (PlayerPreferences.playerPreferences.KeyWasPressed("Aim") && !isReloading) {
                isAiming = !isAiming;
            }
            if (isAiming && !isReloading) {
                originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, aimPos, Time.deltaTime * aodSpeed);
            } else {
                originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, originalPos, Time.deltaTime * aodSpeed);
            }*/
            if (PlayerPreferences.playerPreferences.KeyWasPressed("Aim", true) && !isReloading && !IsPumpActionCocking() && !IsBoltActionCocking() && !isDrawing)
            {
                fpc.SetAiminginFPCAnimator(true);
                if (!isAiming) {
                    isAiming = true;
                    leftCollarCurrentPos = leftCollar.localPosition;
                    rightCollarCurrentPos = rightCollar.localPosition;
                    aimDownSightsTimer = 0f;
                }
                aimDownSightsLock = true;
                if (fpc.equipmentScript.GetGender() == 'M') {
                    // Conditional to display sniper reticle, zoom in, disable the rifle mesh, and lower sensitivity
                    if (aimDownSightsTimer >= 1f) {
                        if (weaponStats.isSniper) {
                            camTransform.GetComponent<Camera>().fieldOfView = zoom;
                            ToggleSniper(false);
                            mouseLook.XSensitivity = 0.25f;
                            mouseLook.YSensitivity = 0.25f;
                            fpc.equipmentScript.ToggleFpcMesh(false);
                            hudScript.toggleSniperOverlay(true);
                        } else {
                            hudScript.ToggleSightCrosshair(true);
                        }
                    }
                } else {
                    // Conditional to display sniper reticle, zoom in, disable the rifle mesh, and lower sensitivity
                    if (aimDownSightsTimer >= 1f) {
                        if (weaponStats.isSniper) {
                            camTransform.GetComponent<Camera>().fieldOfView = zoom;
                            ToggleSniper(false);
                            mouseLook.XSensitivity = 0.25f;
                            mouseLook.YSensitivity = 0.25f;
                            fpc.equipmentScript.ToggleFpcMesh(false);
                            hudScript.toggleSniperOverlay(true);
                        } else {
                            hudScript.ToggleSightCrosshair(true);
                        }
                    }
                }
                //camTransform.GetComponent<Camera>().nearClipPlane = weaponStats.aimDownSightClipping;
            }
            else
            {
                fpc.SetAiminginFPCAnimator(false);
                if (isAiming) {
                    isAiming = false;
                    leftCollarCurrentPos = leftCollar.localPosition;
                    rightCollarCurrentPos = rightCollar.localPosition;
                    aimDownSightsTimer = 0f;
                }

                // Sets everything back to default after zooming in with sniper rifle
                camTransform.GetComponent<Camera>().fieldOfView = defaultFov;
                // weaponHolderFpc.GetComponentInChildren<MeshRenderer>().enabled = true;
                ToggleSniper(true);
                mouseLook.XSensitivity = mouseLook.originalXSensitivity;
                mouseLook.YSensitivity = mouseLook.originalYSensitivity;
                //camTransform.GetComponent<Camera>().nearClipPlane = 0.05f;
                fpc.equipmentScript.ToggleFpcMesh(true);
                hudScript.toggleSniperOverlay(false);
                hudScript.ToggleSightCrosshair(false);
            }
        }
    }

    void AddToTotalKills() {
        playerActionScript.gameController.AddToTotalKills(PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void ExternalInstantiateHitmarker()
    {
        if (pView.IsMine) {
            hudScript.InstantiateHitmarker();
        }
    }

    public void ExternalRewardKill()
    {
        if (pView.IsMine) {
            BetaEnemyScript.NUMBER_KILLED++;
            RewardKill(false);
        }
    }

    // Increment kill count and display HUD popup for kill
    public void RewardKill(bool isHeadshot) {
        AddToTotalKills();
        if (isHeadshot) {
            hudScript.OnScreenEffect("HEADSHOT", true);
            headshotCount++;
        } else {
            hudScript.OnScreenEffect(GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills + " KILLS", true);
        }
        // Recover health from Blood Leech skill
        if (playerActionScript.health > 0 && playerActionScript.health < 100 && playerActionScript.fightingSpiritTimer <= 0f && playerActionScript.lastStandTimer <= 0f) {
            int lvl = playerActionScript.skillController.GetBloodLeechLevel();
            if (lvl == 1) {
                if (GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills % 10 == 0) {
                    playerActionScript.SetHealth(playerActionScript.health + 2, true);
                }
            } else if (lvl == 2) {
                if (GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills % 8 == 0) {
                    playerActionScript.SetHealth(playerActionScript.health + 3, true);
                }
            } else if (lvl == 3) {
                if (GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills % 6 == 0) {
                    playerActionScript.SetHealth(playerActionScript.health + 4, true);
                }
            } else if (lvl == 4) {
                if (GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills % 4 == 0) {
                    playerActionScript.SetHealth(playerActionScript.health + 5, true);
                }
            }
        }

        // Register kill for killstreak skills
        playerActionScript.skillController.RegisterKillForKillstreak(playerActionScript.lastStandTimer > 0f);
        if (playerActionScript.lastStandTimer > 0f && playerActionScript.skillController.CanSelfRevive()) {
            playerActionScript.LastStandRevive();
            playerActionScript.skillController.ResetResilienceKillCount();
        }
    }

    public void SetMouseDynamicsForMelee(bool b) {
        fpc.SetMouseDynamicsForMelee(b);
    }

    bool CanMelee() {
        if (!fpc.m_CharacterController.isGrounded || fpc.GetIsSwimming() || isCocking || isDrawing || isMeleeing || isFiring || isAiming || isCockingGrenade || deployInProgress || isUsingBooster || isUsingDeployable || hudScript.container.pauseMenuGUI.pauseActive || fpc.GetIsIncapacitated()) {
            return false;
        }
        return true;
    }

    public void UpdateMeleeDash() {
        Vector3 dashDir = meleeTargetPos - transform.position;
        fpc.DashMove(dashDir * LUNGE_SPEED);
    }

    public void EndMeleeDash() {
        fpc.EndDash();
    }

    // Initiates a melee attack
    void Melee() {
        if (!CanMelee())
        {
            return;
        }

        isMeleeing = true;
        int enemyMask = 1 << 14;
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, meleeStats.lungeRange * (1f + playerActionScript.skillController.GetMeleeLungeBoost()), enemyMask)) {
            // Dash/warp to the enemyTarget position
            meleeStartingPos = transform.position;
            meleeTargetPos = hit.transform.GetComponentInParent<BetaEnemyScript>().gameObject.transform.position;
            isLunging = true;
            // Lunge
            animatorFpc.Play("MeleeLunge");
        } else {
            // Slash
            isLunging = false;
            animatorFpc.Play("MeleeSwing");
        }
    }

    // Scans for melee
    public void DealMeleeDamage() {
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, meleeStats.range)) {
            if (hit.transform.tag.Equals("Human")) {
                // Determine whether you hit an NPC or enemy
                NpcScript n = hit.transform.gameObject.GetComponentInParent<NpcScript>();
                BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                if (n != null) {
                    int beforeHp = n.health;
                    if (beforeHp > 0) {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                        n.TakeDamage((int)meleeStats.damage, transform.position, 2, 0);
                        n.PlayGruntSound(playerActionScript.gameController.teamMap);
                    }
                }
                if (b != null) {
                    int beforeHp = b.health;
                    if (beforeHp > 0)
                    {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                        hudScript.InstantiateHitmarker();
                        // audioController.PlayHitmarkerSound();
                        float hitmanDamageBoost = 1f;
                        if (b.isOutlined) {
                            hitmanDamageBoost += playerActionScript.skillController.GetHitmanDamageBoost();
                        }
                        float bloodLustDamageBoost = 1f + playerActionScript.skillController.GetBloodLustDamageBoost();
                        float martialArtsDamageBoost = 1f + playerActionScript.skillController.GetMartialArtsAttackBoost();
                        float fireteamDamageBoost = 1f + playerActionScript.skillController.GetFireteamBoost(playerActionScript.gameController.GetAvgDistanceBetweenTeam());
                        float motivateDamageBoost = 1f + playerActionScript.skillController.GetMyMotivateDamageBoost();
                        b.TakeDamage((int)(meleeStats.damage * (1f + playerActionScript.skillController.GetMeleeDamageBoost()) * hitmanDamageBoost * bloodLustDamageBoost * martialArtsDamageBoost * fireteamDamageBoost * motivateDamageBoost), transform.position, 2, 0, playerActionScript.skillController.GetHealthDropChanceBoost(), playerActionScript.skillController.GetAmmoDropChanceBoost());
                        b.PlayGruntSound();
                        b.SetAlerted();
                        if (b.health <= 0 && beforeHp > 0)
                        {
                            BetaEnemyScript.NUMBER_KILLED++;
                            RewardKill(false);
                            audioController.PlayKillSound();
                        }
                    }
                }
            }
        }
    }

    // Comment
    public void Fire()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || isCocking || isDrawing || isMeleeing)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
        animatorFpc.Play("Firing");
        isFiring = true;
        weaponMetaData.weaponAnimator.Play("Fire");
        SpawnShellCasing();
        IncreaseSpread();
        IncreaseRecoil();
        UpdateRecoil(true);
        RaycastHit hit;
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);
        float zSpread = Random.Range(-spread, spread);
        Vector3 impactDir = new Vector3(fpcShootPoint.transform.forward.x + xSpread, fpcShootPoint.transform.forward.y + ySpread, fpcShootPoint.transform.forward.z + zSpread);
        if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range, FIRE_IGNORE_MASK))
        {
            if (hit.transform.tag.Equals("Human"))
            {
                NpcScript n = hit.transform.gameObject.GetComponentInParent<NpcScript>();
                BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                if (n != null) {
                    int bodyPartIdHit = hit.transform.gameObject.GetComponent<BodyPartId>().bodyPartId;
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, (bodyPartIdHit == HEAD_TARGET));
                    int beforeHp = n.health;
                    int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, bodyPartIdHit) - CalculateDamageDropoff(weaponStats.damage, Vector3.Distance(fpcShootPoint.position, hit.transform.position), weaponStats.range);
                    if (beforeHp > 0)
                    {
                        n.TakeDamage(thisDamageDealt, transform.position, 0, bodyPartIdHit);
                        n.PlayGruntSound(playerActionScript.gameController.teamMap);
                    }
                }
                if (b != null) {
                    int bodyPartIdHit = hit.transform.gameObject.GetComponent<BodyPartId>().bodyPartId;
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, (bodyPartIdHit == HEAD_TARGET));
                    int beforeHp = b.health;
                    int thisDamageDealt = 0;
                    if (playerActionScript.skillController.WasCriticalHit()) {
                        thisDamageDealt = 100;
                    } else {
                        thisDamageDealt = CalculateDamageDealt(weaponStats.damage, bodyPartIdHit) - CalculateDamageDropoff(weaponStats.damage, Vector3.Distance(fpcShootPoint.position, hit.transform.position), weaponStats.range);
                    }
                    if (beforeHp > 0)
                    {
                        hudScript.InstantiateHitmarker();
                        // audioController.PlayHitmarkerSound();
                        float sniperAmplificationBoost = 1f;
                        if (weaponStats.isSniper && hudScript.container.SniperOverlay.activeInHierarchy) {
                            sniperAmplificationBoost += playerActionScript.skillController.GetSniperAmplification();
                        }
                        float shootToKillBoost = 1f;
                        if (bodyPartIdHit != HEAD_TARGET && bodyPartIdHit != TORSO_TARGET && bodyPartIdHit != PELVIS_TARGET) {
                            shootToKillBoost += playerActionScript.skillController.GetShootToKillBoost();
                        }
                        float silentKillerBoost = 1f;
                        if (weaponMods.suppressorRef != null) {
                            silentKillerBoost += playerActionScript.skillController.GetSilentKillerBoost();
                        }
                        float hitmanDamageBoost = 1f;
                        if (b.isOutlined) {
                            hitmanDamageBoost += playerActionScript.skillController.GetHitmanDamageBoost();
                        }
                        float oneShotOneKillBoost = 1f;
                        if (playerActionScript.skillController.OneShotOneKillReady()) {
                            oneShotOneKillBoost += playerActionScript.skillController.GetOneShotOneKillDamageBoost();
                            playerActionScript.skillController.ResetOneShotOneKillTimer();
                        }
                        float bloodLustDamageBoost = 1f + playerActionScript.skillController.GetBloodLustDamageBoost();
                        float fireteamDamageBoost = 1f + playerActionScript.skillController.GetFireteamBoost(playerActionScript.gameController.GetAvgDistanceBetweenTeam());
                        float motivateDamageBoost = 1f + playerActionScript.skillController.GetMyMotivateDamageBoost();
                        b.TakeDamage((int)(thisDamageDealt * playerActionScript.skillController.GetDamageBoost() * sniperAmplificationBoost * shootToKillBoost * silentKillerBoost * hitmanDamageBoost * oneShotOneKillBoost * bloodLustDamageBoost * fireteamDamageBoost * motivateDamageBoost), transform.position, 0, bodyPartIdHit, playerActionScript.skillController.GetHealthDropChanceBoost(), playerActionScript.skillController.GetAmmoDropChanceBoost());
                        int nanoparticulatesChance = playerActionScript.skillController.GetNanoparticulatesChanceBoost();
                        if (nanoparticulatesChance > 0) {
                            int r = Random.Range(0, 100);
                            if (r < nanoparticulatesChance) {
                                // Poison the enemy
                                b.SetPoisoned(PhotonNetwork.LocalPlayer.ActorNumber);
                            }
                        }
                        b.PlayGruntSound();
                        b.SetAlerted();
                        if (b.health <= 0 && beforeHp > 0)
                        {
                            BetaEnemyScript.NUMBER_KILLED++;
                            RewardKill(bodyPartIdHit == HEAD_TARGET);
                            if (bodyPartIdHit == HEAD_TARGET) {
                                audioController.PlayHeadshotSound();
                            } else {
                                audioController.PlayKillSound();
                            }
                        }
                    }
                }
            } else if (hit.transform.tag.Equals("Player")) {
                if (hit.transform != gameObject.transform) {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                }
            } else {
                if (hit.transform.gameObject.layer == WATER_LAYER) {
                    pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, -2, PhotonNetwork.LocalPlayer.ActorNumber);
                } else {
                    Terrain t = hit.transform.gameObject.GetComponent<Terrain>();
                    pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, (t == null ? -1 : t.index), PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }
        if (weaponMods.suppressorRef == null)
        {
            playerActionScript.gameController.SetLastGunshotHeardPos(false, transform.position);
            pView.RPC("FireEffects", RpcTarget.All, DetermineAmmoDeductSkip());
        }
        else
        {
            pView.RPC("FireEffectsSuppressed", RpcTarget.All, DetermineAmmoDeductSkip());
        }
    }

    public void FireLauncher() {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || isCocking || isDrawing || isMeleeing)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
        animatorFpc.Play("Firing");
        isFiring = true;
        weaponMetaData.weaponAnimator.Play("Fire");
        IncreaseRecoil();
        UpdateRecoil(true);
        pView.RPC("FireEffectsLauncher", RpcTarget.All);
        UseLauncherItem();
    }

    public void SpawnShellCasing() {
        GameObject o = Instantiate(weaponMetaData.weaponShell, weaponMetaData.weaponShellPoint.position, Quaternion.Euler(0f, 0f, 0f));
        o.transform.forward = -weaponMetaData.transform.right;
        o.GetComponent<Rigidbody>().velocity = weaponMetaData.transform.forward * SHELL_SPEED;
        o.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * SHELL_TUMBLE;
        Destroy(o, 3f);
        pView.RPC("RpcSpawnShellCasing", RpcTarget.Others);
    }

    [PunRPC]
    void RpcSpawnShellCasing() {
        if (gameObject.layer == 0) return;
        GameObject o = Instantiate(weaponMetaData.weaponShell, weaponMetaData.weaponShellPoint.position, Quaternion.Euler(-90f, -90f, 90f));
        o.transform.forward = -weaponMetaData.transform.right;
        o.GetComponent<Rigidbody>().velocity = weaponMetaData.transform.forward * SHELL_SPEED;
        o.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * SHELL_TUMBLE;
        Destroy(o, 3f);
    }

    public void FireShotgun ()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || (isCocking && isReloading) || isDrawing || isMeleeing)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
        animatorFpc.Play("Firing");
        isCocking = true;
        isFiring = true;
        IncreaseRecoil();
        UpdateRecoil(true);
        RaycastHit hit;
        // 8 shots for shotgun
        float totalDamageDealt = 0f;
        bool critialHit = playerActionScript.skillController.WasCriticalHit();
        bool headHit = false;
        for (int i = 0; i < 8; i++) {
            float xSpread = Random.Range(-maxSpread, maxSpread);
            float ySpread = Random.Range(-maxSpread, maxSpread);
            float zSpread = Random.Range(-maxSpread, maxSpread);
            Vector3 impactDir = new Vector3(fpcShootPoint.transform.forward.x + xSpread, fpcShootPoint.transform.forward.y + ySpread, fpcShootPoint.transform.forward.z + zSpread);
            if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range, FIRE_IGNORE_MASK))
            {
                // Debug.DrawRay(fpcShootPoint.position, impactDir, Color.blue, 10f, false);
                if (hit.transform.tag.Equals("Human"))
                {
                    BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                    NpcScript n = hit.transform.gameObject.GetComponentInParent<NpcScript>();
                    if (n != null) {
                        if (n.health > 0) {
                            int bodyPartIdHit = hit.transform.gameObject.GetComponent<BodyPartId>().bodyPartId;
                            if (bodyPartIdHit == HEAD_TARGET) {
                                headHit = true;
                            }
                            int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, bodyPartIdHit) - CalculateDamageDropoff(weaponStats.damage, Vector3.Distance(fpcShootPoint.position, hit.transform.position), weaponStats.range);
                            totalDamageDealt += thisDamageDealt;
                            if (i == 7) {
                                if (totalDamageDealt > 0)
                                {
                                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                                    n.PlayGruntSound(playerActionScript.gameController.teamMap);
                                    n.TakeDamage((int)totalDamageDealt, transform.position, 0, (headHit ? HEAD_TARGET : bodyPartIdHit));
                                }
                            }
                        }
                    }
                    if (b != null) {
                        if (b.health > 0) {
                            int bodyPartIdHit = hit.transform.gameObject.GetComponent<BodyPartId>().bodyPartId;
                            if (bodyPartIdHit == HEAD_TARGET) {
                                headHit = true;
                            }
                            int thisDamageDealt = 0;
                            if (critialHit) {
                                thisDamageDealt = 100;
                            } else {
                                thisDamageDealt = CalculateDamageDealt(weaponStats.damage, bodyPartIdHit) - CalculateDamageDropoff(weaponStats.damage, Vector3.Distance(fpcShootPoint.position, hit.transform.position), weaponStats.range);
                            }
                            totalDamageDealt += thisDamageDealt;
                            if (i == 7) {
                                if (totalDamageDealt > 0)
                                {
                                    hudScript.InstantiateHitmarker();
                                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, (bodyPartIdHit == HEAD_TARGET));
                                    b.PlayGruntSound();
                                    b.SetAlerted();
                                    float shootToKillBoost = 1f;
                                    if (bodyPartIdHit != HEAD_TARGET && bodyPartIdHit != TORSO_TARGET && bodyPartIdHit != PELVIS_TARGET) {
                                        shootToKillBoost += playerActionScript.skillController.GetShootToKillBoost();
                                    }
                                    float silentKillerBoost = 1f;
                                    if (weaponMods.suppressorRef != null) {
                                        silentKillerBoost += playerActionScript.skillController.GetSilentKillerBoost();
                                    }
                                    float hitmanDamageBoost = 1f;
                                    if (b.isOutlined) {
                                        hitmanDamageBoost += playerActionScript.skillController.GetHitmanDamageBoost();
                                    }
                                    float oneShotOneKillBoost = 1f;
                                    if (playerActionScript.skillController.OneShotOneKillReady()) {
                                        oneShotOneKillBoost += playerActionScript.skillController.GetOneShotOneKillDamageBoost();
                                        playerActionScript.skillController.ResetOneShotOneKillTimer();
                                    }
                                    float bloodLustDamageBoost = 1f + playerActionScript.skillController.GetBloodLustDamageBoost();
                                    float fireteamDamageBoost = 1f + playerActionScript.skillController.GetFireteamBoost(playerActionScript.gameController.GetAvgDistanceBetweenTeam());
                                    float motivateDamageBoost = 1f + playerActionScript.skillController.GetMyMotivateDamageBoost();
                                    b.TakeDamage((int)(totalDamageDealt * playerActionScript.skillController.GetDamageBoost() * shootToKillBoost * silentKillerBoost * hitmanDamageBoost * oneShotOneKillBoost * bloodLustDamageBoost * fireteamDamageBoost * motivateDamageBoost), transform.position, 0, (headHit ? HEAD_TARGET : bodyPartIdHit), playerActionScript.skillController.GetHealthDropChanceBoost(), playerActionScript.skillController.GetAmmoDropChanceBoost());
                                    int nanoparticulatesChance = playerActionScript.skillController.GetNanoparticulatesChanceBoost();
                                    if (nanoparticulatesChance > 0) {
                                        int r = Random.Range(0, 100);
                                        if (r < nanoparticulatesChance) {
                                            // Poison the enemy
                                            b.SetPoisoned(PhotonNetwork.LocalPlayer.ActorNumber);
                                        }
                                    }
                                    if (b.health <= 0)
                                    {
                                        BetaEnemyScript.NUMBER_KILLED++;
                                        RewardKill(headHit);
                                        if (headHit) {
                                            audioController.PlayHeadshotSound();
                                        } else {
                                            audioController.PlayKillSound();
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else if (hit.transform.tag.Equals("Player")) {
                    if (hit.transform != gameObject.transform) {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                    }
                } else {
                    if (hit.transform.gameObject.layer == WATER_LAYER) {
                        pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, -2, PhotonNetwork.LocalPlayer.ActorNumber);
                    } else {
                        Terrain t = hit.transform.gameObject.GetComponent<Terrain>();
                        pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, (t == null ? -1 : t.index), PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                }
            }
        }

        playerActionScript.gameController.SetLastGunshotHeardPos(false, transform.position);
        pView.RPC("FireEffects", RpcTarget.All, DetermineAmmoDeductSkip());
    }

    [PunRPC]
    void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal, bool headshot)
    {
        if (gameObject.layer == 0) return;
        GameObject bloodSpill;
        if (headshot)
        {
            bloodEffect = BloodEffectHeadshot;
        }
        else
        {
            bloodEffect = BloodEffect;
        }
        bloodSpill = Instantiate(bloodEffect, point, Quaternion.FromToRotation(Vector3.forward, normal));
        bloodSpill.transform.Rotate(180f, 0f, 0f);
        Destroy(bloodSpill, 1.5f);
    }

    [PunRPC]
    void RpcHandleBulletVfx(Vector3 point, Vector3 normal, int terrainId, int shooterActorNo) {
        if (gameObject.layer == 0) return;
        if (terrainId == -1) {
            GameObject bulletHoleEffect = Instantiate(OvershieldHitEffect, point, Quaternion.FromToRotation(Vector3.forward, normal));
			bulletHoleEffect.GetComponent<AudioSource>().Play();
			Destroy(bulletHoleEffect, 1.5f);
            return;
        }
        if (terrainId == -2) {
            Destroy(Instantiate(WaterBulletEffect, point, Quaternion.FromToRotation(Vector3.forward, normal)), 4f);
        } else {
            Terrain terrainHit = playerActionScript.gameController.terrainMetaData[terrainId];
            GameObject bulletHoleEffect = Instantiate(terrainHit.GetRandomBulletHole(), point, Quaternion.FromToRotation(Vector3.forward, normal));
            if (shooterActorNo == PhotonNetwork.LocalPlayer.ActorNumber) {
                bulletHoleEffect.GetComponent<BulletHoleScript>().skipRicochetAmbient = true;
            }
            bulletHoleEffect.transform.SetParent(terrainHit.gameObject.transform);
            Destroy(bulletHoleEffect, 4f);
        }
    }

    void InstantiateGunSmokeEffect(float duration) {
        if (weaponMetaData.gunSmoke != null) {
            GameObject gunSmokeEffect = null;
            //if (fpc.equipmentScript.isFirstPerson()) {
                //gunSmokeEffect = Instantiate(weaponStats.gunSmoke, weaponStats.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //} else {
                gunSmokeEffect = Instantiate(weaponMetaData.gunSmoke, weaponMetaData.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //}
            Destroy(gunSmokeEffect, duration);
        }
    }

    public int CalculateDamageDealt(float initialDamage, int bodyPartHit) {
        if (bodyPartHit == HEAD_TARGET) {
            return 100;
        } else if (bodyPartHit == TORSO_TARGET) {
            return (int)initialDamage;
        } else if (bodyPartHit == LEFT_ARM_TARGET) {
            return (int)(initialDamage / 2f);
        } else if (bodyPartHit == LEFT_FOREARM_TARGET) {
            return (int)(initialDamage / 3f);
        } else if (bodyPartHit == RIGHT_ARM_TARGET) {
            return (int)(initialDamage / 2f);
        } else if (bodyPartHit == RIGHT_FOREARM_TARGET) {
            return (int)(initialDamage / 3f);
        } else if (bodyPartHit == PELVIS_TARGET) {
            return (int)initialDamage;
        } else if (bodyPartHit == LEFT_UPPER_LEG_TARGET) {
            return (int)(initialDamage / 1.5f);
        } else if (bodyPartHit == LEFT_LOWER_LEG_TARGET) {
            return (int)(initialDamage / 2f);
        } else if (bodyPartHit == RIGHT_UPPER_LEG_TARGET) {
            return (int)(initialDamage / 1.5f);
        } else if (bodyPartHit == RIGHT_LOWER_LEG_TARGET) {
            return (int)(initialDamage / 2f);
        }
        return 0;
    }

    void PlayMuzzleFlash() {
        if (weaponMetaData.muzzleFlash != null) {
            weaponMetaData.muzzleFlash.Play();
        }
    }

    [PunRPC]
    void FireEffects(bool ammoDeductExempt)
    {
        if (gameObject.layer == 0) return;
        PlayMuzzleFlash();
        InstantiateGunSmokeEffect(1.5f);
        if (weaponMetaData.bulletTracer != null && !weaponMetaData.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponMetaData.bulletTracer.Play();
        }
        animator.SetTrigger("Fire");
        PlayShootSound();
        if (!ammoDeductExempt) {
            currentAmmo--;
        }
        playerActionScript.weaponScript.SyncAmmoCounts();
        // Reset fire timer
        fireTimer = 0.0f;
    }

    [PunRPC]
    void FireEffectsSuppressed(bool ammoDeductExempt)
    {
        if (gameObject.layer == 0) return;
        InstantiateGunSmokeEffect(1.5f);
        if (weaponMetaData.bulletTracer != null && !weaponMetaData.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponMetaData.bulletTracer.Play();
        }
        animator.SetTrigger("Fire");
        PlaySuppressedShootSound();
        if (!ammoDeductExempt) {
            currentAmmo--;
        }
        playerActionScript.weaponScript.SyncAmmoCounts();
        // Reset fire timer
        fireTimer = 0.0f;
    }

    [PunRPC]
    void FireEffectsLauncher()
    {
        if (gameObject.layer == 0) return;
        InstantiateGunSmokeEffect(3f);
        animator.SetTrigger("Fire");
        PlayShootSound();
        // UseLauncherItem();
        playerActionScript.weaponScript.SyncAmmoCounts();
        // Reset fire timer
        fireTimer = 0.0f;
    }

    public void Reload()
    {
        if (!isCocking)
        {
            if (totalAmmoLeft <= 0)
                return;

            int totalAmmoLeftDecrypt = totalAmmoLeft;
            int bulletsToLoad = weaponStats.clipCapacity - currentAmmo;
            int bulletsToDeduct = (totalAmmoLeftDecrypt >= bulletsToLoad) ? bulletsToLoad : totalAmmoLeftDecrypt;
            totalAmmoLeftDecrypt -= bulletsToDeduct;
            totalAmmoLeft = totalAmmoLeftDecrypt;
            currentAmmo += bulletsToDeduct;
            playerActionScript.weaponScript.SyncAmmoCounts();
        }
    }

    public void ReloadShotgun() {
        if (!isCocking)
        {
            if (totalAmmoLeft <= 0)
                return;

            totalAmmoLeft--;
            currentAmmo++;
            playerActionScript.weaponScript.SyncAmmoCounts();
        }
    }

    private void ReloadSupportItem() {
        if (totalAmmoLeft > 0) {
            totalAmmoLeft -= weaponStats.clipCapacity;
            currentAmmo = weaponStats.clipCapacity;
            playerActionScript.weaponScript.SyncAmmoCounts();
        }
    }

    private void ReloadAction()
    {
        //AnimatorStateInfo info = weaponAnimator.GetCurrentAnimatorStateInfo (0);
        if (isReloading)
            return;

        isReloading = true;
        if (isCocking && !fpc.fpcAnimator.GetBool("isShotgun"))
        {
            FpcCockingAnim();
            pView.RPC("RpcCockingAnim", RpcTarget.Others);
        }
        else
        {
            FpcReloadAnim();
            pView.RPC("RpcReloadAnim", RpcTarget.Others);
        }
    }

    [PunRPC]
    void RpcReloadAnim()
    {
        if (gameObject.layer == 0) return;
        // if (isCrouching) {
        //     //animator.CrossFadeInFixedTime("ReloadCrouch", 0.1f);
        //     animator.SetTrigger("Reloading");
        // } else {
            //animator.CrossFadeInFixedTime("Reload", 0.1f);
            animator.SetTrigger("Reload");
        // }
    }

    void FpcReloadAnim() {
        // if (fpc.m_IsCrouching) {
        //     //animator.CrossFadeInFixedTime("ReloadCrouch", 0.1f);
        //     animatorFpc.SetTrigger("Reloading");
        // } else {
            //animator.CrossFadeInFixedTime("Reload", 0.1f);
            if (weaponStats.category.Equals("Shotgun")) {
                animatorFpc.CrossFadeInFixedTime("ShotgunLoad", weaponMetaData.reloadTransitionSpeed);
            } else if (weaponStats.category.Equals("Sniper Rifle")) {
                animatorFpc.CrossFadeInFixedTime("BoltActionLoad", weaponMetaData.reloadTransitionSpeed);
            } else if (weaponStats.type.Equals("Support")) {
                animatorFpc.Play("DrawWeapon");
            } else {
                if (!animatorFpc.GetCurrentAnimatorStateInfo(0).IsName("Reload")) {
                    animatorFpc.CrossFadeInFixedTime("Reload", weaponMetaData.reloadTransitionSpeed);
                    FpcChangeMagazine(weaponMetaData.reloadTransitionSpeed);
                }
            }
            // animatorFpc.SetTrigger("Reload");
            // FpcChangeMagazine();
        // }
    }

    [PunRPC]
    void RpcCockingAnim()
    {
        if (gameObject.layer == 0) return;
        if (animator.GetBool("Crouching") == true)
        {
            animator.CrossFadeInFixedTime("ReloadCrouch", 0.1f, -1, 2.3f);
        }
        else
        {
            animator.CrossFadeInFixedTime("Reload", 0.1f, -1, 2.3f);
        }
    }

    void FpcCockingAnim() {
        if (weaponStats.category.Equals("Shotgun")) {
            animatorFpc.Play("ShotgunCock");
        }
    }

    public void FpcCockShotgun() {
        weaponMetaData.weaponAnimator.Play("Reload");
    }

    public void FpcCockBoltAction() {
        weaponMetaData.weaponAnimator.Play("Cock");
    }

    public void FpcLoadBoltAction() {
        weaponMetaData.weaponAnimator.Play("Reload");
    }

    public void FpcChangeMagazine(float startFrame) {
        //weaponStats.weaponAnimator.Play("Reload", 0, startFrame);
        weaponMetaData.weaponAnimator.CrossFadeInFixedTime("Reload", startFrame);
    }

    [PunRPC]
    void RpcPlayReloadSound(int soundNumber)
    {
        if (gameObject.layer == 0) return;
        weaponMetaData.weaponSoundSource.clip = weaponMetaData.reloadSounds[soundNumber];
        weaponMetaData.weaponSoundSource.Play();
    }

    public void PlayReloadSound(int soundNumber)
    {
        pView.RPC("RpcPlayReloadSound", RpcTarget.All, soundNumber);
    }

    [PunRPC]
    void RpcPlaySupportActionSound() {
        if (gameObject.layer == 0) return;
        weaponMetaData.weaponSoundSource.clip = weaponMetaData.supportActionSound;
        weaponMetaData.weaponSoundSource.Play();
    }

    public void PlaySupportActionSound() {
        pView.RPC("RpcPlaySupportActionSound", RpcTarget.All);
    }

    private void PlayShootSound()
    {
        weaponMetaData.fireSound.Play();
    }

    private void PlaySuppressedShootSound() {
        weaponMetaData.suppressedFireSound.Play();
    }

    private void IncreaseSpread()
    {
        if (spread < maxSpread)
        {
            spread += spreadAcceleration * Time.deltaTime;
            if (spread > maxSpread)
            {
                spread = maxSpread;
            }
        }
    }

    private void DecreaseSpread()
    {
        if (spread > 0f)
        {
            spread -= spreadDeceleration * Time.deltaTime;
            if (spread < 0f)
            {
                spread = 0f;
            }
        }
    }

    private void IncreaseRecoil()
    {
        // If the current camera rotation is not at its maximum recoil, then increase its recoil
        if (recoilTime < MAX_RECOIL_TIME)
        {
            recoilTime += RECOIL_ACCELERATION * Time.deltaTime;
        }
        if (swayGauge < weaponStats.sway) {
            swayGauge += SWAY_ACCELERATION * Time.deltaTime;
        }
        if (swayGauge > weaponStats.sway) {
            swayGauge = weaponStats.sway;
        }
    }

    private void DecreaseRecoil()
    {
        // If the current camera rotation is not at its original pos before recoil, then decrease its recoil
        if (recoilTime > 0f)
        {
            recoilTime -= (RECOIL_ACCELERATION / weaponMetaData.recoveryConstant) * Time.deltaTime;
        }
        swayGauge = 0f;
    }

    void UpdateRecoil(bool increase)
    {
        float totalRecoil = weaponStats.recoil * (1f - playerActionScript.skillController.recoilBoost) * (1f - playerActionScript.skillController.GetInspireBoost()) * (1f - playerActionScript.skillController.GetFirmGripBoost());
        if (increase)
        {
            // mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(weaponStats.recoil, 0f, 0f);
            // mouseLook.m_FpcCharacterHorizontalTargetRot *= Quaternion.Euler(0f, (Random.Range(0, 2) == 0 ? 1f : -1f) * swayGauge, 0f);
            mouseLook.SetRecoilInputs(totalRecoil, (Random.Range(0, 2) == 0 ? 1f : -1f) * swayGauge);
        }
        else
        {
            if (recoilTime > 0f)
            {
                // mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(-weaponStats.recoil / weaponStats.recoveryConstant, 0f, 0f);
                mouseLook.SetRecoilInputs(-totalRecoil / weaponMetaData.recoveryConstant, 0f);
            }
        }
    }

    public void SetCurrentAimDownSightPos(string sightName) {
        if (pView.IsMine) {
            if (fpc.equipmentScript.GetGender() == 'M') {
                currentAimDownSightPos = weaponMetaData.aimDownSightPosMale;
                currentAimStableHandPos = weaponMetaData.stableHandPosMale;
            } else if (fpc.equipmentScript.GetGender() == 'F') {
                currentAimDownSightPos = weaponMetaData.aimDownSightPosFemale;
                currentAimStableHandPos = weaponMetaData.stableHandPosFemale;
            }
            if (sightName != null && sightName != "") {
                int index = InventoryScript.itemData.modCatalog[sightName].modIndex;
                currentAimDownSightPos.y += weaponMetaData.crosshairAimOffset[index];
                currentAimStableHandPos.y += weaponMetaData.crosshairAimOffset[index];
            }
        }
    }

    public void SetWeaponStats(WeaponMeta ws, Weapon w) {
        if (w.type.Equals("Melee")) {
            meleeMetaData = ws;
            meleeStats = w;
            SetMeleeSpeed();
        } else {
            weaponMetaData = ws;
            weaponStats = w;
            weaponMods = ws.GetComponent<WeaponMods>();
            fireTimer = w.fireRate;
            firingMode = w.firingModes == null ? FireMode.Semi : (FireMode)w.firingModes[0];
            currentFiringModeIndex = 0;
            weaponCam.nearClipPlane = ws.aimDownSightClipping;
            playerActionScript.weaponSpeedModifier = w.mobility/100f;
            if (playerActionScript.equipmentScript.GetGender() == 'M') {
                fpc.fpcAnimator.runtimeAnimatorController = ws.maleOverrideController as RuntimeAnimatorController;
                animator.runtimeAnimatorController = ws.maleOverrideControllerFullBody as RuntimeAnimatorController;
            } else {
                fpc.fpcAnimator.runtimeAnimatorController = ws.femaleOverrideController as RuntimeAnimatorController;
                animator.runtimeAnimatorController = ws.femaleOverrideController as RuntimeAnimatorController;
            }
            if (!w.type.Equals("Support")) {
                SetReloadSpeed(playerActionScript.skillController.GetReloadSpeedBoostForCurrentWeapon(weaponStats));
                SetFiringSpeed();
            }
            if (weaponStats.type.Equals("Support")) {
                if (weaponStats.category.Equals("Explosive")) {
                    isWieldingThrowable = true;
                    isWieldingBooster = false;
                    isWieldingDeployable = false;
                } else if (weaponStats.category.Equals("Booster") || weaponStats.category.Equals("Etc")) {
                    isWieldingThrowable = false;
                    isWieldingBooster = true;
                    isWieldingDeployable = false;
                    SetFiringSpeed(playerActionScript.skillController.GetFiringSpeedBoostForCurrentWeapon(w));
                } else if (weaponStats.category.Equals("Deployable")) {
                    isWieldingThrowable = false;
                    isWieldingBooster = false;
                    isWieldingDeployable = true;
                    firingMode = FireMode.Auto;
                }
            } else {
                isWieldingThrowable = false;
                isWieldingBooster = false;
                isWieldingDeployable = false;
                if (weaponStats.category.Equals("Shotgun")) {
                    shotMode = ShotMode.Burst;
                } else {
                    shotMode = ShotMode.Single;
                }
            }

            SetFiringSpeedFullBody();

            if (pView.IsMine) {
                hudScript.SetFireMode(w.firingModes == null ? null : firingMode.ToString().ToUpper());
                hudScript.SetWeaponLabel();
            }
        }
    }

    public void SetReloadSpeed(float multipler = 0f) {
        animatorFpc.SetFloat("ReloadSpeed", weaponMetaData.defaultFpcReloadSpeed * (1f + multipler));
        animatorFpc.SetFloat("DrawSpeed", weaponMetaData.defaultWeaponDrawSpeed * (1f + multipler));
        weaponMetaData.weaponAnimator.SetFloat("ReloadSpeed", weaponMetaData.defaultWeaponReloadSpeed * (1f + multipler));
        weaponMetaData.weaponAnimator.SetFloat("CockingSpeed", weaponMetaData.defaultWeaponCockingSpeed);
    }

    public void SetFiringSpeed(float multiplier = 0f) {
        animatorFpc.SetFloat("FireSpeed", weaponMetaData.defaultFireSpeed + multiplier);
    }

    public void SetFiringSpeedFullBody() {
        animator.SetFloat("FireSpeed", weaponMetaData.defaultFireSpeedFullBody);
    }

    public void SetMeleeSpeed(float multiplier = 0f) {
        animatorFpc.SetFloat("MeleeSpeed", meleeMetaData.defaultMeleeSpeed + multiplier);
    }

    public void ModifyWeaponStats(float damage, float accuracy, float recoil, float range, int clipCapacity) {
        weaponStats.damage += damage;
        weaponStats.accuracy += accuracy;
        weaponStats.recoil += recoil;
        weaponStats.range += range;
        weaponStats.clipCapacity += clipCapacity;
    }

    public WeaponMeta GetWeaponMeta() {
        return weaponMetaData;
    }

    void FireGrenades() {
        if (fireTimer < weaponStats.fireRate)
        {
            ResetGrenadeState();
            return;
        }
        if (currentAmmo == 0) {
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) {
            ResetGrenadeState();
            return;
        }
        if (isCockingGrenade) {
            animatorFpc.SetTrigger("isCockingGrenade");
            // return;
        }
        if (isCockingGrenade && throwGrenade) {
            animatorFpc.SetTrigger("ThrowGrenade");
            pView.RPC("RpcUseBooster", RpcTarget.Others);
            throwGrenade = false;
        }
    }

    void FireBooster() {
        if (playerActionScript.fightingSpiritTimer > 0f || playerActionScript.lastStandTimer > 0f) return;
        if (fireTimer < weaponStats.fireRate || hudScript.container.pauseMenuGUI.pauseActive)
        {
            ResetBoosterState();
            return;
        }
        if (currentAmmo == 0) {
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) {
            ResetBoosterState();
            return;
        }
        // If using a medkit on max health, ignore the request
        if (weaponStats.name.Equals("Medkit") && playerActionScript.health == playerActionScript.playerScript.health) {
            return;
        }
        if (isWieldingBooster && PlayerPreferences.playerPreferences.KeyWasPressed("Fire")) {
            pView.RPC("RpcUseBooster", RpcTarget.Others);
            animatorFpc.SetTrigger("UseBooster");
            isUsingBooster = true;
        }
    }

    void FireEtc()
    {
        if (fireTimer < weaponStats.fireRate || hudScript.container.pauseMenuGUI.pauseActive)
        {
            ResetBoosterState();
            return;
        }
        if (currentAmmo == 0) {
            if (playerActionScript.weaponScript.currentlyEquippedType == -2 || playerActionScript.weaponScript.currentlyEquippedType == -3) {
                return;
            }
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) {
            ResetBoosterState();
            return;
        }
        if (isWieldingBooster && PlayerPreferences.playerPreferences.KeyWasPressed("Fire")) {
            pView.RPC("RpcUseBooster", RpcTarget.Others);
            animatorFpc.SetTrigger("UseBooster");
            isUsingBooster = true;
        }
    }

    void FireDeployable() {
        if (fireTimer < weaponStats.fireRate || hudScript.container.pauseMenuGUI.pauseActive)
        {
            ResetDeployableState();
            return;
        }
        if (currentAmmo == 0) {
            if (playerActionScript.weaponScript.currentlyEquippedType == -1) {
                return;
            }
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) {
            ResetDeployableState();
            return;
        }
        // Handle deployment time and initiating deployment
        if (isWieldingDeployable) {
            if (shootInput && !meleeInput && !isMeleeing && !isUsingDeployable) {
                // Charge up deploy gauge
                deployTimer += (Time.deltaTime / ((1f - playerActionScript.skillController.deploymentTimeBoost) * DEPLOY_BASE_TIME));
                if (!deployInProgress) {
                    InstantiateDeployPlanMesh();
                    hudScript.ToggleActionBar(true, "DEPLOYING...");
                }
                deployInProgress = true;
                hudScript.SetActionBarSlider(deployTimer);
                // Determine if the deploy position is valid or not. If it isn't valid,
                // then skip deployment and reset. Else,
                // Reset deploy time and set deploy position
                if (deployTimer >= 1f && deployInProgress) {
                    // If deploy position was valid, then deploy the item
                    if (DeployPositionIsValid()) {
                        deployPos = deployPlanMesh.gameObject.transform.position;
                        deployRot = deployPlanMesh.gameObject.transform.rotation;
                        isUsingDeployable = true;
                        deployInProgress = false;
                        pView.RPC("RpcUseDeployable", RpcTarget.Others);
                        UseDeployable();
                        if (currentAmmo <= 0) {
                            playerActionScript.weaponScript.HideWeapon(true);
                        }
                        animatorFpc.SetTrigger("UseDeployable");
                    } else {
                        // Else, reset the deploy timer
                        deployTimer = 0f;
                    }
                }
            } else {
                if (deployPlanMesh != null) {
                    DestroyDeployPlanMesh();
                }
                deployTimer = 0f;
                if (deployInProgress) {
                    hudScript.ToggleActionBar(false, null);
                    hudScript.ToggleDeployInvalidText(false);
                }
                deployInProgress = false;
            }
        }
    }

    public void UseSupportItem() {
        // If the item is a grenade, instantiate and launch the grenade
        if (weaponStats.category.Equals("Explosive")) {
            GameObject projectile = GameObject.Instantiate((GameObject)Resources.Load(InventoryScript.itemData.weaponCatalog[weaponStats.name].projectilePath), weaponHolderFpc.transform.position, Quaternion.identity);
            PhotonView thisPView = projectile.GetComponent<PhotonView>();
            if (PhotonNetwork.AllocateViewID(thisPView))
            {
                projectile.transform.forward = weaponHolderFpc.transform.forward;
                projectile.GetComponent<ThrowableScript>().Launch(pView.ViewID, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z, 1f + playerActionScript.skillController.throwForceBoost);
                currentAmmo--;
                playerActionScript.weaponScript.SyncAmmoCounts();
                fireTimer = 0.0f;
                SpawnThrowableItemOnOthers(projectile);
            }
            else
            {
                Debug.Log("Failed to allocate a ViewId for throwable.");
                Destroy(projectile);
            }
        } else if (weaponStats.category.Equals("Booster")) {
            // Reset fire timer and subtract ammo used
            BoosterScript boosterScript = weaponMetaData.GetComponentInChildren<BoosterScript>();
            boosterScript.UseBoosterItem(weaponStats.name, playerActionScript);
            currentAmmo--;
            playerActionScript.weaponScript.SyncAmmoCounts();
            fireTimer = 0.0f;
        } else if (weaponStats.category.Equals("Deployable")) {
            if (weaponStats.name.EndsWith("(Skill)")) {
                DeployDeployable(deployPos, deployRot, true);
                currentAmmo--;
                // ResetDeployableState();
                playerActionScript.weaponScript.DrawPrimary();
                playerActionScript.skillController.ActivateBubbleShield();
            } else {
                DeployDeployable(deployPos, deployRot, false);
                currentAmmo--;
                playerActionScript.weaponScript.SyncAmmoCounts();
                fireTimer = 0.0f;
            }
        } else if (weaponStats.category.Equals("Etc")) {
            if (weaponStats.name.EndsWith("(Skill)")) {
                PhoneDeviceScript p = weaponMetaData.GetComponentInChildren<PhoneDeviceScript>();
                p.UseDevice(playerActionScript);
                if (playerActionScript.weaponScript.currentlyEquippedType == -2) {
                    playerActionScript.skillController.ActivateEcmFeedback();
                } else if (playerActionScript.weaponScript.currentlyEquippedType == -3) {
                    playerActionScript.skillController.ActivateInfraredScan();
                }
                currentAmmo--;
                playerActionScript.weaponScript.DrawPrimary();
            }
        }
    }

    public void UseLauncherItem() {
        GameObject projectile = GameObject.Instantiate((GameObject)Resources.Load(InventoryScript.itemData.weaponCatalog[weaponStats.name].projectilePath), camTransform.position + camTransform.forward, Quaternion.identity);
        PhotonView thisPView = projectile.GetComponent<PhotonView>();
        // photonView.SetOwnerInternal(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        if (PhotonNetwork.AllocateViewID(thisPView))
        {
            // projectile.transform.right = -weaponHolderFpc.transform.forward;
            projectile.transform.right = -camTransform.forward;
            projectile.GetComponent<LauncherScript>().Launch(pView.ViewID, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
            currentAmmo--;
            playerActionScript.weaponScript.SyncAmmoCounts();
            fireTimer = 0.0f;
            SpawnLauncherItemOnOthers(projectile);
        }
        else
        {
            Debug.Log("Failed to allocate a ViewId for projectile.");
            Destroy(projectile);
        }
    }

    void SpawnThrowableItemOnOthers(GameObject projectile)
    {
        PhotonView photonView = projectile.GetComponent<PhotonView>();
        object[] data = new object[]
        {
            InventoryScript.itemData.weaponCatalog[weaponStats.name].projectilePath, weaponHolderFpc.transform.position.x, weaponHolderFpc.transform.position.y, weaponHolderFpc.transform.position.z, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z, photonView.ViewID, playerActionScript.gameController.teamMap, pView.ViewID, (1f + playerActionScript.skillController.throwForceBoost)
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent(THROWABLE_SPAWN_CODE, data, raiseEventOptions, sendOptions);
    }

    void SpawnLauncherItemOnOthers(GameObject projectile)
    {
        PhotonView photonView = projectile.GetComponent<PhotonView>();
        object[] data = new object[]
        {
            InventoryScript.itemData.weaponCatalog[weaponStats.name].projectilePath, camTransform.position.x, camTransform.position.y, camTransform.position.z, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z, photonView.ViewID, playerActionScript.gameController.teamMap, pView.ViewID
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent(LAUNCHER_SPAWN_CODE, data, raiseEventOptions, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == LAUNCHER_SPAWN_CODE)
        {
            object[] data = (object[]) photonEvent.CustomData;
            int fromViewId = (int) data[9];
            if (fromViewId != pView.ViewID) return;

            string team = (string) data[8];

            if (team != playerActionScript.gameController.teamMap) return;

            string projectilePath = (string) data[0];
            Vector3 origin = new Vector3((float) data[1], (float) data[2], (float) data[3]);
            Vector3 forward = new Vector3((float) data[4], (float) data[5], (float) data[6]);

            GameObject projectile = GameObject.Instantiate((GameObject)Resources.Load(projectilePath), origin + forward, Quaternion.identity);
            PhotonView photonView = projectile.GetComponent<PhotonView>();
            photonView.ViewID = (int) data[7];
            Debug.Log("Spawned launcher projectile " + projectile.gameObject.name + " with view ID " + photonView.ViewID);
            
            // projectile.transform.right = -weaponHolderFpc.transform.forward;
            projectile.transform.right = -forward;
            projectile.GetComponent<LauncherScript>().Launch((int) data[7], forward.x, forward.y, forward.z);
            currentAmmo--;
            playerActionScript.weaponScript.SyncAmmoCounts();
            fireTimer = 0.0f;
        } else if (photonEvent.Code == THROWABLE_SPAWN_CODE) {
            object[] data = (object[]) photonEvent.CustomData;
            int fromViewId = (int) data[9];
            if (fromViewId != pView.ViewID) return;

            string team = (string) data[8];

            if (team != playerActionScript.gameController.teamMap) return;

            string projectilePath = (string) data[0];
            Vector3 origin = new Vector3((float) data[1], (float) data[2], (float) data[3]);
            Vector3 forward = new Vector3((float) data[4], (float) data[5], (float) data[6]);

            GameObject projectile = GameObject.Instantiate((GameObject)Resources.Load(projectilePath), origin, Quaternion.identity);
            PhotonView photonView = projectile.GetComponent<PhotonView>();
            photonView.ViewID = (int) data[7];
            Debug.Log("Spawned throwable projectile " + projectile.gameObject.name + " with view ID " + photonView.ViewID);

            projectile.transform.forward = forward;
            projectile.GetComponent<ThrowableScript>().Launch((int) data[7], forward.x, forward.y, forward.z, 1f + (float)data[10]);
            currentAmmo--;
            playerActionScript.weaponScript.SyncAmmoCounts();
            fireTimer = 0.0f;
        }
    }

    bool DeployPositionIsValid() {
        // Nothing can ever be planted in mid-air.
        // If the deploy plan mesh is sticky, then it can be planted anywhere.
        // If it isn't, then it can only be planted if the up vector is above 45 degrees
        RaycastHit hit;
        int validTerrainMask = (1 << 4) | (1 << 5) | (1 << 9) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15) | (1 << 17) | (1 << 16) | (1 << 18);
        validTerrainMask = ~validTerrainMask;
        if (deployPlanMesh.collidingWithObject == null) {
            return false;
        }
        if (weaponMetaData.isSticky) {
            return true;
        } else {
            if (deployPlanMesh.gameObject.transform.up.y >= 0.5f) {
                return true;
            }
        }
        return false;
    }

    void InstantiateDeployPlanMesh() {
        GameObject m = (GameObject)Instantiate(weaponMetaData.deployPlanMesh, CalculateDeployPlanMeshPos(), Quaternion.identity);
        deployPlanMesh = m.GetComponent<DeployMeshScript>();
    }

    Vector3 CalculateDeployPlanMeshPos() {
        return camTransform.position + (camTransform.forward * DEPLOY_OFFSET); 
    }

    void UpdateDeployPlanMesh() {
        Rigidbody deployPlanMeshR = deployPlanMesh.gameObject.GetComponent<Rigidbody>();
        // deployPlanMesh.gameObject.transform.position = CalculateDeployPlanMeshPos();
        Vector3 destinationPos = CalculateDeployPlanMeshPos();
        var toDestination = destinationPos - deployPlanMesh.gameObject.transform.position;
        // Calculate force
        var force = toDestination / Time.fixedDeltaTime * 0.5f;
        // Remove any existing velocity and add force to move to final position
        deployPlanMeshR.velocity = Vector3.zero;
        deployPlanMeshR.AddForce(force, ForceMode.VelocityChange);
        if (deployPlanMesh.collidingWithObject == null) {
            deployPlanMesh.gameObject.transform.rotation = Quaternion.identity;
        } else {
            deployPlanMesh.gameObject.transform.rotation = Quaternion.FromToRotation (Vector3.up, deployPlanMesh.contactNormal);
            // deployPlanMesh.gameObject.transform.up = deployPlanMesh.collidingWithObject.transform.up;
        }
        if (!DeployPositionIsValid()) {
            hudScript.ToggleDeployInvalidText(true);
            deployPlanMesh.IndicateIsInvalid(true);
        } else {
            hudScript.ToggleDeployInvalidText(false);
            deployPlanMesh.IndicateIsInvalid(false);
        }
    }

    void DestroyDeployPlanMesh() {
        Destroy(deployPlanMesh.gameObject);
        deployPlanMesh = null;
    }

    public void ConfirmGrenadeThrow() {
        throwGrenade = true;
    }

    public void ResetGrenadeState() {
        throwGrenade = false;
        isCockingGrenade = false;
        animatorFpc.ResetTrigger("isCockingGrenade");
        animatorFpc.ResetTrigger("ThrowGrenade");
    }

    public void ResetBoosterState() {
        isUsingBooster = false;
        animatorFpc.ResetTrigger("UseBooster");
    }

    public void ResetDeployableState() {
        isUsingDeployable = false;
        animatorFpc.ResetTrigger("UseDeployable");
    }

    public void CockGrenadeAnim()
    {
        pView.RPC("RpcCockGrenade", RpcTarget.Others);
    }

    [PunRPC]
    void RpcCockGrenade() {
        if (gameObject.layer == 0) return;
        //GetComponentInChildren<ThrowableScript>().PlayPinSound();
        animator.SetTrigger("Cock");
    }

    [PunRPC]
    void RpcUseBooster() {
        if (gameObject.layer == 0) return;
        animator.SetTrigger("Fire");
    }

    [PunRPC]
    void RpcUseDeployable() {
        if (gameObject.layer == 0) return;
        animator.SetTrigger("Fire");
    }

    void UseDeployable() {
        PlaySupportActionSound();
        UseSupportItem();
		// animatorFpc.ResetTrigger("UseDeployable");
    }

    bool CanInitiateReload() {
        if ((!playerActionScript.fpc.m_IsRunning || (playerActionScript.fpc.m_IsRunning && playerActionScript.skillController.HasRunNGun())) && !fpc.GetIsSwimming() && currentAmmo < weaponStats.clipCapacity && totalAmmoLeft > 0 && !IsPumpActionCocking() && !IsBoltActionCocking() && !isDrawing && !isReloading && (playerActionScript.weaponScript.currentlyEquippedType == 1 || playerActionScript.weaponScript.currentlyEquippedType == 2)) {
            return true;
        }
        return false;
    }

    public void ResetMyActionStates() {
        isDrawing = false;
        isFiring = false;
        isMeleeing = false;
        isReloading = false;
        isCockingGrenade = false;
        isUsingBooster = false;
        isUsingDeployable = false;
        deployInProgress = false;
        isCocking = false;
    }

    public void UseFirstAidKit(int deployableId) {
        pView.RPC("RpcUseFirstAidKit", RpcTarget.All, deployableId);
    }

    [PunRPC]
    void RpcUseFirstAidKit(int deployableId) {
        playerActionScript.health = 100;
        DeployableScript d = playerActionScript.gameController.GetDeployable(deployableId).GetComponent<DeployableScript>();
        d.UseDeployableItem();
        if (d.CheckOutOfUses()) {
            playerActionScript.gameController.DestroyDeployable(d.deployableId);
        }
    }

    public void UseAmmoBag(int deployableId) {
        pView.RPC("RpcUseAmmoBag", RpcTarget.All, deployableId);
    }

    [PunRPC]
    void RpcUseAmmoBag(int deployableId) {
        playerActionScript.weaponScript.MaxRefillAllAmmo();
        DeployableScript d = playerActionScript.gameController.GetDeployable(deployableId).GetComponent<DeployableScript>();
        d.UseDeployableItem();
        if (d.CheckOutOfUses()) {
            playerActionScript.gameController.DestroyDeployable(d.deployableId);
        }
    }

    public void DeployDeployable(Vector3 pos, Quaternion rot, bool fromSkill) {
        GameObject o = GameObject.Instantiate(weaponMetaData.deployRef, pos, rot);
        DeployableScript d = o.GetComponent<DeployableScript>();
        int skillBoost = 0;
        if (fromSkill) {
            skillBoost = PlayerData.playerdata.skillList["2/11"].Level;
        } else {
            if (PlayerData.playerdata.skillList["4/0"].Level == 1) {
                skillBoost = 1;
            } else if (PlayerData.playerdata.skillList["4/0"].Level == 2) {
                skillBoost = 2;
            } else if (PlayerData.playerdata.skillList["4/0"].Level == 3) {
                skillBoost = 3;
            }
        }
        int dId = d.InstantiateDeployable(skillBoost);
		playerActionScript.gameController.DeployDeployable(dId, o);
		pView.RPC("RpcDeployDeployable", RpcTarget.Others, dId, skillBoost, playerActionScript.gameController.teamMap, pos.x, pos.y, pos.z, rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
    }

    [PunRPC]
    public void RpcDeployDeployable(int deployableId, int skillBoost, string team, float posX, float posY, float posZ, float rotX, float rotY, float rotZ) {
        if (team != playerActionScript.gameController.teamMap) return;
		GameObject o = GameObject.Instantiate(weaponMetaData.deployRef, new Vector3(posX, posY, posZ), Quaternion.Euler(rotX, rotY, rotZ));
        DeployableScript d = o.GetComponent<DeployableScript>();
		d.deployableId = deployableId;
        d.InstantiateDeployable(skillBoost);
		playerActionScript.gameController.DeployDeployable(deployableId, o);
    }

    public void SetUnpaused()
    {
        unpauseDelay = UNPAUSE_DELAY;
    }

    public int GetHeadshotCount()
    {
        return headshotCount;
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

    bool DetermineAmmoDeductSkip()
    {
        if (playerActionScript.skillController.GetSnipersDelBoost() && weaponStats.category == "Sniper Rifle") {
            return true;
        } else if (playerActionScript.skillController.GetBulletStreamBoost() && weaponStats.category != "Sniper Rifle") {
            return true;
        }
        return false;
    }

}
