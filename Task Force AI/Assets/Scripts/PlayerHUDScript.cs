using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PlayerHUDScript : MonoBehaviourPunCallbacks {

	// Player reference
	private PlayerScript playerScript;
	private WeaponScript wepScript;
	private GameControllerScript gameController;

    // Health HUD
    private const string HEALTH_TEXT = "Health: ";
    private Text healthText;

    // Weapon HUD
    private Text weaponLabelTxt;
    private Text ammoTxt;

    // Pause/in-game menu HUD
    public GameObject pauseMenuGUI;
    public GameObject pauseExitBtn;
    public GameObject pauseResumeBtn;
    public GameObject pauseOptionsBtn;
    public GameObject scoreboard;
    public GameObject endGameText;
    public GameObject endGameButton;

    // Hit indication HUD
    public GameObject hitFlare;
    private GameObject hitDir;

    // Map HUD
    public GameObject hudMap;
    public GameObject hudWaypoint;
	private ArrayList missionWaypoints;

    // On-screen indication HUD
    public Text objectivesText;
    public GameObject missionText;
    public GameObject actionBar;
    public Text defusingText;
    public Text hintText;
	private ObjectivesTextScript objectiveFormatter;
	public Text spectatorText;

    // Use this for initialization
    void Start () {
        if (!GetComponent<PhotonView>().IsMine) {
            this.enabled = false;
			return;
        }
		// Find/load HUD components
		LoadHUDComponents ();

		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;

		pauseMenuGUI.SetActive (false);
		ToggleActionBar(false);
		defusingText.enabled = false;
		hintText.enabled = false;
		scoreboard.GetComponent<Image> ().enabled = false;
		endGameText.SetActive (false);
		endGameButton.SetActive (false);
		spectatorText.gameObject.SetActive (false);

		//hudMap.SetActive (true);

		playerScript = GetComponent<PlayerScript> ();
		wepScript = GetComponent<WeaponScript> ();
		gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript> ();

		LoadBetaLevel ();
	}

	void LoadBetaLevel() {
		if (SceneManager.GetActiveScene().name.Equals("BetaLevelNetworkTest") || SceneManager.GetActiveScene().name.Equals("BetaLevelNetwork")) {
			gameController.bombsRemaining = 4;
			gameController.currentMap = 1;
			objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);

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

			StartCoroutine(ShowMissionText());

		}
	}

	void LoadHUDComponents() {
		// Health HUD
		healthText = GameObject.Find ("HealthBar").GetComponent<Text>();

		// Weapon HUD
		weaponLabelTxt = GameObject.Find ("WeaponLabel").GetComponent<Text>();
		ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();

		// Pause/in-game menu HUD
		pauseMenuGUI = GameObject.Find ("PausePanel");
		pauseExitBtn = GameObject.Find ("QuitBtn");
		pauseResumeBtn = GameObject.Find ("ResumeBtn");
		pauseOptionsBtn = GameObject.Find ("OptionsBtn");
		scoreboard = GameObject.Find ("Scoreboard");
		endGameText = GameObject.Find ("EndGameTxt");
		endGameButton = GameObject.Find ("EndGameBtn");

		// Hit indication HUD
		hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");

		// Map HUD
		hudMap = GameObject.Find ("HUDMap");
		missionWaypoints = new ArrayList ();

		// On-screen indication HUD
		objectivesText = GameObject.Find ("ObjectivesText").GetComponent<Text>();
		missionText = GameObject.Find ("IntroMissionText");
		actionBar = GameObject.Find ("ActionBar");
		defusingText = GameObject.Find ("DefusingText").GetComponent<Text>();
		hintText = GameObject.Find ("HintText").GetComponent<Text>();
		spectatorText = GameObject.Find ("SpectatorTxt").GetComponent<Text> ();
		objectiveFormatter = new ObjectivesTextScript();

	}
	
	// Update is called once per frame
	void Update () {
		if (playerScript == null || wepScript == null) {
			playerScript = GetComponent<PlayerScript> ();
			wepScript = GetComponent<WeaponScript> ();
		}
		if (gameController == null) {
			gameController = GameObject.Find ("GameController").GetComponent<GameControllerScript> ();
		}
		healthText.text = (healthText ? HEALTH_TEXT + playerScript.health : "");

		// Update UI
		weaponLabelTxt.text = playerScript.currWep;
		ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
		if (!gameController.gameOver) {
			UpdateWaypoints ();
		} else {
			missionWaypoints = null;
		}

		UpdateCursorStatus ();

		if (gameController.gameOver && healthText.enabled) {
			DisableHUD();
			ToggleScoreboard ();
		}
    }

	void FixedUpdate() {
		UpdateHitFlare();
	}

	void UpdateCursorStatus() {
		if (Input.GetKeyDown(KeyCode.Escape) && !scoreboard.GetComponent<Image>().enabled)
			Pause();

		if (pauseMenuGUI.activeInHierarchy || endGameText.activeInHierarchy)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void UpdateWaypoints() {
		for (int i = 0; i < missionWaypoints.Count; i++)
		{
			if (gameController.c == null)
				break;
			if (i == missionWaypoints.Count - 1)
			{
				float renderCheck = Vector3.Dot((gameController.exitPoint.transform.position - gameController.c.transform.position).normalized, gameController.c.transform.forward);
				if (renderCheck <= 0)
					continue;
				if (gameController.bombsRemaining == 0)
				{
					((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = true;
					((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = gameController.c.WorldToScreenPoint(gameController.exitPoint.transform.position);
				}
			}
			else
			{
				float renderCheck = Vector3.Dot((gameController.bombs[i].transform.position - gameController.c.transform.position).normalized, gameController.c.transform.forward);
				if (renderCheck <= 0)
					continue;
				if (!gameController.bombs[i].GetComponent<BombScript>().defused && gameController.c != null)
				{
					Vector3 p = new Vector3(gameController.bombs[i].transform.position.x, gameController.bombs[i].transform.position.y + gameController.bombs[i].transform.lossyScale.y, gameController.bombs[i].transform.position.z);
					((GameObject)missionWaypoints[i]).GetComponent<RectTransform>().position = gameController.c.WorldToScreenPoint(p);
				}
				if (((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled && gameController.bombs[i].GetComponent<BombScript>().defused)
				{
					((GameObject)missionWaypoints[i]).GetComponent<RawImage>().enabled = false;
				}
			}
		}
	}

	void UpdateHitFlare() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		if (playerScript.hitTimer < 1f) {
			hitFlare.GetComponent<RawImage> ().enabled = true;
			hitDir.GetComponent<RawImage> ().enabled = true;
			playerScript.hitTimer += Time.deltaTime;
		} else {
			hitFlare.GetComponent<RawImage> ().enabled = false;
			hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward, playerScript.hitLocation);
			Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}
	}

    public void DisableHUD()
    {
        healthText.enabled = false;
        weaponLabelTxt.enabled = false;
        ammoTxt.enabled = false;
        hudMap.SetActive(false);
    }

    public void ToggleScoreboard()
    {
        scoreboard.GetComponent<Image>().enabled = true;
        endGameText.SetActive(true);
        endGameButton.SetActive(true);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Title");
		PhotonNetwork.Disconnect ();
    }

    void Pause()
    {
        if (!pauseMenuGUI.activeInHierarchy)
        {
            pauseMenuGUI.SetActive(true);
        }
        else
        {
            pauseMenuGUI.SetActive(false);
        }
    }

    IEnumerator ShowMissionText()
    {
        yield return new WaitForSeconds(5f);
		missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

    public void ToggleActionBar(bool enable)
    {
        int c = actionBar.GetComponentsInChildren<Image>().Length;
        if (!enable)
        {
            // Disable all actionbar components
            for (int i = 0; i < c; i++)
            {
                actionBar.GetComponentsInChildren<Image>()[i].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < c; i++)
            {
                actionBar.GetComponentsInChildren<Image>()[i].enabled = true;
            }
        }
    }

    public void UpdateObjectives()
    {
        objectivesText.text = objectiveFormatter.LoadObjectives(gameController.currentMap, gameController.bombsRemaining);
    }

    public void EscapePopup()
    {
        missionText.GetComponent<Text>().text = "Escape available! Head to the waypoint!";
		missionText.GetComponent<MissionTextAnimScript> ().SetStarted ();
    }

	public void SetActionBarSlider(float val) {
		actionBar.GetComponent<Slider> ().value = val;
	}

	public void EnableSpectatorMessage() {
		spectatorText.gameObject.SetActive (true);
	}

	//public override void OnPlayerEnteredRoom(Player newPlayer) {
	//	Debug.Log (newPlayer.NickName + " has joined the room");
	//	GameControllerScript.playerList.Add (gameObject);
	//	Debug.Log ("anotha one");
	//}

}
