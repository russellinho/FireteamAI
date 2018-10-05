using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TitleControllerScript : MonoBehaviour {

	public GameObject mainMenu;
	public GameObject networkMan;
	public GameObject matchmakingMenu;

	// Use this for initialization
	void Start () {
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
	}

	public void GoToMatchmakingMenu() {
		mainMenu.SetActive (false);
		matchmakingMenu.SetActive (true);
	}

	public void ReturnToMainMenu() {
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
	}
}
