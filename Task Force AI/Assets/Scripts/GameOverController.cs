using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameOverController : MonoBehaviourPunCallbacks {

	public GameObject namesCol;
	public GameObject killsCol;
	public GameObject deathsCol;

	private Text[] names;
	private Text[] kills;
	private Text[] deaths;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

		names = namesCol.GetComponentsInChildren<Text> ();
		kills = killsCol.GetComponentsInChildren<Text> ();
		deaths = deathsCol.GetComponentsInChildren<Text> ();
		int i = 1;

		foreach (string s in GameControllerScript.totalKills.Keys) {
			names [i].text = s;
			kills [i].text = ""+GameControllerScript.totalKills [s];
			deaths [i].text = ""+GameControllerScript.totalDeaths [s];
			i++;
		}

		while (i < 9) {
			names [i].text = "";
			kills [i].text = "";
			deaths [i].text = "";
			i++;
		}
    }

    public void ExitButton() {
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom() {
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene (0);
	}
}
