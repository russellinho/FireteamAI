using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PlayerScript : MonoBehaviour {

	// Object references
	public GameObject gameController;
	private CharacterController charController;
	private PhotonView photonView;
	public GameObject fpsHands;
	private WeaponScript wepScript;

	// Player variables
	private string currWep; // TODO: Needs to be changed soon to account for other weps
	public int health;
	public bool isCrouching;
	public bool canShoot;
	private float charHeightOriginal;
	private float crouchSpeed;

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
	private bool rotationSaved;

	public float hitTimer;
	public Vector3 hitLocation;

	// Use this for initialization
	void Start () {
		// Setting original positions for returning from crouching
		charController = GetComponent<CharacterController>();
		charHeightOriginal = charController.height;
		fpcPositionYOriginal = fpcPosition.localPosition.y;
		bodyScaleOriginal = bodyScaleTrans.lossyScale.y;
		// If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
        if (!GetComponent<PhotonView>().IsMine) {
			Destroy (GetComponentInChildren<AudioListener>());
			GetComponentInChildren<Camera> ().enabled = false;
			enabled = false;
			return;
		}
			
		photonView = GetComponent<PhotonView> ();

		gameController = GameObject.Find ("GameController");
		//GameControllerScript.playerList.Add (gameObject);

		wepScript = gameObject.GetComponent<WeaponScript> ();

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
		rotationSaved = false;

		hitTimer = 1f;

	}

	// Update is called once per frame
	void Update () {
        if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		Crouch ();
		//BombCheck ();
		DeathCheck ();
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

	/**[Command]
	public void CmdCrouch(float bodyScale) {
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
	}*/

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
			if (fpsHands.activeInHierarchy) fpsHands.SetActive (false);
			if (!rotationSaved) {
				alivePosition = new Vector3 (0f, transform.eulerAngles.y, 0f);
				deadPosition = new Vector3 (-90f, transform.eulerAngles.y, 0f);
				rotationSaved = true;
			}
			GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().enabled = false;
			if (transform.rotation.x > -90f) {
				fraction += Time.deltaTime * 8f;
				transform.rotation = Quaternion.Euler (Vector3.Lerp(alivePosition, deadPosition, fraction));
			}
		}
	}

	// If map objective is defusing bombs, this method checks if the player is near any bombs
	/**void BombCheck() {
		if (bombs == null) {
			return;
		}

		if (currentBomb == null) {
			bool found = false;
			int count = 0;
			foreach (GameObject i in bombs) {
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
				gameController.GetComponent<GameControllerScript> ().hintText.enabled = false;
				return;
			}

			if (Input.GetKey (KeyCode.E)) {
				// TODO: Disallow movement
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = false;

				gameController.GetComponent<GameControllerScript> ().hintText.enabled = false;
				gameController.GetComponent<GameControllerScript> ().ToggleActionBar (true);
				gameController.GetComponent<GameControllerScript> ().defusingText.enabled = true;
				bombDefuseCounter += (Time.deltaTime / 8f);
				Debug.Log (bombDefuseCounter);
				gameController.GetComponent<GameControllerScript> ().actionBar.GetComponent<Slider> ().value = bombDefuseCounter;
				if (bombDefuseCounter >= 1f) {
					bombDefuseCounter = 0f;

					bombs[currentBombIndex].GetComponent<BombScript>().Defuse ();
					gameController.GetComponent<GameControllerScript> ().bombsRemaining--;
					gameController.GetComponent<GameControllerScript> ().UpdateObjectives ();
					currentBomb = null;

					gameController.GetComponent<GameControllerScript> ().ToggleActionBar (false);
					gameController.GetComponent<GameControllerScript> ().defusingText.enabled = false;
					gameController.GetComponent<GameControllerScript> ().hintText.enabled = false;
					// Enable movement again
					gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;
					if (gameController.GetComponent<GameControllerScript> ().bombsRemaining == 0) {
						gameController.GetComponent<GameControllerScript> ().EscapePopup ();
					}
				}
			} else {
				// Enable movement again
				gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().canMove = true;

				gameController.GetComponent<GameControllerScript> ().ToggleActionBar (false);
				gameController.GetComponent<GameControllerScript> ().defusingText.enabled = false;
				gameController.GetComponent<GameControllerScript> ().hintText.enabled = true;
				//Debug.Log (gameController.GetComponent<GameControllerScript> ().hintText.enabled);
				bombDefuseCounter = 0f;
			}
		}
	}*/

}
