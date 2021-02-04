using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
using AlertStatus = BetaEnemyScript.AlertStatus;

public class PlayerHUDScript : MonoBehaviourPunCallbacks {
	private const float COMMAND_DELAY = 0.5f;
	// HUD object reference
	public HUDContainer container;
	public InGameMessengerHUD inGameMessenger;
	// private PauseMenuScript pauseMenuScript;

    // Player reference
    public PlayerActionScript playerActionScript;
	public WeaponActionScript wepActionScript;
    public WeaponScript wepScript;

    private GameControllerScript gameController;
    public Camera myHudMarkerCam1;
    public Camera myHudMarkerCam2;
    public GameObject myHudMarker;
	public GameObject myHudMarker2;

	private ArrayList missionWaypoints;
	private Dictionary<int, GameObject> playerMarkers = new Dictionary<int, GameObject> ();
	private Dictionary<int, AlertMarker> enemyMarkers = new Dictionary<int, AlertMarker> ();

	// Other vars
	public float commandDelay;
	private float killPopupTimer;
	private bool popupIsStarting;
	private bool roundStartFadeIn;
	private bool enemyMarkersCleared;
	private float hitmarkerTimer;
	private float disorientationTimer;
	private float totalDisorientationTime;
	private float detectedTextTimer;
	public bool screenGrab;
	private bool voiceChatActive;
	private const float HEIGHT_OFFSET = 1.9f;
	private enum StatusMode {Team, Ally1, Ally2, Ally3};
	private StatusMode currentStatusMode;
	private bool statusOn;
	private int updateIteration;
	private int deleteIteration;

	private bool initialized;

	void Awake() {
		container = GameObject.FindWithTag ("HUD").GetComponent<HUDContainer> ();
	}

    // Use this for initialization
    public void Initialize () {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
        if (!GetComponent<PhotonView>().IsMine) {
			myHudMarkerCam1.targetTexture = null;
			myHudMarkerCam2.targetTexture = null;
			myHudMarkerCam1.enabled = false;
			myHudMarkerCam2.enabled = false;
			// myHudMarkerCam1.gameObject.SetActive(false);
			// myHudMarkerCam2.gameObject.SetActive(false);
            this.enabled = false;
			initialized = true;
			return;
        }
		// Find/load HUD components
		// gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
		missionWaypoints = new ArrayList ();
		statusOn = true;
		deleteIteration = 7;

		container.hitFlare.GetComponent<RawImage> ().enabled = false;
		container.hitDir.GetComponent<RawImage> ().enabled = false;
		container.hitMarker.GetComponent<RawImage> ().enabled = false;
		// pauseMenuScript = container.pauseMenuGUI.gameObject.GetComponent<PauseMenuScript>();
		container.pauseMenuGUI.GetComponent<PauseMenuScript>().SetPlayerRef(gameObject);

		foreach (int actorId in gameController.enemyList.Keys) {
			GameObject marker = GameObject.Instantiate(container.enemyAlerted);
			marker.GetComponent<RectTransform>().SetParent(container.enemyMarkers.transform);
			marker.SetActive(false);
			AlertMarker m = new AlertMarker(marker, AlertStatus.Neutral);
			enemyMarkers.Add(actorId, m);
		}

		ToggleActionBar(false, null);
		container.hintText.enabled = false;
		container.scoreboard.SetActive(false);
		container.spectatorText.enabled = false;
		ToggleDetectionHUD(false);
		InitHealth();

		if (gameController.matchType == 'C') {
			ToggleVersusHUD(false);
		} else if (gameController.matchType == 'V') {
			ToggleVersusHUD(true);
		}
		killPopupTimer = 0f;
		hitmarkerTimer = 0f;
		detectedTextTimer = 0f;
		popupIsStarting = false;
		screenGrab = false;
		enemyMarkersCleared = false;

		LoadHUDForMission ();
		StartMatchCameraFade ();
		StartCoroutine("UpdatePlayerMarkers");
		StartCoroutine("UpdateEnemyMarkers");
		StartCoroutine("UpdateWaypoints");

		initialized = true;
	}

	public void StartMatchCameraFade() {
		container.screenColor.enabled = true;
		container.screenColor.color = new Color (0f, 0f, 0f, 1f);
		roundStartFadeIn = true;
	}

	void LoadHUDForMission() {
		container.screenColor.color = new Color (0f, 0f, 0f, 1f);
		container.objectivesText = new TextMeshProUGUI[gameController.objectives.objectivesText.Length];
		for (int i = 0; i < gameController.objectives.objectivesText.Length; i++) {
			string o = gameController.objectives.objectivesText[i];
			GameObject objEntry = Instantiate(container.objectiveTextEntry);
			objEntry.GetComponent<ObjectiveEntryScript>().SetObjectiveText(o);
			objEntry.transform.SetParent(container.objectivesTextParent.transform);
			container.objectivesText[i] = objEntry.GetComponent<ObjectiveEntryScript>().objectiveText;
		}
        if (gameController.currentMap == 1) {
			GameObject m1 = GameObject.Instantiate (container.hudWaypoint);
			m1.GetComponent<RawImage>().enabled = false;
			m1.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m2 = GameObject.Instantiate (container.hudWaypoint);
			m2.GetComponent<RawImage>().enabled = false;
			m2.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m3 = GameObject.Instantiate (container.hudWaypoint);
			m3.GetComponent<RawImage>().enabled = false;
			m3.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m4 = GameObject.Instantiate (container.hudWaypoint);
			m4.GetComponent<RawImage>().enabled = false;
			m4.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m5 = GameObject.Instantiate (container.hudWaypoint);
			m5.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			m5.GetComponent<RawImage> ().enabled = false;

			missionWaypoints.Add (m1);
			missionWaypoints.Add (m2);
			missionWaypoints.Add (m3);
			missionWaypoints.Add (m4);
			missionWaypoints.Add (m5);
		} else if (gameController.currentMap == 2) {
			UpdateAssaultModeIndHud(true);
			GameObject m1 = GameObject.Instantiate (container.hudWaypoint);
			m1.GetComponent<RawImage>().enabled = false;
			m1.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m2 = GameObject.Instantiate (container.hudWaypoint);
			m2.GetComponent<RawImage>().enabled = false;
			m2.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m3 = GameObject.Instantiate (container.hudWaypoint);
			m3.GetComponent<RawImage>().enabled = false;
			m3.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m4 = GameObject.Instantiate (container.hudWaypoint);
			m4.GetComponent<RawImage>().enabled = false;
			m4.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);
			GameObject m5 = GameObject.Instantiate (container.hudWaypoint);
			m5.GetComponent<RawImage>().enabled = false;
			m5.GetComponent<RectTransform> ().SetParent (container.waypointMarkers.transform);

			missionWaypoints.Add (m1);
			missionWaypoints.Add (m2);
			missionWaypoints.Add (m3);
			missionWaypoints.Add (m4);
			missionWaypoints.Add (m5);
		}
		StartCoroutine(ShowMissionText(gameController.currentMap));
		if (PhotonNetwork.IsMasterClient) {
			InitialComBoxRoutineForMission(gameController.currentMap);
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

	void HandleStatusPanels()
	{
		if (Input.GetKeyDown(KeyCode.Alpha9)) {
			// Turn the statuses on/off
			statusOn = !statusOn;
			if (!statusOn) {
				foreach (AllyHealthBar a in container.teamHealthPage) {
					a.gameObject.SetActive(false);
				}
				foreach (AllyHealthBar a in container.allyHealthPage1) {
					a.gameObject.SetActive(false);
				}
				foreach (AllyHealthBar a in container.allyHealthPage2) {
					a.gameObject.SetActive(false);
				}
				foreach (AllyHealthBar a in container.allyHealthPage3) {
					a.gameObject.SetActive(false);
				}
				container.statusHiddenText.text = "[9] SHOW STATUS";
			} else {
				container.statusHiddenText.text = "[9] HIDE STATUS";
				RepopulateStatusForCurrentMode();
			}
		} else if (Input.GetKeyDown(KeyCode.Alpha0)) {
			if (statusOn) {
				SwitchStatusPage(true);
			}
		} else if (Input.GetKeyDown(KeyCode.Alpha8)) {
			if (statusOn) {
				SwitchStatusPage(false);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (!initialized) {
			return;
		}
		if (gameController == null) {
			gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript> ();
			return;
		}
		HandleVoiceChat();
		HandleVoiceCommands();
		UpdateVoteUI();
		UpdateHealth();
		if (container.staminaGroup.alpha == 1f) {
			float f = (playerActionScript.sprintTime / playerActionScript.playerScript.stamina);
			container.staminaBar.value = f;
			// container.staminaPercentTxt.text = ((int)(f * 100f)) + "%";
		}

		HandleStatusPanels();
		
		if (statusOn) {
			HandleAllyHealthBars();
			HandleTeamHealthBars();
		}

		ToggleScoreboard (PlayerPreferences.playerPreferences.KeyWasPressed("Scoreboard", true));

		UpdateHitmarker ();

		container.ammoTxt.textObject.text = "" + wepActionScript.currentAmmo + '/' + wepActionScript.totalAmmoLeft;
		
		UpdateCursorStatus ();
		if (gameController.matchType == 'V') {
			UpdateRedTeamScore(Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redScore"]));
			UpdateBlueTeamScore(Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueScore"]));
			HandleGameOverPopupsForVersus();
		} else if (gameController.matchType == 'C') {
			HandleGameOverPopupsForCampaign();
		}

        UpdateMissionTimeText();

		// Update kill popups
		OnScreenEffectUpdate ();

		OnStartScreenFade ();

		HandleRespawnBar ();
		// UpdateObjectives ();
		FlashbangUpdate();
		UpdateDetectedText();
		UpdateCarryingText();

    }

	void FixedUpdate() {
		if (!initialized) {
			return;
		}
		// Hierarchy: hit (red) flare takes 1st priority, heal (green) second, boost (yellow) third
		UpdateHitFlare();
		if (!container.hitFlare.GetComponent<RawImage> ().enabled) {
			UpdateHealFlare();
			if (!container.healFlare.GetComponent<RawImage>().enabled) {
				UpdateBoostFlare();
			}
		}
	}

	void SwitchStatusPage(bool increase)
	{
		deleteIteration = 7;
		if (currentStatusMode == StatusMode.Team) {
			updateIteration = 0;
			foreach (AllyHealthBar a in container.teamHealthPage) {
				a.gameObject.SetActive(false);
			}
			if (increase) {
				currentStatusMode = StatusMode.Ally1;
			} else {
				currentStatusMode = StatusMode.Ally3;
			}
		} else if (currentStatusMode == StatusMode.Ally1) {
			updateIteration = 0;
			foreach (AllyHealthBar a in container.allyHealthPage1) {
				a.gameObject.SetActive(false);
			}
			if (increase) {
				currentStatusMode = StatusMode.Ally2;
			} else {
				currentStatusMode = StatusMode.Team;
			}
		} else if (currentStatusMode == StatusMode.Ally2) {
			updateIteration = 8;
			foreach (AllyHealthBar a in container.allyHealthPage2) {
				a.gameObject.SetActive(false);
			}
			if (increase) {
				currentStatusMode = StatusMode.Ally3;
			} else {
				currentStatusMode = StatusMode.Ally1;
			}
		} else if (currentStatusMode == StatusMode.Ally3) {
			updateIteration = 16;
			foreach (AllyHealthBar a in container.allyHealthPage3) {
				a.gameObject.SetActive(false);
			}
			if (increase) {
				currentStatusMode = StatusMode.Team;
			} else {
				currentStatusMode = StatusMode.Ally2;
			}
		}

		RepopulateStatusForCurrentMode();
	}

	void RepopulateStatusForCurrentMode()
	{
		if (currentStatusMode == StatusMode.Team) {
			container.statusPageTitle.text = "TEAM STATUS";
			foreach (AllyHealthBar a in container.teamHealthPage) {
				if (a.viewId != -1) {
					a.gameObject.SetActive(true);
				}
			}
		} else if (currentStatusMode == StatusMode.Ally1) {
			container.statusPageTitle.text = "ALLY STATUS 1";
			foreach (AllyHealthBar a in container.allyHealthPage1) {
				if (a.viewId != -1) {
					a.gameObject.SetActive(true);
				}
			}
		} else if (currentStatusMode == StatusMode.Ally2) {
			container.statusPageTitle.text = "ALLY STATUS 2";
			foreach (AllyHealthBar a in container.allyHealthPage2) {
				if (a.viewId != -1) {
					a.gameObject.SetActive(true);
				}
			}
		} else if (currentStatusMode == StatusMode.Ally3) {
			container.statusPageTitle.text = "ALLY STATUS 3";
			foreach (AllyHealthBar a in container.allyHealthPage3) {
				if (a.viewId != -1) {
					a.gameObject.SetActive(true);
				}
			}
		}
	}

	void HandleAllyHealthBars()
	{
		if (currentStatusMode == StatusMode.Ally1) {
			int i = -1;
			try {
				i = gameController.npcList.Keys.ElementAt(updateIteration++);
			} catch (Exception e) {
				updateIteration = 0;
			}
			if (updateIteration > 7) {
				updateIteration = 0;
			}
			// In case there are no NPCs
			if (i != -1) {
				NpcScript toUpdate = gameController.npcList[i].GetComponent<NpcScript>();
				UpdateNpcSlot(i, toUpdate.npcName, toUpdate.health, 0);
			}

			// Loop back through current list
			// Delete players that are no longer in the game
			AllyHealthBar toDel = container.allyHealthPage1[deleteIteration--];
			if (deleteIteration < 0) {
				deleteIteration = 7;
			}
			if (toDel.viewId != -1 && !gameController.npcList.ContainsKey(toDel.viewId)) {
				toDel.ResetData();
			}
		} else if (currentStatusMode == StatusMode.Ally2) {
			if (gameController.npcList.Count <= 8) {
				return;
			}
			
			int i = -1;
			try {
				i = gameController.npcList.Keys.ElementAt(updateIteration++);
			} catch (Exception e) {
				updateIteration = 8;
			}
			if (updateIteration > 15) {
				updateIteration = 8;
			}
			// In case there are no NPCs
			if (i != -1) {
				NpcScript toUpdate = gameController.npcList[i].GetComponent<NpcScript>();
				UpdateNpcSlot(i, toUpdate.npcName, toUpdate.health, 1);
			}

			// Loop back through current list
			// Delete players that are no longer in the game
			AllyHealthBar toDel = container.allyHealthPage2[deleteIteration--];
			if (deleteIteration < 0) {
				deleteIteration = 7;
			}
			if (toDel.viewId != -1 && !gameController.npcList.ContainsKey(toDel.viewId)) {
				toDel.ResetData();
			}
		} else if (currentStatusMode == StatusMode.Ally3) {
			if (gameController.npcList.Count <= 16) {
				return;
			}

			int i = -1;
			try {
				i = gameController.npcList.Keys.ElementAt(updateIteration++);
			} catch (Exception e) {
				updateIteration = 16;
			}
			if (updateIteration > 23) {
				updateIteration = 16;
			}
			// In case there are no NPCs
			if (i != -1) {
				NpcScript toUpdate = gameController.npcList[i].GetComponent<NpcScript>();
				UpdateNpcSlot(i, toUpdate.npcName, toUpdate.health, 2);
			}

			// Loop back through current list
			// Delete players that are no longer in the game
			AllyHealthBar toDel = container.allyHealthPage3[deleteIteration--];
			if (deleteIteration < 0) {
				deleteIteration = 7;
			}
			if (toDel.viewId != -1 && !gameController.npcList.ContainsKey(toDel.viewId)) {
				toDel.ResetData();
			}
		}
	}

	void HandleTeamHealthBars()
	{
		// If player health list is active
		if (currentStatusMode == StatusMode.Team) {
			// Loop through player list
			int i = -1;
			try {
				i = GameControllerScript.playerList.Keys.ElementAt(updateIteration++);
			} catch (Exception e) {
				updateIteration = 0;
			}
			if (updateIteration > 7) {
				updateIteration = 0;
			}
			if (i != -1) {
				PlayerStat toUpdate = GameControllerScript.playerList[i];
				if (toUpdate.actorId != PhotonNetwork.LocalPlayer.ActorNumber) {
					// Add player names and health that aren't in the list
					// Update player health that is already on the list
					UpdatePlayerSlot(i, toUpdate.name, toUpdate.objRef.GetComponent<PlayerActionScript>().health);
				}
			}

			// Loop back through current list
			// Delete players that are no longer in the game
			AllyHealthBar toDel = container.teamHealthPage[deleteIteration--];
			if (deleteIteration < 0) {
				deleteIteration = 7;
			}
			if (toDel.viewId != -1 && !GameControllerScript.playerList.ContainsKey(toDel.viewId)) {
				toDel.ResetData();
			}
		}
	}

	void UpdatePlayerSlot(int viewId, string playerName, int health)
	{
		AllyHealthBar lastEmptySlot = null;
		foreach (AllyHealthBar a in container.teamHealthPage) {
			if (lastEmptySlot == null && a.viewId == -1) {
				lastEmptySlot = a;
			} else if (a.viewId == viewId) {
				a.SetHealth(health);
				return;
			}
		}
		// Else, not currently in list, so add it
		if (lastEmptySlot != null) {
			lastEmptySlot.gameObject.SetActive(true);
			lastEmptySlot.InitData(viewId, playerName, health);
		}
	}

	void UpdateNpcSlot(int viewId, string npcName, int health, int page) {
		if (page == 0) {
			AllyHealthBar lastEmptySlot = null;
			foreach (AllyHealthBar a in container.allyHealthPage1) {
				if (lastEmptySlot == null && (a.viewId == -1 || a.healthSlider.value == 0f)) {
					lastEmptySlot = a;
				} else if (a.viewId == viewId) {
					a.SetHealth(health);
					return;
				}
			}
			// Else, not currently in list, so add it
			if (lastEmptySlot != null) {
				lastEmptySlot.gameObject.SetActive(true);
				lastEmptySlot.InitData(viewId, npcName, health);
			}
		} else if (page == 1) {
			AllyHealthBar lastEmptySlot = null;
			foreach (AllyHealthBar a in container.allyHealthPage2) {
				if (lastEmptySlot == null && (a.viewId == -1 || a.healthSlider.value == 0f)) {
					lastEmptySlot = a;
				} else if (a.viewId == viewId) {
					a.SetHealth(health);
					return;
				}
			}
			// Else, not currently in list, so add it
			if (lastEmptySlot != null) {
				lastEmptySlot.gameObject.SetActive(true);
				lastEmptySlot.InitData(viewId, npcName, health);
			}
		} else if (page == 2) {
			AllyHealthBar lastEmptySlot = null;
			foreach (AllyHealthBar a in container.allyHealthPage3) {
				if (lastEmptySlot == null && (a.viewId == -1 || a.healthSlider.value == 0f)) {
					lastEmptySlot = a;
				} else if (a.viewId == viewId) {
					a.SetHealth(health);
					return;
				}
			}
			// Else, not currently in list, so add it
			if (lastEmptySlot != null) {
				lastEmptySlot.gameObject.SetActive(true);
				lastEmptySlot.InitData(viewId, npcName, health);
			}
		}
	}

	void HandleGameOverPopupsForCampaign() {
		if (gameController.gameOver) {
			if (gameController.exitLevelLoaded) {
				ToggleGameOverPopup (false);
				ToggleGameOverBanner (true);
			} else if (container.spectatorText.enabled && PhotonNetwork.CurrentRoom.Players.Count == gameController.GetDeadCount()) {
				ToggleGameOverPopup (true);
			}
			ToggleHUD (false);
		} else {
			if (playerActionScript.health > 0 || playerActionScript.isRespawning) {
				ToggleGameOverPopup (false);
			}
		}
	}

	void HandleGameOverPopupsForVersus() {
		if (gameController.gameOver) {
			if (gameController.exitLevelLoaded) {
				ToggleGameOverPopup (false);
				ToggleGameOverBanner (true);
			} else if (container.spectatorText.enabled && (gameController.teamMap == "R" && gameController.GetRedTeamCount() == gameController.GetDeadCount()) || (gameController.teamMap == "B" && gameController.GetBlueTeamCount() == gameController.GetDeadCount())) {
				ToggleGameOverPopup (true);
			}
			ToggleHUD (false);
		} else {
			if (playerActionScript.health > 0 || playerActionScript.isRespawning) {
				ToggleGameOverPopup (false);
			}
		}
	}

	bool CanPause() {
		if (wepActionScript.isCocking || wepActionScript.isDrawing || wepActionScript.isMeleeing || wepActionScript.isFiring || wepActionScript.isAiming || wepActionScript.isCockingGrenade 
			|| wepActionScript.deployInProgress || wepActionScript.isUsingBooster || wepActionScript.isUsingDeployable || wepActionScript.isReloading || wepActionScript.fpc.m_IsRunning
			|| container.scoreboard.activeInHierarchy) {
				return false;
			}
		return true;
	}

	void UpdateCursorStatus() {
		if (PlayerPreferences.playerPreferences.KeyWasPressed("Pause") && CanPause()) {
			if (container.inGameMessenger.inputText.enabled) {
				inGameMessenger.CloseTextChat();
			} else {
				Pause();
			}
		}

		// if (container.pauseMenuManager.pauseActive)
		// {
		// 	Cursor.lockState = CursorLockMode.None;
		// 	Cursor.visible = true;
		// }
		// else
		// {
		// 	Cursor.lockState = CursorLockMode.Locked;
		// 	Cursor.visible = false;
		// }
	}

	IEnumerator UpdateWaypoints() {
		HandleWaypointsForMission();
		yield return new WaitForSeconds(0.025f);
		StartCoroutine("UpdateWaypoints");
	}

	void HandleWaypointsForMission() {
		if (gameController.currentMap == 1) {
			for (int i = 0; i < missionWaypoints.Count; i++)
			{
				if (i == missionWaypoints.Count - 1)
				{
					if (playerActionScript.viewCam.enabled) {
						float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
						if (renderCheck <= 0)
							continue;
						if (gameController.objectives.itemsRemaining == 0)
						{
							HandleWaypointRender(i, false, gameController.exitPoint.transform.position);
						}
					} else if (playerActionScript.thisSpectatorCam != null) {
						float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
						if (renderCheck <= 0)
							continue;
						if (gameController.objectives.itemsRemaining == 0)
						{
							HandleWaypointRender(i, true, gameController.exitPoint.transform.position);
						}
					}
				}
				else
				{
					if (playerActionScript.viewCam.enabled) {
						float renderCheck = Vector3.Dot ((gameController.items [i].transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
						if (renderCheck <= 0)
							continue;
						if (!gameController.items [i].GetComponent<BombScript> ().defused) {
							HandleWaypointRender(i, false, new Vector3 (gameController.items [i].transform.position.x, gameController.items [i].transform.position.y + gameController.items [i].transform.lossyScale.y, gameController.items [i].transform.position.z));
						}
						if (gameController.items [i].GetComponent<BombScript> ().defused) {
							((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
						}
					} else if (playerActionScript.thisSpectatorCam != null) {
						float renderCheck = Vector3.Dot ((gameController.items [i].transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
						if (renderCheck <= 0)
							continue;
						if (!gameController.items [i].GetComponent<BombScript> ().defused) {
							HandleWaypointRender(i, true, new Vector3 (gameController.items [i].transform.position.x, gameController.items [i].transform.position.y + gameController.items [i].transform.lossyScale.y, gameController.items [i].transform.position.z));
						}
						if (gameController.items [i].GetComponent<BombScript> ().defused) {
							((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
						}
					}
				}
			}
		} else if (gameController.currentMap == 2) {
			if (gameController.objectives.stepsLeftToCompletion <= 4) {
				// Render marker over pilot
				Vector3 a = new Vector3(gameController.vipRef.transform.position.x, gameController.vipRef.transform.position.y + 3f, gameController.vipRef.transform.position.z);
				if (playerActionScript.viewCam.enabled) {
					float renderCheck = Vector3.Dot((a - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
					if (renderCheck > 0) {
						HandleWaypointRender(0, false, a);
					}
				} else if (playerActionScript.thisSpectatorCam != null) {
					float renderCheck = Vector3.Dot((a - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
					if (renderCheck > 0) {
						HandleWaypointRender(0, true, a);
					}
				}
			}
			if (gameController.objectives.stepsLeftToCompletion == 3) {
				// Render marker over town
				Vector3 a = new Vector3(gameController.checkpointRef.transform.position.x, gameController.checkpointRef.transform.position.y + 8f, gameController.checkpointRef.transform.position.z);
				if (playerActionScript.viewCam.enabled) {
					float renderCheck = Vector3.Dot((a - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
					if (renderCheck > 0) {
						HandleWaypointRender(1, false, a);
					}
				} else if (playerActionScript.thisSpectatorCam != null) {
					float renderCheck = Vector3.Dot((a - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
					if (renderCheck > 0) {
						HandleWaypointRender(1, true, a);
					}
				}
			} else if (gameController.objectives.stepsLeftToCompletion == 2) {
				// Render over all possible evac spots
				((GameObject)missionWaypoints [1]).GetComponent<RawImage> ().enabled = false;
				for (int i = 2; i < 5; i++) {
					GameObject w = gameController.items[i - 2];
					Vector3 a = new Vector3(w.transform.position.x, w.transform.position.y + 3f, w.transform.position.z);
					if (playerActionScript.viewCam.enabled) {
						float renderCheck = Vector3.Dot((a - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
						if (renderCheck > 0) {
							HandleWaypointRender(i, false, a);
						}
					} else if (playerActionScript.thisSpectatorCam != null) {
						float renderCheck = Vector3.Dot((a - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
						if (renderCheck > 0) {
							HandleWaypointRender(i, true, a);
						}
					}
				}
			} else if (gameController.objectives.stepsLeftToCompletion == 1) {
				// Render marker over selected evac spot
				((GameObject)missionWaypoints [1]).GetComponent<RawImage> ().enabled = false;
				for (int i = 2; i < 5; i++) {
					if ((i - 2) != gameController.objectives.selectedEvacIndex) {
						((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
					} else {
						// if (gameController.exitPoint == null || gameController.objectives.missionTimer3 > 0f) {
						// 	((GameObject)missionWaypoints [gameController.objectives.selectedEvacIndex]).GetComponent<RawImage> ().enabled = false;
						// 	continue;
						// }
						Vector3 a = new Vector3(gameController.exitPoint.transform.position.x, gameController.exitPoint.transform.position.y + 3f, gameController.exitPoint.transform.position.z);
						if (playerActionScript.viewCam.enabled) {
							float renderCheck = Vector3.Dot((a - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
							if (renderCheck > 0) {
								HandleWaypointRender(i, false, a);
							}
						} else if (playerActionScript.thisSpectatorCam != null) {
							float renderCheck = Vector3.Dot((a - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
							if (renderCheck > 0) {
								HandleWaypointRender(i, true, a);
							}
						}
					}
				}
			}
		}
	}

	void HandleWaypointRender(int i, bool useSpectatorCam, Vector3 target) {
		if (!useSpectatorCam) {
			((GameObject)missionWaypoints [i]).GetComponent<RawImage>().enabled = true;
			RectTransform missionWaypointTrans = ((GameObject)missionWaypoints [i]).GetComponent<RectTransform> ();
			Vector3 destPoint = playerActionScript.viewCam.WorldToScreenPoint (target);
			Vector3 startPoint = missionWaypointTrans.position;
			missionWaypointTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
		} else {
			((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = true;
			RectTransform missionWaypointTrans = ((GameObject)missionWaypoints[i]).GetComponent<RectTransform>();
			Vector3 destPoint = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(target);
			Vector3 startPoint = missionWaypointTrans.position;
			missionWaypointTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
		}
	}

	public void SetFireMode(string mode)
	{
		if (mode == null) {
			container.fireModeTxt.gameObject.SetActive(false);
		} else {
			container.fireModeTxt.gameObject.SetActive(true);
			container.fireModeTxt.textObject.text = mode;
		}
	}

	public void SetWeaponLabel()
	{
		container.weaponLabelTxt.textObject.text = wepScript.equippedWepInGame;
	}

	IEnumerator UpdatePlayerMarkers() {
		foreach (PlayerStat stat in GameControllerScript.playerList.Values) {
			GameObject p = stat.objRef;
			if (!p)
				continue;
			int actorNo = p.GetComponent<PhotonView> ().Owner.ActorNumber;
			if (actorNo == PhotonNetwork.LocalPlayer.ActorNumber) {
				continue;
			}
			if (!playerMarkers.ContainsKey (actorNo)) {
				GameObject marker = GameObject.Instantiate (container.hudPlayerMarker);
				PlayerMarkerScript pms = marker.GetComponent<PlayerMarkerScript>();
				pms.nametagRef.text = p.GetComponent<PhotonView> ().Owner.NickName;
				pms.rankInsigniaRef.texture = PlayerData.playerdata.GetRankInsigniaForRank(PlayerData.playerdata.GetRankFromExp(stat.exp).name);
				marker.GetComponent<RectTransform> ().SetParent (container.playerMarkers.transform);
				marker.SetActive(false);
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
					playerMarkers[actorNo].GetComponent<PlayerMarkerScript>().healthbarRef.value = (((float)p.GetComponent<PlayerActionScript>().health) / 100.0f);
					Vector3 o = new Vector3(p.transform.position.x, p.transform.position.y + HEIGHT_OFFSET, p.transform.position.z);
					RectTransform playerMarkerTrans = playerMarkers[actorNo].GetComponent<RectTransform>();
					Vector3 destPoint = playerActionScript.viewCam.WorldToScreenPoint(o);
					Vector3 startPoint = playerMarkerTrans.position;
					playerMarkerTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
				}
				if (playerMarkers[actorNo].GetComponent<PlayerMarkerScript>().nametagRef.enabled && p.GetComponent<PlayerActionScript>().health <= 0)
				{
					playerMarkers [actorNo].SetActive (false);
				}
			} else if (playerActionScript.thisSpectatorCam != null) {
				float renderCheck = Vector3.Dot((p.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
				if (renderCheck <= 0)
					continue;
				// If the player is alive and on camera, then render the player name and health bar
				if (p.GetComponent<PlayerActionScript>().health > 0)
				{
					playerMarkers [actorNo].SetActive (true);
					playerMarkers[actorNo].GetComponent<PlayerMarkerScript>().healthbarRef.value = (((float)p.GetComponent<PlayerActionScript>().health) / 100.0f);
					Vector3 o = new Vector3(p.transform.position.x, p.transform.position.y + HEIGHT_OFFSET, p.transform.position.z);
					RectTransform playerMarkerTrans = playerMarkers[actorNo].GetComponent<RectTransform>();
					Vector3 destPoint = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(o);
					Vector3 startPoint = playerMarkerTrans.position;
					playerMarkerTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
				}
				if (playerMarkers[actorNo].GetComponent<PlayerMarkerScript>().nametagRef.enabled && p.GetComponent<PlayerActionScript>().health <= 0)
				{
					playerMarkers [actorNo].SetActive (false);
				}
			}
		}
		yield return new WaitForSeconds(0.025f);
		StartCoroutine("UpdatePlayerMarkers");
	}

	IEnumerator UpdateEnemyMarkers() {
		if (gameController.assaultMode && !enemyMarkersCleared) {
			ClearEnemyMarkers();
			yield return null;
		}
		foreach (KeyValuePair<int, AlertMarker> marker in enemyMarkers) {
			int actorNo = marker.Key;
			GameObject e = gameController.enemyList[actorNo];
			BetaEnemyScript en = e.GetComponent<BetaEnemyScript>();
			if (en.health <= 0) {
				enemyMarkers[actorNo].markerRef.SetActive(false);
				continue;
			}
			// Check if it can be rendered to the screen
			if (playerActionScript.viewCam.enabled) {
				if (en.alertStatus == AlertStatus.Alert) {
					// if enemy is alerted, display the alert symbol
					if (enemyMarkers[actorNo].alertStatus != AlertStatus.Alert) {
						enemyMarkers[actorNo].markerRef.SetActive(true);
						enemyMarkers[actorNo].markerRef.GetComponent<RawImage>().texture = container.alertSymbol;
						enemyMarkers[actorNo].alertStatus = AlertStatus.Alert;
					}
				} else if (en.alertStatus == AlertStatus.Suspicious) {
					// if enemy is suspicious, display the suspicious symbol
					if (enemyMarkers[actorNo].alertStatus != AlertStatus.Suspicious) {
						enemyMarkers[actorNo].markerRef.SetActive(true);
						enemyMarkers[actorNo].markerRef.GetComponent<RawImage>().texture = container.suspiciousSymbol;
						enemyMarkers[actorNo].alertStatus = AlertStatus.Suspicious;
					}
				} else {
					enemyMarkers[actorNo].markerRef.SetActive(false);
					enemyMarkers[actorNo].alertStatus = AlertStatus.Neutral;
					continue;
				}

				float renderCheck = Vector3.Dot((e.transform.position - playerActionScript.viewCam.transform.position).normalized, playerActionScript.viewCam.transform.forward);
				if (renderCheck <= 0)
					continue;

				if (enemyMarkers[actorNo].markerRef.activeInHierarchy) {
					Vector3 o = new Vector3(e.transform.position.x, e.transform.position.y + (HEIGHT_OFFSET * 1.5f), e.transform.position.z);
					RectTransform enemyMarkerTrans = enemyMarkers[actorNo].markerRef.GetComponent<RectTransform>();
					Vector3 destPoint = playerActionScript.viewCam.WorldToScreenPoint(o);
					Vector3 startPoint = enemyMarkerTrans.position;
					enemyMarkerTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
				}
			} else if (playerActionScript.thisSpectatorCam != null) {
				if (en.alertStatus == AlertStatus.Alert) {
					// if enemy is alerted, display the alert symbol
					if (enemyMarkers[actorNo].alertStatus != AlertStatus.Alert) {
						enemyMarkers[actorNo].markerRef.SetActive(true);
						enemyMarkers[actorNo].markerRef.GetComponent<RawImage>().texture = container.alertSymbol;
						enemyMarkers[actorNo].alertStatus = AlertStatus.Alert;
					}
				} else if (en.alertStatus == AlertStatus.Suspicious) {
					// if enemy is suspicious, display the suspicious symbol
					if (enemyMarkers[actorNo].alertStatus != AlertStatus.Suspicious) {
						enemyMarkers[actorNo].markerRef.SetActive(true);
						enemyMarkers[actorNo].markerRef.GetComponent<RawImage>().texture = container.suspiciousSymbol;
						enemyMarkers[actorNo].alertStatus = AlertStatus.Suspicious;
					}
				} else {
					enemyMarkers[actorNo].markerRef.SetActive(false);
					enemyMarkers[actorNo].alertStatus = AlertStatus.Neutral;
					continue;
				}

				float renderCheck = Vector3.Dot((e.transform.position - playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.position).normalized, playerActionScript.thisSpectatorCam.GetComponent<Camera>().transform.forward);
				if (renderCheck <= 0)
					continue;

				if (enemyMarkers[actorNo].markerRef.activeInHierarchy) {
					Vector3 o = new Vector3(e.transform.position.x, e.transform.position.y + (HEIGHT_OFFSET * 1.5f), e.transform.position.z);
					RectTransform enemyMarkerTrans = enemyMarkers[actorNo].markerRef.GetComponent<RectTransform>();
					Vector3 destPoint = playerActionScript.thisSpectatorCam.GetComponent<Camera>().WorldToScreenPoint(o);
					Vector3 startPoint = enemyMarkerTrans.position;
					enemyMarkerTrans.position = Vector3.Slerp(startPoint, destPoint, Time.deltaTime * 20f);
				}
			}
		}
		yield return new WaitForSeconds(0.025f);
		StartCoroutine("UpdateEnemyMarkers");
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

	int GetAngleSide(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1;
		} else if (dir < 0f) {
			return -1;
		}

		return 0;
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
			// Vector3 hitDirectionVector = transform.position - playerActionScript.hitLocation;
			Vector3 hitDirectionVector = playerActionScript.hitLocation - transform.position;
			float a = Vector3.Angle (playerActionScript.viewCam.gameObject.transform.forward, hitDirectionVector);
			float dir = GetAngleSide(playerActionScript.viewCam.gameObject.transform.forward, hitDirectionVector, playerActionScript.viewCam.gameObject.transform.up);
			if (dir == 1) {
				a = 360f - a;
			}
			// Debug.Log(a);
			Vector3 temp = container.hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			container.hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(0f,0f,a));
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
		if (b && container.timeGroup.activeInHierarchy) {
			return;
		}
		if (!b && !container.timeGroup.activeInHierarchy) {
			return;
		}
        container.healthGroup.alpha = (b ? 1f : 0f);
		container.staminaGroup.alpha = (b ? 1f : 0f);
        container.weaponLabelTxt.gameObject.SetActive(b);
        container.ammoTxt.gameObject.SetActive(b);
		container.minimapGroup.SetActive(b);
		container.timeGroup.SetActive(b);
		container.hintText.enabled = false;
		if (!b) {
			container.itemCarryingText.text = null;
			ToggleCrosshair(false);
		}
    }

	public void ToggleScoreboard(bool b)
    {
		if (playerActionScript.health <= 0) {
			container.healthGroup.alpha = 0f;
			container.staminaGroup.alpha = 0f;
			container.weaponLabelTxt.gameObject.SetActive(false);
			container.ammoTxt.gameObject.SetActive(false);
		} else {
			container.healthGroup.alpha = (!b ? 1f : 0f);
			container.staminaGroup.alpha = (!b ? 1f : 0f);
			container.weaponLabelTxt.gameObject.SetActive(!b);
			container.ammoTxt.gameObject.SetActive(!b);
		}
		container.missionTimeText.enabled = !b;
		container.missionTimeRemainingText.enabled = !b;
		container.assaultModeIndText.enabled = !b;
		container.objectivesTextParent.SetActive(!b);
        container.scoreboard.SetActive(b);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Title");
		PhotonNetwork.Disconnect ();
    }

    void Pause()
    {
		container.voiceCommandsPanel.SetActive(false);
        if (!container.pauseMenuManager.pauseActive)
        {
            container.pauseMenuManager.OpenPause();
        } else {
			container.pauseMenuScript.HandleEscape();
		}
    }

    IEnumerator ShowMissionText(int mission)
    {
        yield return new WaitForSeconds(5f);
		if (mission == 1) {
			MessagePopup("Collaborate with allies and carry out the mission!");
		} else if (mission == 2) {
			MessagePopup("Check the cockpit for survivors!");
		}
    }

	void InitialComBoxRoutineForMission(int mission) {
		if (mission == 1) {
			ComBoxPopup(7f, "Democko", "The local Cicada cannibal gang has planted gas bombs to turn the townspeople into minced meat. Let's take care of 'em.", "HUD/democko");
		} else if (mission == 2) {
			ComBoxPopup(4f, "Democko", "Alpha team! What the hell just happened?! Give me a sitrep!", "HUD/democko");
			ComBoxPopup(9f, "Red Ruby", "Tail rotor got shot out with an RPG!", "HUD/redruby");
			ComBoxPopup(11f, "Democko", "Damn it! Give me a status report!", "HUD/democko");
			ComBoxPopup(14f, "Red Ruby", "I’m alive; leg is fractured! I’m stuck in the pilot seat! The rest of the team seems to be okay! This chopper’s combusting pretty fast!", "HUD/redruby");
			ComBoxPopup(20f, "Democko", "Okay, just stay calm! Alpha team, get her out of there to safety! I’m dispatching another chopper to get you guys out of there ASAP!", "HUD/democko");
			ComBoxPopup(29f, "Red Ruby", "Double time it, please! We’re sitting ducks out here!", "HUD/redruby");
			ComBoxPopup(35f, "Democko", "Roger that, it’s on the way! Just sit tight!", "HUD/democko");
			ComBoxPopup(40f, "Democko", "You guys won’t get far on foot. I recommend you set up a perimeter in the nearby buildings and defend yourselves until the chopper arrives. ETA is approximately 15 minutes!", "HUD/democko");
		}
	}

	IEnumerator ShowComBox(float t, string speaker, string s, string picPath) {
		yield return new WaitForSeconds (t);
		playerActionScript.NetworkComBoxMessage(speaker, s, picPath);
	}

	public void DisplayComBox(string speaker, string message, string picPath) {
		container.comBox.SetActive (true);
		container.comBoxText.GetComponent<ComboxTextEffect> ().SetText (message, speaker, picPath);
	}

	public void ComBoxPopup(float t, string speaker, string s, string picPath) {
		StartCoroutine (ShowComBox(t, speaker, s, picPath));
	}

    public void ToggleActionBar(bool enable, string actionText)
    {
		container.actionBar.SetActive(enable);
		container.actionBarText.text = actionText;
    }

    public void UpdateObjectives()
    {
		if (gameController.updateObjectivesFlag) {
			for (int i = 0; i < gameController.objectives.objectivesText.Length; i++) {
				string o = gameController.objectives.objectivesText[i];
				container.objectivesText[i].text = o;
			}
			gameController.updateObjectivesFlag = false;
		}
    }

	public void MessagePopup(string message)
    {
		if (container != null) {
			container.missionText.GetComponent<MissionTextAnimScript> ().Reset ();
			container.missionText.GetComponent<TextMeshProUGUI> ().text = message;
			container.missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
		}
    }

	public void SetActionBarSlider(float val) {
		container.actionBarSlider.value = val;
	}

	public void ToggleSpectatorMessage(bool b) {
		container.spectatorText.text = "You've been eliminated.\nYou can respawn if an ally clears the sector.";
		container.spectatorText.enabled = b;
	}

    private void UpdateMissionTimeText() {
        float totalSecs = GameControllerScript.missionTime;
        int mins = (int)(totalSecs / 60f);
        int remainingSecs = Mathf.RoundToInt((totalSecs - (mins * 60f)));
        container.missionTimeText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));

		// Set remaining time
		if (gameController.currentMap == 1) {
			mins = (int)((GameControllerScript.MAX_MISSION_TIME - totalSecs) / 60f);
			remainingSecs = Mathf.RoundToInt(((GameControllerScript.MAX_MISSION_TIME - totalSecs) - (mins * 60f)));
			container.missionTimeRemainingText.text = (remainingSecs < 10 ? (mins + ":0" + remainingSecs) : (mins + ":" + remainingSecs));
		} else if (gameController.currentMap == 2) {
			if (gameController.objectives.missionTimer1 > 0f) {
				int remSecs = (int)gameController.objectives.missionTimer1 % 60;
				container.missionTimeRemainingText.text = ((int)gameController.objectives.missionTimer1 / 60) + ":" + (remSecs < 10 ? "0" : "") + remSecs;
			} else if (gameController.objectives.missionTimer2 > 0f) {
				int remSecs = (int)gameController.objectives.missionTimer2 % 60;
				container.missionTimeRemainingText.text = ((int)gameController.objectives.missionTimer2 / 60) + ":" + (remSecs < 10 ? "0" : "") + remSecs;
			} else if (gameController.objectives.missionTimer3 > 0f) {
				int remSecs = (int)gameController.objectives.missionTimer3 % 60;
				container.missionTimeRemainingText.text = ((int)gameController.objectives.missionTimer3 / 60) + ":" + (remSecs < 10 ? "0" : "") + remSecs;
			}
		}
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
			container.gameOverBanner.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			container.gameOverBanner.SetActive (true);
		} else {
			container.gameOverBanner.SetActive (false);
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		if (!GetComponent<PhotonView>().IsMine) return;
		RemovePlayerMarker(otherPlayer.ActorNumber);
		gameController.TogglePlayerSpeaking(false, otherPlayer.ActorNumber, otherPlayer.NickName);
	}

	public void RemovePlayerMarker(int actorNumber) {
		if (!playerMarkers.ContainsKey(actorNumber)) return;
		Destroy (playerMarkers [actorNumber]);
		playerMarkers.Remove (actorNumber);
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

	void UpdateCarryingText() {
		if (container.itemCarryingText.text != null && container.itemCarryingText.text != "") {
			container.itemCarryingGroup.alpha = 1f;
		} else {
			container.itemCarryingGroup.alpha = 0f;
		}
	}

	public void SetCarryingText(string t) {
		container.itemCarryingText.text = t;
	}

	void UpdateCarryingKeyText() {
		container.itemCarryingKeyText.text = "CARRYING (PRESS [" + PlayerPreferences.playerPreferences.keyMappings["Drop"].key.ToString() + "] TO DROP/THROW):";
	}

	public void UpdateKeyHints()
	{
		UpdateCarryingKeyText();
	}

	public void SetDetectionMeter(float detection) {
		container.detectionMeter.fillAmount = detection;
	}

	void ClearEnemyMarkers() {
		foreach(KeyValuePair<int, AlertMarker> entry in enemyMarkers)
		{
			Destroy(entry.Value.markerRef);
		}
		enemyMarkers.Clear();
		enemyMarkersCleared = true;
	}

    public void ToggleVersusHUD(bool b)
    {
        container.blueScore.SetActive(b);
        container.redScore.SetActive(b);
		if (gameController.teamMap == "R") {
			container.redTeamHighlight.SetActive(true);
			container.redTeamUnderline.SetActive(true);
		} else if (gameController.teamMap == "B") {
			container.blueTeamHighlight.SetActive(true);
			container.blueTeamUnderline.SetActive(true);
		}
    }

    public void UpdateRedTeamScore(int score)
    {
        container.redScoreTxt.text = score + "%";
		if (!gameController.enemyTeamNearingVictoryTrigger && score >= 80) {
			if (gameController.teamMap != "R") {
				gameController.SetEnemyTeamNearingVictoryMessage();
			}
 		}
    }

    public void UpdateBlueTeamScore(int score)
    {
        container.blueScoreTxt.text = score + "%";
		if (!gameController.enemyTeamNearingVictoryTrigger && score >= 80) {
			if (gameController.teamMap != "B") {
				gameController.SetEnemyTeamNearingVictoryMessage();
			}
		}
    }

	public void SetSightCrosshairForSight(string sightName) {
		if (GetComponent<PhotonView>().IsMine) {
			container.sightCrosshair.texture = (Texture)Resources.Load(InventoryScript.itemData.modCatalog[sightName].crosshairPath);
		}
	}

	public void EquipSightCrosshair(bool on) {
		if (GetComponent<PhotonView>().IsMine) {
			if (on) {
				container.sightCrosshair.gameObject.SetActive(true);
			} else {
				container.sightCrosshair.gameObject.SetActive(false);
			}
		}
	}

	public void ToggleSightCrosshair(bool b) {
		container.sightCrosshair.enabled = b;
	}

	public void ToggleDeployInvalidText(bool b) {
		container.deployInvalidText.enabled = b;
	}

	public void ToggleHintText(string message) {
		if (message != null) {
			container.hintText.text = message;
			container.hintText.enabled = true;
		} else {
			container.hintText.enabled = false;
			container.hintText.text = "";
		}
	}

	public void ToggleChatText(bool b) {
		container.inGameMessenger.inputText.enabled = b;
	}

	public bool PauseIsActive() {
		return container.pauseMenuGUI.pauseActive;
	}

	public void UpdateHealth() {
		container.healthBar.value = playerActionScript.health / 100f;
		container.healthPercentTxt.text = playerActionScript.health + "%";
	}

	void InitHealth() {
		container.healthBar.value = 1f;
		container.healthPercentTxt.text = "100%";
	}

	void UpdateVoteUI() {
		if (gameController.voteInProgress) {
			if (!container.votePanel.gameObject.activeInHierarchy) {
				if (gameController.currentVoteAction == GameControllerScript.VoteActions.KickPlayer) {
					container.votePanel.text = "VOTE CALLED: KICK PLAYER [" + gameController.playerBeingKickedName + "]?";
					if (gameController.playerBeingKicked?.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
						container.voteOptions.GetComponent<TextMeshProUGUI>().text = "A VOTE IS BEING HELD TO KICK YOU.";
						container.voteResults.SetActive(true);
					} else {
						container.voteOptions.GetComponent<TextMeshProUGUI>().text = "[F1] YES            [F2] NO";
						container.voteResults.SetActive(false);
					}
				}
				container.votePanel.gameObject.SetActive(true);
				container.voteOptions.SetActive(true);
				container.voteTime.gameObject.SetActive(true);
				container.finalVoteResults.gameObject.SetActive(false);
			}
			if (gameController.iHaveVoted) {
				if (!container.voteResults.activeInHierarchy) {
					container.voteResults.SetActive(true);
					container.voteOptions.SetActive(false);
				}
			}
			container.voteTime.text = ""+(int)gameController.voteTimer;
			container.yesVoteCount.text = ""+gameController.yesVotes;
			container.noVoteCount.text = ""+gameController.noVotes;
		} else {
			if (container.votePanel.gameObject.activeInHierarchy && !container.finalVoteResults.gameObject.activeInHierarchy) {
				StartCoroutine("DisplayFinalVoteResults");
			}
		}
	}

	IEnumerator DisplayFinalVoteResults()
	{
		if (gameController.VoteHasSucceeded()) {
			if (gameController.currentVoteAction == GameControllerScript.VoteActions.KickPlayer) {
				container.finalVoteResults.text = "THE VOTE TO KICK [" + gameController.playerBeingKickedName + "] HAS PASSED.";
			}
		} else {
			if (gameController.currentVoteAction == GameControllerScript.VoteActions.KickPlayer) {
				container.finalVoteResults.text = "THE VOTE TO KICK [" + gameController.playerBeingKickedName + "] HAS BEEN VETOED.";
			}
		}
		container.finalVoteResults.gameObject.SetActive(true);
		container.voteOptions.SetActive(false);
		container.voteResults.SetActive(false);
		container.voteTime.gameObject.SetActive(false);
		yield return new WaitForSeconds(6f);
		container.votePanel.gameObject.SetActive(false);
		container.finalVoteResults.gameObject.SetActive(false);
		container.voteOptions.SetActive(true);
		container.voteResults.SetActive(false);
	}

	public void AddPlayerSpeakingIndicator(int actorNo, string playerName)
	{
		foreach (VoiceChatEntryScript v in container.voiceChatEntries)
		{
			if (!v.gameObject.activeInHierarchy) {
				v.SetPlayerNameEntry(actorNo, playerName);
				v.gameObject.SetActive(true);
				break;
			}
		}
	}

	public void RemovePlayerSpeakingIndicator(int actorNo)
	{
		foreach (VoiceChatEntryScript v in container.voiceChatEntries)
		{
			if (v.gameObject.activeInHierarchy && actorNo == v.actorNo) {
				v.gameObject.SetActive(false);
				break;
			}
		}
	}

	void HandleVoiceChat()
	{
		if (PlayerPreferences.playerPreferences.KeyWasPressed("VoiceChat", true)) {
			if (!voiceChatActive && CanVoiceChat()) {
				VivoxVoiceManager.Instance.AudioInputDevices.Muted = false;
				MarkMyselfAsSpeaking();
				voiceChatActive = true;
			}
		} else {
			if (voiceChatActive) {
				VivoxVoiceManager.Instance.AudioInputDevices.Muted = true;
				UnmarkMyselfAsSpeaking();
				voiceChatActive = false;
			}
		}
	}

	void MarkMyselfAsSpeaking()
	{
		gameController.ToggleMyselfSpeaking(true);
	}

	void UnmarkMyselfAsSpeaking()
	{
		gameController.ToggleMyselfSpeaking(false);
	}

	bool CanVoiceChat()
	{
		if (container.pauseMenuManager.pauseActive) return false;
		if (PlayerPreferences.playerPreferences.preferenceData.audioInputName == "None") return false;
		return true;
	}

	bool CanVoiceCommand()
	{
		if (container.pauseMenuManager.pauseActive) return false;
		if (!container.voiceCommandsPanel.activeInHierarchy) return false;
		if (playerActionScript.health <= 0) return false;
		if (gameController.gameOver) return false;
		return true;
	}

	public AudioClip GetVoiceCommandAudio(char type, int i, char gender)
	{
		if (type == 'r') {
			return container.reportCommands[i].GetCommandAudio(gender);
		} else if (type == 't') {
			return container.tacticalCommands[i].GetCommandAudio(gender);
		}
		return container.supportCommands[i].GetCommandAudio(gender);
	}

	string GetVoiceCommandText(char type, int i)
	{
		if (type == 'r') {
			return container.reportCommands[i].commandString;
		} else if (type == 't') {
			return container.tacticalCommands[i].commandString;
		}
		return container.supportCommands[i].commandString;
	}

	bool CanUseVoiceCommands()
	{
		if (playerActionScript.health <= 0 || container.inGameMessenger.inputText.enabled) return false;
		return true;
	}

	void HandleVoiceCommands()
	{
		if (commandDelay > 0f) {
			commandDelay -= Time.deltaTime;
			return;
		}
		if (!CanUseVoiceCommands()) {
			container.voiceCommandsPanel.SetActive(false);
			return;
		}
		// Open/closing menu
		if (PlayerPreferences.playerPreferences.KeyWasPressed("VCReport")) {
			if (!container.voiceCommandsPanel.activeInHierarchy) {
				container.voiceCommandsReport.SetActive(true);
				container.voiceCommandsSocial.SetActive(false);
				container.voiceCommandsTactical.SetActive(false);
				container.voiceCommandsPanel.SetActive(true);
			} else {
				if (container.voiceCommandsReport.activeInHierarchy) {
					container.voiceCommandsPanel.SetActive(false);
				} else {
					container.voiceCommandsReport.SetActive(true);
					container.voiceCommandsSocial.SetActive(false);
					container.voiceCommandsTactical.SetActive(false);
				}
			}
		} else if (PlayerPreferences.playerPreferences.KeyWasPressed("VCTactical")) {
			if (!container.voiceCommandsPanel.activeInHierarchy) {
				container.voiceCommandsReport.SetActive(false);
				container.voiceCommandsSocial.SetActive(false);
				container.voiceCommandsTactical.SetActive(true);
				container.voiceCommandsPanel.SetActive(true);
			} else {
				if (container.voiceCommandsTactical.activeInHierarchy) {
					container.voiceCommandsPanel.SetActive(false);
				} else {
					container.voiceCommandsReport.SetActive(false);
					container.voiceCommandsSocial.SetActive(false);
					container.voiceCommandsTactical.SetActive(true);
				}
			}
		} else if (PlayerPreferences.playerPreferences.KeyWasPressed("VCSocial")) {
			if (!container.voiceCommandsPanel.activeInHierarchy) {
				container.voiceCommandsReport.SetActive(false);
				container.voiceCommandsSocial.SetActive(true);
				container.voiceCommandsTactical.SetActive(false);
				container.voiceCommandsPanel.SetActive(true);
			} else {
				if (container.voiceCommandsSocial.activeInHierarchy) {
					container.voiceCommandsPanel.SetActive(false);
				} else {
					container.voiceCommandsReport.SetActive(false);
					container.voiceCommandsSocial.SetActive(true);
					container.voiceCommandsTactical.SetActive(false);
				}
			}
		}

		// Giving commands
		if (container.voiceCommandsPanel.activeInHierarchy) {
			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 0);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 1);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 2);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha4)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 3);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha5)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 4);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha6)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 5);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha7)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 6);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha8)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 7);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			} else if (Input.GetKeyDown(KeyCode.Alpha9)) {
				if (CanVoiceCommand()) {
					char type = GetCurrentVoiceCommandType();
					TriggerVoiceCommand(type, 8);
				}
				container.voiceCommandsPanel.SetActive(false);
				commandDelay = COMMAND_DELAY;
			}
		}
	}

	char GetCurrentVoiceCommandType()
	{
		if (container.voiceCommandsReport.activeInHierarchy) {
			return 'r';
		} else if (container.voiceCommandsSocial.activeInHierarchy) {
			return 's';
		} else if (container.voiceCommandsTactical.activeInHierarchy) {
			return 't';
		}
		return '0';
	}

	void TriggerVoiceCommand(char type, int i) {
		playerActionScript.SendVoiceCommand(type, i);
	}

	public void PlayVoiceCommand(string playerName, char type, int i)
	{
		string voiceCommandMessage = GetVoiceCommandText(type, i);
		inGameMessenger.SendVoiceCommandMessage(playerName, voiceCommandMessage);
	}

	public void QueuePlayerJoining(string playerName)
	{
		GameObject p = GameObject.Instantiate(container.acceptPlayerTemplate, container.acceptPlayerSlots);
		p.GetComponentInChildren<Text>().text = playerName + " IS JOINING... ([F5] ACCEPT | [F6] DECLINE)";
	}

	public void DequeuePlayerJoining()
	{
		RectTransform[] j = container.acceptPlayerSlots.GetComponentsInChildren<RectTransform>();
		if (j.Length > 1) {
			GameObject.Destroy(j[1].gameObject);
		}
	}

}

public class AlertMarker {
	public GameObject markerRef;
	// Current alert symbol being displayed. Used to prevent script from setting the texture over and over again
	public AlertStatus alertStatus;
	public AlertMarker(GameObject markerRef, AlertStatus alertStatus) {
		this.markerRef = markerRef;
		this.alertStatus = alertStatus;
	}
}
