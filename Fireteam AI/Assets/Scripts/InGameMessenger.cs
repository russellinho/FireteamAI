using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameMessenger : MonoBehaviour {

	public GameObject messageInfo;
	public Queue<GameObject> messageInfos;
	private TextMeshProUGUI chatText;
	public TMP_InputField inputText;
	private static short MAX_MESSAGES = 6;

    void Awake()
    {
        chatText = GetComponent<TextMeshProUGUI>();
        chatText.enabled = false;
		inputText.enabled = false;
        messageInfos = new Queue<GameObject>();
    }

    // Use this for initialization
    void Start () {
		chatText.text = "";
		inputText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
		if (messageInfos.Count > 0) {
			GameObject o = messageInfos.Peek ();
			// If message is out of time, dequeue it
			if (o.GetComponent<MessageInfo> ().timeRemaining <= 0f) {
				ExpireMessage ();
			}
		} else {
			chatText.enabled = false;
		}
	}

	public void AddMessage(string message, string playerName) {
        // First, check if the queue already has 5 messages in it, don't want more than 5 messages on screen at a time
		if (messageInfos.Count > MAX_MESSAGES) {
            ExpireMessage();
        }

        // Second, create message info and add to queue
        string totalMessage = playerName + ": " + message + "\n";
        GameObject toAdd = (GameObject)Instantiate(messageInfo);
        toAdd.GetComponent<MessageInfo>().SetLength((short)totalMessage.Length);
        messageInfos.Enqueue(toAdd);

        // Last, actually add the message to the chat box
        if (!chatText.enabled) {
            chatText.enabled = true;
        }
        chatText.text += totalMessage;
	}

    void ExpireMessage() {
        GameObject expiredMessage = messageInfos.Dequeue();
        chatText.text = chatText.text.Substring(expiredMessage.GetComponent<MessageInfo>().GetLength());
    }
}
