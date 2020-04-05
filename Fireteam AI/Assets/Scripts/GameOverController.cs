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
    public GameObject levelUpPopup;

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
            uint newExp = s.exp + s.expGained;
            Rank newRank = PlayerData.playerdata.GetRankFromExp(newExp);
            // If these are my scores, save the earned EXP and GP
            if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {

                Rank oldRank = PlayerData.playerdata.GetRankFromExp(s.exp);
                if (oldRank.minExp != newRank.minExp) {
                    prevExpSlider.value = 0f;
                } else {
                    prevExpSlider.value = (float)(s.exp - oldRank.minExp) / (float)(oldRank.maxExp - oldRank.minExp);
                }
                newExpSlider.value = (float)(newExp - newRank.minExp) / (float)(newRank.maxExp - newRank.minExp);
                uint toNextLevel = newRank.maxExp - newExp;
                expGainedTxt.text = s.expGained + " / " + toNextLevel;
                SaveEarnings(s.expGained, s.gpGained);
            }
            campaignNames[i].text = s.name;
            campaignKills[i].text = ""+s.kills;
            campaignDeaths[i].text = ""+s.deaths;
            campaignExp[i].text = "+"+s.expGained;
            campaignGp[i].text = "+"+s.gpGained;
            if (s.exp < newRank.minExp) {
                campaignLevelUp[i].enabled = true;
                if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {
                    ToggleLevelUpPopup(newRank);
                }
            } else {
                campaignLevelUp[i].enabled = false;
            }
            campaignRanks[i].texture = PlayerData.playerdata.GetRankInsigniaForRank(newRank.name);
            i++;
        }

        while (i < 8)
        {
            campaignNames[i].text = "";
            campaignKills[i].text = "";
            campaignDeaths[i].text = "";
            campaignExp[i].text = "";
            campaignGp[i].text = "";
            campaignLevelUp[i].enabled = false;
            campaignRanks[i].enabled = false;
            i++;
        }
    }

    void PopulateVersusFinalStats()
    {
        int redI = 0;
        int blueI = 0;
        
        foreach (PlayerStat s in GameControllerScript.playerList.Values) {
            uint newExp = s.exp + s.expGained;
            Rank newRank = PlayerData.playerdata.GetRankFromExp(newExp);
            // If these are my scores, save the earned EXP and GP
            if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {

                Rank oldRank = PlayerData.playerdata.GetRankFromExp(s.exp);
                if (oldRank.minExp != newRank.minExp) {
                    prevExpSlider.value = 0f;
                } else {
                    prevExpSlider.value = (float)(s.exp - oldRank.minExp) / (float)(oldRank.maxExp - oldRank.minExp);
                }
                newExpSlider.value = (float)(newExp - newRank.minExp) / (float)(newRank.maxExp - newRank.minExp);
                uint toNextLevel = newRank.maxExp - newExp;
                expGainedTxt.text = s.expGained + " / " + toNextLevel;
                SaveEarnings(s.expGained, s.gpGained);
            }
            if (s.team == 'R') {
                redNames[redI].text = s.name;
                redKills[redI].text = ""+s.kills;
                redDeaths[redI].text = ""+s.deaths;
                redExp[redI].text = "+"+s.expGained;
                redGp[redI].text = "+"+s.gpGained;
                if (s.exp < newRank.minExp) {
                    redLevelUp[redI].enabled = true;
                    if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {
                        ToggleLevelUpPopup(newRank);
                    }
                } else {
                    redLevelUp[redI].enabled = false;
                }
                redRanks[redI].texture = PlayerData.playerdata.GetRankInsigniaForRank(newRank.name);
                redI++;
            } else if (s.team == 'B') {
                blueNames[blueI].text = s.name;
                blueKills[blueI].text = ""+s.kills;
                blueDeaths[blueI].text = ""+s.deaths;
                blueExp[blueI].text = "+"+s.expGained;
                blueGp[blueI].text = "+"+s.gpGained;
                if (s.exp < newRank.minExp) {
                    blueLevelUp[blueI].enabled = true;
                } else {
                    blueLevelUp[blueI].enabled = false;
                }
                blueRanks[blueI].texture = PlayerData.playerdata.GetRankInsigniaForRank(newRank.name);
                blueI++;
            }
        }

        while (redI < 8)
        {
            redNames[redI].text = "";
            redKills[redI].text = "";
            redDeaths[redI].text = "";
            redExp[redI].text = "";
            redGp[redI].text = "";
            redLevelUp[redI].enabled = false;
            redRanks[redI].enabled = false;
            redI++;
        }

        while (blueI < 8)
        {
            blueNames[blueI].text = "";
            blueKills[blueI].text = "";
            blueDeaths[blueI].text = "";
            blueExp[blueI].text = "";
            blueGp[blueI].text = "";
            blueLevelUp[blueI].enabled = false;
            blueRanks[blueI].enabled = false;
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

    void SaveEarnings(uint expEarned, uint gpEarned) {        
        // Save it to player
        PlayerData.playerdata.AddExpAndGpToPlayer(expEarned, gpEarned);
    }

    void ToggleLevelUpPopup(Rank r) {
        levelUpPopup.GetComponent<LevelUpPopupScript>().rankInsigniaRef.texture = PlayerData.playerdata.GetRankInsigniaForRank(r.name);
        levelUpPopup.GetComponent<LevelUpPopupScript>().rankNameTxt.text = r.name;
        levelUpPopup.SetActive(true);
    }

    public void CloseLevelUpPopup() {
        levelUpPopup.SetActive(false);
    }

}
