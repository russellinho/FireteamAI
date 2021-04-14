using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Shift;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PauseMenuScript : MonoBehaviourPunCallbacks {
	public GameControllerScript gameController;
	public PauseMenuManager pauseMenuManager;
	public BlurManager blurManager;
	public ModalWindowManager alertPopup;
	public ModalWindowManager confirmPopup;
	public ModalWindowManager exitPopup;
	public GameObject playerRef;
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
	public HorizontalSelector joinModeSelector;
	// public HorizontalSelector audioInputSelector;
	public bool isChangingKeyMapping;
	public Text changingKeyMappingText;
	public KeyMappingInput[] keyMappingInputs;
	public PlayerKick[] playerKickEntries;
	private enum ConfirmType {KickPlayer, ResetKeyBindings};
	private ConfirmType confirmType;
	private Player playerSelected;
	public GameObject gameOptionsMenu;
	public Animator mainAnimator;
	public Animator settingsAnimator;
	public Animator gameOptionsAnimator;
	public Animator kickPlayerAnimator;
	public Button xButton;
	public string currentPanel;
	private AudioSourceModifier[] sceneAudioSources;

	void Awake() {
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

		joinModeSelector.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
		joinModeSelector.UpdateUI();

		GetSceneAudioSources();
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
			IEnumerator<DictionaryEntry> j = PhotonNetwork.CurrentRoom.CustomProperties.GetEnumerator();
			while (j.MoveNext()) {
				string thisKey = j.Current.Key.ToString();
				if (thisKey.EndsWith("GA")) {
					Debug.LogError("Resetting " + thisKey);
					h.Add(thisKey, null);
				}
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			gameController.EndGameForAll();
		} else {
			// Else
			// Add myself to dead list if I'm dead
			if (!PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().waitingOnAccept && PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().health <= 0) {
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
			PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().OnPlayerLeftMatch();
			if (gameController.vipRef != null) {
				gameController.vipRef.GetComponent<NpcScript>().OnPlayerLeftGame(PhotonNetwork.LocalPlayer.ActorNumber);
			}
			PlayerData.playerdata.DestroyMyself();
		}
	}

	void GetSceneAudioSources()
	{
		sceneAudioSources = (AudioSourceModifier[]) GameObject.FindObjectsOfType (typeof(AudioSourceModifier));
	}

	void UpdateAllAudioSources()
	{
		foreach (AudioSourceModifier a in sceneAudioSources)
		{
			a.SetVolume();
		}
	}

	// public override void OnLeftRoom() {
	// 	PhotonNetwork.Disconnect ();
	// }

	public void OnMusicVolumeSliderChanged() {
		SetMusicVolume(musicVolumeSlider.value);
	}

	public void OnGameVolumeSliderChanged() {
		SetGameVolume(gameVolumeSlider.value);
	}

	public void OnAmbientVolumeSliderChanged() {
		SetAmbientVolume(ambientVolumeSlider.value);
	}

	public void OnVoiceInputSliderChanged() {
		SetVoiceInputVolume(voiceInputVolumeSlider.value);
	}

	public void OnVoiceOutputSliderChanged() {
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
		VivoxVoiceManager.Instance.AudioInputDevices.VolumeAdjustment = ((int)(v * 100f) - 50);
	}

	void SetVoiceOutputVolume(float v) {
		voiceOutputVolumeField.text = ""+(int)(v * 100f);
		VivoxVoiceManager.Instance.AudioOutputDevices.VolumeAdjustment = ((int)(v * 100f) - 50);
	}

	public void SaveAudioSettings() {
		GetSceneAudioSources();
		PlayerPreferences.playerPreferences.preferenceData.musicVolume = (int)(musicVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.gameVolume = (int)(gameVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.ambientVolume = (int)(ambientVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.voiceInputVolume = (int)(voiceInputVolumeSlider.value * 100f);
		PlayerPreferences.playerPreferences.preferenceData.voiceOutputVolume = (int)(voiceOutputVolumeSlider.value * 100f);
		UpdateAllAudioSources();
		PlayerPreferences.playerPreferences.SavePreferences();
	}

	public void SaveKeyBindings() {
		PlayerPreferences.playerPreferences.SaveKeyMappings();
		PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().UpdateKeyHints();
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

	public void KickPlayer(Player p) {
		if (p == null) return;
		string canCallVoteResult = gameController.CanCallVote();
		if (canCallVoteResult == null) {
			confirmType = ConfirmType.KickPlayer;
			playerSelected = p;
			confirmPopup.SetText("ARE YOU SURE YOU WISH TO KICK PLAYER [" + p.NickName + "]?");
			blurManager.BlurInAnim();
			confirmPopup.ModalWindowIn();
		} else {
			alertPopup.SetText(canCallVoteResult);
			blurManager.BlurInAnim();
			alertPopup.ModalWindowIn();
		}
	}

	public void OnConfirmPopup()
	{
		if (confirmType == ConfirmType.KickPlayer) {
			gameController.StartVote(playerSelected, GameControllerScript.VoteActions.KickPlayer);
			gameOptionsMenu.GetComponent<Animator>().Play("Window Out");
			ResetPauseMenu();
			pauseMenuManager.ClosePause(true);
		} else if (confirmType == ConfirmType.ResetKeyBindings) {
			ResetKeyBindings();
		}
	}

	public void KickPlayerPrepareList()
	{
		string myTeam = null;
		if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus") {
			myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
		}
		int i = 0; // Player kick slot iterator
		foreach (Player p in PhotonNetwork.PlayerList) {
			// Cannot kick yourself or master client
			if (p.IsMasterClient || p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
				continue;
			}
			if (myTeam != null) {
				string theirTeam = (string)p.CustomProperties["team"];
				if (myTeam != theirTeam) continue;
			}
			playerKickEntries[i].Initialize(p);
			playerKickEntries[i].gameObject.SetActive(true);
			i++;
		}
		if (i <= 7) {
			for (int j = i; j < 8; j++) {
				playerKickEntries[j].gameObject.SetActive(false);
			}
		}
	}

	public void SetXButtonActions()
	{
		if (currentPanel == "Game Options") {
			gameOptionsAnimator.Play("Panel Out");
			mainAnimator.Play("Panel In");
			xButton.gameObject.SetActive(false);
			gameOptionsAnimator.gameObject.SetActive(false);
			SaveJoinMode();
			SetCurrentPanel("Main");
		} else if (currentPanel == "Settings") {
			SaveAudioSettings();
			SaveKeyBindings();
			xButton.gameObject.SetActive(false);
			settingsAnimator.Play("Panel Out");
			mainAnimator.Play("Panel In");
			SetCurrentPanel("Main");
		} else if (currentPanel == "Kick") {
			kickPlayerAnimator.Play("Panel Out");
			gameOptionsAnimator.Play("Panel In");
			kickPlayerAnimator.gameObject.SetActive(false);
			gameOptionsAnimator.gameObject.SetActive(true);
			SetCurrentPanel("Game Options");
		}
	}

	public void SetCurrentPanel(string panel) {
		this.currentPanel = panel;
	}

	public void HandleEscape()
	{
		if (alertPopup.isOn || confirmPopup.isOn || exitPopup.isOn) return;
		if (currentPanel == "Main") {
			pauseMenuManager.ClosePause();
		} else {
			if (currentPanel != "Settings") {
				SetXButtonActions();
			}
		}
	}

	void ResetPauseMenu()
	{
		if (currentPanel == "Settings") {
			settingsAnimator.Play("Panel Out");
		}
		gameOptionsAnimator.gameObject.SetActive(false);
		kickPlayerAnimator.gameObject.SetActive(false);
		xButton.gameObject.SetActive(false);
		mainAnimator.Play("Panel In");
		SaveJoinMode();
		SetCurrentPanel("Main");
	}

	void SaveJoinMode()
	{
		// Save join mode
		Hashtable h = new Hashtable();
		h.Add("joinMode", joinModeSelector.index);
		PhotonNetwork.CurrentRoom.SetCustomProperties(h);

		if (joinModeSelector.index == 0) {
			gameController.AcceptAllPlayers();
		}
	}

	public void OnResetKeyBindingsClicked()
	{
		confirmType = ConfirmType.ResetKeyBindings;
		confirmPopup.SetText("ARE YOU SURE YOU WISH TO RESET YOUR KEY BINDINGS TO DEFAULT?");
		blurManager.BlurInAnim();
		confirmPopup.ModalWindowIn();
	}

	public void ResetKeyBindings()
	{
		PlayerPreferences.playerPreferences.ResetKeyMappings();
		foreach (KeyMappingInput k in keyMappingInputs) {
			k.ResetKeyDisplay();
		}
	}

}
