using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEntryScript : MonoBehaviour
{
    public char team;
    public string nickname;
    public TMP_Text nametag;
    public GameObject readyIndicator;
    public Image readyMarker;
    public bool isReady;

    public void SetTeam(char team) {
        this.team = team;
        SetNameColor(team == 'R' ? Color.red : Color.blue);
    }

    public void ChangeTeam() {
        SetTeam(this.team == 'R' ? 'B' : 'R');
    }

    public void SetNameColor(Color c) {
        nametag.color = c;
    }

    public void SetNameTag(string tag) {
        Debug.Log("Setting name tag: " + tag);
        nickname = tag;
        nametag.text = tag;
    }

    public void ToggleReadyIndicator(bool b) {
        readyIndicator.SetActive(b);
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
        if (isReady)
        {
            readyMarker.color = Color.green;
        } else
        {
            readyMarker.color = Color.red;
        }
    }

}
