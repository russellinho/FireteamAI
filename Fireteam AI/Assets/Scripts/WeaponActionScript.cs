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
    private const byte LAUNCHER_SPAWN_CODE = 126;
    private const byte THROWABLE_SPAWN_CODE = 127;
    private const float SHELL_SPEED = 3f;
    private const float SHELL_TUMBLE = 4f;
    private const float DEPLOY_BASE_TIME = 2f;
    private const short DEPLOY_OFFSET = 2;
    private const float LUNGE_SPEED = 20f;
    private const float UNPAUSE_DELAY = 0.5f;

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
    public GameObject BloodEffect;
    public GameObject BloodEffectHeadshot;

    // Projectile spread constants
    public const float MAX_SPREAD = 0.15f;
    public const float SPREAD_ACCELERATION = 0.05f;
    public const float SPREAD_DECELERATION = 0.03f;

    // Projectile recoil constants
    public const float MAX_RECOIL_TIME = 1.4f;
    public const float RECOIL_ACCELERATION = 4.2f;
    public const float RECOIL_DECELERATION = 4.2f;
    private const float SWAY_ACCELERATION = 1.5f;

    // Projectile variables
    public EncryptedFloat spread = 0f;
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
    // Timer that prevents player from accidentally firing right after unpausing
    private float unpauseDelay;

    // Use this for initialization
    private bool initialized;

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
            if (weaponStats.category == "Assault Rifle") {
                if (firingMode == FireMode.Semi)
                    firingMode = FireMode.Auto;
                else
                    firingMode = FireMode.Semi;
            }
            hudScript.SetFireMode(firingMode.ToString().ToUpper());
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

        if (!playerActionScript.canShoot || isWieldingThrowable || isWieldingBooster || isWieldingDeployable)
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
        if (isDrawing || isFiring || isMeleeing || isReloading || isCockingGrenade || isUsingBooster || isUsingDeployable || deployInProgress || isCocking || fpc.m_IsRunning) {
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

    // Increment kill count and display HUD popup for kill
    public void RewardKill(bool isHeadshot) {
        AddToTotalKills();
        if (isHeadshot) {
            hudScript.OnScreenEffect("HEADSHOT", true);
        } else {
            hudScript.OnScreenEffect(GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills + " KILLS", true);
        }
    }

    public void SetMouseDynamicsForMelee(bool b) {
        fpc.SetMouseDynamicsForMelee(b);
    }

    bool CanMelee() {
        if (!fpc.m_CharacterController.isGrounded || isCocking || isDrawing || isMeleeing || isFiring || isAiming || isCockingGrenade || deployInProgress || isUsingBooster || isUsingDeployable || hudScript.container.pauseMenuGUI.pauseActive) {
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
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, meleeStats.lungeRange, enemyMask)) {
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
                NpcScript n = hit.transform.gameObject.GetComponent<NpcScript>();
                BetaEnemyScript b = hit.transform.gameObject.GetComponent<BetaEnemyScript>();
                if (n != null) {
                    int beforeHp = n.health;
                    if (beforeHp > 0) {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                        n.TakeDamage((int)meleeStats.damage);
                        n.PlayGruntSound(playerActionScript.gameController.teamMap);
                    }
                }
                if (b != null) {
                    int beforeHp = b.health;
                    if (beforeHp > 0)
                    {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                        hudScript.InstantiateHitmarker();
                        audioController.PlayHitmarkerSound();
                        b.TakeDamage((int)meleeStats.damage);
                        b.PlayGruntSound();
                        b.SetAlerted();
                        if (b.health <= 0 && beforeHp > 0)
                        {
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
        int headshotLayer = (1 << 13);
        if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range, headshotLayer))
        {
            pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
            NpcScript n = hit.transform.gameObject.GetComponentInParent<NpcScript>();
            BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
            if (n != null) {
                if (n.health > 0)
                {
                    n.TakeDamage(100);
                }
            }
            if (b != null) {
                if (b.health > 0)
                {
                    hudScript.InstantiateHitmarker();
                    b.TakeDamage(100);
                    RewardKill(true);
                    audioController.PlayHeadshotSound();
                }
            }
        } else if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range))
        {
            if (hit.transform.tag.Equals("Human"))
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                NpcScript n = hit.transform.gameObject.GetComponent<NpcScript>();
                BetaEnemyScript b = hit.transform.gameObject.GetComponent<BetaEnemyScript>();
                if (n != null) {
                    int beforeHp = n.health;
                    int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, hit.transform.position.y, hit.point.y, n.col.height);
                    if (beforeHp > 0)
                    {
                        n.TakeDamage(thisDamageDealt);
                        n.PlayGruntSound(playerActionScript.gameController.teamMap);
                    }
                }
                if (b != null) {
                    int beforeHp = b.health;
                    int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, hit.transform.position.y, hit.point.y, hit.transform.gameObject.GetComponent<CapsuleCollider>().height);
                    if (beforeHp > 0)
                    {
                        hudScript.InstantiateHitmarker();
                        audioController.PlayHitmarkerSound();
                        b.TakeDamage(thisDamageDealt);
                        b.PlayGruntSound();
                        b.SetAlerted();
                        if (b.health <= 0 && beforeHp > 0)
                        {
                            RewardKill(false);
                            audioController.PlayKillSound();
                        }
                    }
                }
            } else if (hit.transform.tag.Equals("Player")) {
                if (hit.transform != gameObject.transform) {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                }
            } else {
                Terrain t = hit.transform.gameObject.GetComponent<Terrain>();
                pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, (t == null ? -1 : t.index));
            }
        }
        if (weaponMods.suppressorRef == null)
        {
            playerActionScript.gameController.SetLastGunshotHeardPos(false, transform.position);
            pView.RPC("FireEffects", RpcTarget.All);
        }
        else
        {
            pView.RPC("FireEffectsSuppressed", RpcTarget.All);
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
        bool headshotDetected = false;
        float totalDamageDealt = 0f;
        int headshotLayer = (1 << 13);
        for (int i = 0; i < 8; i++) {
            float xSpread = Random.Range(-0.03f, 0.03f);
            float ySpread = Random.Range(-0.03f, 0.03f);
            float zSpread = Random.Range(-0.03f, 0.03f);
            Vector3 impactDir = new Vector3(fpcShootPoint.transform.forward.x + xSpread, fpcShootPoint.transform.forward.y + ySpread, fpcShootPoint.transform.forward.z + zSpread);
            if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range, headshotLayer) && !headshotDetected)
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                NpcScript n = hit.transform.gameObject.GetComponentInParent<NpcScript>();
                BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                if (n != null) {
                    if (n.health > 0)
                    {
                        n.TakeDamage(100);
                    }
                }
                if (b != null) {
                    if (b.health > 0)
                    {
                        hudScript.InstantiateHitmarker();
                        b.TakeDamage(100);
                        RewardKill(true);
                        audioController.PlayHeadshotSound();
                    }
                }
                headshotDetected = true;
            } else if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range))
            {
                // Debug.DrawRay(fpcShootPoint.position, impactDir, Color.blue, 10f, false);
                if (hit.transform.tag.Equals("Human"))
                {
                    BetaEnemyScript b = hit.transform.gameObject.GetComponent<BetaEnemyScript>();
                    NpcScript n = hit.transform.gameObject.GetComponent<NpcScript>();
                    if (n != null) {
                        int beforeHp = 0;
                        int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, hit.transform.position.y, hit.point.y, n.col.height, 8);
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                        beforeHp = n.health;
                        if (totalDamageDealt == 0f) {
                            if (beforeHp > 0)
                            {
                                n.PlayGruntSound(playerActionScript.gameController.teamMap);
                            }
                        }
                        n.TakeDamage(thisDamageDealt);
                        totalDamageDealt += thisDamageDealt;
                    }
                    if (b != null) {
                        int beforeHp = 0;
                        int thisDamageDealt = CalculateDamageDealt(weaponStats.damage, hit.transform.position.y, hit.point.y, hit.transform.gameObject.GetComponent<CapsuleCollider>().height, 8);
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                        beforeHp = b.health;
                        if (totalDamageDealt == 0f) {
                            if (beforeHp > 0)
                            {
                                hudScript.InstantiateHitmarker();
                                audioController.PlayHitmarkerSound();
                                //hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)weaponStats.damage);
                                b.PlayGruntSound();
                                b.SetAlerted();
                            }
                        }
                        b.TakeDamage(thisDamageDealt);
                        if (b.health <= 0 && beforeHp > 0)
                        {
                            RewardKill(false);
                            audioController.PlayKillSound();
                        }
                        totalDamageDealt += thisDamageDealt;
                    }
                } else if (hit.transform.tag.Equals("Player")) {
                    if (hit.transform != gameObject.transform) {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                    }
                } else {
                    Terrain t = hit.transform.gameObject.GetComponent<Terrain>();
                    pView.RPC("RpcHandleBulletVfx", RpcTarget.All, hit.point, -hit.normal, (t == null ? -1 : t.index));
                }
            }
        }

        playerActionScript.gameController.SetLastGunshotHeardPos(false, transform.position);
        pView.RPC("FireEffects", RpcTarget.All);
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
    void RpcHandleBulletVfx(Vector3 point, Vector3 normal, int terrainId) {
        if (gameObject.layer == 0) return;
        if (terrainId == -1) return;
        Terrain terrainHit = playerActionScript.gameController.terrainMetaData[terrainId];
        GameObject bulletHoleEffect = Instantiate(terrainHit.GetRandomBulletHole(), point, Quaternion.FromToRotation(Vector3.forward, normal));
        bulletHoleEffect.transform.SetParent(terrainHit.gameObject.transform);
        Destroy(bulletHoleEffect, 4f);
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

    public int CalculateDamageDealt(float initialDamage, float baseY, float hitY, float height, int divisor = 1) {
        float total = initialDamage / (float)divisor;
        // Determine how high/low on the body was hit. The closer to 1, the closer to shoulders; closer to 0, closer to feet
        float bodyHeightHit = Mathf.Abs(hitY - baseY) / height;
        // Higher the height, the more damage dealt
        if (bodyHeightHit < 0.2f) {
            total *= 0.6f;
        } else if (bodyHeightHit < 0.4f) {
            total *= 0.85f;
        }
        return (int)total;
    }

    void PlayMuzzleFlash() {
        if (weaponMetaData.muzzleFlash != null) {
            weaponMetaData.muzzleFlash.Play();
        }
    }

    [PunRPC]
    void FireEffects()
    {
        if (gameObject.layer == 0) return;
        PlayMuzzleFlash();
        InstantiateGunSmokeEffect(1.5f);
        if (weaponMetaData.bulletTracer != null && !weaponMetaData.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponMetaData.bulletTracer.Play();
        }
        PlayShootSound();
        currentAmmo--;
        playerActionScript.weaponScript.SyncAmmoCounts();
        // Reset fire timer
        fireTimer = 0.0f;
    }

    [PunRPC]
    void FireEffectsSuppressed()
    {
        if (gameObject.layer == 0) return;
        InstantiateGunSmokeEffect(1.5f);
        if (weaponMetaData.bulletTracer != null && !weaponMetaData.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponMetaData.bulletTracer.Play();
        }
        PlaySuppressedShootSound();
        currentAmmo--;
        playerActionScript.weaponScript.SyncAmmoCounts();
        // Reset fire timer
        fireTimer = 0.0f;
    }

    [PunRPC]
    void FireEffectsLauncher()
    {
        if (gameObject.layer == 0) return;
        InstantiateGunSmokeEffect(3f);
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
            animator.SetTrigger("Reloading");
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
        if (spread < MAX_SPREAD)
        {
            spread += SPREAD_ACCELERATION * Time.deltaTime;
            if (spread > MAX_SPREAD)
            {
                spread = MAX_SPREAD;
            }
        }
    }

    private void DecreaseSpread()
    {
        if (spread > 0f)
        {
            spread -= SPREAD_DECELERATION * Time.deltaTime;
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
        if (increase)
        {
            // mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(weaponStats.recoil, 0f, 0f);
            // mouseLook.m_FpcCharacterHorizontalTargetRot *= Quaternion.Euler(0f, (Random.Range(0, 2) == 0 ? 1f : -1f) * swayGauge, 0f);
            mouseLook.SetRecoilInputs(weaponStats.recoil, (Random.Range(0, 2) == 0 ? 1f : -1f) * swayGauge);
        }
        else
        {
            if (recoilTime > 0f)
            {
                // mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(-weaponStats.recoil / weaponStats.recoveryConstant, 0f, 0f);
                mouseLook.SetRecoilInputs(-weaponStats.recoil / weaponMetaData.recoveryConstant, 0f);
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
            weaponCam.nearClipPlane = ws.aimDownSightClipping;
            playerActionScript.weaponSpeedModifier = w.mobility/100f;
            if (playerActionScript.equipmentScript.GetGender() == 'M') {
                fpc.fpcAnimator.runtimeAnimatorController = ws.maleOverrideController as RuntimeAnimatorController;
            } else {
                fpc.fpcAnimator.runtimeAnimatorController = ws.femaleOverrideController as RuntimeAnimatorController;
            }
            if (!w.type.Equals("Support")) {
                SetReloadSpeed();
                SetFiringSpeed();
            }
            if (weaponStats.type.Equals("Support")) {
                if (weaponStats.category.Equals("Explosive")) {
                    isWieldingThrowable = true;
                    isWieldingBooster = false;
                    isWieldingDeployable = false;
                    firingMode = FireMode.Semi;
                } else if (weaponStats.category.Equals("Booster")) {
                    isWieldingThrowable = false;
                    isWieldingBooster = true;
                    isWieldingDeployable = false;
                    firingMode = FireMode.Semi;
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
                    firingMode = FireMode.Semi;
                } else {
                    shotMode = ShotMode.Single;
                    if (weaponStats.category.Equals("Pistol") || weaponStats.category.Equals("Launcher")) {
                        firingMode = FireMode.Semi;
                    }
                }
            }

            if (pView.IsMine) {
                hudScript.SetFireMode(firingMode.ToString().ToUpper());
                hudScript.SetWeaponLabel();
            }
        }
    }

    public void SetReloadSpeed(float multipler = 1f) {
        animatorFpc.SetFloat("ReloadSpeed", weaponMetaData.defaultFpcReloadSpeed * multipler);
        animatorFpc.SetFloat("DrawSpeed", weaponMetaData.defaultWeaponDrawSpeed * multipler);
        weaponMetaData.weaponAnimator.SetFloat("ReloadSpeed", weaponMetaData.defaultWeaponReloadSpeed * multipler);
        weaponMetaData.weaponAnimator.SetFloat("CockingSpeed", weaponMetaData.defaultWeaponCockingSpeed * multipler);
    }

    public void SetFiringSpeed(float multiplier = 1f) {
        animatorFpc.SetFloat("FireSpeed", weaponMetaData.defaultFireSpeed * multiplier);
    }

    public void SetMeleeSpeed(float multiplier = 1f) {
        animatorFpc.SetFloat("MeleeSpeed", meleeMetaData.defaultMeleeSpeed * multiplier);
    }

    public void ModifyWeaponStats(float damage, float accuracy, float recoil, float range, int clipCapacity, int maxAmmo) {
        weaponStats.damage += damage;
        weaponStats.accuracy += accuracy;
        weaponStats.recoil += recoil;
        weaponStats.range += range;
        weaponStats.clipCapacity += clipCapacity;
        weaponStats.maxAmmo += maxAmmo;
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
            pView.RPC("RpcCockGrenade", RpcTarget.Others, isCockingGrenade);
            // return;
        }
        if (isCockingGrenade && throwGrenade) {
            animatorFpc.SetTrigger("ThrowGrenade");
            throwGrenade = false;
            pView.RPC("RpcCockGrenade", RpcTarget.Others, isCockingGrenade);
        }
    }

    void FireBooster() {
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
            pView.RPC("RpcUseBooster", RpcTarget.All);
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
                deployTimer += (Time.deltaTime / DEPLOY_BASE_TIME);
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
                        pView.RPC("RpcUseDeployable", RpcTarget.All);
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
            GameObject projectile = PhotonNetwork.Instantiate(InventoryScript.itemData.weaponCatalog[weaponStats.name].projectilePath, weaponHolderFpc.transform.position, Quaternion.identity);
            projectile.transform.forward = weaponHolderFpc.transform.forward;
            projectile.GetComponent<ThrowableScript>().Launch(gameObject, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
            // Reset fire timer and subtract ammo used
        } else if (weaponStats.category.Equals("Booster")) {
            // Reset fire timer and subtract ammo used
            BoosterScript boosterScript = weaponMetaData.GetComponentInChildren<BoosterScript>();
            boosterScript.UseBoosterItem(weaponStats.name);
        } else if (weaponStats.category.Equals("Deployable")) {
            DeployDeployable(deployPos, deployRot);
        }
        currentAmmo--;
        playerActionScript.weaponScript.SyncAmmoCounts();
        fireTimer = 0.0f;

        // GameObject player = Instantiate((GameObject)Resources.Load(playerPrefab), spawnPoints, Quaternion.Euler(Vector3.zero));
        // PlayerData.playerdata.inGamePlayerReference = player;
        // PhotonView photonView = player.GetComponent<PhotonView>();
        // // photonView.ViewID = PhotonNetwork.LocalPlayer.ActorNumber;
        // photonView.SetOwnerInternal(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        // VivoxVoiceManager.Instance.AudioInputDevices.Muted = true;

        // if (PhotonNetwork.AllocateViewID(photonView))
        // {
        //     InitPlayerInGame(player);
        //     AddMyselfToPlayerList(photonView, player);
        //     SpawnMyselfOnOthers(true);
        // }
        // else
        // {
        //     Debug.Log("Failed to allocate a ViewId.");
        //     Destroy(player);
        // }
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
        int fromViewId = (int) data[9];
        if (fromViewId != pView.ViewID) return;
        if (photonEvent.Code == LAUNCHER_SPAWN_CODE)
        {
            object[] data = (object[]) photonEvent.CustomData;

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
        }
    }

    bool DeployPositionIsValid() {
        // Nothing can ever be planted in mid-air.
        // If the deploy plan mesh is sticky, then it can be planted anywhere.
        // If it isn't, then it can only be planted if the up vector is above 45 degrees
        RaycastHit hit;
        int validTerrainMask = (1 << 4) | (1 << 5) | (1 << 9) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15) | (1 << 16) | (1 << 17) | (1 << 18);
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

    [PunRPC]
    void RpcCockGrenade(bool cocking) {
        if (gameObject.layer == 0) return;
        //GetComponentInChildren<ThrowableScript>().PlayPinSound();
        animator.SetBool("isCockingGrenade", cocking);
    }

    [PunRPC]
    void RpcUseBooster() {
        if (gameObject.layer == 0) return;
        animator.SetTrigger("useBooster");
    }

    [PunRPC]
    void RpcUseDeployable() {
        if (gameObject.layer == 0) return;
        // TODO: This needs to be changed later to a different animation
        animator.SetTrigger("useBooster");
    }

    void UseDeployable() {
        PlaySupportActionSound();
        UseSupportItem();
		// animatorFpc.ResetTrigger("UseDeployable");
    }

    bool CanInitiateReload() {
        if (!playerActionScript.fpc.m_IsRunning && currentAmmo < weaponStats.clipCapacity && totalAmmoLeft > 0 && !IsPumpActionCocking() && !IsBoltActionCocking() && !isDrawing && !isReloading && (playerActionScript.weaponScript.currentlyEquippedType == 1 || playerActionScript.weaponScript.currentlyEquippedType == 2)) {
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

    public void DeployDeployable(Vector3 pos, Quaternion rot) {
        GameObject o = GameObject.Instantiate(weaponMetaData.deployRef, pos, rot);
        DeployableScript d = o.GetComponent<DeployableScript>();
        int dId = d.InstantiateDeployable();
		playerActionScript.gameController.DeployDeployable(dId, o);
		pView.RPC("RpcDeployDeployable", RpcTarget.Others, dId, playerActionScript.gameController.teamMap, pos.x, pos.y, pos.z, rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
    }

    [PunRPC]
    public void RpcDeployDeployable(int deployableId, string team, float posX, float posY, float posZ, float rotX, float rotY, float rotZ) {
        if (team != playerActionScript.gameController.teamMap) return;
		GameObject o = GameObject.Instantiate(weaponMetaData.deployRef, new Vector3(posX, posY, posZ), Quaternion.Euler(rotX, rotY, rotZ));
		o.GetComponent<DeployableScript>().deployableId = deployableId;
		playerActionScript.gameController.DeployDeployable(deployableId, o);
    }

    public void SetUnpaused()
    {
        unpauseDelay = UNPAUSE_DELAY;
    }

}
