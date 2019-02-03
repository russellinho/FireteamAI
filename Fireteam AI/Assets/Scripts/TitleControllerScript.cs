using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Networking;

public class TitleControllerScript : MonoBehaviourPunCallbacks {

	private Vector3 customizationCameraPos = new Vector3(-4.82f, 4.08f, 21.5f);
	private Vector3 defaultCameraPos = new Vector3(-7.3f, 4.08f, 22.91f);
	private int camPos;
	private float camMoveTimer;

	public GameObject mainMenu;
	public Camera mainCam;
	public Text titleText;
	//public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;
	public GameObject loadingScreen;
	public GameObject jukebox;
	public GameObject mainMenuPopup;

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
	public Button primaryWepBtn;
	public Button secondaryWepBtn;
	public Button characterBtn;

	// Use this for initialization
	void Start () {
		//PlayerData.playerdata.LoadPlayerData ();
		PlayerData.playerdata.FindBodyRef ();
//		PlayerData.playerdata.UpdateBodyColor ();
		//PlayerNameInput.text = PlayerData.playerdata.playername;
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

		// Load the headgear items in inventory
		//ArrayList items = InventoryScript.

		// Populate into grid layout
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

		// Load the headgear items in inventory

		// Populate into grid layout
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

		// Load the headgear items in inventory

		// Populate into grid layout
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

		// Load the headgear items in inventory

		// Populate into grid layout
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

		// Load the headgear items in inventory

		// Populate into grid layout
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

		// Load the headgear items in inventory

		// Populate into grid layout
	}

	public void OnPrimaryWepBtnClicked() {
		// Change all button colors
		headgearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		faceBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		armorBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		topsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		bottomsBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		footwearBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		primaryWepBtn.GetComponent<Image>().color = new Color(188f / 255f, 136f / 255f, 45f / 255f, 214f / 255f);
		secondaryWepBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);
		characterBtn.GetComponent<Image>().color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 214f / 255f);

		// Load the headgear items in inventory

		// Populate into grid layout
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

		// Load the headgear items in inventory


		// Populate into grid layout
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

		// Load the headgear items in inventory
		

		// Populate into grid layout
	}
		
}
