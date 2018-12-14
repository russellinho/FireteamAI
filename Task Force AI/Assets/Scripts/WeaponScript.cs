using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

public class WeaponScript : MonoBehaviour {

	private MouseLook mouseLook;

	// Projectile spread constants
	public const float MAX_SPREAD = 0.35f;
	public const float SPREAD_ACCELERATION = 0.15f;
	public const float SPREAD_DECELERATION = 0.1f;

	// Projectile recoil constants
	public const float MAX_RECOIL_TIME = 1.4f;
	public const float RECOIL_ACCELERATION = 4.2f;
	public const float RECOIL_DECELERATION = 4.2f;

	// Projectile variables
	public float range = 100f;
	public float spread = 0f;
	//private Quaternion targetGunRot;
	//private Quaternion originalGunRot;
	private float recoil = 0.5f;
	private float recoilTime = 0f;
	private bool voidRecoilRecover = true;
	//private float recoilSlerp = 0f;

	public Animator gunAnimator;
	public AudioSource audioSource;

	public int bulletsPerMag = 30;
	public int totalBulletsLeft = 120;
	public int currentBullets;
	public AudioClip shootSound;
	public AudioClip reloadSound;

	public Transform shootPoint;
	public ParticleSystem muzzleFlash;
	public ParticleSystem bulletTrace;
	private bool isReloading = false;
	public bool isCocking = false;
	private bool isAiming;

	public GameObject hitParticles;
	public GameObject bulletImpact;
	public GameObject bloodEffect;

	public float fireRate = 0.1f;
	public float damage = 20f;

	public enum FireMode {Auto, Semi}
	public FireMode firingMode;
	private bool shootInput;

	// Once it equals fireRate, it will allow us to shoot
	float fireTimer = 0.0f;

	// Aiming down sights
	public Transform originalTrans;
	private Vector3 originalPos;
	public Vector3 aimPos;
	// Aiming speed
	public float aodSpeed = 8f;
	private PhotonView pView;

	// Use this for initialization
	void Start () {
		pView = GetComponent<PhotonView> ();
		currentBullets = bulletsPerMag;
		originalPos = originalTrans.localPosition;

		mouseLook = GetComponent<FirstPersonController> ().m_MouseLook;
		//targetGunRot = mouseLook.m_CameraTargetRot * Quaternion.Euler(-MAX_RECOIL, 0f, 0f);
		//originalGunRot = new Quaternion(mouseLook.m_CameraTargetRot.x, mouseLook.m_CameraTargetRot.y, mouseLook.m_CameraTargetRot.z, mouseLook.m_CameraTargetRot.w);
		//originalGunRot = mouseLook.m_CameraTargetRot;

		CockingAction ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!pView.IsMine) {
			return;
		}
			
		if (Input.GetKeyDown (KeyCode.Q)) {
			if (firingMode == FireMode.Semi)
				firingMode = FireMode.Auto;
			else
				firingMode = FireMode.Semi;
		}
		switch (firingMode) {
		case FireMode.Auto:
			shootInput = Input.GetButton ("Fire1");
			break;
		case FireMode.Semi:
			shootInput = Input.GetButtonDown ("Fire1");
			break;
		}
		if (!GetComponent<PlayerScript> ().canShoot) {
			return;
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			if (currentBullets < bulletsPerMag && totalBulletsLeft > 0) {
				ReloadAction ();
			}
		}
		RefillFireTimer ();
		AimDownSights ();
	}
		
	public void RefillFireTimer() {
		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}
	}

	void FixedUpdate() {
		if (!pView.IsMine || GetComponent<PlayerScript>().health <= 0) {
			return;
		}
		if (gunAnimator.gameObject.activeSelf) {
			AnimatorStateInfo info = gunAnimator.GetCurrentAnimatorStateInfo (0);
			isReloading = info.IsName ("Reloading");
			gunAnimator.SetBool ("Aim", isAiming);
		}
		// Shooting mechanics
		if (shootInput && !isReloading) {
			if (currentBullets > 0) {
				Fire ();
				GetComponentInParent<CameraShakeScript> ().SetShake (true);
				IncreaseSpread ();
				voidRecoilRecover = false;
				IncreaseRecoil ();
				UpdateRecoil (true);
			} else if (totalBulletsLeft > 0) {
                GetComponentInParent<CameraShakeScript>().SetShake(false);
                ReloadAction ();
			}
		} else {
			DecreaseSpread ();
			DecreaseRecoil ();
			UpdateRecoil (false);
			GetComponentInParent<CameraShakeScript> ().SetShake (false);
			if (bulletTrace.isPlaying) {
                pView.RPC ("StopFireEffects", RpcTarget.Others);
			}
			/**if (CrossPlatformInputManager.GetAxis ("Mouse X") == 0 && CrossPlatformInputManager.GetAxis ("Mouse Y") == 0 && !voidRecoilRecover) {
				DecreaseRecoil ();
				UpdateRecoil (false);
			} else {
				voidRecoilRecover = true;
				recoilTime = 0f;
			}*/
		}
	}

	public void AimDownSights() {
		if (Input.GetButton ("Fire2") && !isReloading) {
			originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, aimPos, Time.deltaTime * aodSpeed);
			isAiming = true;
		} else {
			originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, originalPos, Time.deltaTime * aodSpeed);
			isAiming = false;
		}
	}

	// Comment
	public void Fire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading) {
			return;
		}

		RaycastHit hit;
		float xSpread = Random.Range (-spread, spread);
		float ySpread = Random.Range (-spread, spread);
		float zSpread = Random.Range (-spread, spread);
		//Debug.Log ("xSpread: " + xSpread + " ySpread: " + ySpread);
		Vector3 impactDir = new Vector3 (shootPoint.transform.forward.x + xSpread, shootPoint.transform.forward.y + ySpread, shootPoint.transform.forward.z + zSpread);
		int headshotLayer = (1 << 13);
		if (Physics.Raycast (shootPoint.position, impactDir, out hit, range, headshotLayer)) {
			pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, true);
			if (hit.transform.gameObject.GetComponentInParent<BetaEnemyScript> ().health > 0) {
				GetComponentInParent<PlayerHUDScript> ().InstantiateHitmarker ();
				hit.transform.gameObject.GetComponentInParent<BetaEnemyScript> ().TakeDamage (100);
				GetComponentInParent<PlayerScript> ().kills++;
				GetComponentInParent<PlayerHUDScript> ().OnScreenEffect ("HEADSHOT", true);
				GetComponentInParent<PlayerScript> ().gameController.GetComponent<GameControllerScript> ().PlayHeadshotSound ();
			}
		} else if (Physics.Raycast (shootPoint.position, impactDir, out hit, range)) {
			GameObject bloodSpill = null;
			if (hit.transform.tag.Equals ("Human")) {
				pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal, false);
				int beforeHp = hit.transform.gameObject.GetComponent<BetaEnemyScript> ().health;
				if (beforeHp > 0) {
					GetComponentInParent<PlayerHUDScript> ().InstantiateHitmarker ();
					GetComponentInParent<PlayerScript> ().gameController.GetComponent<GameControllerScript> ().PlayHitmarkerSound ();
					hit.transform.gameObject.GetComponent<BetaEnemyScript> ().TakeDamage ((int)damage);
					hit.transform.gameObject.GetComponent<BetaEnemyScript> ().SetAlerted (true);
					if (hit.transform.gameObject.GetComponent<BetaEnemyScript> ().health <= 0 && beforeHp > 0) {
						GetComponentInParent<PlayerScript> ().kills++;
						GetComponentInParent<PlayerHUDScript> ().OnScreenEffect (GetComponentInParent<PlayerScript> ().kills + " KILLS", true);
					}
				}
			} else {
				pView.RPC ("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
				pView.RPC ("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
			}
		}

		GetComponentInParent<PlayerScript> ().gameController.GetComponent<GameControllerScript> ().SetLastGunshotHeardPos (transform.position.x, transform.position.y, transform.position.z);
		//GameControllerScript.lastGunshotHeardPos = transform.position;
		pView.RPC ("FireEffects", RpcTarget.All);
	}

	[PunRPC]
	void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal, bool headshot) {
		GameObject bloodSpill;
		if (headshot) {
			bloodEffect = (GameObject)Resources.Load ("BloodEffectHeadshot");
		} else {
			bloodEffect = (GameObject)Resources.Load ("BloodEffect");
		}
		bloodSpill = Instantiate (bloodEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bloodSpill.transform.Rotate (180f, 0f, 0f);
		Destroy (bloodSpill, 1.5f);
	}

	[PunRPC]
	void RpcInstantiateBulletHole(Vector3 point, Vector3 normal, string parentName) {
		GameObject bulletHoleEffect = Instantiate (bulletImpact, point, Quaternion.FromToRotation (Vector3.forward, normal));
		bulletHoleEffect.transform.SetParent (GameObject.Find(parentName).transform);
		Destroy (bulletHoleEffect, 3f);
	}

	[PunRPC]
	void StopFireEffects() {
		bulletTrace.Stop ();
	}

	[PunRPC]
	void RpcInstantiateHitParticleEffect(Vector3 point, Vector3 normal) {
		GameObject hitParticleEffect = Instantiate (hitParticles, point, Quaternion.FromToRotation (Vector3.up, normal));
		Destroy (hitParticleEffect, 1f);
	}

	[PunRPC]
	void FireEffects() {
		gunAnimator.CrossFadeInFixedTime ("Firing", 0.01f);
		muzzleFlash.Play ();
        if (!bulletTrace.isPlaying && !pView.IsMine) {
			bulletTrace.Play ();
		}
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}

	public void Reload() {
		if (!isCocking) {
			if (totalBulletsLeft <= 0)
				return;

			int bulletsToLoad = bulletsPerMag - currentBullets;
			int bulletsToDeduct = (totalBulletsLeft >= bulletsToLoad) ? bulletsToLoad : totalBulletsLeft;
			totalBulletsLeft -= bulletsToDeduct;
			currentBullets += bulletsToDeduct;
		}
		pView.RPC ("RpcPlayReloadSound", RpcTarget.All);
	}
		
	private void ReloadAction() {
		//AnimatorStateInfo info = gunAnimator.GetCurrentAnimatorStateInfo (0);
		if (isReloading)
			return;
		
		if (isCocking) {
			pView.RPC ("RpcCockingAnim", RpcTarget.All);
		} else {
			pView.RPC ("RpcReloadAnim", RpcTarget.All);
		}
	}

	public void CockingAction() {
		isCocking = true;
		ReloadAction ();
	}

	[PunRPC]
	void RpcReloadAnim() {
		gunAnimator.CrossFadeInFixedTime ("Reloading", 0.1f);
	}

	[PunRPC]
	void RpcCockingAnim() {
		gunAnimator.CrossFadeInFixedTime ("Reloading", 0.1f, -1, 3.1f);
	}

	[PunRPC]
	void RpcPlayReloadSound() {
		audioSource.PlayOneShot (reloadSound);
	}

	private void PlayShootSound() {
		audioSource.PlayOneShot (shootSound);
		//audioSource.clip = shootSound;
		//audioSource.Play ();
	}

	private void IncreaseSpread() {
		if (spread < MAX_SPREAD) {
			spread += SPREAD_ACCELERATION * Time.deltaTime;
			if (spread > MAX_SPREAD) {
				spread = MAX_SPREAD;
			}
		}
	}

	private void DecreaseSpread() {
		if (spread > 0f) {
			spread -= SPREAD_DECELERATION * Time.deltaTime;
			if (spread < 0f) {
				spread = 0f;
			}
		}
	}

	private void IncreaseRecoil()
	{
		// If the current camera rotation is not at its maximum recoil, then increase its recoil
		if (recoilTime < MAX_RECOIL_TIME) {
			recoilTime += RECOIL_ACCELERATION * Time.deltaTime;
		}

	}

	private void DecreaseRecoil() {
		// If the current camera rotation is not at its original pos before recoil, then decrease its recoil
		if (recoilTime > 0f) {
			recoilTime -= RECOIL_DECELERATION * Time.deltaTime;
		}
	}

	void UpdateRecoil(bool increase) {
		if (increase) {
			if (recoilTime < MAX_RECOIL_TIME) {
				mouseLook.m_CameraTargetRot *= Quaternion.Euler (-recoil, 0f, 0f);
			}
		} else {
			if (recoilTime > 0f) {
				mouseLook.m_CameraTargetRot *= Quaternion.Euler (recoil, 0f, 0f);
			}
		}
	}

}
