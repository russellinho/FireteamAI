using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class InGameMessengerHUD : MonoBehaviour {

	// HUD object reference
	public HUDContainer container;

	// Player reference
	public PlayerActionScript playerScript;
	public PhotonView pView;

	// Update is called once per frame
	void Update () {
        if (container == null) {
			GameObject c = GameObject.FindWithTag ("HUD");
			if (c != null) {
				container = c.GetComponent<HUDContainer> ();
			}
			return;
		}
		if (pView.IsMine) {
			HandleChat ();
		}
	}

	void HandleChat() {
		// Handle activating the input box
		if (PlayerPreferences.playerPreferences.KeyWasPressed("AllChat")) {
			if (!container.inGameMessenger.inputText.enabled) {
				container.inGameMessenger.inputText.text = "";
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
			CloseTextChat();
		}
	}

	public void CloseTextChat()
	{
		container.inGameMessenger.inputText.enabled = false;
		playerScript.fpc.canMove = true;
	}

	void SendChatMessage(string message) {
		pView.RPC ("RpcSendChatMessage", RpcTarget.All, message);
	}

	[PunRPC]
	void RpcSendChatMessage(string message) {
		if (gameObject.layer == 0) return;
		container.inGameMessenger.AddMessage ('n', message, pView.Owner.NickName);
	}

	public void SendVoiceCommandMessage(string playerName, string message)
	{
		container.inGameMessenger.AddMessage ('v', message, playerName);
	}

}



