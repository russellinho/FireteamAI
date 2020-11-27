using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEntryPrefab : MonoBehaviour
{
    private Texture rankLogo;
    private string nametag;
    public int actorId;

    public GameObject campaignEntry;
    public RawImage campaignLogo;
    public TextMeshProUGUI campaignNameTag;
    public GameObject campaignReady;
    public TextMeshProUGUI campaignReadyText;

    public GameObject redEntry;
    public RawImage redLogo;
    public TextMeshProUGUI redNameTag;
    public GameObject redReady;
    public TextMeshProUGUI redReadyText;

    public GameObject blueEntry;
    public RawImage blueLogo;
    public TextMeshProUGUI blueNameTag;
    public GameObject blueReady;
    public TextMeshProUGUI blueReadyText;

    public void CreateEntry(string nametag, string rank, int actorId, char team) {
        SetNameTag(nametag);
        SetRank(rank);
        this.actorId = actorId;
        InitializeData();
        ToggleEntryByTeam(team);
    }

    void InitializeData() {
        campaignLogo.texture = rankLogo;
        redLogo.texture = rankLogo;
        blueLogo.texture = rankLogo;

        campaignNameTag.text = nametag;
        redNameTag.text = nametag;
        blueNameTag.text = nametag;

        campaignReady.SetActive(false);
        redReady.SetActive(false);
        blueReady.SetActive(false);
    }

    public void ToggleEntryByTeam(char team) {
        campaignEntry.SetActive(false);
        redEntry.SetActive(false);
        blueEntry.SetActive(false);
        if (team == 'C') {
            campaignEntry.SetActive(true);
        } else if (team == 'R') {
            redEntry.SetActive(true);
        } else if (team == 'B') {
            blueEntry.SetActive(true);
        }
    }

    public void SetReady(bool b) {
        if (campaignEntry.activeInHierarchy) {
            campaignReady.SetActive(b);
        } else if (redEntry.activeInHierarchy) {
            redReady.SetActive(b);
        } else if (blueEntry.activeInHierarchy) {
            blueReady.SetActive(b);
        }
    }

    public bool IsReady() {
        if (campaignEntry.activeInHierarchy) {
            return campaignReady.activeInHierarchy;
        } else if (redEntry.activeInHierarchy) {
            return redReady.activeInHierarchy;
        } else if (blueEntry.activeInHierarchy) {
            return blueReady.activeInHierarchy;
        }
        return false;
    }

    void SetNameTag(string nametag) {
        this.nametag = nametag;
    }

    public void SetRank(string rank) {
        this.rankLogo = PlayerData.playerdata.GetRankInsigniaForRank(rank);
    }

    public void SetTeam(char team) {
        ToggleEntryByTeam(team);
    }

    public void SetActorId(int id)
    {
        actorId = id;
    }

    public char GetTeam() {
        if (campaignEntry.activeInHierarchy) {
            return 'C';
        } else if (redEntry.activeInHierarchy) {
            return 'R';
        } else if (blueEntry.activeInHierarchy) {
            return 'B';
        }
        return 'C';
    }

    public void SetReadyText(char s) {
        if (s == 'i') {
            if (campaignEntry.activeInHierarchy) {
                campaignReadyText.text = "In game";
            } else if (redEntry.activeInHierarchy) {
                redReadyText.text = "In game";
            } else if (blueEntry.activeInHierarchy) {
                blueReadyText.text = "In game";
            }
        } else if (s == 'r') {
            if (campaignEntry.activeInHierarchy) {
                campaignReadyText.text = "Ready";
            } else if (redEntry.activeInHierarchy) {
                redReadyText.text = "Ready";
            } else if (blueEntry.activeInHierarchy) {
                blueReadyText.text = "Ready";
            }
        }
    }
}
