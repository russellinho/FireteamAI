using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class TitleControllerScript : MonoBehaviourPunCallbacks {

	public GameObject mainMenu;
	//public GameObject networkMan;
	public GameObject matchmakingMenu;
	public GameObject customizationMenu;
	public GameObject loadingScreen;
	public GameObject jukebox;

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

	// Use this for initialization
	void Start () {
		PlayerData.playerdata.LoadPlayerData();
		PlayerNameInput.text = PlayerData.playerdata.playername;
		mainMenu.SetActive (true);
		loadingStatus = 0;
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
		}
	}

	public void GoToMatchmakingMenu() {
		if (!PhotonNetwork.IsConnected) {
			PhotonNetwork.LocalPlayer.NickName = PlayerData.playerdata.playername;
			PhotonNetwork.ConnectUsingSettings();
		}

		mainMenu.SetActive (false);
		customizationMenu.SetActive (false);
		matchmakingMenu.SetActive (true);
	}
		
	public void ReturnToMainMenu() {
		// Save settings if the settings are active
		if (customizationMenu.activeInHierarchy) {
			savePlayerData ();
		}
		customizationMenu.SetActive (false);
		mainMenu.SetActive (true);
		matchmakingMenu.SetActive (false);
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
        mainMenu.SetActive(true);
    }

    public void GoToCustomization() {
		mainMenu.SetActive (false);
		matchmakingMenu.SetActive (false);
		customizationMenu.SetActive (true);
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

	public void SetRedColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 0;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetBlueColor() {
		PlayerData.playerdata.color.x = 0;
		PlayerData.playerdata.color.y = 0;
		PlayerData.playerdata.color.z = 255;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetGreenColor() {
		PlayerData.playerdata.color.x = 0;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetYellowColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 0;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetOrangeColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 119;
		PlayerData.playerdata.color.z = 1;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetPurpleColor() {
		PlayerData.playerdata.color.x = 81;
		PlayerData.playerdata.color.y = 2;
		PlayerData.playerdata.color.z = 126;
		PlayerData.playerdata.UpdateBodyColor ();
	}

	public void SetWhiteColor() {
		PlayerData.playerdata.color.x = 255;
		PlayerData.playerdata.color.y = 255;
		PlayerData.playerdata.color.z = 255;
		PlayerData.playerdata.UpdateBodyColor ();
	}
		
}
