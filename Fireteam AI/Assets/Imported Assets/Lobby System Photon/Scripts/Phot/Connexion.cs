using Photon.Realtime;
using UnityEngine;
using System;
using UITemplate;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Firebase.Database;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Photon.Pun.LobbySystemPhoton
{
	public class Connexion : MonoBehaviourPunCallbacks
	{
		public Template templateUIClass;
        public Template templateUIVersusClass;
		public ListPlayer listPlayer;
		public ListRoom listRoom;
		public TitleControllerScript titleController;

		public override void OnJoinedLobby()  
		{
			// ToggleLobbyLoadingScreen(false);
			templateUIClass.BtnCreatRoom.interactable = true;
			templateUIVersusClass.BtnCreatRoom.interactable = true;
			StartCoroutine("AutoRefreshListRoom");
		}

		public void OnRefreshButtonClicked()
		{
            StopCoroutine("AutoRefreshListRoom");
			PhotonNetwork.JoinLobby();
            StartCoroutine("AutoRefreshListRoom");
		}

		public override void OnConnectedToMaster()
		{
            PhotonNetwork.NickName = PlayerData.playerdata.info.Playername;
			templateUIClass.ListRoomPanel.SetActive(true);
			templateUIVersusClass.ListRoomPanel.SetActive(true);
            if (!PhotonNetwork.InLobby) {
				PhotonNetwork.JoinLobby ();
			}
        }

		public void OnCreateRoomButtonClicked()
		{
			titleController.TriggerCreateRoomPopup();
		}

		void CreateCampaignRoom(string roomName)
		{
			RoomOptions options = new RoomOptions { MaxPlayers = 8 };
            Hashtable h = new Hashtable();
            h.Add("gameMode", "camp");
			h.Add("inGame", 0);
			h.Add("mapName", listPlayer.mapSelector.GetCurrentItem());
			h.Add("ping", (int)PhotonNetwork.GetPing());
			h.Add("kickedPlayers", "");
			h.Add("joinMode", 0);
			h.Add("privacy", 0);
			string[] lobbyProperties = new string[7] {"gameMode", "mapName", "ping", "inGame", "kickedPlayers", "privacy", "password"};
            options.CustomRoomProperties = h;
			options.CustomRoomPropertiesForLobby = lobbyProperties;

            PhotonNetwork.CreateRoom(roomName, options, null);
		}

		void CreateVersusRoom(string roomName)
		{
			RoomOptions options = new RoomOptions { MaxPlayers = 16 };
            Hashtable h = new Hashtable();
            h.Add("gameMode", "versus");
			h.Add("inGame", 0);
			h.Add("mapName", listPlayer.mapSelectorVs.GetCurrentItem());
			h.Add("ping", (int)PhotonNetwork.GetPing());
			h.Add("redScore", 0);
			h.Add("blueScore", 0);
			h.Add("redHost", -1);
			h.Add("blueHost", -1);
			h.Add("kickedPlayers", "");
			h.Add("joinMode", 0);
			h.Add("privacy", 0);
            string[] lobbyProperties = new string[7] {"gameMode", "mapName", "ping", "inGame", "kickedPlayers", "privacy", "password"};
            options.CustomRoomProperties = h;
            options.CustomRoomPropertiesForLobby = lobbyProperties;

			PhotonNetwork.CreateRoom(roomName, options, null);
		}

		void OnCreateRoomFailed(short returnCode, string message)
		{
			templateUIClass.BtnCreatRoom.interactable = true;
			templateUIVersusClass.BtnCreatRoom.interactable = true;
			titleController.TriggerAlertPopup("A ROOM WITH THIS NAME ALREADY EXISTS. PLEASE CHOOSE ANOTHER.");
		}

		void OnCreatedRoom()
		{
			titleController.CloseCreateRoom();
		}
		
		public void OnLeaveGameButtonClicked()
		{
			PhotonNetwork.LeaveRoom();
		}
		
		public void theJoinRoom(string roomName)
		{
			// ToggleLobbyLoadingScreen(true);
			RoomInfo joiningRoomInfo = listRoom.cachedRoomList[roomName];
			string kickedPlayers = (string)joiningRoomInfo.CustomProperties["kickedPlayers"];
			string[] kickedPlayersList = kickedPlayers.Split(',');
			if (kickedPlayersList.Contains(PhotonNetwork.NickName)) {
				OnJoinRoomFailed(-1, "You've been kicked from this game.");
			} else {
				int thisRoomPrivacy = Convert.ToInt32(joiningRoomInfo.CustomProperties["privacy"]);
				if (thisRoomPrivacy == 0) {
					PhotonNetwork.JoinRoom(roomName);
				} else if (thisRoomPrivacy == 1) {
					titleController.TriggerRoomPasswordEnterPopup(roomName);
				}
			}
		}

		public void AttemptJoinRoom(string roomName, string passwordEntered)
		{
			string roomPassword = (string)listRoom.cachedRoomList[roomName].CustomProperties["password"];
			if (passwordEntered == roomPassword) {
				PhotonNetwork.JoinRoom(roomName);
				titleController.CloseRoomPasswordEnter();
			} else {
				titleController.TriggerAlertPopup("THIS PASSWORD IS INCORRECT. PLEASE TRY AGAIN.");
			}
		}

		public void AttemptCreateRoom(string roomName)
		{
			templateUIClass.BtnCreatRoom.interactable = false;
			templateUIVersusClass.BtnCreatRoom.interactable = false;
			Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
			if (roomName.Length == 0 || roomName.Length > 30 || regex.Matches(roomName).Count == 0) {
				titleController.TriggerAlertPopup("The room name must only consist of alphanumeric characters!");
				templateUIClass.BtnCreatRoom.interactable = true;
				templateUIVersusClass.BtnCreatRoom.interactable = true;
				return;
        	}
			if (templateUIClass.gameObject.activeInHierarchy) {
				CreateCampaignRoom(roomName);
			} else if (templateUIVersusClass.gameObject.activeInHierarchy) {
				CreateVersusRoom(roomName);
			}
		}

		public override void OnJoinRoomFailed(short returnCode, string message) {
			// ToggleLobbyLoadingScreen(false);
			titleController.TriggerAlertPopup("Unable to join room.\nReason: " + message + "\nCode: " + returnCode);
		}

		IEnumerator AutoRefreshListRoom()
		{
			yield return new WaitForSeconds(0.5f);
            StopCoroutine("AutoRefreshListRoom");
            StartCoroutine("AutoRefreshListRoom");
		}

		// public void ClosePopup() {
		// 	templateUIClass.popup.SetActive (false);
        //     templateUIVersusClass.popup.SetActive(false);
		// }

		// public void PopupMessage(string message) {
        //     if (templateUIClass.gameObject.activeInHierarchy)
        //     {
        //         templateUIClass.popup.GetComponentInChildren<Text>().text = message;
        //         templateUIClass.popup.SetActive(true);
        //     } else
        //     {
        //         templateUIVersusClass.popup.GetComponentInChildren<Text>().text = message;
        //         templateUIVersusClass.popup.SetActive(true);
        //     }
		// }

		// public void ToggleLobbyLoadingScreen(bool b) {
		// 	templateUIClass.LoadingPanel.SetActive(b);
		// 	templateUIVersusClass.LoadingPanel.SetActive(b);
		// }
	}
}
