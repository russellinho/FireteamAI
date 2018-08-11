using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class PlayerTestingScript : NetworkBehaviour {

	private const string HEALTH_TEXT = "Health: ";

	public bool invincibility;
	public int health;
	private Text healthText;
	private GameObject hitFlare;
	private GameObject hitDir;
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

	private NetworkManager networkMan;

	// Level stuff
	public GameObject[] bombs;
	private GameObject currentBomb;
	private int currentBombIndex = 0;
	private float bombDefuseCounter = 0f;

	private Text weaponLabelTxt;
	private Text ammoTxt;
	private string currWep = "AK-47"; // TODO: Needs to be changed soon to account for other weps

	private WeaponTestingScript wepScript;
	public float hitTimer = 1f;
	public Vector3 hitLocation;

	public GameObject gameController;

	// Use this for initialization
	void Start () {
		invincibility = true;
		if (!isLocalPlayer) {
			Destroy (GetComponentInChildren<AudioListener>());
			GetComponentInChildren<Camera> ().enabled = false;
			return;
		}

		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			bombs = GameObject.FindGameObjectsWithTag ("Bomb");
		}
		gameController = GameObject.Find ("GameControllerTest");
		GameControllerTestScript.playerList.Add (gameObject);
		isCrouching = false;
		health = 100;
		crouchSpeed = 3f;
		healthText = GameObject.Find ("HealthBar").GetComponent<Text>();

		// Setting original positions to return from crouching
		charController = GetComponent<CharacterController>();
		charHeight = charController.height;
		fpcPositionYOriginal = fpcPosition.localPosition.y;
		bodyScaleOriginal = bodyScaleTrans.lossyScale.y;

		fraction = 0f;
		rotationSaved = false;
		isCrouching = false;

		// Get weapon and ammo UI
		weaponLabelTxt = GameObject.Find("WeaponLabel").GetComponent<Text>();
		ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();
		wepScript = gameObject.GetComponent<WeaponTestingScript> ();
		canShoot = true;

		hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");
		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		healthText.text = HEALTH_TEXT + health;

		Crouch ();
		// Update UI
		weaponLabelTxt.text = currWep;
		ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
	}

	void FixedUpdate() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		if (hitTimer < 1f) {
			hitFlare.GetComponent<RawImage> ().enabled = true;
			hitDir.GetComponent<RawImage> ().enabled = true;
			hitTimer += Time.deltaTime;
		} else {
			hitFlare.GetComponent<RawImage> ().enabled = false;
			hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward,hitLocation);
			Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
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

		if (!isServer)
			CmdCrouch (bodyScale);
		else
			RpcCrouch (bodyScale);

		float lastHeight = charController.height;
		float lastCameraHeight = fpcPosition.position.y;
		charController.height = Mathf.Lerp (charController.height, h, 10 * Time.deltaTime);
		fpcPosition.localPosition = new Vector3 (fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
		//Debug.Log (fpcPosition.position.y);
		transform.position = new Vector3 (transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
	}

	[Command]
	public void CmdCrouch(float bodyScale) {
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
	}

	[ClientRpc]
	public void RpcCrouch(float bodyScale) {
		bodyScaleTrans.localScale = new Vector3 (bodyScaleTrans.localScale.x, bodyScale, bodyScaleTrans.localScale.z);
	}
		

	public void disconnectFromGame() {
		Debug.Log (1);
		if (!isServer) {
			Debug.Log (2);
			// Disconnect from server
			NetworkManager.singleton.StopHost();
		}
	}
		

}
