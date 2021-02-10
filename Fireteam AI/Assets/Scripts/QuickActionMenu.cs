using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuickActionMenu : MonoBehaviour
{
    public Canvas parentCanvas;
    public RectTransform rectCanvas;
    public RectTransform rectTransform;
    public FriendsMessenger friendsMessenger;
    private MessengerEntryScript actingOnEntry;
    public Button sendMessageBtn;
    public Button joinBtn;
    public Button removeFriendBtn;
    public Button blockBtn;
    public Button unblockBtn;
    public Button acceptFriendBtn;
    public Button declineFriendBtn;

    public void UpdatePosition() {
        Vector2 movePos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition, parentCanvas.worldCamera,
            out movePos);

        transform.position = parentCanvas.transform.TransformPoint(movePos);
        KeepPopupInScreenBounds();
    }

    void KeepPopupInScreenBounds()
    {
        var sizeDelta = rectCanvas.sizeDelta - rectTransform.sizeDelta;
        var panelPivot = rectTransform.pivot;
        var position = rectTransform.anchoredPosition;
        position.x = Mathf.Clamp(position.x, 0f, sizeDelta.x * (1.035f));
        position.y = Mathf.Clamp(position.y, -sizeDelta.y * (1.035f), 0f);
        rectTransform.anchoredPosition = position;
    }

    void Update()
    {
        HandleClick();
    }

    public void InitButtons(int status, bool online, string requestor, string requestee)
    {
        sendMessageBtn.gameObject.SetActive(false);
        joinBtn.gameObject.SetActive(false);
        removeFriendBtn.gameObject.SetActive(false);
        blockBtn.gameObject.SetActive(false);
        unblockBtn.gameObject.SetActive(false);
        acceptFriendBtn.gameObject.SetActive(false);
        declineFriendBtn.gameObject.SetActive(false);

        if (status == 0) {
            if (AuthScript.authHandler.user.UserId == requestee) {
                acceptFriendBtn.gameObject.SetActive(true);
                declineFriendBtn.gameObject.SetActive(true);
            } else {
                removeFriendBtn.gameObject.SetActive(true);
            }
        } else if (status == 1) {
            removeFriendBtn.gameObject.SetActive(true);
            blockBtn.gameObject.SetActive(true);
            if (online) {
                sendMessageBtn.gameObject.SetActive(true);
                joinBtn.gameObject.SetActive(true);
            }
        } else if (status == 2) {
            unblockBtn.gameObject.SetActive(true);
        }
    }

    public void SetActingOnEntry(MessengerEntryScript m)
    {
        actingOnEntry = m;
    }

    public MessengerEntryScript GetActingOnEntry()
    {
        return this.actingOnEntry;
    }

    public void OnSendMessageButtonClicked()
    {
        actingOnEntry.ToggleNotification(false);
        friendsMessenger.ToggleMessengerChatBox(true, actingOnEntry.GetFriendRequestId());
    }

    public void OnJoinButtonClicked()
    {
        friendsMessenger.JoinFriendGame(PlayerData.playerdata.friendsList[actingOnEntry.GetFriendRequestId()].FriendUsername);
    }

    public void OnRemoveFriendButtonClicked()
    {
        friendsMessenger.RemoveFriend(actingOnEntry.GetFriendRequestId());
    }

    public void OnAcceptFriendButtonClicked()
    {
        friendsMessenger.AcceptFriendRequest(actingOnEntry.GetFriendRequestId());
    }

    public void OnBlockButtonClicked()
    {
        friendsMessenger.BlockFriend(actingOnEntry.GetFriendRequestId());
    }

    public void OnUnblockButtonClicked()
    {
        friendsMessenger.UnblockFriend(actingOnEntry.GetFriendRequestId());
    }

    void HandleClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {
                gameObject.SetActive(false);
            }
        }
    }

    public void ToggleMenu(bool b)
    {
        gameObject.SetActive(b);
    }
}
