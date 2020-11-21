using System;
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

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListPlayer : MonoBehaviourPunCallbacks, IInRoomCallbacks
	{
		public MainPanelManager mainPanelManager;
		public PhotonView pView;

		[Header("Inside Room Panel")]
		public Transform PlayersInRoomPanel;
		public Transform PlayersInRoomPanelVsRed;
		public Transform PlayersInRoomPanelVsBlue;
		private int lastSlotUsed;

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
		public RawImage mapPreviewThumb;
		public RawImage mapPreviewVsThumb;
		public TextMeshProUGUI mapDescription;
		public TextMeshProUGUI mapDescriptionVs;
		public HorizontalSelector mapSelector;
		public HorizontalSelector mapSelectorVs; 
		public Button sendMsgBtn;
		public Button sendMsgBtnVs;
		// public Button emojiBtn;
		// public Button emojiBtnVs;
		public Button leaveGameBtn;
		public Button leaveGameBtnVs;
		public Button switchTeamsBtnVs;
		public GameObject titleController;
		public AudioClip countdownSfx;

		// Map options
		private string[] mapNames = new string[]{"The Badlands: Act I", "The Badlands: Act II"};
		private string[] mapStrings = new string[]{"MapImages/badlands1", "MapImages/badlands2"};
		private string[] mapDescriptions = new string[]{"A local cannibal insurgent group in the de-facto midwest of the New States of America known as the Cicadas has taken over a local refugee outpost in order to develop chemical warheads. Disrupt their operation and salvage the outpost.", 
			"The local Cicadas have shot down one of our evac choppers in the badlands. Rescue the surviving pilot and defend her until evac arrives."};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(-2f,1f,1f), new Vector3(119f, -5.19f, 116f) };

		// Ready status
		private GameObject myPlayerListEntry;
		private bool gameStarting = false;
        private char currentMode;

		// Versus mode state
		private ArrayList redTeam;
		private ArrayList blueTeam;
        private Queue loadPlayerQueue = new Queue();

		void Start() {
			SetMapInfo (true);
			pView = GetComponent<PhotonView> ();
			redTeam = new ArrayList();
			blueTeam = new ArrayList();
		}

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
					pView.RPC("RpcStartGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					CampaignGameStart();
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
						if (p.CustomProperties["team"] == "red") {
							redReadyCount++;
						} else if (p.CustomProperties["team"] == "blue") {
							blueReadyCount++;
						}
					}
					if (redReadyCount > 0 && blueReadyCount > 0) {
						break;
					}
				}

				if (redReadyCount > 0 && blueReadyCount > 0) {
					pView.RPC("RpcStartVersusGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					VersusGameStart();
				}
			}
		}

		void ToggleButtons(bool status) {
			readyButton.GetComponent<Button> ().interactable = status;
            readyButtonVs.GetComponent<Button>().interactable = status;
            // sendMsgBtn.interactable = status;
            // sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
            // emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
            leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
			ToggleMapChangeButtons(status);
		}

		[PunRPC]
		void RpcToggleButtons(bool status, bool gameIsStarting) {
			gameStarting = gameIsStarting;
			readyButton.GetComponent<Button> ().interactable = status;
			readyButtonVs.GetComponent<Button> ().interactable = status;
			// sendMsgBtn.interactable = status;
			// sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
			// emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
			leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
			ToggleMapChangeButtons(status);
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
			PhotonNetwork.LoadLevel(level);
			if (PhotonNetwork.LocalPlayer.IsMasterClient) {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
			} else {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = false;
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
			PhotonNetwork.LoadLevel (level + myTeam);
			if (PhotonNetwork.LocalPlayer.IsMasterClient) {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
			} else {
				PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = false;
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

			LoadingScreen();
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

			LoadingScreen();
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

		public void SetMapInfo(bool offline = false) {
			int i = currentMode == 'C' ? mapSelector.index : mapSelectorVs.index;
			if (offline) {
				Texture mapTexture = (Texture)Resources.Load(mapStrings[i]);
				mapPreviewThumb.texture = mapTexture;
				mapPreviewVsThumb.texture = mapTexture;
				mapDescription.text = mapDescriptions[i];
				mapDescriptionVs.text = mapDescriptions[i];
				if (PhotonNetwork.InRoom) {
					Hashtable h = new Hashtable();
					h.Add("mapName", mapNames[i]);
					PhotonNetwork.CurrentRoom.SetCustomProperties(h);
				}
			} else {
				if (PhotonNetwork.IsMasterClient) {
            		pView.RPC("RpcSetMapInfo", RpcTarget.All, i);
				}
			}
		}

		[PunRPC]
		void RpcSetMapInfo(int i) {
			Hashtable h = new Hashtable();
			Texture mapTexture = (Texture)Resources.Load(mapStrings[i]);
            mapPreviewThumb.texture = mapTexture;
            mapPreviewVsThumb.texture = mapTexture;
			mapDescription.text = mapDescriptions[i];
			mapDescriptionVs.text = mapDescriptions[i];
			h.Add("mapName", mapNames[i]);
			PhotonNetwork.CurrentRoom.SetCustomProperties(h);
		}

		[PunRPC]
		void RpcSetRank(int actorId, int exp) {
			PlayerEntryPrefab p = playerListEntries[actorId].GetComponent<PlayerEntryPrefab>();
			p.SetRank(PlayerData.playerdata.GetRankFromExp((uint)exp).name);
		}

		public override void OnJoinedRoom()
		{
			ToggleButtons(true);
			mainPanelManager.ToggleTopBar(false);
			// mainPanelManager.ToggleBottomBar(false);
			if (PhotonNetwork.IsMasterClient) {
				SetMapInfo(true);
				PhotonNetwork.CurrentRoom.IsOpen = true;
				PhotonNetwork.CurrentRoom.IsVisible = true;
			}
            // Disable any loading screens
            // connexion.ToggleLobbyLoadingScreen(false);
			Hashtable h = new Hashtable();
			h.Add("exp", (int)PlayerData.playerdata.info.Exp);
			h.Add("readyStatus", 0);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
			pView.RPC("RpcSetRank", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, (int)PlayerData.playerdata.info.Exp);
			currentMode = (!templateUIClassVs.gameObject.activeInHierarchy ? 'C' : 'V');
			if (!PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(false);
			} else {
				ToggleMapChangeButtons(true);
			}
			if (currentMode == 'V') {
				PhotonNetwork.AutomaticallySyncScene = false;
				OnJoinedRoomVersus();
			} else if (currentMode == 'C')
            {
				PhotonNetwork.AutomaticallySyncScene = false;
                OnJoinedRoomCampaign();
            }
		}

		void OnJoinedRoomCampaign() {
			templateUIClass.ListRoomPanel.SetActive(false);
			templateUIClass.RoomPanel.SetActive(true);
            templateUIClass.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

			lastSlotUsed = 0;
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
            chat.SendServerMessage(PhotonNetwork.LocalPlayer.NickName + " has joined the game.");
		}

		void OnJoinedRoomVersus() {
			templateUIClassVs.ListRoomPanel.SetActive(false);
			templateUIClassVs.RoomPanel.SetActive(true);
			templateUIClassVs.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

            lastSlotUsed = 0;
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
                    // If it's me, set team captain as me if possible and set my team
                    if (redTeam.Count <= blueTeam.Count)
                    {
						entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
						// entry.transform.localPosition = Vector3.zero;
                        entryScript.SetTeam('R');
                        redTeam.Add(p.ActorNumber);
                        SetTeamCaptain('R');
                        Hashtable h = new Hashtable();
                        h.Add("team", "red");
                        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
                        pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "red");
                    }
                    else
                    {
						entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
						// entry.transform.localPosition = Vector3.zero;
                        entryScript.SetTeam('B');
                        blueTeam.Add(p.ActorNumber);
                        SetTeamCaptain('B');
                        Hashtable h = new Hashtable();
                        h.Add("team", "blue");
                        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
                        pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "blue");
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
                        redTeam.Add(p.ActorNumber);
                    }
                    else if (theirTeam == "blue")
                    {
						entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
						// entry.transform.localPosition = Vector3.zero;
                        entryScript.SetTeam('B');
                        blueTeam.Add(p.ActorNumber);
                    }
                }
				playerListEntries.Add(p.ActorNumber, entry);
			}
            chatVs.SendServerMessage(PhotonNetwork.LocalPlayer.NickName + " has joined the game.");
		}

        public void OnSwitchTeamsButtonClicked()
        {
            GameObject playerEntry = playerListEntries[PhotonNetwork.LocalPlayer.ActorNumber];
            PlayerEntryPrefab entry = playerEntry.GetComponent<PlayerEntryPrefab>();
            entry.ChangeTeam();
            char newTeam = entry.GetTeam();
            if (newTeam == 'R')
            {
				entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
                blueTeam.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                redTeam.Add(PhotonNetwork.LocalPlayer.ActorNumber);
				Hashtable h = new Hashtable();
				h.Add("team", "red");
                PhotonNetwork.LocalPlayer.SetCustomProperties(h);
            } else if (newTeam == 'B')
            {
				entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
                redTeam.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                blueTeam.Add(PhotonNetwork.LocalPlayer.ActorNumber);
				Hashtable h = new Hashtable();
				h.Add("team", "blue");
                PhotonNetwork.LocalPlayer.SetCustomProperties(h);
            }
            SetTeamCaptain(newTeam);
            pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, (newTeam == 'R' ? "red" : "blue"));
        }

        [PunRPC]
        void RpcSwitchTeams(int actorId, string newTeam)
        {
            GameObject entry = playerListEntries[actorId];
            PlayerEntryPrefab entryScript = entry.GetComponent<PlayerEntryPrefab>();
            if (newTeam == "red")
            {
                entryScript.SetTeam('R');
                blueTeam.Remove(actorId);
                redTeam.Add(actorId);
				entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
            } else if (newTeam == "blue")
            {
                entryScript.SetTeam('B');
                redTeam.Remove(actorId);
                blueTeam.Add(actorId);
				entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
            }
        }

        void SetTeamCaptain(char team)
        {
            int myId = PhotonNetwork.LocalPlayer.ActorNumber;
            if (team == 'R')
            {
                int currentRedCaptain = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
                // Add yourself as a captain if there isn't one
                if (currentRedCaptain == -1)
                {
					Hashtable h = new Hashtable();
					h.Add("redHost", myId);
                    // Remove yourself from captain of blue team if you were captain and set the next available person if there is one
                    int currentBlueCaptain = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
                    if (currentBlueCaptain != -1 && currentBlueCaptain == myId)
                    {
                        if (blueTeam.Count > 0)
                        {
                            int nextBlueCaptain = playerListEntries[(int)blueTeam[0]].GetComponent<PlayerEntryPrefab>().actorId;
                            h.Add("blueHost", nextBlueCaptain);
                        } else
                        {
                            h.Add("blueHost", -1);
                        }
                    }
					PhotonNetwork.CurrentRoom.SetCustomProperties(h);
                }
            } else
            {
                int currentBlueCaptain = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["blueHost"]);
                // Add yourself as a captain if there isn't one
                if (currentBlueCaptain == -1)
                {
					Hashtable h = new Hashtable();
                    h.Add("blueHost", myId);
                    // Remove yourself from captain of red team if you were captain and set the next available person if there is one
                    int currentRedCaptain = Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["redHost"]);
                    if (currentRedCaptain != -1 && currentRedCaptain == myId)
                    {
                        if (redTeam.Count > 0)
                        {
                            int nextRedCaptain = playerListEntries[(int)redTeam[0]].GetComponent<PlayerEntryPrefab>().actorId;
                            h.Add("redHost", nextRedCaptain);
                        }
                        else
                        {
                            h.Add("redHost", -1);
                        }
                    }
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
            loadPlayerQueue.Enqueue(newPlayer);
			SetMapInfo();
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
				if (otherPlayer.IsMasterClient) {
					PlayerData.playerdata.disconnectedFromServer = true;
					PlayerData.playerdata.disconnectReason = "The host has left the game.";
					TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
					ts.ToggleLoadingScreen(false);
					ts.mainPanelManager.OpenFirstTab();
				}
			}
			Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
			playerListEntries.Remove(otherPlayer.ActorNumber);
			RearrangePlayerSlots ();
			if (PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(true);
			}
			if (PhotonNetwork.IsMasterClient) {
				Hashtable h = new Hashtable();
				h.Add("ping", (int)PhotonNetwork.GetPing());
				PhotonNetwork.CurrentRoom.SetCustomProperties(h);
			}
		}

		void ToggleMapChangeButtons(bool b) {
			mapSelector.ToggleSelectorButtons(b);
			mapSelectorVs.ToggleSelectorButtons(b);
		}

		public override void OnLeftRoom()
		{
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
			// mainPanelManager.ToggleBottomBar(true);
		}

		void RearrangePlayerSlots() {
			lastSlotUsed = 0;
			foreach (GameObject entry in playerListEntries.Values) {
				if (currentMode == 'C') {
					entry.transform.SetParent(PlayersInRoomPanel, false);
				} else if (currentMode == 'V') {
					PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
					if (p.redEntry.activeInHierarchy) {
						entry.transform.SetParent(PlayersInRoomPanelVsRed, false);
					} else if (p.blueEntry.activeInHierarchy) {
						entry.transform.SetParent(PlayersInRoomPanelVsBlue, false);
					}
				}
			}
		}

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
				playerListEntries[actorNo].GetComponent<PlayerEntryPrefab>().SetReady(newStatus == 1);
			}
		}

		public override void OnRoomPropertiesUpdate (Hashtable propertiesThatChanged) {
			// If going in game or coming out of game, update everyone's entry
			if (propertiesThatChanged.ContainsKey("inGame")) {
				int val = Convert.ToInt32(propertiesThatChanged["inGame"]);
				if (val == 1) {
					foreach (GameObject entry in playerListEntries.Values) {
						PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
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
					foreach (GameObject entry in playerListEntries.Values) {
						PlayerEntryPrefab p = entry.GetComponent<PlayerEntryPrefab>();
						p.SetReadyText('r');
						p.SetReady(false);
					}
				}
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

			if (!PhotonNetwork.InRoom) {
				TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
				ts.ToggleLoadingScreen(false);
				ts.mainPanelManager.OpenFirstTab();
			} else {
				if (PhotonNetwork.LevelLoadingProgress >= 0.9f) {
					PhotonNetwork.IsMessageQueueRunning = true;
				}
				if (Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["inGame"]) == 1) {
					Debug.Log("after");
					PhotonNetwork._AsyncLevelLoadingOperation.allowSceneActivation = true;
				} else {
					StartCoroutine("DetermineMasterClientLoaded");
				}
			}
		}

	}
}
