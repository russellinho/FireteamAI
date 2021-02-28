using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuickActionMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Canvas parentCanvas;
    public RectTransform rectCanvas;
    public RectTransform rectTransform;
    public FriendsMessenger friendsMessenger;
    public GiftInbox giftInbox;
    private MessengerEntryScript actingOnEntry;
    private GiftEntryScript actingOnEntryGift;
    public Button sendMessageBtn;
    public Button joinBtn;
    public Button removeFriendBtn;
    public Button blockBtn;
    public Button unblockBtn;
    public Button acceptFriendBtn;
    public Button declineFriendBtn;
    public Button acceptGiftBtn;
    public Button sellGiftBtn;
    private bool pointerOn;

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

    public void InitButtonsForMessenger(int status, bool online, string requestor, string requestee)
    {
        sellGiftBtn.gameObject.SetActive(false);
        acceptGiftBtn.gameObject.SetActive(false);
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

    public void InitButtonsForGiftInbox()
    {
        sellGiftBtn.gameObject.SetActive(true);
        acceptGiftBtn.gameObject.SetActive(true);
        sendMessageBtn.gameObject.SetActive(false);
        joinBtn.gameObject.SetActive(false);
        removeFriendBtn.gameObject.SetActive(false);
        blockBtn.gameObject.SetActive(false);
        unblockBtn.gameObject.SetActive(false);
        acceptFriendBtn.gameObject.SetActive(false);
        declineFriendBtn.gameObject.SetActive(false);
    }

    public void SetActingOnEntry(MessengerEntryScript m)
    {
        actingOnEntry = m;
        actingOnEntryGift = null;
    }

    public void SetActingOnEntry(GiftEntryScript g)
    {
        actingOnEntry = null;
        actingOnEntryGift = g;
    }

    public MessengerEntryScript GetActingOnEntry()
    {
        return this.actingOnEntry;
    }

    public void OnSendMessageButtonClicked()
    {
        actingOnEntry?.ToggleNotification(false);
        friendsMessenger.ToggleMessengerChatBox(true, actingOnEntry?.GetFriendRequestId());
        actingOnEntry?.ToggleNotification(false);
    }

    public void OnJoinButtonClicked()
    {
        if (PlayerData.playerdata.friendsList.ContainsKey(actingOnEntry?.GetFriendRequestId())) {
            friendsMessenger.JoinFriendGame(PlayerData.playerdata.friendsList[actingOnEntry?.GetFriendRequestId()].FriendUsername);
        }
    }

    public void OnRemoveFriendButtonClicked()
    {
        if (actingOnEntry != null) friendsMessenger.RemoveFriend(actingOnEntry.GetFriendRequestId());
    }

    public void OnAcceptFriendButtonClicked()
    {
        if (actingOnEntry != null) friendsMessenger.AcceptFriendRequest(actingOnEntry.GetFriendRequestId());
    }

    public void OnBlockButtonClicked()
    {
        if (actingOnEntry != null) friendsMessenger.BlockFriend(actingOnEntry.GetFriendRequestId());
    }

    public void OnUnblockButtonClicked()
    {
        if (actingOnEntry != null) friendsMessenger.UnblockFriend(actingOnEntry.GetFriendRequestId());
    }

    public void OnAcceptGiftButtonClicked()
    {
        giftInbox.AcceptGift(actingOnEntryGift.GetGiftId());
    }

    public void OnSellGiftButtonClicked()
    {
        GiftData g = PlayerData.playerdata.giftList[actingOnEntryGift.GetGiftId()];
        int cost = 0;
        if (g.Category == "Character") {
            Character c = InventoryScript.itemData.characterCatalog[g.ItemName];
            cost = c.gpPrice == 0 ? c.kashPrice : c.gpPrice;
        } else if (g.Category == "Armor") {
            Armor a = InventoryScript.itemData.armorCatalog[g.ItemName];
            cost = a.gpPrice == 0 ? a.kashPrice : a.gpPrice;
        } else if (g.Category == "Weapon") {
            Weapon w = InventoryScript.itemData.weaponCatalog[g.ItemName];
            cost = w.gpPrice == 0 ? w.kashPrice : w.gpPrice;
        } else if (g.Category == "Mod") {
            Mod m = InventoryScript.itemData.modCatalog[g.ItemName];
            cost = m.gpPrice == 0 ? m.kashPrice : m.gpPrice;
        } else {
            Equipment e = InventoryScript.itemData.equipmentCatalog[g.ItemName];
            cost = e.gpPrice == 0 ? e.kashPrice : e.gpPrice;
        }
        giftInbox.titleController.PrepareGiftSale((int)g.Duration, cost, g.ItemName, actingOnEntryGift.GetGiftId());
    }

    void HandleClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (!pointerOn) {
                actingOnEntry = null;
                actingOnEntryGift = null;
                gameObject.SetActive(false);
            }
        }
    }

    public void ToggleMenu(bool b)
    {
        gameObject.SetActive(b);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOn = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOn = false;
    }
}
