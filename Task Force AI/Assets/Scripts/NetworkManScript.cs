using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManScript : MonoBehaviour {

	private NetworkManagerHUD manHud;

	void Start() {
		manHud = GetComponent<NetworkManagerHUD> ();
	}

	// Update is called once per frame
	void Update () {
		if (manHud.showGUI == true && SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetwork")) {
			manHud.showGUI = false;
		}
	}
}
