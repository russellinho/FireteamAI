using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoiceChatEntryScript : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public int actorNo;

    public void SetPlayerNameEntry(int actorNo, string s)
    {
        this.actorNo = actorNo;
        playerName.text = s;
    }
}
