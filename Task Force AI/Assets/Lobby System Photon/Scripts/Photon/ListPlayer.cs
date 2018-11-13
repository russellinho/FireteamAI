using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UITemplate;
using TMPro;

namespace Photon.Pun.LobbySystemPhoton
{
	public class ListPlayer : MonoBehaviourPunCallbacks
	{

		[Header("Inside Room Panel")]
		public GameObject InsideRoomPanel;

		public Template templateUIClass;
		public GameObject PlayerListEntryPrefab;
		public Dictionary<int, GameObject> playerListEntries;
		public TChat chat;

		public override void OnJoinedRoom()
		{
			if (PhotonNetwork.InRoom) {
				return;
			}
			templateUIClass.ListRoomPanel.SetActive(false);
			templateUIClass.RoomPanel.SetActive(true);

			if (playerListEntries == null)
			{
				playerListEntries = new Dictionary<int, GameObject>();
			}

			foreach (Player p in PhotonNetwork.PlayerList)
			{
				GameObject entry = Instantiate(PlayerListEntryPrefab);
				entry.transform.SetParent(InsideRoomPanel.transform);
				entry.GetComponent<TMP_Text>().text = p.NickName;
				playerListEntries.Add(p.ActorNumber, entry);
			}
			templateUIClass.TitleRoom.text = PhotonNetwork.CurrentRoom.Name;
            chat.SendMsgConnection(PhotonNetwork.LocalPlayer.NickName);
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			GameObject entry = Instantiate(PlayerListEntryPrefab);
			entry.transform.SetParent(InsideRoomPanel.transform);
			entry.transform.localScale = Vector3.one;
			entry.GetComponent<Text>().text = newPlayer.NickName;

			playerListEntries.Add(newPlayer.ActorNumber, entry);
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
			playerListEntries.Remove(otherPlayer.ActorNumber); 
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
	}
}
