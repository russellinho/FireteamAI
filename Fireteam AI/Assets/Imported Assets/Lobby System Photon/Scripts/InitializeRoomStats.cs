using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.LobbySystemPhoton;
using TMPro;

public class InitializeRoomStats : MonoBehaviour {

	public TextMeshProUGUI TitleRoom;
	public TextMeshProUGUI MapName;
	public TextMeshProUGUI PlayersInRoom;
	public RawImage MapImage;
	public Connexion conn;
	public TextMeshProUGUI GameMode;
	public TextMeshProUGUI Ping;

	private string TitleRoom2;

	// Use this for initialization
	public void Init (Connexion connexion, string NameRoom, int CountPlayer, int MaxPlayer, string mapName, string mapImage, char gameMode, int ping) {
		TitleRoom.text = NameRoom;
		TitleRoom2 = NameRoom;
		MapName.text = mapName;
		MapImage.texture = (Texture)Resources.Load(mapImage);
		if (gameMode == 'C') {
			GameMode.text = "CAMPAIGN";
		} else if (gameMode == 'V') {
			GameMode.text = "VERSUS";
		}
		PlayersInRoom.text = CountPlayer + "/" + MaxPlayer;
		Ping.text = ""+ping;
		conn = connexion;
	}
	
	public void ClickedJoinRoom()
	{
		conn.theJoinRoom("" + TitleRoom2);
	}
}
