﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviourPunCallbacks {

	public GameObject keyMappingsPanel;
	public GameObject mainMenuGroup;
	public GameObject optionsMenuGroup;
	public GameObject audioSettingsGroup;
	public Slider musicVolumeSlider;
	public InputField musicVolumeField;
	public Button optionsKeyBindingsBtn;
	public Button optionsAudioSettingsBtn;
	public Button optionsBackBtn;
	public Text optionsTitle;
	private bool isChangingKeyMapping;
	public Text changingKeyMappingText;

	void Awake() {
		musicVolumeSlider.value = (float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f;
		musicVolumeField.text = ""+PlayerPreferences.playerPreferences.preferenceData.musicVolume;
	}

	public void HandleEscPress() {
		// If the key binding menu is up and you press escape, then return to options menu
		if (audioSettingsGroup.activeInHierarchy) {
			CloseAudioSettings();
		} else if (keyMappingsPanel.activeInHierarchy) {
			CloseKeyMappings();
		} else if (optionsMenuGroup.activeInHierarchy) {
			// If the options menu group is active, then go back to main menu
			CloseOptionsMenu();
		} else if (mainMenuGroup.activeInHierarchy) {
			// If back on the main menu, then resume game
			ResumeGame();
		}
	}

	public void ResumeGame() {
		gameObject.SetActive (false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void LeaveGame() {
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom() {
		PhotonNetwork.Disconnect ();
	}

	public void OpenKeyMappings() {
		ToggleSettingsMainButtons(false);
		keyMappingsPanel.SetActive(true);
	}

	public void CloseKeyMappings() {
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

	void SaveAudioSettings() {
		PlayerPreferences.playerPreferences.preferenceData.musicVolume = (int)(musicVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.SavePreferences();
	}

	void SaveKeyBindings() {
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

}
