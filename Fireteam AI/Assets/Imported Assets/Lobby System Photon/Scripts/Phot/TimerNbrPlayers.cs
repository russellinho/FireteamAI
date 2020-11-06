using UnityEngine;
using UITemplate;

namespace Photon.Pun.LobbySystemPhoton
{
	public class TimerNbrPlayers : MonoBehaviourPunCallbacks
	{
		public Template templateUIClass;
		public float time;

		// Use this for initialization

		// Update is called once per frame
		// void Update()
		// {
		// 	if (PhotonNetwork.InLobby)
		// 	{
		// 		time -= Time.deltaTime;
		// 		if (time <= 0f)
		// 		{
		// 			time = 5f;
		// 			templateUIClass.NbrPlayers.text = PhotonNetwork.CountOfPlayers.ToString("00");
		// 		}
		// 	}
		// }
	}
}