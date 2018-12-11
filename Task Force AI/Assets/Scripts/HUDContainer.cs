using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDContainer : MonoBehaviour {

	// Health HUD
	public Text healthText;

	// Weapon HUD
	public Text weaponLabelTxt;
	public Text ammoTxt;

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
	public GameObject healFlare;
	public GameObject hitDir;
	public GameObject hitMarker;

	// Map HUD
	public GameObject hudMap;
	public GameObject hudWaypoint;
	public GameObject hudPlayerMarker;

	// On-screen indication HUD
	public Text objectivesText;
	public GameObject missionText;
	public GameObject actionBar;
	public Text defusingText;
	public Text hintText;
	public Text spectatorText;
	public Text missionTimeText;
	public Text missionTimeRemainingText;
	public Text assaultModeIndText;
	public TextMeshProUGUI killPopupText;
	public Image screenColor;

}
