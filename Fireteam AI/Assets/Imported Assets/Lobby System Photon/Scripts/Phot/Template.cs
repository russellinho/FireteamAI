using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UITemplate
{
	public class Template : MonoBehaviour
	{

		//[Header("Input Panel")]
		//public GameObject InputPanel;
		//public InputField PlayerNameInput;

		//[Header("Login Panel")]
		//public GameObject LoginPanel;

		[Header("ListRoom Panel")]
		public GameObject ListRoomPanel;
		// public Text NbrPlayers;
		public Button BtnCreatRoom;
		// public Button ExitMatchmakingBtn;

		[Header("Loading Panel")]
		// public GameObject LoadingPanel;
		// public GameObject popup;

		[Header("Room Panel")]
		public GameObject RoomPanel;
		public TextMeshProUGUI TitleRoom;

		[Header("Chat Panel")]
		public TextMeshProUGUI ChatText;
	}
}
