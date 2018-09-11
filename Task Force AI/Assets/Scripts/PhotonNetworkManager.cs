using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks {

	public GameObject startButton;
	public GameObject joinButton;
	public GameObject panel;
	public GameObject mainCam;
	public GameObject playerPrefab;

	private char buttonClicked = 'a';

	void Start() {
		DontDestroyOnLoad (this);
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public override void OnConnectedToMaster() {
		if (buttonClicked == 'a') {
			PhotonNetwork.CreateRoom("bullshit");
		} else if (buttonClicked == 'b') {
			PhotonNetwork.JoinRoom ("bullshit");
		}

		startButton.SetActive(false);
		mainCam.SetActive (false);
		joinButton.SetActive (false);
	}

	public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode) {
		if (scene.name.Equals ("photon-testing-action") && PhotonNetwork.IsMasterClient) {
			PhotonNetwork.Instantiate (playerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero));
		}
	}

	public override void OnJoinedRoom() {
		//if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.LoadLevel("photon-testing-action");
			//Instantiate (playerPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
		//}
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
		buttonClicked = 'a';
		PhotonNetwork.ConnectUsingSettings ();
	}

	public void JoinMatch() {
		PhotonNetwork.GameVersion = "0.1";
		buttonClicked = 'b';
		PhotonNetwork.ConnectUsingSettings ();

	}
}
