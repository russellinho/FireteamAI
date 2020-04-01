using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Database;

public class GameOverController : MonoBehaviourPunCallbacks {
    public Slider prevExpSlider;
    public Slider newExpSlider;
    public Text expGainedTxt;

	public GameObject namesCol;
	public GameObject killsCol;
	public GameObject deathsCol;

	public Text[] campaignNames;
	public Text[] campaignKills;
	public Text[] campaignDeaths;
    public Text[] campaignExp;
    public Text[] campaignGp;
    public Text[] campaignLevelUp;
    public RawImage[] campaignRanks;

    public Text[] redNames;
    public Text[] redKills;
    public Text[] redDeaths;
    public Text[] redExp;
    public Text[] redGp;
    public Text[] redLevelUp;
    public RawImage[] redRanks;

    public Text[] blueNames;
    public Text[] blueKills;
    public Text[] blueDeaths;
    public Text[] blueExp;
    public Text[] blueGp;
    public Text[] blueLevelUp;
    public RawImage[] blueRanks;
    private bool exitButtonPressed;

    public GameObject versusPanel;
    public GameObject campaignPanel;
    private bool isVersus;
	void Awake() {
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

        foreach (PlayerStat s in GameControllerScript.playerList.Values)
        {
            campaignNames[i].text = s.name;
            campaignKills[i].text = ""+s.kills;
            campaignDeaths[i].text = ""+s.deaths;
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
        int redI = 0;
        int blueI = 0;
        
        foreach (PlayerStat s in GameControllerScript.playerList.Values) {
            Debug.Log(s.name + " " + s.team);
            if (s.team == 'R') {
                redNames[redI].text = s.name;
                redKills[redI].text = ""+s.kills;
                redDeaths[redI].text = ""+s.deaths;
                redI++;
            } else if (s.team == 'B') {
                blueNames[blueI].text = s.name;
                blueKills[blueI].text = ""+s.kills;
                blueDeaths[blueI].text = ""+s.deaths;
                blueI++;
            }
        }

        while (redI < 8)
        {
            redNames[redI].text = "";
            redKills[redI].text = "";
            redDeaths[redI].text = "";
            redI++;
        }

        while (blueI < 8)
        {
            blueNames[blueI].text = "";
            blueKills[blueI].text = "";
            blueDeaths[blueI].text = "";
            blueI++;
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
		SceneManager.LoadScene ("Title");
	}

	void ClearMatchData() {
		// Destroy the 
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			Destroy(entry.objRef);
		}

		GameControllerScript.playerList.Clear();
	}

}
