using UnityEngine;
using UnityEngine.UI;

public class PlayerInRoomIcons : MonoBehaviour
{
	public Image[] IconPlayer;
	public Sprite imgMember1;
	public Sprite imgMember2;

	public void InitIcon(int CountPlayer)
	{
		for (int i = 0; i < CountPlayer; i++)
		{
			IconPlayer[i].sprite = imgMember2;
		}
	}
}
