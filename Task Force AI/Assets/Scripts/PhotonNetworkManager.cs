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
	//public GameControllerTestScript gameController;

	public static GameObject localPlayer;

	private char buttonClicked = 'a';

	public static PhotonNetworkManager instance;

	void Awake()
	{
		if(instance != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		instance = this;
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	void Start() {
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
		/**if (scene.name.Equals ("photon-testing-action") && PhotonNetwork.LocalPlayer.IsLocal) {
			PhotonNetwork.Instantiate (playerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero));
		}*/
		if(!PhotonNetwork.InRoom) return;

		localPlayer = PhotonNetwork.Instantiate(
			"PlayerPho",
			new Vector3(-27f,0.4f,-27f),
			Quaternion.identity, 0);

		//gameController = GameObject.Find ("GameControllerTest").GetComponent<GameControllerTestScript>();
	}

	public override void OnJoinedRoom() {
        //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
		//if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LoadLevel("BetaLevelNetwork");
			//Instantiate (playerPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
		}
		//}
	}

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
