using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameControllerScript : MonoBehaviourPunCallbacks {

    // Timer
    public static float missionTime;
    public static float MAX_MISSION_TIME = 1800f;

	public int currentMap;

    // variable for last gunshot position
    public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	private Vector3 lastGunshotHeardPosClone = Vector3.negativeInfinity;
	private float lastGunshotTimer = 0f;
    public float endGameTimer = 0f;
	private bool loadExitCalled;
	public static Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject> ();
	public static Dictionary<string, int> totalKills = new Dictionary<string, int> ();
	public static Dictionary<string, int> totalDeaths = new Dictionary<string, int> ();
	public GameObject[] enemyList;

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
	public bool assaultMode;
    // Sync mission time to clients every 10 seconds
    private float syncMissionTimeTimer;

	private PhotonView pView;

	// Use this for initialization
    void Start () {
        PhotonNetwork.AutomaticallySyncScene = true;
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

	}

	// Update is called once per frame
	void Update () {
		if (!PhotonNetwork.InRoom) {
			return;
		}
        if (currentMap == 1)
        {
			if (bombsRemaining == 0) {
				escapeAvailable = true;
			}
            if (!gameOver)
            {
                UpdateMissionTime();
            }
            if (PhotonNetwork.IsMasterClient) {
				// Check if the mission is over or if all players eliminated or out of time
				if (deadCount == PhotonNetwork.CurrentRoom.Players.Count || CheckOutOfTime()) {
					if (!gameOver)
					{
						pView.RPC ("RpcEndGame", RpcTarget.All, 9f);
					}
				} else if (bombsRemaining == 0) {
					if (!gameOver && CheckEscape ()) {
						// If they can escape, end the game and bring up the stat board
						pView.RPC ("RpcEndGame", RpcTarget.All, 3f);
					}
				}

				// Cbeck if mode has been changed to assault or not
				if (!assaultMode) {
					if (!lastGunshotHeardPos.Equals (Vector3.negativeInfinity)) {
						pView.RPC ("UpdateAssaultMode", RpcTarget.All, true);
					}
				}

				ResetLastGunshotPos ();
				UpdateEndGameTimer();
            }
        }
	}

	[PunRPC]
	void RpcEndGame(float f) {
		endGameTimer = f;
		gameOver = true;
	}

	[PunRPC]
	public void UpdateAssaultMode(bool assaultInProgress) {
		StartCoroutine (UpdateAssaultModeTimer(5f, assaultInProgress));
	}

	IEnumerator UpdateAssaultModeTimer(float secs, bool assaultInProgress) {
		yield return new WaitForSeconds (secs);
		assaultMode = assaultInProgress;
	}

	public bool CheckEscape() {
		if (deadCount + escaperCount == PhotonNetwork.CurrentRoom.PlayerCount) {
			return true;
		}
		return false;
	}

	[PunRPC]
	void RpcSetLastGunshotHeardTimer(float t) {
		lastGunshotTimer = t;
	}

	public void SetLastGunshotHeardPos(float x, float y, float z) {
		pView.RPC ("RpcSetLastGunshotHeardPos", RpcTarget.All, true, x, y, z);
	}

	[PunRPC]
	void RpcSetLastGunshotHeardPos(bool b, float x, float y, float z) {
		if (!b) {
			lastGunshotHeardPos = Vector3.negativeInfinity;
		} else {
			lastGunshotHeardPos = new Vector3 (x, y, z);
		}
	}

	[PunRPC]
	void RpcSetLastGunshotHeardPosClone(bool b, float x, float y, float z) {
		if (!b) {
			lastGunshotHeardPosClone = Vector3.negativeInfinity;
		} else {
			lastGunshotHeardPosClone = new Vector3 (x, y, z);
		}
	}

	void ResetLastGunshotPos() {
		if (!Vector3.Equals (lastGunshotHeardPos, lastGunshotHeardPosClone)) {
			pView.RPC ("RpcSetLastGunshotHeardTimer", RpcTarget.All, 10f);
			pView.RPC ("RpcSetLastGunshotHeardPosClone", RpcTarget.All, true, lastGunshotHeardPos.x, lastGunshotHeardPos.y, lastGunshotHeardPos.z);
		} else {
			lastGunshotTimer -= Time.deltaTime;
			if (lastGunshotTimer <= 0f) {
				pView.RPC ("RpcSetLastGunshotHeardTimer", RpcTarget.All, 10f);
				pView.RPC ("RpcSetLastGunshotHeardPos", RpcTarget.All, false, 0f, 0f, 0f);
				pView.RPC ("RpcSetLastGunshotHeardPosClone", RpcTarget.All, false, 0f, 0f, 0f);
			}
		}
	}

	public void IncrementDeathCount() {
		pView.RPC ("RpcIncrementDeathCount", RpcTarget.All);
	}

	[PunRPC]
	void RpcIncrementDeathCount() {
		deadCount++;
	}

	public void ConvertCounts(int dead, int escape) {
		pView.RPC ("RpcConvertCounts", RpcTarget.All, dead, escape);
	}

	[PunRPC]
	void RpcConvertCounts(int dead, int escape) {
		deadCount += dead;
		escaperCount += escape;
	}

	public void IncrementEscapeCount() {
		pView.RPC ("RpcIncrementEscapeCount", RpcTarget.All);
	}

	[PunRPC]
	void RpcIncrementEscapeCount() {
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
    void RpcUpdateMissionTime(float t) {
        missionTime = t;
    }

    [PunRPC]
    void RpcSendMissionTimeToClients()
    {
        pView.RPC("RpcUpdateMissionTime", RpcTarget.Others, missionTime);
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
		foreach (GameObject entry in playerList.Values)
		{
			Destroy(entry.gameObject);
		}

		playerList.Clear();
		totalKills.Clear ();
		totalDeaths.Clear ();
		/**playerList = null;
		totalKills = null;
		totalDeaths = null;*/
	}

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		ResetEscapeValues ();
		foreach (GameObject entry in playerList.Values)
		{
			entry.GetComponent<PlayerActionScript> ().escapeValueSent = false;
		}

		Destroy (playerList[otherPlayer.ActorNumber].gameObject);
		playerList.Remove (otherPlayer.ActorNumber);
		totalKills.Remove (otherPlayer.NickName);
		totalDeaths.Remove (otherPlayer.NickName);
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
	void RpcUpdateEndGameTimer(float t) {
		endGameTimer = t;
	}

	public void DecrementBombsRemaining() {
		pView.RPC ("RpcDecrementBombsRemaining", RpcTarget.All);
	}

	[PunRPC]
	void RpcDecrementBombsRemaining() {
		bombsRemaining--;
	}

	[PunRPC]
	void RpcSetExitLevelLoaded() {
		exitLevelLoaded = true;
		exitLevelLoadedTimer = 4f;
	}

    void UpdateEndGameTimer() {
        if (gameOver) {
            if (endGameTimer > 0f)
            {
                endGameTimer -= Time.deltaTime;
				pView.RPC ("RpcUpdateEndGameTimer", RpcTarget.Others, endGameTimer);
            }

            if (endGameTimer <= 0f) {
				if (!exitLevelLoaded) {
					pView.RPC ("RpcSetExitLevelLoaded", RpcTarget.All);
				} else {
					if (exitLevelLoadedTimer <= 0f && !loadExitCalled) {
						loadExitCalled = true;
						if (deadCount == PhotonNetwork.CurrentRoom.Players.Count) {
							SwitchToGameOverScene (false);
						} else {
							SwitchToGameOverScene (true);
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
		SceneManager.LoadScene (0);
	}

	void SwitchToGameOverScene(bool win) {
		if (!win) {
			PhotonNetwork.LoadLevel("GameOverFail");
		} else {
			PhotonNetwork.LoadLevel("GameOverSuccess");
		}
	}

}
