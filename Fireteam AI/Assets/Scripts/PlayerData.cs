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
	public string playername;
	public bool disconnectedFromServer;
	public string disconnectReason;
	public bool testMode;

	public GameObject bodyReference;
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
			if (PlayerData.playerdata.inGamePlayerReference == null) {
				PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate (
					"PlayerPho",
					Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints [0],
					Quaternion.identity, 0);
			}
		} else {
			if (PlayerData.playerdata.inGamePlayerReference != null) {
				PhotonNetwork.Destroy (PlayerData.playerdata.inGamePlayerReference);
			}
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

		PhotonNetwork.NickName = playername;
	}

	public void LoadPlayerData()
	{
		FindBodyRef();
		if (File.Exists (Application.persistentDataPath + "/playerData.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerData.dat", FileMode.Open);
			PlayerInfo info = (PlayerInfo)bf.Deserialize (file);
			file.Close ();
			playername = info.playername;
			EquipmentScript e = bodyReference.GetComponent<EquipmentScript>();
			e.EquipCharacter(info.equippedCharacter, null);
			e.EquipHeadgear(info.equippedHeadgear, null);
			e.EquipFacewear(info.equippedFacewear, null);
			e.EquipTop(info.equippedTop, TitleControllerScript.CheckSkinType(info.equippedTop), null);
			e.EquipBottom(info.equippedBottom, null);
			e.EquipFootwear(info.equippedFootwear, null);
			e.EquipArmor(info.equippedArmor, null);
		} else {
			// Else, load defaults
			playername = "Player";
		}
		PhotonNetwork.NickName = playername;
	}

	public void FindBodyRef() {
		if (bodyReference == null) {
			bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterPrefabs["Lucas"]));
		}
	}

	public void ChangeBodyRef(string character) {
		Destroy(bodyReference);
		bodyReference = null;
		bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterPrefabs[character]));
	}

}

[Serializable]
class PlayerInfo
{
	public string playername;
    public string equippedCharacter;
    public string equippedHeadgear;
    public string equippedFacewear;
    public string equippedTop;
    public string equippedBottom;
    public string equippedFootwear;
    public string equippedArmor;
}