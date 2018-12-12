using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;

public class GameControllerScript : MonoBehaviourPunCallbacks {

    // Timer
    public static float missionTime;
    public static float MAX_MISSION_TIME = 1800f;

	public int currentMap;

	// Audio stuff
	public AudioSource bgm;
	private AudioSource fxSound1;
	private AudioSource fxSound2;
	private AudioSource fxSound3;
	private AudioSource fxSound4;
	private AudioSource fxSound5;
	public AudioClip stealthMusic;
	public AudioClip assaultMusic;
	public AudioClip headshotSound;
	public AudioClip playerHitSound;
	public AudioClip playerGruntSound1;
	public AudioClip playerGruntSound2;
	public AudioClip hitmarkerSound;
	public AudioClip sirenSound;

    // variable for last gunshot position
    public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	private Vector3 lastGunshotHeardPosClone = Vector3.negativeInfinity;
	private float lastGunshotTimer = 0f;
    public float endGameTimer = 0f;
    public static Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject> ();
	public GameObject[] enemyList;

    // Bomb defusal mission variables
	public GameObject[] bombs;
    public int bombsRemaining;
	public bool gameOver;
    private bool exitLevelLoaded;
    public bool escapeAvailable;

	public Camera c;
	public GameObject exitPoint;

	public int deadCount;
	public int escaperCount;
	public bool assaultMode;

	private PhotonView pView;

	// Use this for initialization
    void Start () {
        PhotonNetwork.AutomaticallySyncScene = true;
		Physics.IgnoreLayerCollision (9, 12);
		Physics.IgnoreLayerCollision (14, 12);
		Physics.IgnoreLayerCollision (15, 12);
		//playerList = GameObject.FindGameObjectsWithTag ("Player");
		fxSound1 = GetComponentsInChildren<AudioSource>() [1];
		fxSound2 = GetComponentsInChildren<AudioSource>() [2];
		fxSound3 = GetComponentsInChildren<AudioSource>() [3];
		fxSound4 = GetComponentsInChildren<AudioSource>() [4];
		fxSound5 = GetComponentsInChildren<AudioSource> () [5];
		assaultMode = false;
		gameOver = false;
		deadCount = 0;
		escaperCount = 0;
		escapeAvailable = false;
		pView = GetComponent<PhotonView> ();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        exitLevelLoaded = false;

        missionTime = 0f;
		lastGunshotTimer = 10f;

	}

	// Update is called once per frame
	void Update () {
        if (currentMap == 1)
        {
			// Control BGM
			if (assaultMode) {
				if (!bgm.isPlaying || !bgm.clip.name.Equals(assaultMusic.name)) {
					bgm.clip = assaultMusic;
					bgm.Play ();
					fxSound5.clip = sirenSound;
					fxSound5.loop = true;
					fxSound5.Play ();
					StartCoroutine (RestartBgmTimer(assaultMusic.length - bgm.time));
				}
			} else {
				if (!bgm.isPlaying || !bgm.clip.name.Equals (stealthMusic.name)) {
					bgm.clip = stealthMusic;
					bgm.Play ();
				}
			}

            // Update waypoints
            if (c == null)
            {
                GameObject temp = GameObject.FindWithTag("MainCamera");
                if (temp != null) c = temp.GetComponent<Camera>();
            }
            /**if (bombs == null || bombs.Length == 0)
            {
                bombs = GameObject.FindGameObjectsWithTag("Bomb");
            }*/

            // Check if the mission is over or if all players eliminated
            if (deadCount == PhotonNetwork.CurrentRoom.Players.Count) {
                if (!gameOver)
                {
                    endGameTimer = 8f;
                    gameOver = true;
                }
            } else if (bombsRemaining == 0) {
				escapeAvailable = true;
				if (!gameOver && CheckEscape ()) {
                    // If they can escape, end the game and bring up the stat board
                    endGameTimer = 3f;
					gameOver = true;
					EndGame();
				}
			}

            if (PhotonNetwork.IsMasterClient) {
                UpdateMissionTime();
            }

            // Check if out of time (30 mins)

			// Cbeck if mode has been changed to assault or not
			if (!assaultMode) {
				if (!lastGunshotHeardPos.Equals (Vector3.negativeInfinity)) {
					pView.RPC ("UpdateAssaultMode", RpcTarget.All, true);
				}
			}

            UpdateEndGameTimer();
        }
	}

	IEnumerator RestartBgmTimer(float secs) {
		yield return new WaitForSeconds (secs);
		bgm.Stop ();
		bgm.time = 1.15f;
		bgm.Play ();
		StartCoroutine (RestartBgmTimer(assaultMusic.length - bgm.time));
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

	void ResetLastGunshotPos() {
		if (!Vector3.Equals (lastGunshotHeardPos, lastGunshotHeardPosClone)) {
			lastGunshotTimer = 10f;
			lastGunshotHeardPosClone = new Vector3 (lastGunshotHeardPos.x, lastGunshotHeardPos.y, lastGunshotHeardPos.z);
		} else {
			lastGunshotTimer -= Time.deltaTime;
			if (lastGunshotTimer <= 0f) {
				lastGunshotTimer = 10f;
				lastGunshotHeardPos = Vector3.negativeInfinity;
				lastGunshotHeardPosClone = Vector3.negativeInfinity;
			}
		}
	}

	void EndGame() {
		// Remove all enemies
		for (int i = 0; i < enemyList.Length; i++) {
			enemyList [i].SetActive (false);
		}
	}

	public void IncrementDeathCount() {
		pView.RPC ("RpcIncrementDeathCount", RpcTarget.MasterClient);
	}

	[PunRPC]
	void RpcIncrementDeathCount() {
		deadCount++;
	}

	public void IncrementEscapeCount() {
		pView.RPC ("RpcIncrementEscapeCount", RpcTarget.MasterClient);
	}

	[PunRPC]
	void RpcIncrementEscapeCount() {
		escaperCount++;
	}

    void UpdateMissionTime() {
        missionTime += Time.deltaTime;
        pView.RPC ("RpcUpdateMissionTime", RpcTarget.Others, missionTime);
    }

    [PunRPC]
    void RpcUpdateMissionTime(float t) {
        missionTime = t;
    }

	// When someone leaves the game in the middle of an escape, reset the values to recount
	void ResetEscapeValues() {
		deadCount = 0;
		escaperCount = 0;
	}

	void ChangeCursorStatus() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			} else {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}

    void CheckOutOfTime() {
        if (missionTime >= MAX_MISSION_TIME) {
            gameOver = true;
            EndGame();
        }
    }

	/**public override void OnJoinedRoom()
	{
		GameObject[] playerPrefabs = GameObject.FindGameObjectsWithTag ("Player");
		for (int i = 0; i < playerPrefabs.Length; i++)
		{
			playerList.Add (playerPrefabs[i].GetComponent<PhotonView>().OwnerActorNr, playerPrefabs[i]);
		}

	}*/

	// TODO: Need to clear player list, go back to lobby upon leaving match
	public override void OnLeftRoom()
	{
		// Destroy the 
		foreach (GameObject entry in playerList.Values)
		{
			Destroy(entry.gameObject);
		}

		playerList.Clear();
		playerList = null;
		PhotonNetwork.JoinLobby();
	}

	// When a player leaves the room in the middle of an escape, resend the escape status of the player (dead or escaped/not escaped)
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		Destroy (playerList[otherPlayer.ActorNumber].gameObject);
		playerList.Remove (otherPlayer.ActorNumber);
		Debug.Log (playerList.Count);
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

	public void PlayHeadshotSound() {
		fxSound1.clip = headshotSound;
		fxSound1.Play ();
	}

	public void PlayGruntSound() {
		int r = Random.Range (1, 3);
		if (r == 1) {
			fxSound2.clip = playerGruntSound1;
		} else {
			fxSound2.clip = playerGruntSound2;
		}
		fxSound2.Play ();
	}

	public void PlayHitmarkerSound() {
		fxSound3.clip = hitmarkerSound;
		fxSound3.Play ();
	}

	public void PlayHitSound() {
		fxSound4.clip = playerHitSound;
		fxSound4.Play ();
	}

    void UpdateEndGameTimer() {
        if (gameOver) {
            if (endGameTimer > 0f)
            {
                endGameTimer -= Time.deltaTime;
            }
            if (endGameTimer <= 0f && PhotonNetwork.IsMasterClient && !exitLevelLoaded) {
                exitLevelLoaded = true;
                if (deadCount == PhotonNetwork.CurrentRoom.Players.Count) {
                    PhotonNetwork.LoadLevel("GameOverFail");
                } else {
                    PhotonNetwork.LoadLevel("GameOverSuccess");
                }
            }
        }
    }

}
