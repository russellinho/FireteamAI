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
    private string versusId;

	void Awake() {
		ClearPlayerData ();
        exitButtonPressed = false;
        versusId = (string)PhotonNetwork.CurrentRoom.CustomProperties["versusId"];
        if (string.IsNullOrEmpty(versusId))
        {
            campaignPanel.SetActive(true);
            versusPanel.SetActive(false);
        } else
        {
            versusPanel.SetActive(true);
            campaignPanel.SetActive(false);
        }
	}

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (string.IsNullOrEmpty(versusId))
        {
            PopulateCampaignFinalStats();
        } else
        {
            PopulateVersusFinalStats();
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
        int i = 0;
        DAOScript.dao.dbRef.Child("fteam_ai_matches").Child(versusId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                IEnumerator<DataSnapshot> redTeamStats = snapshot.Child("redTeamPlayers").Children.GetEnumerator();
                IEnumerator<DataSnapshot> blueTeamStats = snapshot.Child("blueTeamPlayers").Children.GetEnumerator();

                while (redTeamStats.MoveNext())
                {
                    string thisRedTeamName = redTeamStats.Current.Key;
                    DataSnapshot thisRedPlayerData = redTeamStats.Current;
                    redNames[i].text = thisRedTeamName;
                    redKills[i].text = thisRedPlayerData.Child("kills").Value.ToString();
                    redDeaths[i].text = thisRedPlayerData.Child("deaths").Value.ToString();
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

                while (blueTeamStats.MoveNext())
                {
                    string thisBlueTeamName = blueTeamStats.Current.Key;
                    DataSnapshot thisBluePlayerData = blueTeamStats.Current;
                    blueNames[i].text = thisBlueTeamName;
                    blueKills[i].text = thisBluePlayerData.Child("kills").Value.ToString();
                    blueDeaths[i].text = thisBluePlayerData.Child("deaths").Value.ToString();
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
        });
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
