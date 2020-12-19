using System;
using System.Linq;
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
	private const float FORFEIT_CHECK_DELAY = 3f;
	private const float VOTE_TIME = 15f;
	private const float VOTE_DELAY = 300f;

	// A number value to the maps/missions starting with 1. The number correlates with the time it was released, so the lower the number, the earlier it was released.
	// 1 = The Badlands: Act 1; 2 = The Badlands: Act 2
	public int currentMap;
    public string teamMap;
	public Terrain[] terrainMetaData;

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
	private float forfeitDelayCheck;
	// Voting variables
	public enum VoteActions {KickPlayer};
	public VoteActions currentVoteAction;
	public Player playerBeingKicked;
	public string playerBeingKickedName;
	public bool iHaveVoted;
	public bool voteInProgress;
	public float voteTimer;
	public short yesVotes;
	public short noVotes;
	private float voteDelay;

	// Use this for initialization
	void Awake() {
		forfeitDelayCheck = 20f;
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
		UpdateVote();
		HandleVoteCast();
		if (Input.GetKeyDown(KeyCode.P)) {
			Debug.Log("R: " + Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]) + " | B: " + Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]));
		}
	}

	void UpdateTimers() {
		if (PhotonNetwork.IsMasterClient) {
			ResetLastGunshotPos ();
			UpdateEndGameTimer();
		}
		if (voteDelay > 0f) {
			voteDelay -= Time.deltaTime;
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
				// Debug.Log("r: " + redTeamCount + ", b: " + blueTeamCount);
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
		if (PhotonNetwork.LocalPlayer.IsMasterClient) {
			PhotonNetwork.CurrentRoom.IsOpen = false;
		}
		endGameTimer = f;
		gameOver = true;
		endGameWithWin = win;
		if (eventMessage != null) {
			alertMessage = eventMessage;
		}
		int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().container.voiceCommandsPanel.SetActive(false);
		pView.RPC("RpcSetMyExpAndGpGained", RpcTarget.All, myActorId, (int)CalculateExpGained(playerList[myActorId].kills, playerList[myActorId].deaths), (int)CalculateGpGained(playerList[myActorId].kills, playerList[myActorId].deaths));
	}

    [PunRPC]
    void RpcEndVersusGame(float f, string winner, string winnerEventMessage, string loserEventMessage)
    {
		if (gameOver) return;
		if (PhotonNetwork.LocalPlayer.IsMasterClient) {
			PhotonNetwork.CurrentRoom.IsOpen = false;
		}
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
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().container.voiceCommandsPanel.SetActive(false);
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
		if (forfeitDelayCheck > 0f) {
			forfeitDelayCheck -= 1.5f;
			return;
		}
		forfeitDelayCheck = FORFEIT_CHECK_DELAY;
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
    public void ResetEscapeValues() {
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

	public override void OnRoomPropertiesUpdate (Hashtable propertiesThatChanged) {
		if (propertiesThatChanged.ContainsKey("kickedPlayers")) {
			string newKickedPlayers = (string)propertiesThatChanged["kickedPlayers"];
			string[] newKickedPlayersList = newKickedPlayers.Split(',');
			if (newKickedPlayersList.Contains(PhotonNetwork.NickName)) {
				PhotonNetwork.CurrentRoom.IsVisible = false;
				PhotonNetwork.Disconnect();
				PhotonNetwork.LeaveRoom();
				PlayerData.playerdata.disconnectReason = "YOU'VE BEEN KICKED FROM THE GAME.";
				OnDisconnected(DisconnectCause.DisconnectByClientLogic);
			}
		}
	}

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		if (!GameControllerScript.playerList.ContainsKey(otherPlayer.ActorNumber)) return;
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().RemovePlayerMarker(otherPlayer.ActorNumber);
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
                        SwitchToGameOverScene();
					} else {
						exitLevelLoadedTimer -= Time.deltaTime;
					}
				}
            }
        }
    }

	public override void OnDisconnected(DisconnectCause cause) {
		PlayerData.playerdata.disconnectedFromServer = true;
		if (string.IsNullOrEmpty(PlayerData.playerdata.disconnectReason)) {
			PlayerData.playerdata.disconnectReason = cause.ToString ();
		}
		SceneManager.LoadScene ("Title");
	}

	void SwitchToGameOverScene() {
		if (PhotonNetwork.IsMasterClient) {
			pView.RPC("RpcSwitchToGameOverScene", RpcTarget.All);
		}
	}

	[PunRPC]
	void RpcSwitchToGameOverScene() {
		string matchType = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
		if (matchType == "versus") {
			string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
			string teamStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties[myTeam + "Status"];
			if (teamStatus == "win") {
				PhotonNetwork.LoadLevel("GameOverSuccess");
			} else {
				PhotonNetwork.LoadLevel("GameOverFail");
			}
		} else {
			if (endGameWithWin) {
				PhotonNetwork.LoadLevel("GameOverSuccess");
			} else {
				PhotonNetwork.LoadLevel("GameOverFail");
			}
		}
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
		if (PhotonNetwork.LocalPlayer.IsMasterClient) return true;
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

	string SerializeDeployables()
	{
		string s = null;
		bool first = true;
		foreach (KeyValuePair<int, GameObject> k in deployableList)
		{
			if (!first) {
				s += ",";
			} else {
				s = "";
			}
			DeployableScript d = k.Value.GetComponent<DeployableScript>();
			// Add id, uses remaining, position
			s += d.deployableId + '|' + d.usesRemaining + '|' + d.gameObject.transform.position.x + '|' + d.gameObject.transform.position.y + '|' + d.gameObject.transform.position.z + '|' + d.gameObject.transform.rotation.eulerAngles.x + '|' + d.gameObject.transform.rotation.eulerAngles.y + '|' + d.gameObject.transform.rotation.eulerAngles.z + '|' + d.refString;
			first = false;
		}
		return s;
	}

	[PunRPC]
	void RpcSetMyExpAndGpGained(int actorId, int expGained, int gpGained) {
		playerList[actorId].expGained = (uint)expGained;
		playerList[actorId].gpGained = (uint)gpGained;
	}

	[PunRPC]
	void RpcAskServerForDataGc() {
		if (PhotonNetwork.IsMasterClient || isVersusHostForThisTeam()) {
			string currentSceneName = SceneManager.GetActiveScene().name;
			string serializedObjectives = "";
			bool first = true;
			foreach (string s in objectives.objectivesText) {
				if (!first) {
					serializedObjectives += "#";
				}
				serializedObjectives += s;
				first = false;
			}

			serializedObjectives += "|";
			serializedObjectives += objectives.itemsRemaining;
			serializedObjectives += "|";
			serializedObjectives += objectives.stepsLeftToCompletion;
			serializedObjectives += "|";
			serializedObjectives += objectives.totalStepsToCompletion;
			serializedObjectives += "|";
			serializedObjectives += objectives.escaperCount;
			serializedObjectives += "|";
			serializedObjectives += objectives.escapeAvailable;
			serializedObjectives += "|";
			serializedObjectives += objectives.missionTimer1;
			serializedObjectives += "|";
			serializedObjectives += objectives.missionTimer2;
			serializedObjectives += "|";
			serializedObjectives += objectives.missionTimer3;
			serializedObjectives += "|";
			serializedObjectives += objectives.checkpoint1Passed;
			serializedObjectives += "|";
			serializedObjectives += objectives.checkpoint2Passed;
			serializedObjectives += "|";
			serializedObjectives += objectives.selectedEvacIndex;

			if (currentSceneName.StartsWith("Badlands1")) {
				for (int i = 0; i < items.Length; i++) {
					BombScript b = items[i].GetComponent<BombScript>();
					if (b.defused) {
						serializedObjectives += "|";
						serializedObjectives += b.bombId;
					}
				}
			} else if (currentSceneName.StartsWith("Badlands2")) {
				for (int i = 0; i < items.Length; i++) {
					FlareScript f = items[i].GetComponentInChildren<FlareScript>(true);
					if (!f.gameObject.activeInHierarchy) {
						serializedObjectives += "|" + f.flareId + ":0";
					} else {
						if (f.popped) {
							serializedObjectives += "|" + f.flareId + ":2";
						} else {
							serializedObjectives += "|" + f.flareId + ":1";
						}
					}
				}
			}
			int playerBeingKickedId = playerBeingKicked == null ? -1 : playerBeingKicked.ActorNumber;
			pView.RPC("RpcSyncDataGc", RpcTarget.All, lastGunshotHeardPos.x, lastGunshotHeardPos.y, lastGunshotHeardPos.z, lastGunshotTimer, endGameTimer, loadExitCalled,
				spawnMode, gameOver, (int)sectorsCleared, assaultMode, enemyTeamNearingVictoryTrigger, endGameWithWin, assaultModeChangedIndicator, serializedObjectives, GameControllerScript.missionTime, 
				currentVoteAction, playerBeingKickedId, playerBeingKickedName, voteInProgress, voteTimer, (int)yesVotes, (int)noVotes, SerializeDeployables(), teamMap);
		}
	}

	[PunRPC]
	void RpcSyncDataGc(float lastGunshotHeardPosX, float lastGunshotHeardPosY, float lastGunshotHeardPosZ, float lastGunshotTimer, float endGameTimer,
		bool loadExitCalled, SpawnMode spawnMode, bool gameOver, int sectorsCleared, bool assaultMode, bool enemyTeamNearingVictoryTrigger, 
		bool endGameWithWin, bool assaultModeChangedIndicator, string serializedObjectives, float missionTime, VoteActions currentVoteAction,
		int playerBeingKickedId, string playerBeingKickedName, bool voteInProgress, float voteTimer, int yesVotes, int noVotes, string serializedDeployables, string team) {
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
		this.currentVoteAction = currentVoteAction;
		this.playerBeingKicked = (playerBeingKickedId == -1 ? null : PhotonNetwork.CurrentRoom.GetPlayer(playerBeingKickedId));
		this.playerBeingKickedName = playerBeingKickedName;
		this.voteInProgress = voteInProgress;
		this.voteTimer = voteTimer;
		this.yesVotes = (short)yesVotes;
		this.noVotes = (short)noVotes;
		GameControllerScript.missionTime = missionTime;
		
		string[] parsedSerializations = serializedObjectives.Split('|');
		// Sync objectives text - TODO: Sync formatting too
		string[] objectivesText = parsedSerializations[0].Split('#');
		for (int i = 0; i < objectivesText.Length; i++) {
			this.objectives.objectivesText[i] = objectivesText[i];
		}
		// Sync itemsRemaining
		this.objectives.itemsRemaining = int.Parse(parsedSerializations[1]);
		// Sync stepsLeftToCompletion
		this.objectives.stepsLeftToCompletion = int.Parse(parsedSerializations[2]);
		// Sync totalStepsToCompletion
		this.objectives.totalStepsToCompletion = int.Parse(parsedSerializations[3]);
		// Sync escaperCount
		this.objectives.escaperCount = int.Parse(parsedSerializations[4]);
		// Sync escapeAvailable
		this.objectives.escapeAvailable = bool.Parse(parsedSerializations[5]);
		// Sync missionTimer1
		this.objectives.missionTimer1 = float.Parse(parsedSerializations[6]);
		// Sync missionTimer2
		this.objectives.missionTimer2 = float.Parse(parsedSerializations[7]);
		// Sync missionTimer3
		this.objectives.missionTimer3 = float.Parse(parsedSerializations[8]);
		// Sync checkpoint1Passed
		this.objectives.checkpoint1Passed = bool.Parse(parsedSerializations[9]);
		// Sync checkpoint2Passed
		this.objectives.checkpoint2Passed = bool.Parse(parsedSerializations[10]);
		// Sync selectedEvacIndex
		this.objectives.selectedEvacIndex = int.Parse(parsedSerializations[11]);
		// Sync mission specific data
		string currentSceneName = SceneManager.GetActiveScene().name;
		if (currentSceneName.StartsWith("Badlands1")) {
			if (parsedSerializations.Length > 12) {
				for (int i = 12; i < parsedSerializations.Length; i++) {
					int bombId = int.Parse(parsedSerializations[i]);
					for (int j = 0; j < items.Length; j++) {
						BombScript b = items[j].GetComponent<BombScript>();
						if (bombId == b.bombId) {
							b.Defuse();
						}
					}
				}
			}
			// Update objectives formatting
			if (this.objectives.stepsLeftToCompletion == 1) {
				this.objectives.RemoveObjective(0);
			}
			if (this.objectives.stepsLeftToCompletion == 0) {
				this.objectives.RemoveObjective(1);
			}
		} else if (currentSceneName.StartsWith("Badlands2")) {
			for (int i = 12; i < parsedSerializations.Length; i++) {
				string[] flareStatuses = parsedSerializations[i].Split(':');
				int flareId = int.Parse(flareStatuses[0]);
				int status = int.Parse(flareStatuses[1]);
				for (int j = 0; j < items.Length; j++) {
					FlareScript f = items[j].GetComponentInChildren<FlareScript>(true);
					if (f.flareId == flareId) {
						if (status == 0) {
							f.gameObject.SetActive(false);
						} else if (status == 1) {
							f.gameObject.SetActive(true);
							f.ToggleFlareTemplate(true);
						} else if (status == 2) {
							f.gameObject.SetActive(true);
							f.PopFlare();
						}
					}
				}
			}
			// Update objectives formatting
			if (this.objectives.stepsLeftToCompletion == 2) {
				this.objectives.RemoveObjective(0);
			}
			if (this.objectives.stepsLeftToCompletion == 1) {
				this.objectives.RemoveObjective(1);
			}
			if (this.objectives.stepsLeftToCompletion == 0) {
				this.objectives.RemoveObjective(2);
			}
		}

		if (serializedDeployables != null) {
			// Get serialized deployables
			parsedSerializations = serializedDeployables.Split(',');
			foreach (string d in parsedSerializations)
			{
				string[] dDetails = d.Split('|');
				// Sync deployable
				GameObject o = GameObject.Instantiate((GameObject)Resources.Load(dDetails[8]), new Vector3(float.Parse(dDetails[2]), float.Parse(dDetails[3]), float.Parse(dDetails[4])), Quaternion.Euler(float.Parse(dDetails[5]), float.Parse(dDetails[6]), float.Parse(dDetails[7])));
				DeployableScript dScript = o.GetComponent<DeployableScript>();
				dScript.deployableId = int.Parse(dDetails[0]);
				dScript.usesRemaining = short.Parse(dDetails[1]);
				DeployDeployable(dScript.deployableId, o);
			}
		}
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
		foreach (KeyValuePair<int, PlayerStat> entry in GameControllerScript.playerList)
		{
			Player p = PhotonNetwork.CurrentRoom.GetPlayer(entry.Key);
			if ((string)p.CustomProperties["team"] == "red") {
				total++;
			}
		}
		return total;
	}

	public int GetBlueTeamCount() {
		int total = 0;
		foreach (KeyValuePair<int, PlayerStat> entry in GameControllerScript.playerList)
		{
			Player p = PhotonNetwork.CurrentRoom.GetPlayer(entry.Key);
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
		if (GameControllerScript.playerList.ContainsKey(actorNo)) {
			GameControllerScript.playerList[actorNo].deaths++;
		}
	}

	public void AddToTotalKills(int actorNo) {
		pView.RPC("RpcAddToTotalKills", RpcTarget.All, actorNo);
	}

	[PunRPC]
	void RpcAddToTotalKills(int actorNo) {
		if (GameControllerScript.playerList.ContainsKey(actorNo)) {
			GameControllerScript.playerList[actorNo].kills++;
		}
	}

	public void StartVote(Player p, VoteActions voteAc)
	{
		if (p == null) return;
		if (voteDelay > 0f) return;
		if (gameOver) return;
		if (voteAc == VoteActions.KickPlayer && p.IsMasterClient) return;
		pView.RPC("RpcStartVote", RpcTarget.All, p.ActorNumber, voteAc, teamMap);
		voteDelay = VOTE_DELAY;
	}

	[PunRPC]
	void RpcStartVote(int actorNo, VoteActions voteAc, string team)
	{
		if (team != teamMap) return;
		if (voteAc == VoteActions.KickPlayer) {
			playerBeingKicked = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);
			if (playerBeingKicked == null) return;
			if (playerBeingKicked.IsMasterClient) {
				playerBeingKicked = null;
				return;
			}
		}
		playerBeingKickedName = playerBeingKicked.NickName;
		currentVoteAction = voteAc;
		noVotes = 0;
		yesVotes = 0;
		voteTimer = VOTE_TIME;
		iHaveVoted = false;
		voteInProgress = true;
	}

	void SetLeftPlayerAsKicked() {
		string currentKickedPlayers = (string)PhotonNetwork.CurrentRoom.CustomProperties["kickedPlayers"];
		if (string.IsNullOrEmpty(currentKickedPlayers)) {
			currentKickedPlayers = playerBeingKickedName;
		} else {
			currentKickedPlayers += ',' + playerBeingKickedName;
		}
		Hashtable h = new Hashtable();
		h.Add("kickedPlayers", currentKickedPlayers);
		PhotonNetwork.CurrentRoom.SetCustomProperties(h);
	}

	void KickPlayer(Player playerToKick)
	{
		if (gameOver) return;
		if (PhotonNetwork.LocalPlayer.IsMasterClient && playerToKick?.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) return;
		if (isVersusHostForThisTeam()) {
			if (playerToKick == null) {
				SetLeftPlayerAsKicked();
				return;
			}
			string nickname = playerToKick.NickName;
			pView.RPC("RpcAlertKickedPlayer", RpcTarget.All, playerToKick.ActorNumber);
			if (PhotonNetwork.LocalPlayer.IsMasterClient) {
				PhotonNetwork.CloseConnection(playerToKick);
			} else {
				pView.RPC("RpcKickPlayer", RpcTarget.MasterClient, playerToKick.ActorNumber);
			}
			string currentKickedPlayers = (string)PhotonNetwork.CurrentRoom.CustomProperties["kickedPlayers"];
			if (string.IsNullOrEmpty(currentKickedPlayers)) {
				currentKickedPlayers = nickname;
			} else {
				currentKickedPlayers += ',' + nickname;
			}
			Hashtable h = new Hashtable();
			h.Add("kickedPlayers", currentKickedPlayers);
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}
	}

	[PunRPC]
	void RpcKickPlayer(int actorNo)
	{
		PhotonNetwork.CloseConnection(PhotonNetwork.CurrentRoom.GetPlayer(actorNo));
	}

	[PunRPC]
	void RpcAlertKickedPlayer(int actorNo)
	{
		if (PhotonNetwork.LocalPlayer.ActorNumber == actorNo) {
			PlayerData.playerdata.disconnectReason = "YOU'VE BEEN KICKED FROM THE GAME.";
			OnDisconnected(DisconnectCause.DisconnectByClientLogic);
		}
	}

	public bool VoteHasSucceeded() {
		if (yesVotes > noVotes) {
			return true;
		}
		return false;
	}

	void HandleVoteCast() {
		if (voteInProgress && !iHaveVoted) {
			// You may not vote in a vote called to kick you
			if (currentVoteAction == VoteActions.KickPlayer && playerBeingKicked?.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) return;
			if (Input.GetKeyDown(KeyCode.F1)) {
				pView.RPC("RpcCastVote", RpcTarget.All, true, teamMap);
				iHaveVoted = true;
			} else if (Input.GetKeyDown(KeyCode.F2)) {
				pView.RPC("RpcCastVote", RpcTarget.All, false, teamMap);
				iHaveVoted = true;
			}
		}
	}

	[PunRPC]
	void RpcCastVote(bool yes, string team) {
		if (team != teamMap) return;
		if (yes) {
			yesVotes++;
		} else {
			noVotes++;
		}
	}

	void UpdateVote() 
	{	
		if (voteInProgress) {
			voteTimer -= Time.deltaTime;
			if (voteTimer <= 0f) {
				if (VoteHasSucceeded()) {
					if (currentVoteAction == VoteActions.KickPlayer) {
						KickPlayer(playerBeingKicked);
					}
				}
				voteInProgress = false;
			}
		}
	}

	public string CanCallVote()
	{
		if (voteInProgress) {
			return "THERE IS CURRENTLY A VOTE IN PROGRESS. PLEASE WAIT UNTIL IT ENDS BEFORE CALLING ANOTHER.";
		} else if (voteDelay > 0f) {
			return "YOU'VE RECENTLY CALLED A VOTE. PLEASE WAIT SOME TIME BEFORE CALLING ANOTHER.";
		}
		return null;
	}

	public void ToggleMyselfSpeaking(bool b)
	{
		pView.RPC("RpcTogglePlayerSpeaking", RpcTarget.All, b, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.LocalPlayer.NickName, teamMap);
	}

	[PunRPC]
	void RpcTogglePlayerSpeaking(bool b, int actorNo, string playerName, string team)
	{
		if (team != teamMap) return;
		TogglePlayerSpeaking(b, actorNo, playerName);
	}

	public void TogglePlayerSpeaking(bool b, int actorNo, string playerName)
	{
		if (b) {
			PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().AddPlayerSpeakingIndicator(actorNo, playerName);
		} else {
			PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().RemovePlayerSpeakingIndicator(actorNo);
		}
	}

	public void SendVoiceCommand(char type, int i)
	{
		pView.RPC("RpcSendVoiceCommand", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, (int)type, i, (int)InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender, teamMap);
	}

	[PunRPC]
	void RpcSendVoiceCommand(string playerName, int type, int i, int gender, string team)
	{
		if (team != teamMap) return;
		char typeChar = (char)type;
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().PlayVoiceCommand(playerName, typeChar, i);
		PlayerData.playerdata.inGamePlayerReference.GetComponent<AudioControllerScript>().PlayVoiceCommand(typeChar, i, (char)gender);
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
