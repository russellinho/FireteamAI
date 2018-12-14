using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHUDScript : MonoBehaviourPunCallbacks {

	// HUD object reference
	public HUDContainer container;

	// Player reference
	private PlayerScript playerScript;
	private WeaponScript wepScript;
	private GameControllerScript gameController;

	private ArrayList missionWaypoints;
	private Dictionary<int, GameObject> playerMarkers = new Dictionary<int, GameObject> ();
	private ObjectivesTextScript objectiveFormatter;

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
		missionWaypoints = new ArrayList ();
		objectiveFormatter = new ObjectivesTextScript();

		container = GameObject.Find ("HUD").GetComponent<HUDContainer> ();
		container.hitFlare.GetComponent<RawImage> ().enabled = false;
		container.hitDir.GetComponent<RawImage> ().enabled = false;
		container.hitMarker.GetComponent<RawImage> ().enabled = false;

		container.pauseMenuGUI.SetActive (false);
		ToggleActionBar(false);
		container.defusingText.enabled = false;
		container.hintText.enabled = false;
		container.scoreboard.GetComponent<Image> ().enabled = false;
		container.endGameText.SetActive (false);
		container.endGameButton.SetActive (false);
		container.spectatorText.gameObject.SetActive (false);

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
		container.screenColor.enabled = true;
		container.screenColor.color = new Color (0f, 0f, 0f, 1f);
		roundStartFadeIn = true;
	}

	void LoadBetaLevel() {
		if (SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			container.screenColor.color = new Color (0f, 0f, 0f, 1f);
			gameController.bombsRemaining = 4;
			gameController.currentMap = 1;
			container.objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);

			GameObject m1 = GameObject.Instantiate (container.hudWaypoint);
			m1.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
			GameObject m2 = GameObject.Instantiate (container.hudWaypoint);
			m2.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
			GameObject m3 = GameObject.Instantiate (container.hudWaypoint);
			m3.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
			GameObject m4 = GameObject.Instantiate (container.hudWaypoint);
			m4.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
			GameObject m5 = GameObject.Instantiate (container.hudWaypoint);
			m5.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
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
			float newAlpha = container.screenColor.color.a - (Time.deltaTime * 1.75f);
			container.screenColor.color = new Color (0f, 0f, 0f, newAlpha);
			if (container.screenColor.color.a <= 0f) {
				roundStartFadeIn = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (playerScript == null || wepScript == null) {
			playerScript = GetComponent<PlayerScript> ();
			wepScript = GetComponent<WeaponScript> ();
		}
		if (gameController == null) {
			gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript> ();
		}
		container.healthText.text = (container.healthText ? "Health: " + playerScript.health : "");

		UpdateHitmarker ();

		// Update UI
		container.weaponLabelTxt.text = playerScript.currWep;
		container.ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
		UpdatePlayerMarkers ();
		UpdateWaypoints ();
		UpdateCursorStatus ();

		if (gameController.gameOver) {
            if (PhotonNetwork.CurrentRoom.Players.Count == gameController.deadCount)
            {
                GameOverPopup();
            }
		}

        UpdateMissionTimeText();

		// Update kill popups
		OnScreenEffectUpdate ();

		OnStartScreenFade ();
    }

	void FixedUpdate() {
		UpdateHitFlare();
		if (!container.hitFlare.GetComponent<RawImage> ().enabled) {
			UpdateHealFlare();
		}
	}

	void UpdateCursorStatus() {
		if (Input.GetKeyDown(KeyCode.Escape) && !container.scoreboard.GetComponent<Image>().enabled)
			Pause();

		if (container.pauseMenuGUI.activeInHierarchy || container.endGameText.activeInHierarchy)
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
				GameObject marker = GameObject.Instantiate (container.hudPlayerMarker);
				marker.GetComponent<TextMeshProUGUI> ().text = p.GetComponent<PhotonView> ().Owner.NickName;
				marker.GetComponent<RectTransform> ().SetParent (container.hudMap.transform.parent);
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
		hitmarkerTimer = 0.75f;
	}

	void UpdateHitmarker() {
		if (hitmarkerTimer > 0f) {
			container.hitMarker.GetComponent<RawImage> ().enabled = true;
			hitmarkerTimer -= Time.deltaTime;
		} else {
			container.hitMarker.GetComponent<RawImage> ().enabled = false;
		}
	}

	void UpdateHitFlare() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		container.healFlare.GetComponent<RawImage>().enabled = false;
		if (playerScript.hitTimer < 1f) {
			container.hitFlare.GetComponent<RawImage> ().enabled = true;
			container.hitDir.GetComponent<RawImage> ().enabled = true;
			playerScript.hitTimer += Time.deltaTime;
		} else {
			container.hitFlare.GetComponent<RawImage> ().enabled = false;
			container.hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward, playerScript.hitLocation);
			Vector3 temp = container.hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			container.hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}
	}

	void UpdateHealFlare() {
		if (playerScript.healTimer < 1f) {
			container.healFlare.GetComponent<RawImage> ().enabled = true;
			playerScript.healTimer += Time.deltaTime;
		} else {
			container.healFlare.GetComponent<RawImage> ().enabled = false;
		}
	}

    public void DisableHUD()
    {
        container.healthText.enabled = false;
        container.weaponLabelTxt.enabled = false;
        container.ammoTxt.enabled = false;
        container.hudMap.SetActive(false);
    }

    public void ToggleScoreboard()
    {
        container.scoreboard.GetComponent<Image>().enabled = true;
        container.endGameText.SetActive(true);
        container.endGameButton.SetActive(true);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Title");
		PhotonNetwork.Disconnect ();
    }

    void Pause()
    {
        if (!container.pauseMenuGUI.activeInHierarchy)
        {
            container.pauseMenuGUI.SetActive(true);
        }
        else
        {
            container.pauseMenuGUI.SetActive(false);
        }
    }

    IEnumerator ShowMissionText()
    {
        yield return new WaitForSeconds(5f);
		container.missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

    public void ToggleActionBar(bool enable)
    {
        int c = container.actionBar.GetComponentsInChildren<Image>().Length;
        if (!enable)
        {
            // Disable all actionbar components
            for (int i = 0; i < c; i++)
            {
                container.actionBar.GetComponentsInChildren<Image>()[i].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < c; i++)
            {
                container.actionBar.GetComponentsInChildren<Image>()[i].enabled = true;
            }
        }
    }

    public void UpdateObjectives()
    {
		photonView.RPC ("RpcUpdateObjectives", RpcTarget.All);
    }

	[PunRPC]
	void RpcUpdateObjectives() {
		container.objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);
	}

	public void MessagePopup(string message)
    {
		photonView.RPC ("RpcMessagePopup", RpcTarget.All, message);
    }

	[PunRPC]
	void RpcMessagePopup(string message) {
		container.missionText.GetComponent<MissionTextAnimScript> ().Reset ();
		container.missionText.GetComponent<Text> ().text = message;
		container.missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
	}

	public void SetActionBarSlider(float val) {
		container.actionBar.GetComponent<Slider> ().value = val;
	}

	public void EnableSpectatorMessage() {
		container.spectatorText.gameObject.SetActive (true);
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
        container.missionTimeText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));

		// Set remaining time
		mins = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) / 60f);
		remainingSecs = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) - (mins * 60f));
		container.missionTimeRemainingText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));
    }

	public void UpdateAssaultModeIndHud(bool assaultInProgress) {
		if (assaultInProgress) {
			container.assaultModeIndText.fontStyle = FontStyle.Normal;
			container.assaultModeIndText.text = "ASSAULT";
			container.assaultModeIndText.color = Color.red;
		} else {
			container.assaultModeIndText.fontStyle = FontStyle.Italic;
			container.assaultModeIndText.text = "Stealth";
			container.assaultModeIndText.color = Color.blue;
		}
	}

	public void OnScreenEffect(string message, bool headshot) {
		ResetOnScreenEffect ();
		container.killPopupText.text = message;
		popupIsStarting = true;
		container.killPopupText.enabled = true;
	}

	void ResetOnScreenEffect() {
		container.killPopupText.alpha = 0.1f;
		container.killPopupText.gameObject.transform.localScale = new Vector3 (12f, 8f, 1f);
		killPopupTimer = 0f;
	}

	void OnScreenEffectUpdate() {
		if (container.killPopupText.enabled) {
			killPopupTimer += Time.deltaTime;
			if (popupIsStarting) {
				if (container.killPopupText.alpha < 1f) {
					container.killPopupText.alpha += (Time.deltaTime * 3f);
				}
				container.killPopupText.gameObject.transform.localScale = Vector3.Lerp (new Vector3 (12f, 8f, 1f), new Vector3 (1f, 1f, 1f), killPopupTimer * 3f);
				if (killPopupTimer >= 1.8f) {
					popupIsStarting = false;
				}
			} else {
				if (container.killPopupText.alpha > 0f) {
					container.killPopupText.alpha -= (Time.deltaTime * 3f);
				}
				container.killPopupText.gameObject.transform.localScale = Vector3.Lerp (new Vector3 (1f, 1f, 1f), new Vector3 (12f, 8f, 1f), (killPopupTimer - 1.8f) * 3f);
				if (killPopupTimer >= 2.8f) {
					container.killPopupText.enabled = false;
				}
			}
		}
	}

    void GameOverPopup() {
        if (!container.spectatorText.enabled) {
            container.spectatorText.enabled = true;
        }
        container.spectatorText.text = "Your team has been eliminated. The match will end in " + (int)gameController.endGameTimer;
    }

}
