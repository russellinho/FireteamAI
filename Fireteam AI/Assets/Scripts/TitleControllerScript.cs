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

public class TitleControllerScript : MonoBehaviourPunCallbacks {
	private const float NINETY_DAYS_MINS = 129600f;
	private const float THIRTY_DAYS_MINS = 43200;
	private const float SEVEN_DAYS_MINS = 10080f;
	private const float ONE_DAY_MINS = 1440f;
	private const float PERMANENT = -1f;
    private const float COST_MULT_FRACTION = 0.5f / 3f;
    private const float SEVEN_DAY_COST_MULTIPLIER = 7f * (1f - (COST_MULT_FRACTION * 1f));
    private const float THIRTY_DAY_COST_MULTIPLIER = 30f * (1f - (COST_MULT_FRACTION * 2f));
    private const float NINETY_DAY_COST_MULTIPLIER = 90f * (1f - (COST_MULT_FRACTION * 3f));
    private const float PERMANENT_COST_MULTIPLIER = 365f;

	private Vector3 modCameraRot = new Vector3(3.5f, 43.5f, 0f);
	private Vector3 modCameraPos = new Vector3(2f, 4.5f, 26.7f);
	public GameObject itemDescriptionPopupRef;
	public GameObject modDescriptionPopupRef;
	public GameObject marketplaceItemDescriptionPopupRef;
	public GameObject marketplaceModDescriptionPopupRef;

	public TextMeshProUGUI mainNametagTxt;
	public RawImage mainRankImg;
	public TextMeshProUGUI mainRankTxt;
	public Slider mainLevelProgress;
	public TextMeshProUGUI mainExpTxt;
	public Slider musicVolumeSlider;
	public TextMeshProUGUI musicVolumeField;
	public CanvasGroup loadingScreen;
	public CanvasGroup mainPanels;
	public Animator mainPanelsAnimator;
	public ModalWindowManager alertPopup;
	public ModalWindowManager confirmPopup;
	public ModalWindowManager keyBindingsPopup;
	public ModalWindowManager makePurchasePopup;
	private string itemBeingPurchased;
	private string typeBeingPurchased;
	private uint totalGpCostBeingPurchased;
	public char currentCharGender;

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
	public GameObject currentlyEquippedItemPrefab;
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
	public GameObject modInventoryContent;
	public GameObject modWeaponInventoryContent;
	public Button suppressorsBtn;
	public Button sightsBtn;
	public GameObject currentlyEquippedWeaponToMod;
	public GameObject equippedSuppressorSlot;
	public GameObject equippedSightSlot;
	public string equippedSuppressorId;
	public string equippedSightId;
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
	public string alertPopupMessage;
	public string confirmPopupMessage;
	public MainPanelManager mainPanelManager;

	// Use this for initialization
	void Awake() {
		if (PlayerData.playerdata == null || PlayerData.playerdata.bodyReference == null) {
			InstantiateLoadingScreen(null);
			ToggleLoadingScreen(true);
		}
		musicVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f;
		musicVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.musicVolume;
	}

	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Safety destroy previous match data
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			Destroy(entry.objRef);
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

	public void InstantiateLoadingScreen(string mapName) {
		if (mapName != null) {
			JukeboxScript.jukebox.audioSource1.Stop ();
			JukeboxScript.jukebox.audioSource2.Stop ();
			if (mapName.Equals ("Badlands: Act I")) {
				screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/badlands1_load");
			} else if (mapName.Equals("Badlands: Act II")) {
				screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/badlands2_load");
			}
			proTipText.text = proTips[Random.Range(0, 2)];
			mapTitleText.text = mapName;
			screenArtContainer.gameObject.SetActive(true);
			proTipContainer.gameObject.SetActive(true);
			mapTitleText.gameObject.SetActive(true);
		} else {
			screenArtContainer.gameObject.SetActive(false);
			proTipContainer.gameObject.SetActive(false);
			mapTitleText.gameObject.SetActive(false);
		}
	}

	public void ToggleLoadingScreen(bool b) {
		if (b) {
			mainPanels.alpha = 0f;
			mainPanelsAnimator.enabled = false;
			loadingScreen.alpha = 1f;
		} else {
			loadingScreen.alpha = 0f;
			mainPanelsAnimator.enabled = true;
			mainPanelsAnimator.Play("Start");
		}
	}

	void Update() {
		if (loadingScreen.alpha == 1f && PlayerData.playerdata.bodyReference != null) {
			ToggleLoadingScreen(false);
			mainPanelManager.OpenFirstTab();
		}

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
			alertPopup.ModalWindowIn();
		}
		if (triggerConfirmPopupFlag) {
			triggerConfirmPopupFlag = false;
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
	}

	public void GoToMatchmakingMenu() {
		// if (!PhotonNetwork.IsConnected) {
		// 	PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
		// 	PhotonNetwork.ConnectUsingSettings();
		// }

		// titleText.enabled = false;
		// mainMenu.SetActive (false);
		// customizationMenu.SetActive (false);
        // versusMenu.SetActive(false);
		// matchmakingMenu.SetActive (true);
	}

    public void GoToVersusMenu()
    {
        // if (!PhotonNetwork.IsConnected)
        // {
        //     PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
        //     PhotonNetwork.ConnectUsingSettings();
        // }

        // titleText.enabled = false;
        // mainMenu.SetActive(false);
        // customizationMenu.SetActive(false);
        // versusMenu.SetActive(true);
    }
		
	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		// settingsMenu.SetActive(false);
		// customizationMenu.SetActive (false);
		// matchmakingMenu.SetActive (false);
        // versusMenu.SetActive(false);
		// keyBindingsPopup.SetActive(false);
		// titleText.enabled = true;
		// mainMenu.SetActive(true);
	}

	// public void GoToKeyBindings() {
	// 	keyBindingsPopup.SetActive(true);
	// 	titleText.enabled = false;
	// 	ToggleSettingsMainButtons(false);
	// }

	bool SettingsIsOpen() {
		return false;
		// return settingsMenu.activeInHierarchy;
	}

	// public void GoToSettings() {
	// 	settingsMenu.SetActive(true);
	// 	titleText.enabled = false;
	// 	mainMenu.SetActive (false);
	// }

	// public void GoToAudioSettings() {
	// 	ToggleSettingsMainButtons(false);
	// 	audioSettingsMenu.SetActive(true);
	// }

	public void OnMusicVolumeSliderChanged() {
		SetMusicVolume(musicVolumeSlider.value);
	}

	void SetMusicVolume(float v) {
		musicVolumeField.text = ""+(int)(v * 100f);
		JukeboxScript.jukebox.SetMusicVolume(v);
	}

	// void ToggleSettingsMainButtons(bool b) {
	// 	audioSettingsBtn.gameObject.SetActive(b);
	// 	keyBindingsBtn.gameObject.SetActive(b);
	// }

	public void SaveAudioSettings() {
		PlayerPreferences.playerPreferences.preferenceData.musicVolume = (int)(musicVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.SavePreferences();
	}

	public void SaveKeyBindings() {
		PlayerPreferences.playerPreferences.SaveKeyMappings();
	}

	public void SetDefaultAudioSettings() {
		musicVolumeSlider.value = (float)JukeboxScript.DEFAULT_MUSIC_VOLUME / 100f;
		SetMusicVolume(musicVolumeSlider.value);
	}

	public void ReturnToMainMenuFromSettings() {
		// if (isChangingKeyMapping) return;
		// if (keyBindingsPopup.activeInHierarchy) {
		// 	SaveKeyBindings();
		// 	keyBindingsPopup.SetActive(false);
		// } else if (audioSettingsMenu.activeInHierarchy) {
		// 	SaveAudioSettings();
		// 	audioSettingsMenu.SetActive(false);
		// } else {
		// 	ReturnToMainMenu();
		// }
		// ToggleSettingsMainButtons(true);
	}

	public void ReturnToMainMenuFromCustomization() {
        // Save settings if the settings are active
		//  if (customizationMenu.activeInHierarchy) {
		// 	ClearCustomizationContent();
		// 	ResetCustomizationButtons();
		//  }
		SwitchToEquipmentScreen();
		// customizationMenu.SetActive (false);
		// matchmakingMenu.SetActive (false);
        // versusMenu.SetActive(false);
		// previousCamPos = camPos;
		// camPos = 0;
		// camMoveTimer = 0f;
	}

	public void ReturnToMainMenuFromMarketplace() {
		// Clear previews and return to main menu
		//  if (marketplaceMenu.activeInHierarchy) {
		//  	ClearPreview();
		// 	ClearMarketplaceContent();
		// 	ResetMarketplaceButtons();
		//  }
		// SwitchToMarketplaceEquipmentScreen();
		// marketplaceMenu.SetActive (false);
		// matchmakingMenu.SetActive (false);
        // versusMenu.SetActive(false);
		// previousCamPos = camPos;
		// camPos = 0;
		// camMoveTimer = 0f;
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
		// titleText.enabled = true;
        // mainMenu.SetActive(true);
    }

    public void GoToCustomization() {
		// titleText.enabled = false;
		// mainMenu.SetActive (false);
		// modMenu.SetActive(false);
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
		equippedSupportSlot.SetActive(false);
		equippedMeleeSlot.SetActive(false);
		// marketplaceMenu.SetActive(false);
		// previousCamPos = camPos;
		// camPos = 1;
		// camMoveTimer = 0f;
	}

	public void GoToMarketplace() {
		// titleText.enabled = false;
		// mainMenu.SetActive (false);
		// modMenu.SetActive(false);
		// shopEquippedPrimarySlot.SetActive(false);
		// shopEquippedSecondarySlot.SetActive(false);
		// shopEquippedSupportSlot.SetActive(false);
		// shopEquippedMeleeSlot.SetActive(false);
		// matchmakingMenu.SetActive (false);
        // versusMenu.SetActive(false);
		// customizationMenu.SetActive(false);
		// previousCamPos = camPos;
		// camPos = 2;
		// camMoveTimer = 0f;
	}

	public void GoToMod() {
		// mainMenu.SetActive(false);
		// customizationMenu.SetActive(false);
		// previousCamPos = camPos;
		// camPos = 3;
		// camMoveTimer = 0f;
	}

	public void ReturnToCustomizationFromModMenu() {
		// Save whatever was being customized
		SaveModsForCurrentWeapon();
		// Disable modification menu
		ResetModMenu();
		GoToCustomization();
		SwitchToLoadoutScreen();
	}

	private void ResetModMenu() {
		// Return all buttons to regular color
		suppressorsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sightsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// Destroy weapon template
		Destroy(weaponPreviewRef);
		weaponPreviewRef = null;
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
				currentlyEquippedItemPrefab = null;
				Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
			}
		} else if (type == 'w') {
			RawImage[] existingThumbnails = contentInventoryWeapons.GetComponentsInChildren<RawImage>();
			foreach (RawImage r in existingThumbnails) {
				currentlyEquippedItemPrefab = null;
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
	void ClearModCustomizationContent() {
		RawImage[] existingThumbnails = modInventoryContent.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedModPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}
	}

	public void OnHeadBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisHeadgear;
			s.itemName = thisItemName;
            s.itemType = "Headgear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisHeadgear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisHeadgear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedHeadgear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceHeadBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.equipmentDetails = thisHeadgear;
			s.itemName = entry.Key;
            s.itemType = "Headgear";
			s.itemDescription = thisHeadgear.description;
			s.gpPriceTxt.text = ""+thisHeadgear.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisHeadgear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnFaceBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFacewear;
			s.itemName = thisItemName;
            s.itemType = "Facewear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisFacewear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFacewear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFacewear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceFaceBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.equipmentDetails = thisFacewear;
			s.itemName = entry.Key;
            s.itemType = "Facewear";
			s.itemDescription = thisFacewear.description;
			s.gpPriceTxt.text = ""+thisFacewear.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFacewear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnArmorBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.armorDetails = thisArmor;
			s.itemName = thisItemName;
            s.itemType = "Armor";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisArmor.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisArmor.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedArmor)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceArmorBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.armorDetails = thisArmor;
			s.itemName = entry.Key;
            s.itemType = "Armor";
			s.itemDescription = thisArmor.description;
			s.gpPriceTxt.text = ""+thisArmor.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisArmor.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnTopsBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisTop;
            s.itemName = thisItemName;
            s.itemType = "Top";
			s.itemDescription = thisTop.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 4f, t.sizeDelta.y / 4f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedTop)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceTopsBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.equipmentDetails = thisEquipment;
			s.itemName = entry.Key;
            s.itemType = "Top";
			s.itemDescription = thisEquipment.description;
			s.gpPriceTxt.text = ""+thisEquipment.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisEquipment.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnBottomsBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisBottom;
			s.itemName = thisItemName;
            s.itemType = "Bottom";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisBottom.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedBottom)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceBottomsBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.equipmentDetails = thisBottom;
			s.itemName = entry.Key;
            s.itemType = "Bottom";
			s.itemDescription = thisBottom.description;
			s.gpPriceTxt.text = ""+thisBottom.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisBottom.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnFootwearBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFootwear;
			s.itemName = thisItemName;
            s.itemType = "Footwear";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = thisFootwear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFootwear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryEquipment.transform);
		}
	}

	public void OnMarketplaceFootwearBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.equipmentDetails = thisFootwear;
			s.itemName = entry.Key;
            s.itemType = "Footwear";
			s.itemDescription = thisFootwear.description;
			s.gpPriceTxt.text = ""+thisFootwear.gpPrice;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFootwear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnPrimaryWepBtnClicked() {
		// Moving secondary and support button down for submenu
		// RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = secondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		// rt = supportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		// rt = meleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));

		// Change all button colors
		primaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Remove sub buttons
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		launcherSubBtn.gameObject.SetActive(false);
		launcherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(false);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.gameObject.SetActive(false);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		knivesSubBtn.gameObject.SetActive(false);
		knivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		assaultRifleSubBtn.gameObject.SetActive(true);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.gameObject.SetActive(true);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.gameObject.SetActive(true);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(true);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(true);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplacePrimaryWepBtnClicked() {
		// Moving secondary and support button down for submenu
		// RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		// rt = shopSupportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		// rt = shopMeleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		// rt = shopModsBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));

		// Change all button colors
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Remove sub buttons
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.gameObject.SetActive(false);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.gameObject.SetActive(false);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopKnivesSubBtn.gameObject.SetActive(false);
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSightsSubBtn.gameObject.SetActive(false);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(true);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.gameObject.SetActive(true);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.gameObject.SetActive(true);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(true);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(true);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnSecondaryWepBtnClicked() {
		// Change all button colors
		secondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = secondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = supportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));
		// rt = meleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.gameObject.SetActive(false);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.gameObject.SetActive(false);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(false);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.gameObject.SetActive(false);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		knivesSubBtn.gameObject.SetActive(false);
		knivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		pistolSubBtn.gameObject.SetActive(true);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		launcherSubBtn.gameObject.SetActive(true);
		launcherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceSecondaryWepBtnClicked() {
		// Change all button colors
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = shopSupportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));
		// rt = shopMeleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));
		// rt = shopModsBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.gameObject.SetActive(false);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.gameObject.SetActive(false);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.gameObject.SetActive(false);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopKnivesSubBtn.gameObject.SetActive(false);
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSightsSubBtn.gameObject.SetActive(false);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopPistolSubBtn.gameObject.SetActive(true);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.gameObject.SetActive(true);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnSupportWepBtnClicked() {
		// Change all button colors
		supportWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = secondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = supportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		// rt = meleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, SUPPORT_SUBBUTTON_COUNT));

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.gameObject.SetActive(false);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.gameObject.SetActive(false);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		launcherSubBtn.gameObject.SetActive(false);
		launcherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		knivesSubBtn.gameObject.SetActive(false);
		knivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		explosivesSubBtn.gameObject.SetActive(true);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(true);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.gameObject.SetActive(true);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceSupportWepBtnClicked() {
		// Change all button colors
		shopSupportWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = shopSupportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		// rt = shopMeleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, SUPPORT_SUBBUTTON_COUNT));
		// rt = shopModsBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, SUPPORT_SUBBUTTON_COUNT));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.gameObject.SetActive(false);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.gameObject.SetActive(false);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.gameObject.SetActive(false);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSightsSubBtn.gameObject.SetActive(false);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopKnivesSubBtn.gameObject.SetActive(false);
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopExplosivesSubBtn.gameObject.SetActive(true);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(true);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.gameObject.SetActive(true);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnMeleeWepBtnClicked() {
		// Change all button colors
		meleeWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = secondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = supportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		// rt = meleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, 0));

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.gameObject.SetActive(false);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.gameObject.SetActive(false);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		launcherSubBtn.gameObject.SetActive(false);
		launcherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(false);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.gameObject.SetActive(false);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		knivesSubBtn.gameObject.SetActive(true);
		knivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedMeleeWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceMeleeWepBtnClicked() {
		// Change all button colors
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = shopSupportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		// rt = shopMeleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, 0));
		// rt = shopModsBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, MELEE_SUBBUTTON_COUNT));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.gameObject.SetActive(false);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.gameObject.SetActive(false);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.gameObject.SetActive(false);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSightsSubBtn.gameObject.SetActive(false);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.gameObject.SetActive(false);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopKnivesSubBtn.gameObject.SetActive(true);
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnAssaultRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceAssaultRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnSmgSubBtnClicked() {
		// Make tab orange and clear other tabs
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceSmgSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnLmgSubBtnClicked() {
		// Make tab orange and clear other tabs
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceLmgSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnShotgunSubBtnClicked() {
		// Make tab orange and clear other tabs
		shotgunSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceShotgunSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnSniperRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		smgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		lmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedPrimaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceSniperRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnPistolSubBtnClicked() {
		// Make tab orange and clear other tabs
		pistolSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		launcherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplacePistolSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopPistolSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnLaunchersSubBtnClicked() {
		// Make tab orange and clear other tabs
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		launcherSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceLaunchersSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnExplosivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		explosivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSecondaryWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceExplosivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnBoostersSubBtnClicked() {
		// Make tab orange and clear other tabs
		boostersSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		deployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceBoostersSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnDeployablesSubBtnClicked() {
		// Make tab orange and clear other tabs
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		deployablesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedSupportWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceDeployablesSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnKnivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		knivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (thisWeaponName.Equals(PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().equippedMeleeWeapon)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceKnivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.weaponDetails = w;
			s.itemName = w.name;
            s.itemType = "Weapon";
			s.itemDescription = w.description;
			s.weaponCategory = w.category;
            s.gpPriceTxt.text = "" + w.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(w.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnMarketplaceModsBtnClicked() {
		// Change all button colors
		shopModsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		// RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		// rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		// rt = shopSupportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		// rt = shopMeleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, 0));
		// rt = shopModsBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, 0));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSmgSubBtn.gameObject.SetActive(false);
		shopSmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLmgSubBtn.gameObject.SetActive(false);
		shopLmgSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopLauncherSubBtn.gameObject.SetActive(false);
		shopLauncherSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopDeployablesSubBtn.gameObject.SetActive(false);
		shopDeployablesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopKnivesSubBtn.gameObject.SetActive(false);
		shopKnivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopSuppressorsSubBtn.gameObject.SetActive(true);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSightsSubBtn.gameObject.SetActive(true);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.modDescriptionPopupRef = marketplaceModDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
            s.gpPriceTxt.text = "" + m.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnMarketplaceSuppressorsSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSightsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.modDescriptionPopupRef = marketplaceModDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
            s.gpPriceTxt.text = "" + m.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnMarketplaceSightsSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopSightsSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

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
			s.modDescriptionPopupRef = marketplaceModDescriptionPopupRef;
			s.modDetails = m;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.weaponCategory = m.category;
            s.gpPriceTxt.text = "" + m.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			o.transform.SetParent(shopContentWeapons.transform);
		}
	}

	public void OnCharacterBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.characterDetails = c;
			s.itemName = thisCharacterName;
            s.itemType = "Character";
			s.duration = ed.Duration;
			s.acquireDate = ed.AcquireDate;
			s.itemDescription = c.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisCharacterName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventoryWeapons.transform);
		}
	}

	public void OnMarketplaceCharacterBtnClicked() {
		// Change all button colors
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

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
			s.itemDescriptionPopupRef = marketplaceItemDescriptionPopupRef;
			s.characterDetails = c;
			s.itemName = entry.Key;
            s.itemType = "Character";
			s.itemDescription = c.description;
            s.gpPriceTxt.text = "" + c.gpPrice;
            s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			o.transform.SetParent(shopContentEquipment.transform);
		}
	}

	public void OnSuppressorsBtnClicked() {
		// Change all button colors
		suppressorsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		sightsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearModCustomizationContent();
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
			s.modDescriptionPopupRef = modDescriptionPopupRef;
			s.modDetails = m;
			s.id = entry.Key;
			s.equippedOn = modData.EquippedOn;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.modCategory = m.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			// if (modWeaponLbl.text.Equals(modData.EquippedOn)) {
			// 	s.ToggleEquippedIndicator(true);
			// 	currentlyEquippedModPrefab = o;
			// }
			o.transform.SetParent(modInventoryContent.transform);
		}
	}

	public void OnSightsBtnClicked() {
		// Change all button colors
		sightsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		suppressorsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearModCustomizationContent();
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
			s.modDescriptionPopupRef = modDescriptionPopupRef;
			s.modDetails = m;
			s.id = entry.Key;
			s.equippedOn = modData.EquippedOn;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.modCategory = m.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			// if (modWeaponLbl.text.Equals(modData.EquippedOn)) {
			// 	s.ToggleEquippedIndicator(true);
			// 	currentlyEquippedModPrefab = o;
			// }
			o.transform.SetParent(modInventoryContent.transform);
		}
	}

	void SwitchToLoadoutScreen() {
		// loadoutBtn.GetComponentInChildren<Text>().text = "Equipment";
		headgearBtn.gameObject.SetActive(false);
		faceBtn.gameObject.SetActive(false);
		topsBtn.gameObject.SetActive(false);
		bottomsBtn.gameObject.SetActive(false);
		footwearBtn.gameObject.SetActive(false);
		characterBtn.gameObject.SetActive(false);
		armorBtn.gameObject.SetActive(false);
		
		equippedHeadSlot.SetActive(false);
		equippedFaceSlot.SetActive(false);
		equippedTopSlot.SetActive(false);
		equippedBottomSlot.SetActive(false);
		equippedFootSlot.SetActive(false);
		equippedCharacterSlot.SetActive(false);
		equippedArmorSlot.SetActive(false);

		primaryWepBtn.gameObject.SetActive(true);
		secondaryWepBtn.gameObject.SetActive(true);
		supportWepBtn.gameObject.SetActive(true);
		meleeWepBtn.gameObject.SetActive(true);
		RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = secondaryWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = supportWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		rt = meleeWepBtn.GetComponent<RectTransform>();
		// rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(meleeWepBtnYPos, 0));
		equippedPrimarySlot.SetActive(true);
		equippedSecondarySlot.SetActive(true);
		equippedSupportSlot.SetActive(true);
		equippedMeleeSlot.SetActive(true);
	}

	void SwitchToEquipmentScreen() {
		// loadoutBtn.GetComponentInChildren<Text>().text = "Loadout";
		headgearBtn.gameObject.SetActive(true);
		faceBtn.gameObject.SetActive(true);
		topsBtn.gameObject.SetActive(true);
		bottomsBtn.gameObject.SetActive(true);
		footwearBtn.gameObject.SetActive(true);
		characterBtn.gameObject.SetActive(true);
		armorBtn.gameObject.SetActive(true);

		equippedHeadSlot.SetActive(true);
		equippedFaceSlot.SetActive(true);
		equippedTopSlot.SetActive(true);
		equippedBottomSlot.SetActive(true);
		equippedFootSlot.SetActive(true);
		equippedCharacterSlot.SetActive(true);
		equippedArmorSlot.SetActive(true);

		primaryWepBtn.gameObject.SetActive(false);
		secondaryWepBtn.gameObject.SetActive(false);
		supportWepBtn.gameObject.SetActive(false);
		meleeWepBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.gameObject.SetActive(false);
		smgSubBtn.gameObject.SetActive(false);
		lmgSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.gameObject.SetActive(false);
		pistolSubBtn.gameObject.SetActive(false);
		launcherSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.gameObject.SetActive(false);
		boostersSubBtn.gameObject.SetActive(false);
		deployablesSubBtn.gameObject.SetActive(false);
		knivesSubBtn.gameObject.SetActive(false);
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
		equippedSupportSlot.SetActive(false);
		equippedMeleeSlot.SetActive(false);
	}

	void SwitchToMarketplaceEquipmentScreen() {
		// weaponsBtn.GetComponentInChildren<Text>().text = "Weapons";
		shopHeadgearBtn.gameObject.SetActive(true);
		shopFaceBtn.gameObject.SetActive(true);
		shopTopsBtn.gameObject.SetActive(true);
		shopBottomsBtn.gameObject.SetActive(true);
		shopFootwearBtn.gameObject.SetActive(true);
		shopCharacterBtn.gameObject.SetActive(true);
		shopArmorBtn.gameObject.SetActive(true);

		shopPrimaryWepBtn.gameObject.SetActive(false);
		shopSecondaryWepBtn.gameObject.SetActive(false);
		shopSupportWepBtn.gameObject.SetActive(false);
		shopMeleeWepBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopSmgSubBtn.gameObject.SetActive(false);
		shopLmgSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopLauncherSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopDeployablesSubBtn.gameObject.SetActive(false);
		shopKnivesSubBtn.gameObject.SetActive(false);
		shopModsBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSightsSubBtn.gameObject.SetActive(false);
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

	private void PopulateWeaponDropdownForModScreen() {
		// Clear dropdown from previous query
		// modWeaponSelect.ClearOptions();
		// Populate the dropdown with all weapons the player owns
		List<string> myWepsList = new List<string>();
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.inventory.myWeapons)
        {
            string weaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[weaponName];
			if (w.canBeModded) {
				myWepsList.Add(weaponName);
			}
		}
		// modWeaponSelect.AddOptions(myWepsList);

		// Initialize the dropdown with the first option
		// modWeaponSelect.value = 0;
		// LoadWeaponForModding(modWeaponSelect.options[modWeaponSelect.value].text);

	}

	public void LoadWeaponForModding(string weaponName) {
		// Destroy old weapon preview
		DestroyOldWeaponTemplate();
		// Load the proper weapon modding template
		// modWeaponLbl.text = weaponName;
		GameObject t = (GameObject)Instantiate(Resources.Load("WeaponTemplates/" + weaponName));

		// Place the weapon template in the proper position
		t.transform.SetParent(weaponPreviewSlot.transform);
		// t.transform.localPosition = t.GetComponent<WeaponMods>().previewPos;
		weaponPreviewRef = t;

		// Set base stats
		// SetWeaponModValues(modWeaponLbl.text, null);

		// Place the saved mods for that weapon back on the weapon template
		ModInfo savedModInfo = PlayerData.playerdata.LoadModDataForWeapon(weaponName);
		// SetWeaponModValues(modWeaponLbl.text, true, null, savedModInfo.SuppressorId, true, null, savedModInfo.SightId);
		EquipModOnWeaponTemplate(savedModInfo.EquippedSuppressor, "Suppressor", savedModInfo.SuppressorId);
		EquipModOnWeaponTemplate(savedModInfo.EquippedSight, "Sight", savedModInfo.SightId);

		// Update shop items with the mods that are equipped
		// If the suppressors menu was selected, update the shop items with what's equipped on the current weapon
		if (suppressorsBtn.GetComponent<Image>().color.r == (188f / 255f)) {
			OnSuppressorsBtnClicked();
		} else if (sightsBtn.GetComponent<Image>().color.r == (188f / 255f)) {
			OnSightsBtnClicked();
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
			if (string.IsNullOrEmpty(suppressorName)) {
				// equippedSuppressorTxt.text = "";
			} else {
				// equippedSuppressorTxt.text = suppressorName;
			}
			equippedSuppressorId = suppressorId;

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
			if (string.IsNullOrEmpty(sightName)) {
				// equippedSightTxt.text = "";
			} else {
				// equippedSightTxt.text = sightName;
			}
			equippedSightId = sightId;

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

	private void UnequipModFromWeaponTemplate(string modType) {
		// switch(modType) {
		// 	case "Suppressor":
		// 		if (equippedSuppressorSlot.GetComponent<ShopItemScript>() == null) return;
		// 		SetWeaponModValues(modWeaponLbl.text, true, null, equippedSuppressorId, false, null, null);
		// 		weaponPreviewRef.GetComponent<WeaponMods>().UnequipSuppressor();
		// 		break;
		// 	case "Sight":
		// 		if (equippedSightSlot.GetComponent<ShopItemScript>() == null) return;
		// 		SetWeaponModValues(modWeaponLbl.text, false, null, null, true, null, equippedSightId);
		// 		weaponPreviewRef.GetComponent<WeaponMods>().UnequipSight();
		// 		break;
		// }
	}

	private void SetWeaponModdedStats(float damage, float accuracy, float recoil, float range, float clipCapacity, float maxAmmo) {
		modDamageTxt.text = damage != -1 ? ""+damage : "-";
		modAccuracyTxt.text = accuracy != -1 ? ""+accuracy : "-";
		modRecoilTxt.text = recoil != -1 ? ""+recoil : "-";
		modRangeTxt.text = range != -1 ? ""+range : "-";
		modClipCapacityTxt.text = clipCapacity != -1 ? ""+clipCapacity : "-";
		modMaxAmmoTxt.text = maxAmmo != -1 ? ""+maxAmmo : "-";
	}

    public void RemoveSuppressorFromWeapon(string weaponName, bool removeSuppressorClicked)
    {
        PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().UnequipMod("Suppressor", weaponName);
        if (removeSuppressorClicked)
        {
            UnequipModFromWeaponTemplate("Suppressor");
        }
    }

	public void RemoveSightFromWeapon(string weaponName, bool removeSightClicked) {
		PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().UnequipMod("Sight", weaponName);
        if (removeSightClicked)
        {
            UnequipModFromWeaponTemplate("Sight");
        }
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

	private void DestroyOldWeaponTemplate() {
		// Destroy a weapon that is currently in the modding slot to make way for a new one
		if (weaponPreviewRef != null) {
			Destroy(weaponPreviewRef);
			weaponPreviewRef = null;
		}
		// Transform[] children = weaponPreviewSlot.GetComponentsInChildren<Transform>();
		// if (children.Length > 1) {
		// 	Destroy(children[1].gameObject);
		// }
	}

	private void SaveModsForCurrentWeapon() {
		// if (!modWeaponLbl.text.Equals("")) {
		// 	PlayerData.playerdata.SaveModDataForWeapon(modWeaponLbl.text, equippedSuppressorSlot.GetComponent<ShopItemScript>().itemName, equippedSightSlot.GetComponent<ShopItemScript>().itemName, equippedSuppressorId, equippedSightId);
		// }
	}

	public void OnWeaponModDropdownSelect() {
		// If the weapon that was selected is the same as the current one, then don't do anything
		// string selectedWeapon = modWeaponSelect.options[modWeaponSelect.value].text;
		// if (selectedWeapon.Equals(modWeaponLbl.text)) {
		// 	return;
		// }
		// First, destroy the old weapon that was being modded and save its data
		// SaveModsForCurrentWeapon();
		// DestroyOldWeaponTemplate();

		// Then create the new one
		// LoadWeaponForModding(selectedWeapon);
	}

	public string EquipModOnWeaponTemplate(string modName, string modType, string modId) {
		// if (modName == null || modName.Equals("")) return modWeaponLbl.text;
		// Weapon w = InventoryScript.itemData.weaponCatalog[modWeaponLbl.text];
		// switch(modType) {
		// 	case "Suppressor":
		// 		if (w.suppressorCompatible) {
		// 			SetWeaponModValues(modWeaponLbl.text, true, modName, modId, false, null, null);
		// 			weaponPreviewRef.GetComponent<WeaponMods>().EquipSuppressor(modName);
		// 			return modWeaponLbl.text;
		// 		} else {
		// 			ToggleModMenuPopup(true, "Suppressors cannot be equipped on this weapon!");
		// 		}
		// 		break;
		// 	case "Sight":
		// 		if (w.sightCompatible) {
		// 			SetWeaponModValues(modWeaponLbl.text, false, null, null, true, modName, modId);
		// 			weaponPreviewRef.GetComponent<WeaponMods>().EquipSight(modName);
		// 			return modWeaponLbl.text;
		// 		} else {
		// 			ToggleModMenuPopup(true, "Sights cannot be equipped on this weapon!");
		// 		}
		// 		break;
		// }
		return null;
	}

	private void ToggleModMenuPopup(bool b, string message) {
		// if (b) {
		// 	modMenuPopup.GetComponentInChildren<Text>().text = message;
		// 	modMenuPopup.SetActive(true);
		// } else {
		// 	modMenuPopup.SetActive(false);
		// }
	}

	public void PreparePurchase(string itemName, string itemType, Texture thumb) {
		PrepareDurationDropdown(itemType == "Mod");
		durationSelection.index = 0;
		// durationSelection.RefreshShownValue();
		itemBeingPurchased = itemName;
		typeBeingPurchased = itemType;
		// preparePurchasePopup.GetComponentInChildren<RawImage>().texture = thumb;
		// preparePurchasePopup.SetActive(true);
        // Initialize with 1 day price
        SetTotalGPCost(0, "1 day");
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
		// preparePurchasePopup.SetActive(false);
		// confirmPurchaseTxt.text = "Are you sure you would like to buy " + itemBeingPurchased + " for " +
		// durationSelectionDropdown.options[durationSelectionDropdown.value].text + "? (" + totalGpCostBeingPurchased + " GP)";
		// confirmPurchasePopup.SetActive(true);
	}

	public void OnConfirmPurchaseClicked() {
		ConfirmPurchase();
	}

	public void OnDurationSelect() {
		int durationInput = durationSelection.index;
        SetTotalGPCost(durationInput, durationSelection.GetCurrentItem());
	}

    void SetTotalGPCost(int duration, string durationText)
    {
        totalGpCostBeingPurchased = GetGPCostForItemAndType(itemBeingPurchased, typeBeingPurchased, duration);
        totalGpCostTxt.text = "YOU ARE BUYING [" + itemBeingPurchased + "] FOR [" + durationText + "] FOR " + totalGpCostBeingPurchased + " GP/KASH.";
    }

	public void OnCancelPurchaseClicked() {
		itemBeingPurchased = null;
		typeBeingPurchased = null;
		// confirmPurchasePopup.SetActive(false);
	}

	public void OnCancelPreparePurchaseClicked() {
		itemBeingPurchased = null;
		typeBeingPurchased = null;
		// preparePurchasePopup.SetActive(false);
	}

	void ConfirmPurchase() {
		// confirmPurchasePopup.SetActive(false);
        // Ensure that the user doesn't already have this item
        float hasDuplicateCheck = HasDuplicateItem(itemBeingPurchased, typeBeingPurchased);
        if (hasDuplicateCheck < 0f) {
			TriggerAlertPopup("You already own this item.");
			return;
		}
        bool isStacking = (hasDuplicateCheck >= 0f && !Mathf.Approximately(0f, hasDuplicateCheck));
        float totalNewDuration = ConvertDurationInput(durationSelection.index);
        totalNewDuration = (Mathf.Approximately(totalNewDuration, -1f) ? totalNewDuration : totalNewDuration + hasDuplicateCheck);
		if (PlayerData.playerdata.info.Gp >= totalGpCostBeingPurchased) {
			PlayerData.playerdata.AddItemToInventory(itemBeingPurchased, typeBeingPurchased, totalNewDuration, true, "gp");
		} else {	
			TriggerAlertPopup("You do not have enough GP to purchase this item.");	
		}	
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



	uint GetGPCostForItemAndType(string itemName, string itemType, int duration) {
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
			return (uint)(InventoryScript.itemData.armorCatalog[itemName].gpPrice * durationMultiplier);
		} else if (itemType.Equals("Character")) {
			return (uint)(InventoryScript.itemData.characterCatalog[itemName].gpPrice * durationMultiplier);
		} else if (itemType.Equals("Weapon")) {
			return (uint)(InventoryScript.itemData.weaponCatalog[itemName].gpPrice * durationMultiplier);
		} else if (itemType.Equals("Mod")) {
			return (uint)(InventoryScript.itemData.modCatalog[itemName].gpPrice * durationMultiplier);
		}
		return (uint)(InventoryScript.itemData.equipmentCatalog[itemName].gpPrice * durationMultiplier);
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

	void ResetMarketplaceButtons() {
		shopHeadgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFaceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopArmorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopTopsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopFootwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopMeleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
	}

	void ResetCustomizationButtons() {
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		meleeWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
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
        // emergencyPopupTxt.text = message;
        // .SetActive(true);
    }

    public void CloseEmergencyPopup() {
        // emergencyPopup.SetActive(false);
    }

	public void TogglePlayerBody(bool b) {
		PlayerData.playerdata.bodyReference.SetActive(b);
		PlayerData.playerdata.bodyReference.GetComponent<Animator>().SetBool("onTitle", true);
	}
		
}
