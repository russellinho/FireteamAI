using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks {

	public GameObject startButton;
	public GameObject panel;
	public GameObject mainCam;
	public GameObject playerPrefab;

	void Start() {
		if (SceneManager.GetActiveScene ().name.Equals ("photon-testing-action")) {
			PhotonNetwork.Instantiate (playerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero));
		}
	}

	public override void OnConnectedToMaster() {
		Debug.Log ("Connected to master");
		PhotonNetwork.CreateRoom("bullshit");
		//PhotonNetwork.ConnectUsingSettings ();
		//Debug.Log ("Connecting to photon...");
		startButton.SetActive(false);
		mainCam.SetActive (false);
		//panel.SetActive (true);
	}

	public override void OnJoinedRoom() {
		if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {

			PhotonNetwork.LoadLevel("photon-testing-action");
			Instantiate (playerPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
		}
	}

/**	private void OnConnectedToPhoton() {
		Debug.Log ("Connected to master");
		PhotonNetwork.JoinLobby (TypedLobby.Default);
	}

	private void OnJoinedLobby() {
		Debug.Log ("Lobby joined");
	}

	private void OnDisconnectedFromPhoton() {
		Debug.Log ("Disconnected from photon");
	}*/

	public void CreateMatch() {
		PhotonNetwork.GameVersion = "0.1";
		PhotonNetwork.ConnectUsingSettings();
	}
}
