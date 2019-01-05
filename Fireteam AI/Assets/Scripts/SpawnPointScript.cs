using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPointScript : MonoBehaviour {

	private bool firstSpawn;
	private GameObject spawnedEnemyRef;
	private bool spawning;

	// Use this for initialization
	void Awake () {
		firstSpawn = false;
		spawning = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!PhotonNetwork.IsMasterClient) {
			return;
		}

		if (!firstSpawn) {
			firstSpawn = true;
			spawnedEnemyRef = PhotonNetwork.Instantiate(
				"Y_Bot",
				transform.position,
				transform.rotation, 0);
		}

		if (!spawning && (!spawnedEnemyRef || spawnedEnemyRef.GetComponent<BetaEnemyScript> ().health <= 0)) {
			spawning = true;
			StartCoroutine ("SpawnNewEnemy");
		}
	}

	IEnumerator SpawnNewEnemy() {
		yield return new WaitForSeconds (25f);
		spawnedEnemyRef = PhotonNetwork.Instantiate(
			"Y_Bot",
			transform.position,
			transform.rotation, 0);
		spawning = false;
		
	}

}
