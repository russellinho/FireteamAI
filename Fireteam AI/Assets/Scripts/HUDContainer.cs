using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDContainer : MonoBehaviour {

	// Health HUD
	public Text healthText;
	public Slider staminaBar;

	// Weapon HUD
	public Text weaponLabelTxt;
	public Text ammoTxt;

	// Pause/in-game menu HUD
	public GameObject pauseMenuGUI;
	public GameObject pauseExitBtn;
	public GameObject pauseResumeBtn;
	public GameObject pauseOptionsBtn;
	public Canvas scoreboard;
    public InGameMessenger inGameMessenger;

	// Hit indication HUD
	public RawImage hitFlare;
	public RawImage healFlare;
	public GameObject hitDir;
	public GameObject hitMarker;

	// Map HUD
	public RawImage hudMap;
	public RawImage hudMap2;
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
	public Text comBoxText;
	public GameObject comBox;
	public Slider respawnBar;
	public GameObject gameOverBanner;

}
