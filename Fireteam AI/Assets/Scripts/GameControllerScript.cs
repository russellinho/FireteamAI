using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using NpcActionState = NpcScript.ActionStates;

public class GameControllerScript : MonoBehaviourPunCallbacks {

    // Timer
    public static float missionTime;
    public static float MAX_MISSION_TIME = 1800f;

	// A number value to the maps/missions starting with 1. The number correlates with the time it was released, so the lower the number, the earlier it was released.
	// 1 = The Badlands: Act 1; 2 = The Badlands: Act 2
	public int currentMap;
    public string teamMap;

    // variable for last gunshot position
    public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	private float lastGunshotTimer = 0f;
    public float endGameTimer = 0f;
	private bool loadExitCalled;
	public static Dictionary<int, PlayerStat> playerList = new Dictionary<int, PlayerStat> ();
	public Dictionary<short, GameObject> coverSpots;
	public Dictionary<int, GameObject> enemyList = new Dictionary<int, GameObject> ();
	private Dictionary<int, GameObject> pickupList = new Dictionary<int, GameObject>();
	private Dictionary<int, GameObject> deployableList = new Dictionary<int, GameObject>();

    // Mission variables
	public AIControllerScript aIController;
	public enum SpawnMode {Paused, Fixed, Routine};
	public SpawnMode spawnMode;
	public Objectives objectives;
	public bool updateObjectivesFlag;
	public GameObject[] items;
	public bool gameOver;
    public bool exitLevelLoaded;
	private float exitLevelLoadedTimer;
	public short sectorsCleared;

	public GameObject exitPoint;
	public Transform spawnLocation;
	public Transform outOfBoundsPoint;

	public bool assaultMode;
    // Sync mission time to clients every 10 seconds
    private float syncMissionTimeTimer;

	public PhotonView pView;

    // Match state data
    public char matchType;
    private string myTeam;
    private string opposingTeam;
	public bool enemyTeamNearingVictoryTrigger;
	public string campaignAlertMessage;
	public string alertMessage;
	public GameObject vipRef;
	public GameObject checkpointRef;
	public GameObject escapeVehicleRef;
	private bool endGameWithWin;
	public bool assaultModeChangedIndicator;

	// Use this for initialization
	void Awake() {
		coverSpots = new Dictionary<short, GameObject>();
        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        opposingTeam = (myTeam == "red" ? "blue" : "red");
		DetermineObjectivesForMission(SceneManager.GetActiveScene().name);
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
		if (!PhotonNetwork.IsMasterClient && !isVersusHostForThisTeam()) {
			pView.RPC("RpcAskServerForDataGc", RpcTarget.All);
		}
	}

    void Start () {
        if (matchType == 'C') {
            PhotonNetwork.AutomaticallySyncScene = false;
        } else if (matchType == 'V') {
            PhotonNetwork.AutomaticallySyncScene = false;
        }
		Physics.IgnoreLayerCollision (9, 12);
		Physics.IgnoreLayerCollision (9, 15);
		Physics.IgnoreLayerCollision (14, 12);
		Physics.IgnoreLayerCollision (15, 12);
		Physics.IgnoreLayerCollision (14, 15);
		Physics.IgnoreLayerCollision (0, 19);
		Physics.IgnoreLayerCollision (1, 19);
		Physics.IgnoreLayerCollision (2, 19);
		Physics.IgnoreLayerCollision (3, 19);
		Physics.IgnoreLayerCollision (4, 19);
		Physics.IgnoreLayerCollision (5, 19);
		Physics.IgnoreLayerCollision (6, 19);
		Physics.IgnoreLayerCollision (7, 19);
		Physics.IgnoreLayerCollision (8, 19);
		Physics.IgnoreLayerCollision (10, 19);
		Physics.IgnoreLayerCollision (11, 19);
		Physics.IgnoreLayerCollision (12, 19);
		Physics.IgnoreLayerCollision (13, 19);
		Physics.IgnoreLayerCollision (14, 19);
		Physics.IgnoreLayerCollision (15, 19);
		Physics.IgnoreLayerCollision (16, 19);
		Physics.IgnoreLayerCollision (17, 19);
		Physics.IgnoreLayerCollision (18, 19);

		gameOver = false;
		objectives.escaperCount = 0;
		objectives.escapeAvailable = false;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        exitLevelLoaded = false;
		exitLevelLoadedTimer = 0f;

        missionTime = 0f;
		lastGunshotTimer = 10f;
		sectorsCleared = 0;
		loadExitCalled = false;
        syncMissionTimeTimer = 0f;

		lastGunshotHeardPos = Vector3.negativeInfinity;
		if (matchType == 'C') {
			StartCoroutine("GameOverCheckForCampaign");
		} else if (matchType == 'V') {
			StartCoroutine("GameOverCheckForVersus");
		}
	}

	public void UpdateObjectives() {
		objectives.UpdateObjectives(currentMap);
		if (matchType == 'V') {
			SetMyTeamScore((short)(objectives.GetMissionProgress() * 100f));
		}
		updateObjectivesFlag = true;
	}

	void DetermineObjectivesForMission(string sceneName) {
		objectives = new Objectives(this);
		objectives.LoadObjectives(currentMap);
	}

	// Update is called once per frame
	void Update () {
		if (!PhotonNetwork.InRoom) {
			return;
		}
		UpdateMissionTime();
		if (matchType == 'C') {
			UpdateMissionProgressForCampaign();
		} else if (matchType == 'V') {
			UpdateMissionProgressForVersus();
		}
		UpdateTimers();
		DecrementLastGunshotTimer();
	}

	void UpdateTimers() {
		if (PhotonNetwork.IsMasterClient) {
			ResetLastGunshotPos ();
			UpdateEndGameTimer();
		}
	}

	void UpdateMissionProgressForCampaign() {
		if (currentMap == 1) {
			if (objectives.itemsRemaining == 0) {
				objectives.escapeAvailable = true;
				if (PhotonNetwork.CurrentRoom.IsOpen) {
					LockRoom();
				}
			}
		} else if (currentMap == 2) {
			if (objectives.stepsLeftToCompletion == 1) {
				if (objectives.missionTimer3 > 0f) {
					objectives.escapeAvailable = true;
				} else {
					objectives.escapeAvailable = false;
				}
				if (PhotonNetwork.CurrentRoom.IsOpen) {
					LockRoom();
				}
			}
		}
	}

	void UpdateMissionProgressForVersus() {
		if (currentMap == 1) {
			if (objectives.itemsRemaining == 0) {
				objectives.escapeAvailable = true;
			}
		} else if (currentMap == 2) {
			if (objectives.stepsLeftToCompletion == 1) {
				if (objectives.missionTimer3 > 0f) {
					objectives.escapeAvailable = true;
				} else {
					objectives.escapeAvailable = false;
				}
			}
		}
	}

	IEnumerator GameOverCheckForCampaign() {
		yield return new WaitForSeconds(1.5f);
		int deadCount = GetDeadCount();
		if (currentMap == 1) {
			if (PhotonNetwork.IsMasterClient) {
				// Check if the mission is over or if all players eliminated or out of time
				if (deadCount == PhotonNetwork.CurrentRoom.Players.Count || CheckOutOfTime())
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndGame", RpcTarget.All, 9f, null, false);
					}
				}
				else if (objectives.itemsRemaining == 0)
				{
					if (!gameOver && CheckEscapeForCampaign(deadCount))
					{
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndGame", RpcTarget.All, 2f, null, true);
					}
				}
			}
		} else if (currentMap == 2) {
			if (PhotonNetwork.IsMasterClient) {
				// Check if the mission is over or if all players eliminated or out of time
				if (deadCount == PhotonNetwork.CurrentRoom.Players.Count)
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndGame", RpcTarget.All, 9f, null, false);
					}
				} else if (vipRef.GetComponent<NpcScript>().actionState == NpcActionState.Dead) {
					if (!gameOver)
					{
						pView.RPC("RpcEndGame", RpcTarget.All, 4f, "The VIP has been killed!", false);
					}
				} else if (objectives.stepsLeftToCompletion == 1 && objectives.escapeAvailable)
				{
					if (!gameOver && CheckEscapeForCampaign(deadCount))
					{
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndGame", RpcTarget.All, 2f, null, true);
					}
				}
			}
		}
		StartCoroutine("GameOverCheckForCampaign");
	}

	IEnumerator GameOverCheckForVersus() {
		yield return new WaitForSeconds(1.5f);
		int deadCount = GetDeadCount();
		if (currentMap == 1) {
			if (isVersusHostForThisTeam()) {
				int redTeamCount = GetRedTeamCount();
				int blueTeamCount = GetBlueTeamCount();
				int playerCount = (teamMap == "R" ? redTeamCount : blueTeamCount);
				if (deadCount == playerCount)
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, (teamMap == "R" ? "B" : "R"), "The enemy team has been eliminated!", "Your team has been eliminated!");
					}
				} else if (CheckOutOfTime()) {
					if (!gameOver) {
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, "T", null, null);
					}
				} else if (objectives.itemsRemaining == 0)
				{
					if (!gameOver && CheckEscapeForVersus(deadCount, redTeamCount, blueTeamCount))
					{
						// Set completion to 100%
						SetMyTeamScore(100);
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 2f, teamMap, null, "The enemy team has reached victory!");
					}
				}
				// Check to see if either team has forfeited
				DetermineEnemyTeamForfeited(redTeamCount, blueTeamCount);
			}
		} else if (currentMap == 2) {
			if (isVersusHostForThisTeam()) {
				int redTeamCount = GetRedTeamCount();
				int blueTeamCount = GetBlueTeamCount();
				int playerCount = (teamMap == "R" ? redTeamCount : blueTeamCount);
				if (deadCount == playerCount)
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, (teamMap == "R" ? "B" : "R"), "The enemy team has been eliminated!", "Your team has been eliminated!");
					}
				} else if (vipRef.GetComponent<NpcScript>().actionState == NpcActionState.Dead) {
					if (!gameOver) {
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 4f, (teamMap == "R" ? "B" : "R"), "The enemy team's VIP has been killed!", "The VIP has been killed!");
					} 
				} else if (CheckOutOfTime()) {
					if (!gameOver) {
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, "T", null, null);
					}
				} else if (objectives.stepsLeftToCompletion == 1 && objectives.escapeAvailable)
				{
					if (!gameOver && CheckEscapeForVersus(deadCount, redTeamCount, blueTeamCount))
					{
						// Set completion to 100%
						SetMyTeamScore(100);
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 2f, teamMap, null, "The enemy team has reached victory!");
					}
				}
				// Check to see if either team has forfeited
				DetermineEnemyTeamForfeited(redTeamCount, blueTeamCount);
			}
		}
		StartCoroutine("GameOverCheckForVersus");
	}

	[PunRPC]
	void RpcEndGame(float f, string eventMessage, bool win) {
		endGameTimer = f;
		gameOver = true;
		endGameWithWin = win;
		if (eventMessage != null) {
			alertMessage = eventMessage;
		}
		int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;
		pView.RPC("RpcSetMyExpAndGpGained", RpcTarget.All, myActorId, (int)CalculateExpGained(playerList[myActorId].kills, playerList[myActorId].deaths), (int)CalculateGpGained(playerList[myActorId].kills, playerList[myActorId].deaths));
	}

    [PunRPC]
    void RpcEndVersusGame(float f, string winner, string winnerEventMessage, string loserEventMessage)
    {
		if (gameOver) return;

        endGameTimer = f;
        gameOver = true;
        
		Hashtable h = new Hashtable();

		if (winner == "R") {
			h.Add("redStatus", "win");
			h.Add("blueStatus", "lose");
			if (teamMap == "R") {
				alertMessage = winnerEventMessage;
			} else if (teamMap == "B") {
				alertMessage = loserEventMessage;
			}
		} else if (winner == "B") {
			h.Add("redStatus", "lose");
			h.Add("blueStatus", "win");
			if (teamMap == "B") {
				alertMessage = winnerEventMessage;
			} else if (teamMap == "R") {
				alertMessage = loserEventMessage;
			}
		} else if (winner == "T") {
			h.Add("redStatus", "lose");
			h.Add("blueStatus", "lose");
			alertMessage = "Time up!";
		}

		PhotonNetwork.CurrentRoom.SetCustomProperties(h);

		int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;
		pView.RPC("RpcSetMyExpAndGpGained", RpcTarget.All, myActorId, (int)CalculateExpGained(playerList[myActorId].kills, playerList[myActorId].deaths, (winner == teamMap)), (int)CalculateGpGained(playerList[myActorId].kills, playerList[myActorId].deaths, (winner == teamMap)));
    }

	public void UpdateAssaultMode() {
		pView.RPC ("RpcUpdateAssaultMode", RpcTarget.All, true, teamMap);
	}

    [PunRPC]
	public void RpcUpdateAssaultMode(bool assaultInProgress, string team) {
        if (team != teamMap) return;
		// StartCoroutine (UpdateAssaultModeTimer(5f, assaultInProgress));
		assaultMode = assaultInProgress;
	}

	IEnumerator UpdateAssaultModeTimer(float secs, bool assaultInProgress) {
		yield return new WaitForSeconds (secs);
		assaultMode = assaultInProgress;
	}

	bool CheckEscapeForCampaign(int deadCount) {
		if (currentMap == 1) {
			if (deadCount + objectives.escaperCount == PhotonNetwork.CurrentRoom.PlayerCount) {
				return true;
			}
		} else if (currentMap == 2) {
			if (deadCount + objectives.escaperCount == PhotonNetwork.CurrentRoom.PlayerCount && Vector3.Distance(vipRef.transform.position, exitPoint.transform.position) <= 6f) {
				return true;
			}
		}
		return false;
	}

	bool CheckEscapeForVersus(int deadCount, int redCount, int blueCount) {
		if (currentMap == 1) {
			if (teamMap == "R") {
				if (deadCount + objectives.escaperCount == redCount) {
					return true;
				}
			} else if (teamMap == "B") {
				if (deadCount + objectives.escaperCount == blueCount) {
					return true;
				}
			}
		} else if (currentMap == 2) {
			if (teamMap == "R") {
				if (deadCount + objectives.escaperCount == redCount && Vector3.Distance(vipRef.transform.position, exitPoint.transform.position) <= 6f) {
					return true;
				}
			} else if (teamMap == "B") {
				if (deadCount + objectives.escaperCount == blueCount && Vector3.Distance(vipRef.transform.position, exitPoint.transform.position) <= 6f) {
					return true;
				}
			}
		}
		return false;
	}

    void DetermineEnemyTeamForfeited(int redTeamCount, int blueTeamCount)
    {
        if (gameOver) return;

        // Check if the other team has forfeited - can be determine by any players left on the opposing team
		if ((teamMap == "R" && blueTeamCount == 0) || (teamMap == "B" && redTeamCount == 0)) {
			// Couldn't find another player on the other team. This means that they forfeit
			pView.RPC("RpcEndVersusGame", RpcTarget.All, 3f, teamMap, "The enemy team has forfeited!", null);
		}
    }

	public void SetLastGunshotHeardPos(bool clear, Vector3 pos) {
		if ((clear && Vector3.Equals(lastGunshotHeardPos, Vector3.negativeInfinity)) || lastGunshotTimer > 0f) return;
		if (spawnMode != SpawnMode.Paused) {
			pView.RPC ("RpcSetLastGunshotHeardPos", RpcTarget.All, clear, pos.x, pos.y, pos.z, teamMap);
		}
	}

	[PunRPC]
	void RpcSetLastGunshotHeardPos(bool b, float x, float y, float z, string team) {
        if (team != teamMap) return;
		if (b) {
			lastGunshotHeardPos = Vector3.negativeInfinity;
		} else {
			lastGunshotHeardPos = new Vector3 (x, y, z);
		}
		lastGunshotTimer = 10f;
	}

	void ResetLastGunshotPos() {
		if (lastGunshotTimer <= 0f) {
			SetLastGunshotHeardPos(true, Vector3.zero);
		}
	}

	void DecrementLastGunshotTimer() {
		if (lastGunshotTimer > 0f) {
			lastGunshotTimer -= Time.deltaTime;
		}
	}

	public void ConvertCounts(int escape) {
		pView.RPC ("RpcConvertCounts", RpcTarget.All, escape, teamMap);
	}

	[PunRPC]
	void RpcConvertCounts(int escape, string team) {
        if (team != teamMap) return;
		objectives.escaperCount += escape;
	}

	public void IncrementEscapeCount() {
		pView.RPC ("RpcIncrementEscapeCount", RpcTarget.All, teamMap);
	}

	[PunRPC]
	void RpcIncrementEscapeCount(string team) {
        if (team != teamMap) return;
        objectives.escaperCount++;
	}

    void UpdateMissionTime() {
		if (gameOver) return;
        missionTime += Time.deltaTime;

        // Query server for sync time if not master client every 30 seconds
		if (matchType == 'C') {
			if (!PhotonNetwork.IsMasterClient)
			{
				syncMissionTimeTimer -= Time.deltaTime;
				if (syncMissionTimeTimer <= 0f)
				{
					syncMissionTimeTimer = 30f;
					pView.RPC("RpcSendMissionTimeToClients", RpcTarget.MasterClient);
				}
			}
		} else if (matchType == 'V') {
			if (!isVersusHostForThisTeam()) {
				syncMissionTimeTimer -= Time.deltaTime;
				if (syncMissionTimeTimer <= 0f)
				{
					syncMissionTimeTimer = 30f;
					pView.RPC("RpcSendMissionTimeToClients", RpcTarget.MasterClient);
				}
			}
		}
    }

    [PunRPC]
    void RpcUpdateMissionTime(float mTime, float mTimer1, float mTimer2, float mTimer3, string team) {
        if (team != teamMap) return;
        missionTime = mTime;
		objectives.missionTimer1 = mTimer1;
		objectives.missionTimer2 = mTimer2;
		objectives.missionTimer3 = mTimer3;
    }

    [PunRPC]
    void RpcSendMissionTimeToClients()
    {
        pView.RPC("RpcUpdateMissionTime", RpcTarget.Others, missionTime, objectives.missionTimer1, objectives.missionTimer2, objectives.missionTimer3, teamMap);
    }

    // When someone leaves the game in the middle of an escape, reset the values to recount
    void ResetEscapeValues() {
		objectives.escaperCount = 0;
	}

    bool CheckOutOfTime() {
        if (missionTime >= MAX_MISSION_TIME) {
			return true;
        }
		return false;
    }



	/**public override void OnJoinedRoom()
	{
		GameObject[] playerPrefabs = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < playerPrefabs.Length; i++)
		{
			playerList.Add (playerPrefabs[i].GetComponent<PhotonView>().OwnerActorNr, playerPrefabs[i]);
		}

	}*/

	// public override void OnLeftRoom()
	// {
	// 	foreach (PlayerStat entry in playerList.Values)
	// 	{
	// 		Destroy(entry.objRef);
	// 	}

	// 	playerList.Clear();
	// }

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		if (!playerList.ContainsKey(otherPlayer.ActorNumber)) return;
		ResetEscapeValues ();
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			if (entry.objRef == null) continue;
			entry.objRef.GetComponent<PlayerActionScript> ().escapeValueSent = false;
		}

		if (GameControllerScript.playerList[otherPlayer.ActorNumber].objRef != null) {
			Destroy (GameControllerScript.playerList[otherPlayer.ActorNumber].objRef);
		}
		GameControllerScript.playerList.Remove (otherPlayer.ActorNumber);
	}

	public void OnPlayerLeftGame(int actorNo) {
		pView.RPC("RpcPlayerLeftGame", RpcTarget.All, actorNo);
	}

	[PunRPC]
	void RpcOnPlayerLeftGame(int actorNo) {
		PlayerData.playerdata.GetComponent<PlayerActionScript>().OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
		PlayerData.playerdata.GetComponent<PlayerHUDScript>().RemovePlayerMarker(PhotonNetwork.LocalPlayer.ActorNumber);
		if (!playerList.ContainsKey(actorNo)) return;
		ResetEscapeValues ();
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			if (entry.objRef == null) continue;
			entry.objRef.GetComponent<PlayerActionScript> ().escapeValueSent = false;
		}

		if (GameControllerScript.playerList[actorNo].objRef != null) {
			Destroy (GameControllerScript.playerList[actorNo].objRef);
		}
		GameControllerScript.playerList.Remove (actorNo);
	}

	public override void OnMasterClientSwitched(Player newMasterClient) {
		PhotonNetwork.CurrentRoom.IsVisible = false;
		PhotonNetwork.Disconnect();
		PhotonNetwork.LeaveRoom();
		PlayerData.playerdata.disconnectReason = "The host has left the game.";
		OnDisconnected(DisconnectCause.DisconnectByClientLogic);
	}

	/**public override void OnPlayerEnteredRoom(Player newPlayer) {
		GameObject[] playerPrefabs = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < playerPrefabs.Length; i++) {
			if (playerPrefabs [i].GetComponent<PhotonView> ().OwnerActorNr == newPlayer.ActorNumber) {
				playerList.Add (newPlayer.ActorNumber, playerPrefabs[i]);
				Debug.Log ("Added new player " + newPlayer.ActorNumber);
			}
		}
	}*/

	[PunRPC]
	void RpcUpdateEndGameTimer(float t, string team) {
        if (team != teamMap) return;
        endGameTimer = t;
	}

	[PunRPC]
	void RpcSetExitLevelLoaded(string team) {
        if (team != teamMap) return;
        exitLevelLoaded = true;
		exitLevelLoadedTimer = 4f;
	}

    void SetMyTeamScore(short score)
    {
		Hashtable h = new Hashtable();
		h.Add(myTeam + "Score", (int)score);
		PhotonNetwork.CurrentRoom.SetCustomProperties(h);
    }

    void UpdateEndGameTimer() {
        if (gameOver) {
            if (endGameTimer > 0f)
            {
                endGameTimer -= Time.deltaTime;
				pView.RPC ("RpcUpdateEndGameTimer", RpcTarget.Others, endGameTimer, teamMap);
            }

            if (endGameTimer <= 0f) {
				if (!exitLevelLoaded) {
					pView.RPC ("RpcSetExitLevelLoaded", RpcTarget.All, teamMap);
				} else {
					if (exitLevelLoadedTimer <= 0f && !loadExitCalled) {
                        loadExitCalled = true;
                        if (matchType == 'C')
                        {
                            SwitchToGameOverScene();
                        } else if (matchType == 'V')
                        {
                            string teamStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties[myTeam + "Status"];
                            if (teamStatus == "win")
                            {
                                SwitchToGameOverScene(true);
                            } else if (teamStatus == "lose")
                            {
                                SwitchToGameOverScene(false);
                            }
                        }
					} else {
						exitLevelLoadedTimer -= Time.deltaTime;
					}
				}
            }
        }
    }

	public override void OnDisconnected(DisconnectCause cause) {
		PlayerData.playerdata.disconnectedFromServer = true;
		if (string.IsNullOrEmpty(PlayerData.playerdata.disconnectReason) && !cause.ToString ().Equals (DisconnectCause.DisconnectByClientLogic)) {
			PlayerData.playerdata.disconnectReason = cause.ToString ();
		}
		SceneManager.LoadScene ("Title");
	}

	void SwitchToGameOverScene(bool win) {
		if (!win) {
			// PhotonNetwork.LoadLevel("GameOverFail");
			pView.RPC("RpcSwitchToGameOverScene", RpcTarget.All, "GameOverFail");
		} else {
			// PhotonNetwork.LoadLevel("GameOverSuccess");
			pView.RPC("RpcSwitchToGameOverScene", RpcTarget.All, "GameOverSuccess");
		}
	}

	void SwitchToGameOverScene() {
		if (endGameWithWin) {
			// PhotonNetwork.LoadLevel("GameOverSuccess");
			pView.RPC("RpcSwitchToGameOverScene", RpcTarget.All, "GameOverSuccess");
		} else {
			// PhotonNetwork.LoadLevel("GameOverFail");
			pView.RPC("RpcSwitchToGameOverScene", RpcTarget.All, "GameOverFail");
		}
	}

	[PunRPC]
	void RpcSwitchToGameOverScene(string s) {
		PhotonNetwork.LoadLevel(s);
	}

	public void AddCoverSpot(GameObject coverSpot) {
		CoverSpotScript cs = coverSpot.GetComponent<CoverSpotScript>();
		coverSpots.Add(cs.coverId, coverSpot);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer) {
		if (matchType == 'C') {
			HandlePlayerEnteredRoomCampaign();
		} else if (matchType == 'V') {
			HandlePlayerEnteredRoomVersus();
		}
	}

	void HandlePlayerEnteredRoomCampaign() {
		// Sync cover positions status if a player enters the room
		if (PhotonNetwork.IsMasterClient) {
			foreach(KeyValuePair<short, GameObject> entry in coverSpots) {
				SyncCoverSpot(entry.Key, entry.Value);
			}
		}
	}

	void HandlePlayerEnteredRoomVersus() {
		// Sync cover positions status if a player enters the room
		if (isVersusHostForThisTeam()) {
			foreach(KeyValuePair<short, GameObject> entry in coverSpots) {
				SyncCoverSpot(entry.Key, entry.Value);
			}
		}
	}

	void SyncCoverSpot(short key, GameObject value) {
		pView.RPC("RpcSyncCoverSpot", RpcTarget.Others, key, value.GetComponent<CoverSpotScript>().IsTaken(), teamMap);
	}

	[PunRPC]
	void RpcSyncCoverSpot(short key, bool value, string team) {
        if (team != teamMap) return;
        coverSpots[key].GetComponent<CoverSpotScript>().SetCoverSpot(value);
	}

	public void TakeCoverSpot(short id) {
		coverSpots[id].GetComponent<CoverSpotScript>().TakeCoverSpot();
	}

	public void LeaveCoverSpot(short id) {
		coverSpots[id].GetComponent<CoverSpotScript>().LeaveCoverSpot();
	}

	public bool isVersusHostForThisTeam() {
		if (teamMap == "R" && Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]) == PhotonNetwork.LocalPlayer.ActorNumber) {
			return true;
		} else if (teamMap == "B" && Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]) == PhotonNetwork.LocalPlayer.ActorNumber) {
            return true;
		}
		return false;
	}

	public void DropPickup(int pickupId, GameObject pickupRef) {
		pickupList.Add(pickupId, pickupRef);
	}

	public void DestroyPickup(int pickupId) {
		GameObject.Destroy(pickupList[pickupId]);
		pickupList.Remove(pickupId);
	}

	public GameObject GetPickup(int pickupId) {
		return pickupList[pickupId];
	}

	public void DeployDeployable(int deployableId, GameObject deployableRef) {
		deployableList.Add(deployableId, deployableRef);
	}

	public void DestroyDeployable(int deployableId) {
		GameObject.Destroy(deployableList[deployableId]);
		deployableList.Remove(deployableId);
	}

	public GameObject GetDeployable(int deployableId) {
		return deployableList[deployableId];
	}

	public void MarkAIReadyForRespawn(int pViewId, bool syncWithClientsAgain) {
		if (syncWithClientsAgain) {
			pView.RPC("RpcMarkAIReadyForRespawn", RpcTarget.All, teamMap, pViewId);
		} else {
			aIController.AddToRespawnQueue(pViewId);
		}
	}

	public void ClearAIRespawns() {
		pView.RPC("RpcClearAIRespawns", RpcTarget.All, teamMap);
	}

	[PunRPC]
	void RpcMarkAIReadyForRespawn(string team, int pViewId) {
		if (team != teamMap) return;
		aIController.AddToRespawnQueue(pViewId);
	}

	[PunRPC]
	void RpcClearAIRespawns(string team) {
		if (team != teamMap) return;
		aIController.ClearRespawnQueue();
	}

	public void SetEnemyTeamNearingVictoryMessage() {
		enemyTeamNearingVictoryTrigger = true;
		alertMessage = "Enemy team is nearing victory!";
	}

	uint CalculateExpGained(int kills, int deaths, bool versusWin = false) {
		if (matchType == 'V') {
			return CalculateExpGainedVersus(kills, deaths, versusWin);
		}
		return CalculateExpGainedCampaign(kills, deaths);
	}

	uint CalculateGpGained(int kills, int deaths, bool versusWin = false) {
		if (matchType == 'V') {
			return CalculateGpGainedVersus(kills, deaths, versusWin);
		}
		return CalculateGpGainedCampaign(kills, deaths);
	}

	uint CalculateGpGainedCampaign(int kills, int deaths) {
		float gradeMultiplier = ConvertGradeToMultiplier(GetCompletionGrade());
		return (uint)((gradeMultiplier) * (100f + (GetKillMultiplier(kills) * (float)kills)));
	}

	uint CalculateGpGainedVersus(int kills, int deaths, bool win) {
		float winMultiplier = (win ? 1.25f : 0.75f);
		float gradeMultiplier = ConvertGradeToMultiplier(GetCompletionGrade());
		return (uint)((winMultiplier) * (gradeMultiplier) * (100f + (GetKillMultiplier(kills) * (float)kills)));
	}

	uint CalculateExpGainedCampaign(int kills, int deaths) {
		float gradeMultiplier = ConvertGradeToMultiplier(GetCompletionGrade());
		return (uint)((gradeMultiplier) * (500f + (GetKillMultiplier(kills) * (float)kills) - (200f * (float)deaths)));
	}

	uint CalculateExpGainedVersus(int kills, int deaths, bool win) {
		float winMultiplier = (win ? 1.25f : 0.75f);
		float gradeMultiplier = ConvertGradeToMultiplier(GetCompletionGrade());
		return (uint)((winMultiplier) * (gradeMultiplier) * (500f + (GetKillMultiplier(kills) * (float)kills) - (200f * (float)deaths)));
	}

	char GetCompletionGrade() {
		if (matchType == 'V') {
			return GetCompletionGradeForMapVersus(SceneManager.GetActiveScene().name);
		}
		return GetCompletionGradeForMapCampaign(SceneManager.GetActiveScene().name);
	}

	// View the Leveling system documentation on drive to determine how these thresholds were determined
	char GetCompletionGradeForMapCampaign(string map) {
		if (map == "Badlands1") {
			if (GetDeadCount() == playerList.Count || CheckOutOfTime()) {
				return 'F';
			} else {
				if (missionTime <= 180f) {
					return 'A';
				} else if (missionTime <= 300f) {
					return 'B';
				} else if (missionTime <= 900f) {
					return 'C';
				}
			}
			return 'D';
		} else if (map == "Badlands2") {
			if (GetDeadCount() == playerList.Count || CheckOutOfTime()) {
				return 'F';
			} else {
				if (missionTime <= 900f) {
					return 'A';
				} else if (missionTime <= 1200f) {
					return 'B';
				} else if (missionTime <= 1800f) {
					return 'C';
				}
			}
			return 'D';
		}
		return 'C';
	}

	char GetCompletionGradeForMapVersus(string map) {
		int deadCount = GetDeadCount();
		if (map == "Badlands1_Red" || map == "Badlands1_Blue") {
			if (CheckOutOfTime() || (teamMap == "R" && deadCount == GetRedTeamCount()) || (teamMap == "B" && deadCount == GetBlueTeamCount())) {
				return 'F';
			} else {
				if (missionTime <= 180f) {
					return 'A';
				} else if (missionTime <= 300f) {
					return 'B';
				} else if (missionTime <= 900f) {
					return 'C';
				}
			}
			return 'D';
		} else if (map == "Badlands2_Red" || map == "Badlands2_Blue") {
			if (deadCount == playerList.Count || CheckOutOfTime()) {
				return 'F';
			} else {
				if (missionTime <= 900f) {
					return 'A';
				} else if (missionTime <= 1200f) {
					return 'B';
				} else if (missionTime <= 1800f) {
					return 'C';
				}
			}
			return 'D';
		}
		return 'C';
	}

	float ConvertGradeToMultiplier(char grade) {
		switch (grade) {
			case 'A':
				return 1.8f;
			case 'B':
				return 1.4f;
			case 'C':
				return 1f;
			case 'D':
				return 0.75f;
			case 'F':
				return 0.5f;
			default:
				return 1f;
		}
	}

	float GetKillMultiplier(int kills) {
		return Mathf.Clamp(51f - Mathf.Pow(1.032f, (kills / 10)), 25f, 51f);
	}

	[PunRPC]
	void RpcSetMyExpAndGpGained(int actorId, int expGained, int gpGained) {
		playerList[actorId].expGained = (uint)expGained;
		playerList[actorId].gpGained = (uint)gpGained;
	}

	[PunRPC]
	void RpcAskServerForDataGc() {
		if (PhotonNetwork.IsMasterClient || isVersusHostForThisTeam()) {
			pView.RPC("RpcSyncDataGc", RpcTarget.All, lastGunshotHeardPos.x, lastGunshotHeardPos.y, lastGunshotHeardPos.z, lastGunshotTimer, endGameTimer, loadExitCalled,
				spawnMode, gameOver, (int)sectorsCleared, assaultMode, enemyTeamNearingVictoryTrigger, endGameWithWin, assaultModeChangedIndicator, teamMap);
		}
	}

	[PunRPC]
	void RpcSyncDataGc(float lastGunshotHeardPosX, float lastGunshotHeardPosY, float lastGunshotHeardPosZ, float lastGunshotTimer, float endGameTimer,
		bool loadExitCalled, SpawnMode spawnMode, bool gameOver, int sectorsCleared, 
		bool assaultMode, bool enemyTeamNearingVictoryTrigger, bool endGameWithWin, bool assaultModeChangedIndicator, string team) {
		if (team != teamMap) return;
    	lastGunshotHeardPos = new Vector3(lastGunshotHeardPosX, lastGunshotHeardPosY, lastGunshotHeardPosZ);
		this.lastGunshotTimer = lastGunshotTimer;
		this.endGameTimer = endGameTimer;
		this.loadExitCalled = loadExitCalled;
		this.spawnMode = spawnMode;
		this.gameOver = gameOver;
		this.sectorsCleared = (short)sectorsCleared;
		this.assaultMode = assaultMode;
		this.enemyTeamNearingVictoryTrigger = enemyTeamNearingVictoryTrigger;
		this.assaultModeChangedIndicator = assaultModeChangedIndicator;
		this.endGameWithWin = endGameWithWin;
	}

	public void ClearDeadPlayersList() {
		if (PhotonNetwork.IsMasterClient) {
			Hashtable h = new Hashtable();
			h.Add("deads", null);
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}
	}

	public int GetDeadCount() {
		int total = 0;
		foreach(KeyValuePair<int, PlayerStat> entry in GameControllerScript.playerList)
		{
			GameObject p = entry.Value.objRef;
			if (p == null) continue;
			if (p.GetComponent<PlayerActionScript>().health <= 0) {
				total++;
			}
		}
		return total;
	}

	public int GetRedTeamCount() {
		int total = 0;
		foreach (Player p in PhotonNetwork.PlayerList) {
			if ((string)p.CustomProperties["team"] == "red") {
				total++;
			}
		}
		return total;
	}

	public int GetBlueTeamCount() {
		int total = 0;
		foreach (Player p in PhotonNetwork.PlayerList) {
			if ((string)p.CustomProperties["team"] == "blue") {
				total++;
			}
		}
		return total;
	}

	public void EndGameForAll() {
		pView.RPC("RpcEndGameForAll", RpcTarget.All);
	}

	[PunRPC]
	void RpcEndGameForAll() {
		PlayerData.playerdata.DestroyMyself();
	}

	public void LockRoom()
	{
		PhotonNetwork.CurrentRoom.IsOpen = false;
	}

	public void AddToTotalDeaths(int actorNo) {
		pView.RPC("RpcAddToTotalDeaths", RpcTarget.All, actorNo);
	}

	[PunRPC]
	void RpcAddToTotalDeaths(int actorNo) {
		GameControllerScript.playerList[actorNo].deaths++;
	}

	public void AddToTotalKills(int actorNo) {
		pView.RPC("RpcAddToTotalKills", RpcTarget.All, actorNo);
	}

	[PunRPC]
	void RpcAddToTotalKills(int actorNo) {
		GameControllerScript.playerList[actorNo].kills++;
	}

}

public class PlayerStat {
	public GameObject objRef;
	public int actorId;
	public string name;
	public char team;
	public int kills;
	public int deaths;
	public uint exp;
	public uint expGained;
	public uint gpGained;

	public PlayerStat(GameObject objRef, int actorId, string name, char team, uint exp) {
		this.objRef = objRef;
		this.actorId = actorId;
		this.name = name;
		this.team = team;
		this.exp = exp;
	}
}
