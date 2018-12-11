using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameOverController : MonoBehaviourPunCallbacks {

	public void ExitButton() {
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom() {
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene (0);
	}
}
