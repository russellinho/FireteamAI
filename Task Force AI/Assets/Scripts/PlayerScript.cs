﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PlayerScript : MonoBehaviourPunCallbacks {

	// Object references
	public GameObject gameController;
	private CharacterController charController;
	private PhotonView photonView;
	public GameObject fpsHands;
	private WeaponScript wepScript;
	private AudioSource aud;
	public Camera viewCam;
	public Transform bodyTrans;

	// Player variables
	public string currWep; // TODO: Needs to be changed soon to account for other weps
	public int health;
	public bool isCrouching;
	public bool canShoot;
	private float charHeightOriginal;
	private float crouchSpeed;
	private bool escapeValueSent;

	public Transform fpcPosition;
	private float fpcPositionYOriginal;

	public Transform bodyScaleTrans;
	private float bodyScaleOriginal;

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
	public Vector3 hitLocation;

	// Mission references
	private GameObject currentBomb;
	private int currentBombIndex;
	private float bombDefuseCounter = 0f;

	// Use this for initialization
	void Start () {
		// Setting original positions for returning from crouching
		charController = GetComponent<CharacterController>();
		charHeightOriginal = charController.height;
		fpcPositionYOriginal = fpcPosition.localPosition.y;
		bodyScaleOriginal = bodyScaleTrans.lossyScale.y;
        photonView = GetComponent<PhotonView>();
		escapeValueSent = false;
		Physics.IgnoreLayerCollision (9, 12);

		// If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
        if (!GetComponent<PhotonView>().IsMine) {
			Destroy (GetComponentInChildren<AudioListener>());
			GetComponentInChildren<Camera> ().enabled = false;
			enabled = false;
			return;
		}

		wepScript = gameObject.GetComponent<WeaponScript> ();
		aud = GetComponent<AudioSource> ();

		// Initialize variables
		currWep = "AK-47";
		health = 100;
		isCrouching = false;
		canShoot = true;
		crouchSpeed = 3f;

		crouchPosY = 0.3f;
		crouchBodyPosY = 0.25f;
		crouchBodyScaleY = 0.66f;

		fraction = 0f;
		deathCameraLerpVar = 0f;
		rotationSaved = false;

		hitTimer = 1f;

	}

	// Update is called once per frame
	void Update () {
		if (gameController == null) {
			gameController = GameObject.Find ("GameController");
		}
        if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		Crouch ();
		BombCheck ();
		DeathCheck ();
		DetermineEscaped ();
	}

    public void TakeDamage(int d) {
        photonView.RPC("RpcTakeDamage", RpcTarget.AllBuffered, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d) {
        health -= d;
    }

	public void Crouch() {
		bool originalCrouch = isCrouching;
		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			isCrouching = !isCrouching;
		}

		// Collecting the original character height
		float h = charHeightOriginal;
		// Collect the original y position of the FPS controller since we're going to move it downwards to crouch
		float viewH = fpcPositionYOriginal;
		//float speed = charController.velocity;
		float bodyScale = bodyScaleTrans.lossyScale.y;

		if (isCrouching) {
			h = charHeightOriginal * .65f;
			viewH = .55f;
			bodyScale = .7f;
			//TODO: Change speed
			//TODO: Make it impossible to jump
			//TODO: Make it impossible to sprint
		} else {
			viewH = .8f;
			bodyScale = bodyScaleOriginal;
		}

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (lastHeight, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
		//Debug.Log (fpcPosition.position.y);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);

		if (isCrouching != originalCrouch) {
			photonView.RPC ("RpcCrouch", RpcTarget.OthersBuffered, isCrouching);
		}
	}

	[PunRPC]
	public void RpcCrouch(bool crouch) {
		isCrouching = crouch;
		float h = charHeightOriginal;
		float viewH = fpcPositionYOriginal;
		//float speed = charController.velocity;
		float bodyScale = bodyScaleTrans.lossyScale.y;

		if (isCrouching) {
			h = charHeightOriginal * .65f;
			viewH = .55f;
			bodyScale = .7f;
			//Change speed
			//Make it impossible to jump
			//Make it impossible to sprint
		} else {
			viewH = .8f;
			bodyScale = bodyScaleOriginal;
		}

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (charController.height, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
	}

	void DeathCheck() {
		if (health <= 0) {
			/**if (fpsHands.activeInHierarchy) {
				//fpsHands.SetActive (false);
			}*/
			GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().enabled = false;
			if (!rotationSaved) {
				DisableFPSHands ();
				deathCameraLerpPos = new Vector3 (viewCam.transform.localPosition.x, viewCam.transform.localPosition.y, viewCam.transform.localPosition.z - 5.5f);
				alivePosition = new Vector3 (0f, bodyTrans.eulerAngles.y, 0f);
				deadPosition = new Vector3 (-90f, bodyTrans.eulerAngles.y, 0f);
				rotationSaved = true;
			}
			if (bodyTrans.rotation.x > -90f) {
				fraction += Time.deltaTime * 8f;
				bodyTrans.rotation = Quaternion.Euler (Vector3.Lerp(alivePosition, deadPosition, fraction));
			}
			DeathCameraEffect ();
		}
	}

	// If map objective is defusing bombs, this method checks if the player is near any bombs
	void BombCheck() {
		if (gameController.GetComponent<GameControllerScript>().bombs == null) {
			return;
		}

		if (!currentBomb) {
			bool found = false;
			int count = 0;
			foreach (GameObject i in gameController.GetComponent<GameControllerScript>().bombs) {
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

		if (currentBomb != null) {
			// Check if the player is still near the bomb
			if (Vector3.Distance (gameObject.transform.position, currentBomb.transform.position) > 4.5f || currentBomb.GetComponent<BombScript>().defused) {
				currentBomb = null;
				GetComponent<PlayerHUDScript> ().hintText.enabled = false;
				return;
			}

			if (Input.GetKey (KeyCode.E)) {
				// TODO: Disallow movement
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = false;

				GetComponent<PlayerHUDScript> ().hintText.enabled = false;
				GetComponent<PlayerHUDScript> ().ToggleActionBar (true);
				GetComponent<PlayerHUDScript> ().defusingText.enabled = true;
				bombDefuseCounter += (Time.deltaTime / 8f);
				Debug.Log (bombDefuseCounter);
				GetComponent<PlayerHUDScript> ().SetActionBarSlider(bombDefuseCounter);
				if (bombDefuseCounter >= 1f) {
					bombDefuseCounter = 0f;

					gameController.GetComponent<GameControllerScript> ().bombs[currentBombIndex].GetComponent<BombScript>().Defuse ();
					gameController.GetComponent<GameControllerScript> ().bombsRemaining--;
					GetComponent<PlayerHUDScript> ().UpdateObjectives ();
					currentBomb = null;

					GetComponent<PlayerHUDScript> ().ToggleActionBar (false);
					GetComponent<PlayerHUDScript> ().defusingText.enabled = false;
					GetComponent<PlayerHUDScript> ().hintText.enabled = false;
					// Enable movement again
					gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;
					if (gameController.GetComponent<GameControllerScript> ().bombsRemaining == 0) {
						GetComponent<PlayerHUDScript> ().EscapePopup ();
					}
				}
			} else {
				// Enable movement again
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;

				GetComponent<PlayerHUDScript> ().ToggleActionBar (false);
				GetComponent<PlayerHUDScript> ().defusingText.enabled = false;
				GetComponent<PlayerHUDScript> ().hintText.enabled = true;
				//Debug.Log (gameController.GetComponent<GameControllerScript> ().hintText.enabled);
				bombDefuseCounter = 0f;
			}
		}
	}

	public void ResetHitTimer() {
		photonView.RPC ("RpcResetHitTimer", RpcTarget.AllBuffered);
	}

	[PunRPC]
	void RpcResetHitTimer() {
		hitTimer = 0f;
	}

	public void SetHitLocation(Vector3 pos) {
		photonView.RPC ("RpcSetHitLocation", RpcTarget.AllBuffered, pos);
	}

	[PunRPC]
	void RpcSetHitLocation(Vector3 pos) {
		hitLocation = pos;
	}

	void DetermineEscaped() {
		if (gameController.GetComponent<GameControllerScript> ().escapeAvailable) {
			if (!escapeValueSent) {
				escapeValueSent = true;
				// If dead, 
				if (health <= 0) {
					gameController.GetComponent<GameControllerScript> ().IncrementDeathCount ();
				} else if (health > 0 && Vector3.Distance(gameController.GetComponent<GameControllerScript>().exitPoint.transform.position, transform.position) <= 10f) {
					gameController.GetComponent<GameControllerScript> ().IncrementEscapeCount ();
				}
			}
		}
	}

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		escapeValueSent = false;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag.Equals ("AmmoBox")) {
			wepScript.totalBulletsLeft = 120 + (wepScript.bulletsPerMag - wepScript.currentBullets);
			other.gameObject.GetComponent<PickupScript> ().DestroyPickup ();
		} else if (other.gameObject.tag.Equals("HealthBox")) {
			health = 100;
			other.gameObject.GetComponent<PickupScript> ().DestroyPickup ();
		}
	}

	void DeathCameraEffect() {
		deathCameraLerpVar += (Time.deltaTime / 4f);
		viewCam.transform.localPosition = Vector3.Lerp (viewCam.transform.localPosition, deathCameraLerpPos, deathCameraLerpVar);
	}

	void DisableFPSHands() {
		viewCam.gameObject.GetComponentInChildren<SkinnedMeshRenderer> ().enabled = false;
		viewCam.gameObject.GetComponentInChildren<MeshRenderer> ().enabled = false;
	}

}
