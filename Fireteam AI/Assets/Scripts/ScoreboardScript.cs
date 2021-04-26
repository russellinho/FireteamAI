using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ScoreboardScript : MonoBehaviour {

    private const float SCOREBOARD_UPDATE_DELAY = 0.25f;
    private char mode;
    // Amount of time remaining before next scoreboard update
    private float scoreboardUpdateTimer;

	private IEnumerator playerIterator;
    public GameObject versusPanelRed;
    public GameObject versusPanelBlue;
    public GameObject campaignPanel;
    public Transform campaignParent;
    public Transform versusRedParent;
    public Transform versusBlueParent;
	private Dictionary<int, ScoreboardEntryScript> redEntries;
    private Dictionary<int, ScoreboardEntryScript> blueEntries;
    public GameObject scoreboardEntryPrefab;
    public GameObject scoreboardEntryRedPrefab;
    public GameObject scoreboardEntryBluePrefab;

	// Use this for initialization
	void Awake () {
        scoreboardUpdateTimer = SCOREBOARD_UPDATE_DELAY;
		if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
            redEntries = new Dictionary<int, ScoreboardEntryScript>();
            blueEntries = new Dictionary<int, ScoreboardEntryScript>();
            mode = 'V';
            campaignPanel.SetActive(false);
            versusPanelRed.SetActive(true);
            versusPanelBlue.SetActive(true);
        } else if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") {
            redEntries = new Dictionary<int, ScoreboardEntryScript>();
            mode = 'C';
            campaignPanel.SetActive(true);
            versusPanelRed.SetActive(false);
            versusPanelBlue.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (mode == 'C') {
            UpdateScoreboardForCampaign();
        } else if (mode == 'V') {
            UpdateScoreboardForVersus();
        }
	}

    void UpdateScoreboardForCampaign() {
        if (scoreboardUpdateTimer <= 0f) {
            scoreboardUpdateTimer = SCOREBOARD_UPDATE_DELAY;
            if (playerIterator == null) {
                playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
                return;
            }
            try
            {
                // This mean that is has reached the end
                if (!playerIterator.MoveNext())
                {
                    playerIterator.Reset();
                }
                else
                {
                    PlayerStat curr = (PlayerStat)playerIterator.Current;
                    if (!redEntries.ContainsKey(curr.actorId)) {
                        GameObject o = Instantiate(scoreboardEntryPrefab, campaignParent);
                        ScoreboardEntryScript s = o.GetComponent<ScoreboardEntryScript>();
                        Rank rank = PlayerData.playerdata.GetRankFromExp(curr.exp);
                        s.InitSlot(curr.actorId, curr.team, curr.name, PlayerData.playerdata.GetRankInsigniaForRank(rank.name), this, PlayerData.playerdata.IsGameMaster(curr.exp));
                        redEntries.Add(curr.actorId, s);
                        o.transform.localPosition = Vector3.zero;
                        o.transform.localRotation = Quaternion.identity;
                    } else {
                        ScoreboardEntryScript s = redEntries[curr.actorId];
                        s.SetKills(curr.kills);
                        s.SetDeaths(curr.deaths);
                    }
                }
            } catch (InvalidOperationException e)
            {
                playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
                playerIterator.Reset();
            }
        } else {
            scoreboardUpdateTimer -= Time.deltaTime;
        }
    }

    void UpdateScoreboardForVersus() {
        if (scoreboardUpdateTimer <= 0f) {
            scoreboardUpdateTimer = SCOREBOARD_UPDATE_DELAY;
            if (playerIterator == null) {
                playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
                return;
            }
            try
            {
                // This mean that is has reached the end
                if (!playerIterator.MoveNext())
                {
                    playerIterator.Reset();
                }
                else
                {
                    PlayerStat curr = (PlayerStat)playerIterator.Current;
                    if (curr.team == 'R') {
                        if (!redEntries.ContainsKey(curr.actorId)) {
                            GameObject o = Instantiate(scoreboardEntryRedPrefab, versusRedParent);
                            ScoreboardEntryScript s = o.GetComponent<ScoreboardEntryScript>();
                            Rank rank = PlayerData.playerdata.GetRankFromExp(curr.exp);
                            s.InitSlot(curr.actorId, curr.team, curr.name, PlayerData.playerdata.GetRankInsigniaForRank(rank.name), this, PlayerData.playerdata.IsGameMaster(curr.exp));
                            redEntries.Add(curr.actorId, s);
                            o.transform.localPosition = Vector3.zero;
                            o.transform.localRotation = Quaternion.identity;
                        } else {
                            ScoreboardEntryScript s = redEntries[curr.actorId];
                            s.SetKills(curr.kills);
                            s.SetDeaths(curr.deaths);
                        }
                    } else if (curr.team == 'B') {
                        if (!blueEntries.ContainsKey(curr.actorId)) {
                            GameObject o = Instantiate(scoreboardEntryBluePrefab, versusBlueParent);
                            ScoreboardEntryScript s = o.GetComponent<ScoreboardEntryScript>();
                            Rank rank = PlayerData.playerdata.GetRankFromExp(curr.exp);
                            s.InitSlot(curr.actorId, curr.team, curr.name, PlayerData.playerdata.GetRankInsigniaForRank(rank.name), this, PlayerData.playerdata.IsGameMaster(curr.exp));
                            blueEntries.Add(curr.actorId, s);
                            o.transform.localPosition = Vector3.zero;
                            o.transform.localRotation = Quaternion.identity;
                        } else {
                            ScoreboardEntryScript s = blueEntries[curr.actorId];
                            s.SetKills(curr.kills);
                            s.SetDeaths(curr.deaths);
                        }
                    }
                }
            } catch (InvalidOperationException e)
            {
                playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
                playerIterator.Reset();
            }
        } else {
            scoreboardUpdateTimer -= Time.deltaTime;
        }
    }

    public void RemoveEntry(char team, int actorNo)
    {
        if (team == 'R' || team == 'N') {
            redEntries.Remove(actorNo);
        } else if (team == 'B') {
            blueEntries.Remove(actorNo);
        }
    }
}
