﻿using System.Collections;
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
    public PlayerActionScript playerActionScript;
		public WeaponActionScript wepActionScript;
    public WeaponScript wepScript;

    private GameControllerScript gameController;
    public Camera myHudMarkerCam1;
    public Camera myHudMarkerCam2;
    public GameObject myHudMarker;

	private ArrayList missionWaypoints;
	private Dictionary<int, GameObject> playerMarkers = new Dictionary<int, GameObject> ();
	private Dictionary<int, GameObject> enemyMarkers = new Dictionary<int, GameObject> ();
	private ArrayList enemyAlertMarkers;
	private ObjectivesTextScript objectiveFormatter;

	// Other vars
	private float killPopupTimer;
	private bool popupIsStarting;
	private bool roundStartFadeIn;
	private bool enemyMarkersCleared;
	private float hitmarkerTimer;
	private float disorientationTimer;
	private float totalDisorientationTime;
	private float detectedTextTimer;
	public bool screenGrab;
	private const float HEIGHT_OFFSET = 1.9f;

    // Use this for initialization
    void Start () {
        if (!GetComponent<PhotonView>().IsMine) {
			myHudMarkerCam1.enabled = false;
			myHudMarkerCam2.enabled = false;
			myHudMarkerCam1.gameObject.SetActive(false);
			myHudMarkerCam2.gameObject.SetActive(false);
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
		container.scoreboard.GetComponent<Canvas> ().enabled = false;
		container.spectatorText.enabled = false;
		ToggleDetectionHUD(false);

		gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
		enemyAlertMarkers = gameController.GetComponent<GameControllerScript>().enemyAlertMarkers;
		killPopupTimer = 0f;
		hitmarkerTimer = 0f;
		detectedTextTimer = 0f;
		popupIsStarting = false;
		screenGrab = false;
		enemyMarkersCleared = false;

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
			ComBoxPopup(7f, "The local Cicada cannibal gang has planted gas bombs to turn the townspeople into minced meat. Let's take care of 'em.");

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

	public void ToggleCrosshair(bool b) {
		if (container != null) {
			container.crosshair.SetActive(b);
		}
	}

	// Update is called once per frame
	void Update () {
		if (gameController == null) {
			gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript> ();
			return;
		}
		container.healthText.text = (container.healthText ? "Health: " + playerActionScript.health : "");
		if (container.staminaBar.isActiveAndEnabled) {
			container.staminaBar.value = (playerActionScript.sprintTime / playerActionScript.playerScript.stamina);
		}

		ToggleScoreboard (Input.GetKey(KeyCode.Tab));

		UpdateHitmarker ();

		// Update UI
		//container.weaponLabelTxt.text = playerActionScript.currWep;
		container.weaponLabelTxt.text = wepScript.equippedWep;
		container.ammoTxt.text = "" + wepActionScript.currentAmmo + '/' + wepActionScript.totalAmmoLeft;
		UpdatePlayerMarkers ();
		UpdateEnemyMarkers();
		UpdateWaypoints ();
		UpdateCursorStatus ();

		if (gameController.gameOver) {
			if (gameController.exitLevelLoaded) {
				ToggleGameOverPopup (false);
				ToggleGameOverBanner (true);
			} else if (PhotonNetwork.CurrentRoom.Players.Count == gameController.deadCount) {
				ToggleGameOverPopup (true);
			}
			ToggleHUD (false);
		} else {
			if (playerActionScript.health > 0 || playerActionScript.isRespawning) {
				ToggleGameOverPopup (false);
			}
		}

        UpdateMissionTimeText();

		// Update kill popups
		OnScreenEffectUpdate ();

		OnStartScreenFade ();

		HandleRespawnBar ();
		UpdateObjectives ();
		FlashbangUpdate();
		UpdateDetectedText();
    }

	void FixedUpdate() {
		// Hierarchy: hit (red) flare takes 1st priority, heal (green) second, boost (yellow) third
		UpdateHitFlare();
		if (!container.hitFlare.GetComponent<RawImage> ().enabled) {
			UpdateHealFlare();
			if (!container.healFlare.GetComponent<RawImage>().enabled) {
				UpdateBoostFlare();
			}
		}
	}

	void UpdateCursorStatus() {
		if (Input.GetKeyDown(KeyCode.Escape) && !container.scoreboard.GetComponent<Canvas>().enabled)
			Pause();

		if (container.pauseMenuGUI.activeInHierarchy)
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
			if (i == missionWaypoints.Count - 1)
			{
				if (playerActionScript.viewCam.enabled) {
					float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
					if (renderCheck <= 0)
						continue;
					if (gameController.bombsRemaining == 0)
					{
						((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = true;
						((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = playerActionScript.viewCam.WorldToScreenPoint(gameController.exitPoint.transform.position);
					}
				} else if (playerActionScript.thisSpectatorCam != null) {
					float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
					if (renderCheck <= 0)
						continue;
					if (gameController.bombsRemaining == 0)
					{
						((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = true;
						((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(gameController.exitPoint.transform.position);
					}
				}
			}
			else
			{
				if (playerActionScript.viewCam.enabled) {
					float renderCheck = Vector3.Dot ((gameController.bombs [i].transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
					if (renderCheck <= 0)
						continue;
					if (!gameController.bombs [i].GetComponent<BombScript> ().defused) {
						Vector3 p = new Vector3 (gameController.bombs [i].transform.position.x, gameController.bombs [i].transform.position.y + gameController.bombs [i].transform.lossyScale.y, gameController.bombs [i].transform.position.z);
						((GameObject)missionWaypoints [i]).GetComponent<RectTransform> ().position = playerActionScript.viewCam.WorldToScreenPoint (p);
					}
					if (((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled && gameController.bombs [i].GetComponent<BombScript> ().defused) {
						((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
					}
				} else if (playerActionScript.thisSpectatorCam != null) {
					float renderCheck = Vector3.Dot ((gameController.bombs [i].transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
					if (renderCheck <= 0)
						continue;
					if (!gameController.bombs [i].GetComponent<BombScript> ().defused) {
						Vector3 p = new Vector3 (gameController.bombs [i].transform.position.x, gameController.bombs [i].transform.position.y + gameController.bombs [i].transform.lossyScale.y, gameController.bombs [i].transform.position.z);
						((GameObject)missionWaypoints [i]).GetComponent<RectTransform> ().position = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint (p);
					}
					if (((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled && gameController.bombs [i].GetComponent<BombScript> ().defused) {
						((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
					}
				}
			}
		}
	}

	void UpdatePlayerMarkers() {
		foreach (GameObject p in GameControllerScript.playerList.Values) {
			if (!p)
				continue;
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
			if (playerActionScript.viewCam.enabled) {
				float renderCheck = Vector3.Dot((p.transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
				if (renderCheck <= 0)
					continue;
				// If the player is alive and on camera, then render the player name and health bar
				if (p.GetComponent<PlayerActionScript>().health > 0)
				{
					playerMarkers [actorNo].SetActive (true);
					playerMarkers[actorNo].GetComponentInChildren<Slider>().value = (((float)p.GetComponent<PlayerActionScript>().health) / 100.0f);
					Vector3 o = new Vector3(p.transform.position.x, p.transform.position.y + HEIGHT_OFFSET, p.transform.position.z);
					playerMarkers[actorNo].GetComponent<RectTransform>().position = playerActionScript.viewCam.WorldToScreenPoint(o);
				}
				if (playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled && p.GetComponent<PlayerActionScript>().health <= 0)
				{
					playerMarkers [actorNo].SetActive (false);
					//playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled = false;
				}
			} else if (playerActionScript.thisSpectatorCam != null) {
				float renderCheck = Vector3.Dot((p.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
				if (renderCheck <= 0)
					continue;
				// If the player is alive and on camera, then render the player name and health bar
				if (p.GetComponent<PlayerActionScript>().health > 0)
				{
					playerMarkers [actorNo].SetActive (true);
					playerMarkers[actorNo].GetComponentInChildren<Slider>().value = (((float)p.GetComponent<PlayerActionScript>().health) / 100.0f);
					Vector3 o = new Vector3(p.transform.position.x, p.transform.position.y + HEIGHT_OFFSET, p.transform.position.z);
					playerMarkers[actorNo].GetComponent<RectTransform>().position = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(o);
				}
				if (playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled && p.GetComponent<PlayerActionScript>().health <= 0)
				{
					playerMarkers [actorNo].SetActive (false);
					//playerMarkers[actorNo].GetComponent<TextMeshProUGUI>().enabled = false;
				}
			}
		}
	}

	void UpdateEnemyMarkers() {
		if (gameController.assaultMode && !enemyMarkersCleared) {
			ClearEnemyMarkers();
			return;
		}
		enemyAlertMarkers = gameController.GetComponent<GameControllerScript>().enemyAlertMarkers;
		foreach (int actorNo in enemyAlertMarkers) {
			if (!enemyMarkers.ContainsKey (actorNo)) {
				GameObject marker = GameObject.Instantiate(container.enemyAlerted);
				marker.GetComponent<RectTransform>().SetParent(container.transform);
				enemyMarkers.Add(actorNo, marker);
			}

			GameObject e = gameController.enemyList[actorNo];
			BetaEnemyScript en = e.GetComponent<BetaEnemyScript>();
			// Check if it can be rendered to the screen
			if (playerActionScript.viewCam.enabled) {
				float renderCheck = Vector3.Dot((e.transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
				if (renderCheck <= 0)
					continue;

				if (en.alertStatus == 2) {
					// if enemy is alerted, display the alert symbol
					if (en.alertDisplay != 2) {
						enemyMarkers[actorNo].SetActive(true);
						en.alertDisplay = 2;
						enemyMarkers[actorNo].GetComponent<RawImage>().texture = (Texture)Resources.Load("alert");
					}
					// Debug.Log(enemyMarkers[actorNo]);
				}
				else if (en.alertStatus == 1) {
					// if enemy is close to player, display the caution symbol
					if (en.alertDisplay != 1) {
						enemyMarkers[actorNo].SetActive(true);
						en.alertDisplay = 1;
						enemyMarkers[actorNo].GetComponent<RawImage>().texture = (Texture)Resources.Load("caution");
					}
				}
				else {
					enemyMarkers[actorNo].SetActive(false);
				}

				Vector3 o = new Vector3(e.transform.position.x, e.transform.position.y + (HEIGHT_OFFSET * 1.5f), e.transform.position.z);
				enemyMarkers[actorNo].GetComponent<RectTransform>().position = playerActionScript.viewCam.WorldToScreenPoint(o);

			} else if (playerActionScript.thisSpectatorCam != null) {
				float renderCheck = Vector3.Dot((e.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
				if (renderCheck <= 0)
					continue;
				if (en.alertStatus == 2) {
					// if enemy is alerted, display the alert symbol
					if (en.alertDisplay != 2) {
						enemyMarkers[actorNo].SetActive(true);
						en.alertDisplay = 2;
						enemyMarkers[actorNo].GetComponent<RawImage>().texture = (Texture)Resources.Load("alert");
					}
				}
				else if (en.alertStatus == 1) {
					// if enemy is close to player, display the caution symbol
					if (en.alertDisplay != 1) {
						enemyMarkers[actorNo].SetActive(true);
						en.alertDisplay = 1;
						enemyMarkers[actorNo].GetComponent<RawImage>().texture = (Texture)Resources.Load("caution");
					}
				}
				else {
					enemyMarkers[actorNo].SetActive(false);
				}
				Vector3 o = new Vector3(e.transform.position.x, e.transform.position.y + (HEIGHT_OFFSET * 1.5f), e.transform.position.z);
				enemyMarkers[actorNo].GetComponent<RectTransform>().position = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(o);
			}
		}
		HandleMarkerRemoval();
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
		if (playerActionScript.hitTimer < 1f) {
			// Disable heal and boost flare if active
			container.healFlare.GetComponent<RawImage>().enabled = false;
			playerActionScript.healTimer = 1f;
			container.boostFlare.GetComponent<RawImage>().enabled = false;
			playerActionScript.boostTimer = 1f;

			// Enable hit flare
			container.hitFlare.GetComponent<RawImage> ().enabled = true;
			float a = -Vector3.Angle (transform.forward, playerActionScript.hitLocation);
			Vector3 temp = container.hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			container.hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
			container.hitDir.GetComponent<RawImage> ().enabled = true;
			playerActionScript.hitTimer += Time.deltaTime;
		} else {
			container.hitFlare.GetComponent<RawImage> ().enabled = false;
			container.hitDir.GetComponent<RawImage> ().enabled = false;
		}
	}

	void UpdateHealFlare() {
		if (playerActionScript.healTimer < 1f) {
			// Disable boost flare if active
			container.boostFlare.GetComponent<RawImage>().enabled = false;
			playerActionScript.boostTimer = 1f;

			// Enable the heal flare
			container.healFlare.GetComponent<RawImage> ().enabled = true;
			playerActionScript.healTimer += Time.deltaTime;
		} else {
			container.healFlare.GetComponent<RawImage> ().enabled = false;
		}
	}

	void UpdateBoostFlare() {
		if (playerActionScript.boostTimer < 1f) {
			container.boostFlare.GetComponent<RawImage> ().enabled = true;
			playerActionScript.boostTimer += Time.deltaTime;
		} else {
			container.boostFlare.GetComponent<RawImage> ().enabled = false;
		}
	}

	public void ToggleHUD(bool b)
    {
        container.healthText.enabled = b;
		container.staminaBar.gameObject.SetActive (b);
        container.weaponLabelTxt.enabled = b;
        container.ammoTxt.enabled = b;
		container.hudMap.enabled = b;
		container.hudMap2.enabled = b;
		container.hintText.enabled = false;
		ToggleCrosshair(false);
    }

	public void ToggleScoreboard(bool b)
    {
		if (playerActionScript.health <= 0) {
			container.healthText.enabled = false;
			container.staminaBar.gameObject.SetActive (false);
			container.weaponLabelTxt.enabled = false;
			container.ammoTxt.enabled = false;
		} else {
			container.healthText.enabled = !b;
			container.staminaBar.gameObject.SetActive (!b);
			container.weaponLabelTxt.enabled = !b;
			container.ammoTxt.enabled = !b;
		}
		container.missionTimeText.enabled = !b;
		container.missionTimeRemainingText.enabled = !b;
		container.assaultModeIndText.enabled = !b;
		container.objectivesText.enabled = !b;
        container.scoreboard.GetComponent<Canvas>().enabled = b;
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

	IEnumerator ShowComBox(float t, string s) {
		yield return new WaitForSeconds (t);
		container.comBox.SetActive (true);
		container.comBoxText.GetComponent<ComboxTextEffect> ().SetText (s);
	}

	public void ComBoxPopup(float t, string s) {
		StartCoroutine (ShowComBox(t, s));
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
		container.objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);
    }

	public void MessagePopup(string message)
    {
		container.missionText.GetComponent<MissionTextAnimScript> ().Reset ();
		container.missionText.GetComponent<Text> ().text = message;
		container.missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

	public void SetActionBarSlider(float val) {
		container.actionBar.GetComponent<Slider> ().value = val;
	}

	public void ToggleSpectatorMessage(bool b) {
		container.spectatorText.text = "You've been eliminated.\nYou can respawn if an ally clears the sector.";
		container.spectatorText.enabled = b;
	}

	//public override void OnPlayerEnteredRoom(Player newPlayer) {
	//	Debug.Log (newPlayer.NickName + " has joined the room");
	//	GameControllerScript.playerList.Add (gameObject);
	//	Debug.Log ("anotha one");
	//}

    private void UpdateMissionTimeText() {
        float totalSecs = GameControllerScript.missionTime;
        int mins = (int)(totalSecs / 60f);
        int remainingSecs = Mathf.RoundToInt((totalSecs - (mins * 60f)));
        container.missionTimeText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));

		// Set remaining time
		mins = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) / 60f);
		remainingSecs = Mathf.RoundToInt(((GameControllerScript.MAX_MISSION_TIME - totalSecs) - (mins * 60f)));
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

	void ToggleGameOverPopup(bool b) {
		if (b) {
			container.spectatorText.text = "Your team has been eliminated. The match will end in " + (int)gameController.endGameTimer;
			container.spectatorText.enabled = true;
		} else {
			container.spectatorText.enabled = false;
		}
    }

	public void RespawnBar() {
		container.respawnBar.value = 0f;
		container.respawnBar.gameObject.SetActive (true);
	}

	void HandleRespawnBar() {
		if (container.respawnBar.gameObject.activeInHierarchy) {
			container.respawnBar.value = ((5f - playerActionScript.respawnTimer) / 5f);
			if (playerActionScript.respawnTimer <= 0f) {
				container.respawnBar.gameObject.SetActive (false);
			}
		}
	}

	void ToggleGameOverBanner(bool b) {
		if (b) {
			playerActionScript.HandleGameOverBanner ();
			container.gameOverBanner.SetActive (true);
		} else {
			container.gameOverBanner.SetActive (false);
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		Destroy (playerMarkers [otherPlayer.ActorNumber]);
		playerMarkers.Remove (otherPlayer.ActorNumber);
	}

	public void FlashbangEffect(float disorientationTime) {
		disorientationTimer = disorientationTime;
		totalDisorientationTime = disorientationTime;
	}

	private void FlashbangUpdate() {
		// Subtract from flashbang effect time
		if (disorientationTimer > 0f) {
			// If beginning of the effect, set white screen and capture the last camera frame to put over
			if (disorientationTimer == totalDisorientationTime) {
				SetFlashbangEffect(true);
			}
			// If 1/3 of the time left, start fading both the white screen and the last camera frame
			float fadeOutPortion = totalDisorientationTime / 3f;
			if (disorientationTimer <= fadeOutPortion) {
				float fadeAmount = disorientationTimer / fadeOutPortion;
				container.flashbangOverlay.color = new Color(1f, 1f, 1f, fadeAmount);
				container.flashbangScreenCap.color = new Color(container.flashbangScreenCap.color.r, container.flashbangScreenCap.color.g, container.flashbangScreenCap.color.b, fadeAmount);
			}
			disorientationTimer -= Time.deltaTime;
		} else {
			SetFlashbangEffect(false);
		}
	}

	private void SetFlashbangEffect(bool b) {
		if (b) {
			// Enable the flashbang effect
			screenGrab = true;
		} else {
			// Disable the flashbang effect
			container.flashbangScreenCap.texture = null;
			container.flashbangScreenCap.enabled = false;
			container.flashbangOverlay.enabled = false;
		}
    }

	// This is called after the camera renders a frame. Put flashbang effect in here to avoid the readpixels error
	public void TriggerFlashbangEffect() {
		// White overlay
		container.flashbangScreenCap.enabled = true;
		container.flashbangOverlay.enabled = true;
		container.flashbangOverlay.color = new Color(1f, 1f, 1f, 1f);

		// Incorrect screen graphics
		container.flashbangScreenCap.enabled = true;
		Texture2D result;
		result = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		result.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
		result.Apply();
		container.flashbangScreenCap.texture = result;
	}

	public void toggleSniperOverlay(bool b) {
		container.SniperOverlay.SetActive(b);
	}

	public void ToggleDetectionHUD(bool on) {
		if (on) {
			container.detectionMeter.enabled = true;
		} else {
			ToggleDetectedText(false);
			container.detectionMeter.enabled = false;
			SetDetectionMeter(0f);
		}
	}

	public void ToggleDetectedText(bool on) {
		if (on) {
			detectedTextTimer = 0f;
			container.detectionText.rectTransform.localScale = Vector3.zero;
			container.detectionText.enabled = true;
		} else {
			container.detectionText.enabled = false;
		}
	}

	void UpdateDetectedText() {
		if (detectedTextTimer < 0.35f) {
			detectedTextTimer += Time.deltaTime;
			float scaleAmount = (detectedTextTimer / 0.35f);
			scaleAmount = (scaleAmount > 1f ? 1f : scaleAmount);
			container.detectionText.rectTransform.localScale = new Vector3(scaleAmount, scaleAmount, scaleAmount);
		}
	}

	public void SetDetectionMeter(float detection) {
		container.detectionMeter.fillAmount = detection;
	}

	void ClearEnemyMarkers() {
		foreach(KeyValuePair<int, GameObject> entry in enemyMarkers)
		{
			Destroy(entry.Value);
		}
		enemyMarkers.Clear();
		enemyMarkersCleared = true;
	}

	void HandleMarkerRemoval() {
		if (gameController.enemyMarkerRemovalQueue.Count > 0) {
			int enemyId = (int)gameController.enemyMarkerRemovalQueue.Dequeue();
			if (enemyMarkers.ContainsKey(enemyId)) {
				Destroy(enemyMarkers[enemyId]);
			}
			enemyMarkers.Remove(enemyId);
		}
	}

}
