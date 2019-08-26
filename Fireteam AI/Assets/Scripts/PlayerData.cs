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
        string characterPrefabName = "";
        if (PlayerData.playerdata.info.equippedCharacter.Equals("Lucas")) {
            characterPrefabName = "LucasGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Daryl")) {
            characterPrefabName = "DarylGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Codename Sayre")) {
            characterPrefabName = "SayreGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Hana")) {
            characterPrefabName = "HanaGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Jade")) {
            characterPrefabName = "JadeGamePrefab";
        }
        if (levelName.Equals("BetaLevelNetwork"))
        {
            PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
                characterPrefabName,
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0],
                Quaternion.Euler(Vector3.zero));
        } else if (levelName.Equals("Test")) {
            //Debug.Log(characterPrefabName);
            PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
                characterPrefabName,
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[1],
                Quaternion.Euler(Vector3.zero));
        }
        else
        {
            if (PlayerData.playerdata.inGamePlayerReference != null)
            {
                PhotonNetwork.Destroy(PlayerData.playerdata.inGamePlayerReference);
            }
            if (levelName.Equals("Title"))
            {
                if (PlayerData.playerdata.bodyReference == null)
                {
                    LoadPlayerData();
                }
            }
        }

    }

    public void SavePlayerData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerData.dat");

        EquipmentScript myEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript myWeps = bodyReference.GetComponent<WeaponScript>();
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
        PlayerData.playerdata.info.equippedSupport = myWeps.equippedSupportWeapon;
        PlayerData.playerdata.info.equippedSupportType = myWeps.equippedSupportType;
        bf.Serialize(file, info);
        file.Close();

        PhotonNetwork.NickName = playername;
    }

    public void LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.dat"))
        {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/playerData.dat", FileMode.Open);
                info = (PlayerInfo)bf.Deserialize(file);
                file.Close();
                if (info.equippedCharacter == null || info.equippedCharacter == "") {
                    info.equippedCharacter = "Lucas";
                }
                if (info.equippedPrimary == null || info.equippedPrimary == "") {
                    info.equippedPrimary = "AK-47";
                    info.equippedPrimaryType = "Assault Rifle";
                }
                if (info.equippedSecondary == null || info.equippedSecondary == "") {
                    info.equippedSecondary = "Glock23";
                    info.equippedSecondaryType = "Pistol";
                }
                if (info.equippedSupport == null || info.equippedSupport == "") {
                    info.equippedSupport = "M67 Frag";
                    info.equippedSupportType = "Explosive";
                }
                if (info.equippedTop == null || info.equippedTop == "") {
                    info.equippedTop = "Standard Fatigues Top";
                }
                if (info.equippedBottom == null || info.equippedBottom == "") {
                    info.equippedBottom = "Standard Fatigues Bottom";
                }
                if (info.equippedFootwear == null || info.equippedFootwear == "") {
                    info.equippedFootwear = "Standard Boots";
                }
            } catch (Exception e) {
                Debug.Log("Exception occurred/corrupted file while loading player data. Message: " + e.Message);
                info.equippedCharacter = "Lucas";
                info.equippedPrimary = "AK-47";
                info.equippedPrimaryType = "Assault Rifle";
                info.equippedSecondary = "Glock23";
                info.equippedSecondaryType = "Pistol";
                info.equippedSupport = "M67 Frag";
                info.equippedSupportType = "Explosive";
                info.equippedTop = "Standard Fatigues Top";
                info.equippedBottom = "Standard Fatigues Bottom";
                info.equippedFootwear = "Standard Boots";
            }
            FindBodyRef(info.equippedCharacter);
            playername = info.playername;
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterEquips.EquipCharacter(info.equippedCharacter, null);
            characterEquips.EquipHeadgear(info.equippedHeadgear, null);
            characterEquips.EquipFacewear(info.equippedFacewear, null);
            characterEquips.EquipTop(info.equippedTop, null);
            characterEquips.EquipBottom(info.equippedBottom, null);
            characterEquips.EquipFootwear(info.equippedFootwear, null);
            characterEquips.EquipArmor(info.equippedArmor, null);
            ModInfo primaryModInfo = LoadModDataForWeapon(info.equippedPrimary);
            ModInfo secondaryModInfo = LoadModDataForWeapon(info.equippedSecondary);
            characterWeps.EquipWeapon(info.equippedPrimaryType, info.equippedPrimary, primaryModInfo.equippedSuppressor, null);
            characterWeps.EquipWeapon(info.equippedSecondaryType, info.equippedSecondary, secondaryModInfo.equippedSuppressor, null);
            characterWeps.EquipWeapon(info.equippedSupportType, info.equippedSupport, null, null);
        }
        else
        {
            // Else, load defaults
            FindBodyRef("Lucas");
            EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
            WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
            characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            playername = "Player";
            characterEquips.EquipCharacter("Lucas", null);
            characterWeps.EquipDefaultWeapons();
            SavePlayerData();
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
        WeaponScript weaponScrpt = bodyReference.GetComponent<WeaponScript>();
        PlayerData.playerdata.info.equippedPrimary = weaponScrpt.equippedPrimaryWeapon;
        PlayerData.playerdata.info.equippedSecondary = weaponScrpt.equippedSecondaryWeapon;
        PlayerData.playerdata.info.equippedSupport = weaponScrpt.equippedSupportWeapon;
        PlayerData.playerdata.info.equippedPrimaryType = weaponScrpt.equippedPrimaryType;
        PlayerData.playerdata.info.equippedSecondaryType = weaponScrpt.equippedSecondaryType;
        PlayerData.playerdata.info.equippedSupportType = weaponScrpt.equippedSupportType;
        Destroy(bodyReference);
        bodyReference = null;
        bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.characterCatalog[character].prefabPath));
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        characterWeps.ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        bodyReference.GetComponent<EquipmentScript>().HighlightItemPrefab(shopItem);
        characterEquips.EquipCharacter(character, null);
    }

    public void SaveModDataForWeapon(string weaponName, string equippedSuppressor) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + weaponName + "_mods.dat");
        ModInfo newModInfo = new ModInfo();
        newModInfo.weaponName = weaponName;
        newModInfo.equippedSuppressor = equippedSuppressor;
        bf.Serialize(file, newModInfo);
        file.Close();
    }

    public ModInfo LoadModDataForWeapon(string weaponName) {
        ModInfo modInfo = null;
        if (File.Exists(Application.persistentDataPath + "/" + weaponName + "_mods.dat"))
        {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + weaponName + "_mods.dat", FileMode.Open);
                modInfo = (ModInfo)bf.Deserialize(file);
                file.Close();
                if (modInfo.equippedSuppressor == null) {
                    modInfo.equippedSuppressor = "";
                }
            } catch (Exception e) {
                Debug.Log("Exception occurred/corrupted file while loading mod data for " + weaponName + ". Message: " + e.Message);
                modInfo.equippedSuppressor = "";
            }
            return modInfo;
        }
        // Else, load defaults
        modInfo = new ModInfo();
        modInfo.weaponName = weaponName;
        modInfo.equippedSuppressor = "";
        SaveModDataForWeapon(weaponName, "");

        return modInfo;
    }

    public void SaveModInventoryData() {

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
    public string equippedSupport;
    public string equippedSupportType;
}

[Serializable]
public class ModInfo
{
    public string weaponName;
    public string equippedSuppressor;
}

[Serializable]
public class ModInventoryInfo
{
    public string modName;
    public int modCount;
    public string[] weaponsAttachedTo;
}
