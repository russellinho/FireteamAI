using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerScript : MonoBehaviour
{
    public TextMeshProUGUI nametag;
    public RawImage rankInsignia;

    public void InitEntry(string playername, uint exp)
    {
        if (PlayerData.playerdata.IsGameMaster(exp)) {
            nametag.text = "<color=#ffff00ff>[GM]" + playername + "</color>";
        } else {
            nametag.text = playername;
        }
        UpdateRank(exp);
    }

    public void UpdateRank(uint exp)
    {
        rankInsignia.texture = PlayerData.playerdata.GetRankInsigniaForRank(PlayerData.playerdata.GetRankFromExp(exp).name);
    }
}
