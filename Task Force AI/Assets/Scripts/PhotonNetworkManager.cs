using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviour {

	void Start() {
		PhotonNetwork.ConnectUsingSettings ();
		Debug.Log ("Connecting to photon...");
	}

	private void OnConnectedToMaster() {
		PhotonNetwork.JoinLobby (TypedLobby.Default);
	}

	private void OnJoinedLobby() {
		Debug.Log ("Lobby joined");
	}

	private void OnDisconnectedFromPhoton() {
		Debug.Log ("Disconnected from photon");
	}
}
