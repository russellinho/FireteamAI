using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerData : MonoBehaviour {

	public static PlayerData playerdata;

	public string playername;

	void Awake () {
		if(playerdata == null)
		{
			DontDestroyOnLoad(gameObject);
			LoadPlayerData ();
			playerdata = this;
		}else if(playerdata != this){
			Destroy(gameObject);
		}

	}

	public void SavePlayerData()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

		PlayerInfo info = new PlayerInfo();
		info.playername = playername;
		bf.Serialize(file, info);
		file.Close();
	}
	public void LoadPlayerData()
	{
		if(File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			PlayerInfo info = (PlayerInfo) bf.Deserialize(file);
			file.Close();
			playername = info.playername;
		}
	}

}

[Serializable]
class PlayerInfo
{
	public string playername;
}