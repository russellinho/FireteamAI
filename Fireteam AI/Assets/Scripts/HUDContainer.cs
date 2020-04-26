using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDContainer : MonoBehaviour {

	// Health HUD
	public Text healthText;
	public Slider staminaBar;
	public Image flashbangOverlay;
	public RawImage flashbangScreenCap;

	// Weapon HUD
	public Text weaponLabelTxt;
	public Text ammoTxt;
	public GameObject crosshair;
	public RawImage sightCrosshair;
	public GameObject SniperOverlay;

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
	public RawImage boostFlare;
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
	public Text deployInvalidText;
	public Text actionBarText;
	public GameObject actionBar;
	public Image[] actionBarImgs;
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
	public GameObject enemyAlerted;
	public Image detectionMeter;
	public RawImage detectionText;
	public GameObject waypointMarkers;
	public GameObject playerMarkers;
	public GameObject enemyMarkers;

    // Versus mode HUD
    public GameObject redScore;
    public GameObject blueScore;
    public Text redScoreTxt;
    public Text blueScoreTxt;

}
