using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour {

	public GameObject gameController;

	private const string HEALTH_TEXT = "Health: ";

	public int health;
	private Text healthText;
	//private GameObject hitFlare;
	//private GameObject hitDir;
	public bool isCrouching;
	public bool canShoot;

	private CharacterController charController;
	private float charHeight;
	private float crouchSpeed;

	public Transform fpcPosition;
	private float fpcPositionYOriginal;
	public Transform bodyScaleTrans;
	private float bodyScaleOriginal;

	private float crouchPosY = 0.3f;
	private float crouchBodyPosY = 0.25f;
	private float crouchBodyScaleY = 0.66f;

	private Vector3 alivePosition;
	private Vector3 deadPosition;
	private float fraction;
	private bool rotationSaved;

	public GameObject fpsHands;

	//private NetworkManager networkMan;

	// Level stuff
	public GameObject[] bombs;
	private GameObject currentBomb;
	private int currentBombIndex = 0;
	private float bombDefuseCounter = 0f;

	private Text weaponLabelTxt;
	private Text ammoTxt;
	private string currWep = "AK-47"; // TODO: Needs to be changed soon to account for other weps

	private WeaponScript wepScript;
	public float hitTimer = 1f;
	public Vector3 hitLocation;

	// Use this for initialization
	void Start () {
		/**if (!isLocalPlayer) {
			Destroy (GetComponentInChildren<AudioListener>());
			GetComponentInChildren<Camera> ().enabled = false;
			return;
		}*/

		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			bombs = GameObject.FindGameObjectsWithTag ("Bomb");
		}
		gameController = GameObject.Find ("GameController");
		//GameControllerScript.playerList.Add (gameObject);
		isCrouching = false;
		health = 100;
		crouchSpeed = 3f;
		//healthText = GameObject.Find ("HealthBar").GetComponent<Text>();

		// Setting original positions to return from crouching
		charController = GetComponent<CharacterController>();
		charHeight = charController.height;
		fpcPositionYOriginal = fpcPosition.localPosition.y;
		bodyScaleOriginal = bodyScaleTrans.lossyScale.y;

		fraction = 0f;
		rotationSaved = false;
		isCrouching = false;

		// Get weapon and ammo UI
///		weaponLabelTxt = GameObject.Find("WeaponLabel").GetComponent<Text>();
		//ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();
		wepScript = gameObject.GetComponent<WeaponScript> ();
		//gameController.GetComponent<GameControllerScript> ().hudMap.SetActive (true);
		canShoot = true;

		/**hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");
		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;*/
	}

	// Update is called once per frame
	void Update () {
		/**if (!isLocalPlayer) {
			return;
		}*/
//		healthText.text = HEALTH_TEXT + health;

		Crouch ();
		BombCheck ();
		DeathCheck ();
		// Update UI
//		weaponLabelTxt.text = currWep;
		//ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
	}

	void FixedUpdate() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		if (hitTimer < 1f) {
			//hitFlare.GetComponent<RawImage> ().enabled = true;
			//hitDir.GetComponent<RawImage> ().enabled = true;
			hitTimer += Time.deltaTime;
		} else {
			//hitFlare.GetComponent<RawImage> ().enabled = false;
			//hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward,hitLocation);
			//Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			//hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}
	}

	public void Crouch() {
		float h = charHeight;
		float viewH = fpcPositionYOriginal;
		//float speed = charController.velocity;
		float bodyScale = bodyScaleTrans.lossyScale.y;

		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			isCrouching = !isCrouching;
		}

		if (isCrouching) {
			h = charHeight * .65f;
			viewH = .55f;
			bodyScale = .7f;
			//Change speed
			//Make it impossible to jump
			//Make it impossible to sprint
		} else {
			viewH = .8f;
			bodyScale = bodyScaleOriginal;
		}

		/**if (!isServer)
			CmdCrouch (bodyScale);
		else
			RpcCrouch (bodyScale);*/

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (charController.height, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
		//Debug.Log (fpcPosition.position.y);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
	}

	/**[Command]
	public void CmdCrouch(float bodyScale) {
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
	}*/

	/**[ClientRpc]
	public void RpcCrouch(float bodyScale) {
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
	}*/

	void DeathCheck() {
		if (health <= 0) {
			if (fpsHands.activeInHierarchy) fpsHands.SetActive (false);
			if (!rotationSaved) {
				alivePosition = new Vector3 (0f, transform.eulerAngles.y, 0f);
				deadPosition = new Vector3 (-90f, transform.eulerAngles.y, 0f);
				Debug.Log (alivePosition.y);
				Debug.Log (deadPosition.y);
				rotationSaved = true;
			}
			GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().enabled = false;
			if (transform.rotation.x > -90f) {
				fraction += Time.deltaTime * 8f;
				transform.rotation = Quaternion.Euler (Vector3.Lerp(alivePosition, deadPosition, fraction));
			}
		}
	}

	/**	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer) {
			Debug.Log ("Local server connection disconnected");
		} else {
			if (info == NetworkDisconnection.LostConnection)
				Debug.Log ("Lost connection to the server");
			else
				Debug.Log ("Successfully diconnected from the server");
		}
	}*/

	/**public void disconnectFromGame() {
		Debug.Log (1);
		if (!isServer) {
			Debug.Log (2);
			// Disconnect from server
			NetworkManager.singleton.StopHost();
		}
	}*/

	// If map objective is defusing bombs, this method checks if the player is near any bombs
	void BombCheck() {
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
	}

}
