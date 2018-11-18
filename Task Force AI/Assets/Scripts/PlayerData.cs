using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class PlayerData : MonoBehaviour {

	public static PlayerData playerdata;
	public Vector3 color;
	public string playername;
	public bool testMode;

	public MeshRenderer bodyColorReference;
	public GameObject inGamePlayerReference;

	void Awake () {
		if(playerdata == null)
		{
			DontDestroyOnLoad(gameObject);
			LoadPlayerData ();
			playerdata = this;
			SceneManager.sceneLoaded += OnSceneFinishedLoading;
		} else if(playerdata != this) {
			Destroy(gameObject);
		}

	}

	public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode) {
		if(!PhotonNetwork.InRoom) return;
		if (SceneManager.GetActiveScene ().name.Equals ("BetaLevelNetwork")) {
			PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
				"PlayerPho",
				Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0],
				Quaternion.identity, 0);	
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

		PhotonNetwork.NickName = playername;
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
		PhotonNetwork.NickName = playername;
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