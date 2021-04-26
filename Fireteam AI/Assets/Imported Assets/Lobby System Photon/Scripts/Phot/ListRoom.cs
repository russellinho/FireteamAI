using System;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITemplate;
using Michsky.UI.Shift;
using TMPro;

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListRoom : MonoBehaviourPunCallbacks
	{
		public Dictionary<string, RoomInfo> cachedRoomList;
		private Dictionary<string, GameObject> roomListEntries;
		public ListPlayer listPlayer;
		public Template templateUIClass;
		public Template templateUIClassVs;

		[Header("Room List Panel")]
		public GameObject RoomListContent;
		public GameObject RoomListContentVs;
		public GameObject RoomListEntryPrefab;

		[Header("Lobby Players Panel")]
		public GameObject LobbyPlayerEntryPrefab;
		public GameObject LobbyPlayers;
		public GameObject LobbyPlayersVs;
		public TextMeshProUGUI PlayersOnlineCampaign;
		public TextMeshProUGUI PlayersOnlineVersus;
		private Dictionary<string, GameObject> lobbyPlayersList;

		[Header("Filters")]
		public TMP_InputField roomNameFilter;
		public TMP_InputField roomNameFilterVs;
		public SwitchManager freeSlotsFilter;
		public SwitchManager freeSlotsFilterVs;
		public SwitchManager badlands1Filter;
		public SwitchManager badlands1FilterVs;
		public SwitchManager badlands2Filter;
		public SwitchManager badlands2FilterVs;
		private bool filtersChanged;

		public void Awake()
		{
			cachedRoomList = new Dictionary<string, RoomInfo>();
			roomListEntries = new Dictionary<string, GameObject>();
			lobbyPlayersList = new Dictionary<string, GameObject>();
			StartCoroutine("DeleteStalePlayers");
			StartCoroutine("DetectFilterChange");
		}

		public override void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			if (templateUIClass.gameObject.activeInHierarchy) {
				PhotonNetwork.AutomaticallySyncScene = false;
			} else {
				PhotonNetwork.AutomaticallySyncScene = false;
			}
			ClearRoomListView();
			UpdateCachedRoomList(roomList);
			UpdateRoomListView();
		}

		void RefreshRoomListView()
		{
			ClearRoomListView();
			UpdateRoomListView();
		}

		bool ChatClientConnectedToChannel(string channel)
		{
			if (PlayerData.playerdata.globalChatClient != null && PlayerData.playerdata.globalChatClient.chatClient != null && PlayerData.playerdata.globalChatClient.chatClient.CanChat) {
				if (PlayerData.playerdata.globalChatClient.chatClient.CanChatInChannel(channel)) {
					return true;
				}
			}
			return false;
		}

		IEnumerator DeleteStalePlayers()
		{
			if (ChatClientConnectedToChannel("Campaign")) {
				PlayersOnlineCampaign.text = ""+PhotonNetwork.CountOfPlayersOnMaster;
				HandleStalePlayers("Campaign");
			} else if (ChatClientConnectedToChannel("Versus")) {
				PlayersOnlineVersus.text = ""+PhotonNetwork.CountOfPlayersOnMaster;
				HandleStalePlayers("Versus");
			}
			yield return new WaitForSeconds(8f);
			StartCoroutine("DeleteStalePlayers");
		}

		IEnumerator DetectFilterChange()
		{
			if (filtersChanged) {
				RefreshRoomListView();
				filtersChanged = false;
			}
			yield return new WaitForSeconds(1.5f);
			StopCoroutine("DetectFilterChange");
			StartCoroutine("DetectFilterChange");
		}

		void HandleStalePlayers(string connectedChannel)
		{
			// Iterate back through the current lobby players list and delete any entries that are no longer subscribed
			HashSet<string> playersGone = new HashSet<string>(lobbyPlayersList.Keys);
			playersGone.ExceptWith(PlayerData.playerdata.globalChatClient.chatClient.PublicChannels[connectedChannel].Subscribers);
			HashSet<string>.Enumerator em = playersGone.GetEnumerator();
			while (em.MoveNext()) {
				RemovePlayerListEntry(em.Current);
			}
		}

		public void AddPlayerListEntry(string playername, uint exp, char mode)
		{
			if (!lobbyPlayersList.ContainsKey(playername)) {
				GameObject o = GameObject.Instantiate(LobbyPlayerEntryPrefab, (mode == 'C' ? LobbyPlayers.transform : LobbyPlayersVs.transform));
				LobbyPlayerScript ls = o.GetComponent<LobbyPlayerScript>();
				ls.InitEntry(playername, exp);
				lobbyPlayersList.Add(playername, o);
			}
		}

		void RemovePlayerListEntry(string playername)
		{
			try {
				Destroy(lobbyPlayersList[playername]);
				lobbyPlayersList.Remove(playername);
			} catch (Exception e) {
				Debug.LogError("Tried to remove [" + playername + "], but was not in the dictionary.");
			}
		}

		public override void OnLeftLobby()
		{
			cachedRoomList.Clear();

			ClearRoomListView();
		}

		private void ClearRoomListView()
		{
			foreach (GameObject entry in roomListEntries.Values)
			{
				Destroy(entry.gameObject);
			}

			roomListEntries.Clear();
		}

		public void SetFiltersChanged()
		{
			filtersChanged = true;
		}

		private void UpdateCachedRoomList(List<RoomInfo> roomList)
		{
			char gameMode = (templateUIClass.gameObject.activeInHierarchy ? 'C' : 'V');
			foreach (RoomInfo info in roomList)
			{
				// Remove room from cached room list if it got closed, became invisible or was marked as removed
				// If we are in the versus lobby, don't load campaign matches and vice versa
                if (!info.IsVisible || info.RemovedFromList || 
					((string)info.CustomProperties["gameMode"] == "camp" && gameMode == 'V') ||
					((string)info.CustomProperties["gameMode"] == "versus" && gameMode == 'C'))
				{
					if (cachedRoomList.ContainsKey(info.Name))
					{
						cachedRoomList.Remove(info.Name);
					}

					continue;
				}

				// Update cached room info
				if (cachedRoomList.ContainsKey(info.Name))
				{
					cachedRoomList[info.Name] = info;
				}
				// Add new room info to cache
				else
				{
					cachedRoomList.Add(info.Name, info);
				}
			}
		}
		private void UpdateRoomListView()
		{
			foreach (RoomInfo info in cachedRoomList.Values)
			{
				string mapName = (string)info.CustomProperties["mapName"];
				string roomName = info.Name;
				int currPlayers = (byte)info.PlayerCount;
				int maxPlayers = info.MaxPlayers;
				// Handle
				char gameMode = ' ';
                if ((string)info.CustomProperties["gameMode"] == "camp")
                {
					gameMode = 'C';
					// Handle free slots filter
					if (freeSlotsFilter.isOn && currPlayers >= maxPlayers) {
						continue;
					}
					bool goodMap = false;
					// Handle map filters
					if (!goodMap && badlands1Filter.isOn && mapName == "The Badlands: Act I") {
						goodMap = true;
					}
					if (!goodMap && badlands2Filter.isOn && mapName == "The Badlands: Act II") {
						goodMap = true;
					}
					if (!goodMap) {
						continue;
					}
					// Handle name filter
					if (roomNameFilter.text.Length > 0 && !roomName.Contains(roomNameFilter.text)) {
						continue;
					}
                } else if ((string)info.CustomProperties["gameMode"] == "versus")
                {
					gameMode = 'V';
					// Handle free slots filter
					if (freeSlotsFilterVs.isOn && currPlayers >= maxPlayers) {
						continue;
					}
					bool goodMap = false;
					// Handle map filters
					if (!goodMap && badlands1FilterVs.isOn && mapName == "The Badlands: Act I") {
						goodMap = true;
					}
					if (!goodMap && badlands2FilterVs.isOn && mapName == "The Badlands: Act II") {
						goodMap = true;
					}
					if (!goodMap) {
						continue;
					}
					// Handle name filter
					if (roomNameFilterVs.text.Length > 0 && !roomName.Contains(roomNameFilterVs.text)) {
						continue;
					}
                }
				GameObject entry = Instantiate(RoomListEntryPrefab);
				if (gameMode == 'C') {
					entry.transform.SetParent(RoomListContent.transform, false);
				} else if (gameMode == 'V') {
					entry.transform.SetParent(RoomListContentVs.transform, false);
				}
				entry.transform.localScale = Vector3.one;
				int ping = Convert.ToInt32(info.CustomProperties["ping"]);
				entry.GetComponent<InitializeRoomStats>().Init(listPlayer.connexion, info.Name, (byte)info.PlayerCount, info.MaxPlayers, mapName, listPlayer.GetMapImageFromMapName(mapName), gameMode, ping);

				roomListEntries.Add(info.Name, entry);
			}

		}
	}
}
