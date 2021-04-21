using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardEntryScript : MonoBehaviour
{
    private ScoreboardScript scoreboardRef;
    public RawImage rankImg;
    private int actorNo;
    private char team;
    public TextMeshProUGUI playerNameTxt;
    public TextMeshProUGUI killsTxt;
    public TextMeshProUGUI deathsTxt;
    private float destroyCheckTimer;

    public void InitSlot(int actorNo, char team, string playerName, Texture rankTexture, ScoreboardScript scoreboardRef)
    {
        this.scoreboardRef = scoreboardRef;
        this.actorNo = actorNo;
        this.team = team;
        playerNameTxt.text = playerName;
        rankImg.texture = rankTexture;
        killsTxt.text = "0";
        deathsTxt.text = "0";
        destroyCheckTimer = 1.5f;
    }

    public void SetDeaths(int deaths) 
    {
        deathsTxt.text = ""+deaths;
    }

    public void SetKills(int kills)
    {
        killsTxt.text = ""+kills;
    }

    void Update()
    {
        destroyCheckTimer -= Time.deltaTime;
        if (destroyCheckTimer <= 0f) {
            destroyCheckTimer = 1.5f;
            DestroyCheck();
        }
    }

    void DestroyCheck()
    {
        if (!GameControllerScript.playerList.ContainsKey(actorNo)) {
            scoreboardRef.RemoveEntry(team, actorNo);
            Destroy(gameObject);
        }
    }
}
