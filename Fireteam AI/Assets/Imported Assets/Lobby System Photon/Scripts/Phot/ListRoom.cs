using System;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UITemplate;

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListRoom : MonoBehaviourPunCallbacks
	{
		private Dictionary<string, RoomInfo> cachedRoomList;
		private Dictionary<string, GameObject> roomListEntries;
		public ListPlayer listPlayer;
		public Template templateUIClass;
		public Template templateUIClassVs;

		[Header("Room List Panel")]
		public GameObject RoomListContent;
		public GameObject RoomListContentVs;
		public GameObject RoomListEntryPrefab;

		public void Awake()
		{
			cachedRoomList = new Dictionary<string, RoomInfo>();
			roomListEntries = new Dictionary<string, GameObject>();
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

		private void UpdateCachedRoomList(List<RoomInfo> roomList)
		{
			char gameMode = (templateUIClass.gameObject.activeInHierarchy ? 'C' : 'V');
			foreach (RoomInfo info in roomList)
			{
				// Remove room from cached room list if it got closed, became invisible or was marked as removed
				// If we are in the versus lobby, don't load campaign matches and vice versa
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList || 
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
				GameObject entry = Instantiate(RoomListEntryPrefab);
				char gameMode = ' ';
                if ((string)info.CustomProperties["gameMode"] == "camp")
                {
                    entry.transform.SetParent(RoomListContent.transform, false);
					gameMode = 'C';
                } else if ((string)info.CustomProperties["gameMode"] == "versus")
                {
                    entry.transform.SetParent(RoomListContentVs.transform, false);
					gameMode = 'V';
                }
				entry.transform.localScale = Vector3.one;
				string mapName = (string)info.CustomProperties["mapName"];
				int ping = Convert.ToInt32(info.CustomProperties["ping"]);
				entry.GetComponent<InitializeRoomStats>().Init(listPlayer.connexion, info.Name, (byte)info.PlayerCount, info.MaxPlayers, mapName, listPlayer.GetMapImageFromMapName(mapName), gameMode, ping);

				roomListEntries.Add(info.Name, entry);
			}

		}
	}
}
