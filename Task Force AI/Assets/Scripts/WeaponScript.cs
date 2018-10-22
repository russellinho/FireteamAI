﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WeaponScript : MonoBehaviour {

	// Projectile spread constants
	public const float MAX_SPREAD = 0.35f;
	public const float SPREAD_ACCELERATION = 0.15f;
	public const float SPREAD_DECELERATION = 0.1f;

	// Projectile recoil constants


	public Animator gunAnimator;
	public AudioSource audioSource;

	public float range = 100f;
	public float spread = 0f;
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
	private PhotonView pView;

	// Use this for initialization
	void Start () {
		pView = GetComponent<PhotonView> ();
		currentBullets = bulletsPerMag;
		originalPos = originalTrans.localPosition;
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
		if (shootInput && !isReloading) {
			if (currentBullets > 0) {
				Fire ();
				IncreaseSpread ();
			} else if (totalBulletsLeft > 0) {
				ReloadAction ();
			}
		} else {
			DecreaseSpread ();
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
		Debug.Log ("xSpread: " + xSpread + " ySpread: " + ySpread);
		Vector3 impactDir = new Vector3 (shootPoint.transform.forward.x + xSpread, shootPoint.transform.forward.y + ySpread, shootPoint.transform.forward.z + zSpread);
		if (Physics.Raycast (shootPoint.position, impactDir, out hit, range)) {
			GameObject bloodSpill = null;
			if (hit.transform.tag.Equals ("Human")) {
				pView.RPC ("RpcInstantiateBloodSpill", RpcTarget.All, hit.point, hit.normal);
				hit.transform.gameObject.GetComponent<BetaEnemyScript> ().TakeDamage((int)damage);
			} else {
				pView.RPC ("RpcInstantiateHitParticleEffect", RpcTarget.All, hit.point, hit.normal);
				pView.RPC ("RpcInstantiateBulletHole", RpcTarget.All, hit.point, hit.normal, hit.transform.gameObject.name);
			}
		}
			
		GameControllerTestScript.lastGunshotHeardPos = transform.position;
		pView.RPC ("FireEffects", RpcTarget.All);
	}

	[PunRPC]
	void RpcInstantiateBloodSpill(Vector3 point, Vector3 normal) {
		GameObject bloodSpill = Instantiate(bloodEffect, point, Quaternion.FromToRotation (Vector3.forward, normal));
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
	void RpcInstantiateHitParticleEffect(Vector3 point, Vector3 normal) {
		GameObject hitParticleEffect = Instantiate (hitParticles, point, Quaternion.FromToRotation (Vector3.up, normal));
		Destroy (hitParticleEffect, 1f);
	}

	[PunRPC]
	void FireEffects() {
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
		pView.RPC ("RpcPlayReloadSound", RpcTarget.All);
	}
		
	private void ReloadAction() {
		//AnimatorStateInfo info = gunAnimator.GetCurrentAnimatorStateInfo (0);
		if (isReloading)
			return;
		pView.RPC ("RpcReloadAnim", RpcTarget.All);
	}

	[PunRPC]
	void RpcReloadAnim() {
		gunAnimator.CrossFadeInFixedTime ("Reloading", 0.1f);
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

	public void IncreaseSpread() {
		if (spread < MAX_SPREAD) {
			spread += SPREAD_ACCELERATION * Time.deltaTime;
			if (spread > MAX_SPREAD) {
				spread = MAX_SPREAD;
			}
		}
	}

	public void DecreaseSpread() {
		if (spread > 0f) {
			spread -= SPREAD_DECELERATION * Time.deltaTime;
			if (spread < 0f) {
				spread = 0f;
			}
		}
	}
}
