using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Database;
using TMPro;
using Michsky.UI.Shift;

public class GameOverController : MonoBehaviourPunCallbacks {
    public Slider prevExpSlider;
    public Slider prevExpSliderVs;
    public Slider newExpSlider;
    public Slider newExpSliderVs;
    public BlurManager blurManager;
    public TextMeshProUGUI expGainedTxt;
    public TextMeshProUGUI expGainedTxtVs;
    public ModalWindowManager levelUpPopup;
    public ModalWindowManager alertPopup;
    private bool triggerAlertPopup;
    private string alertPopupMessage;

    public GameObject PlayerEntryPrefab;
    public GameObject KDPrefab;

	public Transform campaignNames;
	public Transform campaignKills;
	public Transform campaignDeaths;

	public Transform redNames;
	public Transform redKills;
	public Transform redDeaths;

	public Transform blueNames;
	public Transform blueKills;
	public Transform blueDeaths;

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
        PlayerData.playerdata.gameOverControllerRef = this;
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

    void Update() {
        if (triggerAlertPopup) {
            triggerAlertPopup = false;
            alertPopup.SetText(alertPopupMessage);
            blurManager.BlurInAnim();
            alertPopup.ModalWindowIn();
        }
    }

    void PopulateCampaignFinalStats()
    {
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
            GameObject thisPlayerEntry = GameObject.Instantiate(PlayerEntryPrefab);
            GameOverPlayerEntry g = thisPlayerEntry.GetComponent<GameOverPlayerEntry>();
            g.nametagText.text = s.name;
            g.expGainedText.text = "+"+s.expGained;
            g.gpGainedText.text = "+"+s.gpGained;
            GameObject killsEntry = GameObject.Instantiate(KDPrefab);
            killsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.kills;
            GameObject deathsEntry = GameObject.Instantiate(KDPrefab);
            deathsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.deaths;
            if (s.exp < newRank.minExp) {
                g.levelUpText.enabled = true;
                if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {
                    ToggleLevelUpPopup(newRank);
                }
            } else {
                g.levelUpText.enabled = false;
            }
            g.rankImage.texture = PlayerData.playerdata.GetRankInsigniaForRank(newRank.name);
            thisPlayerEntry.transform.SetParent(campaignNames, false);
            killsEntry.transform.SetParent(campaignKills, false);
            deathsEntry.transform.SetParent(campaignDeaths, false);
        }
    }

    void PopulateVersusFinalStats()
    {
        foreach (PlayerStat s in GameControllerScript.playerList.Values) {
            uint newExp = s.exp + s.expGained;
            Rank newRank = PlayerData.playerdata.GetRankFromExp(newExp);
            // If these are my scores, save the earned EXP and GP
            if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {

                Rank oldRank = PlayerData.playerdata.GetRankFromExp(s.exp);
                if (oldRank.minExp != newRank.minExp) {
                    prevExpSliderVs.value = 0f;
                } else {
                    prevExpSliderVs.value = (float)(s.exp - oldRank.minExp) / (float)(oldRank.maxExp - oldRank.minExp);
                }
                newExpSliderVs.value = (float)(newExp - newRank.minExp) / (float)(newRank.maxExp - newRank.minExp);
                uint toNextLevel = newRank.maxExp - newExp;
                expGainedTxtVs.text = s.expGained + " / " + toNextLevel;
                SaveEarnings(s.expGained, s.gpGained);
            }
            GameObject thisPlayerEntry = GameObject.Instantiate(PlayerEntryPrefab);
            GameOverPlayerEntry g = thisPlayerEntry.GetComponent<GameOverPlayerEntry>();
            GameObject killsEntry = GameObject.Instantiate(KDPrefab);
            GameObject deathsEntry = GameObject.Instantiate(KDPrefab);
            g.nametagText.text = s.name;
            g.expGainedText.text = "+"+s.expGained;
            g.gpGainedText.text = "+"+s.gpGained;
            if (s.exp < newRank.minExp) {
                g.levelUpText.enabled = true;
                if (s.actorId == PhotonNetwork.LocalPlayer.ActorNumber) {
                    ToggleLevelUpPopup(newRank);
                }
            } else {
                g.levelUpText.enabled = false;
            }
            g.rankImage.texture = PlayerData.playerdata.GetRankInsigniaForRank(newRank.name);
            killsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.kills;
            deathsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.deaths;
            if (s.team == 'R') {
                thisPlayerEntry.transform.SetParent(redNames, false);
                killsEntry.transform.SetParent(redKills, false);
                deathsEntry.transform.SetParent(redDeaths, false);
            } else if (s.team == 'B') {
                thisPlayerEntry.transform.SetParent(blueNames, false);
                killsEntry.transform.SetParent(blueKills, false);
                deathsEntry.transform.SetParent(blueDeaths, false);
            }
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
        blurManager.BlurInAnim();
        levelUpPopup.ModalWindowIn();
    }

    public void TriggerAlertPopup(string message) {
        alertPopupMessage = message;
        triggerAlertPopup = true;
    }

}
