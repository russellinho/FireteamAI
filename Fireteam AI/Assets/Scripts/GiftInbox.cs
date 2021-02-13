using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class GiftInbox : MonoBehaviour
{
    private const float NOTIFICATION_FLASH_TIME = 1.2f;
    private Color GLOW_NORMAL_COLOR = new Color(99f / 255f, 198f / 255f, 255f / 255f, 50f / 255f);
    private Color GLOW_ALERT_COLOR = new Color(255f / 255f, 119f / 255f, 1f / 255f, 50f / 255f);
    public GameObject giftInboxMain;
    public TitleControllerScript titleController;
    public QuickActionMenu quickActionMenu;
    private Dictionary<string, GiftEntryScript> giftEntries;
    private Queue<QueueData> giftEntryCreateQueue;
    public Transform giftListEntries;
    public GameObject giftEntry;
    private bool notificationFlashOn;
    private float notificationFlashTimer;
    public Image glow;
    void Awake()
    {
        giftEntries = new Dictionary<string, GiftEntryScript>();
        giftEntryCreateQueue = new Queue<QueueData>();
    }

    void Update()
    {
        HandleGiftEntryCreateQueue();
        HandleNotificationFlash();
    }

    public void ToggleMessenger()
    {
        if (giftInboxMain.activeInHierarchy) {
            giftInboxMain.SetActive(false);
            quickActionMenu.gameObject.SetActive(false);
            ToggleNotification(false);
        } else {
            giftInboxMain.SetActive(true);
        }
    }

    public void ToggleNotification(bool b)
    {
        if (giftInboxMain.activeInHierarchy) {
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

    public void CreateGiftEntry(string giftId, string category, string sender, string itemName, float duration, string message)
    {
        GameObject o = GameObject.Instantiate(giftEntry, giftListEntries);
        GiftEntryScript m = o.GetComponent<GiftEntryScript>();
        giftEntries.Add(giftId, m);
        m.InitEntry(this, giftId, category, sender, itemName, duration, message);
        ToggleNotification(true);
    }

    public GiftEntryScript GetGiftEntry(string giftId)
    {
        return giftEntries[giftId];
    }

    public void DeleteGiftEntry(string giftId)
    {
        Destroy(giftEntries[giftId].gameObject);
        giftEntries.Remove(giftId);
    }

    public void EnqueueGiftEntryCreation(string giftId, string category, string sender, string itemName, float duration, string message)
    {
        QueueData q = new QueueData();
        q.giftId = giftId;
        q.category = category;
        q.sender = sender;
        q.itemName = itemName;
        q.duration = duration;
        q.message = message;
        giftEntryCreateQueue.Enqueue(q);
    }

    void HandleGiftEntryCreateQueue()
    {
        if (giftEntryCreateQueue.Count > 0) {
            QueueData q = giftEntryCreateQueue.Dequeue();
            CreateGiftEntry(q.giftId, q.category, q.sender, q.itemName, q.duration, q.message);
        }
    }

    public void GiftItem(string username, string itemName, string category, float duration, string message)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["username"] = username;
        inputData["itemName"] = itemName;
        inputData["category"] = category;
        inputData["duration"] = duration;
        inputData["message"] = message;

        titleController.TriggerBlockScreen(true);
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("giftItemToUser");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    titleController.TriggerAlertPopup("GIFT SENT TO [" + username + "] SUCCESSFULLY!");
                } else if (results["status"].ToString() == "401") {
                    titleController.TriggerAlertPopup("THE USER [" + username + "] WAS NOT FOUND. PLEASE ENSURE THAT THE SPELLING IS CORRECT AND TRY AGAIN.");
                } else {
                    titleController.TriggerAlertPopup("THE GIFT COULD NOT BE SENT. PLEASE CONTACT AN ADMIN.");
                }
                titleController.TriggerBlockScreen(false);
                titleController.confirmingGift = false;
            }
        });
    }

    public void AcceptGift(string giftId)
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["giftId"] = giftId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("acceptGift");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    PlayerData.playerdata.AddItemToInventory(results["itemName"].ToString(), results["category"].ToString(), Convert.ToSingle(results["duration"]), false);
                } else if (results["status"].ToString() == "401") {
                    titleController.TriggerAlertPopup("GIFT DOES NOT EXIST.");
                } else {
                    titleController.TriggerAlertPopup("GIFT DOES NOT EXIST.");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    private struct QueueData {
        public string giftId;
        public string category;
        public string sender;
        public string itemName;
        public float duration;
        public string message;
    }
}
