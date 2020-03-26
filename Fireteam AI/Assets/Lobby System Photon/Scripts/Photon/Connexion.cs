using Photon.Realtime;
using UnityEngine;
using UITemplate;
using System.Collections;
using UnityEngine.UI;
using Firebase.Database;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Photon.Pun.LobbySystemPhoton
{
	public class Connexion : MonoBehaviourPunCallbacks
	{
		public Template templateUIClass;
        public Template templateUIVersusClass;
		public ListPlayer listPlayer;
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
			templateUIClass.ListRoomPanel.SetActive(true);
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
			string roomName = "Camp_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 8 };
            Hashtable h = new Hashtable();
            h.Add("gameMode", "camp");
			string[] lobbyProperties = new string[1] {"gameMode"};
            options.CustomRoomProperties = h;
			options.CustomRoomPropertiesForLobby = lobbyProperties;

            PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
		}

		public void OnCreateVersusRoomButtonClicked()
		{
			templateUIVersusClass.BtnCreatRoom.interactable = false;
			templateUIVersusClass.ExitMatchmakingBtn.interactable = false;
			string roomName = "Vs_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 16 };
            Hashtable h = new Hashtable();
            h.Add("gameMode", "versus");
			h.Add("redScore", 0);
			h.Add("blueScore", 0);
            string[] lobbyProperties = new string[1] {"gameMode"};
            options.CustomRoomProperties = h;
            options.CustomRoomPropertiesForLobby = lobbyProperties;

			PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
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
            templateUIVersusClass.popup.SetActive(false);
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
