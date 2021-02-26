using System;
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
using Hashtable = ExitGames.Client.Photon.Hashtable;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class GameOverController : MonoBehaviourPunCallbacks {
    private const string FUNCTIONS_CALL_HASH = "Au9aaFR*ajsU9UuP";
    private const string JOB_HASH = "3Nfp&HGMg8WUpnW6";
    public Slider prevExpSlider;
    public Slider prevExpSliderVs;
    public Slider newExpSlider;
    public Slider newExpSliderVs;
    public BlurManager blurManager;
    public TextMeshProUGUI expGainedTxt;
    public TextMeshProUGUI expGainedTxtVs;
    public ModalWindowManager levelUpPopup;
    public ModalWindowManager achievementPopup;
    public ModalWindowManager alertPopup;
    public Button campaignExitButton;
    public Button versusExitButton;
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
    public AudioSource rankUpSound;

    private List<object> achievementList;
    private List<object> rewardListAchievement;
    private uint newRankExp;
    private List<object> rewardListLevelUp;
    private bool renderAchievements;
    private bool renderRankUpRewards;
	void Awake() {
        campaignExitButton.interactable = false;
        versusExitButton.interactable = false;
        exitButtonPressed = false;
        RecordExpProgress(GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].expGained, GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].gpGained);
        int mapIndex = GetMapNumberForCurrentMap();
        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
            isVersus = true;
            RecordAchievementProgress('V', GetTeamTotalDeaths('V'), MissionWasStealthed('V', mapIndex), mapIndex);
            RecordMatchStats('V', GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].deaths, GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills, mapIndex);
            if (PhotonNetwork.IsMasterClient) {
                Hashtable h = new Hashtable();
                h.Add("deads", null);
                h.Add("inGame", 0);
                h.Add("redScore", 0);
                h.Add("blueScore", 0);
                h.Add("redStatus", null);
                h.Add("blueStatus", null);
                h.Add("RAssault", null);
                h.Add("BAssault", null);
                PhotonNetwork.CurrentRoom.SetCustomProperties(h);
            }
        } else if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") {
            isVersus = false;
            RecordAchievementProgress('C', GetTeamTotalDeaths('C'), MissionWasStealthed('C', mapIndex), mapIndex);
            RecordMatchStats('C', GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].deaths, GameControllerScript.playerList[PhotonNetwork.LocalPlayer.ActorNumber].kills, mapIndex);
            if (PhotonNetwork.IsMasterClient) {
                Hashtable h = new Hashtable();
                h.Add("deads", null);
                h.Add("inGame", 0);
                h.Add("Assault", null);
                PhotonNetwork.CurrentRoom.SetCustomProperties(h);
            }
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
        Hashtable h = new Hashtable();
        h.Add("readyStatus", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
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
        HandleRenderAchievements();
        HandleRenderLevelUp();
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
            }
            GameObject thisPlayerEntry = GameObject.Instantiate(PlayerEntryPrefab);
            GameOverPlayerEntry g = thisPlayerEntry.GetComponent<GameOverPlayerEntry>();
            g.nametagText.text = s.name;
            g.expGainedText.text = "+"+s.expGained + " EXP";
            g.gpGainedText.text = "+"+s.gpGained + " GP";
            GameObject killsEntry = GameObject.Instantiate(KDPrefab);
            killsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.kills;
            GameObject deathsEntry = GameObject.Instantiate(KDPrefab);
            deathsEntry.GetComponent<TextMeshProUGUI>().text = ""+s.deaths;
            if (s.exp < newRank.minExp) {
                g.levelUpText.enabled = true;
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
            }
            GameObject thisPlayerEntry = GameObject.Instantiate(PlayerEntryPrefab);
            GameOverPlayerEntry g = thisPlayerEntry.GetComponent<GameOverPlayerEntry>();
            GameObject killsEntry = GameObject.Instantiate(KDPrefab);
            GameObject deathsEntry = GameObject.Instantiate(KDPrefab);
            g.nametagText.text = s.name;
            g.expGainedText.text = "+"+s.expGained + " EXP";
            g.gpGainedText.text = "+"+s.gpGained + " GP";
            
            if (s.exp < newRank.minExp) {
                g.levelUpText.enabled = true;
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
            // PhotonNetwork.LeaveRoom();
            ClearMatchData();
            PhotonNetwork.LoadLevel("Title");
        }
	}

	// public override void OnLeftRoom() {
	// 	ClearMatchData ();
	// 	PhotonNetwork.Disconnect ();
    //     Debug.Log("two");
	// 	SceneManager.LoadScene ("Title");
	// }

	void ClearMatchData() {
		// Destroy the 
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
            if (entry.objRef != null) {
			    Destroy(entry.objRef);
            }
		}

		GameControllerScript.playerList.Clear();
	}

    void ToggleLevelUpPopup(uint newExp, List<object> rewardsList) {
        newRankExp = newExp;
        rewardListLevelUp = rewardsList;
        renderRankUpRewards = true;
    }

    void ToggleAchievementPopup(List<object> achievements, List<object> rewardsList)
    {
        achievementList = achievements;
        rewardListAchievement = rewardsList;
        renderAchievements = true;
    }

    void HandleRenderAchievements()
    {
        if (renderAchievements) {
            AchievementPopupScript achievementPopupScript = achievementPopup.GetComponent<AchievementPopupScript>();
            for (int i = 0; i < achievementList.Count; i++) {
                string thisAchievementName = achievementList[i].ToString();
                GameObject o = Instantiate(achievementPopupScript.achievementEntry);
                o.GetComponent<TextMeshProUGUI>().text = thisAchievementName;
                o.GetComponentInChildren<RawImage>().texture = PlayerData.playerdata.GetAchievementLogoForName(thisAchievementName);
                o.transform.SetParent(achievementPopupScript.achievementEntryParent);
            }
            if (rewardListAchievement.Count > 0) {
                achievementPopupScript.rewardsTxt.text = ""+Convert.ToInt32(rewardListAchievement[rewardListAchievement.Count - 1]) + " GP";
                for (int i = 0; i < rewardListAchievement.Count - 1; i++) {
                    achievementPopupScript.rewardsTxt.text += ", " + rewardListAchievement[i].ToString();
                }
            } else {
                achievementPopupScript.rewardsTxt.transform.parent.gameObject.SetActive(false);
            }
            renderAchievements = false;
            blurManager.BlurInAnim();
            achievementPopup.ModalWindowIn();
        }
    }

    void HandleRenderLevelUp()
    {
        if (renderRankUpRewards) {
            LevelUpPopupScript levelUpPopupScript = levelUpPopup.GetComponent<LevelUpPopupScript>();
            Rank r = PlayerData.playerdata.GetRankFromExp(newRankExp);
            levelUpPopupScript.rankInsigniaRef.texture = PlayerData.playerdata.GetRankInsigniaForRank(r.name);
            levelUpPopupScript.rankNameTxt.text = r.name;
            if (rewardListLevelUp.Count > 0) {
                levelUpPopupScript.rewardsTxt.text = ""+Convert.ToInt32(rewardListLevelUp[rewardListLevelUp.Count - 1]) + " GP";
                for (int i = 0; i < rewardListLevelUp.Count - 1; i++) {
                    levelUpPopupScript.rewardsTxt.text += ", " + rewardListLevelUp[i].ToString();
                }
            } else {
                levelUpPopupScript.rewardsTxt.transform.parent.gameObject.SetActive(false);
            }
            renderRankUpRewards = false;
            blurManager.BlurInAnim();
            levelUpPopup.ModalWindowIn();
            rankUpSound.Play();
        }
    }

    public void TriggerAlertPopup(string message) {
        alertPopupMessage = message;
        triggerAlertPopup = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
            PhotonNetwork.Disconnect();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LeaveRoom();
        }
    }

    void RecordAchievementProgress(char gameMode, int teamTotalDeaths, bool stealthed, int mapIndex)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["gameOverHash"] = FUNCTIONS_CALL_HASH;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["cicadaKills"] = BetaEnemyScript.NUMBER_KILLED;
        inputData["headshots"] = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["headshots"]);
        inputData["level"] = mapIndex;
        inputData["stealthed"] = stealthed;
        inputData["time"] = (int)GameControllerScript.missionTime;
        inputData["teamDeaths"] = teamTotalDeaths;
        inputData["gameMode"] = ""+gameMode;
        inputData["win"] = SceneManager.GetActiveScene().name == "GameOverSuccess" ? true : false;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("saveAchievementProgress");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Achievement progress update successful.");
                    List<object> achievementsUnlocked = (List<object>)results["achievementsReached"];
                    if (achievementsUnlocked.Count > 0) {
                        try {
                            ToggleAchievementPopup(achievementsUnlocked, (List<object>)results["rewards"]);
                        } catch (Exception e) {
                            Debug.LogError(e.Message);
                        }
                    }
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
            ClearAchievementData();
        });
    }

    void RecordMatchStats(char gameMode, int deaths, int kills, int mapIndex)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["gameOverHash"] = FUNCTIONS_CALL_HASH;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["level"] = mapIndex;
        inputData["kills"] = kills;
        inputData["deaths"] = deaths;
        inputData["time"] = (int)GameControllerScript.missionTime;
        inputData["gameMode"] = ""+gameMode;
        inputData["status"] = (SceneManager.GetActiveScene().name == "GameOverSuccess" ? "W" : "L");
        inputData["validTime"] = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["starter"]);

        DAOScript.dao.functions.GetHttpsCallable("recordMatchStats").CallAsync(inputData);
        // DAOScript.dao.functions.GetHttpsCallable("recordMatchStats").CallAsync(inputData).ContinueWith((task) => {
        //     UpdateLeaderboards();
        // });
    }

    void UpdateLeaderboards()
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = JOB_HASH;

        DAOScript.dao.functions.GetHttpsCallable("updateLeaderboards").CallAsync(inputData);
    }

    void RecordExpProgress(uint exp, uint gp)
    {
        Hashtable h = new Hashtable();
        h.Add("exp", (int)PlayerData.playerdata.info.Exp + (int)exp);
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["gameOverHash"] = FUNCTIONS_CALL_HASH;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["exp"] = exp;
        inputData["gp"] = gp;

        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("progressExpAndGp");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Level progress update successful with no level up.");
                } else if (results["status"].ToString() == "201") {
                    Debug.Log("Level progress update successful with a level up.");
                    ToggleLevelUpPopup(Convert.ToUInt32(results["newExp"]), (List<object>)results["rewards"]);
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
            campaignExitButton.interactable = true;
            versusExitButton.interactable = true;
        });
    }

    int GetMapNumberForCurrentMap()
    {
        string map = (string)PhotonNetwork.CurrentRoom.CustomProperties["mapName"];
        int mapNumber = 0;
        switch (map) {
            case "The Badlands: Act I":
                mapNumber = 0;
                break;
            case "The Badlands: Act II":
                mapNumber = 1;
                break;
            default:
                mapNumber = 0;
                break;
        }
        return mapNumber;
    }

    void ClearAchievementData()
    {
        BetaEnemyScript.NUMBER_KILLED = 0;
        GameControllerScript.missionTime = 0f;
        Hashtable h = new Hashtable();
        h.Add("headshots", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
    }

    int GetTeamTotalDeaths(char mode)
    {
        int total = 0;

        if (mode == 'C') {
            foreach (PlayerStat s in GameControllerScript.playerList.Values) {
                total += s.deaths;
            }
        } else if (mode == 'V') {
            string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
            if (myTeam == "red") {
                foreach (PlayerStat s in GameControllerScript.playerList.Values) {
                    if (s.team == 'R') {
                        total += s.deaths;
                    }
                }
            } else {
                foreach (PlayerStat s in GameControllerScript.playerList.Values) {
                    if (s.team == 'B') {
                        total += s.deaths;
                    }
                }
            }
        }

        return total;
    }

    bool MissionWasStealthed(char mode, int mapIndex)
    {
        if (mapIndex == 1) {
            return false;
        }
        if (mode == 'C') {
            return Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["Assault"]) == 0;
        }

        string myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        char c = 'B';
        if (myTeam == "red") {
            c = 'R';
        }

        return Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties[c + "Assault"]) == 0;
    }

}
