using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameControllerScript : MonoBehaviourPunCallbacks {

    // Timer
    public static float missionTime;
    public static float MAX_MISSION_TIME = 1800f;
    private static float FORFEIT_CHECK_DELAY = 10f;

	public int currentMap;
    public string teamMap;

    // variable for last gunshot position
    public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	private Vector3 lastGunshotHeardPosClone = Vector3.negativeInfinity;
	private float lastGunshotTimer = 0f;
    public float endGameTimer = 0f;
	private bool loadExitCalled;
	public static Dictionary<int, PlayerStat> playerList = new Dictionary<int, PlayerStat> ();
	public Dictionary<short, GameObject> coverSpots;
	public Dictionary<int, GameObject> enemyList = new Dictionary<int, GameObject> ();
	private Dictionary<int, GameObject> pickupList = new Dictionary<int, GameObject>();
	public ArrayList enemyAlertMarkers;
	public Queue enemyMarkerRemovalQueue;

    // Bomb defusal mission variables
	public GameObject[] bombs;
    public int bombsRemaining;
	public bool gameOver;
    public bool exitLevelLoaded;
	private float exitLevelLoadedTimer;
    public bool escapeAvailable;
	public short sectorsCleared;

	public GameObject exitPoint;
	public Transform spawnLocation;

	public int deadCount;
	public int escaperCount;
	// TODO: These numbers need to be instantiated and networked
	public int redTeamPlayerCount;
	public int blueTeamPlayerCount;
	public bool assaultMode;
    // Sync mission time to clients every 10 seconds
    private float syncMissionTimeTimer;

	private PhotonView pView;

    // Match state data
    public char matchType;
    private string myTeam;
    private string opposingTeam;
    private short objectiveCount;
    private short objectiveCompleted;
    private float forfeitDelay;
	public bool enemyTeamNearingVictoryTrigger;
	public string versusAlertMessage;
	private bool endingGainsCalculated;

	// Use this for initialization
	void Awake() {
		coverSpots = new Dictionary<short, GameObject>();
        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        opposingTeam = (myTeam == "red" ? "blue" : "red");
        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp")
        {
            matchType = 'C';
        } else if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus")
        {
            matchType = 'V';
        }
        objectiveCount = 0;
        objectiveCompleted = 0;
        forfeitDelay = FORFEIT_CHECK_DELAY;
	}

    void Start () {
        if (matchType == 'C') {
            PhotonNetwork.AutomaticallySyncScene = true;
        } else if (matchType == 'V') {
            PhotonNetwork.AutomaticallySyncScene = false;
        }
		Physics.IgnoreLayerCollision (9, 12);
		Physics.IgnoreLayerCollision (9, 15);
		Physics.IgnoreLayerCollision (14, 12);
		Physics.IgnoreLayerCollision (15, 12);
		Physics.IgnoreLayerCollision (14, 15);

		assaultMode = false;
		gameOver = false;
		deadCount = 0;
		escaperCount = 0;
		escapeAvailable = false;
		pView = GetComponent<PhotonView> ();

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
		lastGunshotHeardPosClone = Vector3.negativeInfinity;

    	enemyAlertMarkers = new ArrayList ();
		enemyMarkerRemovalQueue = new Queue();

	}

	// Update is called once per frame
	void Update () {
		if (!PhotonNetwork.InRoom) {
			return;
		}
		if (matchType == 'C') {
			GameOverCheckForCampaign();
		} else if (matchType == 'V') {
			GameOverCheckForVersus();
		}
	}

	void GameOverCheckForCampaign() {
		if (currentMap == 1) {
			// Auto end game for testing
            // if (Input.GetKeyDown(KeyCode.B)) {
            // 	pView.RPC ("RpcEndGame", RpcTarget.All, 3f);
            // }
            objectiveCount = 5;
			if (bombsRemaining == 0) {
				escapeAvailable = true;
			}
            if (!gameOver)
            {
                UpdateMissionTime();
            } else {
				if (!endingGainsCalculated) {
					endingGainsCalculated = true;
					int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;
					pView.RPC("RpcSetMyExpAndGpGained", RpcTarget.All, myActorId, (int)CalculateExpGained(playerList[myActorId].kills, playerList[myActorId].deaths), (int)CalculateGpGained(playerList[myActorId].kills, playerList[myActorId].deaths));
				}
			}
			if (PhotonNetwork.IsMasterClient) {
				// Check if the mission is over or if all players eliminated or out of time
				if (deadCount == PhotonNetwork.CurrentRoom.Players.Count || CheckOutOfTime())
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndGame", RpcTarget.All, 9f);

					}
				}
				else if (bombsRemaining == 0)
				{
					if (!gameOver && CheckEscapeForCampaign())
					{
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndGame", RpcTarget.All, 3f);
					}
				}

				// Cbeck if mode has been changed to assault or not
				if (!assaultMode) {
					if (!lastGunshotHeardPos.Equals (Vector3.negativeInfinity)) {
						pView.RPC ("UpdateAssaultMode", RpcTarget.All, true, teamMap);
					}
				}


				ResetLastGunshotPos ();
				UpdateEndGameTimer();
			}
		}
	}

	void GameOverCheckForVersus() {
		if (currentMap == 1) {
			// Auto end game for testing
            // if (Input.GetKeyDown(KeyCode.B)) {
            // 	pView.RPC ("RpcEndGame", RpcTarget.All, 3f);
            // }
            objectiveCount = 5;
			if (bombsRemaining == 0) {
				escapeAvailable = true;
			}
            if (!gameOver)
            {
                UpdateMissionTime();
            } else {
				if (!endingGainsCalculated) {
					endingGainsCalculated = true;
					int myActorId = PhotonNetwork.LocalPlayer.ActorNumber;
					bool winner = (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties[myTeam + "Score"]) == 100);
					pView.RPC("RpcSetMyExpAndGpGained", RpcTarget.All, myActorId, (int)CalculateExpGained(playerList[myActorId].kills, playerList[myActorId].deaths, winner), (int)CalculateGpGained(playerList[myActorId].kills, playerList[myActorId].deaths, winner));
				}
			}
			if (isVersusHostForThisTeam()) {
				int playerCount = (teamMap == "R" ? redTeamPlayerCount : blueTeamPlayerCount);
				if (deadCount == playerCount)
				{
					if (!gameOver)
					{
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, (teamMap == "R" ? "B" : "R"), false, true);
					}
				} else if (CheckOutOfTime()) {
					if (!gameOver) {
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 9f, "T", false, false);
					}
				} else if (bombsRemaining == 0)
				{
					if (!gameOver && CheckEscapeForVersus())
					{
						// Set completion to 100%
						SetMyTeamScore(100);
						// If they can escape, end the game and bring up the stat board
						pView.RPC("RpcEndVersusGame", RpcTarget.All, 3f, teamMap, false, false);
					}
				}
				// Check to see if either team has forfeited
				DetermineEnemyTeamForfeited();

				// Cbeck if mode has been changed to assault or not
				if (!assaultMode) {
					if (!lastGunshotHeardPos.Equals (Vector3.negativeInfinity)) {
						pView.RPC ("UpdateAssaultMode", RpcTarget.All, true, teamMap);
					}
				}


				ResetLastGunshotPos ();
				UpdateEndGameTimer();
			}
		}
		if (forfeitDelay > 0f) {
            forfeitDelay -= Time.deltaTime;
        }
	}

	[PunRPC]
	void RpcEndGame(float f) {
		endGameTimer = f;
		gameOver = true;
	}

    [PunRPC]
    void RpcEndVersusGame(float f, string winner, bool wasForfeit, bool enemyTeamWasEliminated)
    {
		if (gameOver) return;

        endGameTimer = f;
        gameOver = true;
        
		Hashtable h = new Hashtable();

		if (winner == "R") {
			h.Add("redStatus", "win");
			h.Add("blueStatus", "lose");
			if (teamMap != winner) {
				versusAlertMessage = "The enemy team has won!";
			} else {
				if (enemyTeamWasEliminated) {
					versusAlertMessage = "The enemy team has been eliminated!";
				}
			}
		} else if (winner == "B") {
			h.Add("redStatus", "lose");
			h.Add("blueStatus", "win");
			if (teamMap != winner) {
				versusAlertMessage = "The enemy team has won!";
			} else {
				if (enemyTeamWasEliminated) {
					versusAlertMessage = "The enemy team has been eliminated!";
				}
			}
		} else if (winner == "T") {
			h.Add("redStatus", "lose");
			h.Add("blueStatus", "lose");
			versusAlertMessage = "Time up!";
		}

		PhotonNetwork.CurrentRoom.SetCustomProperties(h);

		if (wasForfeit) {
			versusAlertMessage = "The enemy team has forfeited!";
		}
    }

    [PunRPC]
	public void UpdateAssaultMode(bool assaultInProgress, string team) {
        if (team != teamMap) return;
		StartCoroutine (UpdateAssaultModeTimer(5f, assaultInProgress));
	}

	IEnumerator UpdateAssaultModeTimer(float secs, bool assaultInProgress) {
		yield return new WaitForSeconds (secs);
		assaultMode = assaultInProgress;
		if (assaultInProgress) {
			ClearEnemyAlertMarkers();
		}
	}

	bool CheckEscapeForCampaign() {
		if (deadCount + escaperCount == PhotonNetwork.CurrentRoom.PlayerCount) {
			return true;
		}
		return false;
	}

	bool CheckEscapeForVersus() {
		if (teamMap == "R") {
			if (deadCount + escaperCount == redTeamPlayerCount) {
				return true;
			}
		} else if (teamMap == "B") {
			if (deadCount + escaperCount == blueTeamPlayerCount) {
				return true;
			}
		}
		return false;
	}

    void DetermineEnemyTeamForfeited()
    {
        if (gameOver) return;

        // Check if the other team has forfeited - can be determine by any players left on the opposing team
        if (forfeitDelay <= 0f) {
            if ((teamMap == "R" && blueTeamPlayerCount == 0) || (teamMap == "B" && redTeamPlayerCount == 0)) {
				// Couldn't find another player on the other team. This means that they forfeit
            	pView.RPC("RpcEndVersusGame", RpcTarget.All, 3f, teamMap, true, false);
			}
        }
    }

	[PunRPC]
	void RpcSetLastGunshotHeardTimer(float t, string team) {
        if (team != teamMap) return;
		lastGunshotTimer = t;
	}

	public void SetLastGunshotHeardPos(float x, float y, float z) {
		pView.RPC ("RpcSetLastGunshotHeardPos", RpcTarget.All, true, x, y, z, teamMap);
	}

	[PunRPC]
	void RpcSetLastGunshotHeardPos(bool b, float x, float y, float z, string team) {
        if (team != teamMap) return;
		if (!b) {
			lastGunshotHeardPos = Vector3.negativeInfinity;
		} else {
			lastGunshotHeardPos = new Vector3 (x, y, z);
		}
	}

	[PunRPC]
	void RpcSetLastGunshotHeardPosClone(bool b, float x, float y, float z, string team) {
        if (team != teamMap) return;
		if (!b) {
			lastGunshotHeardPosClone = Vector3.negativeInfinity;
		} else {
			lastGunshotHeardPosClone = new Vector3 (x, y, z);
		}
	}

	void ResetLastGunshotPos() {
		if (!Vector3.Equals (lastGunshotHeardPos, lastGunshotHeardPosClone)) {
			pView.RPC ("RpcSetLastGunshotHeardTimer", RpcTarget.All, 10f, teamMap);
			pView.RPC ("RpcSetLastGunshotHeardPosClone", RpcTarget.All, true, lastGunshotHeardPos.x, lastGunshotHeardPos.y, lastGunshotHeardPos.z, teamMap);
		} else {
			lastGunshotTimer -= Time.deltaTime;
			if (lastGunshotTimer <= 0f) {
				pView.RPC ("RpcSetLastGunshotHeardTimer", RpcTarget.All, 10f, teamMap);
				pView.RPC ("RpcSetLastGunshotHeardPos", RpcTarget.All, false, 0f, 0f, 0f, teamMap);
				pView.RPC ("RpcSetLastGunshotHeardPosClone", RpcTarget.All, false, 0f, 0f, 0f, teamMap);
			}
		}
	}

	public void IncrementDeathCount() {
		pView.RPC ("RpcIncrementDeathCount", RpcTarget.All, teamMap);
	}

	[PunRPC]
	void RpcIncrementDeathCount(string team) {
        if (team != teamMap) return;
		deadCount++;
	}

	public void ConvertCounts(int dead, int escape) {
		pView.RPC ("RpcConvertCounts", RpcTarget.All, dead, escape, teamMap);
	}

	[PunRPC]
	void RpcConvertCounts(int dead, int escape, string team) {
        if (team != teamMap) return;
		deadCount += dead;
		escaperCount += escape;
	}

	public void IncrementEscapeCount() {
		pView.RPC ("RpcIncrementEscapeCount", RpcTarget.All, teamMap);
	}

	[PunRPC]
	void RpcIncrementEscapeCount(string team) {
        if (team != teamMap) return;
        escaperCount++;
	}

    void UpdateMissionTime() {
        missionTime += Time.deltaTime;

        // Query server for sync time if not master client every 30 seconds
        if (!PhotonNetwork.IsMasterClient)
        {
            syncMissionTimeTimer -= Time.deltaTime;
            if (syncMissionTimeTimer <= 0f)
            {
                syncMissionTimeTimer = 30f;
                pView.RPC("RpcSendMissionTimeToClients", RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    void RpcUpdateMissionTime(float t, string team) {
        if (team != teamMap) return;
        missionTime = t;
    }

    [PunRPC]
    void RpcSendMissionTimeToClients()
    {
        pView.RPC("RpcUpdateMissionTime", RpcTarget.Others, missionTime, teamMap);
    }

    // When someone leaves the game in the middle of an escape, reset the values to recount
    void ResetEscapeValues() {
		deadCount = 0;
		escaperCount = 0;
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

	public override void OnLeftRoom()
	{
		foreach (PlayerStat entry in playerList.Values)
		{
			Destroy(entry.objRef);
		}

		playerList.Clear();
	}

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		if (!playerList.ContainsKey(otherPlayer.ActorNumber)) return;
		ResetEscapeValues ();
		foreach (PlayerStat entry in playerList.Values)
		{
			entry.objRef.GetComponent<PlayerActionScript> ().escapeValueSent = false;
		}

		char wasTeam = playerList[otherPlayer.ActorNumber].team;
		Destroy (playerList[otherPlayer.ActorNumber].objRef);
		playerList.Remove (otherPlayer.ActorNumber);

		// Update team counts if versus mode
		if (matchType == 'V') {
			if (PhotonNetwork.IsMasterClient) {
				if (wasTeam == 'R') {
					redTeamPlayerCount--;
				} else if (wasTeam == 'B') {
					blueTeamPlayerCount--;
				}
				pView.RPC("RpcSetTeamCounts", RpcTarget.All, redTeamPlayerCount, blueTeamPlayerCount);
			}
		}
	}

	[PunRPC]
	void RpcSetTeamCounts(int red, int blue) {
		redTeamPlayerCount = red;
		blueTeamPlayerCount = blue;
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

	public void DecrementBombsRemaining() {
		pView.RPC ("RpcDecrementBombsRemaining", RpcTarget.All, teamMap);
	}

	[PunRPC]
	void RpcDecrementBombsRemaining(string team) {
        if (team != teamMap) return;
        bombsRemaining--;
        UpdateMyTeamScore(true);
	}

	[PunRPC]
	void RpcSetExitLevelLoaded(string team) {
        if (team != teamMap) return;
        exitLevelLoaded = true;
		exitLevelLoadedTimer = 4f;
	}

    void UpdateMyTeamScore(bool increase)
    {
        if (increase)
        {
            objectiveCompleted++;
        } else
        {
            objectiveCompleted--;
        }
        SetMyTeamScore((short)(((float)objectiveCompleted / (float)objectiveCount) * 100f));
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
                            if (deadCount == PhotonNetwork.CurrentRoom.Players.Count)
                            {
                                SwitchToGameOverScene(false);
                            }
                            else
                            {
                                SwitchToGameOverScene(true);
                            }
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
		if (!cause.ToString ().Equals ("DisconnectByClientLogic")) {
			PlayerData.playerdata.disconnectedFromServer = true;
			PlayerData.playerdata.disconnectReason = cause.ToString ();
		}
		SceneManager.LoadScene ("Title");
	}

	void SwitchToGameOverScene(bool win) {
		if (!win) {
			PhotonNetwork.LoadLevel("GameOverFail");
		} else {
			PhotonNetwork.LoadLevel("GameOverSuccess");
		}
	}

    void ClearEnemyAlertMarkers() {
		enemyAlertMarkers.Clear();
		enemyMarkerRemovalQueue.Clear();
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

	public void SetEnemyTeamNearingVictoryMessage() {
		enemyTeamNearingVictoryTrigger = true;
		versusAlertMessage = "Enemy team is nearing victory!";
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
		if (map == "BetaLevelNetwork") {
			if (deadCount == playerList.Count || CheckOutOfTime()) {
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
		}
		return 'C';
	}

	char GetCompletionGradeForMapVersus(string map) {
		if (map == "BetaLevelNetworkRed" || map == "BetaLevelNetworkBlue") {
			if (CheckOutOfTime() || (teamMap == "R" && deadCount == redTeamPlayerCount) || (teamMap == "B" && deadCount == blueTeamPlayerCount)) {
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
		playerList[actorId].gpGained = (uint)expGained;
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
