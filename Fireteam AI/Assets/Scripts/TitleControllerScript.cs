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

	public RawImage equippedHeadSlot;
	public RawImage equippedFaceSlot;
	public RawImage equippedTopSlot;
	public RawImage equippedBottomSlot;
	public RawImage equippedFootSlot;
	public RawImage equippedCharacterSlot;
	public RawImage equippedArmorSlot;
	public GameObject currentlyEquippedItemPrefab;

	// Weapon loadout stuff
	public Button loadoutBtn;
	public Button primaryWepBtn;
	public Button secondaryWepBtn;
	private float secondaryWepBtnYPos1 = -95f;
	private float secondaryWepBtnYPos2 = -185f;
	public Button assaultRifleSubBtn;
	public Button shotgunSubBtn;
	public Button sniperRifleSubBtn;
	public RawImage equippedPrimarySlot;
	public RawImage equippedSecondarySlot;

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
						customizationMenu.SetActive(true);
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
		// if (customizationMenu.activeInHierarchy) {
		// 	savePlayerData ();
		// }
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myHeadgear.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myHeadgear[i];
            o.GetComponent<ShopItemScript>().itemType = "Headgear";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myHeadgear[i]]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedHeadSlot.texture)) {
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myFacewear.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myFacewear[i];
            o.GetComponent<ShopItemScript>().itemType = "Facewear";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myFacewear[i]]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedFaceSlot.texture)) {
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myArmor.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myArmor[i];
            o.GetComponent<ShopItemScript>().itemType = "Armor";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myArmor[i]]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedArmorSlot.texture)) {
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myTops.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
            o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myTops[i];
            o.GetComponent<ShopItemScript>().itemType = "Top";
			o.GetComponent<ShopItemScript>().skinType = CheckSkinType((string)InventoryScript.myTops[i], currentCharGender);
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myTops[i] + " " + currentCharGender]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 4f, t.sizeDelta.y / 4f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedTopSlot.texture)) {
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myBottoms.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myBottoms[i];
            o.GetComponent<ShopItemScript>().itemType = "Bottom";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myBottoms[i] + " " + currentCharGender]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedBottomSlot.texture)) {
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

		// Populate into grid layout
		for (int i = 0; i < InventoryScript.myFootwear.Count; i++) {
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myFootwear[i];
            o.GetComponent<ShopItemScript>().itemType = "Footwear";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myFootwear[i]]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 3f, t.sizeDelta.y / 3f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedFootSlot.texture)) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				o.GetComponent<ShopItemScript>().equippedInd.enabled = true;
				currentlyEquippedItemPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
		}
	}

	public void OnPrimaryWepBtnClicked() {
		// Change all button colors
		primaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.gameObject.transform.position = new Vector3(primaryWepBtn.gameObject.transform.position.x, secondaryWepBtnYPos2, primaryWepBtn.gameObject.transform.position.z);

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

		// // Populate into grid layout
		// for (int i = 0; i < InventoryScript.myPrimaries.Count; i++) {
		// 	GameObject o = Instantiate(contentPrefab);
		// 	o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myPrimaries[i]]);
		// 	o.transform.SetParent(contentInventory.transform);
		// }
	}

	public void OnSecondaryWepBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Delete any currently existing items in the grid
		// RawImage[] existingThumbnails = contentInventory.GetComponentsInChildren<RawImage>();
		// foreach (RawImage r in existingThumbnails) {
		// 	Destroy(r.GetComponentInParent<ShopItemScript>().gameObject);
		// }

		// // Populate into grid layout
		// for (int i = 0; i < InventoryScript.mySecondaries.Count; i++) {
		// 	GameObject o = Instantiate(contentPrefab);
		// 	o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.mySecondaries[i]]);
		// 	o.transform.SetParent(contentInventory.transform);
		// }
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
			GameObject o = Instantiate(contentPrefab);
			o.GetComponent<ShopItemScript>().itemDescriptionPopupRef = itemDescriptionPopupRef;
			o.GetComponent<ShopItemScript>().itemName = (string)InventoryScript.myCharacters[i];
            o.GetComponent<ShopItemScript>().itemType = "Character";
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[(string)InventoryScript.myCharacters[i]]);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (o.GetComponentInChildren<RawImage>().texture.Equals(equippedCharacterSlot.texture)) {
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
			SwitchToEquipmentScreen();
		} else {
			t.text = "Loadout";
			SwitchToLoadoutScreen();
		}
	}

	void SwitchToLoadoutScreen() {
		headgearBtn.gameObject.SetActive(false);
		faceBtn.gameObject.SetActive(false);
		topsBtn.gameObject.SetActive(false);
		bottomsBtn.gameObject.SetActive(false);
		footwearBtn.gameObject.SetActive(false);
		characterBtn.gameObject.SetActive(false);
		armorBtn.gameObject.SetActive(false);

		equippedHeadSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedFaceSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedTopSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedBottomSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedFootSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedCharacterSlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedArmorSlot.GetComponentInParent<Image>().gameObject.SetActive(false);

		primaryWepBtn.gameObject.SetActive(true);
		secondaryWepBtn.gameObject.SetActive(true);
		secondaryWepBtn.gameObject.transform.position = new Vector3(secondaryWepBtn.gameObject.transform.position.x, secondaryWepBtnYPos1, secondaryWepBtn.gameObject.transform.position.z);
		equippedPrimarySlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedSecondarySlot.GetComponentInParent<Image>().gameObject.SetActive(true);

	}

	void SwitchToEquipmentScreen() {
		headgearBtn.gameObject.SetActive(true);
		faceBtn.gameObject.SetActive(true);
		topsBtn.gameObject.SetActive(true);
		bottomsBtn.gameObject.SetActive(true);
		footwearBtn.gameObject.SetActive(true);
		characterBtn.gameObject.SetActive(true);
		armorBtn.gameObject.SetActive(true);

		equippedHeadSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedFaceSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedTopSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedBottomSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedFootSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedCharacterSlot.GetComponentInParent<Image>().gameObject.SetActive(true);
		equippedArmorSlot.GetComponentInParent<Image>().gameObject.SetActive(true);

		primaryWepBtn.gameObject.SetActive(false);
		secondaryWepBtn.gameObject.SetActive(false);
		equippedPrimarySlot.GetComponentInParent<Image>().gameObject.SetActive(false);
		equippedSecondarySlot.GetComponentInParent<Image>().gameObject.SetActive(false);
	}

	public static int CheckSkinType(string clothingName, char gender) {
		if (gender == 'F') {
			if (clothingName.Equals("Casual T-Shirt")) {
				return 2;
			} else if (clothingName.Equals("Casual Tank Top")) {
				return 1;
			}
			return 0;
		} else {
			if (clothingName.Equals("Casual T-Shirt")) {
				return 2;
			} else if (clothingName.Equals("Casual Shirt")) {
				return 1;
			}
			return 0;
		}
	}
		
}
