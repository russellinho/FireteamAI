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

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListPlayer : MonoBehaviourPunCallbacks
	{
		private PhotonView pView;

		[Header("Inside Room Panel")]
		public GameObject[] InsideRoomPanel;
		public GameObject[] InsideRoomPanelVs;
        public Text myTeamVsTxt;
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
        public Text readyButtonTxt;
        public Text readyButtonVsTxt;
		public RawImage mapPreviewThumb;
        public Text mapPreviewTxt;
		public RawImage mapPreviewVsThumb;
        public Text mapPreviewVsTxt;
		public Button mapNext;
		public Button mapNextVs;
		public Button mapPrev;
		public Button mapPrevVs;
		public Button sendMsgBtn;
		public Button sendMsgBtnVs;
		public Button emojiBtn;
		public Button emojiBtnVs;
		public Button leaveGameBtn;
		public Button leaveGameBtnVs;
		public GameObject titleController;
		public AudioClip countdownSfx;

		// Map options
		private int mapIndex = 0;
		private string[] mapNames = new string[]{"Badlands: Act I", "Badlands: Act II"};
		private string[] mapStrings = new string[]{"MapImages/badlands1", "MapImages/test"};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(-2f,1f,1f), new Vector3(0f, 0f, 0f) };

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
			SetMapInfo ();
			pView = GetComponent<PhotonView> ();
			redTeam = new ArrayList();
			blueTeam = new ArrayList();
		}

        public void DisplayPopup(string message) {
			ToggleButtons (false);
            if (templateUIClass.gameObject.activeInHierarchy)
            {
                templateUIClass.popup.GetComponentsInChildren<Text>()[0].text = message;
                templateUIClass.popup.SetActive(true);
            } else if (templateUIClassVs.gameObject.activeInHierarchy)
            {
                templateUIClassVs.popup.GetComponentsInChildren<Text>()[0].text = message;
                templateUIClassVs.popup.SetActive(true);
            }
		}

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
					StartCoroutine ("StartGameCountdown");
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
					StartCoroutine ("StartGameCountdown");
				} else {
					DisplayPopup ("There must be at least two ready players to start the game!");
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
						StartCoroutine ("StartVersusGameCountdown");
						return;
					} else {
                        // If there's only 1 player, they cannot start the game
                        // Set room invisible once it begins, for now
                        PhotonNetwork.CurrentRoom.IsOpen = true;
                        PhotonNetwork.CurrentRoom.IsVisible = true;
                        DisplayPopup("You cannot start a versus game without a player on both teams!");
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
					StartCoroutine ("StartVersusGameCountdown");
				} else {
					DisplayPopup ("There must be at least two ready players to start the game!");
				}
			} else {
				ChangeReadyStatus ();
			}
		}

		void ToggleButtons(bool status) {
			mapNext.interactable = status;
            mapNextVs.interactable = status;
			mapPrev.interactable = status;
            mapPrevVs.interactable = status;
			readyButton.GetComponent<Button> ().interactable = status;
            readyButtonVs.GetComponent<Button>().interactable = status;
            sendMsgBtn.interactable = status;
            sendMsgBtnVs.interactable = status;
			emojiBtn.interactable = status;
            emojiBtnVs.interactable = status;
			leaveGameBtn.interactable = status;
            leaveGameBtnVs.interactable = status;
		}

		[PunRPC]
		void RpcToggleButtons(bool status, bool gameIsStarting) {
			gameStarting = gameIsStarting;
			mapNext.interactable = status;
			mapPrev.interactable = status;
			readyButton.GetComponent<Button> ().interactable = status;
			sendMsgBtn.interactable = status;
			emojiBtn.interactable = status;
			leaveGameBtn.interactable = status;
		}

		void ChangeReadyStatus() {
			isReady = !isReady;
			pView.RPC ("RpcChangeReadyStatus", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isReady);
		}

		[PunRPC]
		public void RpcChangeReadyStatus(int playerId, bool readyStatus) {
			if (readyStatus) {
				playerListEntries [playerId].GetComponent<PlayerEntryScript> ().SetReady(true);
			} else {
				playerListEntries [playerId].GetComponent<PlayerEntryScript> ().SetReady(false);
			}
		}

		void StartGame(string level) {
			// Photon switch scene from lobby to loading screen to actual game. automaticallySyncScene should load map on clients.
			if (level.Equals ("Badlands: Act I")) {
				PhotonNetwork.LoadLevel ("BetaLevelNetwork");
			} else if (level.Equals("Badlands: Act II")) {
				PhotonNetwork.LoadLevel ("Badlands2");
			} else {
				PhotonNetwork.LoadLevel (level);
			}
		}

        void StartVersusGame(string level) {
            if (level.Equals ("Badlands: Act I")) {
                pView.RPC("RpcStartVersusGame", RpcTarget.All, "BetaLevelNetwork");
			} else if (level.Equals ("Badlands: Act II")) {
				pView.RPC("RpcStartVersusGame", RpcTarget.All, "Badlands2");
			} else {
                pView.RPC("RpcStartVersusGame", RpcTarget.All, level);
			}
        }

        [PunRPC]
        void RpcStartVersusGame(string level) {
			LoadingScreen();
            string myTeam = (myPlayerListEntry.GetComponent<PlayerEntryScript>().team == 'R' ? "Red" : "Blue");
            PhotonNetwork.LoadLevel (level + myTeam);
        }

		private IEnumerator StartGameCountdown() {
			titleController.GetComponent<AudioSource> ().clip = countdownSfx;
			titleController.GetComponent<AudioSource> ().Play ();
			chat.sendChatOfMaster ("Game starting in 5");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chat.sendChatOfMaster ("Game starting in 4");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chat.sendChatOfMaster ("Game starting in 3");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chat.sendChatOfMaster ("Game starting in 2");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chat.sendChatOfMaster ("Game starting in 1");
			yield return new WaitForSeconds (1f);

			pView.RPC ("RpcLoadingScreen", RpcTarget.All);
			if (PhotonNetwork.IsMasterClient) {
				StartGame (mapNames [mapIndex]);
			}
		}

        private IEnumerator StartVersusGameCountdown() {
			titleController.GetComponent<AudioSource> ().clip = countdownSfx;
			titleController.GetComponent<AudioSource> ().Play ();
			chatVs.sendChatOfMaster ("Game starting in 5");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chatVs.sendChatOfMaster ("Game starting in 4");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chatVs.sendChatOfMaster ("Game starting in 3");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chatVs.sendChatOfMaster ("Game starting in 2");
			yield return new WaitForSeconds (1f);
			titleController.GetComponent<AudioSource> ().Play ();
			chatVs.sendChatOfMaster ("Game starting in 1");
			yield return new WaitForSeconds (1f);

			// pView.RPC ("RpcLoadingScreen", RpcTarget.All);
			if (PhotonNetwork.IsMasterClient) {
				StartVersusGame (mapNames [mapIndex]);
			}
		}

		[PunRPC]
		void RpcLoadingScreen() {
			titleController.GetComponent<TitleControllerScript> ().InstantiateLoadingScreen (mapNames[mapIndex]);
		}

		void LoadingScreen() {
			titleController.GetComponent<TitleControllerScript> ().InstantiateLoadingScreen (mapNames[mapIndex]);
		}

		void Update() {
			if (!templateUIClass.popup.activeInHierarchy && !gameStarting) {
				if (!mapNext.interactable) {
					ToggleButtons (true);
				}
			}

			if (PhotonNetwork.IsMasterClient) {
				readyButtonTxt.text = "START";
                readyButtonVsTxt.text = "START";
            } else {
                readyButtonTxt.text = "READY";
                readyButtonVsTxt.text = "READY";
            }

		}

		void SetMapInfo() {
            Texture mapTexture = (Texture)Resources.Load(mapStrings[mapIndex]);
            mapPreviewThumb.texture = mapTexture;
			mapPreviewTxt.text = mapNames [mapIndex];
            mapPreviewVsThumb.texture = mapTexture;
            mapPreviewVsTxt.text = mapNames[mapIndex];
		}

		public void goToNextMap() {
			if (!PhotonNetwork.IsMasterClient) return;
			mapIndex++;
			if (mapIndex >= mapNames.Length) {
				mapIndex = 0;
			}
			SetMapInfo ();
		}

		public void goToPreviousMap() {
			if (!PhotonNetwork.IsMasterClient) return;
			mapIndex--;
			if (mapIndex < 0) {
				mapIndex = mapNames.Length - 1;
			}
			SetMapInfo ();
		}

		[PunRPC]
		void RpcSetRank(int actorId, int exp) {
			PlayerEntryScript p = playerListEntries[actorId].GetComponent<PlayerEntryScript>();
			p.SetRank(PlayerData.playerdata.GetRankFromExp((uint)exp).name);
		}

		public override void OnJoinedRoom()
		{
            // Disable any loading screens
            connexion.ToggleLobbyLoadingScreen(false);
			Hashtable h = new Hashtable();
			h.Add("exp", (int)PlayerData.playerdata.info.exp);
			PhotonNetwork.LocalPlayer.SetCustomProperties(h);
			pView.RPC("RpcSetRank", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, (int)PlayerData.playerdata.info.exp);
			currentMode = (!templateUIClassVs.gameObject.activeInHierarchy ? 'C' : 'V');
			if (!PhotonNetwork.IsMasterClient) {
				ToggleMapChangeButtons(false);
			}
			if (currentMode == 'V') {
				OnJoinedRoomVersus();
			} else if (currentMode == 'C')
            {
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
				PlayerEntryScript entryScript = entry.GetComponent<PlayerEntryScript>();
				if (p.IsLocal) {
					entryScript.SetRank(PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.exp).name);
					myPlayerListEntry = entry;
				} else {
					entryScript.SetRank(PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(p.CustomProperties["exp"])).name);
				}
				if (p.IsMasterClient) {
					entryScript.ToggleReadyIndicator(false);
				}
				entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
				entryScript.SetNameTag(p.NickName);
                entryScript.SetActorId(p.ActorNumber);
			
				playerListEntries.Add(p.ActorNumber, entry);
			}
            chat.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
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
				PlayerEntryScript entryScript = entry.GetComponent<PlayerEntryScript>();
				if (p.IsLocal) {
					entryScript.SetRank(PlayerData.playerdata.GetRankFromExp(PlayerData.playerdata.info.exp).name);
					myPlayerListEntry = entry;
				} else {
					entryScript.SetRank(PlayerData.playerdata.GetRankFromExp(Convert.ToUInt32(p.CustomProperties["exp"])).name);
				}
				if (p.IsMasterClient) {
					entryScript.ToggleReadyIndicator(false);
				}
				entry.transform.SetParent(InsideRoomPanelVs[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
				entryScript.SetNameTag(p.NickName);
                entryScript.SetActorId(p.ActorNumber);
                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // If it's me, set team captain as me if possible and set my team
                    if (redTeam.Count <= blueTeam.Count)
                    {
                        entryScript.SetTeam('R');
                        redTeam.Add(p.ActorNumber);
                        SetTeamCaptain('R');
                        myTeamVsTxt.text = "RED TEAM";
                        Hashtable h = new Hashtable();
                        h.Add("team", "red");
                        PhotonNetwork.LocalPlayer.SetCustomProperties(h);
                        pView.RPC("RpcSwitchTeams", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, "red");
                    }
                    else
                    {
                        entryScript.SetTeam('B');
                        blueTeam.Add(p.ActorNumber);
                        SetTeamCaptain('B');
                        myTeamVsTxt.text = "BLUE TEAM";
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
                        entryScript.SetTeam('R');
                        redTeam.Add(p.ActorNumber);
                    }
                    else if (theirTeam == "blue")
                    {
                        entryScript.SetTeam('B');
                        blueTeam.Add(p.ActorNumber);
                    }
                }
				playerListEntries.Add(p.ActorNumber, entry);
			}
            chatVs.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
		}

        public void OnSwitchTeamsButtonClicked()
        {
            GameObject playerEntry = playerListEntries[PhotonNetwork.LocalPlayer.ActorNumber];
            PlayerEntryScript entry = playerEntry.GetComponent<PlayerEntryScript>();
            entry.ChangeTeam();
            char newTeam = entry.team;
            if (newTeam == 'R')
            {
                blueTeam.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                redTeam.Add(PhotonNetwork.LocalPlayer.ActorNumber);
                myTeamVsTxt.text = "RED TEAM";
				Hashtable h = new Hashtable();
				h.Add("team", "red");
                PhotonNetwork.LocalPlayer.SetCustomProperties(h);
            } else if (newTeam == 'B')
            {
                redTeam.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                blueTeam.Add(PhotonNetwork.LocalPlayer.ActorNumber);
                myTeamVsTxt.text = "BLUE TEAM";
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
            PlayerEntryScript entryScript = entry.GetComponent<PlayerEntryScript>();
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
                            int nextBlueCaptain = playerListEntries[(int)blueTeam[0]].GetComponent<PlayerEntryScript>().actorId;
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
                            int nextRedCaptain = playerListEntries[(int)redTeam[0]].GetComponent<PlayerEntryScript>().actorId;
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
                entry.transform.SetParent(InsideRoomPanelVs[lastSlotUsed++].transform);
            } else if (gameMode == "camp")
            {
                entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
            }
			entry.transform.localPosition = Vector3.zero;
			entry.transform.localScale = Vector3.one;
			entry.GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;

			playerListEntries.Add(newPlayer.ActorNumber, entry);
            loadPlayerQueue.Enqueue(newPlayer);
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
			mapNext.gameObject.SetActive(b);
			mapPrev.gameObject.SetActive(b);
			mapNextVs.gameObject.SetActive(b);
			mapPrevVs.gameObject.SetActive(b);
		}

		public override void OnLeftRoom()
		{
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
		}

		void RearrangePlayerSlots() {
			lastSlotUsed = 0;
			foreach (GameObject entry in playerListEntries.Values) {
				if (currentMode == 'C') {
					entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
					entry.transform.localPosition = Vector3.zero;
				} else if (currentMode == 'V') {
					entry.transform.SetParent(InsideRoomPanelVs[lastSlotUsed++].transform);
					entry.transform.localPosition = Vector3.zero;
				}
			}
		}

	}
}
