using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ScoreboardScript : MonoBehaviour {

	private int index;
    private int redIndex;
    private int blueIndex;
    private char mode;

	private IEnumerator playerIterator;
    public GameObject versusPanel;
    public GameObject campaignPanel;
	public Text[] names;
	public Text[] kills;
	public Text[] deaths;

    public Text[] redNames;
	public Text[] redKills;
	public Text[] redDeaths;

    public Text[] blueNames;
	public Text[] blueKills;
	public Text[] blueDeaths;

	// Use this for initialization
	void Awake () {
		if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
            mode = 'V';
            campaignPanel.SetActive(false);
            versusPanel.SetActive(true);
        } else if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") {
            mode = 'C';
            campaignPanel.SetActive(true);
            versusPanel.SetActive(false);
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
        if (playerIterator == null) {
            index = 0;
			playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
			return;
		}
        try
        {
            // This mean that is has reached the end
            if (!playerIterator.MoveNext())
            {
                if (index < 8)
                {
                    names[index].text = "";
                    kills[index].text = "";
                    deaths[index].text = "";
                    index++;
                }
                else
                {
                    index = 0;
                    playerIterator.Reset();
                }
            }
            else
            {
                PlayerStat curr = (PlayerStat)playerIterator.Current;
                names[index].text = curr.name;
                kills[index].text = "" + curr.kills;
                deaths[index].text = "" + curr.deaths;
                index++;
            }
        } catch (InvalidOperationException e)
        {
            playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
            index = 0;
            playerIterator.Reset();
        }
    }

    void UpdateScoreboardForVersus() {
        if (playerIterator == null) {
            redIndex = 0;
            blueIndex = 0;
			playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
			return;
		}
        try
        {
            // This mean that is has reached the end
            if (!playerIterator.MoveNext())
            {
                if (redIndex < 8)
                {
                    redNames[redIndex].text = "";
                    redKills[redIndex].text = "";
                    redDeaths[redIndex].text = "";
                    redIndex++;
                }

                if (blueIndex < 8) {
                    blueNames[blueIndex].text = "";
                    blueKills[blueIndex].text = "";
                    blueDeaths[blueIndex].text = "";
                    blueIndex++;
                }

                if (redIndex >= 8 && blueIndex >= 8) {
                    redIndex = 0;
                    blueIndex = 0;
                    playerIterator.Reset();
                }
            }
            else
            {
                PlayerStat curr = (PlayerStat)playerIterator.Current;
                if (curr.team == 'R') {
                    redNames[redIndex].text = curr.name;
                    redKills[redIndex].text = "" + curr.kills;
                    redDeaths[redIndex].text = "" + curr.deaths;
                    redIndex++;
                } else if (curr.team == 'B') {
                    blueNames[blueIndex].text = curr.name;
                    blueKills[blueIndex].text = "" + curr.kills;
                    blueDeaths[blueIndex].text = "" + curr.deaths;
                    blueIndex++;
                }
            }
        } catch (InvalidOperationException e)
        {
            playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
            redIndex = 0;
            blueIndex = 0;
            playerIterator.Reset();
        }
    }
}
