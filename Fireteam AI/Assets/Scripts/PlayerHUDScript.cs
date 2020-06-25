using System;
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
	// HUD object reference
	public HUDContainer container;
	private PauseMenuScript pauseMenuScript;

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

	void Awake() {
		container = GameObject.FindWithTag ("HUD").GetComponent<HUDContainer> ();
	}

    // Use this for initialization
    void Start () {
		// container = GameObject.FindWithTag ("HUD").GetComponent<HUDContainer> ();
        if (!GetComponent<PhotonView>().IsMine) {
			myHudMarkerCam1.targetTexture = null;
			myHudMarkerCam2.targetTexture = null;
			myHudMarkerCam1.enabled = false;
			myHudMarkerCam2.enabled = false;
			// myHudMarkerCam1.gameObject.SetActive(false);
			// myHudMarkerCam2.gameObject.SetActive(false);
            this.enabled = false;
			return;
        }
		// Find/load HUD components
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();
		missionWaypoints = new ArrayList ();

		// container = GameObject.FindWithTag ("HUD").GetComponent<HUDContainer> ();
		container.hitFlare.GetComponent<RawImage> ().enabled = false;
		container.hitDir.GetComponent<RawImage> ().enabled = false;
		container.hitMarker.GetComponent<RawImage> ().enabled = false;
		pauseMenuScript = container.pauseMenuGUI.GetComponent<PauseMenuScript>();

		foreach (int actorId in gameController.enemyList.Keys) {
			GameObject marker = GameObject.Instantiate(container.enemyAlerted);
			marker.GetComponent<RectTransform>().SetParent(container.enemyMarkers.transform);
			marker.SetActive(false);
			AlertMarker m = new AlertMarker(marker, AlertStatus.Neutral);
			enemyMarkers.Add(actorId, m);
		}

		container.pauseMenuGUI.SetActive (false);
		ToggleActionBar(false, null);
		container.actionBarText.enabled = false;
		container.hintText.enabled = false;
		container.scoreboard.GetComponent<Canvas> ().enabled = false;
		container.spectatorText.enabled = false;
		ToggleDetectionHUD(false);

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
	}

	public void StartMatchCameraFade() {
		container.screenColor.enabled = true;
		container.screenColor.color = new Color (0f, 0f, 0f, 1f);
		roundStartFadeIn = true;
	}

	void LoadHUDForMission() {
		container.screenColor.color = new Color (0f, 0f, 0f, 1f);
		container.objectivesText.text = gameController.objectives.GetObjectivesString();
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
			container.vipHealthBar.gameObject.SetActive(true);
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
		InitialComBoxRoutineForMission(gameController.currentMap);
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
		
		HandleVipHealthBar();

		ToggleScoreboard (PlayerPreferences.playerPreferences.KeyWasPressed("Scoreboard", true));

		UpdateHitmarker ();

		// Update UI
		//container.weaponLabelTxt.text = playerActionScript.currWep;
		container.weaponLabelTxt.text = wepScript.equippedWep;
		container.ammoTxt.text = "" + wepActionScript.currentAmmo + '/' + wepActionScript.totalAmmoLeft;
		
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
		UpdateObjectives ();
		FlashbangUpdate();
		UpdateDetectedText();
		UpdateCarryingText();
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

	void HandleVipHealthBar() {
		if (gameController.vipRef != null) {
			container.vipHealthBar.gameObject.SetActive(true);
			container.vipHealthBar.value = (float)gameController.vipRef.GetComponent<NpcScript>().health / 100f;
		} else {
			container.vipHealthBar.gameObject.SetActive(false);
		}
	}

	void HandleGameOverPopupsForCampaign() {
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
	}

	void HandleGameOverPopupsForVersus() {
		if (gameController.gameOver) {
			if (gameController.exitLevelLoaded) {
				ToggleGameOverPopup (false);
				ToggleGameOverBanner (true);
			} else if ((gameController.teamMap == "R" && gameController.redTeamPlayerCount == gameController.deadCount) || (gameController.teamMap == "B" && gameController.blueTeamPlayerCount == gameController.deadCount)) {
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
			|| container.scoreboard.GetComponent<Canvas>().enabled) {
				return false;
			}
		return true;
	}

	void UpdateCursorStatus() {
		if (PlayerPreferences.playerPreferences.KeyWasPressed("Pause") && CanPause()) {
			Pause();
		}

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
		if (b && container.healthText.enabled) {
			return;
		}
		if (!b && !container.healthText.enabled) {
			return;
		}
        container.healthText.enabled = b;
		container.staminaBar.gameObject.SetActive (b);
        container.weaponLabelTxt.enabled = b;
        container.ammoTxt.enabled = b;
		container.hudMap.enabled = b;
		container.hudMap2.enabled = b;
		container.hintText.enabled = false;
		if (!b) {
			container.itemCarryingText.text = null;
			ToggleCrosshair(false);
		}
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
            pauseMenuScript.HandleEscPress();
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
			ComBoxPopup(7f, "Democko", "The local Cicada cannibal gang has planted gas bombs to turn the townspeople into minced meat. Let's take care of 'em.", "democko");
		} else if (mission == 2) {
			ComBoxPopup(4f, "Democko", "Alpha team! What the hell just happened?! Give me a sitrep!", "democko");
			ComBoxPopup(9f, "Red Ruby", "Tail rotor got shot out with an RPG!", "redruby");
			ComBoxPopup(11f, "Democko", "Damn it! Give me a status report!", "democko");
			ComBoxPopup(14f, "Red Ruby", "I’m alive; leg is fractured! I’m stuck in the pilot seat! The rest of the team seems to be okay! This chopper’s combusting pretty fast!", "redruby");
			ComBoxPopup(20f, "Democko", "Okay, just stay calm! Alpha team, get her out of there to safety! I’m dispatching another chopper to get you guys out of there ASAP!", "democko");
			ComBoxPopup(29f, "Red Ruby", "Double time it, please! We’re sitting ducks out here!", "redruby");
			ComBoxPopup(35f, "Democko", "Roger that, it’s on the way! Just sit tight!", "democko");
			ComBoxPopup(40f, "Democko", "You guys won’t get far on foot. I recommend you set up a perimeter in the nearby buildings and defend yourselves until the chopper arrives. ETA is approximately 15 minutes!", "democko");
		}
	}

	IEnumerator ShowComBox(float t, string speaker, string s, string picPath) {
		yield return new WaitForSeconds (t);
		container.comBox.SetActive (true);
		container.comBoxText.GetComponent<ComboxTextEffect> ().SetText (s, speaker, picPath);
	}

	public void ComBoxPopup(float t, string speaker, string s, string picPath) {
		StartCoroutine (ShowComBox(t, speaker, s, picPath));
	}

    public void ToggleActionBar(bool enable, string actionText)
    {
        int c = container.actionBarImgs.Length;
        if (!enable)
        {
			// Preemptive check
			if (!container.actionBarImgs[0].enabled) {
				return;
			}
            // Disable all actionbar components
			container.actionBarText.enabled = false;
            for (int i = 0; i < c; i++)
            {
                container.actionBarImgs[i].enabled = false;
            }
        }
        else
        {
			// Preemptive check
			if (container.actionBarImgs[0].enabled) {
				return;
			}
			container.actionBarText.text = actionText;
			container.actionBarText.enabled = true;
            for (int i = 0; i < c; i++)
            {
                container.actionBarImgs[i].enabled = true;
            }
        }
    }

    public void UpdateObjectives()
    {
		if (gameController.updateObjectivesFlag) {
			container.objectivesText.text = gameController.objectives.GetObjectivesString();
			gameController.updateObjectivesFlag = false;
		}
    }

	public void MessagePopup(string message)
    {
		if (container != null) {
			container.missionText.GetComponent<MissionTextAnimScript> ().Reset ();
			container.missionText.GetComponent<Text> ().text = message;
			container.missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
		}
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

	void UpdateCarryingText() {
		if (container.itemCarryingText.text != null && container.itemCarryingText.text != "") {
			container.itemCarryingPnl.SetActive(true);
		} else {
			container.itemCarryingPnl.SetActive(false);
		}
	}

	public void SetCarryingText(string t) {
		container.itemCarryingText.text = t;
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
		return container.pauseMenuGUI.activeInHierarchy;
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
