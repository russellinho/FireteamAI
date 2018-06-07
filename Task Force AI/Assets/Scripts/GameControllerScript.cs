using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameControllerScript : MonoBehaviour {

	public static ArrayList playerList;

	public GameObject pauseMenuGUI;
	public Text objectivesText;
	public GameObject missionText;
	public GameObject actionBar;
	public Text defusingText;
	public Text hintText;
	public GameObject hudMap;
	public GameObject hudWaypoint;
	public GameObject healthBar;
	public GameObject ammoCount;
	public GameObject weaponLabel;
	public GameObject scoreboard;
	public GameObject endGameText;
	public GameObject endGameButton;
	public GameObject pauseExitBtn;
	public GameObject pauseResumeBtn;
	public GameObject pauseOptionsBtn;

	private int currentMap;
	private bool missionDisplayed;

    private ObjectivesTextScript objectiveFormatter;

    // BetaLevelNetworkTest mission variables
    public int bombsRemaining;

	private ArrayList missionWaypoints;
	private Camera c;
	private GameObject[] bs;
	private GameObject exitPoint;

	// Use this for initialization
	void Start () {
		// Find stuff
		pauseMenuGUI = GameObject.Find("PausePanel");
		objectivesText = GameObject.Find ("ObjectivesText").GetComponent<Text>();
		missionText = GameObject.Find ("IntroMissionText");
		actionBar = GameObject.Find ("ActionBar");
		defusingText = GameObject.Find ("DefusingText").GetComponent<Text>();
		hintText = GameObject.Find ("HintText").GetComponent<Text>();
		hudMap = GameObject.Find ("HUDMap");
		healthBar = GameObject.Find ("HealthBar");
		ammoCount = GameObject.Find ("AmmoCount");
		weaponLabel = GameObject.Find ("WeaponLabel");
		scoreboard = GameObject.Find ("Scoreboard");
		endGameText = GameObject.Find ("EndGameTxt");
		endGameButton = GameObject.Find ("EndGameBtn");
		pauseExitBtn = GameObject.Find ("QuitBtn");
		pauseResumeBtn = GameObject.Find ("ResumeBtn");
		pauseOptionsBtn = GameObject.Find ("OptionsBtn");
		endGameButton.GetComponent<Button> ().onClick.AddListener (ReturnToMenu);
		pauseExitBtn.GetComponent<Button> ().onClick.AddListener (ReturnToMenu);
		pauseResumeBtn.GetComponent<Button> ().onClick.AddListener (Pause);

        objectiveFormatter = new ObjectivesTextScript();
		missionWaypoints = new ArrayList ();
		// Which map are we on?
		// 0 - Main Menu, 1 - Beta Level
		// TODO: Before release, change names to indices
		if (SceneManager.GetActiveScene().name.Equals("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
            bombsRemaining = 4;
			currentMap = 1;
            objectivesText.text = objectiveFormatter.LoadObjectives(currentMap, bombsRemaining);

			GameObject m1 = GameObject.Instantiate (hudWaypoint);
			m1.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m2 = GameObject.Instantiate (hudWaypoint);
			m2.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m3 = GameObject.Instantiate (hudWaypoint);
			m3.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m4 = GameObject.Instantiate (hudWaypoint);
			m4.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			GameObject m5 = GameObject.Instantiate (hudWaypoint);
			m5.GetComponent<RectTransform> ().SetParent (hudMap.transform.parent);
			m5.GetComponent<RawImage> ().enabled = false;

			missionWaypoints.Add (m1);
			missionWaypoints.Add (m2);
			missionWaypoints.Add (m3);
			missionWaypoints.Add (m4);
			missionWaypoints.Add (m5);

		}
        
		pauseMenuGUI.SetActive (false);
		//actionBar.SetActive (false);
		ToggleActionBar(false);
		defusingText.enabled = false;
		hintText.enabled = false;
		playerList = new ArrayList();
		missionDisplayed = false;
		hudMap.SetActive (false);
		exitPoint = GameObject.Find ("ExitPoint");

		scoreboard.GetComponent<Image> ().enabled = false;
		endGameText.SetActive (false);
		endGameButton.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		// If the server starts, display the objectives for that mission
		if (currentMap == 1) {
			if (!missionDisplayed) {
				StartCoroutine (ShowMissionText ());
				missionDisplayed = true;
			}
			// Update waypoints
			if (c == null) {
				GameObject temp = GameObject.FindWithTag ("MainCamera");
				if (temp != null) c = temp.GetComponent<Camera> ();
			}
			if (bs == null) bs = GameObject.FindGameObjectsWithTag ("Bomb");
			for (int i = 0; i < missionWaypoints.Count; i++) {
				if (c == null)
					break;
				if (i == missionWaypoints.Count - 1) {
					float renderCheck = Vector3.Dot ((exitPoint.transform.position - c.transform.position).normalized, c.transform.forward);
					if (renderCheck <= 0)
						continue;
					if (bombsRemaining == 0) {
						((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = true;
						((GameObject)missionWaypoints [i]).GetComponent<RectTransform> ().position = c.WorldToScreenPoint (exitPoint.transform.position);
					}
				} else {
					float renderCheck = Vector3.Dot ((bs [i].transform.position - c.transform.position).normalized, c.transform.forward);
					if (renderCheck <= 0)
						continue;
					if (!bs [i].GetComponent<BombScript> ().defused && c != null) {
						Vector3 p = new Vector3 (bs [i].transform.position.x,bs [i].transform.position.y + bs[i].transform.lossyScale.y,bs [i].transform.position.z);
						((GameObject)missionWaypoints [i]).GetComponent<RectTransform> ().position = c.WorldToScreenPoint (p);
					}
					if (((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled && bs [i].GetComponent<BombScript> ().defused) {
						((GameObject)missionWaypoints [i]).GetComponent<RawImage> ().enabled = false;
					}
				}
			}
			// Check if the mission is over
			if (bombsRemaining == 0) {
				if (CheckEscape ()) {
					// If they can escape, end the game and bring up the stat board
					EndGame();
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape) && !scoreboard.GetComponent<Image>().enabled) 
			Pause ();

		if (pauseMenuGUI.activeInHierarchy || endGameText.activeInHierarchy) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

	}

	void Pause () {
		if (!pauseMenuGUI.activeInHierarchy) {
			pauseMenuGUI.SetActive (true);
		} else {
			pauseMenuGUI.SetActive (false);
		}
	}

	IEnumerator ShowMissionText() {
		yield return new WaitForSeconds (5f);
		missionText.SetActive (true);
	}

	public void ToggleActionBar(bool enable) {
		int c = actionBar.GetComponentsInChildren<Image>().Length;
		if (!enable) {
			// Disable all actionbar components
			for (int i = 0; i < c; i++) {
				actionBar.GetComponentsInChildren<Image> () [i].enabled = false;
			}
		} else {
			for (int i = 0; i < c; i++) {
				actionBar.GetComponentsInChildren<Image> () [i].enabled = true;
			}
		}
	}

	public void UpdateObjectives() {
		objectivesText.text = objectiveFormatter.LoadObjectives(currentMap, bombsRemaining);
	}

	public void EscapePopup() {
		missionText.GetComponent<Text> ().text = "Escape available! Head to the waypoint!";
		missionText.SetActive (true);
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
		for (int i = 0; i < playerList.Count; i++) {
			((GameObject)playerList[i]).GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().canMove = false;
			((GameObject)playerList [i]).GetComponent<PlayerScript> ().canShoot = false;
		}
		// Show the scoreboard
		DisableHUD();
		ToggleScoreboard ();
	}

	public void DisableHUD() {
		healthBar.GetComponent<Text> ().enabled = false;
		weaponLabel.GetComponent<Text> ().enabled = false;
		ammoCount.GetComponent<Text> ().enabled = false;
		hudMap.SetActive (false);
	}

	public void ToggleScoreboard() {
		scoreboard.GetComponent<Image> ().enabled = true;
		endGameText.SetActive (true);
		endGameButton.SetActive (true);
	}

	public void ReturnToMenu() {
		SceneManager.LoadScene ("Title");
		GameObject.Find ("NetworkMan").GetComponent<NetworkManager> ().StopHost ();
	}

}
