using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class TitleControllerScript : MonoBehaviour {

	public GameObject mainMenu;
	private GameObject networkMan;
	public GameObject matchmakingMenu;

	// Use this for initialization
	void Start () {
		networkMan = GameObject.Find ("NetworkMan");
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
		networkMan.GetComponent<NetworkManagerHUD> ().showGUI = false;
	}
	

	public void GoToMatchmakingMenu() {
		mainMenu.SetActive (false);
		matchmakingMenu.SetActive (true);
		networkMan.GetComponent<NetworkManagerHUD> ().showGUI = true;
	}

	public void ReturnToMainMenu() {
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
		networkMan.GetComponent<NetworkManagerHUD> ().showGUI = false;
	}
}
