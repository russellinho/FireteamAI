﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHUDScript : MonoBehaviourPunCallbacks {

	// Player reference
	private PlayerScript playerScript;
	private WeaponScript wepScript;
	private GameControllerScript gameController;

    // Health HUD
    private const string HEALTH_TEXT = "Health: ";
    private Text healthText;

    // Weapon HUD
    private Text weaponLabelTxt;
    private Text ammoTxt;

    // Pause/in-game menu HUD
    public GameObject pauseMenuGUI;
    public GameObject pauseExitBtn;
    public GameObject pauseResumeBtn;
    public GameObject pauseOptionsBtn;
    public GameObject scoreboard;
    public GameObject endGameText;
    public GameObject endGameButton;

    // Hit indication HUD
    public GameObject hitFlare;
    private GameObject hitDir;
	public GameObject hitMarker;

    // Map HUD
    public GameObject hudMap;
    public GameObject hudWaypoint;
	public GameObject hudPlayerMarker;
	private ArrayList missionWaypoints;
	private Dictionary<int, GameObject> playerMarkers = new Dictionary<int, GameObject> ();

    // On-screen indication HUD
    public Text objectivesText;
    public GameObject missionText;
    public GameObject actionBar;
    public Text defusingText;
    public Text hintText;
	private ObjectivesTextScript objectiveFormatter;
	public Text spectatorText;
    public Text missionTimeText;
	public Text missionTimeRemainingText;
	public Text assaultModeIndText;
	public TextMeshProUGUI killPopupText;
	public Image screenColor;

	// Other vars
	private float killPopupTimer;
	private bool popupIsStarting;
	private bool roundStartFadeIn;
	private float hitmarkerTimer;

    // Use this for initialization
    void Start () {
        if (!GetComponent<PhotonView>().IsMine) {
            this.enabled = false;
			return;
        }
		// Find/load HUD components
		LoadHUDComponents ();

		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;
		hitMarker.GetComponent<RawImage> ().enabled = false;

		pauseMenuGUI.SetActive (false);
		ToggleActionBar(false);
		defusingText.enabled = false;
		hintText.enabled = false;
		scoreboard.GetComponent<Image> ().enabled = false;
		endGameText.SetActive (false);
		endGameButton.SetActive (false);
		spectatorText.gameObject.SetActive (false);

		//hudMap.SetActive (true);

		playerScript = GetComponent<PlayerScript> ();
		wepScript = GetComponent<WeaponScript> ();
		gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript> ();
		killPopupTimer = 0f;
		hitmarkerTimer = 0f;
		popupIsStarting = false;

		LoadBetaLevel ();
		StartMatchCameraFade ();
	}

	public void StartMatchCameraFade() {
		screenColor.enabled = true;
		screenColor.color = new Color (0f, 0f, 0f, 1f);
		roundStartFadeIn = true;
	}

	void LoadBetaLevel() {
		if (SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			screenColor.color = new Color (0f, 0f, 0f, 1f);
			gameController.bombsRemaining = 4;
			gameController.currentMap = 1;
			objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);

			GameObject m1 = GameObject.Instantiate (hudWaypoint);
			m1.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m2 = GameObject.Instantiate (hudWaypoint);
			m2.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m3 = GameObject.Instantiate (hudWaypoint);
			m3.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m4 = GameObject.Instantiate (hudWaypoint);
			m4.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m5 = GameObject.Instantiate (hudWaypoint);
			m5.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			m5.GetComponent<RawImage> ().enabled = false;

			missionWaypoints.Add (m1);
			missionWaypoints.Add (m2);
			missionWaypoints.Add (m3);
			missionWaypoints.Add (m4);
			missionWaypoints.Add (m5);

			StartCoroutine(ShowMissionText());

		}
	}



	void OnStartScreenFade() {
		if (roundStartFadeIn) {
			float newAlpha = screenColor.color.a - (Time.deltaTime * 1.75f);
			screenColor.color = new Color (0f, 0f, 0f, newAlpha);
			if (screenColor.color.a <= 0f) {
				roundStartFadeIn = false;
			}
		}
	}

	void LoadHUDComponents() {
		// Health HUD
		healthText = GameObject.Find ("HealthBar").GetComponent<Text>();

		// Weapon HUD
		weaponLabelTxt = GameObject.Find ("WeaponLabel").GetComponent<Text>();
		ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();

		// Pause/in-game menu HUD
		pauseMenuGUI = GameObject.Find ("PausePanel");
		pauseExitBtn = GameObject.Find ("QuitBtn");
		pauseResumeBtn = GameObject.Find ("ResumeBtn");
		pauseOptionsBtn = GameObject.Find ("OptionsBtn");
		scoreboard = GameObject.Find ("Scoreboard");
		endGameText = GameObject.Find ("EndGameTxt");
		endGameButton = GameObject.Find ("EndGameBtn");

		// Hit indication HUD
		hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");
		hitMarker = GameObject.Find ("Hitmarker");

		// Map HUD
		hudMap = GameObject.Find ("HUDMap");
		missionWaypoints = new ArrayList ();

		// On-screen indication HUD
		objectivesText = GameObject.Find ("ObjectivesText").GetComponent<Text>();
		missionText = GameObject.Find ("IntroMissionText");
		actionBar = GameObject.Find ("ActionBar");
		defusingText = GameObject.Find ("DefusingText").GetComponent<Text>();
		hintText = GameObject.Find ("HintText").GetComponent<Text>();
		spectatorText = GameObject.Find ("SpectatorTxt").GetComponent<Text> ();
		objectiveFormatter = new ObjectivesTextScript();
        missionTimeText = GameObject.Find ("MissionTimeTxt").GetComponent<Text> ();
		missionTimeRemainingText = GameObject.Find ("MissionTimeRemainingTxt").GetComponent<Text>();
		assaultModeIndText = GameObject.Find ("AssaultModeInd").GetComponent<Text>();
		killPopupText = GameObject.Find ("KillPopup").GetComponent<TextMeshProUGUI>();
		screenColor = GameObject.Find ("ScreenColor").GetComponent<Image>();

	}
	
	// Update is called once per frame
	void Update () {
		if (playerScript == null || wepScript == null) {
			playerScript = GetComponent<PlayerScript> ();
			wepScript = GetComponent<WeaponScript> ();
		}
		if (gameController == null) {
			gameController = GameObject.Find ("GameController").GetComponent<GameControllerScript> ();
		}
		healthText.text = (healthText ? HEALTH_TEXT + playerScript.health : "");

		UpdateHitmarker ();

		// Update UI
		weaponLabelTxt.text = playerScript.currWep;
		ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
		if (!gameController.gameOver) {
			UpdatePlayerMarkers ();
			UpdateWaypoints ();
		} else {
			playerMarkers = null;
			missionWaypoints = null;
		}

		UpdateCursorStatus ();

		if (gameController.gameOver && healthText.enabled) {
			DisableHUD();
			ToggleScoreboard ();
		}

        UpdateMissionTimeText();

		// Update kill popups
		if (killPopupText.enabled) {
			KillPopupUpdate ();
		}

		OnStartScreenFade ();
    }

	void FixedUpdate() {
		UpdateHitFlare();
	}

	void UpdateCursorStatus() {
		if (Input.GetKeyDown(KeyCode.Escape) && !scoreboard.GetComponent<Image>().enabled)
			Pause();

		if (pauseMenuGUI.activeInHierarchy || endGameText.activeInHierarchy)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void UpdateWaypoints() {
		for (int i = 0; i < missionWaypoints.Count; i++)
		{
			if (gameController.c == null)
				break;
			if (i == missionWaypoints.Count - 1)
			{
				float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - gameController.c.transform.position).normalized, gameController.c.transform.forward);
				if (renderCheck <= 0)
					continue;
				if (gameController.bombsRemaining == 0)
				{
					((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = true;
					((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = gameController.c.WorldToScreenPoint(gameController.exitPoint.transform.position);
				}
			}
			else
			{
				float renderCheck = Vector3.Dot((gameController.bombs[i].transform.position - gameController.c.transform.position).normalized, gameController.c.transform.forward);
				if (renderCheck <= 0)
					continue;
				if (!gameController.bombs[i].GetComponent<BombScript>().defused && gameController.c != null)
				{
					Vector3 p = new Vector3(gameController.bombs[i].transform.position.x, gameController.bombs[i].transform.position.y + gameController.bombs[i].transform.lossyScale.y, gameController.bombs[i].transform.position.z);
					((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = gameController.c.WorldToScreenPoint(p);
				}
				if (((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled && gameController.bombs[i].GetComponent<BombScript>().defused)
				{
					((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = false;
				}
			}
		}
	}

	void UpdatePlayerMarkers() {
		foreach (GameObject p in GameControllerScript.playerList.Values) {
			int actorNo = p.GetComponent<PhotonView> ().OwnerActorNr;
			if (actorNo == PhotonNetwork.LocalPlayer.ActorNumber) {
				continue;
			}
			if (!playerMarkers.ContainsKey (actorNo)) {
				GameObject marker = GameObject.Instantiate (hudPlayerMarker);
				marker.GetComponent<TextMeshProUGUI> ().text = p.GetComponent<PhotonView> ().Owner.NickName;
				marker.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
				playerMarkers.Add (actorNo, marker);
			}
			// Check if it can be rendered to the screen
			float renderCheck = Vector3.Dot((p.transform.position - gameController.c.transform.position).normalized, gameController.c.transform.forward);
			if (renderCheck <= 0)
				continue;
			// If the player is alive and on camera, then render the player name and health bar
			if (p.GetComponent<PlayerScript>().health > 0 && gameController.c != null)
			{
				playerMarkers [actorNo].SetActive (true);
				playerMarkers[actorNo].GetComponentInChildren<Slider>().value = ((float)(p.GetComponent<PlayerScript>().health / 100));
				Vector3 o = new Vector3(p.transform.position.x, p.transform.position.y + p.transform.lossyScale.y, p.transform.position.z);
				playerMarkers[actorNo].GetComponent<RectTransform>().position = gameController.c.WorldToScreenPoint(o);
			}
			if (playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled && p.GetComponent<PlayerScript>().health <= 0)
			{
				playerMarkers [actorNo].SetActive (false);
				//playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled = false;
			}
		}
	}

	public void InstantiateHitmarker() {
		hitmarkerTimer = 1.5f;
	}

	void UpdateHitmarker() {
		if (hitmarkerTimer > 0f) {
			hitMarker.GetComponent<RawImage> ().enabled = true;
			hitmarkerTimer -= Time.deltaTime;
		} else {
			hitMarker.GetComponent<RawImage> ().enabled = false;
		}
	}

	void UpdateHitFlare() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		if (playerScript.hitTimer < 1f) {
			hitFlare.GetComponent<RawImage> ().enabled = true;
			hitDir.GetComponent<RawImage> ().enabled = true;
			playerScript.hitTimer += Time.deltaTime;
		} else {
			hitFlare.GetComponent<RawImage> ().enabled = false;
			hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward, playerScript.hitLocation);
			Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}
	}

    public void DisableHUD()
    {
        healthText.enabled = false;
        weaponLabelTxt.enabled = false;
        ammoTxt.enabled = false;
        hudMap.SetActive(false);
    }

    public void ToggleScoreboard()
    {
        scoreboard.GetComponent<Image>().enabled = true;
        endGameText.SetActive(true);
        endGameButton.SetActive(true);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Title");
		PhotonNetwork.Disconnect ();
    }

    void Pause()
    {
        if (!pauseMenuGUI.activeInHierarchy)
        {
            pauseMenuGUI.SetActive(true);
        }
        else
        {
            pauseMenuGUI.SetActive(false);
        }
    }

    IEnumerator ShowMissionText()
    {
        yield return new WaitForSeconds(5f);
		missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

    public void ToggleActionBar(bool enable)
    {
        int c = actionBar.GetComponentsInChildren<Image>().Length;
        if (!enable)
        {
            // Disable all actionbar components
            for (int i = 0; i < c; i++)
            {
                actionBar.GetComponentsInChildren<Image>()[i].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < c; i++)
            {
                actionBar.GetComponentsInChildren<Image>()[i].enabled = true;
            }
        }
    }

    public void UpdateObjectives()
    {
        objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);
    }

	public void MessagePopup(string message)
    {
		missionText.GetComponent<MissionTextAnimScript> ().Reset ();
		missionText.GetComponent<Text> ().text = message;
		missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

	public void SetActionBarSlider(float val) {
		actionBar.GetComponent<Slider> ().value = val;
	}

	public void EnableSpectatorMessage() {
		spectatorText.gameObject.SetActive (true);
	}

	//public override void OnPlayerEnteredRoom(Player newPlayer) {
	//	Debug.Log (newPlayer.NickName + " has joined the room");
	//	GameControllerScript.playerList.Add (gameObject);
	//	Debug.Log ("anotha one");
	//}

    private void UpdateMissionTimeText() {
        float totalSecs = GameControllerScript.missionTime;
        int mins = (int)(totalSecs / 60f);
        int remainingSecs = (int)(totalSecs - (mins * 60f));
        missionTimeText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));

		// Set remaining time
		mins = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) / 60f);
		remainingSecs = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) - (mins * 60f));
		missionTimeRemainingText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));
    }

	public void UpdateAssaultModeIndHud(bool assaultInProgress) {
		if (assaultInProgress) {
			assaultModeIndText.fontStyle = FontStyle.Normal;
			assaultModeIndText.text = "ASSAULT";
			assaultModeIndText.color = Color.red;
		} else {
			assaultModeIndText.fontStyle = FontStyle.Italic;
			assaultModeIndText.text = "Stealth";
			assaultModeIndText.color = Color.blue;
		}
	}

	public void OnScreenEffect(string message, bool headshot) {
		StartCoroutine (StartOnScreenEffect(message, headshot));
	}

	IEnumerator StartOnScreenEffect(string message, bool headshot) {
		ResetOnScreenEffect ();
		killPopupText.text = message;
		popupIsStarting = true;
		killPopupText.enabled = true;
		yield return new WaitForSeconds (3f);
		popupIsStarting = false;
		ResetOnScreenEffect ();
	}

	void ResetOnScreenEffect() {
		if (popupIsStarting) {
			killPopupText.alpha = 0.1f;
			killPopupText.gameObject.transform.localScale = new Vector3 (12f, 8f, 1f);
		}
		killPopupTimer = 0f;
	}

	void KillPopupUpdate() {
		killPopupTimer += Time.deltaTime;
		if (popupIsStarting) {
			killPopupText.alpha += Time.deltaTime;
			killPopupText.gameObject.transform.localScale = Vector3.Lerp (new Vector3(12f,8f,1f), new Vector3(1f,1f,1f), killPopupTimer);
		} else {
			killPopupText.alpha -= Time.deltaTime;
			killPopupText.gameObject.transform.localScale = Vector3.Lerp (new Vector3(1f,1f,1f), new Vector3(12f,8f,1f), killPopupTimer);
			if (killPopupTimer >= 1f) {
				killPopupText.enabled = false;
			}
		}
	}

}
