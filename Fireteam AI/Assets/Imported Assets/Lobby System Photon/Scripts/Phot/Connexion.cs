using Photon.Realtime;
using UnityEngine;
using System;
using UITemplate;
using System.Collections;
using System.Linq;
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

		public void OnCreateCampaignRoomButtonClicked()
		{
			// ToggleLobbyLoadingScreen(true);
			templateUIClass.BtnCreatRoom.interactable = false;
			string roomName = "Camp_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

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

		public void OnCreateVersusRoomButtonClicked()
		{
			// ToggleLobbyLoadingScreen(true);
			templateUIVersusClass.BtnCreatRoom.interactable = false;
			string roomName = "Vs_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

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
