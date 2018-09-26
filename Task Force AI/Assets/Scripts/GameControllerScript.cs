using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameControllerScript : MonoBehaviour {

	public int currentMap;

    // Bomb defusal mission variables
	public GameObject[] bombs;
    public int bombsRemaining;
	public bool gameOver;

	public Camera c;
	public GameObject exitPoint;

	// Use this for initialization
	void Start () {
		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			bombs = GameObject.FindGameObjectsWithTag ("Bomb");
		}
		gameOver = false;
		exitPoint = GameObject.Find ("ExitPoint");

	}

	// Update is called once per frame
	void Update () {
        if (currentMap == 1)
        {

            // Update waypoints
            if (c == null)
            {
                GameObject temp = GameObject.FindWithTag("MainCamera");
                if (temp != null) c = temp.GetComponent<Camera>();
            }
            if (bombs == null) bombs = GameObject.FindGameObjectsWithTag("Bomb");

            // Check if the mission is over
            if (bombsRemaining == 0) {
				if (!gameOver || CheckEscape ()) {
					// If they can escape, end the game and bring up the stat board
					gameOver = true;
					EndGame();
				}
			}
        }

	}

	public bool CheckEscape() {
		for (int i = 0; i < playerList.Count; i++) {
			GameObject p = (GameObject) playerList [i];
			if (p.GetComponent<PlayerScript> ().health <= 0f) {
				continue;
			}
			if (p.transform.position.y < exitPoint.transform.position.y - 0.5f || Vector3.Distance (p.transform.position, exitPoint.transform.position) > 5f) {
				return false;
			}
		}
		return true;
	}

	void EndGame() {
		// Remove all enemies
		GameObject[] es = GameObject.FindGameObjectsWithTag("Human");
		for (int i = 0; i < es.Length; i++) {
			Destroy (es[i]);
		}
		// Don't allow player to move or shoot
		for (int i = 0; i < es.Length; i++) {
			es[i].GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().canMove = false;
			es[i].GetComponent<PlayerScript> ().canShoot = false;
		}

	}

}
