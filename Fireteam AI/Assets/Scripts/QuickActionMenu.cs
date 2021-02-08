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

    public void InitButtons(int status)
    {
        if (status == 0) {

        } else if (status == 1) {
            // If online

            // If offline
        } else if (status == 2) {

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
        // TODO: Pop open the message panel
        // Change chat channel to you and person you clicked on
    }

    public void OnJoinButtonClicked()
    {
        // TODO: Check if the player is in a match. If so, trigger alert popup, toggle block screen, manually enter lobby, then manually join game
        // If not in a game, trigger alert popup saying so
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
}
