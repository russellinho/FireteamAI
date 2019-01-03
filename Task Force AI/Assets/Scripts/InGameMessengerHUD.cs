﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Realtime;
using Photon.Pun;

public class InGameMessengerHUD : MonoBehaviour {

	// HUD object reference
	public HUDContainer container;

	// Player reference
	private PlayerScript playerScript;
	private PhotonView pView;

	// Use this for initialization
	void Start () {
		container = GameObject.Find ("HUD").GetComponent<HUDContainer> ();
		playerScript = GetComponent<PlayerScript> ();
		pView = playerScript.GetComponent<PhotonView> ();
	}
	
	// Update is called once per frame
	void Update () {
		HandleChat ();
	}

	void HandleChat() {
		// Handle activating the input box
		if (Input.GetKeyDown (KeyCode.T)) {
			if (!container.inGameMessenger.inputText.enabled) {
				// Enable
				container.inGameMessenger.inputText.enabled = true;
				playerScript.fpc.canMove = false;
				// Select the input box
				EventSystem.current.SetSelectedGameObject(container.inGameMessenger.inputText.gameObject, null);
				container.inGameMessenger.inputText.OnPointerClick (new PointerEventData(EventSystem.current));
			}
		}

		// Handle message completion and sending
		if (container.inGameMessenger.inputText.enabled && Input.GetKeyDown(KeyCode.Return)) {
			// If the message is empty, then just close the chat. Else, send the message over RPC.
			if (container.inGameMessenger.inputText.text.Length != 0) {
				SendChatMessage (container.inGameMessenger.inputText.text);
				container.inGameMessenger.inputText.text = "";
			}
			container.inGameMessenger.inputText.enabled = false;
			playerScript.fpc.canMove = true;
		}
	}

	void SendChatMessage(string message) {
		pView.RPC ("RpcSendChatMessage", RpcTarget.All, message);
	}

	[PunRPC]
	void RpcSendChatMessage(string message) {
		container.inGameMessenger.AddMessage (message, PhotonNetwork.LocalPlayer.NickName);
	}
		
}
