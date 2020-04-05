using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;
using TMPro;

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

    private Vector3 customizationCameraPos = new Vector3(-4.7f, 4.08f, 21.5f);
	private Vector3 defaultCameraPos = new Vector3(-7.3f, 4.08f, 22.91f);
	private Vector3 defaultCameraRot = new Vector3(10f, 36.2f, 0f);
	private Vector3 modCameraRot = new Vector3(3.5f, 43.5f, 0f);
	private Vector3 modCameraPos = new Vector3(2f, 4.5f, 26.7f);
	private int camPos;
	private int previousCamPos;
	private float camMoveTimer;
	public GameObject itemDescriptionPopupRef;
	public GameObject modDescriptionPopupRef;
	public GameObject marketplaceItemDescriptionPopupRef;
	public GameObject marketplaceModDescriptionPopupRef;

	public GameObject mainMenu;
	public Text mainNametagTxt;
	public RawImage mainRankImg;
	public Text mainRankTxt;
	public Slider mainLevelProgress;
	public Text mainLevelProgressPercentTxt;
	public Text mainExpTxt;
	public Camera mainCam;
	public Text titleText;
	//public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;
    public GameObject versusMenu;
	public GameObject marketplaceMenu;
	public GameObject modMenu;
	public GameObject keyBindingsPopup;
	public GameObject loadingScreen;
	public GameObject jukebox;
	public GameObject mainMenuPopup;
	public GameObject customizationMenuPopup;
	public GameObject marketplaceMenuPopup;
	public GameObject modMenuPopup;
	private string itemBeingPurchased;
	private string typeBeingPurchased;
	private uint totalGpCostBeingPurchased;
	public char currentCharGender;
	public bool equipsModifiedFlag;

	// Loading screen stuff
	public RawImage screenArt;
	public Image bottomShade;
	public Image topShade;
	public Slider progressBar;
	public Text proTipText;
	public Text mapTitleText;
	private short loadingStatus;
	private float t;
	private string[] proTips = new string[2]{"Aim for the head for faster kills.", "Be on the lookout for ammo and health drops from enemies."};
	private bool versionWarning;
	public GameObject splashScreen;
	public Text splashScreenPopup;
	// Marketplace menu
	public Button clearPreviewBtn;
	public Button marketplaceBackBtn;
	public GameObject shopContentPrefab;
	public GameObject shopContentInventory;
	public Button shopHeadgearBtn;
	public Button shopFaceBtn;
	public Button shopArmorBtn;
	public Button shopTopsBtn;
	public Button shopBottomsBtn;
	public Button shopFootwearBtn;
	public Button shopCharacterBtn;
	public GameObject shopEquippedHeadSlot;
	public GameObject shopEquippedFaceSlot;
	public GameObject shopEquippedTopSlot;
	public GameObject shopEquippedBottomSlot;
	public GameObject shopEquippedFootSlot;
	public GameObject shopEquippedCharacterSlot;
	public GameObject shopEquippedArmorSlot;
	public GameObject shopCurrentlyEquippedItemPrefab;
	public GameObject shopEquippedStatsSlot;
	public Text shopArmorBoostPercent;
	public Text shopSpeedBoostPercent;
	public Text shopStaminaBoostPercent;
	public Button shopPrimaryWepBtn;
	public Button shopSecondaryWepBtn;
	public Button shopSupportWepBtn;
	public Button shopAssaultRifleSubBtn;
	public Button shopShotgunSubBtn;
	public Button shopSniperRifleSubBtn;
	public Button shopPistolSubBtn;
	public Button shopExplosivesSubBtn;
	public Button shopBoostersSubBtn;
	public Button shopModsBtn;
	public Button shopSuppressorsSubBtn;
	public GameObject shopEquippedPrimarySlot;
	public GameObject shopEquippedSecondarySlot;
	public GameObject shopEquippedSupportSlot;
	public GameObject preparePurchasePopup;
	public GameObject confirmPurchasePopup;
	public Text confirmPurchaseTxt;
	public Dropdown durationSelectionDropdown;
	public Text totalGpCostTxt;
	public Text myGpTxt;
	public Text myKashTxt;

	// Customization menu
	public GameObject contentPrefab;
	public GameObject contentInventory;
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
	public GameObject equippedStatsSlot;
	public Text armorBoostPercent;
	public Text speedBoostPercent;
	public Text staminaBoostPercent;
	public Button loadoutBtn;
	public Button weaponsBtn;
	public Button primaryWepBtn;
	public Button secondaryWepBtn;
	public Button supportWepBtn;
	// Number of sub buttons for primary weapon dropdown in shop
	private const uint PRIMARY_SUBBUTTON_COUNT = 3;
	private const uint SECONDARY_SUBBUTTON_COUNT = 1;
	private const uint SUPPORT_SUBBUTTON_COUNT = 2;
	private const uint MOD_SUBBUTTON_COUNT = 1;
	private const float SHOP_BUTTON_SPACING = -30f;
	private float primaryWepBtnYPos = -65f;
	private float secondaryWepBtnYPos = -95f;
	private float supportWepBtnYPos = -125f;
	private float modWepBtnYPos = -155f;
	public Button assaultRifleSubBtn;
	public Button shotgunSubBtn;
	public Button sniperRifleSubBtn;
	public Button pistolSubBtn;
	public Button explosivesSubBtn;
	public Button boostersSubBtn;
	public GameObject equippedPrimarySlot;
	public GameObject equippedSecondarySlot;
	public GameObject equippedSupportSlot;

	// Mod menu
	public GameObject currentlyEquippedModPrefab;
	public GameObject weaponPreviewSlot;
	public GameObject weaponPreviewRef;
	public GameObject modInventoryContent;
	public Button suppressorsBtn;
	public TMP_Dropdown modWeaponSelect;
	public Text modWeaponLbl;
	public Text equippedSuppressorTxt;
	public string equippedSuppressorId;
	public Text modDamageTxt;
	public Text modAccuracyTxt;
	public Text modRecoilTxt;
	public Text modRangeTxt;
	public Text modClipCapacityTxt;
	public Text modMaxAmmoTxt;
    public Button removeSuppressorBtn;

	// Use this for initialization
	void Awake() {
		if (PlayerData.playerdata == null) {
			ToggleSplashScreen(true, "Loading player details...");
		}
	}

	void Start () {
		//PlayerData.playerdata.FindBodyRef ();
		titleText.enabled = true;
		mainMenu.SetActive (true);
		loadingStatus = 0;
		previousCamPos = 0;
		camPos = 0;
		camMoveTimer = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Safety destroy previous match data
		foreach (PlayerStat entry in GameControllerScript.playerList.Values)
		{
			Destroy(entry.objRef);
		}
		GameControllerScript.playerList.Clear();
		SetPlayerStatsForTitle();

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
		jukebox.GetComponent<AudioSource> ().Stop ();
		if (mapName.Equals ("Badlands: Act I")) {
			screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/badlands1_load");
		}
        proTipText.text = proTips[Random.Range(0, 2)];
		mapTitleText.text = mapName;
		mapTitleText.rectTransform.localPosition = new Vector3 (1200f, mapTitleText.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z);
		topShade.rectTransform.localPosition = new Vector3 (-1200f, topShade.rectTransform.localPosition.y, topShade.rectTransform.localPosition.z);
		bottomShade.rectTransform.localPosition = new Vector3 (1200f, bottomShade.rectTransform.localPosition.y, bottomShade.rectTransform.localPosition.z);
		loadingStatus = 1;
		loadingScreen.SetActive (true);
	}

	void Update() {
		if (splashScreen.activeInHierarchy && PlayerData.playerdata.bodyReference != null) {
			ToggleSplashScreen(false);
		}

		if (PlayerData.playerdata.disconnectedFromServer) {
			PlayerData.playerdata.disconnectedFromServer = false;
			TriggerMainPopup("Lost connection to server.\nReason: " + PlayerData.playerdata.disconnectReason);
			PlayerData.playerdata.disconnectReason = "";
		} else if (versionWarning) {
			versionWarning = false;
			TriggerMainPopup("Your game is not updated to the latest version of Fireteam AI!\nThis may affect your matchmaking experience.");
		}
		if (loadingScreen.activeInHierarchy) {
			progressBar.value = PhotonNetwork.LevelLoadingProgress;
			// First stage of loading animation
			if (loadingStatus == 1) {
				t += (Time.deltaTime * 0.75f);
				mapTitleText.rectTransform.localPosition = Vector3.Lerp (new Vector3 (1200f, mapTitleText.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z), new Vector3 (120f, mapTitleText.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z), t);
				topShade.rectTransform.localPosition = Vector3.Lerp (new Vector3 (-1200f, topShade.rectTransform.localPosition.y, topShade.rectTransform.localPosition.z), new Vector3 (0f, topShade.rectTransform.localPosition.y, topShade.rectTransform.localPosition.z), t);
				bottomShade.rectTransform.localPosition = Vector3.Lerp (new Vector3 (1200f, bottomShade.rectTransform.localPosition.y, bottomShade.rectTransform.localPosition.z), new Vector3 (0f, bottomShade.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z), t);
				if (t >= 1f) {
					loadingStatus = 2;
					t = 0f;
				}
			} else if (loadingStatus == 2) {
				// Second stage of loading animation
				t += (Time.deltaTime * 0.2f);
				mapTitleText.rectTransform.localPosition = Vector3.Lerp (new Vector3 (120f, mapTitleText.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z), new Vector3 (350f, mapTitleText.rectTransform.localPosition.y, mapTitleText.rectTransform.localPosition.z), t);
				if (t >= 1f) {
					loadingStatus = 0;
					t = 0f;
				}
			}
		} else {
			if (!matchmakingMenu.activeInHierarchy && !versusMenu.activeInHierarchy) {
				// If going to main menu screen
				if (camPos == 0) {
					mainCam.transform.position = Vector3.Lerp(customizationCameraPos, defaultCameraPos, camMoveTimer);
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, defaultCameraPos) && !keyBindingsPopup.activeInHierarchy) {
						titleText.enabled = true;
						mainMenu.SetActive(true);
					}
				// If going to customization screen
				} else if (camPos == 1) {
					if (previousCamPos == 0) {
						mainCam.transform.position = Vector3.Lerp(defaultCameraPos, customizationCameraPos, camMoveTimer);
					} else if (previousCamPos == 3) {
						mainCam.transform.position = Vector3.Lerp(modCameraPos, customizationCameraPos, camMoveTimer);
						mainCam.transform.rotation = Quaternion.Lerp(Quaternion.Euler(modCameraRot), Quaternion.Euler(defaultCameraRot), camMoveTimer);						
					}
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, customizationCameraPos)) {
						if (!customizationMenu.activeInHierarchy) {
							customizationMenu.SetActive(true);
						}
					}
				// If going to marketplace screen
				} else if (camPos == 2) {
					if (previousCamPos == 0) {
						mainCam.transform.position = Vector3.Lerp(defaultCameraPos, customizationCameraPos, camMoveTimer);
					}
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, customizationCameraPos)) {
						if (!marketplaceMenu.activeInHierarchy) {
							marketplaceMenu.SetActive(true);
						}
					}
				// If going to modification menu screen
				} else if (camPos == 3) {
					mainCam.transform.position = Vector3.Lerp(customizationCameraPos, modCameraPos, camMoveTimer);
					mainCam.transform.rotation = Quaternion.Lerp(Quaternion.Euler(defaultCameraRot), Quaternion.Euler(modCameraRot), camMoveTimer);
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, modCameraPos)) {
						if (!modMenu.activeInHierarchy) {
							// Bring up the mod menu
							modMenu.SetActive(true);
							// Set up the weapon dropdown, stats, and template weapon
							PopulateWeaponDropdownForModScreen();
                            // Start on the suppressors menu
                            OnSuppressorsBtnClicked();
						}
					}
				}
			}
		}
	}

	public void GoToMatchmakingMenu() {
		if (!PhotonNetwork.IsConnected) {
			PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
			PhotonNetwork.ConnectUsingSettings();
		}

		titleText.enabled = false;
		mainMenu.SetActive (false);
		customizationMenu.SetActive (false);
        versusMenu.SetActive(false);
		matchmakingMenu.SetActive (true);
	}

    public void GoToVersusMenu()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
            PhotonNetwork.ConnectUsingSettings();
        }

        titleText.enabled = false;
        mainMenu.SetActive(false);
        customizationMenu.SetActive(false);
        versusMenu.SetActive(true);
    }
		
	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
        versusMenu.SetActive(false);
		keyBindingsPopup.SetActive(false);
		titleText.enabled = true;
		mainMenu.SetActive(true);
	}

	public void GoToKeyBindings() {
		keyBindingsPopup.SetActive(true);
		titleText.enabled = false;
		mainMenu.SetActive (false);
	}

	public void ReturnToMainMenuFromCustomization() {
        // Save settings if the settings are active
		 if (customizationMenu.activeInHierarchy) {
			if (equipsModifiedFlag) {
				savePlayerData ();
				equipsModifiedFlag = false;
			}
			ClearCustomizationContent();
			ResetCustomizationButtons();
		 }
		SwitchToEquipmentScreen();
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
        versusMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 0;
		camMoveTimer = 0f;
	}

	public void ReturnToMainMenuFromMarketplace() {
		// Clear previews and return to main menu
		 if (marketplaceMenu.activeInHierarchy) {
		 	ClearPreview();
			ClearMarketplaceContent();
			ResetMarketplaceButtons();
		 }
		SwitchToMarketplaceEquipmentScreen();
		marketplaceMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
        versusMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 0;
		camMoveTimer = 0f;
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
        matchmakingMenu.SetActive(false);
        versusMenu.SetActive(false);
		titleText.enabled = true;
        mainMenu.SetActive(true);
    }

    public void GoToCustomization() {
		titleText.enabled = false;
		mainMenu.SetActive (false);
		modMenu.SetActive(false);
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
		equippedSupportSlot.SetActive(false);
		matchmakingMenu.SetActive (false);
        versusMenu.SetActive(false);
		marketplaceMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 1;
		camMoveTimer = 0f;
	}

	public void GoToMarketplace() {
		titleText.enabled = false;
		mainMenu.SetActive (false);
		modMenu.SetActive(false);
		shopEquippedPrimarySlot.SetActive(false);
		shopEquippedSecondarySlot.SetActive(false);
		shopEquippedSupportSlot.SetActive(false);
		matchmakingMenu.SetActive (false);
        versusMenu.SetActive(false);
		customizationMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 2;
		camMoveTimer = 0f;
	}

	public void GoToMod() {
		mainMenu.SetActive(false);
		customizationMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 3;
		camMoveTimer = 0f;
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
		// Destroy weapon template
		Destroy(weaponPreviewRef);
		weaponPreviewRef = null;
	}

	public void goToMainMenu (){
		PlayerPrefs.SetString ("newScene", "MainMenu");
		SceneManager.LoadScene(7);
	}

	public void quitGame() {
		Application.Quit ();
	}

	public void savePlayerData()
	{
		PlayerData.playerdata.SavePlayerData();
	}

	public void ClosePopup() {
		mainMenuPopup.SetActive (false);
		if (marketplaceMenuPopup.activeInHierarchy) {
			ToggleActiveMarketplaceButtons(true);
		}
		marketplaceMenuPopup.SetActive (false);
		modMenuPopup.SetActive(false);
		customizationMenuPopup.SetActive(false);
	}

    public void TriggerExpirationPopup(ArrayList expiredItems)
    {
        TriggerMainPopup("The following items have expired and have been deleted from your inventory:\n" + string.Join(", ", expiredItems.ToArray()));
    }

	public void TriggerMainPopup(string message) {
		mainMenuPopup.GetComponentInChildren<Text> ().text = message;
		mainMenuPopup.SetActive (true);
	}

	public void TriggerCustomizationPopup(string message) {
		customizationMenuPopup.GetComponentInChildren<Text>().text = message;
		customizationMenuPopup.SetActive(true);
	}

	public void TriggerMarketplacePopup(string message) {
		marketplaceMenuPopup.GetComponentInChildren<Text>().text = message;
		marketplaceMenuPopup.SetActive(true);
	}

	// Clears existing items from the shop panel
	void ClearCustomizationContent() {
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}
	}

	void ClearMarketplaceContent() {
		RawImage[] existingThumbnails = shopContentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.myHeadgear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = ed.name;
			Equipment thisHeadgear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisHeadgear;
			s.itemName = thisItemName;
            s.itemType = "Headgear";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = thisHeadgear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisHeadgear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedHeadgear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.myFacewear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = ed.name;
			Equipment thisFacewear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFacewear;
			s.itemName = thisItemName;
            s.itemType = "Facewear";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = thisFacewear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisFacewear.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFacewear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, ArmorData> entry in PlayerData.playerdata.myArmor)
        {
            ArmorData ed = entry.Value;
			string thisItemName = ed.name;
			Armor thisArmor = InventoryScript.itemData.armorCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.armorDetails = thisArmor;
			s.itemName = thisItemName;
            s.itemType = "Armor";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = thisArmor.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(thisArmor.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedArmor)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

		// Populate into grid layout
		foreach(KeyValuePair<string, Armor> entry in InventoryScript.itemData.armorCatalog) {
			Armor thisArmor = entry.Value;
			if (!thisArmor.category.Equals("Armor") || !thisArmor.purchasable) {
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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.myTops)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 4f, t.sizeDelta.y / 4f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedTop)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.myBottoms)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = ed.name;
			Equipment thisBottom = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisBottom;
			s.itemName = thisItemName;
            s.itemType = "Bottom";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = thisBottom.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedBottom)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, EquipmentData> entry in PlayerData.playerdata.myFootwear)
        {
            EquipmentData ed = entry.Value;
			string thisItemName = ed.name;
			Equipment thisFootwear = InventoryScript.itemData.equipmentCatalog[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.equipmentDetails = thisFootwear;
			s.itemName = thisItemName;
            s.itemType = "Footwear";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = thisFootwear.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[thisItemName].thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (thisItemName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedFootwear)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	float CalculateButtonPositioning(float startingPoint, uint places) {
		// Calculate button positioning
		return startingPoint + (places * SHOP_BUTTON_SPACING);
	}

	public void OnPrimaryWepBtnClicked() {
		// Moving secondary and support button down for submenu
		RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));

		// Change all button colors
		primaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Remove sub buttons
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(false);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		assaultRifleSubBtn.gameObject.SetActive(true);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(true);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(true);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons) {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplacePrimaryWepBtnClicked() {
		// Moving secondary and support button down for submenu
		RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		rt = shopSupportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));
		rt = shopModsBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, PRIMARY_SUBBUTTON_COUNT));

		// Change all button colors
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Remove sub buttons
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(true);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(true);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(true);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnSecondaryWepBtnClicked() {
		// Change all button colors
		secondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		explosivesSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(false);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		pistolSubBtn.gameObject.SetActive(true);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceSecondaryWepBtnClicked() {
		// Change all button colors
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = shopSupportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));
		rt = shopModsBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, SECONDARY_SUBBUTTON_COUNT));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopPistolSubBtn.gameObject.SetActive(true);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnSupportWepBtnClicked() {
		// Change all button colors
		supportWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		explosivesSubBtn.gameObject.SetActive(true);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		boostersSubBtn.gameObject.SetActive(true);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceSupportWepBtnClicked() {
		// Change all button colors
		shopSupportWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = shopSupportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		rt = shopModsBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, SUPPORT_SUBBUTTON_COUNT));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSuppressorsSubBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopExplosivesSubBtn.gameObject.SetActive(true);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(true);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnAssaultRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with assault rifles
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceAssaultRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnShotgunSubBtnClicked() {
		// Make tab orange and clear other tabs
		shotgunSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with shotguns
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceShotgunSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnSniperRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with sniper rifles
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceSniperRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnPistolSubBtnClicked() {
		// Make tab orange and clear other tabs
		pistolSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplacePistolSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopPistolSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnExplosivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		explosivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		boostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceExplosivesSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnBoostersSubBtnClicked() {
		// Make tab orange and clear other tabs
		boostersSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		explosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

        // Populate with pistols
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            WeaponData ed = entry.Value;
			string thisWeaponName = ed.name;
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
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
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
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnMarketplaceBoostersSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnMarketplaceModsBtnClicked() {
		// Change all button colors
		shopModsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopPrimaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSupportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSecondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = shopSupportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		rt = shopModsBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, 0));

		// Remove sub buttons
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		shopSuppressorsSubBtn.gameObject.SetActive(true);
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnMarketplaceSuppressorsSubBtnClicked() {
		// Make tab orange and clear other tabs
		shopSuppressorsSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
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
		characterBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

        // Populate into grid layout
        foreach (KeyValuePair<string, CharacterData> entry in PlayerData.playerdata.myCharacters)
        {
            CharacterData ed = entry.Value;
			string thisCharacterName = ed.name;
			Character c = InventoryScript.itemData.characterCatalog[thisCharacterName];
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.itemDescriptionPopupRef = itemDescriptionPopupRef;
			s.characterDetails = c;
			s.itemName = thisCharacterName;
            s.itemType = "Character";
			s.duration = ed.duration;
			s.acquireDate = ed.acquireDate;
			s.itemDescription = c.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (thisCharacterName.Equals(PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		shopCharacterBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

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
			o.transform.SetParent(shopContentInventory.transform);
		}
	}

	public void OnLoadoutBtnClicked() {
		Text t = loadoutBtn.GetComponentInChildren<Text>();
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearCustomizationContent();

		// If you're on equipment screen, go to loadout screen. Else, go back to loadout.
		if (t.text.Equals("Loadout")) {
			t.text = "Equipment";
			SwitchToLoadoutScreen();
		} else {
			t.text = "Loadout";
			SwitchToEquipmentScreen();
		}
	}

	public void OnWeaponsBtnClicked() {
		Text t = weaponsBtn.GetComponentInChildren<Text>();
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
		shopModsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shopCharacterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearMarketplaceContent();

		// If you're on equipment screen, go to loadout screen. Else, go back to loadout.
		if (t.text.Equals("Weapons")) {
			t.text = "Equipment";
			SwitchToMarketplaceWeaponsScreen();
		} else {
			t.text = "Weapons";
			SwitchToMarketplaceEquipmentScreen();
		}
	}

	public void OnSuppressorsBtnClicked() {
		// Change all button colors
		suppressorsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearModCustomizationContent();
		WeaponScript ws = PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>();

        // Populate into grid layout
        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.myMods)
        {
            ModData modData = entry.Value;
			string thisModName = modData.name;
			Mod m = InventoryScript.itemData.modCatalog[thisModName];
			if (!m.category.Equals("Suppressor")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			ShopItemScript s = o.GetComponent<ShopItemScript>();
			s.modDescriptionPopupRef = modDescriptionPopupRef;
			s.modDetails = m;
			s.id = modData.id;
			s.equippedOn = modData.equippedOn;
			s.itemName = m.name;
            s.itemType = "Mod";
			s.itemDescription = m.description;
			s.modCategory = m.category;
			s.thumbnailRef.texture = (Texture)Resources.Load(m.thumbnailPath);
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (modWeaponLbl.text.Equals(modData.equippedOn)) {
				s.ToggleEquippedIndicator(true);
				currentlyEquippedModPrefab = o;
			}
			o.transform.SetParent(modInventoryContent.transform);
		}
	}

	void SwitchToLoadoutScreen() {
		loadoutBtn.GetComponentInChildren<Text>().text = "Equipment";
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
		equippedStatsSlot.SetActive(false);

		primaryWepBtn.gameObject.SetActive(true);
		secondaryWepBtn.gameObject.SetActive(true);
		supportWepBtn.gameObject.SetActive(true);
		RectTransform rt = primaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		equippedPrimarySlot.SetActive(true);
		equippedSecondarySlot.SetActive(true);
		equippedSupportSlot.SetActive(true);
	}

	void SwitchToMarketplaceWeaponsScreen() {
		weaponsBtn.GetComponentInChildren<Text>().text = "Equipment";
		shopHeadgearBtn.gameObject.SetActive(false);
		shopFaceBtn.gameObject.SetActive(false);
		shopTopsBtn.gameObject.SetActive(false);
		shopBottomsBtn.gameObject.SetActive(false);
		shopFootwearBtn.gameObject.SetActive(false);
		shopCharacterBtn.gameObject.SetActive(false);
		shopArmorBtn.gameObject.SetActive(false);

		shopEquippedHeadSlot.SetActive(false);
		shopEquippedFaceSlot.SetActive(false);
		shopEquippedTopSlot.SetActive(false);
		shopEquippedBottomSlot.SetActive(false);
		shopEquippedFootSlot.SetActive(false);
		shopEquippedCharacterSlot.SetActive(false);
		shopEquippedArmorSlot.SetActive(false);
		shopEquippedStatsSlot.SetActive(false);

		shopPrimaryWepBtn.gameObject.SetActive(true);
		shopSecondaryWepBtn.gameObject.SetActive(true);
		shopSupportWepBtn.gameObject.SetActive(true);
		shopModsBtn.gameObject.SetActive(true);
		RectTransform rt = shopPrimaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(primaryWepBtnYPos, 0));
		rt = shopSecondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(secondaryWepBtnYPos, 0));
		rt = shopSupportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(supportWepBtnYPos, 0));
		rt = shopModsBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, CalculateButtonPositioning(modWepBtnYPos, 0));
		shopEquippedPrimarySlot.SetActive(true);
		shopEquippedSecondarySlot.SetActive(true);
		shopEquippedSupportSlot.SetActive(true);
	}

	void SwitchToEquipmentScreen() {
		loadoutBtn.GetComponentInChildren<Text>().text = "Loadout";
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
		equippedStatsSlot.SetActive(true);

		primaryWepBtn.gameObject.SetActive(false);
		secondaryWepBtn.gameObject.SetActive(false);
		supportWepBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.gameObject.SetActive(false);
		pistolSubBtn.gameObject.SetActive(false);
		explosivesSubBtn.gameObject.SetActive(false);
		boostersSubBtn.gameObject.SetActive(false);
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
		equippedSupportSlot.SetActive(false);
	}

	void SwitchToMarketplaceEquipmentScreen() {
		weaponsBtn.GetComponentInChildren<Text>().text = "Weapons";
		shopHeadgearBtn.gameObject.SetActive(true);
		shopFaceBtn.gameObject.SetActive(true);
		shopTopsBtn.gameObject.SetActive(true);
		shopBottomsBtn.gameObject.SetActive(true);
		shopFootwearBtn.gameObject.SetActive(true);
		shopCharacterBtn.gameObject.SetActive(true);
		shopArmorBtn.gameObject.SetActive(true);

		shopEquippedHeadSlot.SetActive(true);
		shopEquippedFaceSlot.SetActive(true);
		shopEquippedTopSlot.SetActive(true);
		shopEquippedBottomSlot.SetActive(true);
		shopEquippedFootSlot.SetActive(true);
		shopEquippedCharacterSlot.SetActive(true);
		shopEquippedArmorSlot.SetActive(true);
		shopEquippedStatsSlot.SetActive(true);

		shopPrimaryWepBtn.gameObject.SetActive(false);
		shopSecondaryWepBtn.gameObject.SetActive(false);
		shopSupportWepBtn.gameObject.SetActive(false);
		shopAssaultRifleSubBtn.gameObject.SetActive(false);
		shopShotgunSubBtn.gameObject.SetActive(false);
		shopSniperRifleSubBtn.gameObject.SetActive(false);
		shopPistolSubBtn.gameObject.SetActive(false);
		shopExplosivesSubBtn.gameObject.SetActive(false);
		shopBoostersSubBtn.gameObject.SetActive(false);
		shopModsBtn.gameObject.SetActive(false);
		shopSuppressorsSubBtn.gameObject.SetActive(false);

		shopEquippedPrimarySlot.SetActive(false);
		shopEquippedSecondarySlot.SetActive(false);
		shopEquippedSupportSlot.SetActive(false);
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
		modWeaponSelect.ClearOptions();
		// Populate the dropdown with all weapons the player owns
		List<string> myWepsList = new List<string>();
        foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
        {
            string weaponName = entry.Key;
			Weapon w = InventoryScript.itemData.weaponCatalog[weaponName];
			if (w.canBeModded) {
				myWepsList.Add(weaponName);
			}
		}
		modWeaponSelect.AddOptions(myWepsList);

		// Initialize the dropdown with the first option
		modWeaponSelect.value = 0;
		LoadWeaponForModding(modWeaponSelect.options[modWeaponSelect.value].text);

	}

	public void LoadWeaponForModding(string weaponName) {
		// Destroy old weapon preview
		DestroyOldWeaponTemplate();
		// Load the proper weapon modding template
		modWeaponLbl.text = weaponName;
		GameObject t = (GameObject)Instantiate(Resources.Load("WeaponTemplates/" + weaponName));

		// Place the weapon template in the proper position
		t.transform.SetParent(weaponPreviewSlot.transform);
		t.transform.localPosition = Vector3.zero;
		weaponPreviewRef = t;

		// Set base stats
		// SetWeaponModValues(modWeaponLbl.text, null);

		// Place the saved mods for that weapon back on the weapon template
		ModInfo savedModInfo = PlayerData.playerdata.LoadModDataForWeapon(weaponName);
		SetWeaponModValues(modWeaponLbl.text, null, savedModInfo.id);
		EquipModOnWeaponTemplate(savedModInfo.equippedSuppressor, "Suppressor", savedModInfo.id);

		// Update shop items with the mods that are equipped
		// If the suppressors menu was selected, update the shop items with what's equipped on the current weapon
		if (suppressorsBtn.GetComponent<Image>().color.r == (188f / 255f)) {
			OnSuppressorsBtnClicked();
		}
	}

	public void SetWeaponModValues(string weaponName, string suppressorName, string id) {
		modWeaponLbl.text = weaponName;
		if (suppressorName == null || suppressorName.Equals("")) {
			equippedSuppressorTxt.text = "";
		} else {
			equippedSuppressorTxt.text = suppressorName;
		}
        equippedSuppressorId = id;
		
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

		// Add suppressor stats
		if (suppressorName != null && !suppressorName.Equals("")) {
			Mod suppressor = InventoryScript.itemData.modCatalog[suppressorName];
			damageBoost += suppressor.damageBoost;
			accuracyBoost += suppressor.accuracyBoost;
			recoilBoost += suppressor.recoilBoost;
			rangeBoost += suppressor.rangeBoost;
			clipCapacityBoost += suppressor.clipCapacityBoost;
			maxAmmoBoost += suppressor.maxAmmoBoost;
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
		switch(modType) {
			case "Suppressor":
				if (equippedSuppressorTxt.Equals("")) return;
				SetWeaponModValues(modWeaponLbl.text, null, equippedSuppressorId);
				weaponPreviewRef.GetComponent<WeaponMods>().UnequipSuppressor();
				break;
		}
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
		if (!modWeaponLbl.text.Equals("")) {
			PlayerData.playerdata.SaveModDataForWeapon(modWeaponLbl.text, equippedSuppressorTxt.text, equippedSuppressorId);
		}
	}

	public void OnWeaponModDropdownSelect() {
		// If the weapon that was selected is the same as the current one, then don't do anything
		string selectedWeapon = modWeaponSelect.options[modWeaponSelect.value].text;
		if (selectedWeapon.Equals(modWeaponLbl.text)) {
			return;
		}
		// First, destroy the old weapon that was being modded and save its data
		SaveModsForCurrentWeapon();
		DestroyOldWeaponTemplate();

		// Then create the new one
		LoadWeaponForModding(selectedWeapon);
	}

	public string EquipModOnWeaponTemplate(string modName, string modType, string modId) {
		if (modName == null || modName.Equals("")) return modWeaponLbl.text;
		Weapon w = InventoryScript.itemData.weaponCatalog[modWeaponLbl.text];
		switch(modType) {
			case "Suppressor":
				if (w.suppressorCompatible) {
					SetWeaponModValues(modWeaponLbl.text, modName, modId);
					weaponPreviewRef.GetComponent<WeaponMods>().EquipSuppressor(modName);
					return modWeaponLbl.text;
				} else {
					ToggleModMenuPopup(true, "Suppressors cannot be equipped on this weapon!");
				}
				break;
		}
		return null;
	}

	private void ToggleModMenuPopup(bool b, string message) {
		if (b) {
			modMenuPopup.GetComponentInChildren<Text>().text = message;
			modMenuPopup.SetActive(true);
		} else {
			modMenuPopup.SetActive(false);
		}
	}

	void ToggleSplashScreen(bool b, string message = "") {
		splashScreen.SetActive(b);
		splashScreenPopup.text = message;
	}

	public void CloseGameOnError() {
		ToggleSplashScreen(true, "Your data could not be loaded. Either your data is corrupted or the service is unavailable. Please check the webiste for further details. If this issue persists, please create a ticket at koobando.com/support.");
		StartCoroutine(CloseGameOnErrorRoutine());
	}

	IEnumerator CloseGameOnErrorRoutine() {
		yield return new WaitForSeconds(8f);
		Application.Quit();
	}

	public void PreparePurchase(string itemName, string itemType, Texture thumb) {
		PrepareDurationDropdown(itemType == "Mod");
		durationSelectionDropdown.value = 0;
		durationSelectionDropdown.RefreshShownValue();
		ToggleActiveMarketplaceButtons(false);
		itemBeingPurchased = itemName;
		typeBeingPurchased = itemType;
		preparePurchasePopup.GetComponentInChildren<RawImage>().texture = thumb;
		preparePurchasePopup.SetActive(true);
        // Initialize with 1 day price
        SetTotalGPCost(0);
    }

	void PrepareDurationDropdown(bool permOnly) {
		durationSelectionDropdown.options.Clear();
		if (permOnly) {
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("Permanent"));
		} else {
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("1 day"));
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("7 days"));
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("30 days"));
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("90 days"));
			durationSelectionDropdown.options.Add(new Dropdown.OptionData("Permanent"));
		}
	}

	public void OnConfirmPreparePurchaseClicked() {
		preparePurchasePopup.SetActive(false);
		confirmPurchaseTxt.text = "Are you sure you would like to buy " + itemBeingPurchased + " for " +
		durationSelectionDropdown.options[durationSelectionDropdown.value].text + "? (" + totalGpCostBeingPurchased + " GP)";
		confirmPurchasePopup.SetActive(true);
	}

	public void OnConfirmPurchaseClicked() {
		ConfirmPurchase();
	}

	public void OnDurationSelect() {
		int durationInput = durationSelectionDropdown.value;
        SetTotalGPCost(durationInput);
	}

    void SetTotalGPCost(int duration)
    {
        totalGpCostBeingPurchased = GetGPCostForItemAndType(itemBeingPurchased, typeBeingPurchased, duration);
        totalGpCostTxt.text = "" + totalGpCostBeingPurchased;
    }

	public void OnCancelPurchaseClicked() {
		itemBeingPurchased = null;
		typeBeingPurchased = null;
		confirmPurchasePopup.SetActive(false);
		ToggleActiveMarketplaceButtons(true);
	}

	public void OnCancelPreparePurchaseClicked() {
		itemBeingPurchased = null;
		typeBeingPurchased = null;
		preparePurchasePopup.SetActive(false);
		ToggleActiveMarketplaceButtons(true);
	}

	void ConfirmPurchase() {
		confirmPurchasePopup.SetActive(false);
        // Ensure that the user doesn't already have this item
        float hasDuplicateCheck = HasDuplicateItem(itemBeingPurchased, typeBeingPurchased);
        if (hasDuplicateCheck < 0f) {
			TriggerMarketplacePopup("You already own this item.");
			return;
		}
        bool isStacking = (hasDuplicateCheck >= 0f && !Mathf.Approximately(0f, hasDuplicateCheck));
        float totalNewDuration = ConvertDurationInput(durationSelectionDropdown.value);
        totalNewDuration = (Mathf.Approximately(totalNewDuration, -1f) ? totalNewDuration : totalNewDuration + hasDuplicateCheck);
        // Reach out to DB to verify player's GP and KASH before purchase
        DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).GetValueAsync().ContinueWith(task => {
			if (task.IsCompleted) {
				PlayerData.playerdata.info.gp = uint.Parse(task.Result.Child("gp").Value.ToString());
				// PlayerData.playerdata.info.kash = uint.Parse(task.Result.Child("kash").Value.ToString());
				if (PlayerData.playerdata.info.gp >= totalGpCostBeingPurchased) {
					PlayerData.playerdata.AddItemToInventory(itemBeingPurchased, typeBeingPurchased, totalNewDuration, true, isStacking, totalGpCostBeingPurchased, 0);
				} else {
					TriggerMarketplacePopup("You do not have enough GP to purchase this item.");
				}
			} else {
				TriggerMarketplacePopup("Transaction could not be completed at this time. Please try again later.");
			}
		});
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
		myGpTxt.text = ""+PlayerData.playerdata.info.gp;
		myKashTxt.text = ""+PlayerData.playerdata.info.kash;
	}

	// Players cannot own multiple of the same item unless they're mods.
	// Mods are always permanent.
	// Items can only be purchased again if they don't own it for permanent
	public float HasDuplicateItem(string itemName, string type) {
		if (type == "Mod") return 0f;
		if (type.Equals("Weapon")) {
            foreach (KeyValuePair<string, WeaponData> entry in PlayerData.playerdata.myWeapons)
            {
                WeaponData item = entry.Value;
				if (item.name.Equals(itemName)) {
					float duration = float.Parse(item.duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else if (type.Equals("Character")) {
            foreach (KeyValuePair<string, CharacterData> entry in PlayerData.playerdata.myCharacters)
            {
                CharacterData item = entry.Value;
				if (item.name.Equals(itemName)) {
					float duration = float.Parse(item.duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else if (type.Equals("Armor")) {
            foreach (KeyValuePair<string, ArmorData> entry in PlayerData.playerdata.myArmor)
            {
                ArmorData item = entry.Value;
				if (item.name.Equals(itemName)) {
					float duration = float.Parse(item.duration);
					if (Mathf.Approximately(duration, -1f) || (duration >= float.MaxValue - NINETY_DAYS_MINS)) {
						return -1f;
					} else {
						return duration;
					}
				}
			}
		} else {
			string itemCategory = InventoryScript.itemData.equipmentCatalog[itemName].category;
			Dictionary<string, EquipmentData> inventoryRefForCat = null;
			switch (itemCategory) {
				case "Top":
					inventoryRefForCat = PlayerData.playerdata.myTops;
					break;
				case "Bottom":
					inventoryRefForCat = PlayerData.playerdata.myBottoms;
					break;
				case "Footwear":
					inventoryRefForCat = PlayerData.playerdata.myFootwear;
					break;
				case "Headgear":
					inventoryRefForCat = PlayerData.playerdata.myHeadgear;
					break;
				case "Facewear":
					inventoryRefForCat = PlayerData.playerdata.myFacewear;
					break;
			}
            foreach (KeyValuePair<string, EquipmentData> entry in inventoryRefForCat)
            {
                EquipmentData item = entry.Value;
				if (item.name.Equals(itemName)) {
					float duration = float.Parse(item.duration);
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
	}

	void ToggleActiveMarketplaceButtons(bool b) {
		marketplaceBackBtn.interactable = b;
		clearPreviewBtn.interactable = b;
		weaponsBtn.interactable = b;
		shopHeadgearBtn.interactable = b;
		shopFaceBtn.interactable = b;
		shopArmorBtn.interactable = b;
		shopTopsBtn.interactable = b;
		shopBottomsBtn.interactable = b;
		shopFootwearBtn.interactable = b;
		shopCharacterBtn.interactable = b;
		shopPrimaryWepBtn.interactable = b;
		shopSecondaryWepBtn.interactable = b;
		shopSupportWepBtn.interactable = b;
		shopAssaultRifleSubBtn.interactable = b;
		shopShotgunSubBtn.interactable = b;
		shopSniperRifleSubBtn.interactable = b;
		shopPistolSubBtn.interactable = b;
		shopExplosivesSubBtn.interactable = b;
		shopBoostersSubBtn.interactable = b;
		shopModsBtn.interactable = b;
		shopSuppressorsSubBtn.interactable = b;
		Button[] shopItemsBtns = shopContentInventory.GetComponentsInChildren<Button>();
		foreach (Button btn in shopItemsBtns) {
			btn.interactable = b;
		}
	}

	void SetPlayerNameForTitle() {
		mainNametagTxt.text = PlayerData.playerdata.info.playername;
	}

	void SetPlayerRankForTitle() {
		Rank rank = PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.exp);
		mainRankTxt.text = rank.name;
		mainRankImg.texture = PlayerData.playerdata.GetRankInsigniaForRank(rank.name);
	}

	void SetPlayerLevelProgressForTitle() {
		uint myExp = PlayerData.playerdata.info.exp;
		Rank rank = PlayerData.playerdata.GetRankFromExp(myExp);
		uint currExp = myExp - rank.minExp;
		uint toExp = rank.maxExp - rank.minExp;
		float percentProgress = (float)currExp / (float)toExp;
		mainLevelProgress.value = percentProgress;
		mainLevelProgressPercentTxt.text = "" + (uint)(percentProgress * 100);
		mainExpTxt.text = currExp + " / " + toExp;
	}

	public void SetPlayerStatsForTitle() {
		SetPlayerNameForTitle();
		SetPlayerRankForTitle();
		SetPlayerLevelProgressForTitle();
	}
		
}
