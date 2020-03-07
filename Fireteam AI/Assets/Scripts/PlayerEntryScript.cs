using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEntryScript : MonoBehaviour
{
    public char team;
    public TMP_Text nametag;
    public Text readyIndicator;

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
        nametag.text = tag;
    }

    public void ToggleReadyIndicator(bool b) {
        readyIndicator.enabled = b;
    }

}
