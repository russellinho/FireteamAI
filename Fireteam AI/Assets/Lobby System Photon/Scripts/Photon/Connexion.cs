using Photon.Realtime;
using UnityEngine;
using UITemplate;
using System.Collections;
using UnityEngine.UI;

namespace Photon.Pun.LobbySystemPhoton
{
	public class Connexion : MonoBehaviourPunCallbacks
	{
		public Template templateUIClass;
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

		public override void OnConnectedToMaster()
		{
			templateUIClass.LoadingPanel.SetActive(false);
			templateUIClass.ListRoomPanel.SetActive(true);
			if (!PhotonNetwork.InLobby) {
				PhotonNetwork.JoinLobby ();
			}
			nbrPlayersInLobby = PhotonNetwork.CountOfPlayers;
			templateUIClass.NbrPlayers.text = nbrPlayersInLobby.ToString("00");
		}

		public void OnCreateCampaignRoomButtonClicked()
		{
			templateUIClass.BtnCreatRoom.interactable = false;
			templateUIClass.ExitMatchmakingBtn.interactable = false;
			string roomName = "Table_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 8 };
			options.CustomRoomProperties["gameMode"] = "camp";

			PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
		}

		public void OnCreateVersusRoomButtonClicked()
		{
			templateUIClass.BtnCreatRoom.interactable = false;
			templateUIClass.ExitMatchmakingBtn.interactable = false;
			string roomName = "Table_"+ Random.Range(1000, 10000);
			roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

			RoomOptions options = new RoomOptions { MaxPlayers = 16 };
			options.CustomRoomProperties["gameMode"] = "versus";

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
			}
			else
			{
				templateUIClass.ListRoomEmpty.SetActive(false);
			}
            StopCoroutine("AutoRefreshListRoom");
            StartCoroutine("AutoRefreshListRoom");
		}

		public void ClosePopup() {
			templateUIClass.popup.SetActive (false);
		}

		public void PopupMessage(string message) {
			templateUIClass.popup.GetComponentInChildren<Text> ().text = message;
			templateUIClass.popup.SetActive (true);
		}
	}
}
