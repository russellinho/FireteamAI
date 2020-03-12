﻿using Photon.Realtime;
using UnityEngine;
using UITemplate;
using System.Collections;
using UnityEngine.UI;

namespace Photon.Pun.LobbySystemPhoton
{
	public class Connexion : MonoBehaviourPunCallbacks
	{
		public Template templateUIClass;
        public Template templateUIVersusClass;
		private int nbrPlayersInLobby = 0;

		void Start()
		{
			nbrPlayersInLobby = PhotonNetwork.CountOfPlayers;
			//templateUIClass.PlayerNameInput.text = 
		}
			
		/**public void OnLoginButtonClicked()
		{

			string playerName = templateUIClass.PlayerNameInput.text;

			if (!playerName.Equals(""))
			{
				PhotonNetwork.LocalPlayer.NickName = playerName;
				PhotonNetwork.ConnectUsingSettings();

				templateUIClass.InputPanel.SetActive(false);
				templateUIClass.LoadingPanel.SetActive(true);
			}
			else
			{
				Debug.LogError("Player Name is invalid.");
			}
		}*/

		public override void OnJoinedLobby()  
		{
			templateUIClass.BtnCreatRoom.interactable = true;
			templateUIClass.ExitMatchmakingBtn.interactable = true;
            templateUIVersusClass.BtnCreatRoom.interactable = true;
            templateUIVersusClass.ExitMatchmakingBtn.interactable = true;
            StartCoroutine("AutoRefreshListRoom");
		}

		public void OnRefreshButtonClicked()
		{
            StopCoroutine("AutoRefreshListRoom");
			PhotonNetwork.JoinLobby();
			if (PhotonNetwork.CountOfRooms == 0)
			{
				templateUIClass.ListRoomEmpty.SetActive(true);
			}
			else
			{
				templateUIClass.ListRoomEmpty.SetActive(false);
			}
            StartCoroutine("AutoRefreshListRoom");
			nbrPlayersInLobby = PhotonNetwork.CountOfPlayers;
			templateUIClass.NbrPlayers.text = nbrPlayersInLobby.ToString("00");
		}

        public void OnVersusRefreshButtonClicked()
        {
            StopCoroutine("AutoRefreshListRoom");
            PhotonNetwork.JoinLobby();
            if (PhotonNetwork.CountOfRooms == 0)
            {
                templateUIVersusClass.ListRoomEmpty.SetActive(true);
            }
            else
            {
                templateUIVersusClass.ListRoomEmpty.SetActive(false);
            }
            StartCoroutine("AutoRefreshListRoom");
            nbrPlayersInLobby = PhotonNetwork.CountOfPlayers;
            templateUIVersusClass.NbrPlayers.text = nbrPlayersInLobby.ToString("00");
        }

		public override void OnConnectedToMaster()
		{
            PhotonNetwork.NickName = PlayerData.playerdata.info.playername;
			templateUIClass.LoadingPanel.SetActive(false);
			templateUIClass.ListRoomPanel.SetActive(true);
            templateUIVersusClass.LoadingPanel.SetActive(false);
            templateUIVersusClass.ListRoomPanel.SetActive(true);
            if (!PhotonNetwork.InLobby) {
				PhotonNetwork.JoinLobby ();
			}
			nbrPlayersInLobby = PhotonNetwork.CountOfPlayers;
			templateUIClass.NbrPlayers.text = nbrPlayersInLobby.ToString("00");
            templateUIVersusClass.NbrPlayers.text = nbrPlayersInLobby.ToString("00");
        }

		public void OnCreateCampaignRoomButtonClicked()
		{
			templateUIClass.BtnCreatRoom.interactable = false;
			templateUIClass.ExitMatchmakingBtn.interactable = false;
			string roomName = "Table_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 8 };
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("gameMode", "camp");
            //options.CustomRoomProperties.Add("gameMode", "camp");
            options.CustomRoomProperties = h;

            PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
		}

		public void OnCreateVersusRoomButtonClicked()
		{
			templateUIVersusClass.BtnCreatRoom.interactable = false;
			templateUIVersusClass.ExitMatchmakingBtn.interactable = false;
			string roomName = "Table_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 16 };
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("gameMode", "versus");
			//options.CustomRoomProperties.Add("gameMode", "versus");
            options.CustomRoomProperties = h;

			PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
		}

        public void CreateVersusPreplanningRoom(string versusId, string team)
        {
            templateUIVersusClass.BtnCreatRoom.interactable = false;
            templateUIVersusClass.ExitMatchmakingBtn.interactable = false;
            string roomName = "Table_" + Random.Range(1000, 10000);
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

            RoomOptions options = new RoomOptions { MaxPlayers = 8 };
            options.CustomRoomProperties.Add("versusId", versusId);
            options.CustomRoomProperties.Add("myTeam", team);
            options.IsVisible = false;

            PhotonNetwork.CreateRoom(roomName, options, null);
            templateUIClass.NbrPlayers.text = "00";

            // Save the information to DB so that the other players can join
            string newRoomId = PhotonNetwork.CurrentRoom.Name;
            DAOScript.dao.dbRef.Child("fteam_ai_matches").Child(versusId).Child(team).Child("roomId").SetValueAsync(newRoomId);
        }
		
		public void OnLeaveGameButtonClicked()
		{
			PhotonNetwork.LeaveRoom();
		}
		
		public void theJoinRoom(string roomName)
		{
			PhotonNetwork.JoinRoom(roomName);
		}

		public override void OnJoinRoomFailed(short returnCode, string message) {
			PopupMessage ("Unable to join room.\nReason: " + message + "\nCode: " + returnCode);
		}

		IEnumerator AutoRefreshListRoom()
		{
			yield return new WaitForSeconds(0.5f);
			if (PhotonNetwork.CountOfRooms == 0)
			{
				templateUIClass.ListRoomEmpty.SetActive(true);
                templateUIVersusClass.ListRoomEmpty.SetActive(true);
			}
			else
			{
				templateUIClass.ListRoomEmpty.SetActive(false);
                templateUIVersusClass.ListRoomEmpty.SetActive(false);
			}
            StopCoroutine("AutoRefreshListRoom");
            StartCoroutine("AutoRefreshListRoom");
		}

		public void ClosePopup() {
			templateUIClass.popup.SetActive (false);
		}

		public void PopupMessage(string message) {
            if (templateUIClass.gameObject.activeInHierarchy)
            {
                templateUIClass.popup.GetComponentInChildren<Text>().text = message;
                templateUIClass.popup.SetActive(true);
            } else
            {
                templateUIVersusClass.popup.GetComponentInChildren<Text>().text = message;
                templateUIVersusClass.popup.SetActive(true);
            }
		}
	}
}
