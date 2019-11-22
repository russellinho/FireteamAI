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
    private WeaponMods weaponMods;

    // Projectile spread constants
    public const float MAX_SPREAD = 0.15f;
    public const float SPREAD_ACCELERATION = 0.05f;
    public const float SPREAD_DECELERATION = 0.03f;

    // Projectile recoil constants
    public const float MAX_RECOIL_TIME = 1.4f;
    public const float RECOIL_ACCELERATION = 4.2f;
    public const float RECOIL_DECELERATION = 4.2f;

    // Projectile variables
    public float spread = 0f;
    private float recoilTime = 0f;
    private bool voidRecoilRecover = true;
    private bool throwGrenade;
    //private float recoilSlerp = 0f;

    public int totalAmmoLeft;
    public int currentAmmo;

    public Transform shootPoint;
    public Transform fpcShootPoint;
    public bool isReloading = false;
    public bool isCocking = false;
    public bool isFiring = false;
    public bool isAiming;
    // Used for allowing arms to move during aim down sight movement
    private bool aimDownSightsLock;
    private float aimDownSightsTimer;
    
    public GameObject hitParticles;
    public GameObject bulletImpact;
    public GameObject bloodEffect;

    public enum FireMode { Auto, Semi }
    public enum ShotMode { Single, Burst }
    public FireMode firingMode;
    public ShotMode shotMode;
    private bool shootInput;

    // Once it equals fireRate, it will allow us to shoot
    float fireTimer = 0.0f;

    // Aiming down sights
    public Transform camTransform;
    private Vector3 originalPosCam;
    private Vector3 originalPosCamSecondary;
    // Aiming speed
    public PhotonView pView;
    public bool isWieldingThrowable;
    public bool isWieldingBooster;
    public bool isCockingGrenade;
    public bool isUsingBooster;
    public Transform rightCollar;
    public Transform leftCollar;
    private Vector3 leftCollarAimingPos;
    private Vector3 defaultLeftCollarPos;
    private Vector3 defaultRightCollarPos;
    public Vector3 leftCollarOriginalPos;
    public Vector3 rightCollarOriginalPos;

    // Zoom variables
    private int zoom = 3;
    private int defaultFov = 60;
    // Use this for initialization
    void Start()
    {
        leftCollarAimingPos = Vector3.negativeInfinity;
        isCockingGrenade = false;
        isUsingBooster = false;
        isWieldingThrowable = false;
        isWieldingBooster = false;
        aimDownSightsLock = false;
        defaultLeftCollarPos = Vector3.negativeInfinity;
        defaultRightCollarPos = Vector3.negativeInfinity;
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

        CreateAnimEvents();

        //CockingAction();
    }

    void CreateAnimEvents() {
        foreach (AnimationClip a in animatorFpc.runtimeAnimatorController.animationClips) {
            if (a.name.Equals("Loading_R870")) {
                AnimationEvent ae = new AnimationEvent();
                ae.time = 0.7f;
                ae.functionName = "ReloadShotgun";
                a.AddEvent(ae);
                break;
            }
        }
    }

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (firingMode == FireMode.Semi)
                firingMode = FireMode.Auto;
            else
                firingMode = FireMode.Semi;
        }

        switch (firingMode)
        {
            case FireMode.Auto:
                shootInput = Input.GetButton("Fire1");
                break;
            case FireMode.Semi:
                shootInput = Input.GetButtonDown("Fire1");
                break;
        }

        if (!playerActionScript.canShoot || isWieldingThrowable || isWieldingBooster)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.R)) {
            if (!playerActionScript.fpc.m_IsRunning && currentAmmo < weaponStats.clipCapacity && totalAmmoLeft > 0)
            {
                ReloadAction();
            }
        }

        // Automatically reload if no ammo
        if (currentAmmo <= 0 && !isFiring && totalAmmoLeft > 0 && !isReloading && playerActionScript.canShoot) {
            //Debug.Log("current ammo: " + currentAmmo + " isFiring: " + isFiring + " isReloading: " + isReloading);
            cameraShakeScript.SetShake(false);
            ReloadAction();
        }

        AimDownSights();
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
        if (weaponStats.category.Equals("Explosive")) {
            FireGrenades();
            return;
        }
        if (weaponStats.category.Equals("Booster")) {
            FireBooster();
            return;
        }
        
        if (shootInput && !isReloading && playerActionScript.canShoot)
        {
            if (currentAmmo > 0)
            {
                if (shotMode == ShotMode.Single) {
                    Fire();
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
            aimDownSightsTimer += (Time.deltaTime * weaponStats.aimDownSightSpeed);
            // If going to center
            if (isAiming) {
                if (fpc.equipmentScript.gender == 'M') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarOriginalPos, leftCollarAimingPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarOriginalPos, weaponStats.aimDownSightPosMale, aimDownSightsTimer);
                } else if (fpc.equipmentScript.gender == 'F') {
                    leftCollar.localPosition = Vector3.Lerp(leftCollarOriginalPos, leftCollarAimingPos, aimDownSightsTimer);
                    rightCollar.localPosition = Vector3.Lerp(rightCollarOriginalPos, weaponStats.aimDownSightPosFemale, aimDownSightsTimer);
                }
            // If coming back to normal
            } else {
                leftCollar.localPosition = Vector3.Lerp(leftCollarOriginalPos, defaultLeftCollarPos, aimDownSightsTimer);
                rightCollar.localPosition = Vector3.Lerp(rightCollarOriginalPos, defaultRightCollarPos, aimDownSightsTimer);
                // If the player is back in the normal position, then disable the lock
                if (Mathf.Approximately(Vector3.Magnitude(leftCollar.localPosition), Vector3.Magnitude(defaultLeftCollarPos))) {
                    aimDownSightsLock = false;
                }
            }
        }
    }

    void LateUpdate() {
        UpdateAimDownSightsArms();
    }

    void SetDefaultArmPositions() {
        // Set the default arm positions if they aren't set yet
        if (Vector3.Equals(defaultLeftCollarPos, Vector3.negativeInfinity)) {
            defaultLeftCollarPos = leftCollar.localPosition;
            defaultRightCollarPos = rightCollar.localPosition;
        }
    }

    void ToggleFpsWeapon(bool b) {
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

    // Determines if the weapon currently wielded is something to aim slowly
    bool IsSlowAimingWeapon() {
        if (weaponStats.type.Equals("Primary")) {
            return true;
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

            if (Input.GetButton("Fire2") && !isReloading && !isCocking)
            {
                SetDefaultArmPositions();
                fpc.SetAiminginFPCAnimator(true);
                if (!fpc.m_IsAiming && IsSlowAimingWeapon()) {
                    fpc.m_IsAiming = true;
                }
                isAiming = true;
                aimDownSightsLock = true;
                if (fpc.equipmentScript.gender == 'M') {
                    // Setting original arm positions
                    if (leftCollarAimingPos.Equals(Vector3.negativeInfinity)) {
                        leftCollarOriginalPos = leftCollar.localPosition;
                        rightCollarOriginalPos = rightCollar.localPosition;
                        Vector3 offset = weaponStats.aimDownSightPosMale - rightCollar.localPosition;
                        leftCollarAimingPos = leftCollar.localPosition + offset;
                        aimDownSightsTimer = 0f;
                    }
                
                    // Conditional to display sniper reticle, zoom in, disable the rifle mesh, and lower sensitivity
                    if (weaponStats.category == "Sniper Rifle" && Mathf.Approximately(Vector3.Magnitude(leftCollar.localPosition), Vector3.Magnitude(leftCollarAimingPos))) {
                      camTransform.GetComponent<Camera>().fieldOfView = zoom;
                    //   weaponHolderFpc.GetComponentInChildren<MeshRenderer>().enabled = false;
                    ToggleFpsWeapon(false);
                      mouseLook.XSensitivity = 0.25f;
                      mouseLook.YSensitivity = 0.25f;
                      fpc.equipmentScript.ToggleFpcMesh(false);
                      hudScript.toggleSniperOverlay(true);
                    }
                } else {
                    if (leftCollarAimingPos.Equals(Vector3.negativeInfinity)) {
                        leftCollarOriginalPos = leftCollar.localPosition;
                        rightCollarOriginalPos = rightCollar.localPosition;
                        Vector3 offset = weaponStats.aimDownSightPosFemale - rightCollar.localPosition;
                        leftCollarAimingPos = leftCollar.localPosition + offset;
                        aimDownSightsTimer = 0f;
                    }
                    
                    // Conditional to display sniper reticle, zoom in, disable the rifle mesh, and lower sensitivity
                    if (weaponStats.category == "Sniper Rifle" && Mathf.Approximately(Vector3.Magnitude(leftCollar.localPosition), Vector3.Magnitude(leftCollarAimingPos))) {
                      camTransform.GetComponent<Camera>().fieldOfView = zoom;
                    //   weaponHolderFpc.GetComponentInChildren<MeshRenderer>().enabled = false;
                    ToggleFpsWeapon(false);
                      mouseLook.XSensitivity = 0.25f;
                      mouseLook.YSensitivity = 0.25f;
                      fpc.equipmentScript.ToggleFpcMesh(false);
                      hudScript.toggleSniperOverlay(true);
                    }

                }
                //camTransform.GetComponent<Camera>().nearClipPlane = weaponStats.aimDownSightClipping;
            }
            else
            {
                fpc.SetAiminginFPCAnimator(false);
                fpc.m_IsAiming = false;
                isAiming = false;
                if (!Vector3.Equals(leftCollarAimingPos, Vector3.negativeInfinity)) {
                    leftCollarOriginalPos = leftCollar.localPosition;
                    rightCollarOriginalPos = rightCollar.localPosition;
                    leftCollarAimingPos = Vector3.negativeInfinity;
                    aimDownSightsTimer = 0f;
                }

                // Sets everything back to default after zooming in with sniper rifle
                camTransform.GetComponent<Camera>().fieldOfView = defaultFov;
                // weaponHolderFpc.GetComponentInChildren<MeshRenderer>().enabled = true;
                ToggleFpsWeapon(true);
                mouseLook.XSensitivity = mouseLook.originalXSensitivity;
                mouseLook.YSensitivity = mouseLook.originalYSensitivity;
                //camTransform.GetComponent<Camera>().nearClipPlane = 0.05f;
                fpc.equipmentScript.ToggleFpcMesh(true);
                hudScript.toggleSniperOverlay(false);

            }
        }
    }

    [PunRPC]
    void RpcAddToTotalKills()
    {
        playerActionScript.kills++;
        GameControllerScript.totalKills[pView.Owner.NickName]++;
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

    // Comment
    public void Fire()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || isCocking)
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
                hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                RewardKill(true);
                audioController.PlayHeadshotSound();
            }
        }
        else if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range))
        {
            GameObject bloodSpill = null;
            if (hit.transform.tag.Equals("Human"))
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                int beforeHp = hit.transform.gameObject.GetComponent<BetaEnemyScript>().health;
                if (beforeHp > 0)
                {
                    hudScript.InstantiateHitmarker();
                    audioController.PlayHitmarkerSound();
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)weaponStats.damage);
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().PlayGruntSound();
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().SetAlerted(true);
                    if (hit.transform.gameObject.GetComponent<BetaEnemyScript>().health <= 0 && beforeHp > 0)
                    {
                        RewardKill(false);
                    }
                }
            } else if (hit.transform.tag.Equals("Player")) {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
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
        GameObject o = Instantiate(weaponStats.weaponShell, weaponStats.weaponShellPoint.position, Quaternion.Euler(-90f, -90f, 90f));
        o.transform.forward = -weaponStats.transform.right;
        o.GetComponent<Rigidbody>().velocity = weaponStats.transform.forward * SHELL_SPEED;
        o.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * SHELL_TUMBLE;
        Destroy(o, 3f);
    }

    public void FireBoltAction() {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || isCocking)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
        animatorFpc.Play("Firing");
        isFiring = true;
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
                hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                RewardKill(true);
                audioController.PlayHeadshotSound();
            }
        }
        else if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range))
        {
            GameObject bloodSpill = null;
            if (hit.transform.tag.Equals("Human"))
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                int beforeHp = hit.transform.gameObject.GetComponent<BetaEnemyScript>().health;
                if (beforeHp > 0)
                {
                    hudScript.InstantiateHitmarker();
                    audioController.PlayHitmarkerSound();
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)weaponStats.damage);
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().PlayGruntSound();
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().SetAlerted(true);
                    if (hit.transform.gameObject.GetComponent<BetaEnemyScript>().health <= 0 && beforeHp > 0)
                    {
                        RewardKill(false);
                    }
                }
            } else if (hit.transform.tag.Equals("Player")) {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
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

    public void FireShotgun ()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading || isCocking)
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
        int regularHitsLanded = 0;
        float totalDamageDealt = 0f;
        for (int i = 0; i < 8; i++) {
            float xSpread = Random.Range(-0.1f, 0.1f);
            float ySpread = Random.Range(-0.1f, 0.1f);
            float zSpread = Random.Range(-0.1f, 0.1f);
            Vector3 impactDir = new Vector3(fpcShootPoint.transform.forward.x + xSpread, fpcShootPoint.transform.forward.y + ySpread, fpcShootPoint.transform.forward.z + zSpread);
            int headshotLayer = (1 << 13);
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
            }
            else if (Physics.Raycast(fpcShootPoint.position, impactDir, out hit, weaponStats.range))
            {
                int beforeHp = 0;
                GameObject bloodSpill = null;
                if (hit.transform.tag.Equals("Human"))
                {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                    beforeHp = hit.transform.gameObject.GetComponent<BetaEnemyScript>().health;
                    if (totalDamageDealt == 0f) {
                        if (beforeHp > 0)
                        {
                            hudScript.InstantiateHitmarker();
                            audioController.PlayHitmarkerSound();
                            //hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)weaponStats.damage);
                            hit.transform.gameObject.GetComponent<BetaEnemyScript>().PlayGruntSound();
                            hit.transform.gameObject.GetComponent<BetaEnemyScript>().SetAlerted(true);
                        }
                    }
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)(weaponStats.damage / 8f));
                    if (hit.transform.gameObject.GetComponent<BetaEnemyScript>().health <= 0 && beforeHp > 0)
                    {
                        RewardKill(false);
                    }
                    totalDamageDealt += (weaponStats.damage / 8f);
                } else if (hit.transform.tag.Equals("Player")) {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
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
        GameObject hitParticleEffect = Instantiate(hitParticles, point, Quaternion.FromToRotation(Vector3.up, normal));
        Destroy(hitParticleEffect, 1f);
    }

    void InstantiateHitParticleEffect(Vector3 point, Vector3 normal)
    {
        GameObject hitParticleEffect = Instantiate(hitParticles, point, Quaternion.FromToRotation(Vector3.up, normal));
        Destroy(hitParticleEffect, 1f);
    }

    void InstantiateGunSmokeEffect() {
        if (weaponStats.gunSmoke != null) {
            GameObject gunSmokeEffect = null;
            //if (fpc.equipmentScript.isFirstPerson()) {
                //gunSmokeEffect = Instantiate(weaponStats.gunSmoke, weaponStats.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //} else {
                gunSmokeEffect = Instantiate(weaponStats.gunSmoke, weaponStats.weaponShootPoint.position, Quaternion.Euler(315f, 0f, 0f));
            //}
            Destroy(gunSmokeEffect, 1.5f);
        }
    }

    void PlayMuzzleFlash() {
        if (weaponStats.muzzleFlash != null) {
            weaponStats.muzzleFlash.Play();
        }
    }

    [PunRPC]
    void FireEffects()
    {
        PlayMuzzleFlash();
        InstantiateGunSmokeEffect();
        if (weaponStats.bulletTracer != null && !weaponStats.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponStats.bulletTracer.Play();
        }
        PlayShootSound();
        currentAmmo--;
        // Reset fire timer
        fireTimer = 0.0f;
    }

    [PunRPC]
    void FireEffectsSuppressed()
    {
        InstantiateGunSmokeEffect();
        if (weaponStats.bulletTracer != null && !weaponStats.bulletTracer.isPlaying && !pView.IsMine)
        {
            weaponStats.bulletTracer.Play();
        }
        PlaySuppressedShootSound();
        currentAmmo--;
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
        }
    }

    public void ReloadShotgun() {
        if (!isCocking)
        {
            if (totalAmmoLeft <= 0)
                return;

            totalAmmoLeft--;
            currentAmmo++;
        }
    }

    private void ReloadSupportItem() {
        if (totalAmmoLeft > 0) {
            totalAmmoLeft -= weaponStats.clipCapacity;
            currentAmmo = weaponStats.clipCapacity;
        }
    }

    private void ReloadAction()
    {
        //AnimatorStateInfo info = weaponAnimator.GetCurrentAnimatorStateInfo (0);
        if (isReloading)
            return;

        isReloading = true;
        if (isCocking)
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

    public void CockingAction()
    {
        // if (!weaponStats.type.Equals("Support")) {
        //     isCocking = true;
        // }
        isCocking = true;
        ReloadAction();
    }

    [PunRPC]
    void RpcReloadAnim()
    {
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
                animatorFpc.CrossFade("ShotgunLoad", weaponStats.reloadTransitionSpeed);
            } else if (weaponStats.category.Equals("Sniper Rifle")) {
                animatorFpc.CrossFade("BoltActionLoad", weaponStats.reloadTransitionSpeed);
            } else if (weaponStats.type.Equals("Support")) {
                animatorFpc.Play("DrawWeapon");
            } else {
                animatorFpc.CrossFade("Reload", weaponStats.reloadTransitionSpeed);
                FpcChangeMagazine(weaponStats.reloadTransitionSpeed);
            }
            // animatorFpc.SetTrigger("Reload");
            // FpcChangeMagazine();
        // }
    }

    [PunRPC]
    void RpcCockingAnim()
    {
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
            //FpcCockShotgun();
        } else if (weaponStats.category.Equals("Sniper Rifle")) {
            animatorFpc.Play("DrawWeapon");
        } else if (weaponStats.type.Equals("Support")) {
            animatorFpc.Play("DrawWeapon");
        } else {
            weaponStats.weaponAnimator.Play("Reload", 0, weaponStats.cockStartTime);
            animatorFpc.Play("Reload", 0, weaponStats.cockStartTime);
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
        // weaponStats.weaponAnimator.Play("Reload", 0, startFrame);
        weaponStats.weaponAnimator.CrossFade("Reload", startFrame);
    }

    [PunRPC]
    void RpcPlayReloadSound(int soundNumber)
    {
        weaponStats.weaponSoundSource.clip = weaponStats.reloadSounds[soundNumber];
        weaponStats.weaponSoundSource.Play();
    }

    public void PlayReloadSound(int soundNumber)
    {
        pView.RPC("RpcPlayReloadSound", RpcTarget.All, soundNumber);
    }

    [PunRPC]
    void RpcPlaySupportActionSound() {
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

    }

    private void DecreaseRecoil()
    {
        // If the current camera rotation is not at its original pos before recoil, then decrease its recoil
        if (recoilTime > 0f)
        {
            recoilTime -= (RECOIL_ACCELERATION / weaponStats.recoveryConstant) * Time.deltaTime;
        }
    }

    void UpdateRecoil(bool increase)
    {
        if (increase)
        {
            if (recoilTime < MAX_RECOIL_TIME)
            {
                mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(weaponStats.recoil, 0f, 0f);
            }
        }
        else
        {
            if (recoilTime > 0f)
            {
                mouseLook.m_FpcCharacterVerticalTargetRot *= Quaternion.Euler(-weaponStats.recoil / weaponStats.recoveryConstant, 0f, 0f);
            }
        }
    }

    public void SetWeaponStats(WeaponStats ws) {
        weaponStats = ws;
        weaponMods = ws.GetComponent<WeaponMods>();
        fireTimer = ws.fireRate;
        playerActionScript.weaponSpeedModifier = ws.mobility/100f;
        if (playerActionScript.equipmentScript.gender == 'M') {
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
            } else if (weaponStats.category.Equals("Booster")) {
                isWieldingThrowable = false;
                isWieldingBooster = true;
            }
            firingMode = FireMode.Semi;
        } else {
            isWieldingThrowable = false;
            isWieldingBooster = false;
            if (weaponStats.category.Equals("Shotgun")) {
                shotMode = ShotMode.Burst;
                firingMode = FireMode.Semi;
            } else {
                shotMode = ShotMode.Single;
                if (weaponStats.category.Equals("Pistol")) {
                    firingMode = FireMode.Semi;
                }
            }
        }
    }

    public void SetReloadSpeed(float multipler = 1f) {
        animatorFpc.SetFloat("ReloadSpeed", weaponStats.defaultFpcReloadSpeed * multipler);
        weaponStats.weaponAnimator.SetFloat("ReloadSpeed", weaponStats.defaultWeaponReloadSpeed * multipler);
        weaponStats.weaponAnimator.SetFloat("CockingSpeed", weaponStats.defaultWeaponCockingSpeed * multipler);
    }

    public void SetFiringSpeed(float multiplier = 1f) {
        animatorFpc.SetFloat("FireSpeed", weaponStats.defaultFireSpeed * multiplier);
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
        if (weaponStats.category.Equals("Explosive")) {
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
    }

    void FireBooster() {
        if (fireTimer < weaponStats.fireRate)
        {
            return;
        }
        if (currentAmmo == 0) {
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) return;
        if (weaponStats.category.Equals("Booster")) {
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
    }

    public void UseSupportItem() {
        // If the item is a grenade, instantiate and launch the grenade
        if (weaponStats.category.Equals("Explosive")) {
            GameObject projectile = PhotonNetwork.Instantiate(InventoryScript.weaponCatalog[weaponStats.weaponName].prefabPath + "Projectile", weaponHolderFpc.transform.position, Quaternion.identity);
            projectile.transform.forward = weaponHolderFpc.transform.forward;
            projectile.GetComponent<ThrowableScript>().Launch(gameObject, camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
            // Reset fire timer and subtract ammo used
            currentAmmo--;
            fireTimer = 0.0f;
        } else if (weaponStats.category.Equals("Booster")) {
            // Reset fire timer and subtract ammo used
            BoosterScript boosterScript = weaponStats.GetComponentInChildren<BoosterScript>();
            boosterScript.UseBoosterItem(weaponStats.weaponName);
            currentAmmo--;
            fireTimer = 0.0f;
        }
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

    [PunRPC]
    void RpcCockGrenade(bool cocking) {
        GetComponentInChildren<ThrowableScript>().PlayPinSound();
        animator.SetBool("isCockingGrenade", cocking);
    }

    [PunRPC]
    void RpcUseBooster() {
        animator.SetTrigger("useBooster");
    }

}
