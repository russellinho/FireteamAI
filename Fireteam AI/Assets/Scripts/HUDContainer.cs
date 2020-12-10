using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Shift;

public class HUDContainer : MonoBehaviour {

	// Health HUD
	public CanvasGroup healthGroup;
	public CanvasGroup staminaGroup;
	public TextMeshProUGUI healthPercentTxt;
	public TextMeshProUGUI staminaPercentTxt;
	public Slider healthBar;
	public Slider staminaBar;
	public Image flashbangOverlay;
	public RawImage flashbangScreenCap;
	public CanvasGroup vipHealthGroup;
	public TextMeshProUGUI vipHealthPercentTxt;
	public Slider vipHealthBar;

	// Weapon HUD
	public UIManagerText weaponLabelTxt;
	public UIManagerText ammoTxt;
	public GameObject crosshair;
	public RawImage sightCrosshair;
	public GameObject SniperOverlay;
	public CanvasGroup itemCarryingGroup;
	public Text itemCarryingText;

	// Pause/in-game menu HUD
	public PauseMenuManager pauseMenuGUI;
	public GameObject scoreboard;
    public InGameMessenger inGameMessenger;
	public PauseMenuManager pauseMenuManager;
	public PauseMenuScript pauseMenuScript;

	// Hit indication HUD
	public RawImage hitFlare;
	public RawImage healFlare;
	public RawImage boostFlare;
	public GameObject hitDir;
	public GameObject hitMarker;

	// Map HUD
	public GameObject minimapGroup;
	public RawImage hudMap;
	public RawImage hudMap2;
	public GameObject hudWaypoint;
	public GameObject hudPlayerMarker;

	// On-screen indication HUD
	public GameObject objectivesTextParent;
	public GameObject objectiveTextEntry;
	public TextMeshProUGUI[] objectivesText;
	public GameObject missionText;
	public Text deployInvalidText;
	public TextMeshProUGUI actionBarText;
	public Slider actionBarSlider;
	public GameObject actionBar;
	public TextMeshProUGUI hintText;
	public Text spectatorText;
	public GameObject timeGroup;
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
	public Texture alertSymbol;
	public Texture suspiciousSymbol;
	public TextMeshProUGUI votePanel;
	public GameObject voteOptions;
	public GameObject voteResults;
	public TextMeshProUGUI voteTime;
	public TextMeshProUGUI yesVoteCount;
	public TextMeshProUGUI noVoteCount;
	public TextMeshProUGUI finalVoteResults;
	public VoiceChatEntryScript[] voiceChatEntries;
	public GameObject voiceCommandsPanel;
	public GameObject voiceCommandsReport;
	public GameObject voiceCommandsTactical;
	public GameObject voiceCommandsSocial;
	public VoiceCommandScript[] reportCommands;
	public VoiceCommandScript[] tacticalCommands;
	public VoiceCommandScript[] supportCommands;

    // Versus mode HUD
    public GameObject redScore;
    public GameObject blueScore;
    public TextMeshProUGUI redScoreTxt;
    public TextMeshProUGUI blueScoreTxt;
	public GameObject redTeamHighlight;
	public GameObject blueTeamHighlight;
	public GameObject redTeamUnderline;
	public GameObject blueTeamUnderline;

}
