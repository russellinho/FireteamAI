using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

public class WeaponActionScript : MonoBehaviour
{

    private const float SHELL_SPEED = 3f;
    private const float SHELL_TUMBLE = 4f;
    private const float DEPLOY_BASE_TIME = 2f;
    private const short DEPLOY_OFFSET = 2;
    private const float LUNGE_SPEED = 20f;

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
    public WeaponStats weaponStats;
    public WeaponStats meleeStats;
    private WeaponMods weaponMods;

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
    public float spread = 0f;
    private float recoilTime = 0f;
    private float swayGauge = 0f;
    private bool voidRecoilRecover = true;
    private bool throwGrenade;
    //private float recoilSlerp = 0f;

    public int totalAmmoLeft;
    public int currentAmmo;

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
    
    public GameObject hitParticles;
    public GameObject bulletImpact;
    public GameObject bloodEffect;

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
    // Use this for initialization
    void Start()
    {
        deployTimer = 0f;
        aimDownSightsLock = false;
        aimDownSightsTimer = 0f;
        throwGrenade = false;
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        currentAmmo = weaponStats.clipCapacity;

        originalPosCam = camTransform.localPosition;

        originalPosCamSecondary = new Vector3(-0.13f, 0.11f, 0.04f);

        mouseLook = fpc.m_MouseLook;

        // Create animation event for shotgun reload

        // CreateAnimEvents();
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
        if (pView != null && !pView.IsMine)
        {
            return;
        }

        if (playerActionScript.health <= 0) {
            hudScript.toggleSniperOverlay(false);
            return;
        }

        if (!deployInProgress && deployPlanMesh != null) {
            DestroyDeployPlanMesh();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (firingMode == FireMode.Semi)
                firingMode = FireMode.Auto;
            else
                firingMode = FireMode.Semi;
        }

        meleeInput = Input.GetKeyDown(KeyCode.V) || (Input.GetAxis("Mouse ScrollWheel") < 0f);

        switch (firingMode)
        {
            case FireMode.Auto:
                shootInput = Input.GetButton("Fire1");
                break;
            case FireMode.Semi:
                shootInput = Input.GetButtonDown("Fire1");
                break;
        }

        if (!playerActionScript.canShoot || isWieldingThrowable || isWieldingBooster || isWieldingDeployable || hudScript.container.pauseMenuGUI.activeInHierarchy)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.R)) {
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

    void FixedUpdate()
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
        
        if (shootInput && !meleeInput && !isMeleeing && !isDrawing && !isReloading && playerActionScript.canShoot && !hudScript.container.pauseMenuGUI.activeInHierarchy)
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
                aimDownSightsTimer += (Time.deltaTime * weaponStats.aimDownSightSpeed);
            }
            // If going to center
            if (isAiming) {
                if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, currentAimStableHandPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, currentAimDownSightPos, aimDownSightsTimer);
                } else if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'F') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, currentAimStableHandPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, currentAimDownSightPos, aimDownSightsTimer);
                }
            // If coming back to normal
            } else {
                // If the player is back in the normal position, then disable the lock
                if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, weaponStats.defaultLeftCollarPosMale, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, weaponStats.defaultRightCollarPosMale, aimDownSightsTimer);
                    if (aimDownSightsLock && aimDownSightsTimer >= 1f) {
                        aimDownSightsLock = false;
                    }
                } else if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'F') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarCurrentPos, weaponStats.defaultLeftCollarPosFemale, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarCurrentPos, weaponStats.defaultRightCollarPosFemale, aimDownSightsTimer);
                    if (aimDownSightsLock && aimDownSightsTimer >= 1f) {
                        aimDownSightsLock = false;
                    }
                }
            }
        }
    }

    void LateUpdate() {
        UpdateAimDownSightsArms();
        if (deployPlanMesh != null) {
            UpdateDeployPlanMesh();
        }
    }

    void ToggleSniper(bool b) {
        if (!weaponStats.isSniper) return;
        if (weaponStats.weaponParts[0].enabled == b) return;
        foreach (MeshRenderer weaponPart in weaponStats.weaponParts) {
            weaponPart.enabled = b;
        }
        if (weaponStats.suppressorSlot != null) {
            MeshRenderer suppressorRenderer = weaponStats.suppressorSlot.GetComponentInChildren<MeshRenderer>();
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

    public void AimDownSights()
    {
        if (!playerActionScript.fpc.m_IsRunning)
        {
            // Logic for toggle aim rather than hold down aim
            /**if (Input.GetButtonDown ("Fire2") && !isReloading) {
                isAiming = !isAiming;
            }
            if (isAiming && !isReloading) {
                originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, aimPos, Time.deltaTime * aodSpeed);
            } else {
                originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, originalPos, Time.deltaTime * aodSpeed);
            }*/
            if (Input.GetButton("Fire2") && !isReloading && !IsPumpActionCocking() && !isDrawing)
            {
                fpc.SetAiminginFPCAnimator(true);
                if (!isAiming) {
                    isAiming = true;
                    leftCollarCurrentPos = leftCollar.localPosition;
                    rightCollarCurrentPos = rightCollar.localPosition;
                    aimDownSightsTimer = 0f;
                }
                aimDownSightsLock = true;
                if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
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

    [PunRPC]
    void RpcAddToTotalKills()
    {
        GameControllerScript.playerList[pView.Owner.ActorNumber].kills++;
        if (gameObject.layer == 0) return;
        if (playerActionScript.kills != int.MaxValue) {
            playerActionScript.kills++;
        }
    }

    // Increment kill count and display HUD popup for kill
    public void RewardKill(bool isHeadshot) {
        pView.RPC("RpcAddToTotalKills", RpcTarget.All);
        if (isHeadshot) {
            hudScript.OnScreenEffect("HEADSHOT", true);
        } else {
            hudScript.OnScreenEffect(playerActionScript.kills + " KILLS", true);
        }
    }

    public void SetMouseDynamicsForMelee(bool b) {
        fpc.SetMouseDynamicsForMelee(b);
    }

    bool CanMelee() {
        if (!fpc.m_CharacterController.isGrounded || isCocking || isDrawing || isMeleeing || isFiring || isAiming || isCockingGrenade || deployInProgress || isUsingBooster || isUsingDeployable) {
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
                        n.PlayGruntSound();
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
        weaponStats.weaponAnimator.Play("Fire");
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
            if (hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().health > 0)
            {
                hudScript.InstantiateHitmarker();
                BetaEnemyScript b = hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>();
                hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                RewardKill(true);
                audioController.PlayHeadshotSound();
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
                        n.PlayGruntSound();
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
                        }
                    }
                }
            } else if (hit.transform.tag.Equals("Player")) {
                if (hit.transform != gameObject.transform) {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                }
            } else {
                pView.RPC("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
                pView.RPC("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
            }
        }
        if (weaponMods.suppressorRef == null)
        {
            playerActionScript.gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
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
        weaponStats.weaponAnimator.Play("Fire");
        IncreaseRecoil();
        UpdateRecoil(true);
        pView.RPC("FireEffectsLauncher", RpcTarget.All);
    }

    public void SpawnShellCasing() {
        GameObject o = Instantiate(weaponStats.weaponShell, weaponStats.weaponShellPoint.position, Quaternion.Euler(0f, 0f, 0f));
        o.transform.forward = -weaponStats.transform.right;
        o.GetComponent<Rigidbody>().velocity = weaponStats.transform.forward * SHELL_SPEED;
        o.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * SHELL_TUMBLE;
        Destroy(o, 3f);
        pView.RPC("RpcSpawnShellCasing", RpcTarget.Others);
    }

    [PunRPC]
    void RpcSpawnShellCasing() {
        if (gameObject.layer == 0) return;
        GameObject o = Instantiate(weaponStats.weaponShell, weaponStats.weaponShellPoint.position, Quaternion.Euler(-90f, -90f, 90f));
        o.transform.forward = -weaponStats.transform.right;
        o.GetComponent<Rigidbody>().velocity = weaponStats.transform.forward * SHELL_SPEED;
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
            float xSpread = Random.Range(-0.07f, 0.07f);
            float ySpread = Random.Range(-0.07f, 0.07f);
            float zSpread = Random.Range(-0.07f, 0.07f);
            Vector3 impactDir = new Vector3(fpcShootPoint.transform.forward.x + xSpread, fpcShootPoint.transform.forward.y + ySpread, fpcShootPoint.transform.forward.z + zSpread);
            if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range, headshotLayer) && !headshotDetected)
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                if (hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().health > 0)
                {
                    hudScript.InstantiateHitmarker();
                    hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                    RewardKill(true);
                    audioController.PlayHeadshotSound();
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
                                n.PlayGruntSound();
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
                        }
                        totalDamageDealt += thisDamageDealt;
                    }
                } else if (hit.transform.tag.Equals("Player")) {
                    if (hit.transform != gameObject.transform) {
                        pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                    }
                } else {
                    pView.RPC("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
                    pView.RPC("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
                }
            }
        }

        playerActionScript.gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
        pView.RPC("FireEffects", RpcTarget.All);
    }

    [PunRPC]
    void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal, bool headshot)
    {
        if (gameObject.layer == 0) return;
        GameObject bloodSpill;
        if (headshot)
        {
            bloodEffect = (GameObject)Resources.Load("BloodEffectHeadshot");
        }
        else
        {
            bloodEffect = (GameObject)Resources.Load("BloodEffect");
        }
        bloodSpill = Instantiate(bloodEffect, point, Quaternion.FromToRotation(Vector3.forward, normal));
        bloodSpill.transform.Rotate(180f, 0f, 0f);
        Destroy(bloodSpill, 1.5f);
    }

    [PunRPC]
    void RpcInstantiateBulletHole(Vector3 point, Vector3 normal, string parentName)
    {
        if (gameObject.layer == 0) return;
        GameObject bulletHoleEffect = Instantiate(bulletImpact, point, Quaternion.FromToRotation(Vector3.forward, normal));
        bulletHoleEffect.transform.SetParent(GameObject.Find(parentName).transform);
        Destroy(bulletHoleEffect, 3f);
    }

    void InstantiateBulletHole(Vector3 point, Vector3 normal, string parentName)
    {
        GameObject bulletHoleEffect = Instantiate(bulletImpact, point, Quaternion.FromToRotation(Vector3.forward, normal));
        bulletHoleEffect.transform.SetParent(GameObject.Find(parentName).transform);
        Destroy(bulletHoleEffect, 3f);
    }

    [PunRPC]
    void RpcInstantiateHitParticleEffect(Vector3 point, Vector3 normal)
    {
        if (gameObject.layer == 0) return;
        GameObject hitParticleEffect = Instantiate(hitParticles, point, Quaternion.FromToRotation(Vector3.up, normal));
        Destroy(hitParticleEffect, 1f);
    }

    void InstantiateHitParticleEffect(Vector3 point, Vector3 normal)
    {
        GameObject hitParticleEffect = Instantiate(hitParticles, point, Quaternion.FromToRotation(Vector3.up, normal));
        Destroy(hitParticleEffect, 1f);
    }

    void InstantiateGunSmokeEffect(float duration) {
        if (weaponStats.gunSmoke != null) {
            GameObject gunSmokeEffect = null;
            //if (fpc.equipmentScript.isFirstPerson()) {
                //gunSmokeEffect = Instantiate(weaponStats.gunSmoke, weaponStats.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //} else {
                gunSmokeEffect = Instantiate(weaponStats.gunSmoke, weaponStats.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //}
            Destroy(gunSmokeEffect, duration);
        }
    }

    public int CalculateDamageDealt(float initialDamage, float baseY, float hitY, float height, int divisor = 1) {
        float total = initialDamage / (float)divisor;
        // Determine how high/low on the body was hit. The closer to 1, the closer to shoulders; closer to 0, closer to feet
        float bodyHeightHit = Mathf.Abs(hitY - baseY) / height;
        // Higher the height, the more damage dealt
        if (bodyHeightHit <= 0.6f) {
            total *= 0.6f;
        } else if (bodyHeightHit < 0.8f) {
            total *= bodyHeightHit;
        }
        return (int)total;
    }

    void PlayMuzzleFlash() {
        if (weaponStats.muzzleFlash != null) {
            weaponStats.muzzleFlash.Play();
        }
    }

    [PunRPC]
    void FireEffects()
    {
        if (gameObject.layer == 0) return;
        PlayMuzzleFlash();
        InstantiateGunSmokeEffect(1.5f);
        if (weaponStats.bulletTracer != null && !weaponStats.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponStats.bulletTracer.Play();
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
        if (weaponStats.bulletTracer != null && !weaponStats.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponStats.bulletTracer.Play();
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
        UseLauncherItem();
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

            int bulletsToLoad = weaponStats.clipCapacity - currentAmmo;
            int bulletsToDeduct = (totalAmmoLeft >= bulletsToLoad) ? bulletsToLoad : totalAmmoLeft;
            totalAmmoLeft -= bulletsToDeduct;
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
                animatorFpc.CrossFadeInFixedTime("ShotgunLoad", weaponStats.reloadTransitionSpeed);
            } else if (weaponStats.category.Equals("Sniper Rifle")) {
                animatorFpc.CrossFadeInFixedTime("BoltActionLoad", weaponStats.reloadTransitionSpeed);
            } else if (weaponStats.type.Equals("Support")) {
                animatorFpc.Play("DrawWeapon");
            } else {
                animatorFpc.CrossFadeInFixedTime("Reload", weaponStats.reloadTransitionSpeed);
                FpcChangeMagazine(weaponStats.reloadTransitionSpeed);
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
        weaponStats.weaponAnimator.Play("Reload");
    }

    public void FpcCockBoltAction() {
        weaponStats.weaponAnimator.Play("Cock");
    }

    public void FpcLoadBoltAction() {
        weaponStats.weaponAnimator.Play("Reload");
    }

    public void FpcChangeMagazine(float startFrame) {
        //weaponStats.weaponAnimator.Play("Reload", 0, startFrame);
        weaponStats.weaponAnimator.CrossFadeInFixedTime("Reload", startFrame);
    }

    [PunRPC]
    void RpcPlayReloadSound(int soundNumber)
    {
        if (gameObject.layer == 0) return;
        weaponStats.weaponSoundSource.clip = weaponStats.reloadSounds[soundNumber];
        weaponStats.weaponSoundSource.Play();
    }

    public void PlayReloadSound(int soundNumber)
    {
        pView.RPC("RpcPlayReloadSound", RpcTarget.All, soundNumber);
    }

    [PunRPC]
    void RpcPlaySupportActionSound() {
        if (gameObject.layer == 0) return;
        weaponStats.weaponSoundSource.clip = weaponStats.supportActionSound;
        weaponStats.weaponSoundSource.Play();
    }

    public void PlaySupportActionSound() {
        pView.RPC("RpcPlaySupportActionSound", RpcTarget.All);
    }

    private void PlayShootSound()
    {
        weaponStats.fireSound.Play();
    }

    private void PlaySuppressedShootSound() {
        weaponStats.suppressedFireSound.Play();
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
            recoilTime -= (RECOIL_ACCELERATION / weaponStats.recoveryConstant) * Time.deltaTime;
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
                mouseLook.SetRecoilInputs(-weaponStats.recoil / weaponStats.recoveryConstant, 0f);
            }
        }
    }

    public void SetCurrentAimDownSightPos(string sightName) {
        if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
            currentAimDownSightPos = weaponStats.aimDownSightPosMale;
            currentAimStableHandPos = weaponStats.stableHandPosMale;
        } else if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'F') {
            currentAimDownSightPos = weaponStats.aimDownSightPosFemale;
            currentAimStableHandPos = weaponStats.stableHandPosFemale;
        }
        if (sightName != null) {
            int index = InventoryScript.itemData.modCatalog[sightName].modIndex;
            currentAimDownSightPos.y += weaponStats.crosshairAimOffset[index];
            currentAimStableHandPos.y += weaponStats.crosshairAimOffset[index];
        }
    }

    public void SetWeaponStats(WeaponStats ws) {
        if (ws.type.Equals("Melee")) {
            meleeStats = ws;
            SetMeleeSpeed();
        } else {
            weaponStats = ws;
            weaponMods = ws.GetComponent<WeaponMods>();
            fireTimer = ws.fireRate;
            weaponCam.nearClipPlane = ws.aimDownSightClipping;
            playerActionScript.weaponSpeedModifier = ws.mobility/100f;
            if (playerActionScript.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
                fpc.fpcAnimator.runtimeAnimatorController = ws.maleOverrideController as RuntimeAnimatorController;
            } else {
                fpc.fpcAnimator.runtimeAnimatorController = ws.femaleOverrideController as RuntimeAnimatorController;
            }
            if (!ws.type.Equals("Support")) {
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
        }
    }

    public void SetReloadSpeed(float multipler = 1f) {
        animatorFpc.SetFloat("ReloadSpeed", weaponStats.defaultFpcReloadSpeed * multipler);
        animatorFpc.SetFloat("DrawSpeed", weaponStats.defaultWeaponDrawSpeed * multipler);
        weaponStats.weaponAnimator.SetFloat("ReloadSpeed", weaponStats.defaultWeaponReloadSpeed * multipler);
        weaponStats.weaponAnimator.SetFloat("CockingSpeed", weaponStats.defaultWeaponCockingSpeed * multipler);
    }

    public void SetFiringSpeed(float multiplier = 1f) {
        animatorFpc.SetFloat("FireSpeed", weaponStats.defaultFireSpeed * multiplier);
    }

    public void SetMeleeSpeed(float multiplier = 1f) {
        animatorFpc.SetFloat("MeleeSpeed", meleeStats.defaultMeleeSpeed * multiplier);
    }

    public void ModifyWeaponStats(float damage, float accuracy, float recoil, float range, int clipCapacity, int maxAmmo) {
        weaponStats.damage += damage;
        weaponStats.accuracy += accuracy;
        weaponStats.recoil += recoil;
        weaponStats.range += range;
        weaponStats.clipCapacity += clipCapacity;
        weaponStats.maxAmmo += maxAmmo;
    }

    public WeaponStats GetWeaponStats() {
        return weaponStats;
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
        if (fireTimer < weaponStats.fireRate || hudScript.container.pauseMenuGUI.activeInHierarchy)
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
        if (weaponStats.weaponName.Equals("Medkit") && playerActionScript.health == playerActionScript.playerScript.health) {
            return;
        }
        if (isWieldingBooster && Input.GetButtonDown("Fire1")) {
            pView.RPC("RpcUseBooster", RpcTarget.All);
            animatorFpc.SetTrigger("UseBooster");
            isUsingBooster = true;
        }
    }

    void FireDeployable() {
        if (fireTimer < weaponStats.fireRate || hudScript.container.pauseMenuGUI.activeInHierarchy)
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
            GameObject projectile = PhotonNetwork.Instantiate(InventoryScript.itemData.weaponCatalog[weaponStats.weaponName].prefabPath + "Projectile", weaponHolderFpc.transform.position, Quaternion.identity);
            projectile.transform.forward = weaponHolderFpc.transform.forward;
            projectile.GetComponent<ThrowableScript>().Launch(gameObject, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
            // Reset fire timer and subtract ammo used
        } else if (weaponStats.category.Equals("Booster")) {
            // Reset fire timer and subtract ammo used
            BoosterScript boosterScript = weaponStats.GetComponentInChildren<BoosterScript>();
            boosterScript.UseBoosterItem(weaponStats.weaponName);
        } else if (weaponStats.category.Equals("Deployable")) {
            DeployDeployable(weaponStats.weaponName, deployPos, deployRot);
        }
        currentAmmo--;
        playerActionScript.weaponScript.SyncAmmoCounts();
        fireTimer = 0.0f;
    }

    public void UseLauncherItem() {
        GameObject projectile = PhotonNetwork.Instantiate(InventoryScript.itemData.weaponCatalog[weaponStats.weaponName].prefabPath + "Projectile", camTransform.position + camTransform.forward, Quaternion.identity);
        // projectile.transform.right = -weaponHolderFpc.transform.forward;
        projectile.transform.right = -camTransform.forward;
        projectile.GetComponent<LauncherScript>().Launch(gameObject, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
        currentAmmo--;
        playerActionScript.weaponScript.SyncAmmoCounts();
        fireTimer = 0.0f;
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
        if (weaponStats.isSticky) {
            return true;
        } else {
            if (deployPlanMesh.gameObject.transform.up.y >= 0.5f) {
                return true;
            }
        }
        return false;
    }

    void InstantiateDeployPlanMesh() {
        GameObject m = (GameObject)Instantiate(weaponStats.deployPlanMesh, CalculateDeployPlanMeshPos(), Quaternion.identity);
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
        if (!playerActionScript.fpc.m_IsRunning && currentAmmo < weaponStats.clipCapacity && totalAmmoLeft > 0 && !IsPumpActionCocking() && !isDrawing && !isReloading && (playerActionScript.weaponScript.currentlyEquippedType == 1 || playerActionScript.weaponScript.currentlyEquippedType == 2)) {
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

    public void DeployDeployable(string deployable, Vector3 pos, Quaternion rot) {
        GameObject o = GameObject.Instantiate((GameObject)Resources.Load(InventoryScript.itemData.weaponCatalog[deployable].prefabPath + "Deploy"), pos, rot);
        DeployableScript d = o.GetComponent<DeployableScript>();
        int dId = d.InstantiateDeployable();
		playerActionScript.gameController.DeployDeployable(dId, o);
		pView.RPC("RpcDeployDeployable", RpcTarget.Others, dId, deployable, playerActionScript.gameController.teamMap, pos.x, pos.y, pos.z, rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
    }

    [PunRPC]
    public void RpcDeployDeployable(int deployableId, string deployableName, string team, float posX, float posY, float posZ, float rotX, float rotY, float rotZ) {
        if (team != playerActionScript.gameController.teamMap) return;
		GameObject o = GameObject.Instantiate((GameObject)Resources.Load(InventoryScript.itemData.weaponCatalog[deployableName].prefabPath + "Deploy"), new Vector3(posX, posY, posZ), Quaternion.Euler(rotX, rotY, rotZ));
		o.GetComponent<DeployableScript>().deployableId = deployableId;
		playerActionScript.gameController.DeployDeployable(deployableId, o);
    }

}
