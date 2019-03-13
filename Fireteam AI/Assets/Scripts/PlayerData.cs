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
		FileStream file = File.Create(Application.persistentDataPath + "/playerData.dat");

		PlayerInfo info = new PlayerInfo();
        EquipmentScript myEquips = bodyReference.GetComponent<EquipmentScript>();
        TestWeaponScript myWeps = bodyReference.GetComponent<TestWeaponScript>();
		info.playername = playername;
        info.equippedCharacter = myEquips.equippedCharacter;
        info.equippedHeadgear = myEquips.equippedHeadgear;
        info.equippedFacewear = myEquips.equippedFacewear;
        info.equippedTop = myEquips.equippedTop;
        info.equippedBottom = myEquips.equippedBottom;
        info.equippedFootwear = myEquips.equippedFootwear;
        info.equippedArmor = myEquips.equippedArmor;
        info.equippedPrimary = myWeps.equippedPrimaryWeapon;
        info.equippedPrimaryType = myWeps.equippedPrimaryType;
        info.equippedSecondary = myWeps.equippedSecondaryWeapon;
        info.equippedSecondaryType = myWeps.equippedSecondaryType;
        bf.Serialize(file, info);
		file.Close();

		PhotonNetwork.NickName = playername;
	}

	public void LoadPlayerData()
	{
		if (File.Exists (Application.persistentDataPath + "/playerData.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerData.dat", FileMode.Open);
			PlayerInfo info = (PlayerInfo)bf.Deserialize (file);
			file.Close ();
            FindBodyRef(info.equippedCharacter);
            playername = info.playername;
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            TestWeaponScript characterWeps = bodyReference.GetComponent<TestWeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterEquips.EquipCharacter(info.equippedCharacter, null);
            characterEquips.EquipHeadgear(info.equippedHeadgear, null);
            characterEquips.EquipFacewear(info.equippedFacewear, null);
			char gender = ((info.equippedCharacter.Equals("Jade") || info.equippedCharacter.Equals("Hana")) ? 'F' : 'M');
            characterEquips.EquipTop(info.equippedTop, null);
            characterEquips.EquipBottom(info.equippedBottom, null);
            characterEquips.EquipFootwear(info.equippedFootwear, null);
            characterEquips.EquipArmor(info.equippedArmor, null);
            characterWeps.EquipWeapon(info.equippedPrimaryType, info.equippedPrimary, null);
            characterWeps.EquipWeapon(info.equippedSecondaryType, info.equippedSecondary, null);
		} else {
            // Else, load defaults
            FindBodyRef("Lucas");
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            TestWeaponScript characterWeps = bodyReference.GetComponent<TestWeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            playername = "Player";
            characterEquips.EquipCharacter("Lucas", null);
		}
		PhotonNetwork.NickName = playername;
	}

	public void FindBodyRef(string character) {
		if (bodyReference == null) {
			bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterCatalog[character].prefabPath));
		} else {
            bodyReference = GameObject.FindGameObjectWithTag("Player");
        }
    }

	public void ChangeBodyRef(string character, GameObject shopItem) {
		Destroy(bodyReference);
		bodyReference = null;
		bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterCatalog[character].prefabPath));
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        TestWeaponScript characterWeps = bodyReference.GetComponent<TestWeaponScript>();
        characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        bodyReference.GetComponent<EquipmentScript>().HighlightItemPrefab(shopItem);
        characterEquips.EquipCharacter(character, null);
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
    public string equippedPrimary;
    public string equippedPrimaryType;
    public string equippedSecondary;
    public string equippedSecondaryType;
}