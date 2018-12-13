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

		private PhotonView photonView;

		[Header("Inside Room Panel")]
		public GameObject[] InsideRoomPanel;
		private int lastSlotUsed;

		public Template templateUIClass;
		public GameObject PlayerListEntryPrefab;
		public Dictionary<int, GameObject> playerListEntries;
		public TChat chat;
		public GameObject readyButton;
		public GameObject mapPreview;
		public Button mapNext;
		public Button mapPrev;
		public Button sendMsgBtn;
		public Button emojiBtn;
		public Button leaveGameBtn;
		public GameObject titleController;
		public AudioClip countdownSfx;

		// Map options
		private int mapIndex = 0;
		private string[] mapNames = new string[1]{"Citadel"};
		private string[] mapStrings = new string[1]{"MapImages/citadel"};
		public static Vector3[] mapSpawnPoints = new Vector3[]{ new Vector3(-27f,0.4f,-27f) };

		// Ready status
		private GameObject myPlayerListEntry;
		private bool isReady = false;
		private bool gameStarting = false;

		void Awake() {
			photonView = GetComponent<PhotonView> ();
		}

		void Start() {
			SetMapInfo ();
		}

		public void DisplayPopup(string message) {
			ToggleButtons (false);
			templateUIClass.popup.GetComponentsInChildren<Text> () [0].text = message;
			templateUIClass.popup.SetActive (true);
		}

		public void StartGameBtn() {
			// If we're the host, start the game assuming there are at least two ready players
			if (PhotonNetwork.IsMasterClient) {
				// Testing - comment in release
				if (PlayerData.playerdata.testMode == true) {
					gameStarting = true;
					photonView.RPC ("RpcToggleButtons", RpcTarget.All, false);
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
					gameStarting = true;
					ToggleButtons (false);
					StartCoroutine ("StartGameCountdown");
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
		void RpcToggleButtons(bool status) {
			mapNext.interactable = status;
			mapPrev.interactable = status;
			readyButton.GetComponent<Button> ().interactable = status;
			sendMsgBtn.interactable = status;
			emojiBtn.interactable = status;
			leaveGameBtn.interactable = status;
		}

		void ChangeReadyStatus() {
			isReady = !isReady;
			photonView.RPC ("RpcChangeReadyStatus", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isReady);
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
			if (level.Equals ("Citadel")) {
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
			titleController.GetComponent<TitleControllerScript> ().InstantiateLoadingScreen (mapNames[mapIndex]);
			if (PhotonNetwork.IsMasterClient) {
				StartGame (mapNames [mapIndex]);
			}
		}

		void Update() {
			if (!templateUIClass.popup.activeInHierarchy) {
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
			templateUIClass.ListRoomPanel.SetActive(false);
			templateUIClass.RoomPanel.SetActive(true);

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

			lastSlotUsed = 0;
			foreach (Player p in PhotonNetwork.PlayerList)
			{
				GameObject entry = Instantiate(PlayerListEntryPrefab);
				if (p.IsLocal) {
					myPlayerListEntry = entry;
				}
				if (p.IsMasterClient) {
					entry.GetComponentInChildren<Text> ().gameObject.SetActive (false);
				}
				entry.transform.SetParent(InsideRoomPanel[lastSlotUsed++].transform);
				entry.transform.localPosition = Vector3.zero;
				entry.GetComponent<TMP_Text>().text = p.NickName;
				playerListEntries.Add(p.ActorNumber, entry);
			}
			templateUIClass.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;
            chat.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
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
