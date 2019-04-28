using System.Collections;
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
    private Animator weaponAnimator;

    // Projectile spread constants
    public const float MAX_SPREAD = 0.15f;
    public const float SPREAD_ACCELERATION = 0.05f;
    public const float SPREAD_DECELERATION = 0.03f;

    // Projectile recoil constants
    public const float MAX_RECOIL_TIME = 1.4f;
    public const float RECOIL_ACCELERATION = 4.2f;
    public const float RECOIL_DECELERATION = 4.2f;

    // Projectile variables
    public float range = 100f;
    public float spread = 0f;
    private float recoil = 0.5f;
    private float recoilTime = 0f;
    private bool voidRecoilRecover = true;
    //private float recoilSlerp = 0f;
    
    public int bulletsPerMag = 30;
    public int totalBulletsLeft = 120;
    public int currentBullets;

    public Transform shootPoint;
    private ParticleSystem muzzleFlash;
    public ParticleSystem bulletTrace;
    private bool isReloading = false;
    public bool isCocking = false;
    public bool isAiming;

    public GameObject hitParticles;
    public GameObject bulletImpact;
    public GameObject bloodEffect;

    public float fireRate = 0.1f;
    public float damage = 20f;

    public enum FireMode { Auto, Semi }
    public FireMode firingMode;
    private bool shootInput;

    // Once it equals fireRate, it will allow us to shoot
    float fireTimer = 0.0f;

    // Aiming down sights
    public Transform camTransform;
    private Vector3 originalPosCam;
    private Vector3 aimPosCam;
    public Vector3 aimPosOffset;
    // Aiming speed
    public float aodSpeed = 8f;
    public PhotonView pView;

    // Use this for initialization
    void Start()
    {
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        currentBullets = bulletsPerMag;

        originalPosCam = camTransform.localPosition;

        aimPosCam = new Vector3(originalPosCam.x - aimPosOffset.x, originalPosCam.y - aimPosOffset.y, originalPosCam.z - aimPosOffset.z);

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

        RefillFireTimer();

        if (!playerActionScript.canShoot)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!playerActionScript.fpc.m_IsRunning && currentBullets < bulletsPerMag && totalBulletsLeft > 0)
            {
                ReloadAction();
            }
        }

        AimDownSights();
    }

    public void RefillFireTimer()
    {
        if (fireTimer < fireRate)
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
         if (animator.gameObject.activeSelf)
         {
             AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
             isReloading = info.IsName("Reload") || info.IsName("ReloadCrouch");
         }
        // Shooting mechanics
        if (shootInput && !isReloading && playerActionScript.canShoot)
        {
            if (currentBullets > 0)
            {
                Fire();
                cameraShakeScript.SetShake(true);
                IncreaseSpread();
                voidRecoilRecover = false;
                IncreaseRecoil();
                UpdateRecoil(true);
            }
            else if (totalBulletsLeft > 0)
            {
                cameraShakeScript.SetShake(false);
                ReloadAction();
            }
        }
        else
        {
            DecreaseSpread();
            DecreaseRecoil();
            UpdateRecoil(false);
            cameraShakeScript.SetShake(false);
            if (CrossPlatformInputManager.GetAxis ("Mouse X") == 0 && CrossPlatformInputManager.GetAxis ("Mouse Y") == 0 && !voidRecoilRecover) {
                DecreaseRecoil ();
                UpdateRecoil (false);
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
                camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, aimPosCam, Time.deltaTime * aodSpeed);
                camTransform.GetComponent<Camera>().nearClipPlane = 0.089f;
            }
            else
            {
                isAiming = false;
                animator.speed = 1f;
                camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, originalPosCam, Time.deltaTime * aodSpeed);
                camTransform.GetComponent<Camera>().nearClipPlane = 0.12f;
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
        if (fireTimer < fireRate || currentBullets < 0 || isReloading)
        {
            return;
        }

        RaycastHit hit;
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);
        float zSpread = Random.Range(-spread, spread);
        Vector3 impactDir = new Vector3(shootPoint.transform.forward.x + xSpread, shootPoint.transform.forward.y + ySpread, shootPoint.transform.forward.z + zSpread);
        int headshotLayer = (1 << 13);
        if (Physics.Raycast(shootPoint.position, impactDir, out hit, range, headshotLayer))
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
        else if (Physics.Raycast(shootPoint.position, impactDir, out hit, range))
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
                    hit.transform.gameObject.GetComponent<BetaEnemyScript>().TakeDamage((int)damage);
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

       // playerActionScript.gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
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

    [PunRPC]
    void FireEffects()
    {
        if (muzzleFlash == null) {
            muzzleFlash = weaponHolder.GetComponentInChildren<ParticleSystem>();
        }
        muzzleFlash.Play();
        if (!bulletTrace.isPlaying && !pView.IsMine)
        {
            bulletTrace.Play();
        }
        PlayShootSound();
        currentBullets--;
        // Reset fire timer
        fireTimer = 0.0f;
    }

    void FireEffects2()
    {
        if (muzzleFlash == null) {
            muzzleFlash = weaponHolder.GetComponentInChildren<ParticleSystem>();
        }
        muzzleFlash.Play();
        if (!bulletTrace.isPlaying)
        {
            bulletTrace.Play();
        }
        PlayShootSound();
        currentBullets--;
        // Reset fire timer
        fireTimer = 0.0f;
    }

    public void Reload()
    {
        if (!isCocking)
        {
            if (totalBulletsLeft <= 0)
                return;

            int bulletsToLoad = bulletsPerMag - currentBullets;
            int bulletsToDeduct = (totalBulletsLeft >= bulletsToLoad) ? bulletsToLoad : totalBulletsLeft;
            totalBulletsLeft -= bulletsToDeduct;
            currentBullets += bulletsToDeduct;
        }
        pView.RPC("RpcPlayReloadSound", RpcTarget.All);
    }

    private void ReloadAction()
    {
        //AnimatorStateInfo info = weaponAnimator.GetCurrentAnimatorStateInfo (0);
        if (isReloading)
            return;

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
            animator.CrossFadeInFixedTime("ReloadCrouch", 0.1f);
        } else {
            animator.CrossFadeInFixedTime("Reload", 0.1f);
        }
    }

    [PunRPC]
    void RpcCockingAnim()
    {
        if (fpc.m_IsCrouching)
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
        if (reloadSound == null) {
            reloadSound = weaponHolder.GetComponentsInChildren<AudioSource>()[1];
        }
        reloadSound.Play();
    }

    void PlayReloadSound()
    {
        if (reloadSound == null)
        {
            reloadSound = weaponHolder.GetComponentsInChildren<AudioSource>()[1];
        }
        reloadSound.Play();
    }

    private void PlayShootSound()
    {
        if (weaponSound == null) {
            weaponSound = weaponHolder.GetComponentsInChildren<AudioSource>()[0];
        }
        weaponSound.Play();
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
            recoilTime -= RECOIL_DECELERATION * Time.deltaTime;
        }
    }

    void UpdateRecoil(bool increase)
    {
        if (increase)
        {
            if (recoilTime < MAX_RECOIL_TIME)
            {
                mouseLook.m_SpineTargetRot *= Quaternion.Euler(-recoil, 0f, 0f);
            }
        }
        else
        {
            if (recoilTime > 0f)
            {
                mouseLook.m_SpineTargetRot *= Quaternion.Euler(recoil, 0f, 0f);
            }
        }
    }

}
