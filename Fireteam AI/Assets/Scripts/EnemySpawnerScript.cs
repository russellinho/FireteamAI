using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class EnemySpawnerScript : NetworkBehaviour {

	public GameObject enemyPrefab;
	private Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
	private Quaternion spawnRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
	private ArrayList enemyList;
	private GameObject[] enemySpawns;

	public override void OnStartServer() {
		enemyList = new ArrayList ();
		enemySpawns = GameObject.FindGameObjectsWithTag ("EnemySpawnPoint");

		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetworkTest") || SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetwork")) {
			for (int i = 0; i < enemySpawns.Length; i++) {
				Vector3 t = ((GameObject)enemySpawns [i]).transform.position;
				Quaternion q = ((GameObject)enemySpawns [i]).transform.rotation;
				GameObject toSpawn = (GameObject)Instantiate (enemyPrefab, t, q);
				toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
				NetworkServer.Spawn (toSpawn);
				enemyList.Add (toSpawn);
			}
			/**
			spawnPosition = new Vector3 (-21.04f, 7.1f, 155.787f);
			spawnRotation.eulerAngles = new Vector3(0f,180f,0f);
			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);

			spawnPosition = new Vector3 (-27.25f, 7.1f, 114.83f);
			spawnRotation.eulerAngles = new Vector3(0f,180f,0f);
			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);

			// Ground patrol
			spawnPosition = new Vector3 (-14.32f, 0f, 155.787f);
			spawnRotation.eulerAngles = new Vector3(0f,-127.63f,0f);
			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);

			spawnPosition = new Vector3 (-6.87f, 0f, 117.89f);
			spawnRotation.eulerAngles = new Vector3(0f,-127.63f,0f);
			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);

			spawnPosition = new Vector3 (8.669f, 0f, 146.65f);
			spawnRotation.eulerAngles = new Vector3(0f,270f,0f);
			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);
		} else {
			GameObject toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			toSpawn.GetComponent<BetaEnemyScript> ().enemyType = BetaEnemyScript.EnemyType.Scout;
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);*/
			/**
			spawnPosition = new Vector3(-10.55f, 0f, -1.3f);

			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);

			spawnPosition = new Vector3(-0.55f, 0f, -1.7f);

			toSpawn = (GameObject)Instantiate (enemyPrefab, spawnPosition, spawnRotation);
			NetworkServer.Spawn (toSpawn);
			enemyList.Add (toSpawn);
			
		}*/

		}
	}
		
}
