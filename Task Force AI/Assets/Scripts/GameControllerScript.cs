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
    private const float MAX_MISSION_TIME = 1800f;

	public int currentMap;
    // variable for last gunshot position
    public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	public static ArrayList playerList = new ArrayList();
	public GameObject[] enemyList;

    // Bomb defusal mission variables
	public GameObject[] bombs;
    public int bombsRemaining;
	public bool gameOver;
    public bool escapeAvailable;

	public Camera c;
	public GameObject exitPoint;

	public int deadCount;
	public int escaperCount;

	private PhotonView pView;

	// Use this for initialization
    void Start () {
		//playerList = GameObject.FindGameObjectsWithTag ("Player");
		gameOver = false;
		exitPoint = GameObject.Find ("ExitPoint");
		deadCount = 0;
		escaperCount = 0;
		escapeAvailable = false;
		pView = GetComponent<PhotonView> ();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        missionTime = 0f;

	}

	// Update is called once per frame
	void Update () {
        if (currentMap == 1)
        {

            // Update waypoints
            if (c == null)
            {
                GameObject temp = GameObject.FindWithTag("MainCamera");
                if (temp != null) c = temp.GetComponent<Camera>();
            }
            if (bombs == null || bombs.Length == 0)
            {
                //Debug.Log("bombs found");
                bombs = GameObject.FindGameObjectsWithTag("Bomb");
            }

            // Check if the mission is over
            if (bombsRemaining == 0) {
				escapeAvailable = true;
				if (!gameOver && CheckEscape ()) {
					// If they can escape, end the game and bring up the stat board
					gameOver = true;
					EndGame();
				}
			}

            if (PhotonNetwork.IsMasterClient) {
                UpdateMissionTime();
            }

            // Check if out of time (30 mins)
        }

	}

	public bool CheckEscape() {
		if (deadCount + escaperCount == PhotonNetwork.CurrentRoom.PlayerCount) {
			return true;
		}
		return false;
	}

	void EndGame() {
		// Remove all enemies
		GameObject[] es = GameObject.FindGameObjectsWithTag("Human");
		for (int i = 0; i < es.Length; i++) {
			Destroy (es[i]);
		}
		// Don't allow player to move or shoot
		for (int i = 0; i < es.Length; i++) {
			es[i].GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().canMove = false;
			es[i].GetComponent<PlayerScript> ().canShoot = false;
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

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		ResetEscapeValues ();
		playerList = new ArrayList(GameObject.FindGameObjectsWithTag ("Player"));
	}

	/**public override void OnPlayerEnteredRoom(Player newPlayer) {
		Debug.Log (newPlayer.NickName + " has joined the room");
		//playerList = GameObject.FindGameObjectsWithTag ("Player");
	}*/

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

}
