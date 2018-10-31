using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerAITestScript : MonoBehaviour {

	public GameObject[] redTeam;
	public GameObject[] blueTeam;

	void Update() {
		if (redTeam.Length == 0) {
			redTeam = GameObject.FindGameObjectsWithTag ("Red")
		}

		if (blueTeam.Length == 0) {
			blueTeam = GameObject.FindGameObjectsWithTag ("Blue");
		}
	}

}
