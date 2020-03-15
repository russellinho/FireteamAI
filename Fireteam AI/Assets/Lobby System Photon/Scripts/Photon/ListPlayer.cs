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
        // Timeout from joining preplanning after attempting 8 times
        private const short MAX_PREPLANNING_TIMEOUT_CNT = 8; 

		private PhotonView pView;

		[Header("Inside Room Panel")]
		public GameObject[] InsideRoomPanel;
		public GameObject[] InsideRoomPanelVs;
        public GameObject[] InsideRoomPanelPreplanning;
        public Text myTeamVsTxt;
        public Text myTeamPreplanningTxt;
		private int lastSlotUsed;

		public Template templateUIClass;
		public Template templateUIClassVs;
        public Connexion connexion;
		public GameObject PlayerListEntryPrefab;
		public Dictionary<int, GameObject> playerListEntries;
		public TChat chat;
		public TChat chatVs;
        public TChat chatPreplanning;
		public GameObject readyButton;
		public GameObject readyButtonVs;
        public GameObject readyButtonPreplanning;
        public Text readyButtonTxt;
        public Text readyButtonVsTxt;
        public Text readyButtonPreplanningTxt;
		public RawImage mapPreviewThumb;
        public Text mapPreviewTxt;
		public RawImage mapPreviewVsThumb;
        public Text mapPreviewVsTxt;
        public RawImage mapPreviewPreplanningThumb;
        public Text mapPreviewPreplanningTxt;
		public Button mapNext;
		public Button mapNextVs;
		public Button mapPrev;
		public Button mapPrevVs;
		public Button sendMsgBtn;
		public Button sendMsgBtnVs;
        public Button sendMsgBtnPreplanning;
		public Button emojiBtn;
		public Button emojiBtnVs;
        public Button emojiBtnPreplanning;
		public Button leaveGameBtn;
		public Button leaveGameBtnVs;
        public Button leaveGameBtnPreplanning;
		public GameObject titleController;
		public AudioClip countdownSfx;
        private string preplanningTeam;
        private string preplanningVersusId;
        private bool preplanningIsReady;
        private short preplanningSyncDelay;
        private bool preplanningSyncComplete;
        private bool versusGameStarting;

		// Map options
		private int mapIndex = 0;
		private string[] mapNames = new string[]{"Badlands: Act I"};
		private string[] mapStrings = new string[]{"MapImages/badlands1"};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(-2f,1f,1f)};

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
            preplanningSyncDelay = 1800;
		}

        void FixedUpdate()
        {
            // Check whether we can start the game or not
            CheckCanStartVersusMatch();
        }

        void CheckCanStartVersusMatch()
        {
            if (!string.IsNullOrEmpty(preplanningVersusId)) {
                if (!preplanningSyncComplete && preplanningSyncDelay > 0)
                {
                    preplanningSyncDelay--;
                    return;
                } else
                {
                    preplanningSyncComplete = true;
                    preplanningSyncDelay = 400;
                    readyButtonPreplanning.GetComponent<Button>().interactable = true;
                }
                // Only do this if the user is in preplanning and the user is the host.
                // If both sides are marked as ready (in DB), then start the countdown
                // TODO: Later ensure that most players are ready before starting
                if (PhotonNetwork.IsMasterClient && !versusGameStarting)
                {
                    DAOScript.dao.dbRef.Child("fteam_ai_matches").Child(preplanningVersusId).Child(preplanningTeam).Child("isReady").SetValueAsync(preplanningIsReady).ContinueWith(task =>
                    {
                        DAOScript.dao.dbRef.Child("fteam_ai_matches").Child(preplanningVersusId).GetValueAsync().ContinueWith(taskA =>
                        {
                            DataSnapshot snapshot = taskA.Result;
                            string redReady = snapshot.Child("red").Child("isReady").ToString();
                            string blueReady = snapshot.Child("blue").Child("isReady").ToString();
                            if (redReady == "true" && blueReady == "true")
                            {
                                versusGameStarting = true;
                                StartCoroutine(StartPreplanningCountdown());
                            }
                        });
                    });
                }
            }
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
			} else if (currentMode == 'P') {
				StartGamePreplanning();
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

        void StartGamePreplanning()
        {
            // If we're the host, start the game assuming there are at least two ready players
            if (PhotonNetwork.IsMasterClient)
            {
                // Testing - comment in release
                if (PlayerData.playerdata.testMode == true)
                {
                    // Set ready/unready in the DB
                    preplanningIsReady = !preplanningIsReady;
                    if (preplanningIsReady)
                    {
                        chatPreplanning.sendChatOfMaster("Waiting for the other team to be ready... match will begin when both teams are ready.");
                    }
                    return;
                }

                int readyCount = 0;

                // Loops through player entry prefabs and checks if they're ready by their color
                foreach (GameObject o in playerListEntries.Values)
                {
                    Image indicator = o.GetComponentInChildren<Image>();
                    // If the ready status is green or the indicator doesn't exist (master)
                    if (!indicator || indicator.color.g == 1f)
                    {
                        readyCount++;
                    }
                    if (readyCount >= 2)
                    {
                        break;
                    }
                }

                if (readyCount >= 2)
                {
                    pView.RPC("RpcToggleButtons", RpcTarget.All, false, true);
                    StartCoroutine("StartVersusGameCountdown");
                }
                else
                {
                    DisplayPopup("There must be at least two ready players to start the game!");
                }
            }
            else
            {
                ChangeReadyStatus();
            }
        }

		void ToggleButtons(bool status) {
			mapNext.interactable = status;
            mapNextVs.interactable = status;
			mapPrev.interactable = status;
            mapPrevVs.interactable = status;
			readyButton.GetComponent<Button> ().interactable = status;
            readyButtonVs.GetComponent<Button>().interactable = status;
            readyButtonPreplanning.GetComponent<Button>().interactable = status;
            if (PhotonNetwork.IsMasterClient && !preplanningSyncComplete) {
                readyButtonPreplanning.GetComponent<Button>().interactable = false;
            }
            sendMsgBtn.interactable = status;
            sendMsgBtnVs.interactable = status;
            sendMsgBtnPreplanning.interactable = status;
			emojiBtn.interactable = status;
            emojiBtnVs.interactable = status;
            emojiBtnPreplanning.interactable = status;
			leaveGameBtn.interactable = status;
            leaveGameBtnVs.interactable = status;
            leaveGameBtnPreplanning.interactable = status;
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
			} else {
				PhotonNetwork.LoadLevel (level);
			}
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

        private IEnumerator StartPreplanningCountdown()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            titleController.GetComponent<AudioSource>().clip = countdownSfx;
            titleController.GetComponent<AudioSource>().Play();
            chatPreplanning.sendChatOfMaster("Game starting in 5");
            yield return new WaitForSeconds(1f);
            titleController.GetComponent<AudioSource>().Play();
            chatPreplanning.sendChatOfMaster("Game starting in 4");
            yield return new WaitForSeconds(1f);
            titleController.GetComponent<AudioSource>().Play();
            chatPreplanning.sendChatOfMaster("Game starting in 3");
            yield return new WaitForSeconds(1f);
            titleController.GetComponent<AudioSource>().Play();
            chatPreplanning.sendChatOfMaster("Game starting in 2");
            yield return new WaitForSeconds(1f);
            titleController.GetComponent<AudioSource>().Play();
            chatPreplanning.sendChatOfMaster("Game starting in 1");
            yield return new WaitForSeconds(1f);

            pView.RPC("RpcLoadingScreen", RpcTarget.All);
            if (PhotonNetwork.IsMasterClient)
            {
                StartGame(mapNames[mapIndex]);
            }
        }

        private IEnumerator StartVersusGameCountdown() {
			chatVs.sendChatOfMaster("The match is starting. Sending teams to preplanning...");
			yield return new WaitForSeconds(5f);
			// Send teams to preplanning if there are still members on both sides
			if (redTeam.Count >= 1 && blueTeam.Count >= 1) {
                pView.RPC("RpcSendTeamsToPreplanning", RpcTarget.All);
			} else {
				chatVs.sendChatOfMaster("Match could not start because there were not enough players on both teams.");
				pView.RPC ("RpcToggleButtons", RpcTarget.All, true, true);
			}
		}

        [PunRPC]
        private void RpcSendTeamsToPreplanning() {
            char myTeam = myPlayerListEntry.GetComponent<PlayerEntryScript>().team;
            if (myTeam == 'R') {
                SendRedTeamToPreplanning();
            } else if (myTeam == 'B') {
                SendBlueTeamToPreplanning();
            }
        }

		void SendRedTeamToPreplanning() {
            
            // Get the unique room ID to match the team rooms together in DB
            string roomId = PhotonNetwork.CurrentRoom.Name;
            string redTeamCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"];
            // Enable loading panel
            templateUIClassVs.LoadingPanel.SetActive(true);
            // Leave the current room
            PhotonNetwork.LeaveRoom();
            // Enable preplanning panel
            //templateUIClassVs.RoomPanel.SetActive(false);
            // Leave current room and join preplanning room for all players on red team
            if (PhotonNetwork.LocalPlayer.NickName == redTeamCaptain)
            {
                // Start the preplanning room if you're master client, else wait for it to be created and then join
                // connexion.CreateVersusPreplanningRoom(roomId, "red");
                connexion.SetCreatePreplanningRoomValues(roomId, "red");
            } else
            {
                connexion.SetJoinPreplanningRoomValues(roomId, "red");
                // StartCoroutine(TryToJoinPreplanning("red", roomId));
            }
		}

		void SendBlueTeamToPreplanning() {
            // Get the unique room ID to match the team rooms together in DB
            string roomId = PhotonNetwork.CurrentRoom.Name;
            string blueTeamCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"];
            // Enable loading panel
            templateUIClassVs.LoadingPanel.SetActive(true);
            // Leave the current room
            PhotonNetwork.LeaveRoom();
            // Enable preplanning panel
            //templateUIClassVs.RoomPanel.SetActive(false);
            // Leave current room and join preplanning room for all players on red team
            if (PhotonNetwork.LocalPlayer.NickName == blueTeamCaptain)
            {
                // Start the preplanning room if you're master client, else wait for it to be created and then join
                // connexion.CreateVersusPreplanningRoom(roomId, "blue");
                connexion.SetCreatePreplanningRoomValues(roomId, "blue");
            }
            else
            {
                connexion.SetJoinPreplanningRoomValues(roomId, "blue");
                // StartCoroutine(TryToJoinPreplanning("blue", roomId));
            }
        }

		[PunRPC]
		void RpcLoadingScreen() {
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
                readyButtonPreplanningTxt.text = "START";
            } else {
                readyButtonTxt.text = "READY";
                readyButtonVsTxt.text = "READY";
                readyButtonPreplanningTxt.text = "READY";
            }

		}

		void SetMapInfo() {
            Texture mapTexture = (Texture)Resources.Load(mapStrings[mapIndex]);
            mapPreviewThumb.texture = mapTexture;
			mapPreviewTxt.text = mapNames [mapIndex];
            mapPreviewVsThumb.texture = mapTexture;
            mapPreviewVsTxt.text = mapNames[mapIndex];
            mapPreviewPreplanningThumb.texture = mapTexture;
            mapPreviewPreplanningTxt.text = mapNames[mapIndex];
		}

		public void goToNextMap() {
			mapIndex++;
			if (mapIndex >= mapNames.Length) {
				mapIndex = 0;
			}
			SetMapInfo ();
		}

		public void goToPreviousMap() {
			mapIndex--;
			if (mapIndex < 0) {
				mapIndex = mapNames.Length - 1;
			}
			SetMapInfo ();
		}

		public override void OnJoinedRoom()
		{
            // Disable any loading screens
            templateUIClass.LoadingPanel.SetActive(false);
            templateUIClassVs.LoadingPanel.SetActive(false);
			currentMode = (!templateUIClassVs.gameObject.activeInHierarchy ? 'C' : (string.IsNullOrEmpty((string)PhotonNetwork.CurrentRoom.CustomProperties["versusId"]) ? 'V' : 'P'));
			if (currentMode == 'V') {
				OnJoinedRoomVersus();
			} else if (currentMode == 'P') {
				OnJoinedRoomPreplanning();
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
					myPlayerListEntry = entry;
				}
				if (p.IsMasterClient) {
					entryScript.ToggleReadyIndicator(false);
				}
				entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
				entryScript.SetNameTag(p.NickName);
			
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
					myPlayerListEntry = entry;
				}
				if (p.IsMasterClient) {
					entryScript.ToggleReadyIndicator(false);
				}
				entry.transform.SetParent(InsideRoomPanelVs[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
				entryScript.SetNameTag(p.NickName);
                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // If it's me, set team captain as me if possible and set my team
                    if (redTeam.Count <= blueTeam.Count)
                    {
                        entryScript.SetTeam('R');
                        redTeam.Add(p.ActorNumber);
                        SetTeamCaptain('R');
                        myTeamVsTxt.text = "RED TEAM";
                        myTeamPreplanningTxt.text = "RED TEAM";
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
                        myTeamPreplanningTxt.text = "BLUE TEAM";
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

        void OnJoinedRoomPreplanning()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Initialize initial delay before starting match
                preplanningSyncComplete = false;
                readyButtonPreplanning.GetComponent<Button>().interactable = false;
                preplanningSyncDelay = 1800;
            }

            preplanningVersusId = (string)PhotonNetwork.CurrentRoom.CustomProperties["versusId"];
            templateUIClassVs.ListRoomPanel.SetActive(false);
            templateUIClassVs.preplanningRoomPanel.SetActive(true);
            templateUIClassVs.TitleRoomPreplanning.text = preplanningVersusId;

            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            lastSlotUsed = 0;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                PlayerEntryScript entryScript = entry.GetComponent<PlayerEntryScript>();
                if (p.IsLocal)
                {
                    myPlayerListEntry = entry;
                }
                if (p.IsMasterClient)
                {
                    entryScript.ToggleReadyIndicator(false);
                }
                entry.transform.SetParent(InsideRoomPanelPreplanning[lastSlotUsed++].transform);
                entry.transform.localPosition = Vector3.zero;
                entryScript.SetNameTag(p.NickName);

                playerListEntries.Add(p.ActorNumber, entry);
            }
            chatPreplanning.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
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
                myTeamPreplanningTxt.text = "RED TEAM";
                PhotonNetwork.LocalPlayer.CustomProperties["team"] = "red";
            } else if (newTeam == 'B')
            {
                redTeam.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                blueTeam.Add(PhotonNetwork.LocalPlayer.ActorNumber);
                myTeamVsTxt.text = "BLUE TEAM";
                myTeamPreplanningTxt.text = "BLUE TEAM";
                PhotonNetwork.LocalPlayer.CustomProperties["team"] = "blue";
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
            string myNickname = PhotonNetwork.LocalPlayer.NickName;
            if (team == 'R')
            {
                string currentRedCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"];
                // Add yourself as a captain if there isn't one
                if (string.IsNullOrEmpty(currentRedCaptain))
                {
                    PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"] = myNickname;
                    // Remove yourself from captain of blue team if you were captain and set the next available person if there is one
                    string currentBlueCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"];
                    if (!string.IsNullOrEmpty(currentBlueCaptain) && currentBlueCaptain == myNickname)
                    {
                        if (blueTeam.Count > 0)
                        {
                            string nextBlueCaptainName = playerListEntries[(int)blueTeam[0]].GetComponent<PlayerEntryScript>().nickname;
                            PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"] = nextBlueCaptainName;
                        } else
                        {
                            PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"] = null;
                        }
                    }
                }
            } else
            {
                string currentBlueCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"];
                // Add yourself as a captain if there isn't one
                if (string.IsNullOrEmpty(currentBlueCaptain))
                {
                    PhotonNetwork.CurrentRoom.CustomProperties["blueCaptain"] = myNickname;
                    // Remove yourself from captain of red team if you were captain and set the next available person if there is one
                    string currentRedCaptain = (string)PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"];
                    if (!string.IsNullOrEmpty(currentRedCaptain) && currentRedCaptain == myNickname)
                    {
                        if (redTeam.Count > 0)
                        {
                            string nextRedCaptainName = playerListEntries[(int)redTeam[0]].GetComponent<PlayerEntryScript>().nickname;
                            PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"] = nextRedCaptainName;
                        }
                        else
                        {
                            PhotonNetwork.CurrentRoom.CustomProperties["redCaptain"] = null;
                        }
                    }
                }
            }
        }

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
            string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
			GameObject entry = Instantiate(PlayerListEntryPrefab);
            if (gameMode == "versus")
            {
                if (InsideRoomPanelVs[0].activeInHierarchy)
                {
                    entry.transform.SetParent(InsideRoomPanelVs[lastSlotUsed++].transform);
                } else if (InsideRoomPanelPreplanning[0].activeInHierarchy)
                {
                    entry.transform.SetParent(InsideRoomPanelPreplanning[lastSlotUsed++].transform);
                }
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
		}

		public override void OnLeftRoom()
		{
            preplanningVersusId = null;
			templateUIClass.RoomPanel.SetActive(false);
            templateUIClassVs.RoomPanel.SetActive(false);
            templateUIClassVs.preplanningRoomPanel.SetActive(false);
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
            templateUIClassVs.ChatTextPreplanning.text = "";
            ToggleButtons(true);
			PhotonNetwork.JoinLobby();
		}

		void RearrangePlayerSlots() {
			lastSlotUsed = 0;
			foreach (GameObject entry in playerListEntries.Values) {
				entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
			}
		}

	}
}
