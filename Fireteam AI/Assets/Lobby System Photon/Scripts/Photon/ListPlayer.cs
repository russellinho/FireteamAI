using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UITemplate;
using TMPro;

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListPlayer : MonoBehaviourPunCallbacks
	{

		private PhotonView pView;

		[Header("Inside Room Panel")]
		public GameObject[] InsideRoomPanel;
		public GameObject[] InsideRoomPanelVs;
		private int lastSlotUsed;

		public Template templateUIClass;
		public Template templateUIClassVs;
		public GameObject PlayerListEntryPrefab;
		public Dictionary<int, GameObject> playerListEntries;
		public TChat chat;
		public TChat chatVs;
		public GameObject readyButton;
		public GameObject readyButtonVs;
		public GameObject mapPreview;
		public GameObject mapPreviewVs;
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
		private string[] mapNames = new string[]{"Badlands: Act I"};
		private string[] mapStrings = new string[]{"MapImages/badlands1"};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(-2f,1f,1f)};

		// Ready status
		private GameObject myPlayerListEntry;
		private bool isReady = false;
		private bool gameStarting = false;

		// Versus mode state
		private ArrayList redTeam;
		private ArrayList blueTeam;

		void Start() {
			SetMapInfo ();
			pView = GetComponent<PhotonView> ();
			redTeam = new ArrayList();
			blueTeam = new ArrayList();
		}

		public void DisplayPopup(string message) {
			ToggleButtons (false);
			templateUIClass.popup.GetComponentsInChildren<Text> () [0].text = message;
			templateUIClass.popup.SetActive (true);
		}

		public void StartGameBtn() {
			if (templateUIClass.gameObject.activeInHierarchy) {
				StartGameCampaign();
			} else {
				StartGameVersus();
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
					if (redTeam.Count >= 1 && blueTeam.Count >= 1) {
						pView.RPC ("RpcToggleButtons", RpcTarget.All, false, true);
						StartCoroutine ("StartVersusGameCountdown");
						return;
					} else {
						// If there's only 1 player, they cannot start the game
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
			mapPrev.interactable = status;
			readyButton.GetComponent<Button> ().interactable = status;
			sendMsgBtn.interactable = status;
			emojiBtn.interactable = status;
			leaveGameBtn.interactable = status;
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
				playerListEntries [playerId].GetComponentInChildren<Image> ().color = Color.green;
			} else {
				playerListEntries [playerId].GetComponentInChildren<Image> ().color = Color.red;
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

		private IEnumerator StartVersusGameCountdown() {
			chatVs.sendChatOfMaster("The match is starting. Sending teams to preplanning...");
			yield return new WaitForSeconds(5f);
			// Send teams to preplanning if there are still members on both sides
			if (redTeam.Count >= 1 && blueTeam.Count >= 1) {
				SendRedTeamToPreplanning();
				SendBlueTeamToPreplanning();
			} else {
				chatVs.sendChatOfMaster("Match could not start because there were not enough players on both teams.");
				pView.RPC ("RpcToggleButtons", RpcTarget.All, true, true);
			}
		}

		private void SendRedTeamToPreplanning() {
			// Enable loading panel

			// Enable preplanning panel

			// Leave current room and join preplanning room for all players on red team
			// Ensure that the match ID is carried from the versus room into this one
		}

		private void SendBlueTeamToPreplanning() {

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
				readyButton.GetComponentInChildren<Text> ().text = "START";
			} else {
				readyButton.GetComponentInChildren<Text> ().text = "READY";
			}
		}

		void SetMapInfo() {
			mapPreview.GetComponent<RawImage> ().texture = (Texture) Resources.Load (mapStrings[mapIndex]);
			mapPreview.GetComponentInChildren<Text>().text = mapNames [mapIndex];
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
			bool isVersusMode = templateUIClassVs.gameObject.activeInHierarchy;
			if (isVersusMode) {
				OnJoinedRoomVersus();
			} else {
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
				// Select team if versus mode. Choose red by default and blue if red has more members
				if (redTeam.Count <= blueTeam.Count) {
					entryScript.SetTeam('R');
					redTeam.Add(p.ActorNumber);
				} else {
					entryScript.SetTeam('B');
					blueTeam.Add(p.ActorNumber);
				}
				playerListEntries.Add(p.ActorNumber, entry);
			}
            chatVs.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			GameObject entry = Instantiate(PlayerListEntryPrefab);
			entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
			entry.transform.localPosition = Vector3.zero;
			entry.transform.localScale = Vector3.one;
			entry.GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;

			playerListEntries.Add(newPlayer.ActorNumber, entry);
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
			playerListEntries.Remove(otherPlayer.ActorNumber);
			RearrangePlayerSlots ();
		}

		public override void OnLeftRoom()
		{
			templateUIClass.RoomPanel.SetActive(false);
			templateUIClass.ListRoomPanel.SetActive(true);

			foreach (GameObject entry in playerListEntries.Values)
			{
				Destroy(entry.gameObject);
			}
				
			playerListEntries.Clear();
			playerListEntries = null;
			templateUIClass.ChatText.text = "";
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
