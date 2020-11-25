using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PauseMenuScript : MonoBehaviourPunCallbacks {
	public GameControllerScript gameController;
	public GameObject playerRef;
	public GameObject keyMappingsPanel;
	public GameObject mainMenuGroup;
	public GameObject optionsMenuGroup;
	public GameObject audioSettingsGroup;
	public Slider musicVolumeSlider;
	public TextMeshProUGUI musicVolumeField;
	public Button optionsKeyBindingsBtn;
	public Button optionsAudioSettingsBtn;
	public Button optionsBackBtn;
	public Text optionsTitle;
	public bool isChangingKeyMapping;
	public Text changingKeyMappingText;
	public KeyMappingInput[] keyMappingInputs;

	void Awake() {
		musicVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f;
		musicVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.musicVolume;
	}

	public void HandleEscPress() {
		if (isChangingKeyMapping) return;
		// If the key binding menu is up and you press escape, then return to options menu
		if (audioSettingsGroup.activeInHierarchy) {
			CloseAudioSettings();
		} else if (keyMappingsPanel.activeInHierarchy) {
			CloseKeyMappings();
		} else if (optionsMenuGroup.activeInHierarchy) {
			// If the options menu group is active, then go back to main menu
			CloseOptionsMenu();
		}
	}

	public void LeaveGame() {
		// If master client quits, end the game for all and reset deads, redScore, blueScore, redStatus, blueStatus, inGame
		if (PhotonNetwork.LocalPlayer.IsMasterClient) {
			Hashtable h = new Hashtable();
			h.Add("deads", null);
			h.Add("inGame", 0);
			if (gameController.matchType == 'V') {
				h.Add("redScore", 0);
				h.Add("blueScore", 0);
				h.Add("redStatus", null);
				h.Add("blueStatus", null);
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			gameController.EndGameForAll();
		} else {
			// Else
			// Add myself to dead list if I'm dead
			if (PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().health <= 0) {
				string currentDeads = (string)PhotonNetwork.CurrentRoom.CustomProperties["deads"];
				Hashtable h = new Hashtable();
				if (currentDeads == null) {
					currentDeads = PhotonNetwork.LocalPlayer.NickName + ",";
				} else {
					// Check if you're already on that list
					string[] currDeadsList = currentDeads.Split(',');
					if (!currDeadsList.Contains(PhotonNetwork.LocalPlayer.NickName)) {
						currentDeads += PhotonNetwork.LocalPlayer.NickName + ",";
					}
				}
				h.Add("deads", currentDeads);
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
			if (gameController.vipRef != null) {
				gameController.vipRef.GetComponent<NpcScript>().OnPlayerLeftGame(PhotonNetwork.LocalPlayer.ActorNumber);
				gameController.OnPlayerLeftGame(PhotonNetwork.LocalPlayer.ActorNumber);
			}
			PlayerData.playerdata.DestroyMyself();
		}
	}

	// public override void OnLeftRoom() {
	// 	PhotonNetwork.Disconnect ();
	// }

	public void OpenKeyMappings() {
		ToggleSettingsMainButtons(false);
		keyMappingsPanel.SetActive(true);
	}

	public void CloseKeyMappings() {
		if (isChangingKeyMapping) return;
		SaveKeyBindings();
		keyMappingsPanel.SetActive(false);
		ToggleSettingsMainButtons(true);
	}

	public void OpenOptionsMenu() {
		mainMenuGroup.SetActive(false);
		optionsMenuGroup.SetActive(true);
	}

	public void CloseOptionsMenu() {
		mainMenuGroup.SetActive(true);
		optionsMenuGroup.SetActive(false);
	}

	public void OpenAudioSettings() {
		ToggleSettingsMainButtons(false);
		audioSettingsGroup.SetActive(true);
	}

	public void CloseAudioSettings() {
		SaveAudioSettings();
		audioSettingsGroup.SetActive(false);
		optionsMenuGroup.SetActive(true);
		ToggleSettingsMainButtons(true);
	}

	public void OnMusicVolumeSliderChanged() {
		SetMusicVolume(musicVolumeSlider.value);
	}

	void SetMusicVolume(float v) {
		musicVolumeField.text = ""+(int)(v * 100f);
		JukeboxScript.jukebox.SetMusicVolume(v);
	}

	public void SaveAudioSettings() {
		PlayerPreferences.playerPreferences.preferenceData.musicVolume = (int)(musicVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.SavePreferences();
	}

	public void SaveKeyBindings() {
		PlayerPreferences.playerPreferences.SaveKeyMappings();
	}

	void ToggleSettingsMainButtons(bool b) {
		optionsKeyBindingsBtn.gameObject.SetActive(b);
		optionsAudioSettingsBtn.gameObject.SetActive(b);
		optionsBackBtn.gameObject.SetActive(b);
		optionsTitle.gameObject.SetActive(b);
	}

	public void ToggleIsChangingKeyMapping(bool b, string keyChanging) {
		if (b) {
			isChangingKeyMapping = true;
			changingKeyMappingText.text = "Press a button to set a key for [" + keyChanging + "]";
			changingKeyMappingText.enabled = true;
		} else {
			isChangingKeyMapping = false;
			changingKeyMappingText.enabled = false;
		}
	}

	public void SetPlayerRef(GameObject player) {
		playerRef = player;
	}

}
