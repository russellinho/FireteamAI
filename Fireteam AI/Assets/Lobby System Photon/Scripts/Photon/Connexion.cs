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
		private const short MAX_PREPLANNING_TIMEOUT_CNT = 8;

		public Template templateUIClass;
        public Template templateUIVersusClass;
		public ListPlayer listPlayer;
		private int nbrPlayersInLobby = 0;
		private short preplanningJoinTimeoutCount;
		private bool createPreplanningRoomFlag;
		private string createPreplanningRoomTeam;
		private string createPreplanningRoomId;

		private bool joinPreplanningRoomFlag;
		private string joinPreplanningRoomTeam;
		private string joinPreplanningRoomId;

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
			if (createPreplanningRoomFlag) {
				createPreplanningRoomFlag = false;
				CreateVersusPreplanningRoom(createPreplanningRoomId, createPreplanningRoomTeam);
			} else if (joinPreplanningRoomFlag) {
				preplanningJoinTimeoutCount = 0;
				joinPreplanningRoomFlag = false;
				StartCoroutine(TryToJoinPreplanning(joinPreplanningRoomTeam, joinPreplanningRoomId));
			} else {
				templateUIClass.BtnCreatRoom.interactable = true;
				templateUIClass.ExitMatchmakingBtn.interactable = true;
				templateUIVersusClass.BtnCreatRoom.interactable = true;
				templateUIVersusClass.ExitMatchmakingBtn.interactable = true;
				StartCoroutine("AutoRefreshListRoom");
			}
		}

		IEnumerator TryToJoinPreplanning(string team, string versusId)
        {
            yield return new WaitForSeconds(3f);
            DAOScript.dao.dbRef.Child("fteam_ai_matches").Child(versusId).Child(team).GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot snapshot = task.Result;
                string roomId = snapshot.Child("roomId").Value.ToString();
                if (string.IsNullOrEmpty(roomId))
                {
                    // Room is not established yet, so try again
                    preplanningJoinTimeoutCount++;
                    if (preplanningJoinTimeoutCount == MAX_PREPLANNING_TIMEOUT_CNT)
                    {
                        // Timeout, disable loading screens, popup of joining room timed out, and re-enable all buttons
                        templateUIVersusClass.LoadingPanel.SetActive(false); 
                        templateUIClass.LoadingPanel.SetActive(false);
                        listPlayer.DisplayPopup("Joining preplanning timed out.");
                    }
                    else
                    {
                        StartCoroutine(TryToJoinPreplanning(team, versusId));
                    }
                } else
                {
                    // Room was created, join it
                    PhotonNetwork.JoinRoom(roomId);
                    PhotonNetwork.CurrentRoom.CustomProperties.Add("versusId", versusId);
                    PhotonNetwork.CurrentRoom.CustomProperties.Add("myTeam", team);
                }
            });
        }

		public void SetCreatePreplanningRoomValues(string roomId, string team) {
			createPreplanningRoomFlag = true;
			createPreplanningRoomId = roomId;
			createPreplanningRoomTeam = team;
		}

		public void SetJoinPreplanningRoomValues(string roomId, string team) {
			joinPreplanningRoomFlag = true;
			joinPreplanningRoomId = roomId;
			joinPreplanningRoomTeam = team;
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
			if (createPreplanningRoomFlag || joinPreplanningRoomFlag) {
				templateUIClass.LoadingPanel.SetActive(true);
				templateUIVersusClass.LoadingPanel.SetActive(true);
			} else {
				templateUIClass.LoadingPanel.SetActive(false);
				templateUIVersusClass.LoadingPanel.SetActive(false);
			}
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
            string[] lobbyProperties = new string[1] {"gameMode"};
            options.CustomRoomProperties = h;
            options.CustomRoomPropertiesForLobby = lobbyProperties;

			PhotonNetwork.CreateRoom(roomName, options, null);
			templateUIClass.NbrPlayers.text = "00";
		}

        public void CreateVersusPreplanningRoom(string versusId, string team)
        {
            templateUIVersusClass.BtnCreatRoom.interactable = false;
            templateUIVersusClass.ExitMatchmakingBtn.interactable = false;
            string roomName = "Pre_" + Random.Range(1000, 10000);
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

            RoomOptions options = new RoomOptions { MaxPlayers = 8 };
			Hashtable h = new Hashtable();
            h.Add("versusId", versusId);
            h.Add("myTeam", team);
			options.CustomRoomProperties = h;
            options.IsVisible = false;

            PhotonNetwork.CreateRoom(roomName, options, null);
            templateUIClass.NbrPlayers.text = "00";

            // Save the information to DB so that the other players can join
            string newRoomId = roomName;
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
