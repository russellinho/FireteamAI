using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Database;

public class GameOverController : MonoBehaviourPunCallbacks {

	public GameObject namesCol;
	public GameObject killsCol;
	public GameObject deathsCol;

	public Text[] campaignNames;
	public Text[] campaignKills;
	public Text[] campaignDeaths;

    public Text[] redNames;
    public Text[] redKills;
    public Text[] redDeaths;

    public Text[] blueNames;
    public Text[] blueKills;
    public Text[] blueDeaths;
    private bool exitButtonPressed;

    public GameObject versusPanel;
    public GameObject campaignPanel;
    private bool isVersus;
	void Awake() {
		ClearPlayerData ();
        exitButtonPressed = false;
        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
            isVersus = true;
        } else if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") {
            isVersus = false;
        }
        if (isVersus)
        {
            versusPanel.SetActive(true);
            campaignPanel.SetActive(false);
        } else
        {
            campaignPanel.SetActive(true);
            versusPanel.SetActive(false);
        }
	}

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (isVersus)
        {
            PopulateVersusFinalStats();
        } else
        {
            PopulateCampaignFinalStats();
        }
    }

    void PopulateCampaignFinalStats()
    {
        int i = 0;

        foreach (string s in GameControllerScript.totalKills.Keys)
        {
            campaignNames[i].text = s;
            campaignKills[i].text = "" + GameControllerScript.totalKills[s];
            campaignDeaths[i].text = "" + GameControllerScript.totalDeaths[s];
            i++;
        }

        while (i < 8)
        {
            campaignNames[i].text = "";
            campaignKills[i].text = "";
            campaignDeaths[i].text = "";
            i++;
        }
    }

    void PopulateVersusFinalStats()
    {
        Dictionary<string, int> redTeamKills = (Dictionary<string, int>)PhotonNetwork.CurrentRoom.CustomProperties["redKills"];
        Dictionary<string, int> redTeamDeaths = (Dictionary<string, int>)PhotonNetwork.CurrentRoom.CustomProperties["redDeaths"];
        Dictionary<string, int> blueTeamKills = (Dictionary<string, int>)PhotonNetwork.CurrentRoom.CustomProperties["blueKills"];
        Dictionary<string, int> blueTeamDeaths = (Dictionary<string, int>)PhotonNetwork.CurrentRoom.CustomProperties["blueDeaths"];

        int i = 0;

        foreach (string s in redTeamKills.Keys)
        {
            redNames[i].text = s;
            redKills[i].text = "" + redTeamKills[s];
            redDeaths[i].text = "" + redTeamDeaths[s];
            i++;
        }

        while (i < 8)
        {
            redNames[i].text = "";
            redKills[i].text = "";
            redDeaths[i].text = "";
            i++;
        }

        i = 0;

        foreach (string s in blueTeamKills.Keys)
        {
            blueNames[i].text = s;
            blueKills[i].text = "" + blueTeamKills[s];
            blueDeaths[i].text = "" + blueTeamDeaths[s];
            i++;
        }

        while (i < 8)
        {
            blueNames[i].text = "";
            blueKills[i].text = "";
            blueDeaths[i].text = "";
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
