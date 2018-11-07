using UnityEngine;
using UnityEngine.UI;

namespace UITemplate
{
	public class Template : MonoBehaviour	{

		[Header("Input Panel")]
		public GameObject InputPanel;
		public InputField PlayerNameInput;

		[Header("Login Panel")]
		public GameObject LoginPanel;

		[Header("ListRoom Panel")]
		public GameObject ListRoomPanel;
		public GameObject ListRoomEmpty;
		public Text NbrPlayers;
		public Button BtnCreatRoom;

		[Header("Loading Panel")]
		public GameObject LoadingPanel;

		[Header("Room Panel")]
		public GameObject RoomPanel;
		public Text TitleRoom;

		[Header("Chat Panel")]
		public Text ChatText;
	}
}
