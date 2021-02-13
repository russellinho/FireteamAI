using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessengerEntryScript : MonoBehaviour
{
    private const float NOTIFICATION_FLASH_TIME = 1.2f;
    private Color GLOW_NORMAL_COLOR = new Color(99f / 255f, 198f / 255f, 255f / 255f, 50f / 255f);
    private Color BORDER_NORMAL_COLOR = new Color(99f / 255f, 198f / 255f, 255f / 255f, 255f / 255f);
    private Color GLOW_ALERT_COLOR = new Color(255f / 255f, 119f / 255f, 1f / 255f, 50f / 255f);
    private Color BORDER_ALERT_COLOR = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
    private FriendsMessenger friendsMessenger;
    private string friendRequestId;
    public TextMeshProUGUI nametag;
    public TextMeshProUGUI status;
    private bool notificationFlashOn;
    private float notificationFlashTimer;
    public Image background;

    void Update()
    {
        HandleNotificationFlash();
    }

    public void InitEntry(FriendsMessenger friendsMessenger, string friendRequestId, string username)
    {
        this.friendsMessenger = friendsMessenger;
        this.friendRequestId = friendRequestId;
        this.nametag.text = username;
        // If still in friend request phase, then put in that section
        UpdateFriendStatus();
        // Create a cached chat entry
        PlayerData.playerdata.cachedConversations.Add(friendRequestId, new CachedMessage());
    }

    public void UpdateFriendStatus()
    {
        if (friendsMessenger.quickActionMenu.GetActingOnEntry() == this) {
            friendsMessenger.quickActionMenu.gameObject.SetActive(false);
        }
        int newStatus = PlayerData.playerdata.friendsList[friendRequestId].Status;
        if (newStatus == 0) {
            UpdateSocialStatus("OFFLINE");
            transform.SetSiblingIndex(friendsMessenger.friendRequestSection.GetSiblingIndex() + 1);
            transform.SetSiblingIndex(friendsMessenger.friendRequestSection.GetSiblingIndex() + 1);
        } else if (newStatus == 1) {
            UpdateSocialStatus("OFFLINE");
            transform.SetSiblingIndex(friendsMessenger.offlineSection.GetSiblingIndex() + 1);
            transform.SetSiblingIndex(friendsMessenger.offlineSection.GetSiblingIndex() + 1);
        } else {
            UpdateSocialStatus("BLOCKED");
            transform.SetSiblingIndex(friendsMessenger.blockedSection.GetSiblingIndex() + 1);
            transform.SetSiblingIndex(friendsMessenger.blockedSection.GetSiblingIndex() + 1);
            // If blocked, then show it only if the blocker is equal to current player ID
            string blocker = PlayerData.playerdata.friendsList[friendRequestId].Blocker;
            if (blocker != AuthScript.authHandler.user.UserId) {
                ToggleVisible(false);
            }
        }
    }

    public void UpdateSocialStatus(string newStatus)
    {
        if (PlayerData.playerdata.friendsList[friendRequestId].Status == 1) {
            if (newStatus == "ONLINE" || newStatus == "IN GAME") {
                transform.SetSiblingIndex(friendsMessenger.onlineSection.GetSiblingIndex() + 1);
                transform.SetSiblingIndex(friendsMessenger.onlineSection.GetSiblingIndex() + 1);
            } else {
                transform.SetSiblingIndex(friendsMessenger.offlineSection.GetSiblingIndex() + 1);
                transform.SetSiblingIndex(friendsMessenger.offlineSection.GetSiblingIndex() + 1);
            }
        }
        this.status.text = newStatus;
    }

    public string GetFriendRequestId()
    {
        return friendRequestId;
    }

    public void ToggleVisible(bool b)
    {
        gameObject.SetActive(b);
    }

    public void OnEntryClick()
    {
        friendsMessenger.quickActionMenu.InitButtonsForMessenger(PlayerData.playerdata.friendsList[friendRequestId].Status, status.text == "ONLINE", PlayerData.playerdata.friendsList[friendRequestId].Requestor, PlayerData.playerdata.friendsList[friendRequestId].Requestee);
        friendsMessenger.quickActionMenu.SetActingOnEntry(this);
        friendsMessenger.quickActionMenu.gameObject.SetActive(true);
        // Move menu to mouse position
        friendsMessenger.quickActionMenu.UpdatePosition();
    }

    public void ToggleNotification(bool b)
    {
        if (friendsMessenger.GetChattingWithFriendRequestId() == friendRequestId) {
            b = false;
        }
        notificationFlashOn = b;
        if (b) {
            notificationFlashTimer = NOTIFICATION_FLASH_TIME;
            ToggleNotificationFlashColor(true);
        } else {
            notificationFlashTimer = 0f;
            ToggleNotificationFlashColor(false);
        }
    }

    void HandleNotificationFlash()
    {
        if (notificationFlashOn) {
            notificationFlashTimer -= Time.deltaTime;
            if (notificationFlashTimer <= 0f) {
                notificationFlashTimer = NOTIFICATION_FLASH_TIME;
                if (background.color.Equals(BORDER_ALERT_COLOR)) {
                    ToggleNotificationFlashColor(false);
                } else {
                    ToggleNotificationFlashColor(true);
                }
            }
        }
    }

    void ToggleNotificationFlashColor(bool b)
    {
        if (b) {
            background.color = GLOW_ALERT_COLOR;
        } else {
            background.color = GLOW_NORMAL_COLOR;
        }
    }

}
 