﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerScript : MonoBehaviourPunCallbacks {

	// Object references
	public GameObject gameController;
	private AudioControllerScript audioController;
	private CharacterController charController;
	public GameObject fpsHands;
	private WeaponScript wepScript;
	private AudioSource aud;
	public Camera viewCam;
	public Transform bodyTrans;
	public GameObject spectatorCam;
	private GameObject thisSpectatorCam;
	private PlayerHUDScript hud;

	// Player variables
	public string currWep; // TODO: Needs to be changed soon to account for other weps
	public int health;
    public bool godMode;
	public bool canShoot;
	private float charHeightOriginal;
	public bool escapeValueSent;
	private bool assaultModeChangedIndicator;
	public int kills;
	private int deaths;
	public bool isRespawning;
	public float respawnTimer;
	private bool escapeAvailablePopup;

	public GameObject[] subComponents;
	public FirstPersonController fpc;

	public Transform fpcPosition;
	private float fpcPositionYOriginal;

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
	public float healTimer;
	public Vector3 hitLocation;

	// Mission references
	private GameObject currentBomb;
	private int currentBombIndex;
	private float bombDefuseCounter = 0f;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
		AddMyselfToPlayerList ();
		audioController = GetComponent<AudioControllerScript> ();

		// Setting original positions for returning from crouching
		charController = GetComponent<CharacterController>();
		charHeightOriginal = charController.height;
		fpcPositionYOriginal = fpcPosition.localPosition.y;
		bodyScaleOriginal = bodyTrans.lossyScale.y;
		escapeValueSent = false;
		assaultModeChangedIndicator = false;

		health = 100;
		kills = 0;
		deaths = 0;

		// If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
        if (!GetComponent<PhotonView>().IsMine) {
			subComponents[2].SetActive (false);
			subComponents[3].SetActive (false);
			Destroy (GetComponentInChildren<AudioListener>());
			GetComponentInChildren<Camera> ().enabled = false;
			//enabled = false;
			return;
		}

		gameController = GameObject.FindWithTag("GameController");

		photonView.RPC ("SyncPlayerColor", RpcTarget.All, PlayerData.playerdata.color);
		wepScript = gameObject.GetComponent<WeaponScript> ();
		aud = GetComponent<AudioSource> ();
		hud = GetComponent<PlayerHUDScript> ();

		// Initialize variables
		currWep = "AK-47";
		canShoot = true;

		crouchPosY = 0.3f;
		crouchBodyPosY = 0.25f;
		crouchBodyScaleY = 0.66f;

		fraction = 0f;
		deathCameraLerpVar = 0f;
		rotationSaved = false;

		hitTimer = 1f;
		healTimer = 1f;
		isRespawning = false;
		respawnTimer = 0f;
		escapeAvailablePopup = false;

	}

	// Update is called once per frame
	void Update () {
		if (gameController == null) {
			gameController = GameObject.FindWithTag ("GameController");
		}

        if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		// Instant respawn hack
		if (Input.GetKeyDown (KeyCode.P)) {
			BeginRespawn ();
		}

		if (gameController.GetComponent<GameControllerScript>().sectorsCleared == 0 && gameController.GetComponent<GameControllerScript> ().bombsRemaining == 2) {
			gameController.GetComponent<GameControllerScript> ().sectorsCleared++;
			hud.OnScreenEffect ("SECTOR CLEARED!", false);
			BeginRespawn ();
		}

		if (gameController.GetComponent<GameControllerScript> ().bombsRemaining == 0 && !escapeAvailablePopup) {
			escapeAvailablePopup = true;
			hud.MessagePopup ("Escape available! Head to the waypoint!");
			hud.ComBoxPopup (2f, "Well done. There's an extraction waiting for you on the top of the construction site. Democko signing out.");
		}

		// Update assault mode
		hud.UpdateAssaultModeIndHud (gameController.GetComponent<GameControllerScript>().assaultMode);
			
		// On assault mode changed
		bool h = gameController.GetComponent<GameControllerScript> ().assaultMode;
		if (h != assaultModeChangedIndicator) {
			assaultModeChangedIndicator = h;
			hud.MessagePopup ("Your cover is blown!");
			hud.ComBoxPopup (2f, "They know you're here! Slot the bastards!");
			hud.ComBoxPopup (20f, "Cicadas on the rooftops! Watch the rooftops!");
		}

		if (health > 0 && fpc.enabled && fpc.m_IsRunning) {
			audioController.PlaySprintSound (true);
			canShoot = false;
		} else {
			audioController.PlaySprintSound (false);
			canShoot = true;
		}

		DeathCheck ();
		if (health <= 0) {
			if (!escapeValueSent) {
				escapeValueSent = true;
				gameController.GetComponent<GameControllerScript> ().IncrementDeathCount ();
			}
		} else {
			BombCheck ();
		}

		if (fpc.enabled && fpc.canMove) {
			Crouch ();
		}
		DetermineEscaped ();
		RespawnRoutine ();

	}

	void AddMyselfToPlayerList() {
		GameControllerScript.playerList.Add(photonView.OwnerActorNr, gameObject);
		GameControllerScript.totalKills.Add (photonView.Owner.NickName, kills);
		GameControllerScript.totalDeaths.Add (photonView.Owner.NickName, deaths);
	}

	public void TakeDamage(int d) {
        photonView.RPC("RpcTakeDamage", RpcTarget.All, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d) {
		audioController.PlayGruntSound ();
		if (photonView.IsMine) {
			audioController.PlayHitSound ();
		}
        if (!godMode)
        {
            health -= d;
        }
    }

	public void Crouch() {
        bool originalCrouch = fpc.m_IsCrouching;
		if (Input.GetKeyDown (KeyCode.LeftControl)) {
            fpc.m_IsCrouching = !fpc.m_IsCrouching;
		}

		// Collecting the original character height
		float h = charHeightOriginal;
		// Collect the original y position of the FPS controller since we're going to move it downwards to crouch
		float viewH = fpcPositionYOriginal;
		//float speed = charController.velocity;
		float bodyScale = bodyTrans.lossyScale.y;

		if (fpc.m_IsCrouching) {
			h = charHeightOriginal * .65f;
			viewH = .55f;
			bodyScale = .7f;
		} else {
			viewH = .8f;
			bodyScale = bodyScaleOriginal;
		}

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (lastHeight, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyTrans.localScale = new Vector3 (bodyTrans.localScale.x, bodyScale, bodyTrans.localScale.z);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);

		if (fpc.m_IsCrouching != originalCrouch) {
			photonView.RPC ("RpcCrouch", RpcTarget.Others, fpc.m_IsCrouching);
		}
	}

	[PunRPC]
	public void RpcCrouch(bool crouch) {
        fpc.m_IsCrouching = crouch;
		float h = charHeightOriginal;
		float viewH = fpcPositionYOriginal;
		//float speed = charController.velocity;
		float bodyScale = bodyTrans.lossyScale.y;

		if (fpc.m_IsCrouching) {
			h = charHeightOriginal * .65f;
			viewH = .55f;
			bodyScale = .7f;
		} else {
			viewH = .8f;
			bodyScale = bodyScaleOriginal;
		}

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (charController.height, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyTrans.localScale = new Vector3 (bodyTrans.localScale.x, bodyScale, bodyTrans.localScale.z);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
	}

	void DeathCheck() {
		if (health <= 0) {
			/**if (fpsHands.activeInHierarchy) {
				//fpsHands.SetActive (false);
			}*/
			fpc.enabled = false;
			if (!rotationSaved) {
				if (escapeValueSent) {
					gameController.GetComponent<GameControllerScript> ().ConvertCounts (1, -1);
				}
				photonView.RPC ("RpcToggleFPSHands", RpcTarget.All, false);
				hud.ToggleHUD (false);
				deathCameraLerpPos = new Vector3 (viewCam.transform.localPosition.x, viewCam.transform.localPosition.y, viewCam.transform.localPosition.z - 5.5f);
				alivePosition = new Vector3 (0f, bodyTrans.eulerAngles.y, 0f);
				deadPosition = new Vector3 (-90f, bodyTrans.eulerAngles.y, 0f);
				StartCoroutine (RoutineEnterSpectatorMode());
				rotationSaved = true;
				photonView.RPC ("RpcAddToTotalDeaths", RpcTarget.All);
			}
			if (bodyTrans.rotation.x > -90f) {
				fraction += Time.deltaTime * 8f;
				bodyTrans.rotation = Quaternion.Euler (Vector3.Lerp(alivePosition, deadPosition, fraction));
			}
			DeathCameraEffect ();
		}
	}

	[PunRPC]
	void RpcAddToTotalDeaths() {
		deaths++;
		GameControllerScript.totalDeaths [photonView.Owner.NickName]++;
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
				hud.container.hintText.enabled = false;
				return;
			}

            if (Input.GetKey (KeyCode.E) && health > 0) {
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = false;

				hud.container.hintText.enabled = false;
				hud.ToggleActionBar (true);
				hud.container.defusingText.enabled = true;
				bombDefuseCounter += (Time.deltaTime / 8f);
				hud.SetActionBarSlider(bombDefuseCounter);
				if (bombDefuseCounter >= 1f) {
					bombDefuseCounter = 0f;

					photonView.RPC ("RpcDefuseBomb", RpcTarget.All, currentBombIndex);
					gameController.GetComponent<GameControllerScript> ().DecrementBombsRemaining ();
					currentBomb = null;

					hud.ToggleActionBar (false);
					hud.container.defusingText.enabled = false;
					hud.container.hintText.enabled = false;
					// Enable movement again
					gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;
				}
			} else {
				// Enable movement again
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;

				hud.ToggleActionBar (false);
				hud.container.defusingText.enabled = false;
				hud.container.hintText.enabled = true;
				bombDefuseCounter = 0f;
			}
		}
	}

	public void ResetHitTimer() {
		photonView.RPC ("RpcResetHitTimer", RpcTarget.All);
		hitTimer = 0f;
	}

	[PunRPC]
	void RpcResetHitTimer() {
		hitTimer = 0f;
	}

	public void ResetHealTimer() {
		healTimer = 0f;
	}

	[PunRPC]
	void RpcSetHealth(int h) {
		health = h;
	}

	public void SetHitLocation(Vector3 pos) {
		hitLocation = pos;
	}

	void DetermineEscaped() {
		if (gameController.GetComponent<GameControllerScript> ().escapeAvailable) {
			if (!escapeValueSent) {
				if (health > 0 && Vector3.Distance(gameController.GetComponent<GameControllerScript>().exitPoint.transform.position, transform.position) <= 10f && transform.position.y >= (gameController.GetComponent<GameControllerScript>().exitPoint.transform.position.y - 1f)) {
					gameController.GetComponent<GameControllerScript> ().IncrementEscapeCount ();
					escapeValueSent = true;
				}
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (photonView.IsMine) {
			if (other.gameObject.tag.Equals ("AmmoBox")) {
				wepScript.totalBulletsLeft = 120 + (wepScript.bulletsPerMag - wepScript.currentBullets);
				other.gameObject.GetComponent<PickupScript> ().DestroyPickup ();
			} else if (other.gameObject.tag.Equals ("HealthBox")) {
				ResetHealTimer ();
				health = 100;
				other.gameObject.GetComponent<PickupScript> ().DestroyPickup ();
			}
		}
	}

	void DeathCameraEffect() {
		deathCameraLerpVar += (Time.deltaTime / 4f);
		viewCam.transform.localPosition = Vector3.Lerp (viewCam.transform.localPosition, deathCameraLerpPos, deathCameraLerpVar);
	}

	[PunRPC]
	void RpcToggleFPSHands(bool b) {
		viewCam.gameObject.GetComponentInChildren<SkinnedMeshRenderer> ().enabled = b;
		viewCam.gameObject.GetComponentInChildren<MeshRenderer> ().enabled = b;
	}

	IEnumerator RoutineEnterSpectatorMode() {
		yield return new WaitForSeconds (6f);
		EnterSpectatorMode ();
	}

	void EnterSpectatorMode() {
		photonView.RPC ("RpcChangePlayerDisableStatus", RpcTarget.All, false);
		thisSpectatorCam = Instantiate (spectatorCam, Vector3.zero, Quaternion.Euler(Vector3.zero));
		thisSpectatorCam.transform.SetParent (gameObject.transform);
	}

	void LeaveSpectatorMode() {
		Destroy (thisSpectatorCam);
		thisSpectatorCam = null;
		photonView.RPC ("RpcChangePlayerDisableStatus", RpcTarget.All, true);
	}

	[PunRPC]
	void RpcChangePlayerDisableStatus(bool status) {
		subComponents [0].SetActive (status);
		subComponents [1].SetActive (status);
		subComponents [4].SetActive (status);
		if (photonView.IsMine) {
			subComponents [2].SetActive(status);
			subComponents [3].SetActive(status);
			charController.enabled = status;
			fpc.enabled = status;
			viewCam.enabled = status;
			wepScript.enabled = status;
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		escapeValueSent = false;
	}

	public override void OnDisconnected(DisconnectCause cause) {
		Destroy (gameObject);
	}
		
	[PunRPC]
	void SyncPlayerColor(Vector3 c) {
		bodyTrans.gameObject.GetComponent<MeshRenderer> ().material.color = new Color (c.x / 255f, c.y / 255f, c.z / 255f, 1f);
	}

	void BeginRespawn() {
		if (health <= 0) {
			gameController.GetComponent<GameControllerScript> ().ConvertCounts (-1, 0);
			gameController.GetComponent<GameControllerScript> ().gameOver = false;
			// Flash the respawn time bar on the screen
			hud.RespawnBar ();
			// Then, actually start the respawn process
			respawnTimer = 5f;
			isRespawning = true;
		}
	}

	void RespawnRoutine() {
		if (isRespawning) {
			respawnTimer -= Time.deltaTime;
			if (respawnTimer <= 0f) {
				isRespawning = false;
				Respawn ();
			}
		}
	}

	// Reset character health, scale, rotation, position, ammo, re-enable FPS hands, disabled HUD components, disabled scripts, death variables, etc.
	void Respawn() {
		photonView.RPC ("RpcSetHealth", RpcTarget.All, 100);
		photonView.RPC ("RpcToggleFPSHands", RpcTarget.All, true);
		hud.ToggleHUD (true);

		fpc.m_IsCrouching = false;
		fpc.m_IsWalking = true;
		escapeValueSent = false;
		canShoot = true;
		fpc.canMove = true;
		fraction = 0f;
		deathCameraLerpVar = 0f;
		rotationSaved = false;
		hitTimer = 1f;
		healTimer = 1f;
		currentBomb = null;
		bombDefuseCounter = 0f;
		wepScript.totalBulletsLeft = 120;
		wepScript.currentBullets = wepScript.bulletsPerMag;

		// Send player back to spawn position, reset rotation, leave spectator mode
		transform.rotation = Quaternion.Euler(Vector3.zero);
		transform.position = new Vector3 (gameController.GetComponent<GameControllerScript>().spawnLocation.position.x, gameController.GetComponent<GameControllerScript>().spawnLocation.position.y, gameController.GetComponent<GameControllerScript>().spawnLocation.position.z);
		LeaveSpectatorMode ();
		wepScript.CockingAction ();
	}

	[PunRPC]
	void RpcDefuseBomb(int index) {
		gameController.GetComponent<GameControllerScript> ().bombs[index].GetComponent<BombScript>().Defuse ();
	}

	public void HandleGameOverBanner() {
		if (fpc.enabled) {
			EnterSpectatorMode ();
		}
		thisSpectatorCam.GetComponent<SpectatorScript> ().GameOverCam ();
	}
		
}
