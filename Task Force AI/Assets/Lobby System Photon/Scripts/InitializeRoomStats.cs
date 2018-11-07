using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.LobbySystemPhoton;

public class InitializeRoomStats : MonoBehaviour {

	public Text TitleRoom;
	public Text Slogan;
	public PlayerInRoomIcons IconsPlayer;
	public Connexion conn;

	private string TitleRoom2;

	// Use this for initialization
	public void Init (string NameRoom, int CountPlayer, int MaxPlayer) {
		TitleRoom.text = NameRoom;
		TitleRoom2 = NameRoom;
		Slogan.text = "Player(s) in Room - " + CountPlayer.ToString("00") + "/" + MaxPlayer.ToString("00");
		IconsPlayer.InitIcon(CountPlayer);
		conn = GameObject.Find("ConnexionPhoton").GetComponent<Connexion>();
	}
	
	public void ClickedJoinRoom()
	{
		conn.theJoinRoom("" + TitleRoom2);	}
}
