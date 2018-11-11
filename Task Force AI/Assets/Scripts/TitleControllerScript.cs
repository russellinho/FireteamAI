using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class TitleControllerScript : MonoBehaviour {

	public GameObject mainMenu;
	public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;

	public InputField PlayerNameInput;

	// Use this for initialization
	void Start () {
		PlayerData.playerdata.LoadPlayerData();
		PlayerNameInput.text = PlayerData.playerdata.playername;
		mainMenu.SetActive (true);
	}

	public void GoToMatchmakingMenu() {
		if (!PhotonNetwork.IsConnectedAndReady) {
			PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
			PhotonNetwork.ConnectUsingSettings();
		}

		mainMenu.SetActive (false);
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (true);
	}
		
	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		if (customizationMenu.activeInHierarchy) {
			PlayerData.playerdata.SavePlayerData ();
		}
		if (PhotonNetwork.IsConnectedAndReady) {
			PhotonNetwork.Disconnect ();
		}
		customizationMenu.SetActive (false);
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
	}

	public void GoToCustomization() {
		mainMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
		customizationMenu.SetActive (true);
	}

	public void goToMainMenu (){
		PlayerPrefs.SetString ("newScene", "MainMenu");
		SceneManager.LoadScene(7);
	}

	public void quitGame() {
		Application.Quit ();
	}

	public void savePlayerData()
	{
		PlayerData.playerdata.playername = PlayerNameInput.text;
		PlayerData.playerdata.SavePlayerData();
	}

	public void SetRedColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 0;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetBlueColor() {
		PlayerData.playerdata.color.x = 0;
		PlayerData.playerdata.color.y = 0;
		PlayerData.playerdata.color.z = 255;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetGreenColor() {
		PlayerData.playerdata.color.x = 0;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetYellowColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetOrangeColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 119;
		PlayerData.playerdata.color.z = 1;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetPurpleColor() {
		PlayerData.playerdata.color.x = 81;
		PlayerData.playerdata.color.y = 2;
		PlayerData.playerdata.color.z = 126;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetWhiteColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 255;
		PlayerData.playerdata.UpdateBodyColor ();
	}
		
}
