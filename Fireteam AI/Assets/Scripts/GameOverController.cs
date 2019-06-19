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
    private bool exitButtonPressed;

	void Awake() {
		ClearPlayerData ();
        exitButtonPressed = false;
	}

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
        if (!exitButtonPressed)
        {
            exitButtonPressed = true;
            PhotonNetwork.LeaveRoom();
        }
	}

	public override void OnLeftRoom() {
		ClearMatchData ();
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene (0);
	}

	void ClearPlayerData() {
		// Destroy the 
		foreach (GameObject entry in GameControllerScript.playerList.Values)
		{
			Destroy(entry.gameObject);
		}

		GameControllerScript.playerList.Clear();
	}

	void ClearMatchData() {
		GameControllerScript.totalKills.Clear ();
		GameControllerScript.totalDeaths.Clear ();
	}

}
