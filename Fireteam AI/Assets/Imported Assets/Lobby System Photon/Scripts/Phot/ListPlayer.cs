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
	public class ListPlayer : MonoBehaviourPunCallbacks
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
		private bool isReady = false;
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
				// Set room invisible once it begins, for now
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;

				// Testing - comment in release
				if (PlayerData.playerdata.testMode == true) {
					pView.RPC ("RpcToggleButtons", RpcTarget.All, false, true);
					pView.RPC("RpcStartGameCountdown", RpcTarget.All);
					return;
				}

				int readyCount = 0;

				// Loops through player entry prefabs and checks if they're ready by their color
				foreach (GameObject o in playerListEntries.Values) {
					Image indicator = o.GetComponentInChildren<Image> ();
					// If the ready status is green or the indicator doesn't exist (master)
					if (!indicator || indicator.color.g == 1f) {
						readyCount++;
					}
					if (readyCount >= 2) {
						break;
					}
				}

				if (readyCount >= 2) {
					pView.RPC ("RpcToggleButtons", RpcTarget.All, false, true);
					pView.RPC("RpcStartGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
			}
		}

		void StartGameVersus() {
			// If we're the host, start the game assuming there are at least two ready players
			if (PhotonNetwork.IsMasterClient) {
				// Set room invisible once it begins, for now
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;

				// Testing - comment in release
				if (PlayerData.playerdata.testMode == true) {
                    Debug.Log("red: " + redTeam.Count + " blue: " + blueTeam.Count);
					if (redTeam.Count >= 1 && blueTeam.Count >= 1) {
						pView.RPC ("RpcToggleButtons", RpcTarget.All, false, true);
						pView.RPC("RpcStartVersusGameCountdown", RpcTarget.All);
						return;
					} else {
                        // If there's only 1 player, they cannot start the game
                        // Set room invisible once it begins, for now
                        PhotonNetwork.CurrentRoom.IsOpen = true;
                        PhotonNetwork.CurrentRoom.IsVisible = true;
                        titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("You cannot start a versus game without a player on both teams!");
						return;
					}
				}

				int readyCount = 0;

				// Loops through player entry prefabs and checks if they're ready by their color
				foreach (GameObject o in playerListEntries.Values) {
					Image indicator = o.GetComponentInChildren<Image> ();
					// If the ready status is green or the indicator doesn't exist (master)
					if (!indicator || indicator.color.g == 1f) {
						readyCount++;
					}
					if (readyCount >= 2) {
						break;
					}
				}

				if (readyCount >= 2) {
					pView.RPC ("RpcToggleButtons", RpcTarget.All, false, true);
					pView.RPC("RpcStartVersusGameCountdown", RpcTarget.All);
				} else {
					titleController.GetComponent<TitleControllerScript>().TriggerAlertPopup("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
			}
		}

		void ToggleButtons(bool status) {
			readyButton.GetComponent<Button> ().interactable = status;
            readyButtonVs.GetComponent<Button>().interactable = status;
            sendMsgBtn.interactable = status;
            sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
            // emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
            leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
		}

		[PunRPC]
		void RpcToggleButtons(bool status, bool gameIsStarting) {
			gameStarting = gameIsStarting;
			readyButton.GetComponent<Button> ().interactable = status;
			readyButtonVs.GetComponent<Button> ().interactable = status;
			sendMsgBtn.interactable = status;
			sendMsgBtnVs.interactable = status;
			// emojiBtn.interactable = status;
			// emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
			leaveGameBtnVs.interactable = status;
			switchTeamsBtnVs.interactable = status;
		}

		void ChangeReadyStatus() {
			isReady = !isReady;
			pView.RPC ("RpcChangeReadyStatus", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isReady);
		}

		[PunRPC]
		public void RpcChangeReadyStatus(int playerId, bool readyStatus) {
			if (readyStatus) {
				playerListEntries [playerId].GetComponent<PlayerEntryPrefab> ().SetReady(true);
			} else {
				playerListEntries [playerId].GetComponent<PlayerEntryPrefab> ().SetReady(false);
			}
		}

		void StartGame(string level) {
			// Photon switch scene from lobby to loading screen to actual game. automaticallySyncScene should load map on clients.
			if (level.Equals ("Badlands: Act I")) {
				pView.RPC("RpcStartCampaignGame", RpcTarget.All, "Badlands1");
			} else if (level.Equals("Badlands: Act II")) {
				pView.RPC("RpcStartCampaignGame", RpcTarget.All, "Badlands2");
			} else {
				pView.RPC("RpcStartCampaignGame", RpcTarget.All, level);
			}
		}

        void StartVersusGame(string level) {
            if (level.Equals ("Badlands: Act I")) {
                pView.RPC("RpcStartVersusGame", RpcTarget.All, "Badlands1");
			} else if (level.Equals ("Badlands: Act II")) {
				pView.RPC("RpcStartVersusGame", RpcTarget.All, "Badlands2");
			} else {
                pView.RPC("RpcStartVersusGame", RpcTarget.All, level);
			}
        }

		[PunRPC]
		void RpcStartCampaignGame(string level) {
			LoadingScreen();
			if (PhotonNetwork.IsMasterClient) {
				PhotonNetwork.LoadLevel(level);
			}
		}

        [PunRPC]
        void RpcStartVersusGame(string level) {
			LoadingScreen();
            string myTeam = (myPlayerListEntry.GetComponent<PlayerEntryPrefab>().GetTeam() == 'R' ? "_Red" : "_Blue");
            PhotonNetwork.LoadLevel (level + myTeam);
        }

		[PunRPC]
		void RpcStartGameCountdown() {
			StartCoroutine("StartGameCountdown");
		}

		private IEnumerator StartGameCountdown() {
			titleController.GetComponent<AudioSource> ().clip = countdownSfx;
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.sendChatOfMaster ("Game starting in 5");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.sendChatOfMaster ("Game starting in 4");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.sendChatOfMaster ("Game starting in 3");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.sendChatOfMaster ("Game starting in 2");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chat.sendChatOfMaster ("Game starting in 1");
			}
			yield return new WaitForSeconds (1f);

			// pView.RPC ("RpcLoadingScreen", RpcTarget.All);
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
				chatVs.sendChatOfMaster ("Game starting in 5");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.sendChatOfMaster ("Game starting in 4");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.sendChatOfMaster ("Game starting in 3");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.sendChatOfMaster ("Game starting in 2");
			}
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			if (PhotonNetwork.IsMasterClient) {
				chatVs.sendChatOfMaster ("Game starting in 1");
			}
			yield return new WaitForSeconds (1f);

			// pView.RPC ("RpcLoadingScreen", RpcTarget.All);
			if (PhotonNetwork.IsMasterClient) {
				StartVersusGame (mapNames [mapSelectorVs.index]);
			}
		}

		[PunRPC]
		void RpcLoadingScreen(int i) {
			TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
			ts.InstantiateLoadingScreen (mapNames[i]);
			ts.ToggleLoadingScreen(true);
		}

		void LoadingScreen() {
			TitleControllerScript ts = titleController.GetComponent<TitleControllerScript>();
			ts.InstantiateLoadingScreen (mapNames[currentMode == 'C' ? mapSelector.index : mapSelectorVs.index]);
			ts.ToggleLoadingScreen(true);
		}

		void Update() {
			// if (!templateUIClass.popup.activeInHierarchy && !gameStarting) {
			// 	if (!mapNext.interactable) {
			// 		ToggleButtons (true);
			// 	}
			// }

			if (PhotonNetwork.IsMasterClient) {
				readyButtonTxt.text = "START GAME";
                readyButtonVsTxt.text = "START GAME";
            } else {
                readyButtonTxt.text = "READY";
                readyButtonVsTxt.text = "READY";
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
			} else {
				if (PhotonNetwork.IsMasterClient) {
            		pView.RPC("RpcSetMapInfo", RpcTarget.All, i);
				}
			}
		}

		[PunRPC]
		void RpcSetMapInfo(int i) {
			Texture mapTexture = (Texture)Resources.Load(mapStrings[i]);
            mapPreviewThumb.texture = mapTexture;
            mapPreviewVsThumb.texture = mapTexture;
			mapDescription.text = mapDescriptions[i];
			mapDescriptionVs.text = mapDescriptions[i];
		}

		[PunRPC]
		void RpcSetRank(int actorId, int exp) {
			PlayerEntryPrefab p = playerListEntries[actorId].GetComponent<PlayerEntryPrefab>();
			p.SetRank(PlayerData.playerdata.GetRankFromExp((uint)exp).name);
		}

		public override void OnJoinedRoom()
		{
			mainPanelManager.ToggleTopBar(false);
			// mainPanelManager.ToggleBottomBar(false);
			if (PhotonNetwork.IsMasterClient) {
				SetMapInfo(true);
			}
            // Disable any loading screens
            // connexion.ToggleLobbyLoadingScreen(false);
			Hashtable h = new Hashtable();
			h.Add("exp", (int)PlayerData.playerdata.info.Exp);
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
				PhotonNetwork.AutomaticallySyncScene = true;
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
				if (p.IsMasterClient) {
					entryScript.SetReady(false);
				}
				entry.transform.SetParent(PlayersInRoomPanel, false);
				// entry.transform.localPosition = Vector3.zero;
			
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
				if (p.IsMasterClient) {
					entryScript.SetReady(false);
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
            } else if (newTeam == "blue")
            {
                entryScript.SetTeam('B');
                redTeam.Remove(actorId);
                blueTeam.Add(actorId);
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
            if (gameMode == "versus")
            {
                entry.transform.SetParent(PlayersInRoomPanelVsRed);
            } else if (gameMode == "camp")
            {
                entry.transform.SetParent(PlayersInRoomPanel, false);
            }
			// entry.transform.localPosition = Vector3.zero;
			// entry.transform.localScale = Vector3.one;
			// entry.GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;

			playerListEntries.Add(newPlayer.ActorNumber, entry);
            loadPlayerQueue.Enqueue(newPlayer);
			SetMapInfo();
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
			playerListEntries.Remove(otherPlayer.ActorNumber);
			RearrangePlayerSlots ();
			if (PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(true);
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
					entry.transform.SetParent(PlayersInRoomPanel);
					entry.transform.localPosition = Vector3.zero;
				} else if (currentMode == 'V') {
					entry.transform.SetParent(PlayersInRoomPanelVsRed);
					entry.transform.localPosition = Vector3.zero;
				}
			}
		}

	}
}
