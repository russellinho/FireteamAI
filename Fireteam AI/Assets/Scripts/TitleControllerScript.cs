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

	public GameObject mainMenu;
	public Camera mainCam;
	public Text titleText;
	//public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;
	public GameObject modMenu;
	public GameObject keyBindingsPopup;
	public GameObject loadingScreen;
	public GameObject jukebox;
	public GameObject mainMenuPopup;
	public GameObject customizationMenuPopup;
	public GameObject modMenuPopup;
	public char currentCharGender;

	public InputField PlayerNameInput;

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

	// Weapon loadout stuff
	public Button loadoutBtn;
	public Button primaryWepBtn;
	public Button secondaryWepBtn;
	public Button supportWepBtn;
	private float secondaryWepBtnYPos1 = -95f;
	private float secondaryWepBtnYPos2 = -185f;
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
	public Text modDamageTxt;
	public Text modAccuracyTxt;
	public Text modRecoilTxt;
	public Text modRangeTxt;
	public Text modClipCapacityTxt;
	public Text modMaxAmmoTxt;

	// Use this for initialization
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
		foreach (GameObject entry in GameControllerScript.playerList.Values)
		{
			Destroy(entry.gameObject);
		}
		GameControllerScript.playerList.Clear();
		GameControllerScript.totalKills.Clear ();
		GameControllerScript.totalDeaths.Clear ();
		PlayerNameInput.text = PhotonNetwork.NickName;

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
		if (mapName.Equals ("Citadel")) {
			screenArt.texture = (Texture)Resources.Load ("MapImages/Loading/citadel_load");
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
		if (PlayerData.playerdata.disconnectedFromServer) {
			PlayerData.playerdata.disconnectedFromServer = false;
			mainMenuPopup.GetComponentInChildren<Text> ().text = "Lost connection to server.\nReason: " + PlayerData.playerdata.disconnectReason;
			PlayerData.playerdata.disconnectReason = "";
			mainMenuPopup.SetActive (true);
		} else if (versionWarning) {
			versionWarning = false;
			mainMenuPopup.GetComponentInChildren<Text> ().text = "Your game is not updated to the latest version of Fireteam AI!\nThis may affect your matchmaking experience.";
			mainMenuPopup.SetActive (true);
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
			if (!matchmakingMenu.activeInHierarchy) {
				if (camPos == 0) {
					mainCam.transform.position = Vector3.Lerp(customizationCameraPos, defaultCameraPos, camMoveTimer);
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, defaultCameraPos) && !keyBindingsPopup.activeInHierarchy) {
						titleText.enabled = true;
						mainMenu.SetActive(true);
					}
				} else if (camPos == 1) {
					if (previousCamPos == 0) {
						mainCam.transform.position = Vector3.Lerp(defaultCameraPos, customizationCameraPos, camMoveTimer);
					} else if (previousCamPos == 2) {
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
				} else if (camPos == 2) {
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
		matchmakingMenu.SetActive (true);
	}
		
	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
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
		 	savePlayerData ();
			 ClearCustomizationContent();
		 }
		SwitchToEquipmentScreen();
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
		previousCamPos = camPos;
		camPos = 0;
		camMoveTimer = 0f;
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
		previousCamPos = camPos;
		camPos = 1;
		camMoveTimer = 0f;
	}

	public void GoToMod() {
		mainMenu.SetActive(false);
		customizationMenu.SetActive(false);
		previousCamPos = camPos;
		camPos = 2;
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
		PlayerData.playerdata.playername = PlayerNameInput.text;
		PlayerData.playerdata.SavePlayerData();
	}

	public void ClosePopup() {
		mainMenuPopup.SetActive (false);
		modMenuPopup.SetActive(false);
		customizationMenuPopup.SetActive(false);
	}

	// Clears existing items from the shop panel
	void ClearCustomizationContent() {
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Equipment> characterEquipment = InventoryScript.characterCatalog[characterName].equipmentCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myHeadgear.Count; i++) {
			string thisItemName = (string)InventoryScript.myHeadgear[i];
			Equipment thisHeadgear = characterEquipment[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().equipmentDetails = thisHeadgear;
			o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Headgear";
			o.GetComponent<ShopItemScript>().itemDescription = thisHeadgear.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].equipmentCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedHeadSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Equipment> characterEquipment = InventoryScript.characterCatalog[characterName].equipmentCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myFacewear.Count; i++) {
			string thisItemName = (string)InventoryScript.myFacewear[i];
			Equipment thisFacewear = characterEquipment[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().equipmentDetails = thisFacewear;
			o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Facewear";
			o.GetComponent<ShopItemScript>().itemDescription = thisFacewear.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].equipmentCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedFaceSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Armor> characterArmor = InventoryScript.characterCatalog[characterName].armorCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myArmor.Count; i++) {
			string thisItemName = (string)InventoryScript.myArmor[i];
			Armor thisArmor = characterArmor[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().armorDetails = thisArmor;
			o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Armor";
			o.GetComponent<ShopItemScript>().itemDescription = thisArmor.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].armorCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedArmorSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Equipment> characterEquipment = InventoryScript.characterCatalog[characterName].equipmentCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myTops.Count; i++) {
			string thisItemName = (string)InventoryScript.myTops[i];
			Equipment thisTop = characterEquipment[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().equipmentDetails = thisTop;
            o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Top";
			o.GetComponent<ShopItemScript>().itemDescription = thisTop.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].equipmentCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 4f, t.sizeDelta.y / 4f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedTopSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Equipment> characterEquipment = InventoryScript.characterCatalog[characterName].equipmentCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myBottoms.Count; i++) {
			string thisItemName = (string)InventoryScript.myBottoms[i];
			Equipment thisBottom = characterEquipment[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().equipmentDetails = thisBottom;
			o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Bottom";
			o.GetComponent<ShopItemScript>().itemDescription = thisBottom.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].equipmentCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedBottomSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

		string characterName = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().equippedCharacter;
		Dictionary<string, Equipment> characterEquipment = InventoryScript.characterCatalog[characterName].equipmentCatalog;
		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myFootwear.Count; i++) {
			string thisItemName = (string)InventoryScript.myFootwear[i];
			Equipment thisFootwear = characterEquipment[thisItemName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().equipmentDetails = thisFootwear;
			o.GetComponent<ShopItemScript>().itemName = thisItemName;
            o.GetComponent<ShopItemScript>().itemType = "Footwear";
			o.GetComponent<ShopItemScript>().itemDescription = thisFootwear.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.characterCatalog[characterName].equipmentCatalog[thisItemName].thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedFootSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnPrimaryWepBtnClicked() {
		// Moving secondary and support button down for submenu
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos2);
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos2 - 30f);

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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.type.Equals("Primary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedPrimarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnSecondaryWepBtnClicked() {
		// Change all button colors
		secondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		supportWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1);

		// Move support wep button down for submenus
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1 - 60f);

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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.type.Equals("Secondary")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedSecondarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnSupportWepBtnClicked() {
		// Change all button colors
		supportWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1);
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1 - 30f);

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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.type.Equals("Support")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedSupportSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Assault Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedPrimarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Shotgun")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedPrimarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Sniper Rifle")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6.5f, t.sizeDelta.y / 6.5f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedPrimarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnPistolSubBtnClicked() {
		// Make tab orange and clear other tabs
		pistolSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		ClearCustomizationContent();

		// Populate with pistols
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Pistol")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedSecondarySlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Explosive")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedSupportSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			Weapon w = InventoryScript.weaponCatalog[(string)InventoryScript.myWeapons[i]];
			if (!w.category.Equals("Booster")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().weaponDetails = w;
			o.GetComponent<ShopItemScript>().itemName = w.name;
            o.GetComponent<ShopItemScript>().itemType = "Weapon";
			o.GetComponent<ShopItemScript>().itemDescription = w.description;
			o.GetComponent<ShopItemScript>().weaponCategory = w.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedSupportSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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
		for (int i = 0; i < InventoryScript.myCharacters.Count; i++) {
			string thisCharacterName = (string)InventoryScript.myCharacters[i];
			Character c = InventoryScript.characterCatalog[thisCharacterName];
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().characterDetails = c;
			o.GetComponent<ShopItemScript>().itemName = thisCharacterName;
            o.GetComponent<ShopItemScript>().itemType = "Character";
			o.GetComponent<ShopItemScript>().itemDescription = c.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(c.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedCharacterSlot.GetComponentInChildren<RawImage>().texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
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

	public void OnSuppressorsBtnClicked() {
		// Change all button colors
		suppressorsBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		ClearModCustomizationContent();

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myMods.Count; i++) {
			Mod m = InventoryScript.modCatalog[(string)InventoryScript.myMods[i]];
			if (!m.category.Equals("Suppressor")) {
				continue;
			}
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().modDescriptionPopupRef = modDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().modDetails = m;
			o.GetComponent<ShopItemScript>().itemName = m.name;
            o.GetComponent<ShopItemScript>().itemType = "Mod";
			o.GetComponent<ShopItemScript>().itemDescription = m.description;
			o.GetComponent<ShopItemScript>().modCategory = m.category;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(m.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 6f, t.sizeDelta.y / 6f);
			if (equippedSuppressorTxt.text.Equals(m.name)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
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
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1);
		rt = supportWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1 - 30f);
		equippedPrimarySlot.SetActive(true);
		equippedSecondarySlot.SetActive(true);
		equippedSupportSlot.SetActive(true);

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
		for (int i = 0; i < InventoryScript.myWeapons.Count; i++) {
			string weaponName = (string)InventoryScript.myWeapons[i];
			Weapon w = InventoryScript.weaponCatalog[weaponName];
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
		SetWeaponModValues(modWeaponLbl.text, null);

		// Place the saved mods for that weapon back on the weapon template
		ModInfo savedModInfo = PlayerData.playerdata.LoadModDataForWeapon(weaponName);
		EquipModOnWeaponTemplate(savedModInfo.equippedSuppressor, "Suppressor");

		// Update shop items with the mods that are equipped
		// If the suppressors menu was selected, update the shop items with what's equipped on the current weapon
		if (suppressorsBtn.GetComponent<Image>().color.r == (188f / 255f)) {
			OnSuppressorsBtnClicked();
		}
	}

	public void SetWeaponModValues(string weaponName, string suppressorName) {
		modWeaponLbl.text = weaponName;
		equippedSuppressorTxt.text = (suppressorName == null || suppressorName.Equals("") || suppressorName.Equals("None")) ? "None" : suppressorName;
		
		// Set base stats
		Weapon w = InventoryScript.weaponCatalog[weaponName];
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
		if (suppressorName != null && !suppressorName.Equals("") && !suppressorName.Equals("None")) {
			Mod suppressor = InventoryScript.modCatalog[suppressorName];
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
				if (equippedSuppressorTxt.Equals("None")) return;
				SetWeaponModValues(modWeaponLbl.text, null);
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

	public void OnRemoveSuppressorClicked() {
		// Remove suppressor model from the player's weapon and the template weapon
		PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().UnequipMod("Suppressor", modWeaponLbl.text);
		UnequipModFromWeaponTemplate("Suppressor");
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
			PlayerData.playerdata.SaveModDataForWeapon(modWeaponLbl.text, equippedSuppressorTxt.text);
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

	public string EquipModOnWeaponTemplate(string modName, string modType) {
		if (modName == null || modName.Equals("") || modName.Equals("None")) return modWeaponLbl.text;
		Weapon w = InventoryScript.weaponCatalog[modWeaponLbl.text];
		switch(modType) {
			case "Suppressor":
				if (w.suppressorCompatible) {
					SetWeaponModValues(modWeaponLbl.text, modName);
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
		
}
