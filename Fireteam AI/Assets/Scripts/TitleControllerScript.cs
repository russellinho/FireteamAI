using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;

public class TitleControllerScript : MonoBehaviourPunCallbacks {

	private Vector3 customizationCameraPos = new Vector3(-4.7f, 4.08f, 21.5f);
	private Vector3 defaultCameraPos = new Vector3(-7.3f, 4.08f, 22.91f);
	private int camPos;
	private float camMoveTimer;
	public GameObject itemDescriptionPopupRef;

	public GameObject mainMenu;
	public Camera mainCam;
	public Text titleText;
	//public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;
	public GameObject loadingScreen;
	public GameObject jukebox;
	public GameObject mainMenuPopup;
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
	private float secondaryWepBtnYPos1 = -95f;
	private float secondaryWepBtnYPos2 = -185f;
	public Button assaultRifleSubBtn;
	public Button shotgunSubBtn;
	public Button sniperRifleSubBtn;
	public Button pistolSubBtn;
	public GameObject equippedPrimarySlot;
	public GameObject equippedSecondarySlot;

	// Use this for initialization
	void Start () {
		//PlayerData.playerdata.FindBodyRef ();
		titleText.enabled = true;
		mainMenu.SetActive (true);
		loadingStatus = 0;
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
			if (!www.downloadHandler.text.Substring(0, www.downloadHandler.text.Length - 2).Equals(Application.version)) {
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
        else
        {
            screenArt.texture = (Texture)Resources.Load("MapImages/Loading/test_load");
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
					if (Vector3.Equals(mainCam.transform.position, defaultCameraPos)) {
						titleText.enabled = true;
						mainMenu.SetActive(true);
					}
				} else if (camPos == 1) {
					mainCam.transform.position = Vector3.Lerp(defaultCameraPos, customizationCameraPos, camMoveTimer);
					if (camMoveTimer < 1f) {
						camMoveTimer += (Time.deltaTime / 1.2f);
					}
					if (Vector3.Equals(mainCam.transform.position, customizationCameraPos)) {
						if (!customizationMenu.activeInHierarchy) {
							customizationMenu.SetActive(true);
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
		titleText.enabled = true;
		mainMenu.SetActive(true);
	}

	public void ReturnToMainMenuFromCustomization() {
        // Save settings if the settings are active
		 if (customizationMenu.activeInHierarchy) {
		 	savePlayerData ();
		 }
		SwitchToEquipmentScreen();
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
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
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
		matchmakingMenu.SetActive (false);
		camPos = 1;
		camMoveTimer = 0f;
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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		// Moving secondary button down for submenu
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos2);

		// Change all button colors
		primaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Remove sub buttons
		pistolSubBtn.gameObject.SetActive(false);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		assaultRifleSubBtn.gameObject.SetActive(true);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(true);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(true);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1);

		// Remove sub buttons
		assaultRifleSubBtn.gameObject.SetActive(false);
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		shotgunSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Add sub buttons
		pistolSubBtn.gameObject.SetActive(true);
		pistolSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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

	public void OnAssaultRifleSubBtnClicked() {
		// Make tab orange and clear other tabs
		assaultRifleSubBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		shotgunSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		sniperRifleSubBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Clear items
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

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
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		foreach (RawImage r in existingThumbnails) {
			currentlyEquippedItemPrefab = null;
			Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		}

		// If you're on equipment screen, go to loadout screen. Else, go back to loadout.
		if (t.text.Equals("Loadout")) {
			t.text = "Equipment";
			SwitchToLoadoutScreen();
		} else {
			t.text = "Loadout";
			SwitchToEquipmentScreen();
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
		RectTransform rt = secondaryWepBtn.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, secondaryWepBtnYPos1);
		equippedPrimarySlot.SetActive(true);
		equippedSecondarySlot.SetActive(true);

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
		assaultRifleSubBtn.gameObject.SetActive(false);
		shotgunSubBtn.gameObject.SetActive(false);
		sniperRifleSubBtn.gameObject.SetActive(false);
		pistolSubBtn.gameObject.SetActive(false);
		equippedPrimarySlot.SetActive(false);
		equippedSecondarySlot.SetActive(false);
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
		
}
