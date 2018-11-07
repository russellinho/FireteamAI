using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleControllerScript : MonoBehaviour {

	public GameObject mainMenu;
	public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject settingsMenu;

	public InputField PlayerNameInput;

	// Use this for initialization
	void Start () {
		PlayerData.playerdata.LoadPlayerData();
		PlayerNameInput.text = PlayerData.playerdata.playername;
		mainMenu.SetActive (true);
	}

	public void GoToMatchmakingMenu() {
		mainMenu.SetActive (false);
		settingsMenu.SetActive (false);
		matchmakingMenu.SetActive (true);
	}

	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		if (settingsMenu.activeInHierarchy) {
			PlayerData.playerdata.SavePlayerData ();
		}
		settingsMenu.SetActive (false);
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
	}

	public void GoToCustomization() {
		mainMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
		settingsMenu.SetActive (true);
	}

	public void goToMainMenu (){
		PlayerPrefs.SetString ("newScene", "MainMenu");
		SceneManager.LoadScene(7);
	}

	public void goToTutorial()
	{
		PlayerPrefs.SetString ("newScene", "TutorialLevel");
		SceneManager.LoadScene(7);
	}

	public void goToTargetPractice() {
		PlayerPrefs.SetString ("newScene", "TargetPractice");
		SceneManager.LoadScene (7);
	}

	public void savePlayerName()
	{
		PlayerData.playerdata.LoadPlayerData();
		PlayerData.playerdata.playername = PlayerNameInput.text;
		PlayerData.playerdata.SavePlayerData();
	}
}
