using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHUDScript : MonoBehaviour {

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

    // Menu HUD

    // Hit indication HUD
    private GameObject hitFlare;
    private GameObject hitDir;

    // Map HUD
    public GameObject hudMap;
    public GameObject hudWaypoint;

    // On-screen indication HUD
    public Text objectivesText;
    public GameObject missionText;
    public GameObject actionBar;
    public Text defusingText;
    public Text hintText;

    // Use this for initialization
    void Start () {
        if (!GetComponent<PhotonView>().IsMine) {
            this.enabled = false;
        }

		hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");
		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;

		healthText = GameObject.Find ("HealthBar").GetComponent<Text>();
		weaponLabelTxt = GameObject.Find("WeaponLabel").GetComponent<Text>();
		ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();
        hudMap = GameObject.Find("HUDMap");
		hudMap.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
        healthText.text = HEALTH_TEXT + health;

		// Update UI
		weaponLabelTxt.text = currWep;
		ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;

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

	void FixedUpdate() {
		UpdateHitFlare();
	}

	void UpdateHitFlare() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		if (hitTimer < 1f) {
			//hitFlare.GetComponent<RawImage> ().enabled = true;
			//hitDir.GetComponent<RawImage> ().enabled = true;
			hitTimer += Time.deltaTime;
		} else {
			//hitFlare.GetComponent<RawImage> ().enabled = false;
			//hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward,hitLocation);
			//Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			//hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}
	}

    public void DisableHUD()
    {
        healthBar.GetComponent<Text>().enabled = false;
        weaponLabel.GetComponent<Text>().enabled = false;
        ammoCount.GetComponent<Text>().enabled = false;
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
        GameObject.Find("NetworkMan").GetComponent<NetworkManager>().StopHost();
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
        missionText.SetActive(true);
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
        objectivesText.text = objectiveFormatter.LoadObjectives(currentMap, bombsRemaining);
    }

    public void EscapePopup()
    {
        missionText.GetComponent<Text>().text = "Escape available! Head to the waypoint!";
        missionText.SetActive(true);
    }


}
