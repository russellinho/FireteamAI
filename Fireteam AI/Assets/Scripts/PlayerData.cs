using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class PlayerData : MonoBehaviour
{

    public static PlayerData playerdata;
    public string playername;
    public bool disconnectedFromServer;
    public string disconnectReason;
    public bool testMode;
    public PlayerInfo info;

    public GameObject bodyReference;
    public GameObject inGamePlayerReference;

    void Awake()
    {
        if (playerdata == null)
        {
            DontDestroyOnLoad(gameObject);
            this.info = new PlayerInfo();
            playerdata = this;
            LoadPlayerData();
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }
        else if (playerdata != this)
        {
            Destroy(gameObject);
        }

    }

    public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName.Equals("BetaLevelNetwork"))
        {
            PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
                "PlayerPrefabLucasAction", 
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0], 
                Quaternion.Euler(Vector3.zero));
        }
        else
        {
            if (PlayerData.playerdata.inGamePlayerReference != null)
            {
                PhotonNetwork.Destroy(PlayerData.playerdata.inGamePlayerReference);
            }
        }

    }

    public void SavePlayerData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerData.dat");

        EquipmentScript myEquips = bodyReference.GetComponent<EquipmentScript>();
        TestWeaponScript myWeps = bodyReference.GetComponent<TestWeaponScript>();
        PlayerData.playerdata.info.playername = playername;
        PlayerData.playerdata.info.equippedCharacter = myEquips.equippedCharacter;
        PlayerData.playerdata.info.equippedHeadgear = myEquips.equippedHeadgear;
        PlayerData.playerdata.info.equippedFacewear = myEquips.equippedFacewear;
        PlayerData.playerdata.info.equippedTop = myEquips.equippedTop;
        PlayerData.playerdata.info.equippedBottom = myEquips.equippedBottom;
        PlayerData.playerdata.info.equippedFootwear = myEquips.equippedFootwear;
        PlayerData.playerdata.info.equippedArmor = myEquips.equippedArmor;
        PlayerData.playerdata.info.equippedPrimary = myWeps.equippedPrimaryWeapon;
        PlayerData.playerdata.info.equippedPrimaryType = myWeps.equippedPrimaryType;
        PlayerData.playerdata.info.equippedSecondary = myWeps.equippedSecondaryWeapon;
        PlayerData.playerdata.info.equippedSecondaryType = myWeps.equippedSecondaryType;
        bf.Serialize(file, info);
        file.Close();

        PhotonNetwork.NickName = playername;
    }

    public void LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerData.dat", FileMode.Open);
            info = (PlayerInfo)bf.Deserialize(file);
            file.Close();
            FindBodyRef(info.equippedCharacter);
            playername = info.playername;
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            TestWeaponScript characterWeps = bodyReference.GetComponent<TestWeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterEquips.EquipCharacter(info.equippedCharacter, null);
            characterEquips.EquipHeadgear(info.equippedHeadgear, null);
            characterEquips.EquipFacewear(info.equippedFacewear, null);
            characterEquips.EquipTop(info.equippedTop, null);
            characterEquips.EquipBottom(info.equippedBottom, null);
            characterEquips.EquipFootwear(info.equippedFootwear, null);
            characterEquips.EquipArmor(info.equippedArmor, null);
            characterWeps.EquipWeapon(info.equippedPrimaryType, info.equippedPrimary, null);
            characterWeps.EquipWeapon(info.equippedSecondaryType, info.equippedSecondary, null);
        }
        else
        {
            // Else, load defaults
            FindBodyRef("Lucas");
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            TestWeaponScript characterWeps = bodyReference.GetComponent<TestWeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            playername = "Player";
            characterEquips.EquipCharacter("Lucas", null);
            characterWeps.EquipDefaultWeapons();
        }
        PhotonNetwork.NickName = playername;
    }

    public void FindBodyRef(string character)
    {
        if (bodyReference == null)
        {
            bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterCatalog[character].prefabPath));
        }
        else
        {
            bodyReference = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void ChangeBodyRef(string character, GameObject shopItem)
    {
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
public class PlayerInfo
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