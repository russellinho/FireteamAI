﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerData : MonoBehaviour {

	public static PlayerData playerdata;
	public Vector3 color;
	public string playername;

	public MeshRenderer bodyColorReference;

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
		info.r = color.x;
		info.g = color.y;
		info.b = color.z;
		bf.Serialize(file, info);
		file.Close();
	}

	public void LoadPlayerData()
	{
		if (File.Exists (Application.persistentDataPath + "/playerInfo.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			PlayerInfo info = (PlayerInfo)bf.Deserialize (file);
			file.Close ();
			playername = info.playername;
			color = new Vector3(info.r, info.g, info.b);
		} else {
			// Else, load defaults
			playername = "Player";
			color = new Vector3 (255, 255, 255);
		}
		UpdateBodyColor ();
	}

	public void UpdateBodyColor() {
		bodyColorReference.material.color = new Color (color.x / 255, color.y / 255, color.z / 255, 1.0f);
	}

}

[Serializable]
class PlayerInfo
{
	public string playername;
	public float r;
	public float g;
	public float b;
}