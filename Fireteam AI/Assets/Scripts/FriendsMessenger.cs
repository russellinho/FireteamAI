using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class FriendsMessenger : MonoBehaviour
{
    private const float NOTIFICATION_FLASH_TIME = 0.6f;
    private const int MAX_FRIENDS = 25;
    private Color GLOW_NORMAL_COLOR = new Color(99f / 255f, 198f / 255f, 255f / 255f, 50f / 255f);
    private Color GLOW_ALERT_COLOR = new Color(255f / 255f, 119f / 255f, 1f / 255f, 50f / 255f);
    public QuickActionMenu quickActionMenu;
    private Queue<QueueData> messengerEntryCreateQueue;
    public TitleControllerScript titleController;
    public Transform friendsListEntries;
    public Transform friendRequestSection;
    public Transform onlineSection;
    public Transform offlineSection;
    public Transform blockedSection;
    private Dictionary<string, MessengerEntryScript> messengerEntries;
    public GameObject messengerEntry;
    public GameObject messengerMain;
    public GameObject messengerChatBox;
    public ScrollRect myScrollRect;
    public TextMeshProUGUI messengerChatBoxTxt;
    public TMP_InputField messengerInput;
    private bool notificationFlashOn;
    private float notificationFlashTimer;
    public Image glow;

    private bool chatBoxOpen;
    private string chattingWithFriendRequestId;
    private bool isSelect = false;

    void Awake()
    {
        messengerEntries = new Dictionary<string, MessengerEntryScript>();
        messengerEntryCreateQueue = new Queue<QueueData>();
    }

    public void JoinFriendGame(string username)
    {
        PlayerData.playerdata.globalChatClient.AskToJoinGame(username);
    }

    public void OnClickAddFriend()
    {
        if (PlayerData.playerdata.friendsList.Keys.Count < MAX_FRIENDS) {
            titleController.TriggerAddFriendPopup();
        } else {
            titleController.TriggerAlertPopup("YOU HAVE ALREADY REACHED THE MAXIMUM NUMBER OF ALLOWED FRIENDS (40).");
        }
    }

    public void AddFriend(string username)
    {
        if (username.ToLower() == PhotonNetwork.NickName.ToLower()) {
            titleController.TriggerAlertPopup("YOU CANNOT SEND A FRIEND REQUEST TO YOURSELF.");
            titleController.TriggerBlockScreen(false);
            return;
        }
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["requestUsername"] = username;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("sendFriendRequest");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "401") {
                    titleController.TriggerAlertPopup("USER [" + username + "] IS ALREADY YOUR FRIEND OR A REQUEST WITH THEM IS PENDING.");
                } else if (results["status"].ToString() == "402") {
                    titleController.TriggerAlertPopup("YOU CANNOT SEND A FRIEND REQUEST TO YOURSELF.");
                } else if (results["status"].ToString() != "200") {
                    titleController.TriggerAlertPopup("USER [" + username + "] DOES NOT EXIST!");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    public void RemoveFriend(string friendRequestId)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["friendRequestId"] = friendRequestId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("deleteFriendRequest");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    titleController.TriggerAlertPopup("USER IS NOT ON YOUR FRIENDS LIST!");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    public void AcceptFriendRequest(string friendRequestId)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["friendRequestId"] = friendRequestId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("acceptFriendRequest");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    titleController.TriggerAlertPopup("USER IS NOT ON YOUR FRIENDS LIST!");
                } else {
                    if (PlayerData.playerdata.friendsList.ContainsKey(friendRequestId)) {
                        PlayerData.playerdata.globalChatClient.AddStatusListenersToFriends(new List<string>(){PlayerData.playerdata.friendsList[friendRequestId].FriendUsername});
                    }
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    public void BlockFriend(string friendRequestId)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["friendRequestId"] = friendRequestId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("blockFriend");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    titleController.TriggerAlertPopup("USER IS NOT ON YOUR FRIENDS LIST!");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    public void UnblockFriend(string friendRequestId)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["friendRequestId"] = friendRequestId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("unblockFriend");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    titleController.TriggerAlertPopup("USER IS NOT ON YOUR FRIENDS LIST!");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    void Update()
    {
        HandleMessengerEntryCreateQueue();
        HandleNotificationFlash();
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return) && isSelect && messengerInput.text.Length > 0)
        {
            OnSendPrivateMessage();
            // messengerInput.text = "";
            ToggleInputFieldSelected(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && isSelect && messengerInput.text.Length == 0)
        {
            //TextSendObj.SetActive (false);
            ToggleInputFieldSelected(false);
            messengerInput.text = "";
        }
        // else if (!isSelect && TextSend.text.Length > 0)
        // {
        // 	isSelect = true;
        // 	EventSystem.current.SetSelectedGameObject(TextSend.gameObject, null);
        // }
    }

    void ToggleInputFieldSelected(bool b) {
        if (b) {
            EventSystem.current.SetSelectedGameObject(messengerInput.gameObject, null);
            messengerInput.ActivateInputField();
        } else {
            EventSystem.current.SetSelectedGameObject(null);
        }
        isSelect = b;
    }

    public MessengerEntryScript GetMessengerEntry(string friendRequestId)
    {
        if (messengerEntries.ContainsKey(friendRequestId)) {
            return messengerEntries[friendRequestId];
        }
        return null;
    }

    public void DeleteMessengerEntry(string friendRequestId)
    {
        Destroy(messengerEntries[friendRequestId].gameObject);
        messengerEntries.Remove(friendRequestId);
    }

    void HandleMessengerEntryCreateQueue()
    {
        if (messengerEntryCreateQueue.Count > 0) {
            QueueData q = messengerEntryCreateQueue.Dequeue();
            CreateMessengerEntry(q.friendRequestId, q.username, q.exp);
        }
    }

    public void EnqueueMessengerEntryCreation(string friendRequestId, string username, uint exp)
    {
        QueueData q = new QueueData();
        q.friendRequestId = friendRequestId;
        q.username = username;
        q.exp = exp;
        messengerEntryCreateQueue.Enqueue(q);
    }

    public void CreateMessengerEntry(string friendRequestId, string username, uint exp)
    {
        GameObject o = GameObject.Instantiate(messengerEntry, friendsListEntries);
        MessengerEntryScript m = o.GetComponent<MessengerEntryScript>();
        messengerEntries[friendRequestId] = m;
        m.InitEntry(this, friendRequestId, username, exp);
    }

    public void RefreshNotifications()
    {
        // Refresh every existing messenger entry
        foreach (KeyValuePair<string, MessengerEntryScript> p in messengerEntries) {
            if (PlayerData.playerdata.friendsList.ContainsKey(p.Value.GetFriendRequestId())) {
                string newUsername = PlayerData.playerdata.friendsList[p.Value.GetFriendRequestId()].FriendUsername;
                CachedMessage c = PlayerData.playerdata.cachedConversations[p.Value.GetFriendRequestId()];
                int newMessageCount = PlayerData.playerdata.globalChatClient.GetMessageCountForUser(newUsername);
                if (c.previousMessageCount != newMessageCount) {
                    p.Value.ToggleNotification(true);
                    if (!notificationFlashOn) {
                        ToggleNotification(true);
                    }
                }
            }
        }
    }

    public void ToggleMessenger()
    {
        if (messengerMain.activeInHierarchy) {
            messengerMain.SetActive(false);
            messengerChatBox.SetActive(false);
            quickActionMenu.gameObject.SetActive(false);
            ToggleNotification(false);
        } else {
            messengerMain.SetActive(true);
            messengerChatBox.SetActive(chatBoxOpen);
            ToggleNotification(false);
        }
    }

    public void ToggleMessengerChatBoxOff()
    {
        ToggleMessengerChatBox(false, null);
    }

    bool MessageDisplayable(string message)
    {
        if (message == PlayerData.playerdata.globalChatClient.GetRoomRequestCode() || (message.Length >= 5 && message.Substring(0, 5) == PlayerData.playerdata.globalChatClient.GetRoomJoinCode())) {
            return false;
        }
        return true;
    }

    public void ToggleMessengerChatBox(bool b, string friendRequestId)
    {
        if ((friendRequestId != null && !PlayerData.playerdata.friendsList.ContainsKey(friendRequestId)) || (chattingWithFriendRequestId != null && !PlayerData.playerdata.friendsList.ContainsKey(chattingWithFriendRequestId))) {
            chattingWithFriendRequestId = null;
            messengerChatBox.SetActive(false);
            chatBoxOpen = false;
            return;
        }

        string newFriendRequestId = null;
        if (b) {
            newFriendRequestId = friendRequestId;
        }
        
        // Save the previous chat
        if (chattingWithFriendRequestId != null) {
            if (chattingWithFriendRequestId != newFriendRequestId) {
                CachedMessage c = PlayerData.playerdata.cachedConversations[chattingWithFriendRequestId];
                c.cachedMessages = GetCurrentConversation();
                c.previousMessageCount = PlayerData.playerdata.globalChatClient.GetMessageCountForUser(PlayerData.playerdata.friendsList[chattingWithFriendRequestId].FriendUsername);
            }
        }

        // Load the old conversation of the next person plus any new messages
        chattingWithFriendRequestId = newFriendRequestId;

        if (chattingWithFriendRequestId != null) {
            ClearChatBox();
            string newUsername = PlayerData.playerdata.friendsList[chattingWithFriendRequestId].FriendUsername;
            CachedMessage c = PlayerData.playerdata.cachedConversations[chattingWithFriendRequestId];
            messengerChatBoxTxt.text = c.cachedMessages;
            int newMessageCount = PlayerData.playerdata.globalChatClient.GetMessageCountForUser(newUsername);
            if (c.previousMessageCount != newMessageCount) {
                List<object> newCachedMessages = PlayerData.playerdata.globalChatClient.GetCachedMessagesForUser(newUsername);
                for (int i = c.previousMessageCount; i < newMessageCount; i++) {
                    string newMessage = newCachedMessages[i].ToString();
                    if (MessageDisplayable(newMessage)) {
                        SendMsg(false, newMessage, newUsername);
                    }
                }
                // Go ahead and cache the new data again
                c.previousMessageCount = newMessageCount;
                c.cachedMessages = GetCurrentConversation();
            }
        }

        messengerChatBox.SetActive(b);
        chatBoxOpen = b;
    }

    public void UpdateStatusForUsername(string username, bool online, string newStatus)
    {
        foreach (KeyValuePair<string, MessengerEntryScript> entry in messengerEntries)
        {
            MessengerEntryScript m = entry.Value;
            if (PlayerData.playerdata.friendsList[m.GetFriendRequestId()].FriendUsername == username) {
                if (PlayerData.playerdata.friendsList[m.GetFriendRequestId()].Status == 1) {
                    m.UpdateSocialStatus(newStatus);
                }
                break;
            }
        }
    }

    public bool CheckIsVerifiedFriendByUsername(string username)
    {
        foreach (KeyValuePair<string, FriendData> entry in PlayerData.playerdata.friendsList)
        {
            if (entry.Value.FriendUsername == username) {
                if (entry.Value.Status == 1) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        
        return false;
    }

    public string GetFriendRequestIdByUsername(string username)
    {
        string idToRet = null;
        foreach (KeyValuePair<string, FriendData> entry in PlayerData.playerdata.friendsList)
        {
            if (entry.Value.FriendUsername == username) {
                idToRet = entry.Value.FriendRequestId;
                break;
            }
        }
        return idToRet;
    }

    public string GetCurrentConversation()
    {
        return messengerChatBoxTxt.text;
    }

    public void ClearChatBox()
    {
        messengerChatBoxTxt.text = "";
    }

    public void CacheCurrentChat()
    {
        if (chattingWithFriendRequestId != null) {
            CachedMessage c = PlayerData.playerdata.cachedConversations[chattingWithFriendRequestId];
            c.cachedMessages = GetCurrentConversation();
            c.previousMessageCount = PlayerData.playerdata.globalChatClient.GetMessageCountForUser(PlayerData.playerdata.friendsList[chattingWithFriendRequestId].FriendUsername);
        }
    }

    public string GetChattingWithFriendRequestId()
    {
        return chattingWithFriendRequestId;
    }

    public void OnSendPrivateMessage()
    {
        SendMsg(true, messengerInput.text, PhotonNetwork.NickName);
        if (!CanSendMessage(chattingWithFriendRequestId)) {
            SendServerMsg("THE USER IS CURRENTLY OFFLINE AND WILL NOT RECEIVE YOUR MESSAGES.");
            return;
        }
        PlayerData.playerdata.globalChatClient.SendPrivateMessageToUser(PlayerData.playerdata.friendsList[chattingWithFriendRequestId].FriendUsername, messengerInput.text);
        messengerInput.text = "";
    }

    bool CanSendMessage(string friendRequestId)
    {
        if (friendRequestId == null) return false;
        if (PlayerData.playerdata.friendsList[friendRequestId].Status != 1 || messengerEntries[friendRequestId].status.text == "OFFLINE") {
            return false;
        }

        return true;
    }

    public void SendServerMsg(string message)
    {
        messengerChatBoxTxt.text += "<color=#ffa500ff><b>SERVER: </b>" + message + "</color>\n";
        myScrollRect.verticalNormalizedPosition = 0f;
    }

    public void SendMsg(bool master, string message, string user)
    {
        if (master)
        {
            messengerChatBoxTxt.text += "<color=#63c6ffff>" + user + " : </color><color=#ffffffff>" + message + "</color>\n";
        }
        else
        {
            messengerChatBoxTxt.text += "<color=#9c4141ff>" + user + " : </color><color=#ffffffff>" + message + "</color>\n";
        }
        myScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ToggleNotification(bool b)
    {
        if (messengerMain.activeInHierarchy) {
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
                if (glow.color.Equals(GLOW_ALERT_COLOR)) {
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
            glow.color = GLOW_ALERT_COLOR;
        } else {
            glow.color = GLOW_NORMAL_COLOR;
        }
    }

    public void SelectInputByClick()
    {
        if (!isSelect)
        {
            isSelect = true;
        }
    }

    public void DeselectInputByClick() {
        if (isSelect) {
            isSelect = false;
        }
    }

    private struct QueueData {
        public string friendRequestId;
        public string username;
        public uint exp;
    }
}
