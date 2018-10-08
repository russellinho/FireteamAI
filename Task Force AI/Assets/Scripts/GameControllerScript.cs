using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;

public class GameControllerScript : MonoBehaviourPunCallbacks {

	public int currentMap;
	// variable for last gunshot position
	public static Vector3 lastGunshotHeardPos = Vector3.negativeInfinity;
	public static GameObject[] playerList;
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
		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			bombs = GameObject.FindGameObjectsWithTag ("Bomb");
		}

		playerList = GameObject.FindGameObjectsWithTag ("Player");
		gameOver = false;
		exitPoint = GameObject.Find ("ExitPoint");
		deadCount = 0;
		escaperCount = 0;
		escapeAvailable = false;
		pView = GetComponent<PhotonView> ();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

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
            if (bombs == null) bombs = GameObject.FindGameObjectsWithTag("Bomb");

            // Check if the mission is over
            if (bombsRemaining == 0) {
				escapeAvailable = true;
				if (!gameOver && CheckEscape ()) {
					// If they can escape, end the game and bring up the stat board
					gameOver = true;
					EndGame();
				}
			}
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

	// When someone leaves the game in the middle of an escape, reset the values to recount
	void ResetEscapeValues() {
		deadCount = 0;
		escaperCount = 0;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		ResetEscapeValues ();
		playerList = GameObject.FindGameObjectsWithTag ("Player");
	}

	public override void OnPlayerEnteredRoom(Player newPlayer) {
		Debug.Log (newPlayer.NickName + " has joined the room");
		playerList = GameObject.FindGameObjectsWithTag ("Player");
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

}
