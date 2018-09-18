using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {

	public Animator gunAnimator;
	public AudioSource audioSource;

	public float range = 100f;
	public int bulletsPerMag = 30;
	public int totalBulletsLeft = 120;
	public int currentBullets;
	public AudioClip shootSound;
	public AudioClip reloadSound;

	public Transform shootPoint;
	public ParticleSystem muzzleFlash;
	private bool isReloading = false;
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

	// Use this for initialization
	void Start () {
		//gunAnimator = GetComponent<Animator> ();
		currentBullets = bulletsPerMag;
		//audioSource = GetComponent<AudioSource> ();
		originalPos = originalTrans.localPosition;
		/**if (isServer) {
			networkMan = GameObject.Find ("NetworkMan").GetComponent<NetworkManager>();
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		/**if (!isLocalPlayer) {
			return;
		}*/
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
		if (shootInput) {
			if (currentBullets > 0) {
				/**if (!isServer) {
					CmdFire ();
				} else {
					if (networkMan.numPlayers >= 2) RpcFire ();
				}*/
				Fire ();
			} else if (totalBulletsLeft > 0) {
				ReloadAction ();
			}
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			if (currentBullets < bulletsPerMag && totalBulletsLeft > 0) {
				ReloadAction ();
			}
		}
		//if (!isServer) CmdRefillFireTimer ();
		RefillFireTimer ();
		AimDownSights ();
	}

	/**[Command]
	public void CmdRefillFireTimer() {
		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}
	}*/
		
	public void RefillFireTimer() {
		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}
	}

	void FixedUpdate() {
		/**if (!isLocalPlayer) {
			return;
		}*/
		AnimatorStateInfo info = gunAnimator.GetCurrentAnimatorStateInfo (0);
		isReloading = info.IsName ("Reloading");
		gunAnimator.SetBool ("Aim", isAiming);
	}

	/**[Command]
	public void CmdAimDownSights(Vector3 p) {
		originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, p, Time.deltaTime * aodSpeed);
	}*/

	public void AimDownSights() {
		if (Input.GetButton ("Fire2") && !isReloading) {
			originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, aimPos, Time.deltaTime * aodSpeed);
			isAiming = true;
		} else {
			originalTrans.localPosition = Vector3.Lerp (originalTrans.localPosition, originalPos, Time.deltaTime * aodSpeed);
			isAiming = false;
		}
	}

	/**[ClientRpc]
	public void RpcFire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading) {
			return;
		}

		RaycastHit hit;
		if (Physics.Raycast (shootPoint.position, shootPoint.transform.forward, out hit, range)) {
			
			GameObject bloodSpill = null;
			if (hit.transform.tag.Equals ("Human")) {
				bloodSpill = Instantiate (bloodEffect, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bloodSpill.transform.Rotate (180f, 0f, 0f);
				//hit.transform.gameObject.GetComponent<BetaEnemyScript> ().health -= (int)damage;
			} else {
				GameObject hitParticleEffect = Instantiate (hitParticles, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				GameObject bulletHoleEffect = Instantiate (bulletImpact, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bulletHoleEffect.transform.SetParent (hit.transform);
				Destroy (hitParticleEffect, 1f);
				Destroy (bulletHoleEffect, 3f);
			}
			if (bloodSpill != null)
				Destroy (bloodSpill, 1.5f);
		}

		gunAnimator.CrossFadeInFixedTime ("Firing", 0.01f);

		muzzleFlash.Play ();
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}*/

	/**[Command]
	public void CmdFire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading) {
			return;
		}

		RaycastHit hit;
		if (Physics.Raycast (shootPoint.position, shootPoint.transform.forward, out hit, range)) {
			
			GameObject bloodSpill = null;
			if (hit.transform.tag.Equals ("Human")) {
				bloodSpill = Instantiate (bloodEffect, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bloodSpill.transform.Rotate (180f, 0f, 0f);
				hit.transform.gameObject.GetComponent<BetaEnemyScript> ().health -= (int)damage;
			} else {
				GameObject hitParticleEffect = Instantiate (hitParticles, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				GameObject bulletHoleEffect = Instantiate (bulletImpact, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bulletHoleEffect.transform.SetParent (hit.transform);
				Destroy (hitParticleEffect, 1f);
				Destroy (bulletHoleEffect, 3f);
			}
			if (bloodSpill != null)
				Destroy (bloodSpill, 1.5f);
		}

		gunAnimator.CrossFadeInFixedTime ("Firing", 0.01f);

		muzzleFlash.Play ();
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}*/

	// Comment
	public void Fire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading) {
			return;
		}

		RaycastHit hit;
		if (Physics.Raycast (shootPoint.position, shootPoint.transform.forward, out hit, range)) {
			
			GameObject bloodSpill = null;
			if (hit.transform.tag.Equals ("Human")) {
				bloodSpill = Instantiate (bloodEffect, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bloodSpill.transform.Rotate (180f, 0f, 0f);
				//Debug.Log ("hes hit");
				hit.transform.gameObject.GetComponent<BetaEnemyScript> ().TakeDamage((int)damage);
			} else {
				GameObject hitParticleEffect = Instantiate (hitParticles, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				GameObject bulletHoleEffect = Instantiate (bulletImpact, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
				bulletHoleEffect.transform.SetParent (hit.transform);
				Destroy (hitParticleEffect, 1f);
				Destroy (bulletHoleEffect, 3f);
			}
			if (bloodSpill != null)
				Destroy (bloodSpill, 1.5f);
		}

		gunAnimator.CrossFadeInFixedTime ("Firing", 0.01f);

		muzzleFlash.Play ();
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}

	public void Reload() {
		if (totalBulletsLeft <= 0)
			return;

		int bulletsToLoad = bulletsPerMag - currentBullets;
		int bulletsToDeduct = (totalBulletsLeft >= bulletsToLoad) ? bulletsToLoad : totalBulletsLeft;
		totalBulletsLeft -= bulletsToDeduct;
		currentBullets += bulletsToDeduct;
		audioSource.PlayOneShot (reloadSound);
	}
		
	private void ReloadAction() {
		AnimatorStateInfo info = gunAnimator.GetCurrentAnimatorStateInfo (0);
		if (isReloading)
			return;
		gunAnimator.CrossFadeInFixedTime ("Reloading", 0.1f);
	}

	private void PlayShootSound() {
		audioSource.PlayOneShot (shootSound);
		//audioSource.clip = shootSound;
		//audioSource.Play ();
	}
}
