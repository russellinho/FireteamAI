using System;
using System.Text.RegularExpressions;
using System.Linq;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UITemplate;
using TMPro;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Michsky.UI.Shift;
using VivoxUnity;
using VivoxUnity.Common;
using VivoxUnity.Private;
using Random = UnityEngine.Random;

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListPlayer : MonoBehaviourPunCallbacks, IInRoomCallbacks
	{
		private const short LOADING_TIMEOUT_CYCLES = 45; // 1.5 minute timeout = 45 loading screen thread cycles
		public MainPanelManager mainPanelManager;
		public PhotonView pView;

		[Header("Inside Room Panel")]
		public Transform PlayersInRoomPanel;
		public Transform PlayersInRoomPanelVsRed;
		public Transform PlayersInRoomPanelVsBlue;

		public Template templateUIClass;
		public Template templateUIClassVs;
        public Connexion connexion;
		public GameObject PlayerListEntryPrefab;
		public Dictionary<int, GameObject> playerListEntries;
		public TChat chat;
		public TChat chatVs;
		public GameObject readyButton;
		public GameObject readyButtonVs;
        public TextMeshProUGUI readyButtonTxt;
        public TextMeshProUGUI readyButtonVsTxt;
		public Button voiceChatBtn;
		public Button voiceChatBtnVs;
		public RawImage mapPreviewThumb;
		public RawImage mapPreviewVsThumb;
		public TextMeshProUGUI mapDescription;
		public TextMeshProUGUI mapDescriptionVs;
		public HorizontalSelector mapSelector;
		public HorizontalSelector mapSelectorVs;
		public HorizontalSelector stealthTrackSelector;
		public HorizontalSelector assaultTrackSelector;
		public HorizontalSelector stealthTrackSelectorVs;
		public HorizontalSelector assaultTrackSelectorVs;
		public HorizontalSelector joinModeSelector;
		public HorizontalSelector joinModeSelectorVs;
		public HorizontalSelector privacySelector;
		public HorizontalSelector privacySelectorVs;
		public TextMeshProUGUI passwordDisplayText;
		public TextMeshProUGUI passwordDisplayTextVs;
		public Button sendMsgBtn;
		public Button sendMsgBtnVs;
		// public Button emojiBtn;
		// public Button emojiBtnVs;
		public Button leaveGameBtn;
		public Button leaveGameBtnVs;
		public Button switchTeamsBtnVs;
		public Button gameOptionsBtn;
		public Button gameOptionsBtnVs;
		public Button voteKickBtn;
		public Button voteKickBtnVs;
		public Button changePasswordBtn;
		public Button changePasswordBtnVs;
		public GameObject titleController;
		public AudioClip countdownSfx;
		public GameObject mainMenuCampaign;
		public GameObject mainMenuVersus;
		public GameObject gameOptionsMenuCampaign;
		public GameObject gameOptionsMenuVersus;
		public GameObject gameMusicMenuCampaign;
		public GameObject gameMusicMenuVersus;
		public GameObject kickPlayerMenuCampaign;
		public GameObject kickPlayerMenuVersus;
		public GameObject privacyMenuCampaign;
		public GameObject privacyMenuVersus;
		public PlayerKick[] playerKickSlotsCampaign;
		public PlayerKick[] playerKickSlotsRed;
		public PlayerKick[] playerKickSlotsBlue;

		// Map options
		private string[] mapNames = new string[]{"The Badlands: Act I", "The Badlands: Act II"};
		private string[] mapStrings = new string[]{"MapImages/badlands1", "MapImages/badlands2"};
		private string[] mapDescriptions = new string[]{"A local cannibal insurgent group in the de-facto midwest of the New States of America known as the Cicadas has taken over a local refugee outpost in order to develop chemical warheads. Disrupt their operation and salvage the outpost.", 
			"The local Cicadas have shot down one of our evac choppers in the badlands. Rescue the surviving pilot and defend her until evac arrives."};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(1298f,10.53f,954.459f), new Vector3(119f, 0.17f, 116f) };

		// Ready status
		private GameObject myPlayerListEntry;
		private bool gameStarting = false;
        private char currentMode;
        public Player playerBeingKicked;
		public GameObject playerBeingKickedButton;
		public bool kickingPlayerFlag;
		public bool rejoinedRoomFlag;
		private short loadingCycles;

        // public void DisplayPopup(string message) {
		// 	ToggleButtons (false);
        //     if (templateUIClass.gameObject.activeInHierarchy)
        //     {
        //         templateUIClass.popup.GetComponentsInChildren<Text>()[0].text = message;
        //         templateUIClass.popup.SetActive(true);
        //     } else if (templateUIClassVs.gameObject.activeInHierarchy)
        //     {
        //         templateUIClassVs.popup.GetComponentsInChildren<Text>()[0].text = message;
        //         templateUIClassVs.popup.SetActive(true);
        //     }
		// }

		public void StartGameBtn() {
			if (!PhotonNetwork.CurrentRoom.IsOpen) {
				titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("This game is currently ending... please wait until the next round to join.");
				return;
			}
			if (currentMode == 'V') {
				StartGameVersus();
			} else if (currentMode == 'C')
            {
                StartGameCampaign();
            }
		}

		void StartGameCampaign() {
			// If we're the host, start the game assuming there are at least two ready players
			if (PhotonNetwork.IsMasterClient) {
				int readyCount = 0;

				// Loops through player entry prefabs and checks if they're ready by their color
				foreach (Player p in PhotonNetwork.PlayerList) {
					// If the ready status is green or the indicator doesn't exist (master)
					if (p.IsMasterClient || Convert.ToInt32(p.CustomProperties["readyStatus"]) == 1) {
						readyCount++;
					}
					if (readyCount >= 1) {
						break;
					}
				}

				if (readyCount >= 1) {
					SetMyselfAsStarter();
					pView.RPC("RpcStartGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					CampaignGameStart();
				} else {
					SetMyselfAsStarter();
				}
			}
		}

		void StartGameVersus() {
			// If we're the host, start the game assuming there are at least two ready players
			if (PhotonNetwork.IsMasterClient) {
				int redReadyCount = 0;
				int blueReadyCount = 0;

				// Loops through player entry prefabs and checks if they're ready by their color
				foreach (Player p in PhotonNetwork.PlayerList) {
					// If the ready status is green or the indicator doesn't exist (master)
					if (p.IsMasterClient || Convert.ToInt32(p.CustomProperties["readyStatus"]) == 1) {
						if ((string)p.CustomProperties["team"] == "red") {
							redReadyCount++;
						} else if ((string)p.CustomProperties["team"] == "blue") {
							blueReadyCount++;
						}
					}
					if (redReadyCount > 0 && blueReadyCount > 0) {
						break;
					}
				}

				if (redReadyCount > 0 && blueReadyCount > 0) {
					SetMyselfAsStarter();
					pView.RPC("RpcStartVersusGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					VersusGameStart();
				} else {
					SetMyselfAsStarter();
				}
			}
		}

		void ToggleButtons(bool status) {
			readyButton.GetComponent<Button> ().interactable = status;
            readyButtonVs.GetComponent<Button>().interactable = status;
			if (PlayerPreferences.playerPreferences.preferenceData.audioInputName != "None") {
				voiceChatBtn.interactable = status;
				voiceChatBtnVs.interactable = status;
			}
            // sendMsgBtn.interactable = status;
            // sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
            // emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
            leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
			gameOptionsBtn.interactable = status;
			gameOptionsBtnVs.interactable = status;
			ToggleMapChangeButtons(status);
			if (!status) {
				kickPlayerMenuCampaign.SetActive(false);
				kickPlayerMenuVersus.SetActive(false);
				gameOptionsMenuCampaign.SetActive(false);
				gameOptionsMenuVersus.SetActive(false);
				gameMusicMenuCampaign.SetActive(false);
				gameMusicMenuVersus.SetActive(false);
				privacyMenuCampaign.SetActive(false);
				privacyMenuVersus.SetActive(false);
			}
		}

		[PunRPC]
		void RpcToggleButtons(bool status, bool gameIsStarting) {
			gameStarting = gameIsStarting;
			readyButton.GetComponent<Button> ().interactable = status;
			readyButtonVs.GetComponent<Button> ().interactable = status;
			if (PlayerPreferences.playerPreferences.preferenceData.audioInputName != "None") {
				voiceChatBtn.interactable = status;
				voiceChatBtnVs.interactable = status;
			}
			// sendMsgBtn.interactable = status;
			// sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
			// emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
			leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
			gameOptionsBtn.interactable = status;
			gameOptionsBtnVs.interactable = status;
			ToggleMapChangeButtons(status);
			if (!status) {
				kickPlayerMenuCampaign.SetActive(false);
				kickPlayerMenuVersus.SetActive(false);
				gameOptionsMenuCampaign.SetActive(false);
				gameOptionsMenuVersus.SetActive(false);
				gameMusicMenuCampaign.SetActive(false);
				gameMusicMenuVersus.SetActive(false);
				privacyMenuCampaign.SetActive(false);
				privacyMenuVersus.SetActive(false);
			}
		}

		void ChangeReadyStatus() {
			int newStatus = Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1 ? 0 : 1;
			Hashtable h = new Hashtable();
			h.Add("readyStatus", newStatus);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
			if (gameStarting) {
				ToggleButtons(false);
			}
			// pView.RPC("RpcChangeReadyStatus", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, newStatus);
		}

		// [PunRPC]
		// void RpcChangeReadyStatus(int actorId, int newStatus) {
		// 	PlayerEntryPrefab p = playerListEntries[actorId].GetComponent<PlayerEntryPrefab>();
		// 	Player pl = PhotonNetwork.CurrentRoom.GetPlayer(actorId);
		// 	int isInGame = Convert.ToInt32(pl.CustomProperties["inGame"]);
		// 	if (newStatus == 0) {
		// 		if (isInGame == 1) {
		// 			p.SetReadyText('i');
		// 		} else {
		// 			p.SetReadyText('r');
		// 		}
		// 		p.SetReady(false);
		// 	} else if (newStatus == 1) {
		// 		if (isInGame == 1) {
		// 			p.SetReadyText('i');
		// 		} else {
		// 			p.SetReadyText('r');
		// 		}
		// 		p.SetReady(true);
		// 	}
		// }

		void StartGame(string level) {
			pView.RPC("RpcStartCampaignGame", RpcTarget.All);
		}

        void StartVersusGame(string level) {
            pView.RPC("RpcStartVersusGame", RpcTarget.All);
        }

		[PunRPC]
		void RpcStartCampaignGame() {
			if (PhotonNetwork.IsMasterClient) {
				CampaignGameStart();
			} else if (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
				CampaignGameStart();
			} else {
				ToggleButtons(true);
			}
		}

		void CampaignGameStart() {
			if (titleController.GetComponent<TitleControllerScript>().loadingScreen.alpha != 1f) {
				LoadingScreen();
			}
			string level = GetMapShortenedNameForMapName((string)PhotonNetwork.CurrentRoom.CustomProperties["mapName"]);
			titleController.GetComponent<TitleControllerScript>().friendsMessenger.CacheCurrentChat();
			PhotonNetwork.LoadLevel(level);
			if (PhotonNetwork.LocalPlayer.IsMasterClient) {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
			} else {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = false;
				loadingCycles = 0;
				StartCoroutine("DetermineMasterClientLoaded");
			}
		}

        [PunRPC]
        void RpcStartVersusGame() {
			if (PhotonNetwork.IsMasterClient) {
				VersusGameStart();
			} else if (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
				VersusGameStart();
			} else {
				ToggleButtons(true);
			}
        }

		void VersusGameStart() {
			if (titleController.GetComponent<TitleControllerScript>().loadingScreen.alpha != 1f) {
				LoadingScreen();
			}
			string level = GetMapShortenedNameForMapName((string)PhotonNetwork.CurrentRoom.CustomProperties["mapName"]);
			string myTeam = ((string)PhotonNetwork.LocalPlayer.CustomProperties["team"] == "red" ? "_Red" : "_Blue");
			titleController.GetComponent<TitleControllerScript>().friendsMessenger.CacheCurrentChat();
			PhotonNetwork.LoadLevel (level + myTeam);
			if (PhotonNetwork.LocalPlayer.IsMasterClient) {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
			} else {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = false;
				loadingCycles = 0;
				StartCoroutine("DetermineMasterClientLoaded");
			}
		}

		[PunRPC]
		void RpcStartGameCountdown() {
			StartCoroutine("StartGameCountdown");
		}

		private IEnumerator StartGameCountdown() {
			titleController.GetComponent<AudioSource> ().clip = countdownSfx;
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.SendServerMessage ("Game starting in 5");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.SendServerMessage ("Game starting in 4");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.SendServerMessage ("Game starting in 3");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.SendServerMessage ("Game starting in 2");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.SendServerMessage ("Game starting in 1");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);

			if (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
				LoadingScreen();
			}
			if (PhotonNetwork.IsMasterClient) {
				StartGame (mapNames [mapSelector.index]);
			}
		}

		[PunRPC]
		void RpcStartVersusGameCountdown() {
			StartCoroutine("StartVersusGameCountdown");
		}

        private IEnumerator StartVersusGameCountdown() {
			titleController.GetComponent<AudioSource> ().clip = countdownSfx;

			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.SendServerMessage ("Game starting in 5");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.SendServerMessage ("Game starting in 4");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.SendServerMessage ("Game starting in 3");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.SendServerMessage ("Game starting in 2");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.SendServerMessage ("Game starting in 1");
				pView.RPC("RpcSetGameIsStarting", RpcTarget.All, true);
			}
			yield return new WaitForSeconds (1f);

			if (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
				LoadingScreen();
			}
			if (PhotonNetwork.IsMasterClient) {
				StartVersusGame (mapNames [mapSelectorVs.index]);
			}
		}

		[PunRPC]
		void RpcLoadingScreen() {
			TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
			int i = (currentMode == 'C' ? mapSelector.index : mapSelectorVs.index);
			ts.InstantiateLoadingScreen (mapNames[i], mapDescriptions[i]);
			ts.ToggleLoadingScreen(true);
		}

		void LoadingScreen() {
			TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
			int i = (currentMode == 'C' ? mapSelector.index : mapSelectorVs.index);
			ts.InstantiateLoadingScreen (mapNames[i], mapDescriptions[i]);
			ts.ToggleLoadingScreen(true);
		}

		void Update() {
			if (PhotonNetwork.IsMasterClient) {
				readyButtonTxt.text = "START GAME";
                readyButtonVsTxt.text = "START GAME";
            } else {
                readyButtonTxt.text = "READY";
                readyButtonVsTxt.text = "READY";
            }
			if (gameStarting && (readyButton.GetComponent<Button>().interactable || readyButtonVs.GetComponent<Button>().interactable)) {
				if (PhotonNetwork.IsMasterClient || Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
					ToggleButtons(false);
				}
			}
		}

		public void SetMapInfo() {
			if (PhotonNetwork.IsMasterClient) {
				int i = currentMode == 'C' ? mapSelector.index : mapSelectorVs.index;
				Hashtable h = new Hashtable();
				h.Add("mapName", mapNames[i]);
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
		}

		void UpdateMapInfo()
		{
			string currentMapName = (string)PhotonNetwork.CurrentRoom.CustomProperties["mapName"];
			int i = (currentMode == 'C' ? mapSelector.GetIndexFromItem(currentMapName) : mapSelectorVs.GetIndexFromItem(currentMapName));
			Texture mapTexture = (Texture)Resources.Load(mapStrings[i]);
			if (currentMode == 'C') {
				mapSelector.index = i;
				mapPreviewThumb.texture = mapTexture;
				mapDescription.text = mapDescriptions[i];
				mapSelector.UpdateUI();
			} else if (currentMode == 'V') {
				mapSelectorVs.index = i;
				mapPreviewVsThumb.texture = mapTexture;
				mapDescriptionVs.text = mapDescriptions[i];
				mapSelectorVs.UpdateUI();
			}
		}

		[PunRPC]
		void RpcSetRank(int actorId, int exp) {
			if (playerListEntries == null || !playerListEntries.ContainsKey(actorId) || playerListEntries[actorId].GetComponent<PlayerEntryPrefab>() == null) return;
			PlayerEntryPrefab p = playerListEntries[actorId].GetComponent<PlayerEntryPrefab>();
			p.SetRank(PlayerData.playerdata.GetRankFromExp((uint)exp).name);
		}

		void ResetMapSelector()
		{
			mapSelector.index = 0;
			mapSelectorVs.index = 0;
		}

		public override void OnJoinedRoom()
		{
			PhotonNetwork.IsMessageQueueRunning = true;
			gameStarting = false;
			kickingPlayerFlag = false;
			ToggleButtons(true);
			mainPanelManager.ToggleTopBar(false);
			currentMode = ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp" ? 'C' : 'V');
			if (!rejoinedRoomFlag && PhotonNetwork.IsMasterClient) {
				ResetMapSelector();
				SetMapInfo();
			}
			PhotonNetwork.CurrentRoom.IsOpen = true;
			PhotonNetwork.CurrentRoom.IsVisible = true;
			UpdateMapInfo();
			SetStealthMusic();
			SetAssaultMusic();
			int privacyMode = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["privacy"]);
			if (privacyMode == 0) {
				passwordDisplayText.gameObject.SetActive(false);
				passwordDisplayTextVs.gameObject.SetActive(false);
				changePasswordBtn.interactable = false;
				changePasswordBtnVs.interactable = false;
				privacySelector.index = 0;
				privacySelectorVs.index = 0;
				privacySelector.UpdateUI();
				privacySelectorVs.UpdateUI();
			} else if (privacyMode == 1) {
				passwordDisplayText.gameObject.SetActive(true);
				passwordDisplayTextVs.gameObject.SetActive(true);
				string roomPass = (string)PhotonNetwork.CurrentRoom.CustomProperties["password"];
				passwordDisplayText.text = roomPass;
				passwordDisplayTextVs.text = roomPass;
				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					changePasswordBtn.interactable = true;
					changePasswordBtnVs.interactable = true;
				} else {
					changePasswordBtn.interactable = false;
					changePasswordBtnVs.interactable = false;
				}
				privacySelector.index = 1;
				privacySelectorVs.index = 1;
				privacySelector.UpdateUI();
				privacySelectorVs.UpdateUI();
			}
            // Disable any loading screens
            // connexion.ToggleLobbyLoadingScreen(false);
			Hashtable h = new Hashtable();
			// h.Add("exp", (int)PlayerData.playerdata.info.Exp);
			h.Add("readyStatus", 0);
			h.Add("starter", 0);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
			// pView.RPC("RpcSetRank", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, (int)PlayerData.playerdata.info.Exp);
			if (!PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(false);
				voteKickBtn.enabled = false;
				voteKickBtnVs.enabled = false;
				privacySelector.nextBtn.interactable = false;
				privacySelector.prevBtn.interactable = false;
				privacySelectorVs.nextBtn.interactable = false;
				privacySelectorVs.prevBtn.interactable = false;
				joinModeSelector.prevBtn.interactable = false;
				joinModeSelector.nextBtn.interactable = false;
				joinModeSelectorVs.prevBtn.interactable = false;
				joinModeSelectorVs.nextBtn.interactable = false;
			} else {
				ToggleMapChangeButtons(true);
				voteKickBtn.enabled = true;
				voteKickBtnVs.enabled = true;
				privacySelector.nextBtn.interactable = true;
				privacySelector.prevBtn.interactable = true;
				privacySelectorVs.nextBtn.interactable = true;
				privacySelectorVs.prevBtn.interactable = true;
				joinModeSelector.prevBtn.interactable = true;
				joinModeSelector.nextBtn.interactable = true;
				joinModeSelectorVs.prevBtn.interactable = true;
				joinModeSelectorVs.nextBtn.interactable = true;
			}
			joinModeSelector.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
			joinModeSelectorVs.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
			joinModeSelector.UpdateUI();
			joinModeSelectorVs.UpdateUI();
			if (currentMode == 'V') {
				PhotonNetwork.AutomaticallySyncScene = false;
				OnJoinedRoomVersus();
			} else if (currentMode == 'C')
            {
				PhotonNetwork.AutomaticallySyncScene = false;
                OnJoinedRoomCampaign();
            }
			// Voice chat handling
			if (VivoxVoiceManager.Instance.TransmittingSession == null) {
				VivoxVoiceManager.Instance.JoinChannel(PhotonNetwork.CurrentRoom.Name, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.AudioOnly);
				VivoxVoiceManager.Instance.AudioInputDevices.Muted = true;
				VivoxVoiceManager.Instance.AudioInputDevices.VolumeAdjustment = ((int)(titleController.GetComponent<TitleControllerScript>().voiceInputVolumeSlider.value * 100f) - 50);
				VivoxVoiceManager.Instance.AudioOutputDevices.VolumeAdjustment = ((int)(titleController.GetComponent<TitleControllerScript>().voiceOutputVolumeSlider.value * 100f) - 50);
			}
			if (PlayerPreferences.playerPreferences.preferenceData.audioInputName == "None") {
				voiceChatBtn.interactable = false;
				voiceChatBtnVs.interactable = false;
			}

			if (!PhotonNetwork.IsMasterClient) {
				pView.RPC("RpcPingServerForLobbyStates", RpcTarget.MasterClient);
			}

			rejoinedRoomFlag = false;
		}

		void OnJoinedRoomCampaign() {
			templateUIClass.ListRoomPanel.SetActive(false);
			templateUIClass.RoomPanel.SetActive(true);
            templateUIClass.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

			foreach (Player p in PhotonNetwork.PlayerList)
			{
				GameObject entry = Instantiate(PlayerListEntryPrefab);
				PlayerEntryPrefab entryScript = entry.GetComponent<PlayerEntryPrefab>();
				string rankToSet = null;
				if (p.IsLocal) {
					rankToSet = PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.Exp).name;
					myPlayerListEntry = entry;
				} else {
					rankToSet = PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(p.CustomProperties["exp"])).name;
				}
				entryScript.CreateEntry(p.NickName, rankToSet, p.ActorNumber, 'C');
				int readyStatus = Convert.ToInt32(p.CustomProperties["readyStatus"]);
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					entryScript.SetReadyText('i');
					if (p.IsMasterClient || readyStatus == 1) {
						entryScript.SetReady(true);
					} else {
						entryScript.SetReady(false);
					}
				} else {
					entryScript.SetReadyText('r');
					if (readyStatus == 1) {
						entryScript.SetReady(true);
					} else {
						entryScript.SetReady(false);
					}
				}
				entry.transform.SetParent(PlayersInRoomPanel, false);

				playerListEntries.Add(p.ActorNumber, entry);
			}

			if (!rejoinedRoomFlag) {
            	chat.SendServerMessage(PhotonNetwork.LocalPlayer.NickName + " has joined the game.");
			}
		}

		void OnJoinedRoomVersus() {
			templateUIClassVs.ListRoomPanel.SetActive(false);
			templateUIClassVs.RoomPanel.SetActive(true);
			templateUIClassVs.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

			foreach (Player p in PhotonNetwork.PlayerList)
			{
				GameObject entry = Instantiate(PlayerListEntryPrefab);
				PlayerEntryPrefab entryScript = entry.GetComponent<PlayerEntryPrefab>();
				string rankToSet = null;
				if (p.IsLocal) {
					rankToSet = PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.Exp).name;
					myPlayerListEntry = entry;
				} else {
					rankToSet = PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(p.CustomProperties["exp"])).name;
				}
				entryScript.CreateEntry(p.NickName, rankToSet, p.ActorNumber, 'R');
				int readyStatus = Convert.ToInt32(p.CustomProperties["readyStatus"]);
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					entryScript.SetReadyText('i');
					if (p.IsMasterClient || readyStatus == 1) {
						entryScript.SetReady(true);
					} else {
						entryScript.SetReady(false);
					}
				} else {
					entryScript.SetReadyText('r');
					if (readyStatus == 1) {
						entryScript.SetReady(true);
					} else {
						entryScript.SetReady(false);
					}
				}
                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
					if (!rejoinedRoomFlag) {
						// If it's me, set team captain as me if possible and set my team
						if (GetRedTeamSize(true) <= GetBlueTeamSize(true))
						{
							entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
							// entry.transform.localPosition = Vector3.zero;
							entryScript.SetTeam('R');
							SetTeamCaptainOnJoin('R');
							Hashtable h = new Hashtable();
							h.Add("team", "red");
							PhotonNetwork.LocalPlayer.SetCustomProperties(h);
							pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "R", true);
						}
						else
						{
							entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
							// entry.transform.localPosition = Vector3.zero;
							entryScript.SetTeam('B');
							SetTeamCaptainOnJoin('B');
							Hashtable h = new Hashtable();
							h.Add("team", "blue");
							PhotonNetwork.LocalPlayer.SetCustomProperties(h);
							pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "B", true);
						}
					} else {
						string myTeam = (string)p.CustomProperties["team"];
						if (myTeam == "red") {
							entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
							entryScript.SetTeam('R');
							SetTeamCaptainOnJoin('R');
							pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "R", true);
						} else if (myTeam == "blue") {
							entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
							entryScript.SetTeam('B');
							SetTeamCaptainOnJoin('B');
							pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "B", true);
						}
					}
                }
                else
                {
                    // Set this player's team on your end. Set the value for entry script and UI
                    string theirTeam = (string)p.CustomProperties["team"];
                    if (theirTeam == "red")
                    {
						entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
						// entry.transform.localPosition = Vector3.zero;
                        entryScript.SetTeam('R');
                    }
                    else if (theirTeam == "blue")
                    {
						entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
						// entry.transform.localPosition = Vector3.zero;
                        entryScript.SetTeam('B');
                    }
                }
				playerListEntries.Add(p.ActorNumber, entry);
			}

			if (!rejoinedRoomFlag) {
            	chatVs.SendServerMessage(PhotonNetwork.LocalPlayer.NickName + " has joined the game.");
			}
		}

        public void OnSwitchTeamsButtonClicked()
        {
            GameObject playerEntry = playerListEntries[PhotonNetwork.LocalPlayer.ActorNumber];
            PlayerEntryPrefab entry = playerEntry.GetComponent<PlayerEntryPrefab>();
            string currentTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
            char newTeam = (currentTeam == "red" ? 'B' : 'R');
            if (newTeam == 'R')
            {
				entry.SetTeam('R');
				entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
				Hashtable h = new Hashtable();
				h.Add("team", "red");
				h.Add("readyStatus", 0);
                PhotonNetwork.LocalPlayer.SetCustomProperties(h);
            } else if (newTeam == 'B')
            {
				entry.SetTeam('B');
				entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
				Hashtable h = new Hashtable();
				h.Add("team", "blue");
				h.Add("readyStatus", 0);
                PhotonNetwork.LocalPlayer.SetCustomProperties(h);
            }
            pView.RPC("RpcSwitchTeams", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, ""+newTeam, false);
        }

        [PunRPC]
        void RpcSwitchTeams(int actorId, string newTeam, bool joiningFlag)
        {
            GameObject entry = playerListEntries[actorId];
            PlayerEntryPrefab entryScript = entry.GetComponent<PlayerEntryPrefab>();
			entryScript.SetTeam(char.Parse(newTeam));
            if (newTeam == "R")
            {
				entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
            } else if (newTeam == "B")
            {
				entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
            }
			if (!joiningFlag) {
				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					int oldRedHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
					int oldBlueHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
					int newRedHost = -1;
					int newBlueHost = -1;
					Hashtable h = new Hashtable();
					// If master client just switched teams, set the captain of the team he just switched to to that person.
					// Then, assign a new captain to the team he just left
					if (actorId == PhotonNetwork.LocalPlayer.ActorNumber) {
						if (newTeam == "R") {
							newRedHost = PhotonNetwork.LocalPlayer.ActorNumber;
							newBlueHost = GetNextPlayerOnBlueTeam(actorId);
						} else if (newTeam == "B") {
							newBlueHost = PhotonNetwork.LocalPlayer.ActorNumber;
							newRedHost = GetNextPlayerOnRedTeam(actorId);
						}
					} else {
						// If not master client
						if (newTeam == "R") {
							// If just switched to red team and was previous blue captain, assign a new captain to blue
							if (oldBlueHost == actorId) {
								newBlueHost = GetNextPlayerOnBlueTeam(actorId);
							} else {
								if (oldBlueHost != -1 && PlayerStillInRoom(oldBlueHost)) {
									newBlueHost = oldBlueHost;
								}
							}
							// If there currently isn't a captain for red, then assign himself as captain
							if (oldRedHost == -1 || !PlayerStillInRoom(oldRedHost)) {
								newRedHost = actorId;
							}
						} else if (newTeam == "B") {
							// If just switched to blue team and was previous red captain, assign a new captain to red
							if (oldRedHost == actorId) {
								newRedHost = GetNextPlayerOnRedTeam(actorId);
							} else {
								if (oldRedHost != -1 && PlayerStillInRoom(oldRedHost)) {
									newRedHost = oldRedHost;
								}
							}
							// If there currently isn't a captain for blue, then assign himself as captain
							if (oldBlueHost == -1 || !PlayerStillInRoom(oldBlueHost)) {
								newBlueHost = actorId;
							}
						}
					}
					h.Add("redHost", newRedHost);
					h.Add("blueHost", newBlueHost);
					PhotonNetwork.CurrentRoom.SetCustomProperties(h);
				}
			}
        }

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
            string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
			GameObject entry = Instantiate(PlayerListEntryPrefab);
			PlayerEntryPrefab entryScript = entry.GetComponent<PlayerEntryPrefab>();
			string rankToSet = PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(newPlayer.CustomProperties["exp"])).name;
			entryScript.SetReady(false);
            if (gameMode == "versus")
            {
				entryScript.CreateEntry(newPlayer.NickName, rankToSet, newPlayer.ActorNumber, 'V');
                entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
            } else if (gameMode == "camp")
            {
				entryScript.CreateEntry(newPlayer.NickName, rankToSet, newPlayer.ActorNumber, 'C');
                entry.transform.SetParent(PlayersInRoomPanel, false);
            }
			if (PhotonNetwork.IsMasterClient) {
				Hashtable h = new Hashtable();
				h.Add("ping", (int)PhotonNetwork.GetPing());
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
			playerListEntries.Add(newPlayer.ActorNumber, entry);
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			Hashtable h = new Hashtable();
			Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
			playerListEntries.Remove(otherPlayer.ActorNumber);
			// RearrangePlayerSlots ();
			if (PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(true);
			}
			if (PhotonNetwork.IsMasterClient) {
				h.Add("ping", (int)PhotonNetwork.GetPing());
			}
			// If the player that left was a team captain, give it to someone else
			if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "versus")
			{
				// Ensure that master client is host of whichever team he's on
				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					int oldRedHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
					int oldBlueHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
					int newRedHost = -1;
					int newBlueHost = -1;
					string masterClientTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["team"];
					if (masterClientTeam == "red") {
						newRedHost = PhotonNetwork.LocalPlayer.ActorNumber;
					} else if (masterClientTeam == "blue") {
						newBlueHost = PhotonNetwork.LocalPlayer.ActorNumber;
					}
					// Ensure that a new team captain is set for both teams, if available
					if (newRedHost == -1 || !PlayerStillInRoom(newRedHost)) {
						// See if the current host still exists. If not, get the next one available
						if (oldRedHost == otherPlayer.ActorNumber) {
							newRedHost = GetNextPlayerOnRedTeam(-1);
						} else {
							newRedHost = oldRedHost;
						}
					}
					if (newBlueHost == -1 || !PlayerStillInRoom(newBlueHost)) {
						// See if the current host still exists. If not, get the next one available
						if (oldBlueHost == otherPlayer.ActorNumber) {
							newBlueHost = GetNextPlayerOnBlueTeam(-1);
						} else {
							newBlueHost = oldBlueHost;
						}
					}
					// Save
					h.Add("redHost", newRedHost);
					h.Add("blueHost", newBlueHost);
				}
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}

		public override void OnMasterClientSwitched(Player newMasterClient) {
			if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
				PhotonNetwork.Disconnect();
				PhotonNetwork.CurrentRoom.IsVisible = false;
				PhotonNetwork.LeaveRoom();
				TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
				ts.ToggleLoadingScreen(false);
				ts.TriggerAlertPopup("Lost connection to server.\nReason: The host has left the game.");
				ts.mainPanelManager.OpenFirstTab();
			} else {
				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					voteKickBtn.enabled = true;
					voteKickBtnVs.enabled = true;
					privacySelector.nextBtn.interactable = true;
					privacySelector.prevBtn.interactable = true;
					privacySelectorVs.nextBtn.interactable = true;
					privacySelectorVs.prevBtn.interactable = true;
					privacySelector.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["privacy"]);
					privacySelectorVs.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["privacy"]);
					privacySelector.UpdateUI();
					privacySelectorVs.UpdateUI();
					if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["privacy"]) == 1) {
						changePasswordBtn.interactable = true;
						changePasswordBtnVs.interactable = true;
					} else {
						changePasswordBtn.interactable = false;
						changePasswordBtnVs.interactable = false;
					}
					joinModeSelector.prevBtn.interactable = true;
					joinModeSelector.nextBtn.interactable = true;
					joinModeSelectorVs.prevBtn.interactable = true;
					joinModeSelectorVs.nextBtn.interactable = true;
					joinModeSelector.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
					joinModeSelectorVs.index = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["joinMode"]);
					joinModeSelector.UpdateUI();
					joinModeSelectorVs.UpdateUI();
				} else {
					voteKickBtn.enabled = false;
					voteKickBtnVs.enabled = false;
					privacySelector.nextBtn.interactable = false;
					privacySelector.prevBtn.interactable = false;
					privacySelectorVs.nextBtn.interactable = false;
					privacySelectorVs.prevBtn.interactable = false;
					changePasswordBtn.interactable = false;
					changePasswordBtnVs.interactable = false;
					joinModeSelector.prevBtn.interactable = false;
					joinModeSelector.nextBtn.interactable = false;
					joinModeSelectorVs.prevBtn.interactable = false;
					joinModeSelectorVs.nextBtn.interactable = false;
					changePasswordBtn.interactable = false;
					changePasswordBtnVs.interactable = false;
				}
				ResetLoadingState();
			}
		}

		void ToggleMapChangeButtons(bool b) {
			mapSelector.ToggleSelectorButtons(b);
			mapSelectorVs.ToggleSelectorButtons(b);
		}

		public override void OnLeftRoom()
		{
			if (playerListEntries == null) return;
			mapSelector.index = 0;
			mapSelectorVs.index = 0;
			templateUIClass.RoomPanel.SetActive(false);
            templateUIClassVs.RoomPanel.SetActive(false);
			templateUIClass.ListRoomPanel.SetActive(true);
            templateUIClassVs.ListRoomPanel.SetActive(true);

			foreach (GameObject entry in playerListEntries.Values)
			{
				Destroy(entry.gameObject);
			}
				
			playerListEntries.Clear();
			playerListEntries = null;
			templateUIClass.ChatText.text = "";
            templateUIClassVs.ChatText.text = "";
            ToggleButtons(true);
			PhotonNetwork.JoinLobby();
			mainPanelManager.ToggleTopBar(true);
			mainPanelManager.ReopenCurrentPanel();
			ChannelId leavingChannelId = VivoxVoiceManager.Instance.TransmittingSession.Channel;
			VivoxVoiceManager.Instance.TransmittingSession.Disconnect();
			VivoxVoiceManager.Instance.LoginSession.DeleteChannelSession(leavingChannelId);
			// mainPanelManager.ToggleBottomBar(true);
		}

		// void RearrangePlayerSlots() {
		// 	lastSlotUsed = 0;
		// 	foreach (GameObject entry in playerListEntries.Values) {
		// 		if (currentMode == 'C') {
		// 			entry.transform.SetParent(PlayersInRoomPanel, false);
		// 		} else if (currentMode == 'V') {
		// 			PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
		// 			if (p.redEntry.activeInHierarchy) {
		// 				entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
		// 			} else if (p.blueEntry.activeInHierarchy) {
		// 				entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
		// 			}
		// 		}
		// 	}
		// }

		public string GetMapImageFromMapName(string mapName) {
			for (int i = 0; i < mapStrings.Length; i++) {
				if (mapNames[i] == mapName) {
					return mapStrings[i];
				}
			}
			return "";
		}

		private string GetMapShortenedNameForMapName(string m) {
			if (m == "The Badlands: Act I") {
				return "Badlands1";
			} else if (m == "The Badlands: Act II") {
				return "Badlands2";
			}
			return "";
		}

		public override void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps) {
			int actorNo = targetPlayer.ActorNumber;
			if (changedProps.ContainsKey("readyStatus")) {
				int newStatus = Convert.ToInt32(changedProps["readyStatus"]);
				if (playerListEntries == null || !playerListEntries.ContainsKey(actorNo) || playerListEntries[actorNo].GetComponent<PlayerEntryPrefab>() == null) return;
				playerListEntries[actorNo].GetComponent<PlayerEntryPrefab>().SetReady(newStatus == 1);
			}
			if (changedProps.ContainsKey("exp")) {
				if (playerListEntries.ContainsKey(targetPlayer.ActorNumber)) {
					playerListEntries[actorNo].GetComponent<PlayerEntryPrefab>().SetRank(PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(changedProps["exp"])).name);
				}
			}
		}

		void ResetLoadingState()
		{
			TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
			ts.ToggleLoadingScreen(false);
			PhotonNetwork.IsMessageQueueRunning = true;
			gameStarting = false;
			kickingPlayerFlag = false;
			ToggleButtons(true);
			Hashtable h = new Hashtable();
			h.Add("readyStatus", 0);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
		}

		public override void OnRoomPropertiesUpdate (Hashtable propertiesThatChanged) {
			// If going in game or coming out of game, update everyone's entry
			if (propertiesThatChanged.ContainsKey("inGame")) {
				int val = Convert.ToInt32(propertiesThatChanged["inGame"]);
				if (val == 1) {
					if (playerListEntries == null) return;
					foreach (GameObject entry in playerListEntries.Values) {
						PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
						if (p == null) return;
						p.SetReadyText('i');
						if (PhotonNetwork.CurrentRoom.GetPlayer(p.actorId).IsMasterClient) {
							p.SetReady(true);
						}
					}
					if (Convert.ToInt32(PhotonNetwork.LocalPlayer.CustomProperties["readyStatus"]) == 1) {
						if (titleController.GetComponent<TitleControllerScript>().loadingScreen.alpha != 1f) {
							string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
							if (gameMode == "camp") {
								CampaignGameStart();
							} else if (gameMode == "versus") {
								VersusGameStart();
							}
						}
					}
				} else if (val == 0) {
					if (playerListEntries == null) return;
					foreach (GameObject entry in playerListEntries.Values) {
						PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
						if (p == null) return;
						p.SetReadyText('r');
						p.SetReady(false);
					}
					if (myPlayerListEntry.GetComponent<PlayerEntryPrefab>().IsReady()) {
						ResetLoadingState();
					}
				}
			}

			if (propertiesThatChanged.ContainsKey("kickedPlayers")) {
				string newKickedPlayers = (string)propertiesThatChanged["kickedPlayers"];
				string[] newKickedPlayersList = newKickedPlayers.Split(',');
				if (newKickedPlayersList.Contains(PhotonNetwork.NickName)) {
					PhotonNetwork.Disconnect();
					PhotonNetwork.LeaveRoom();
					TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
					ts.ToggleLoadingScreen(false);
					ts.TriggerAlertPopup("Lost connection to server.\nReason: You've been kicked from the game.");
					ts.mainPanelManager.OpenFirstTab();
				}
			}

			if (propertiesThatChanged.ContainsKey("mapName")) {
				UpdateMapInfo();
			}

			if (propertiesThatChanged.ContainsKey("privacy")) {
				// Show/remove password text label
				int newPrivacy = Convert.ToInt32(propertiesThatChanged["privacy"]);
				if (newPrivacy == 0) {
					passwordDisplayText.gameObject.SetActive(false);
					passwordDisplayTextVs.gameObject.SetActive(false);
					changePasswordBtn.interactable = false;
					changePasswordBtnVs.interactable = false;
				} else if (newPrivacy == 1) {
					passwordDisplayText.gameObject.SetActive(true);
					passwordDisplayTextVs.gameObject.SetActive(true);
					changePasswordBtn.interactable = true;
					changePasswordBtnVs.interactable = true;
				}
			}

			if (propertiesThatChanged.ContainsKey("password")) {
				// Update password text label
				passwordDisplayText.text = (string)propertiesThatChanged["password"];
				passwordDisplayTextVs.text = (string)propertiesThatChanged["password"];
			}
		}

		[PunRPC]
		void RpcSetGameIsStarting(bool b) {
			gameStarting = b;
		}

		public void ClearGameStarting() {
			pView.RPC("RpcSetGameIsStarting", RpcTarget.All, false);
		}

		IEnumerator DetermineMasterClientLoaded() {
			yield return new WaitForSeconds(2f);
			loadingCycles++;

			if (!PhotonNetwork.InRoom) {
				TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
				ts.ToggleLoadingScreen(false);
				ts.TriggerAlertPopup("Lost connection to server.\nReason: The host has left the game.");
				ts.mainPanelManager.OpenFirstTab();
			} else {
				if (loadingCycles >= LOADING_TIMEOUT_CYCLES) {
					// If stuck on loading screen for whatever reason, this is a fail-safe to ensure that the player eventually gets to go back to screen
					ResetLoadingState();
					yield break;
				}
				if (PhotonNetwork.LevelLoadingProgress >= 0.9f) {
					PhotonNetwork.IsMessageQueueRunning = true;
				}
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
				} else {
					StartCoroutine("DetermineMasterClientLoaded");
				}
			}
		}

		int GetRedTeamSize(bool skipMyself = false) {
			if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") return 0;
			int total = 0;
			foreach (Player p in PhotonNetwork.PlayerList) {
				if (skipMyself && p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
					continue;
				}
				if ((string)p.CustomProperties["team"] == "red") {
					total++;
				}
			}
			return total;
		}

		int GetBlueTeamSize(bool skipMyself = false) {
			if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") return 0;
			int total = 0;
			foreach (Player p in PhotonNetwork.PlayerList) {
				if (skipMyself&& p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
					continue;
				}
				if ((string)p.CustomProperties["team"] == "blue") {
					total++;
				}
			}
			return total;
		}

		bool ActorIsRedCaptain(int actorNo) {
			if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") return false;
			if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]) == actorNo) {
				return true;
			}
			return false;
		}

		bool ActorIsBlueCaptain(int actorNo) {
			if ((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] == "camp") return false;
			if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]) == actorNo) {
				return true;
			}
			return false;
		}

		int GetNextPlayerOnRedTeam(int skipId) {
			int next = -1;
			foreach (Player p in PhotonNetwork.PlayerList) {
				if ((string)p.CustomProperties["team"] == "red") {
					if (skipId == p.ActorNumber) continue;
					next = p.ActorNumber;
					break;
				}
			}
			return next;
		}

		int GetNextPlayerOnBlueTeam(int skipId) {
			int next = -1;
			foreach (Player p in PhotonNetwork.PlayerList) {
				if ((string)p.CustomProperties["team"] == "blue") {
					if (skipId == p.ActorNumber) continue;
					next = p.ActorNumber;
					break;
				}
			}
			return next;
		}

		void SetTeamCaptainOnJoin(char team)
        {
			if (team == 'R') {
				Hashtable h = new Hashtable();

				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					h.Add("redHost", PhotonNetwork.LocalPlayer.ActorNumber);
				} else {
					int currRedHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
					if (currRedHost == -1 || !PlayerStillInRoom(currRedHost)) {
						h.Add("redHost", PhotonNetwork.LocalPlayer.ActorNumber);
					}
				}

				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			} else if (team == 'B')
			{
				Hashtable h = new Hashtable();

				if (PhotonNetwork.LocalPlayer.IsMasterClient) {
					h.Add("blueHost", PhotonNetwork.LocalPlayer.ActorNumber);
				} else {
					int currBlueHost = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
					if (currBlueHost == -1 || !PlayerStillInRoom(currBlueHost)) {
						h.Add("blueHost", PhotonNetwork.LocalPlayer.ActorNumber);
					}
				}

				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
        }

		public void ResetPlayerKick() {
			playerBeingKicked = null;
			playerBeingKickedButton = null;
			kickingPlayerFlag = false;
		}

		public void ConfirmKickForPlayer(Player playerToKick, GameObject clickedButton) {
			titleController.GetComponent<TitleControllerScript>().TriggerConfirmPopup("ARE YOU SURE YOU WISH TO KICK PLAYER [" + playerToKick.NickName + "]?");
			playerBeingKicked = playerToKick;
			playerBeingKickedButton = clickedButton;
			kickingPlayerFlag = true;
		}

		public void KickPlayer(Player playerToKick)
		{
			if (PhotonNetwork.IsMasterClient && playerToKick != null) {
				string nickname = playerToKick.NickName;
				pView.RPC("RpcAlertKickedPlayer", RpcTarget.All, playerToKick.ActorNumber);
				PhotonNetwork.CloseConnection(playerToKick);
				string currentKickedPlayers = (string)PhotonNetwork.CurrentRoom.CustomProperties["kickedPlayers"];
				if (string.IsNullOrEmpty(currentKickedPlayers)) {
					currentKickedPlayers = nickname;
				} else {
					currentKickedPlayers += "," + nickname;
				}
				Hashtable h = new Hashtable();
				h.Add("kickedPlayers", currentKickedPlayers);
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
				playerBeingKickedButton.SetActive(false);
			}
			ResetPlayerKick();
		}

		public void ToggleMainMenuCampaign(bool on) {
			mainMenuCampaign.SetActive(on);
		}

		public void ToggleMainMenuVersus(bool on) {
			mainMenuVersus.SetActive(on);
		}

		public void ToggleGameOptionsMenuCampaign(bool on) {
			gameOptionsMenuCampaign.SetActive(on);
			if (!on) {
				// Save join mode
				Hashtable h = new Hashtable();
				h.Add("joinMode", joinModeSelector.index);
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
		}

		public void ToggleGameOptionsMenuVersus(bool on) {
			gameOptionsMenuVersus.SetActive(on);
			if (!on) {
				// Save join mode
				Hashtable h = new Hashtable();
				h.Add("joinMode", joinModeSelectorVs.index);
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
		}

		public void ToggleGameMusicMenuCampaign(bool on) {
			gameMusicMenuCampaign.SetActive(on);
			if (!on) {
				PlayerPreferences.playerPreferences.SavePreferences();
			}
		}

		public void ToggleGameMusicMenuVersus(bool on) {
			gameMusicMenuVersus.SetActive(on);
			if (!on) {
				PlayerPreferences.playerPreferences.SavePreferences();
			}
		}

		public void TogglePrivacyMenuCampaign(bool on) {
			privacyMenuCampaign.SetActive(on);
		}

		public void TogglePrivacyMenuVersus(bool on) {
			privacyMenuVersus.SetActive(on);
		}

		public void OnStealthMusicChanged(bool increase)
		{
			// Change playerpreferences
			if (increase) {
				if (PlayerPreferences.playerPreferences.preferenceData.stealthTrack < (PlayerPreferences.STEALTH_TRACK_COUNT - 1)) {
					PlayerPreferences.playerPreferences.preferenceData.stealthTrack++;
				}
			} else {
				if (PlayerPreferences.playerPreferences.preferenceData.stealthTrack > 0) {
					PlayerPreferences.playerPreferences.preferenceData.stealthTrack--;
				}
			}
			
			// Preview the song for a few seconds
			JukeboxScript.jukebox.PreviewTrack('S', PlayerPreferences.playerPreferences.preferenceData.stealthTrack);
		}

		public void OnAssaultMusicChanged(bool increase)
		{
			// Change playerpreferences
			if (increase) {
				if (PlayerPreferences.playerPreferences.preferenceData.assaultTrack < (PlayerPreferences.ASSAULT_TRACK_COUNT - 1)) {
					PlayerPreferences.playerPreferences.preferenceData.assaultTrack++;
				}
			} else {
				if (PlayerPreferences.playerPreferences.preferenceData.assaultTrack > 0) {
					PlayerPreferences.playerPreferences.preferenceData.assaultTrack--;
				}
			}

			// Preview the song for a few seconds
			JukeboxScript.jukebox.PreviewTrack('A', PlayerPreferences.playerPreferences.preferenceData.assaultTrack);
		}

		void SetStealthMusic()
		{
			stealthTrackSelector.index = PlayerPreferences.playerPreferences.preferenceData.stealthTrack;
			stealthTrackSelectorVs.index = PlayerPreferences.playerPreferences.preferenceData.stealthTrack;
			stealthTrackSelector.UpdateUI();
			stealthTrackSelectorVs.UpdateUI();
		}

		void SetAssaultMusic()
		{
			assaultTrackSelector.index = PlayerPreferences.playerPreferences.preferenceData.assaultTrack;
			assaultTrackSelectorVs.index = PlayerPreferences.playerPreferences.preferenceData.assaultTrack;
			assaultTrackSelector.UpdateUI();
			assaultTrackSelectorVs.UpdateUI();
		}

		public void SetRoomPrivacyCampaign()
		{
			if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
			Hashtable h = new Hashtable();
			h.Add("privacy", privacySelector.index);
			if (privacySelector.index == 0) {
				h.Add("password", null);
			} else {
				string newPass = "";
				for (int i = 0; i < 10; i++) {
					newPass += GenerateRandomAlphanumericChar();
				}
				h.Add("password", newPass);
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}

		public void SetRoomPrivacyVersus()
		{
			if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
			Hashtable h = new Hashtable();
			h.Add("privacy", privacySelectorVs.index);
			if (privacySelectorVs.index == 0) {
				h.Add("password", null);
			} else {
				string newPass = "";
				for (int i = 0; i < 10; i++) {
					newPass += GenerateRandomAlphanumericChar();
				}
				h.Add("password", newPass);
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}

		public bool SetRoomPassword(string passedPass = null)
		{
			Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
			string proposedPassword = passedPass == null ? titleController.GetComponent<TitleControllerScript>().roomPasswordInput.text : passedPass;
			if (proposedPassword.Length == 0 || proposedPassword.Length > 12 || regex.Matches(proposedPassword).Count == 0) {
				titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("The password must only consist of alphanumeric characters!");
				return false;
        	}

			// Passed, set room password
			Hashtable h = new Hashtable();
			h.Add("password", passedPass);
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			return true;
		}

		void ClearPassword()
		{
			Hashtable h = new Hashtable();
			h.Add("password", null);
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}

		public void ToggleKickPlayerListMenuCampaign(bool on) {
			if (on) {
				PopulateVoteKickSlotsCampaign();
			}
			kickPlayerMenuCampaign.SetActive(on);
		}

		public void ToggleKickPlayerListMenuVersus(bool on) {
			if (on) {
				PopulateVoteKickSlotsVersus();
			}
			kickPlayerMenuVersus.SetActive(on);
		}

		void PopulateVoteKickSlotsCampaign() {
			int i = 0; // Player kick slot iterator
			foreach (Player p in PhotonNetwork.PlayerList) {
				// Cannot kick yourself or master client
				if (p.IsMasterClient || p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
					continue;
				}
				playerKickSlotsCampaign[i].Initialize(p);
				playerKickSlotsCampaign[i].gameObject.SetActive(true);
				i++;
			}
			if (i <= 7) {
				for (int j = i; j < 8; j++) {
					playerKickSlotsCampaign[j].gameObject.SetActive(false);
				}
			}
		}

		void PopulateVoteKickSlotsVersus() {
			int redI = 0; // Player kick slot iterator
			int blueI = 0;
			foreach (Player p in PhotonNetwork.PlayerList) {
				// Cannot kick yourself or master client
				if (p.IsMasterClient || p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
					continue;
				}
				// Get team
				string theirTeam = (string)p.CustomProperties["team"];
				if (theirTeam == "red") {
					playerKickSlotsRed[redI].Initialize(p);
					playerKickSlotsRed[redI].gameObject.SetActive(true);
					redI++;
				} else if (theirTeam == "blue") {
					playerKickSlotsBlue[blueI].Initialize(p);
					playerKickSlotsBlue[blueI].gameObject.SetActive(true);
					blueI++;
				}
			}
			if (redI <= 7) {
				for (int j = redI; j < 8; j++) {
					playerKickSlotsRed[j].gameObject.SetActive(false);
				}
			}
			if (blueI <= 7) {
				for (int j = blueI; j < 8; j++) {
					playerKickSlotsBlue[j].gameObject.SetActive(false);
				}
			}
		}

		[PunRPC]
		void RpcAlertKickedPlayer(int actorNo) {
			if (PhotonNetwork.LocalPlayer.ActorNumber == actorNo) {
				titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("YOU'VE BEEN KICKED FROM THE GAME.");
			}
		}

		public void OnSpeechButtonPress()
		{
			if (VivoxVoiceManager.Instance.AudioInputDevices.Muted) {
				OnSpeechButtonDown();
			} else {
				OnSpeechButtonUp();
			}
		}

		void OnSpeechButtonDown()
		{
			JukeboxScript.jukebox.SetMusicVolume(0.02f);
			myPlayerListEntry.GetComponent<PlayerEntryPrefab>().ToggleSpeakingIndicator(true);
			VivoxVoiceManager.Instance.AudioInputDevices.Muted = false;
			pView.RPC("RpcToggleSpeechIndicatorForPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, 1);
			if (currentMode == 'C') {
				voiceChatBtn.GetComponent<Animator>().Play("Pressed");
			} else if (currentMode == 'V') {
				voiceChatBtnVs.GetComponent<Animator>().Play("Pressed");
			}
		}

		void OnSpeechButtonUp()
		{
			JukeboxScript.jukebox.SetMusicVolume((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
			myPlayerListEntry.GetComponent<PlayerEntryPrefab>().ToggleSpeakingIndicator(false);
			VivoxVoiceManager.Instance.AudioInputDevices.Muted = true;
			pView.RPC("RpcToggleSpeechIndicatorForPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, 0);
			if (currentMode == 'C') {
				voiceChatBtn.GetComponent<Animator>().Play("Normal");
			} else if (currentMode == 'V') {
				voiceChatBtnVs.GetComponent<Animator>().Play("Normal");
			}
		}

		[PunRPC]
		void RpcToggleSpeechIndicatorForPlayer(int actorNo, int on)
		{
			playerListEntries[actorNo].GetComponent<PlayerEntryPrefab>().ToggleSpeakingIndicator((on == 1 ? true : false));
		}

		[PunRPC]
		void RpcPingServerForLobbyStates()
		{
			string serializedSpeakers = null;
			bool first = true;
			foreach (KeyValuePair<int, GameObject> p in playerListEntries) {
				if (!first) {
					serializedSpeakers += ",";
				}
				serializedSpeakers += p.Key + "|" + (p.Value.GetComponent<PlayerEntryPrefab>().campaignVoiceActiveIndicator.activeInHierarchy || p.Value.GetComponent<PlayerEntryPrefab>().blueVoiceActiveIndicator.activeInHierarchy || p.Value.GetComponent<PlayerEntryPrefab>().redVoiceActiveIndicator.activeInHierarchy)
								+ '|' + (p.Value.GetComponent<PlayerEntryPrefab>().campaignReady.activeInHierarchy || p.Value.GetComponent<PlayerEntryPrefab>().blueReady.activeInHierarchy || p.Value.GetComponent<PlayerEntryPrefab>().redReady.activeInHierarchy);
				first = false;
			}

			pView.RPC("RpcSendStates", RpcTarget.All, serializedSpeakers);
		}

		[PunRPC]
		void RpcSendStates(string serializedSpeakers)
		{
			string[] speakersList = serializedSpeakers.Split(',');
			for (int i = 0; i < speakersList.Length; i++) {
				string[] thisSpeakerData = speakersList[i].Split('|');
				int thisActorNo = int.Parse(thisSpeakerData[0]);
				if (thisActorNo == PhotonNetwork.LocalPlayer.ActorNumber) {
					continue;
				}
				PlayerEntryPrefab pEntry = playerListEntries[thisActorNo].GetComponent<PlayerEntryPrefab>();
				bool thisActorSpeaking = bool.Parse(thisSpeakerData[1]);
				pEntry.ToggleSpeakingIndicator(thisActorSpeaking);
				bool thisActorReady = bool.Parse(thisSpeakerData[2]);
				pEntry.SetReady(thisActorReady);
			}
		}

		bool PlayerStillInRoom(int actorNo)
		{
			foreach (Player p in PhotonNetwork.PlayerList) {
				if (p.ActorNumber == actorNo) return true;
			}
			return false;
		}

		string GetJoiningModeString(int i)
		{
			if (i == 0) {
				return "Always Allow";
			} else if (i == 1) {
				return "Prompt";
			} else if (i == 2) {
				return "Stealth Prompt";
			}
			return "";
		}

		char GenerateRandomAlphanumericChar()
		{
			int r = Random.Range(48, 123);
			if (r >= 58 && r <= 64) {
				r = Random.Range(65, 91);
			} else if (r >= 91 && r <= 96) {
				r = Random.Range(97, 123);
			}
			return (char)r;
		}

		void SetMyselfAsStarter()
		{
			Hashtable h = new Hashtable();
			h.Add("starter", 1);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
		}

	}
}
