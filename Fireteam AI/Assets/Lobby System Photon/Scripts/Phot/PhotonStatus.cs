using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.LobbySystemPhoton
{
	public class PhotonStatus : MonoBehaviour
	{
		private readonly string connectionStatusMessage = "    Connection Status: ";

		[Header("UI References")]
		public Text ConnectionStatusText;

		#region UNITY

		public void Update()
		{
			ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
		}

		#endregion
	}
}