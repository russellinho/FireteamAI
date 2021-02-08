using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class FriendsMessenger : MonoBehaviour
{
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
    private bool chatBoxOpen;

    void Awake()
    {
        messengerEntries = new Dictionary<string, MessengerEntryScript>();
        messengerEntryCreateQueue = new Queue<QueueData>();
    }

    public void OnClickAddFriend()
    {
        titleController.TriggerAddFriendPopup();
    }

    public void AddFriend(string username)
    {
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
    }

    public MessengerEntryScript GetMessengerEntry(string friendRequestId)
    {
        return messengerEntries[friendRequestId];
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
            CreateMessengerEntry(q.friendRequestId, q.username);
        }
    }

    public void EnqueueMessengerEntryCreation(string friendRequestId, string username)
    {
        QueueData q = new QueueData();
        q.friendRequestId = friendRequestId;
        q.username = username;
        messengerEntryCreateQueue.Enqueue(q);
    }

    public void CreateMessengerEntry(string friendRequestId, string username)
    {
        GameObject o = GameObject.Instantiate(messengerEntry, friendsListEntries);
        MessengerEntryScript m = o.GetComponent<MessengerEntryScript>();
        messengerEntries[friendRequestId] = m;
        m.InitEntry(this, friendRequestId, username);
    }

    public void ToggleMessenger()
    {
        if (messengerMain.activeInHierarchy) {
            messengerMain.SetActive(false);
            messengerChatBox.SetActive(false);
            quickActionMenu.gameObject.SetActive(false);
        } else {
            messengerMain.SetActive(true);
            messengerChatBox.SetActive(chatBoxOpen);
        }
    }

    public void ToggleMessengerChatBox(bool b)
    {
        messengerChatBox.SetActive(b);
        chatBoxOpen = b;
    }

    private struct QueueData {
        public string friendRequestId;
        public string username;
    }
}
