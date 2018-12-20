using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameMessenger : MonoBehaviour {

	public GameObject messageInfo;
	public Queue<GameObject> messageInfos;
	public TextMeshProUGUI chatText;
	public TMP_InputField inputText;

	// Use this for initialization
	void Start () {
		messageInfos = new Queue<GameObject> ();
		chatText.text = "";
		inputText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
		if (messageInfos.Count > 0) {
			GameObject o = messageInfos.Peek ();
			// If message is out of time, dequeue it
			if (o.GetComponent<MessageInfo> ().timeRemaining <= 0f) {
				chatText.text = chatText.text.Substring (o.GetComponent<MessageInfo>().GetLength());
				messageInfos.Dequeue ();
			}
		}
	}

	public void Add	qMessage(string message, ) {
		
	}
}
