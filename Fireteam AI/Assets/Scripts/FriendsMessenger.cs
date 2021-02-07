using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class FriendsMessenger : MonoBehaviour
{
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

    public void AddFriend()
    {
        // Dictionary<string, object> inputData = new Dictionary<string, object>();
        // inputData["callHash"] = DAOScript.functionsCallHash;
		// inputData["uid"] = AuthScript.authHandler.user.UserId;
        // inputData["equipped" + type] = weaponName;
        
        // ts.TriggerBlockScreen(true);
		// HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		// func.CallAsync(inputData).ContinueWith((taskA) => {
        //     if (taskA.IsFaulted) {
        //         PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
        //     } else {
        //         Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
        //         if (results["status"].ToString() == "200") {
        //             Debug.Log("Save successful.");
        //         } else {
        //             PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
        //         }
        //     }
        // });
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
        Debug.Log("1");
        GameObject o = GameObject.Instantiate(messengerEntry, friendsListEntries);
        Debug.Log("2");
        MessengerEntryScript m = o.GetComponent<MessengerEntryScript>();
        Debug.Log("3");
        messengerEntries[friendRequestId] = m;
        Debug.Log("4");
        m.InitEntry(this, friendRequestId, username);
        Debug.Log("5");
    }

    public void ToggleMessenger()
    {
        if (messengerMain.activeInHierarchy) {
            messengerMain.SetActive(false);
            messengerChatBox.SetActive(false);
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
