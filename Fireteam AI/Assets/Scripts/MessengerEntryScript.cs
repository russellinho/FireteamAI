using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessengerEntryScript : MonoBehaviour
{
    public FriendsMessenger friendsMessenger;
    private string friendRequestId;
    public TextMeshProUGUI nametag;
    public TextMeshProUGUI status;

    public void InitEntry(FriendsMessenger friendsMessenger, string friendRequestId, string username)
    {
        this.friendsMessenger = friendsMessenger;
        this.friendRequestId = friendRequestId;
        this.nametag.text = username;
        // If still in friend request phase, then put in that section
        UpdateFriendStatus();
    }

    public void UpdateFriendStatus()
    {
        int newStatus = PlayerData.playerdata.friendsList[friendRequestId].Status;
        if (newStatus == 0) {
            this.status.text = "Offline";
            transform.SetSiblingIndex(friendsMessenger.friendRequestSection.GetSiblingIndex() + 1);
        } else if (newStatus == 1) {
            transform.SetSiblingIndex(friendsMessenger.offlineSection.GetSiblingIndex() + 1);
            UpdateSocialStatus();
        } else {
            this.status.text = "Offline";
            transform.SetSiblingIndex(friendsMessenger.blockedSection.GetSiblingIndex() + 1);
            // If blocked, then show it only if the blocker is equal to current player ID
            string blocker = PlayerData.playerdata.friendsList[friendRequestId].Blocker;
            if (blocker != AuthScript.authHandler.user.UserId) {
                ToggleVisible(false);
            }
        }
    }

    public void UpdateSocialStatus()
    {
        // TODO: Fill out status based on online, offline, or in-game
        this.status.text = "TODO";
    }

    public string GetFriendRequestId()
    {
        return friendRequestId;
    }

    public void ToggleVisible(bool b)
    {
        gameObject.SetActive(b);
    }

}
