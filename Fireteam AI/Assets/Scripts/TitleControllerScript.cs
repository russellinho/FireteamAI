using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;
using TMPro;
using Firebase.Database;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Koobando.AntiCheat;
using Michsky.UI.Shift;
using Photon.Pun.LobbySystemPhoton;
using VivoxUnity;
using VivoxUnity.Common;
using VivoxUnity.Private;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class TitleControllerScript : MonoBehaviourPunCallbacks {
	private const float NINETY_DAYS_MINS = 129600f;
	private const float THIRTY_DAYS_MINS = 43200f;
	private const float SEVEN_DAYS_MINS = 10080f;
	private const float ONE_DAY_MINS = 1440f;
	private const float PERMANENT = -1f;
    private const float COST_MULT_FRACTION = 0.5f / 3f;
    private const float SEVEN_DAY_COST_MULTIPLIER = 7f * (1f - (COST_MULT_FRACTION * 1f));
    private const float THIRTY_DAY_COST_MULTIPLIER = 30f * (1f - (COST_MULT_FRACTION * 2f));
    private const float NINETY_DAY_COST_MULTIPLIER = 90f * (1f - (COST_MULT_FRACTION * 3f));
    private const float PERMANENT_COST_MULTIPLIER = 365f;
	public Connexion connexion;
	public FriendsMessenger friendsMessenger;
	private AudioSourceModifier[] sceneAudioSources;

	public GameObject itemDescriptionPopupRef;
	public Text versionText;
	public TextMeshProUGUI mainNametagTxt;
	public RawImage mainRankImg;
	public TextMeshProUGUI mainRankTxt;
	public Slider mainLevelProgress;
	public TextMeshProUGUI mainExpTxt;
	public Slider musicVolumeSlider;
	public Slider gameVolumeSlider;
	public Slider ambientVolumeSlider;
	public Slider voiceInputVolumeSlider;
	public Slider voiceOutputVolumeSlider;
	public TextMeshProUGUI musicVolumeField;
	public TextMeshProUGUI gameVolumeField;
	public TextMeshProUGUI ambientVolumeField;
	public TextMeshProUGUI voiceInputVolumeField;
	public TextMeshProUGUI voiceOutputVolumeField;
	public HorizontalSelector audioInputSelector;
	// Graphics settings
	public HorizontalSelector resolutionSelector;
	public HorizontalSelector qualitySelector;
	public HorizontalSelector vSyncSelector;
	public Slider lodBiasSlider;
	public HorizontalSelector antiAliasingSelector;
	public HorizontalSelector anisotropicFilteringSelector;
	public Slider masterTextureLimitSlider;
	public HorizontalSelector shadowCascadesSelector;
	public HorizontalSelector shadowResolutionSelector;
	public HorizontalSelector shadowSelector;
	public SwitchManager bloomSwitch;
	public SwitchManager motionBlurSwitch;
	public Slider brightnessSlider;

	public CanvasGroup loadingScreen;
	public CanvasGroup mainPanels;
	public Animator mainPanelsAnimator;
	public CreditsManager creditsManager;
	public GameObject glowMotes;
	public Animator backgroundAnimator;
	public Button creditsButton;
	public Button creditsExitButton;
	public ModalWindowManager alertPopup;
	public ModalWindowManager confirmPopup;
	public ModalWindowManager keyBindingsPopup;
	public ModalWindowManager makePurchasePopup;
	public ModalWindowManager roomPasswordPopup;
	public ModalWindowManager roomEnterPasswordPopup;
	public ModalWindowManager addFriendPopup;
	public BlurManager blurManager;
	public TMP_InputField roomPasswordInput;
	public TMP_InputField roomEnterPasswordInput;
	public TMP_InputField addFriendInput;
	// Block screen used for blocking player interaction with screen while something is going on in the backgronud (example: if a transaction is in progress)
	private bool blockScreenTrigger;
	public GameObject blockScreen;
	public GameObject blockBlur;
	private string itemBeingPurchased;
	private string typeBeingPurchased;
	private uint totalCostBeingPurchased;
	private char currencyTypeBeingPurchased;
	public bool confirmingTransaction;
	public bool confirmingSale;
	private bool resettingKeysFlag;
	private bool resettingGraphicsFlag;
	public char currentCharGender;
	private bool audioInputDevicesInitialized;

	// Loading screen stuff
	public RawImage screenArt;
	public GameObject screenArtContainer;
	public TextMeshProUGUI proTipText;
	public GameObject proTipContainer;
	public TextMeshProUGUI mapTitleText;
	public TextMeshProUGUI mapDescriptionText;
	private string[] proTips = new string[2]{"Aim for the head for faster kills.", "Be on the lookout for ammo and health drops from enemies."};
	private bool versionWarning;
	// Marketplace menu
	public Button clearPreviewBtn;
	public GameObject shopContentPrefab;
	public GameObject shopContentEquipment;
	public GameObject shopContentWeapons;
	public Button shopHeadgearBtn;
	public Button shopFaceBtn;
	public Button shopArmorBtn;
	public Button shopTopsBtn;
	public Button shopBottomsBtn;
	public Button shopFootwearBtn;
	public Button shopCharacterBtn;
	public Button shopPrimaryWepBtn;
	public Button shopSecondaryWepBtn;
	public Button shopSupportWepBtn;
	public Button shopMeleeWepBtn;
	public Button shopAssaultRifleSubBtn;
	public Button shopSmgSubBtn;
	public Button shopLmgSubBtn;
	public Button shopShotgunSubBtn;
	public Button shopSniperRifleSubBtn;
	public Button shopPistolSubBtn;
	public Button shopLauncherSubBtn;
	public Button shopExplosivesSubBtn;
	public Button shopBoostersSubBtn;
	public Button shopDeployablesSubBtn;
	public Button shopKnivesSubBtn;
	public Button shopModsBtn;
	public Button shopSuppressorsSubBtn;
	public Button shopSightsSubBtn;
	public Button modShopPrimaryWepBtn;
	public Button modShopSecondaryWepBtn;
	public Button modShopSupportWepBtn;
	public Button modShopMeleeWepBtn;
	public Button modShopSuppressorsBtn;
	public Button modShopSightsBtn;
	public HorizontalSelector durationSelection;
	public TextMeshProUGUI totalGpCostTxt;
	public TextMeshProUGUI myGpTxt;
	public TextMeshProUGUI myKashTxt;

	// Customization menu
	public GameObject contentPrefab;
	public GameObject contentInventoryEquipment;
	public GameObject contentInventoryWeapons;
	public Button headgearBtn;
	public Button faceBtn;
	public Button armorBtn;
	public Button topsBtn;
	public Button bottomsBtn;
	public Button footwearBtn;
	public Button characterBtn;
	public GameObject equippedHeadSlot;
	public GameObject equippedFaceSlot;
	public GameObject equippedTopSlot;
	public GameObject equippedBottomSlot;
	public GameObject equippedFootSlot;
	public GameObject equippedCharacterSlot;
	public GameObject equippedArmorSlot;
	public GameObject equippedSuppressorSlot;
	public GameObject equippedSightSlot;
	public GameObject currentlyEquippedWeaponPrefab;
	public GameObject currentlyEquippedEquipmentPrefab;
	public TextMeshProUGUI armorBoostPercent;
	public TextMeshProUGUI speedBoostPercent;
	public TextMeshProUGUI staminaBoostPercent;
	public Button primaryWepBtn;
	public Button secondaryWepBtn;
	public Button supportWepBtn;
	public Button meleeWepBtn;
	// Number of sub buttons for primary weapon dropdown in shop
	public Button assaultRifleSubBtn;
	public Button smgSubBtn;
	public Button lmgSubBtn;
	public Button shotgunSubBtn;
	public Button sniperRifleSubBtn;
	public Button pistolSubBtn;
	public Button launcherSubBtn;
	public Button explosivesSubBtn;
	public Button boostersSubBtn;
	public Button deployablesSubBtn;
	public Button knivesSubBtn;
	public GameObject equippedPrimarySlot;
	public GameObject equippedSecondarySlot;
	public GameObject equippedSupportSlot;
	public GameObject equippedMeleeSlot;

	// Mod menu
	public GameObject currentlyEquippedModPrefab;
	public GameObject weaponPreviewSlot;
	public GameObject weaponPreviewRef;
	public ShopItemScript weaponPreviewShopSlot;
	public GameObject modInventoryContent;
	public GameObject modWeaponInventoryContent;
	public Button suppressorsBtn;
	public Button sightsBtn;
	public TextMeshProUGUI modDamageTxt;
	public TextMeshProUGUI modAccuracyTxt;
	public TextMeshProUGUI modRecoilTxt;
	public TextMeshProUGUI modRangeTxt;
	public TextMeshProUGUI modClipCapacityTxt;
	public TextMeshProUGUI modMaxAmmoTxt;
    public Button removeSuppressorBtn;
	public Button removeSightBtn;
	public Dictionary<string, int> charactersRefsIndices = new Dictionary<string, int>(){["Lucas"] = 0, ["Daryl"] = 1, ["Yongjin"] = 2, ["Rocko"] = 3, ["Hana"] = 4, ["Jade"] = 5, ["Dani"] = 6, ["Codename Sayre"] = 7};
	public GameObject[] characterRefs;
	public KeyMappingInput[] keyMappingInputs;
	public bool triggerAlertPopupFlag;
	public bool triggerConfirmPopupFlag;
	public bool triggerKeyBindingsPopupFlag;
	public bool triggerMakePurchasePopupFlag;
	public bool triggerRoomPasswordChangePopupFlag;
	public bool triggerRoomPasswordEnterPopupFlag;
	public bool triggerAddFriendPopupFlag;
	public string alertPopupMessage;
	public string confirmPopupMessage;
	private string roomEnteringName;
	private bool confirmClicked;
	public MainPanelManager mainPanelManager;
	public Button previouslyPressedButtonLeft;
	public Button previouslyPressedSubButtonLeft;
	public Button previouslyPressedButtonRight;
	public Button previouslyPressedSubButtonRight;
	public GameObject primaryWeaponTabs;
	public GameObject secondaryWeaponTabs;
	public GameObject supportWeaponTabs;
	public GameObject meleeWeaponTabs;
	public GameObject marketplacePrimaryWeaponTabs;
	public GameObject marketplaceSecondaryWeaponTabs;
	public GameObject marketplaceSupportWeaponTabs;
	public GameObject marketplaceMeleeWeaponTabs;
	public GameObject marketplaceModsWeaponTabs;
	public GameObject modShopPrimaryWeaponTabs;
	public GameObject modShopSecondaryWeaponTabs;
	public GameObject modShopSupportWeaponTabs;
	public GameObject modShopMeleeWeaponTabs;
	public enum ConfirmType {KickPlayer};
	public ConfirmType confirmType;
	public GlobalChat chatManagerCamp;
	public GlobalChat chatManagerVersus;

	// Use this for initialization
	void Awake() {
		if (PlayerData.playerdata == null || PlayerData.playerdata.bodyReference == null) {
			InstantiateLoadingScreen(null, null);
			ToggleLoadingScreen(true);
		}
		musicVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f;
		musicVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.musicVolume;

		gameVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.gameVolume / 100f;
		gameVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.gameVolume;

		ambientVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.ambientVolume / 100f;
		ambientVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.ambientVolume;

		voiceInputVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.voiceInputVolume / 100f;
		voiceInputVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.voiceInputVolume;
		
		voiceOutputVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.voiceOutputVolume / 100f;
		voiceOutputVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.voiceOutputVolume;

		GetSceneAudioSources();
		InitializeGraphicSettings();
	}

	void Start () {
		versionText.text = "V: " + Application.version;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Safety destroy previous match data
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			if (entry.objRef != null) {
				Destroy(entry.objRef);
			}
		}
		GameControllerScript.playerList.Clear();
		// SetPlayerStatsForTitle();

		StartCoroutine (VersionNumberCheck());
	}

	IEnumerator VersionNumberCheck() {
		UnityWebRequest www = UnityWebRequest.Get("https://koobando-web.firebaseapp.com/versionCheck.txt");
		yield return www.SendWebRequest();

		if(www.isNetworkError || www.isHttpError) {
			Debug.Log("Error while getting version number: " + www.error);
		} else {
			// Show results as text
			string gameAppVersion = Application.version;
			string currentAppVersion = www.downloadHandler.text.Substring(0, 4);
			if (!currentAppVersion.Equals(gameAppVersion)) {
				versionWarning = true;
			} else {
				versionWarning = false;
			}
		}
	}

	void GetSceneAudioSources()
	{
		sceneAudioSources = (AudioSourceModifier[]) GameObject.FindObjectsOfType (typeof(AudioSourceModifier));
	}

	public void InstantiateLoadingScreen(string mapName, string mapDescription) {
		if (mapName != null) {
			JukeboxScript.jukebox.StopMusic();
			if (mapName.Equals ("The Badlands: Act I")) {
				screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/badlands1_load");
			} else if (mapName.Equals("The Badlands: Act II")) {
				screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/badlands2_load");
			}
			proTipText.text = proTips[Random.Range(0, 2)];
			mapTitleText.text = mapName;
			mapDescriptionText.text = mapDescription;
			screenArtContainer.gameObject.SetActive(true);
			proTipContainer.gameObject.SetActive(true);
			mapTitleText.gameObject.SetActive(true);
			mapDescriptionText.gameObject.SetActive(true);
		} else {
			screenArtContainer.gameObject.SetActive(false);
			proTipContainer.gameObject.SetActive(false);
			mapTitleText.gameObject.SetActive(false);
			mapDescriptionText.gameObject.SetActive(false);
		}
	}

	public void ToggleLoadingScreen(bool b) {
		if (b) {
			mainPanelsAnimator.enabled = false;
			mainPanels.alpha = 0f;
			loadingScreen.alpha = 1f;
		} else {
			JukeboxScript.jukebox.StartTitleMusic();
			loadingScreen.alpha = 0f;
			mainPanelsAnimator.enabled = true;
			// mainPanelsAnimator.Play("Start");
		}
	}

	void Update() {
		// if (loadingScreen.alpha == 1f && PlayerData.playerdata.bodyReference != null) {
		// 	ToggleLoadingScreen(false);
		// 	mainPanelManager.OpenFirstTab();
		// }

		ToggleBlockScreen(blockScreenTrigger);
		if (PlayerData.playerdata.disconnectedFromServer) {
			PlayerData.playerdata.disconnectedFromServer = false;
			TriggerAlertPopup("Lost connection to server.\nReason: " + PlayerData.playerdata.disconnectReason);
			PlayerData.playerdata.disconnectReason = "";
		} else if (versionWarning) {
			versionWarning = false;
			TriggerAlertPopup("Your game is not updated to the latest version of Fireteam AI!\nThis may affect your matchmaking experience.");
		}
		if (triggerAlertPopupFlag) {
			triggerAlertPopupFlag = false;
			alertPopup.SetText(alertPopupMessage);
			alertPopup.ModalWindowIn();
		}
		if (triggerConfirmPopupFlag) {
			triggerConfirmPopupFlag = false;
			confirmPopup.SetText(confirmPopupMessage);
			confirmPopup.ModalWindowIn();
		}
		if (triggerKeyBindingsPopupFlag) {
			triggerKeyBindingsPopupFlag = false;
			keyBindingsPopup.ModalWindowIn();
		}
		if (triggerMakePurchasePopupFlag) {
			triggerMakePurchasePopupFlag = false;
			makePurchasePopup.ModalWindowIn();
		}
		if (triggerRoomPasswordEnterPopupFlag) {
			triggerRoomPasswordEnterPopupFlag = false;
			roomEnterPasswordPopup.ModalWindowIn();
		}
		if (triggerRoomPasswordChangePopupFlag) {
			triggerRoomPasswordChangePopupFlag = false;
			roomPasswordPopup.ModalWindowIn();
		}
		if (triggerAddFriendPopupFlag) {
			triggerAddFriendPopupFlag = false;
			addFriendPopup.ModalWindowIn();
		}
		if (alertPopup.isOn || confirmPopup.isOn || keyBindingsPopup.isOn || makePurchasePopup.isOn && (PlayerData.playerdata.bodyReference != null && PlayerData.playerdata.bodyReference.activeInHierarchy)) {
			HideAll(false);
		}
	}

	public void OnMusicVolumeSliderChanged() {
		SetMusicVolume(musicVolumeSlider.value);
	}

	public void OnGameVolumeSliderChanged() {
		SetGameVolume(gameVolumeSlider.value);
	}

	public void OnAmbientVolumeSliderChanged() {
		SetAmbientVolume(ambientVolumeSlider.value);
	}

	public void OnVoiceInputVolumeChanged() {
		SetVoiceInputVolume(voiceInputVolumeSlider.value);
	}

	public void OnVoiceOutputVolumeChanged() {
		SetVoiceOutputVolume(voiceOutputVolumeSlider.value);
	}

	void SetMusicVolume(float v) {
		musicVolumeField.text = ""+(int)(v * 100f);
		JukeboxScript.jukebox.SetMusicVolume(v);
	}

	void SetGameVolume(float v) {
		gameVolumeField.text = ""+(int)(v * 100f);
	}

	void SetAmbientVolume(float v) {
		ambientVolumeField.text = ""+(int)(v * 100f);
	}

	void SetVoiceInputVolume(float v) {
		voiceInputVolumeField.text = ""+(int)(v * 100f);
		if (VivoxVoiceManager.Instance != null) {
			VivoxVoiceManager.Instance.AudioInputDevices.VolumeAdjustment = ((int)(v * 100f) - 50);
		}
	}

	void SetVoiceOutputVolume(float v) {
		voiceOutputVolumeField.text = ""+(int)(v * 100f);
		if (VivoxVoiceManager.Instance != null) {
			VivoxVoiceManager.Instance.AudioOutputDevices.VolumeAdjustment = ((int)(v * 100f) - 50);
		}
	}

	void UpdateAllAudioSources()
	{
		foreach (AudioSourceModifier a in sceneAudioSources)
		{
			a.SetVolume();
		}
	}

	public void SaveSettings() {
		PlayerPreferences.playerPreferences.preferenceData.musicVolume = (int)(musicVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.gameVolume = (int)(gameVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.ambientVolume = (int)(ambientVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.audioInputName = audioInputSelector.GetCurrentItem();
		PlayerPreferences.playerPreferences.preferenceData.voiceInputVolume = (int)(voiceInputVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.voiceOutputVolume = (int)(voiceOutputVolumeSlider.value * 100f);
		UpdateAllAudioSources();
		PlayerPreferences.playerPreferences.SavePreferences();
	}

	public void SaveKeyBindings() {
		PlayerPreferences.playerPreferences.SaveKeyMappings();
	}

	public void RefreshSavedAudioDevice()
    {
		if (!audioInputDevicesInitialized) {
			audioInputSelector.ClearItems();
			audioInputSelector.CreateNewItem("None");
		}
		bool audioDeviceValid = false;
        string currentAudioDevice = PlayerPreferences.playerPreferences.preferenceData.audioInputName;
        foreach (IAudioDevice p in VivoxVoiceManager.Instance.AudioInputDevices.AvailableDevices) {
			if (!audioInputDevicesInitialized) {
				audioInputSelector.CreateNewItem(p.Name);
			}
            if (p.Name == currentAudioDevice) {
				audioDeviceValid = true;
            }
        }
        // Set to default if not found
		if (!audioDeviceValid) {
        	PlayerPreferences.playerPreferences.preferenceData.audioInputName = "None";
		}
		SetAudioDevice(PlayerPreferences.playerPreferences.preferenceData.audioInputName);
		audioInputDevicesInitialized = true;
    }

    void SetAudioDevice(string deviceName)
    {
		audioInputSelector.SetSelector(deviceName);
    }

	public void SetDefaultAudioSettings() {
		musicVolumeSlider.value = (float)JukeboxScript.DEFAULT_MUSIC_VOLUME / 100f;
		gameVolumeSlider.value = 1f;
		ambientVolumeSlider.value = 1f;
		voiceInputVolumeSlider.value = 0.5f;
		voiceOutputVolumeSlider.value = 0.5f;
		SetMusicVolume(musicVolumeSlider.value);
		SetGameVolume(gameVolumeSlider.value);
		SetAmbientVolume(ambientVolumeSlider.value);
		SetVoiceInputVolume(0.5f);
		SetVoiceOutputVolume(0.5f);
	}

	public void ClearPreview() {
		PlayerData.playerdata.InstantiatePlayer();
	}

    public void ExitMatchmaking() {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

	public void JoinMatchmaking() {
		if (!PhotonNetwork.IsConnectedAndReady) {
			PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.info.Playername;
			Hashtable h = new Hashtable();
			h.Add("exp", (int)PlayerData.playerdata.info.Exp);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
			PhotonNetwork.ConnectUsingSettings();
		}
	}

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
		// titleText.enabled = true;
        // mainMenu.SetActive(true);
    }

	public void goToMainMenu (){
		PlayerPrefs.SetString ("newScene", "MainMenu");
		SceneManager.LoadScene(7);
	}

	public void quitGame() {
		Dictionary<string, object> inputData = new Dictionary<string, object>();
		inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
		inputData["loggedIn"] = "0";

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("setUserIsLoggedIn");
		func.CallAsync(inputData).ContinueWith((task) => {
			Application.Quit ();
		});
	}

    public void TriggerExpirationPopup(List<object> expiredItems)
    {
        TriggerAlertPopup("The following items have expired and have been deleted from your inventory:\n" + string.Join(", ", expiredItems.ToArray()));
    }

	public void TriggerAlertPopup(string message) {
		triggerAlertPopupFlag = true;
		alertPopupMessage = message;
	}

	public void TriggerRoomPasswordChangePopup()
	{
		triggerRoomPasswordChangePopupFlag = true;
	}

	public void TriggerAddFriendPopup()
	{
		triggerAddFriendPopupFlag = true;
	}

	public void TriggerRoomPasswordEnterPopup(string roomName)
	{
		triggerRoomPasswordEnterPopupFlag = true;
		roomEnteringName = roomName;
	}

	public void TriggerConfirmPopup(string message) {
		triggerConfirmPopupFlag = true;
		confirmPopupMessage = message;
	}

	public void TriggerKeyBindingsPopup() {
		triggerKeyBindingsPopupFlag = true;
	}

	public void TriggerMakePurchasePopup() {
		triggerMakePurchasePopupFlag = true;
	}

	// Clears existing items from the shop panel
	void ClearCustomizationContent(char type) {
		if (type == 'e') {
			RawImage[] existingThumbnails = contentInventoryEquipment.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				currentlyEquippedEquipmentPrefab = null;
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		} else if (type == 'w') {
			RawImage[] existingThumbnails = contentInventoryWeapons.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				currentlyEquippedWeaponPrefab = null;
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		}
	}

	void ClearMarketplaceContent(char type) {
		if (type == 'e') {
			RawImage[] existingThumbnails = shopContentEquipment.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		} else if (type == 'w') {
			RawImage[] existingThumbnails = shopContentWeapons.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		}
	}

	// Clears existing items from the mod shop panel
	void ClearModCustomizationContent(char type) {
		if (type == 'm') {
			RawImage[] existingThumbnails = modInventoryContent.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				currentlyEquippedModPrefab = null;
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		} else if (type == 'w') {
			RawImage[] existingThumbnails = modWeaponInventoryContent.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				currentlyEquippedModPrefab = null;
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		}
	}

	public void OnHeadBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.inventory.myHeadgear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = entry.Key;
			Equipment thisHeadgear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisHeadgear.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisHeadgear;
			s.itemName = thisItemName;
            s.itemType = "Headgear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisHeadgear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisHeadgear.thumbnailPath);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedHeadgear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceHeadBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Equipment> entry in InventoryScript.itemData.equipmentCatalog) {
			Equipment thisHeadgear = entry.Value;
			if (!thisHeadgear.category.Equals("Headgear") || !thisHeadgear.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisHeadgear;
			s.itemName = entry.Key;
            s.itemType = "Headgear";
			s.itemDescription = thisHeadgear.description;
			if (thisHeadgear.gpPrice == 0) {
				s.priceTxt.text = ""+thisHeadgear.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisHeadgear.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisHeadgear.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnFaceBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.inventory.myFacewear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = entry.Key;
			Equipment thisFacewear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisFacewear.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFacewear;
			s.itemName = thisItemName;
            s.itemType = "Facewear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisFacewear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFacewear.thumbnailPath);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFacewear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceFaceBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Equipment> entry in InventoryScript.itemData.equipmentCatalog) {
			Equipment thisFacewear = entry.Value;
			if (!thisFacewear.category.Equals("Facewear") || !thisFacewear.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFacewear;
			s.itemName = entry.Key;
            s.itemType = "Facewear";
			s.itemDescription = thisFacewear.description;
			if (thisFacewear.gpPrice == 0) {
				s.priceTxt.text = ""+thisFacewear.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisFacewear.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFacewear.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnArmorBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, ArmorData> entry in PlayerData.playerdata.inventory.myArmor)
        {
            ArmorData ed = entry.Value;
			string thisItemName = entry.Key;
			Armor thisArmor = InventoryScript.itemData.armorCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisArmor.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.armorDetails = thisArmor;
			s.itemName = thisItemName;
            s.itemType = "Armor";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisArmor.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisArmor.thumbnailPath);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedArmor)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceArmorBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Armor> entry in InventoryScript.itemData.armorCatalog) {
			Armor thisArmor = entry.Value;
			if (!thisArmor.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.armorDetails = thisArmor;
			s.itemName = entry.Key;
            s.itemType = "Armor";
			s.itemDescription = thisArmor.description;
			if (thisArmor.gpPrice == 0) {
				s.priceTxt.text = ""+thisArmor.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisArmor.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisArmor.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnTopsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.inventory.myTops)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = entry.Key;
			Equipment thisTop = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisTop.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisTop;
            s.itemName = thisItemName;
            s.itemType = "Top";
			s.itemDescription = thisTop.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedTop)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceTopsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Equipment> entry in InventoryScript.itemData.equipmentCatalog) {
			Equipment thisEquipment = entry.Value;
			if (!thisEquipment.category.Equals("Top") || !thisEquipment.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisEquipment;
			s.itemName = entry.Key;
            s.itemType = "Top";
			s.itemDescription = thisEquipment.description;
			if (thisEquipment.gpPrice == 0) {
				s.priceTxt.text = ""+thisEquipment.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisEquipment.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisEquipment.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnBottomsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.inventory.myBottoms)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = entry.Key;
			Equipment thisBottom = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisBottom.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisBottom;
			s.itemName = thisItemName;
            s.itemType = "Bottom";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisBottom.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedBottom)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceBottomsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Equipment> entry in InventoryScript.itemData.equipmentCatalog) {
			Equipment thisBottom = entry.Value;
			if (!thisBottom.category.Equals("Bottom") || !thisBottom.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisBottom;
			s.itemName = entry.Key;
            s.itemType = "Bottom";
			s.itemDescription = thisBottom.description;
			if (thisBottom.gpPrice == 0) {
				s.priceTxt.text = ""+thisBottom.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisBottom.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisBottom.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnFootwearBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.inventory.myFootwear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = entry.Key;
			Equipment thisFootwear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(thisFootwear.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFootwear;
			s.itemName = thisItemName;
            s.itemType = "Footwear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisFootwear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFootwear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceFootwearBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Equipment> entry in InventoryScript.itemData.equipmentCatalog) {
			Equipment thisFootwear = entry.Value;
			if (!thisFootwear.category.Equals("Footwear") || !thisFootwear.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFootwear;
			s.itemName = entry.Key;
            s.itemType = "Footwear";
			s.itemDescription = thisFootwear.description;
			if (thisFootwear.gpPrice == 0) {
				s.priceTxt.text = ""+thisFootwear.kashPrice + " KASH";
			} else {
				s.priceTxt.text = ""+thisFootwear.gpPrice + " GP";
			}
			s.SetItemForMarket();
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFootwear.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnPrimaryWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('w');

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.type.Equals("Primary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplacePrimaryWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('w');

		// Populate into grid layout
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.type.Equals("Primary") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopPrimaryWepBtnClicked(bool first = false) {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.type.Equals("Primary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (first) {
				LoadWeaponForModding(s);
				first = false;
			}
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnSecondaryWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('w');

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.type.Equals("Secondary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceSecondaryWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('w');

		// Populate into grid layout
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.type.Equals("Secondary") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopSecondaryWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.type.Equals("Secondary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnSupportWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('w');

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.type.Equals("Support")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceSupportWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('w');

		// Populate into grid layout
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.type.Equals("Support") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopSupportWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.type.Equals("Support")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnMeleeWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('w');

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.type.Equals("Melee")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedMeleeWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceMeleeWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('w');

		// Populate into grid layout
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.type.Equals("Melee") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopMeleeWepBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.type.Equals("Primary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnAssaultRifleSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with assault rifles
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Assault Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceAssaultRifleSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with assault rifles
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Assault Rifle") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopAssaultRifleSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Assault Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnSmgSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with SMGs
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("SMG")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceSmgSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with assault rifles
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("SMG") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopSmgSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("SMG")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnLmgSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with SMGs
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("LMG")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceLmgSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with assault rifles
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("LMG") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopLmgSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("LMG")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnShotgunSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with shotguns
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Shotgun")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceShotgunSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with shotguns
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Shotgun") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopShotgunSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Shotgun")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnSniperRifleSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with sniper rifles
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Sniper Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceSniperRifleSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with sniper rifles
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Sniper Rifle") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopSniperRifleSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Sniper Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnPistolSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Pistol")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplacePistolSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Pistol") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopPistolSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Pistol")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnLaunchersSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Launcher")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceLaunchersSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Launcher") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopLaunchersSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Launcher")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnExplosivesSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Explosive")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceExplosivesSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Explosive") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopExplosivesSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Explosive")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnBoostersSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Booster")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceBoostersSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Booster") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopBoostersSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Booster")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnDeployablesSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Deployable")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceDeployablesSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Deployable") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopDeployablesSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Deployable")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnKnivesSubBtnClicked() {
		// Clear items
		ClearCustomizationContent('w');

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.category.Equals("Knife")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(w.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedMeleeWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedWeaponPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform, false);
		}
	}

	public void OnMarketplaceKnivesSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Weapon> entry in InventoryScript.itemData.weaponCatalog) {
			Weapon w = entry.Value;
			if (!w.category.Equals("Knife") || !w.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			if (w.gpPrice == 0) {
				s.priceTxt.text = "" + w.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + w.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnModShopKnivesSubBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('w');

		// Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[thisWeaponName];
			if (!w.canBeModded || !w.category.Equals("Knife")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.SetItemForModShop();
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			if (weaponPreviewShopSlot.itemName == thisWeaponName) {
				s.ToggleWeaponPreviewIndicator(true);
			}
			o.transform.SetParent(modWeaponInventoryContent.transform, false);
		}
	}

	public void OnMarketplaceModsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('w');

		// Populate into grid layout
		foreach(KeyValuePair<string, Mod> entry in InventoryScript.itemData.modCatalog) {
			Mod m = entry.Value;
			if (!m.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef= itemDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
			if (m.gpPrice == 0) {
				s.priceTxt.text = "" + m.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + m.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnMarketplaceSuppressorsSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Mod> entry in InventoryScript.itemData.modCatalog) {
			Mod m = entry.Value;
			if (!m.category.Equals("Suppressor") || !m.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
			if (m.gpPrice == 0) {
				s.priceTxt.text = "" + m.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + m.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnMarketplaceSightsSubBtnClicked() {
		// Clear items
		ClearMarketplaceContent('w');

		// Populate with pistols
		foreach(KeyValuePair<string, Mod> entry in InventoryScript.itemData.modCatalog) {
			Mod m = entry.Value;
			if (!m.category.Equals("Sight") || !m.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
			if (m.gpPrice == 0) {
				s.priceTxt.text = "" + m.kashPrice + " KASH";
			} else {
            	s.priceTxt.text = "" + m.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			o.transform.SetParent(shopContentWeapons.transform, false);
		}
	}

	public void OnCharacterBtnClicked() {
		// Delete any currently existing items in the grid
		ClearCustomizationContent('e');

        // Populate into grid layout
        foreach (KeyValuePair<string, CharacterData> entry in PlayerData.playerdata.inventory.myCharacters)
        {
            CharacterData ed = entry.Value;
			string thisCharacterName = entry.Key;
			Character c = InventoryScript.itemData.characterCatalog[thisCharacterName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.SetItemForLoadout(c.deleteable);
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.characterDetails = c;
			s.itemName = thisCharacterName;
            s.itemType = "Character";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = c.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			if (thisCharacterName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedEquipmentPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform, false);
		}
	}

	public void OnMarketplaceCharacterBtnClicked() {
		// Delete any currently existing items in the grid
		ClearMarketplaceContent('e');

		// Populate into grid layout
		foreach(KeyValuePair<string, Character> entry in InventoryScript.itemData.characterCatalog) {
			Character c = entry.Value;
			if (!c.purchasable) {
				continue;
			}
			GameObject o = Instantiate(shopContentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.characterDetails = c;
			s.itemName = entry.Key;
            s.itemType = "Character";
			s.itemDescription = c.description;
			if (c.gpPrice == 0) {
            	s.priceTxt.text = "" + c.kashPrice + " KASH";
			} else {
				s.priceTxt.text = "" + c.gpPrice + " GP";
			}
			s.SetItemForMarket();
            s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			o.transform.SetParent(shopContentEquipment.transform, false);
		}
	}

	public void OnSuppressorsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('m');
		WeaponScript ws = PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>();

        // Populate into grid layout
        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.inventory.myMods)
        {
            ModData modData = entry.Value;
			string thisModName = modData.Name;
			Mod m = InventoryScript.itemData.modCatalog[thisModName];
			if (!m.category.Equals("Suppressor")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.modDetails = m;
			s.SetItemForModShop();
			s.id = entry.Key;
			s.equippedOn = modData.EquippedOn;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.modCategory = m.category;
			s.acquireDate = modData.AcquireDate;
			s.duration = modData.Duration;
			s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			if (weaponPreviewShopSlot.itemName.Equals(modData.EquippedOn)) {
				s.ToggleModEquippedIndicator(true);
				currentlyEquippedModPrefab = o;
			}
			o.transform.SetParent(modInventoryContent.transform, false);
		}
	}

	public void OnSightsBtnClicked() {
		// Delete any currently existing items in the grid
		ClearModCustomizationContent('m');
		WeaponScript ws = PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>();

        // Populate into grid layout
        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.inventory.myMods)
        {
            ModData modData = entry.Value;
			string thisModName = modData.Name;
			Mod m = InventoryScript.itemData.modCatalog[thisModName];
			if (!m.category.Equals("Sight")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.modDetails = m;
			s.SetItemForModShop();
			s.id = entry.Key;
			s.equippedOn = modData.EquippedOn;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.modCategory = m.category;
			s.acquireDate = modData.AcquireDate;
			s.duration = modData.Duration;
			s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			if (weaponPreviewShopSlot.itemName.Equals(modData.EquippedOn)) {
				s.ToggleModEquippedIndicator(true);
				currentlyEquippedModPrefab = o;
			}
			o.transform.SetParent(modInventoryContent.transform, false);
		}
	}

	public void RemoveItemFromShopContent(char shopType, string itemId)
	{
		if (shopType == 'c') {
			ShopItemScript[] currentlyDisplayedContent = contentInventoryEquipment.GetComponentsInChildren<ShopItemScript>();
			if (itemId == currentlyEquippedEquipmentPrefab?.GetComponent<ShopItemScript>().itemName) {
				foreach (ShopItemScript s in currentlyDisplayedContent) {
					if (s.itemName == PlayerData.playerdata.info.DefaultChar) {
						s.ToggleEquippedIndicator(true);
						currentlyEquippedEquipmentPrefab = s.gameObject;
						break;
					}
				}
			}
			foreach (ShopItemScript s in currentlyDisplayedContent) {
				if (s.itemName == itemId) {
					GameObject.Destroy(s.gameObject);
					break;
				}
			}
		} else if (shopType == 'e') {
			ShopItemScript[] currentlyDisplayedContent = contentInventoryEquipment.GetComponentsInChildren<ShopItemScript>();
			if (itemId == currentlyEquippedEquipmentPrefab?.GetComponent<ShopItemScript>().itemName) {
				Character myChar = InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter];
				string myDefaultTop = myChar.defaultTop;
				string myDefaultBottom = myChar.defaultBottom;
				string myDefaultFootwear = (myChar.gender == 'M' ? PlayerData.DEFAULT_FOOTWEAR_MALE : PlayerData.DEFAULT_FOOTWEAR_FEMALE);
				foreach (ShopItemScript s in currentlyDisplayedContent) {
					if (s.itemName == myDefaultTop || s.itemName == myDefaultBottom || s.itemName == myDefaultFootwear) {
						s.ToggleEquippedIndicator(true);
						currentlyEquippedEquipmentPrefab = s.gameObject;
						break;
					}
				}
			}
			foreach (ShopItemScript s in currentlyDisplayedContent) {
				if (s.itemName == itemId) {
					GameObject.Destroy(s.gameObject);
					break;
				}
			}
		} else if (shopType == 'w') {
			ShopItemScript[] currentlyDisplayedContent = contentInventoryWeapons.GetComponentsInChildren<ShopItemScript>();
			if (itemId == currentlyEquippedWeaponPrefab?.GetComponent<ShopItemScript>().itemName) {
				foreach (ShopItemScript s in currentlyDisplayedContent) {
					if (s.itemName == PlayerData.playerdata.info.DefaultWeapon || s.itemName == PlayerData.DEFAULT_SECONDARY ||
						s.itemName == PlayerData.DEFAULT_SUPPORT || s.itemName == PlayerData.DEFAULT_MELEE) {
						s.ToggleEquippedIndicator(true);
						currentlyEquippedWeaponPrefab = s.gameObject;
						break;
					}
				}
			}
			foreach (ShopItemScript s in currentlyDisplayedContent) {
				if (s.itemName == itemId) {
					GameObject.Destroy(s.gameObject);
					break;
				}
			}
		} else if (shopType == 'm') {
			ShopItemScript[] currentlyDisplayedContent = modInventoryContent.GetComponentsInChildren<ShopItemScript>();
			foreach (ShopItemScript s in currentlyDisplayedContent) {
				if (s.id == itemId) {
					GameObject.Destroy(s.gameObject);
					break;
				}
			}
		}
	}

	public void RefreshModShopContent()
	{
		ShopItemScript[] currentlyDisplayedContent = modInventoryContent.GetComponentsInChildren<ShopItemScript>();
		foreach (ShopItemScript s in currentlyDisplayedContent) {
			ModData thisModData = PlayerData.playerdata.inventory.myMods[s.id];
			s.equippedOn = thisModData.EquippedOn;
			s.duration = thisModData.Duration;
			s.acquireDate = thisModData.AcquireDate;
		}
	}

	public void OnRemoveArmorClicked() {
		PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().RemoveArmor();
	}

	public void OnRemoveHeadgearClicked() {
		PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().RemoveHeadgear();
	}

	public void OnRemoveFacewearClicked() {
		PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().RemoveFacewear();
	}

	public void SetArmorBoostPercent(int armor) {
		if (armor == 0) {
			armorBoostPercent.color = Color.white;
			armorBoostPercent.text = "+" + armor + "%";
		} else if (armor > 0) {
			armorBoostPercent.color = Color.green;
			armorBoostPercent.text = "+" + armor + "%";
		} else {
			armorBoostPercent.color = Color.red;
			armorBoostPercent.text = armor + "%";
		}
	}

	public void SetSpeedBoostPercent(int speed) {
		if (speed == 0) {
			speedBoostPercent.color = Color.white;
			speedBoostPercent.text = "+" + speed + "%";
		} else if (speed > 0) {
			speedBoostPercent.color = Color.green;
			speedBoostPercent.text = "+" + speed + "%";
		} else {
			speedBoostPercent.color = Color.red;
			speedBoostPercent.text = speed + "%";
		}
	}

	public void SetStaminaBoostPercent(int stamina) {
		if (stamina == 0) {
			staminaBoostPercent.color = Color.white;
			staminaBoostPercent.text = "+" + stamina + "%";
		} else if (stamina > 0) {
			staminaBoostPercent.color = Color.green;
			staminaBoostPercent.text = "+" + stamina + "%";
		} else {
			staminaBoostPercent.color = Color.red;
			staminaBoostPercent.text = stamina + "%";
		}
	}

	public void SetStatBoosts(int armor, int speed, int stamina) {
		SetArmorBoostPercent(armor);
		SetSpeedBoostPercent(speed);
		SetStaminaBoostPercent(stamina);
	}

	public void LoadWeaponForModding(ShopItemScript s) {
		// if (weaponPreviewShopSlot != null) {
		// 	SaveModsForCurrentWeapon();
		// }

		// Destroy old weapon preview
		DestroyOldWeaponTemplate();
		// Load the proper weapon modding template
		GameObject t = (GameObject)Instantiate(Resources.Load("WeaponTemplates/" + s.itemName));

		// Place the weapon template in the proper position
		t.transform.SetParent(weaponPreviewSlot.transform);
		t.transform.localPosition = Vector3.zero;
		t.transform.localRotation = Quaternion.identity;

		weaponPreviewRef = t;
		weaponPreviewShopSlot = s;
		weaponPreviewShopSlot.itemName = s.itemName;
		s.ToggleWeaponPreviewIndicator(true);

		// Set base stats
		// Place the saved mods for that weapon back on the weapon template
		ModInfo savedModInfo = PlayerData.playerdata.LoadModDataForWeapon(s.itemName);
		SetWeaponModValues(s.itemName, true, null, savedModInfo.SuppressorId, true, null, savedModInfo.SightId);
		EquipModOnWeaponTemplate(savedModInfo.EquippedSuppressor, "Suppressor", savedModInfo.SuppressorId, null);
		EquipModOnWeaponTemplate(savedModInfo.EquippedSight, "Sight", savedModInfo.SightId, null);

		// Update shop items with the mods that are equipped
		ShopItemScript[] shopItems = modInventoryContent.GetComponentsInChildren<ShopItemScript>();
		foreach (ShopItemScript si in shopItems) {
			if (si.id == savedModInfo.SuppressorId || si.id == savedModInfo.SightId) {
				si.ToggleModEquippedIndicator(true);
				currentlyEquippedModPrefab = si.gameObject;
			} else {
				si.ToggleModEquippedIndicator(false);
			}
		}
	}

	public void SetWeaponModValues(string weaponName, bool updateSuppressor, string suppressorName, string suppressorId, bool updateSight, string sightName, string sightId) {
		// Set base stats
		Weapon w = InventoryScript.itemData.weaponCatalog[weaponName];
		float totalDamage = w.damage;
		float totalAccuracy = w.accuracy;
		float totalRecoil = w.recoil;
		float totalRange = w.range;
		int totalClipCapacity = w.clipCapacity;
		int totalMaxAmmo = w.maxAmmo;
		float damageBoost = 0f;
		float accuracyBoost = 0f;
		float recoilBoost = 0f;
		float rangeBoost = 0f;
		int clipCapacityBoost = 0;
		int maxAmmoBoost = 0;
		
		// modWeaponLbl.text = weaponName;

		if (updateSuppressor) {

			// Add suppressor stats
			if (!string.IsNullOrEmpty(suppressorName)) {
				Mod suppressor = InventoryScript.itemData.modCatalog[suppressorName];
				damageBoost += suppressor.damageBoost;
				accuracyBoost += suppressor.accuracyBoost;
				recoilBoost += suppressor.recoilBoost;
				rangeBoost += suppressor.rangeBoost;
				clipCapacityBoost += suppressor.clipCapacityBoost;
				maxAmmoBoost += suppressor.maxAmmoBoost;
			}
		}

		if (updateSight) {

			// Add sight stats
			if (!string.IsNullOrEmpty(sightName)) {
				Mod sight = InventoryScript.itemData.modCatalog[sightName];
				damageBoost += sight.damageBoost;
				accuracyBoost += sight.accuracyBoost;
				recoilBoost += sight.recoilBoost;
				rangeBoost += sight.rangeBoost;
				clipCapacityBoost += sight.clipCapacityBoost;
				maxAmmoBoost += sight.maxAmmoBoost;
			}
		}

		totalDamage += damageBoost;
		totalAccuracy += accuracyBoost;
		totalRecoil += recoilBoost;
		totalRange += rangeBoost;
		totalClipCapacity += clipCapacityBoost;
		totalMaxAmmo += maxAmmoBoost;

		SetWeaponModdedStats(totalDamage, totalAccuracy, totalRecoil, totalRange, totalClipCapacity, totalMaxAmmo);
		SetWeaponModdedStatsTextColor(damageBoost, accuracyBoost, recoilBoost, rangeBoost, clipCapacityBoost, maxAmmoBoost);
	}

	private void SetWeaponModdedStats(float damage, float accuracy, float recoil, float range, float clipCapacity, float maxAmmo) {
		modDamageTxt.text = damage != -1 ? ""+damage : "-";
		modAccuracyTxt.text = accuracy != -1 ? ""+accuracy : "-";
		modRecoilTxt.text = recoil != -1 ? ""+recoil : "-";
		modRangeTxt.text = range != -1 ? ""+range : "-";
		modClipCapacityTxt.text = clipCapacity != -1 ? ""+clipCapacity : "-";
		modMaxAmmoTxt.text = maxAmmo != -1 ? ""+maxAmmo : "-";
	}

	public void OnRemoveSuppressorClicked()
    {
        if (string.IsNullOrEmpty(weaponPreviewShopSlot.itemName) || string.IsNullOrEmpty(PlayerData.playerdata.LoadModDataForWeapon(weaponPreviewShopSlot.itemName).SuppressorId))
        {
            return;
        }
        
		PlayerData.playerdata.SaveModDataForWeapon(weaponPreviewShopSlot.itemName, "", null);
    }

    public void OnRemoveSightClicked() {
        if (string.IsNullOrEmpty(weaponPreviewShopSlot.itemName) || string.IsNullOrEmpty(PlayerData.playerdata.LoadModDataForWeapon(weaponPreviewShopSlot.itemName).SightId)) {
            return;
        }
        
		PlayerData.playerdata.SaveModDataForWeapon(weaponPreviewShopSlot.itemName, null, "");
    }

	private void SetWeaponModdedStatsTextColor(float damage, float accuracy, float recoil, float range, float clipCapacity, float maxAmmo) {
		if (damage > 0) {
			modDamageTxt.color = Color.green;
		} else if (damage < 0) {
			modDamageTxt.color = Color.red;
		} else {
			modDamageTxt.color = Color.white;
		}

		if (accuracy > 0) {
			modAccuracyTxt.color = Color.green;
		} else if (accuracy < 0) {
			modAccuracyTxt.color = Color.red;
		} else {
			modAccuracyTxt.color = Color.white;
		}

		if (recoil < 0) {
			modRecoilTxt.color = Color.green;
		} else if (recoil > 0) {
			modRecoilTxt.color = Color.red;
		} else {
			modRecoilTxt.color = Color.white;
		}

		if (range > 0) {
			modRangeTxt.color = Color.green;
		} else if (range < 0) {
			modRangeTxt.color = Color.red;
		} else {
			modRangeTxt.color = Color.white;
		}

		if (clipCapacity > 0) {
			modClipCapacityTxt.color = Color.green;
		} else if (clipCapacity < 0) {
			modClipCapacityTxt.color = Color.red;
		} else {
			modClipCapacityTxt.color = Color.white;
		}

		if (maxAmmo > 0) {
			modMaxAmmoTxt.color = Color.green;
		} else if (maxAmmo < 0) {
			modMaxAmmoTxt.color = Color.red;
		} else {
			modMaxAmmoTxt.color = Color.white;
		}
	}

	public void DestroyOldWeaponTemplate() {
		// Destroy a weapon that is currently in the modding slot to make way for a new one
		if (weaponPreviewRef != null) {
			Destroy(weaponPreviewRef);
			if (weaponPreviewShopSlot != null) {
				weaponPreviewShopSlot.ToggleWeaponPreviewIndicator(false);
			}
			weaponPreviewRef = null;
			weaponPreviewShopSlot = null;
		}
	}

	public string EquipModOnWeaponTemplate(string modName, string modType, string modId, ShopItemScript itemSlot) {
		Weapon w = null;
		switch(modType) {
			case "Suppressor":
				if (modName == null || modName.Equals("")) {
					equippedSuppressorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
					return weaponPreviewShopSlot.itemName;
				}
				w = InventoryScript.itemData.weaponCatalog[weaponPreviewShopSlot.itemName];
				if (w.suppressorCompatible) {
					SetWeaponModValues(weaponPreviewShopSlot.itemName, true, modName, modId, false, null, null);
					weaponPreviewRef.GetComponent<WeaponMods>().EquipSuppressor(modName);
					equippedSuppressorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, InventoryScript.itemData.modCatalog[modName].thumbnailPath);
					if (itemSlot != null) {
						currentlyEquippedModPrefab = itemSlot.gameObject;
					}
					return weaponPreviewShopSlot.itemName;
				} else {
					TriggerAlertPopup("Suppressors cannot be equipped on this weapon!");
					return "0";
				}
			case "Sight":
				if (modName == null || modName.Equals("")) {
					equippedSightSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
					return weaponPreviewShopSlot.itemName;
				}
				w = InventoryScript.itemData.weaponCatalog[weaponPreviewShopSlot.itemName];
				if (w.sightCompatible) {
					SetWeaponModValues(weaponPreviewShopSlot.itemName, false, null, null, true, modName, modId);
					weaponPreviewRef.GetComponent<WeaponMods>().EquipSight(modName);
					equippedSightSlot.GetComponent<SlotScript>().ToggleThumbnail(true, InventoryScript.itemData.modCatalog[modName].thumbnailPath);
					if (itemSlot != null) {
						currentlyEquippedModPrefab = itemSlot.gameObject;
					}
					return weaponPreviewShopSlot.itemName;
				} else {
					TriggerAlertPopup("Sights cannot be equipped on this weapon!");
					return "0";
				}
		}
		return null;
	}

	public void PreparePurchase(string itemName, string itemType, char currencyType, Texture thumb) {
		PrepareDurationDropdown(itemType == "Mod");
		durationSelection.index = 0;
		durationSelection.UpdateUI();
		itemBeingPurchased = itemName;
		typeBeingPurchased = itemType;
		currencyTypeBeingPurchased = currencyType;
		makePurchasePopup.GetComponentInChildren<RawImage>().texture = thumb;
        // Initialize with 1 day price
        SetTotalCost(0, durationSelection.GetCurrentItem());
		TriggerMakePurchasePopup();
    }

	void PrepareDurationDropdown(bool permOnly) {
		durationSelection.ClearItems();
		if (permOnly) {
			durationSelection.CreateNewItem("Permanent");
		} else {
			durationSelection.CreateNewItem("1 day");
			durationSelection.CreateNewItem("7 days");
			durationSelection.CreateNewItem("30 days");
			durationSelection.CreateNewItem("90 days");
			durationSelection.CreateNewItem("Permanent");
		}
	}

	public void OnConfirmPreparePurchaseClicked() {
		confirmingTransaction = true;
		makePurchasePopup.ModalWindowOut();
		TriggerConfirmPopup("ARE YOU SURE YOU WOULD LIKE TO BUY " + itemBeingPurchased + " FOR " + durationSelection.GetCurrentItem() + " FOR " + totalCostBeingPurchased + " " + (currencyTypeBeingPurchased == 'G' ? "GP" : "KASH") + "?");
	}

	public void OnConfirmPurchaseClicked() {
		TriggerBlockScreen(true);
		ConfirmPurchase();
	}

	public void OnConfirmSaleClicked() {
		TriggerBlockScreen(true);
		ConfirmSale();
	}

	public void OnDurationSelect() {
		int durationInput = durationSelection.index;
        SetTotalCost(durationInput, durationSelection.GetCurrentItem());
	}

    void SetTotalCost(int duration, string durationText)
    {
        totalCostBeingPurchased = GetCostForItemAndType(itemBeingPurchased, typeBeingPurchased, duration);
        totalGpCostTxt.text = "YOU ARE BUYING [" + itemBeingPurchased + "] FOR [" + durationText + "] FOR " + totalCostBeingPurchased + " " + (currencyTypeBeingPurchased == 'G' ? "GP" : "KASH") + ".";
    }

	public void OnCancelPurchaseClicked() {
		itemBeingPurchased = null;
		typeBeingPurchased = null;
		confirmingTransaction = false;
		confirmingSale = false;
		resettingKeysFlag = false;
		resettingGraphicsFlag = false;
		// confirmPurchasePopup.SetActive(false);
	}

	public void PrepareSale(DateTime acquireDate, int duration, int cost, string itemName, string itemType, string id)
	{
		confirmingSale = true;
		itemBeingPurchased = id == null ? itemName : id;
		typeBeingPurchased = itemType;
		int salePrice = GetSalePriceForItem(acquireDate, duration, cost);
		TriggerConfirmPopup("YOU MAY SELL [" + itemName + "] FOR [" + salePrice + "] GP.\n\nWOULD YOU LIKE TO PROCEED WITH THIS SALE?");
	}

	void ConfirmPurchase() {
        // Ensure that the user doesn't already have this item
        float hasDuplicateCheck = HasDuplicateItem(itemBeingPurchased, typeBeingPurchased);
        if (hasDuplicateCheck < 0f) {
			TriggerAlertPopup("You already own this item.");
			TriggerBlockScreen(false);
			confirmingTransaction = false;
			return;
		}
        bool isStacking = (hasDuplicateCheck >= 0f && !Mathf.Approximately(0f, hasDuplicateCheck));
        float totalNewDuration = ConvertDurationInput(durationSelection.index);
        totalNewDuration = (Mathf.Approximately(totalNewDuration, -1f) ? totalNewDuration : totalNewDuration + hasDuplicateCheck);
		if (currencyTypeBeingPurchased == 'G') {
			if (PlayerData.playerdata.info.Gp >= totalCostBeingPurchased) {
				PlayerData.playerdata.AddItemToInventory(itemBeingPurchased, typeBeingPurchased, totalNewDuration, true, "gp");
				confirmPopup.ModalWindowOut();
			} else {	
				TriggerAlertPopup("You do not have enough GP to purchase this item.");
				TriggerBlockScreen(false);
				confirmingTransaction = false;
			}	
		} else if (currencyTypeBeingPurchased == 'K') {
			if (PlayerData.playerdata.info.Kash >= totalCostBeingPurchased) {
				PlayerData.playerdata.AddItemToInventory(itemBeingPurchased, typeBeingPurchased, totalNewDuration, true, "kash");
				confirmPopup.ModalWindowOut();
			} else {	
				TriggerAlertPopup("You do not have enough KASH to purchase this item.");
				TriggerBlockScreen(false);
				confirmingTransaction = false;
			}	
		}
	}

	void ConfirmSale() {
		PlayerData.playerdata.SellItemFromInventory(itemBeingPurchased, typeBeingPurchased);
	}

	float ConvertDurationInput(int durationSelection) {
		float duration = 0f;
		switch (durationSelection) {
			case 0:
				duration = ONE_DAY_MINS;
				break;
			case 1:
				duration = SEVEN_DAYS_MINS;
				break;
			case 2:
				duration = THIRTY_DAYS_MINS;
				break;
			case 3:
				duration = NINETY_DAYS_MINS;
				break;
			case 4:
				duration = PERMANENT;
				break;
		}
		return duration;
	}

	uint GetCostForItemAndType(string itemName, string itemType, int duration) {
        float durationMultiplier = 1f;
		
		if (itemType != "Mod") {
			switch (duration)
			{
				case 1:
					durationMultiplier = SEVEN_DAY_COST_MULTIPLIER;
					break;
				case 2:
					durationMultiplier = THIRTY_DAY_COST_MULTIPLIER;
					break;
				case 3:
					durationMultiplier = NINETY_DAY_COST_MULTIPLIER;
					break;
				case 4:
					durationMultiplier = PERMANENT_COST_MULTIPLIER;
					break;
				default:
					break;

			}
		}
		if (itemType.Equals("Armor")) {
			return (uint)((currencyTypeBeingPurchased == 'G' ? InventoryScript.itemData.armorCatalog[itemName].gpPrice : InventoryScript.itemData.armorCatalog[itemName].kashPrice) * durationMultiplier);
		} else if (itemType.Equals("Character")) {
			return (uint)((currencyTypeBeingPurchased == 'G' ? InventoryScript.itemData.characterCatalog[itemName].gpPrice : InventoryScript.itemData.characterCatalog[itemName].kashPrice) * durationMultiplier);
		} else if (itemType.Equals("Weapon")) {
			return (uint)((currencyTypeBeingPurchased == 'G' ? InventoryScript.itemData.weaponCatalog[itemName].gpPrice : InventoryScript.itemData.weaponCatalog[itemName].kashPrice) * durationMultiplier);
		} else if (itemType.Equals("Mod")) {
			return (uint)((currencyTypeBeingPurchased == 'G' ? InventoryScript.itemData.modCatalog[itemName].gpPrice : InventoryScript.itemData.modCatalog[itemName].kashPrice) * durationMultiplier);
		}
		return (uint)((currencyTypeBeingPurchased == 'G' ? InventoryScript.itemData.equipmentCatalog[itemName].gpPrice : InventoryScript.itemData.equipmentCatalog[itemName].kashPrice) * durationMultiplier);
	}

	public void UpdateCurrency() {
		myGpTxt.text = ""+PlayerData.playerdata.info.Gp;
		myKashTxt.text = ""+PlayerData.playerdata.info.Kash;
	}

	// Players cannot own multiple of the same item unless they're mods.
	// Mods are always permanent.
	// Items can only be purchased again if they don't own it for permanent
	public float HasDuplicateItem(string itemName, string type) {
		if (type == "Mod") return 0f;
		if (type.Equals("Weapon")) {
            foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
            {
                WeaponData item = entry.Value;
				if (entry.Key.Equals(itemName)) {
					float duration = float.Parse(item.Duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else if (type.Equals("Character")) {
            foreach (KeyValuePair<string, CharacterData> entry in PlayerData.playerdata.inventory.myCharacters)
            {
                CharacterData item = entry.Value;
				if (entry.Key.Equals(itemName)) {
					float duration = float.Parse(item.Duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else if (type.Equals("Armor")) {
            foreach (KeyValuePair<string, ArmorData> entry in PlayerData.playerdata.inventory.myArmor)
            {
                ArmorData item = entry.Value;
				if (entry.Key.Equals(itemName)) {
					float duration = float.Parse(item.Duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else {
			string itemCategory = InventoryScript.itemData.equipmentCatalog[itemName].category;
			ObservableDict<string, EquipmentData> inventoryRefForCat = null;
			switch (itemCategory) {
				case "Top":
					inventoryRefForCat = PlayerData.playerdata.inventory.myTops;
					break;
				case "Bottom":
					inventoryRefForCat = PlayerData.playerdata.inventory.myBottoms;
					break;
				case "Footwear":
					inventoryRefForCat = PlayerData.playerdata.inventory.myFootwear;
					break;
				case "Headgear":
					inventoryRefForCat = PlayerData.playerdata.inventory.myHeadgear;
					break;
				case "Facewear":
					inventoryRefForCat = PlayerData.playerdata.inventory.myFacewear;
					break;
			}
            foreach (KeyValuePair<string, EquipmentData> entry in inventoryRefForCat)
            {
                EquipmentData item = entry.Value;
				if (entry.Key.Equals(itemName)) {
					float duration = float.Parse(item.Duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		}
		return 0f;
	}

	void SetPlayerNameForTitle() {
		mainNametagTxt.text = PlayerData.playerdata.info.Playername;
	}

	void SetPlayerRankForTitle() {
		Rank rank = PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.Exp);
		mainRankTxt.text = rank.name;
		mainRankImg.texture = PlayerData.playerdata.GetRankInsigniaForRank(rank.name);
	}

	void SetPlayerLevelProgressForTitle() {
		uint myExp = PlayerData.playerdata.info.Exp;
		Rank rank = PlayerData.playerdata.GetRankFromExp(myExp);
		uint currExp = myExp - rank.minExp;
		uint toExp = rank.maxExp - rank.minExp;
		float percentProgress = (float)currExp / (float)toExp;
		mainLevelProgress.value = percentProgress;
		mainExpTxt.text = currExp + " / " + toExp + "(" + (uint)(percentProgress * 100) + "%)";
	}

	void SetPlayerCurrency() {
		myGpTxt.text = ""+PlayerData.playerdata.info.Gp;
		myKashTxt.text = ""+PlayerData.playerdata.info.Kash;
	}

	public void SetPlayerStatsForTitle() {
		SetPlayerNameForTitle();
		SetPlayerRankForTitle();
		SetPlayerLevelProgressForTitle();
		SetPlayerCurrency();
	}

	public void TriggerEmergencyPopup(string message) {
        TriggerAlertPopup(message);
    }

	public void TogglePlayerTemplate(bool b) {
		if (PlayerData.playerdata.bodyReference != null) {
			PlayerData.playerdata.bodyReference.SetActive(b);
			PlayerData.playerdata.bodyReference.GetComponent<Animator>().SetBool("onTitle", true);
		}
	}

	public void ToggleWeaponPreview(bool b) {
		weaponPreviewSlot.SetActive(b);
	}

	public void HideAll(bool b) {
		TogglePlayerTemplate(b);
		ToggleWeaponPreview(b);
	}

	public void HandleLeftSideButtonPress(Button b) {
		if (b == previouslyPressedButtonLeft) {
			return;
		}
		if (previouslyPressedButtonLeft != null) {
			previouslyPressedButtonLeft.GetComponent<Animator>().Play("Pressed to Normal");
		}
		b.GetComponent<Animator>().Play("Normal to Pressed");
		previouslyPressedButtonLeft = b;
	}

	public void HandleLeftSideSubButtonPress(Button b) {
		if (b == previouslyPressedSubButtonLeft) {
			return;
		}
		if (previouslyPressedSubButtonLeft != null) {
			previouslyPressedSubButtonLeft.GetComponent<Animator>().Play("Pressed to Normal");
		}
		b.GetComponent<Animator>().Play("Normal to Pressed");
		previouslyPressedSubButtonLeft = b;
	}

	public void HandleRightSideButtonPress(Button b) {
		if (b == previouslyPressedButtonRight) {
			return;
		}
		if (previouslyPressedButtonRight != null) {
			previouslyPressedButtonRight.GetComponent<Animator>().Play("Pressed to Normal");
		}
		b.GetComponent<Animator>().Play("Normal to Pressed");
		previouslyPressedButtonRight = b;
	}

	public void HandleRightSideSubButtonPress(Button b) {
		if (b == previouslyPressedSubButtonRight) {
			return;
		}
		if (previouslyPressedSubButtonRight != null) {
			previouslyPressedSubButtonRight.GetComponent<Animator>().Play("Pressed to Normal");
		}
		b.GetComponent<Animator>().Play("Normal to Pressed");
		previouslyPressedSubButtonRight = b;
	}

	public void OpenPrimaryWeaponTabs() {
		ResetLeftSubTabs();
		primaryWeaponTabs.SetActive(true);
		secondaryWeaponTabs.SetActive(false);
		supportWeaponTabs.SetActive(false);
		meleeWeaponTabs.SetActive(false);
	}

	public void OpenSecondaryWeaponTabs() {
		ResetLeftSubTabs();
		primaryWeaponTabs.SetActive(false);
		secondaryWeaponTabs.SetActive(true);
		supportWeaponTabs.SetActive(false);
		meleeWeaponTabs.SetActive(false);
	}

	public void OpenSupportWeaponTabs() {
		ResetLeftSubTabs();
		primaryWeaponTabs.SetActive(false);
		secondaryWeaponTabs.SetActive(false);
		supportWeaponTabs.SetActive(true);
		meleeWeaponTabs.SetActive(false);
	}

	public void OpenMeleeWeaponTabs() {
		ResetLeftSubTabs();
		primaryWeaponTabs.SetActive(false);
		secondaryWeaponTabs.SetActive(false);
		supportWeaponTabs.SetActive(false);
		meleeWeaponTabs.SetActive(true);
	}

	public void OpenMarketplacePrimaryWeaponTabs() {
		ResetMarketplaceLeftSubTabs();
		marketplacePrimaryWeaponTabs.SetActive(true);
		marketplaceSecondaryWeaponTabs.SetActive(false);
		marketplaceSupportWeaponTabs.SetActive(false);
		marketplaceMeleeWeaponTabs.SetActive(false);
		marketplaceModsWeaponTabs.SetActive(false);
	}

	public void OpenMarketplaceSecondaryWeaponTabs() {
		ResetMarketplaceLeftSubTabs();
		marketplacePrimaryWeaponTabs.SetActive(false);
		marketplaceSecondaryWeaponTabs.SetActive(true);
		marketplaceSupportWeaponTabs.SetActive(false);
		marketplaceMeleeWeaponTabs.SetActive(false);
		marketplaceModsWeaponTabs.SetActive(false);
	}

	public void OpenMarketplaceSupportWeaponTabs() {
		ResetMarketplaceLeftSubTabs();
		marketplacePrimaryWeaponTabs.SetActive(false);
		marketplaceSecondaryWeaponTabs.SetActive(false);
		marketplaceSupportWeaponTabs.SetActive(true);
		marketplaceMeleeWeaponTabs.SetActive(false);
		marketplaceModsWeaponTabs.SetActive(false);
	}

	public void OpenMarketplaceMeleeWeaponTabs() {
		ResetMarketplaceLeftSubTabs();
		marketplacePrimaryWeaponTabs.SetActive(false);
		marketplaceSecondaryWeaponTabs.SetActive(false);
		marketplaceSupportWeaponTabs.SetActive(false);
		marketplaceMeleeWeaponTabs.SetActive(true);
		marketplaceModsWeaponTabs.SetActive(false);
	}

	public void OpenMarketplaceModsWeaponTabs() {
		ResetMarketplaceLeftSubTabs();
		marketplacePrimaryWeaponTabs.SetActive(false);
		marketplaceSecondaryWeaponTabs.SetActive(false);
		marketplaceSupportWeaponTabs.SetActive(false);
		marketplaceMeleeWeaponTabs.SetActive(false);
		marketplaceModsWeaponTabs.SetActive(true);
	}

	public void OpenModShopPrimaryWeaponTabs() {
		ResetModShopLeftSubTabs();
		modShopPrimaryWeaponTabs.SetActive(true);
		modShopSecondaryWeaponTabs.SetActive(false);
		modShopSupportWeaponTabs.SetActive(false);
		modShopMeleeWeaponTabs.SetActive(false);
	}

	public void OpenModShopSecondaryWeaponTabs() {
		ResetModShopLeftSubTabs();
		modShopPrimaryWeaponTabs.SetActive(false);
		modShopSecondaryWeaponTabs.SetActive(true);
		modShopSupportWeaponTabs.SetActive(false);
		modShopMeleeWeaponTabs.SetActive(false);
	}

	public void OpenModShopSupportWeaponTabs() {
		ResetModShopLeftSubTabs();
		modShopPrimaryWeaponTabs.SetActive(false);
		modShopSecondaryWeaponTabs.SetActive(false);
		modShopSupportWeaponTabs.SetActive(true);
		modShopMeleeWeaponTabs.SetActive(false);
	}

	public void OpenModShopMeleeWeaponTabs() {
		ResetModShopLeftSubTabs();
		modShopPrimaryWeaponTabs.SetActive(false);
		modShopSecondaryWeaponTabs.SetActive(false);
		modShopSupportWeaponTabs.SetActive(false);
		modShopMeleeWeaponTabs.SetActive(true);
	}

	void ResetLeftSubTabs() {
		primaryWeaponTabs.SetActive(false);
		secondaryWeaponTabs.SetActive(false);
		supportWeaponTabs.SetActive(false);
		meleeWeaponTabs.SetActive(false);
	}

	void ResetMarketplaceLeftSubTabs() {
		marketplacePrimaryWeaponTabs.SetActive(false);
		marketplaceSecondaryWeaponTabs.SetActive(false);
		marketplaceSupportWeaponTabs.SetActive(false);
		marketplaceMeleeWeaponTabs.SetActive(false);
		marketplaceModsWeaponTabs.SetActive(false);
	}

	void ResetModShopLeftSubTabs() {
		modShopPrimaryWeaponTabs.SetActive(false);
		modShopSecondaryWeaponTabs.SetActive(false);
		modShopSupportWeaponTabs.SetActive(false);
		modShopMeleeWeaponTabs.SetActive(false);
	}

	public void OnConfirmButtonClicked() {
		if (confirmingTransaction) {
			OnConfirmPurchaseClicked();
		} else if (confirmingSale) {
			OnConfirmSaleClicked();
		} else if (connexion.listPlayer.kickingPlayerFlag) {
			connexion.listPlayer.KickPlayer(connexion.listPlayer.playerBeingKicked);
		} else if (resettingKeysFlag) {
			ResetKeyBindings();
		} else if (resettingGraphicsFlag) {
			ResetGraphicsSettings();
		}
	}

	public void TriggerBlockScreen(bool b) {
		blockScreenTrigger = b;
	}

	void ToggleBlockScreen(bool b) {
		blockScreen.SetActive(b);
		blockBlur.SetActive(b);
	}

	public bool WeaponIsSuppressorCompatible(string weaponName) {
		return InventoryScript.itemData.weaponCatalog[weaponName].suppressorCompatible;
	}

	public bool WeaponIsSightCompatible(string weaponName) {
		return InventoryScript.itemData.weaponCatalog[weaponPreviewShopSlot.itemName].sightCompatible;
	}

	public void UnloadDeadScenes()
	{
		if (SceneManager.sceneCount > 1) {
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				Scene thisScene = SceneManager.GetSceneAt(i);
				if (thisScene.name != "Title") {
					SceneManager.UnloadSceneAsync(thisScene);
				}
			}
		}
	}

	public void OnResetKeyBindingsClicked()
	{
		resettingKeysFlag = true;
		TriggerConfirmPopup("ARE YOU SURE YOU WISH TO RESET YOUR KEY BINDINGS TO DEFAULT?");
	}

	public void OnResetGraphicsSettingsClicked()
	{
		resettingGraphicsFlag = true;
		TriggerConfirmPopup("ARE YOU SURE YOU WISH TO RESET YOUR GRAPHICS SETTINGS TO DEFAULT?");
	}

	void ResetKeyBindings()
	{
		resettingKeysFlag = false;
		PlayerPreferences.playerPreferences.ResetKeyMappings();
		foreach (KeyMappingInput k in keyMappingInputs) {
			k.ResetKeyDisplay();
		}
	}

	void ResetGraphicsSettings()
	{
		resettingGraphicsFlag = false;
		PlayerPreferences.playerPreferences.SetDefaultGraphics();
		InitializeGraphicSettings();
		PlayerPreferences.playerPreferences.SetGraphicsSettings();
	}

	public void OnCreditsButtonClicked()
	{
		// Hide templates
		TogglePlayerTemplate(false);
		ToggleWeaponPreview(false);
		DestroyOldWeaponTemplate();

		// Disable main panel, main panel animator, top panel, bottom panel
		mainPanelsAnimator.enabled = false;
		mainPanels.alpha = 0f;
		mainPanels.interactable = false;
		mainPanels.blocksRaycasts = false;

		// Change background to credits video
		creditsManager.backgroundPlayer.GetComponent<UIManagerBackground>().enabled = false;
		backgroundAnimator.enabled = false;
		creditsManager.backgroundPlayer.gameObject.GetComponent<RawImage>().color = Color.white;
		glowMotes.SetActive(false);
		creditsManager.backgroundPlayer.Stop();
		creditsManager.backgroundPlayer.enabled = false;
		creditsManager.backgroundPlayerCredits.enabled = true;
		creditsManager.backgroundPlayerCredits.Play();

		// Activate credits exit button
		creditsExitButton.gameObject.SetActive(true);
	}

	public void OnCreditsExitButtonClicked()
	{
		// Change background back to normal
		creditsManager.backgroundPlayer.GetComponent<UIManagerBackground>().enabled = true;
		creditsManager.backgroundPlayerCredits.enabled = false;
		creditsManager.backgroundPlayerCredits.Stop();
		creditsExitButton.gameObject.SetActive(false);
		backgroundAnimator.enabled = true;
		glowMotes.SetActive(true);
		creditsManager.backgroundPlayer.enabled = true;
		creditsManager.backgroundPlayer.Play();

		// Enable main panel, main panel animator, top panel, bottom panel
		StartCoroutine("CreditsExit");
	}

	IEnumerator CreditsExit()
	{
		yield return new WaitForSeconds(1f);
		ReturnToTitle();
	}

	public void ReturnToTitle()
	{
		mainPanelsAnimator.enabled = true;
		mainPanels.alpha = 1f;
		mainPanels.interactable = true;
		mainPanels.blocksRaycasts = true;
		mainPanelManager.OpenFirstTab();
		mainPanelManager.OpenPanel("Title");
	}

	public void JoinCampaignGlobalChat()
	{
		PlayerData.playerdata.globalChatClient.SubscribeToGlobalChat('C');
	}

	public void JoinVersusGlobalChat()
	{
		PlayerData.playerdata.globalChatClient.SubscribeToGlobalChat('V');
	}

	public void LeaveGlobalChats()
	{
		PlayerData.playerdata.globalChatClient.UnsubscribeFromGlobalChat();
	}

	public void OnRoomPasswordEnter()
	{
		string passwordEntered = roomEnterPasswordInput.text;
		roomEnterPasswordInput.text = "";
		connexion.AttemptJoinRoom(roomEnteringName, passwordEntered);
	}

	public void OnRoomPasswordChange()
	{
		string passwordEntered = roomPasswordInput.text;
		bool success = connexion.listPlayer.SetRoomPassword(passwordEntered);
		if (success) {
			roomPasswordInput.text = "";
			CloseRoomPasswordChange();
		}
	}

	public void OnAddFriend()
	{
		friendsMessenger.AddFriend(addFriendInput.text);
	}

	public void CloseRoomPasswordEnter()
	{
		roomEnterPasswordPopup.ModalWindowOut();
		blurManager.BlurOutAnim();
	}

	public void CloseRoomPasswordChange()
	{
		roomPasswordPopup.ModalWindowOut();
		blurManager.BlurOutAnim();
	}

	public void CloseAddFriend()
	{
		addFriendPopup.ModalWindowOut();
		blurManager.BlurOutAnim();
	}

	void InitializeGraphicSettings()
	{
		PopulateResolutions();
		qualitySelector.index = PlayerPreferences.playerPreferences.preferenceData.qualityPreset;
		qualitySelector.UpdateUI();
		vSyncSelector.index = PlayerPreferences.playerPreferences.preferenceData.vSyncCount;
		vSyncSelector.UpdateUI();
		lodBiasSlider.value = PlayerPreferences.playerPreferences.preferenceData.lodBias;
		antiAliasingSelector.index = PlayerPreferences.playerPreferences.preferenceData.antiAliasing;
		antiAliasingSelector.UpdateUI();
		anisotropicFilteringSelector.index = PlayerPreferences.playerPreferences.preferenceData.anisotropicFiltering;
		anisotropicFilteringSelector.UpdateUI();
		masterTextureLimitSlider.value = PlayerPreferences.playerPreferences.preferenceData.masterTextureLimit;
		if (PlayerPreferences.playerPreferences.preferenceData.shadowCascades > PlayerPreferences.MAX_SHADOW_CASCADES) {
			PlayerPreferences.playerPreferences.preferenceData.shadowCascades = PlayerPreferences.MAX_SHADOW_CASCADES;
		}
		shadowCascadesSelector.index = PlayerPreferences.playerPreferences.preferenceData.shadowCascades;
		shadowCascadesSelector.UpdateUI();
		shadowResolutionSelector.index = PlayerPreferences.playerPreferences.preferenceData.shadowResolution;
		shadowResolutionSelector.UpdateUI();
		shadowSelector.index = PlayerPreferences.playerPreferences.preferenceData.shadows;
		shadowSelector.UpdateUI();
		bloomSwitch.isOn = PlayerPreferences.playerPreferences.preferenceData.bloom;
		bloomSwitch.RefreshSwitch();
		motionBlurSwitch.isOn = PlayerPreferences.playerPreferences.preferenceData.motionBlur;
		motionBlurSwitch.RefreshSwitch();
		brightnessSlider.value = (int)(PlayerPreferences.playerPreferences.preferenceData.brightness * 100f);
	}

	public void OnResolutionSelect()
	{
		Resolution r = Screen.resolutions[resolutionSelector.index];
		Screen.SetResolution(r.width, r.height, true);
	}

	void PopulateResolutions()
	{
		resolutionSelector.ClearItems();
		Resolution currRes = Screen.currentResolution;
		Resolution[] rrs = Screen.resolutions;
		int i = 0;
		foreach (Resolution r in rrs) {
			resolutionSelector.CreateNewItem(r.ToString());
			if (r.ToString() == currRes.ToString()) {
				resolutionSelector.index = i;
			}
			i++;
		}
		resolutionSelector.UpdateUI();
	}

	public void OnQualitySelect()
	{
		int q = qualitySelector.index;
		PlayerPreferences.playerPreferences.preferenceData.qualityPreset = q;
		QualitySettings.SetQualityLevel(q);
		PlayerPreferences.playerPreferences.preferenceData.vSyncCount = QualitySettings.vSyncCount;
        PlayerPreferences.playerPreferences.preferenceData.lodBias = QualitySettings.lodBias;
        PlayerPreferences.playerPreferences.preferenceData.antiAliasing = QualitySettings.antiAliasing;
        PlayerPreferences.playerPreferences.preferenceData.anisotropicFiltering = (int)QualitySettings.anisotropicFiltering;
        PlayerPreferences.playerPreferences.preferenceData.masterTextureLimit = QualitySettings.masterTextureLimit;
		if (QualitySettings.shadowCascades > PlayerPreferences.MAX_SHADOW_CASCADES) {
			QualitySettings.shadowCascades = PlayerPreferences.MAX_SHADOW_CASCADES;
		}
        PlayerPreferences.playerPreferences.preferenceData.shadowCascades = QualitySettings.shadowCascades;
        PlayerPreferences.playerPreferences.preferenceData.shadowResolution = (int)QualitySettings.shadowResolution;
		PlayerPreferences.playerPreferences.preferenceData.brightness = 1f;
		PlayerPreferences.playerPreferences.SetGraphicsSettings();
		InitializeGraphicSettings();
	}

	public void OnVSyncSelect()
	{
		int i = vSyncSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.vSyncCount = i;
		QualitySettings.vSyncCount = i;
	}

	public void OnLodBiasSelect()
	{
		int i = (int)lodBiasSlider.value;
		PlayerPreferences.playerPreferences.preferenceData.lodBias = i;
		QualitySettings.lodBias = i;
	}

	public void OnAntiAliasingSelect()
	{
		int i = antiAliasingSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.antiAliasing = i;
		QualitySettings.antiAliasing = i;
	}

	public void OnAnisotropicFilteringSelect()
	{
		int i = anisotropicFilteringSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.anisotropicFiltering = i;
		QualitySettings.anisotropicFiltering = (AnisotropicFiltering)i;
	}

	public void OnMasterTextureLimitSelect()
	{
		int i = (int)masterTextureLimitSlider.value;
		PlayerPreferences.playerPreferences.preferenceData.masterTextureLimit = i;
		QualitySettings.masterTextureLimit = i;
	}

	public void OnShadowCascadesSelect()
	{
		int i = shadowCascadesSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.shadowCascades = i;
		QualitySettings.shadowCascades = i;
	}

	public void OnShadowResolutionSelect()
	{
		int i = shadowResolutionSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.shadowResolution = i;
		QualitySettings.shadowResolution = (ShadowResolution)i;
	}

	public void OnShadowSelect()
	{
		int i = shadowSelector.index;
		PlayerPreferences.playerPreferences.preferenceData.shadows = i;
		QualitySettings.shadows = (ShadowQuality)i;
	}

	public void OnBloomSwitch(bool o)
	{
		PlayerPreferences.playerPreferences.preferenceData.bloom = o;
	}

	public void OnMotionBlurSwitch(bool o)
	{
		PlayerPreferences.playerPreferences.preferenceData.motionBlur = o;
	}

	public void OnBrightnessSelect()
	{
		PlayerPreferences.playerPreferences.preferenceData.brightness = brightnessSlider.value / 100f;
	}

	public int GetSalePriceForItem(DateTime acquireDate, int duration, int cost) {
		cost /= 4;
		if (duration == -1) {
			return cost;
		}

		DateTime expirationDate = acquireDate.AddMinutes(duration);
		DateTime currentDate = DateTime.Now;
		int remainingDays = (expirationDate.Millisecond - currentDate.Millisecond) / 86400000;
		if (remainingDays == 0) {
			remainingDays = 1;
		}

		if (remainingDays <= 7) {
			return (int)(cost * remainingDays * (1f - (0.5f / 1f)));
		} else if (remainingDays <= 30) {
			return (int)(cost * remainingDays * (1f - (0.5f / 2f)));
		}

		return (int)(cost * remainingDays * (1f - (0.5f / 3f)));
	}

	public void WarpJoinGame(string roomId)
	{
		StartCoroutine(WarpJoinGameRoutine(roomId));
	}

	IEnumerator WarpJoinGameRoutine(string roomId)
	{
		TriggerBlockScreen(true);
		yield return new WaitForSeconds(1f);
		// If currently in a match, leave it
		if (PhotonNetwork.InRoom) {
			PlayerData.playerdata.titleRef.connexion.OnLeaveGameButtonClicked();
			yield return new WaitForSeconds(1f);
		}
		// Go home
		PlayerData.playerdata.titleRef.ReturnToTitle();
		yield return new WaitForSeconds(1f);
		bool cont = true;
		RoomInfo joiningRoomInfo = null; 
		// Get room properties
		try {
			joiningRoomInfo = PlayerData.playerdata.titleRef.connexion.listRoom.cachedRoomList[roomId];
		} catch (Exception e) {
			TriggerAlertPopup("UNABLE TO JOIN ROOM.");
			cont = false;
		}
		if (cont) {
			// If versus, join versus lobby. Else, join campaign lobby.
			if (joiningRoomInfo.CustomProperties["gameMode"] == "versus") {
				PlayerData.playerdata.titleRef.mainPanelManager.OpenPanel("Versus");
			} else {
				PlayerData.playerdata.titleRef.mainPanelManager.OpenPanel("Campaign");
			}
			yield return new WaitForSeconds(1f);
			PlayerData.playerdata.titleRef.connexion.theJoinRoom(roomId);
			yield return new WaitForSeconds(0.1f);
		}
		TriggerBlockScreen(false);
	}
		
}
