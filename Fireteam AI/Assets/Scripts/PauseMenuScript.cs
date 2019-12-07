using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PauseMenuScript : MonoBehaviourPunCallbacks {

	public GameObject keyMappingsPanel;
	public GameObject mainMenuGroup;
	public GameObject optionsMenuGroup;
	public GameObject returnToMainBtn;

	public void HandleEscPress() {
		// If the key binding menu is up and you press escape, then return to options menu
		if (keyMappingsPanel.activeInHierarchy) {
			CloseKeyMappings();
		} else if (optionsMenuGroup.activeInHierarchy) {
			// If the options menu group is active, then go back to main menu
			CloseOptionsMenu();
		} else if (mainMenuGroup.activeInHierarchy) {
			// If back on the main menu, then resume game
			ResumeGame();
		}
	}

	public void ResumeGame() {
		gameObject.SetActive (false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void LeaveGame() {
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom() {
		PhotonNetwork.Disconnect ();
	}

	public void OpenKeyMappings() {
		optionsMenuGroup.SetActive(false);
		returnToMainBtn.SetActive(false);
		keyMappingsPanel.SetActive(true);
	}

	public void CloseKeyMappings() {
		optionsMenuGroup.SetActive(true);
		returnToMainBtn.SetActive(true);
		keyMappingsPanel.SetActive(false);
	}

	public void OpenOptionsMenu() {
		mainMenuGroup.SetActive(false);
		optionsMenuGroup.SetActive(true);
		returnToMainBtn.SetActive(true);
	}

	public void CloseOptionsMenu() {
		mainMenuGroup.SetActive(true);
		optionsMenuGroup.SetActive(false);
		returnToMainBtn.SetActive(false);
	}

}
