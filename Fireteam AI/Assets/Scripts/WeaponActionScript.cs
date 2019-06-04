﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

public class WeaponActionScript : MonoBehaviour
{

    public MouseLook mouseLook;
    public PlayerActionScript playerActionScript;
    public CameraShakeScript cameraShakeScript;
    public PlayerHUDScript hudScript;
    public AudioControllerScript audioController;
    public FirstPersonController fpc;
    public GameObject weaponHolder;
    private AudioSource weaponSound;
    private AudioSource reloadSound;
    public Animator animator;
    private WeaponStats weaponStats;
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
    //private float recoilSlerp = 0f;

    public int totalAmmoLeft;
    public int currentAmmo;

    public Transform shootPoint;
    public ParticleSystem bulletTrace;
    public bool isReloading = false;
    public bool isCocking = false;
    public bool isAiming;

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
    public float aodSpeed = 8f;
    public PhotonView pView;
    private bool isWieldingSupportItem;
    private bool isCockingGrenade;

    // Use this for initialization
    void Start()
    {
        isCockingGrenade = false;
        isWieldingSupportItem = false;
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        currentAmmo = weaponStats.clipCapacity;

        originalPosCam = camTransform.localPosition;

        originalPosCamSecondary = new Vector3(-0.13f, 0.11f, 0.04f);

        mouseLook = fpc.m_MouseLook;

        CockingAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (pView != null && !pView.IsMine)
        {
            return;
        }

        if (weaponStats.category.Equals("Shotgun")) {
            shotMode = ShotMode.Burst;
        } else {
            shotMode = ShotMode.Single;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (firingMode == FireMode.Semi)
                firingMode = FireMode.Auto;
            else
                firingMode = FireMode.Semi;
        }
        if (weaponStats.type.Equals("Support")) {
            isWieldingSupportItem = true;
        } else {
            isWieldingSupportItem = false;
        }
        if (weaponStats.category.Equals("Pistol") || weaponStats.category.Equals("Shotgun") || isWieldingSupportItem) {
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

        RefillFireTimer();

        if (!playerActionScript.canShoot || isWieldingSupportItem)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!playerActionScript.fpc.m_IsRunning && currentAmmo < weaponStats.clipCapacity && totalAmmoLeft > 0)
            {
                ReloadAction();
            }
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
                    FireBurst();
                }
                voidRecoilRecover = false;
            }
            else if (totalAmmoLeft > 0)
            {
                cameraShakeScript.SetShake(false);
                ReloadAction();
            }
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

            if (Input.GetButton("Fire2") && !isReloading)
            {
                isAiming = true;
                if (animator.GetInteger("Moving") == 0)
                {
                    animator.speed = 0f;
                }
                else
                {
                    animator.speed = 1f;
                }
                if (fpc.equipmentScript.gender == 'M') {
                    camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, weaponStats.aimDownSightPosMale, Time.deltaTime * aodSpeed);
                } else {
                    camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, weaponStats.aimDownSightPosFemale, Time.deltaTime * aodSpeed);
                }
                camTransform.GetComponent<Camera>().nearClipPlane = weaponStats.aimDownSightClipping;
            }
            else
            {
                isAiming = false;
                animator.speed = 1f;
                //if (animator.GetInteger("WeaponType") == 1) {
                camTransform.localPosition = Vector3.Slerp(camTransform.localPosition, originalPosCam, Time.deltaTime * aodSpeed);
                //} else if (animator.GetInteger("WeaponType") == 2) {
                  //  camTransform.localPosition = Vector3.Slerp(camTransform.localPosition, originalPosCamSecondary, Time.deltaTime * aodSpeed);
                //}
                camTransform.GetComponent<Camera>().nearClipPlane = 0.05f;
            }
        }
    }

    [PunRPC]
    void RpcAddToTotalKills()
    {
        playerActionScript.kills++;
        GameControllerScript.totalKills[pView.Owner.NickName]++;
    }

    // Comment
    public void Fire()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
        IncreaseSpread();
        IncreaseRecoil();
        UpdateRecoil(true);
        RaycastHit hit;
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);
        float zSpread = Random.Range(-spread, spread);
        Vector3 impactDir = new Vector3(shootPoint.transform.forward.x + xSpread, shootPoint.transform.forward.y + ySpread, shootPoint.transform.forward.z + zSpread);
        int headshotLayer = (1 << 13);
        if (Physics.Raycast(shootPoint.position, impactDir, out hit, weaponStats.range, headshotLayer))
        {
            pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
            if (hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().health > 0)
            {
                hudScript.InstantiateHitmarker();
                hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                pView.RPC("RpcAddToTotalKills", RpcTarget.All);
                hudScript.OnScreenEffect("HEADSHOT", true);
                audioController.PlayHeadshotSound();
            }
        }
        else if (Physics.Raycast(shootPoint.position, impactDir, out hit, weaponStats.range))
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
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().PainSound();
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().SetAlerted(true);
                    if (hit.transform.gameObject.GetComponent<BetaEnemyScript>().health <= 0 && beforeHp > 0)
                    {
                        pView.RPC("RpcAddToTotalKills", RpcTarget.All);
                        hudScript.OnScreenEffect(playerActionScript.kills + " KILLS", true);
                    }
                }
            }
            else
            {
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

    public void FireBurst ()
    {
        if (fireTimer < weaponStats.fireRate || currentAmmo <= 0 || isReloading)
        {
            return;
        }

        cameraShakeScript.SetShake(true);
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
            Vector3 impactDir = new Vector3(shootPoint.transform.forward.x + xSpread, shootPoint.transform.forward.y + ySpread, shootPoint.transform.forward.z + zSpread);
            int headshotLayer = (1 << 13);
            if (Physics.Raycast(shootPoint.position, impactDir, out hit, weaponStats.range, headshotLayer) && !headshotDetected)
            {
                pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
                if (hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().health > 0)
                {
                    hudScript.InstantiateHitmarker();
                    hit.transform.gameObject.GetComponentInParent<BetaEnemyScript>().TakeDamage(100);
                    pView.RPC("RpcAddToTotalKills", RpcTarget.All);
                    hudScript.OnScreenEffect("HEADSHOT", true);
                    audioController.PlayHeadshotSound();
                }
                headshotDetected = true;
            }
            else if (Physics.Raycast(shootPoint.position, impactDir, out hit, weaponStats.range))
            {
                int beforeHp = 0;
                GameObject bloodSpill = null;
                if (hit.transform.tag.Equals("Human"))
                {
                    pView.RPC("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
                    beforeHp = hit.transform.gameObject.GetComponent<BetaEnemyScript>().health;
                    if (totalDamageDealt == 0f) {
                        if (beforeHp > 0)
                        {
                            hudScript.InstantiateHitmarker();
                            audioController.PlayHitmarkerSound();
                            //hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)weaponStats.damage);
                            hit.transform.gameObject.GetComponent<BetaEnemyScript>().PainSound();
                            hit.transform.gameObject.GetComponent<BetaEnemyScript>().SetAlerted(true);
                        }
                    }
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)(weaponStats.damage / 8f));
                    if (hit.transform.gameObject.GetComponent<BetaEnemyScript>().health <= 0 && beforeHp > 0)
                    {
                        pView.RPC("RpcAddToTotalKills", RpcTarget.All);
                        hudScript.OnScreenEffect(playerActionScript.kills + " KILLS", true);
                    }
                    totalDamageDealt += (weaponStats.damage / 8f);
                }
                else
                {
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
        GameObject gunSmokeEffect = Instantiate(weaponStats.gunSmoke, shootPoint.position, Quaternion.identity);
        Destroy(gunSmokeEffect, 1.5f);
    }

    void PlayMuzzleFlash() {
        weaponStats.muzzleFlash.Play();
    }

    [PunRPC]
    void FireEffects()
    {
        PlayMuzzleFlash();
        InstantiateGunSmokeEffect();
        if (!bulletTrace.isPlaying && !pView.IsMine)
        {
            bulletTrace.Play();
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
        if (!bulletTrace.isPlaying)
        {
            bulletTrace.Play();
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
        pView.RPC("RpcPlayReloadSound", RpcTarget.All);
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
            pView.RPC("RpcCockingAnim", RpcTarget.All);
        }
        else
        {
            pView.RPC("RpcReloadAnim", RpcTarget.All);
        }
    }

    public void CockingAction()
    {
        isCocking = true;
        ReloadAction();
    }

    [PunRPC]
    void RpcReloadAnim()
    {
        if (fpc.m_IsCrouching) {
            //animator.CrossFadeInFixedTime("ReloadCrouch", 0.1f);
            animator.SetTrigger("Reloading");
        } else {
            //animator.CrossFadeInFixedTime("Reload", 0.1f);
            animator.SetTrigger("Reloading");
        }
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

    [PunRPC]
    void RpcPlayReloadSound()
    {
        weaponStats.reloadSound.Play();
    }

    void PlayReloadSound()
    {
        weaponStats.reloadSound.Play();
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
                mouseLook.m_SpineTargetRot *= Quaternion.Euler(-weaponStats.recoil, 0f, 0f);
            }
        }
        else
        {
            if (recoilTime > 0f)
            {
                mouseLook.m_SpineTargetRot *= Quaternion.Euler(weaponStats.recoil / weaponStats.recoveryConstant, 0f, 0f);
            }
        }
    }

    public void SetWeaponStats(WeaponStats ws) {
        weaponStats = ws;
        weaponMods = ws.GetComponent<WeaponMods>();
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
            return;
        }
        if (currentAmmo == 0) {
            ReloadSupportItem();
        }
        if (currentAmmo <= 0) return;
        if (weaponStats.category.Equals("Explosive")) {
            if (!isCockingGrenade && isWieldingSupportItem && (Input.GetButtonDown("Fire1") || Input.GetButton("Fire1"))) {
                isCockingGrenade = true;
                pView.RPC("RpcCockGrenade", RpcTarget.All, isCockingGrenade);
                return;
            }
            if (isCockingGrenade && Input.GetButtonUp("Fire1")) {
                isCockingGrenade = false;
                pView.RPC("RpcCockGrenade", RpcTarget.All, isCockingGrenade);
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
            if (isWieldingSupportItem && Input.GetButtonDown("Fire1")) {
                pView.RPC("RpcUseBooster", RpcTarget.All);

            }
        }


        }

    public void UseSupportItem() {
        // If the item is a grenade, instantiate and launch the grenade
        if (weaponStats.category.Equals("Explosive")) {
            GameObject projectile = PhotonNetwork.Instantiate(InventoryScript.weaponCatalog[weaponStats.weaponName].prefabPath, weaponHolder.transform.position, Quaternion.identity);
            projectile.transform.forward = weaponHolder.transform.forward;
            projectile.GetComponent<ThrowableScript>().Launch(camTransform.forward.x, camTransform.forward.y, camTransform.forward.z);
            // Reset fire timer and subtract ammo used
            currentAmmo--;
            fireTimer = 0.0f;
        }
        else if (weaponStats.category.Equals("Booster")) {
            // Reset fire timer and subtract ammo used
            BoosterScript boosterScript = GetComponentInChildren<BoosterScript>();
            boosterScript.UseBoosterItem(weaponStats.weaponName);
            currentAmmo--;
            fireTimer = 0.0f;
        }
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
