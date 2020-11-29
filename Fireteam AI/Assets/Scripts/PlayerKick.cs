using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.LobbySystemPhoton;
using TMPro;

public class PlayerKick : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI label;
    public ListPlayer listPlayer;
    public PauseMenuScript pauseMenuScript;

    public void KickPlayerFromLobby() {
        listPlayer.ConfirmKickForPlayer(player);
    }

    public void KickPlayerFromGame() {
        pauseMenuScript.KickPlayer(player);
    }

    public void Initialize(Player p) {
        this.player = p;
        this.label.text = p.NickName;
    }

}
