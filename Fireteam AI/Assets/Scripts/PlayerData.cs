﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using Firebase.Database;

public class PlayerData : MonoBehaviour
{

    public static PlayerData playerdata;
    public string playername;
    public bool disconnectedFromServer;
    public string disconnectReason;
    public bool testMode;
    public PlayerInfo info;
    public ModInfo primaryModInfo;
    public ModInfo secondaryModInfo;
    public ModInfo supportModInfo;

    public GameObject bodyReference;
    public GameObject inGamePlayerReference;
    public TitleControllerScript titleRef;
    public ArrayList myHeadgear;
    public ArrayList myTops;
    public ArrayList myBottoms;
    public ArrayList myFacewear;
    public ArrayList myFootwear;
    public ArrayList myArmor;
    public ArrayList myWeapons;
    public ArrayList myCharacters;
    public ArrayList myMods;

    void Awake()
    {
        if (playerdata == null)
        {
            DontDestroyOnLoad(gameObject);
            this.info = new PlayerInfo();
            this.primaryModInfo = new ModInfo();
            this.secondaryModInfo = new ModInfo();
            this.supportModInfo = new ModInfo();
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
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        // Check if the DB has equipped data for the player. If not, then set default char and equips.
        // If error occurs, show error message on splash and quit the application
        DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).GetValueAsync().ContinueWith(task => {
            DataSnapshot snapshot = task.Result;
            if (task.IsFaulted || task.IsCanceled) {
                titleRef.CloseGameOnError();
            } else {
                playername = snapshot.Child("username").Value.ToString();
                // Equip previously equipped if available. Else, equip defaults and save it
                if (snapshot.HasChild("equipment")) {
                    DataSnapshot equipSnapshot = snapshot.Child("equipment");
                    info.equippedCharacter = equipSnapshot.Child("equippedCharacter").Value.ToString();
                    info.equippedPrimary = equipSnapshot.Child("equippedPrimary").Value.ToString();
                    info.equippedSecondary = equipSnapshot.Child("equippedSecondary").Value.ToString();
                    info.equippedSupport = equipSnapshot.Child("equippedSupport").Value.ToString();
                    info.equippedTop = equipSnapshot.Child("equippedTop").Value.ToString();
                    info.equippedBottom = equipSnapshot.Child("equippedBottom").Value.ToString();
                    info.equippedFootwear = equipSnapshot.Child("equippedFootwear").Value.ToString();
                    info.equippedFacewear = equipSnapshot.Child("equippedFacewear").Value.ToString();
                    info.equippedHeadgear = equipSnapshot.Child("equippedHeadgear").Value.ToString();
                    info.equippedArmor = equipSnapshot.Child("equippedArmor").Value.ToString();

                    // TODO

                    DataSnapshot modSnapshot = snapshot.Child("primaryMods");
                    primaryModInfo.equippedSuppressor = modSnapshot.Child("equippedSuppressor").Value.ToString();
                    primaryModInfo.weaponName = modSnapshot.Child("weaponName").Value.ToString();

                    modSnapshot = snapshot.Child("secondaryMods");
                    secondaryModInfo.equippedSuppressor = modSnapshot.Child("equippedSuppressor").Value.ToString();
                    secondaryModInfo.weaponName = modSnapshot.Child("weaponName").Value.ToString();

                    modSnapshot = snapshot.Child("supportMods");
                    supportModInfo.equippedSuppressor = modSnapshot.Child("equippedSuppressor").Value.ToString();
                    supportModInfo.weaponName = modSnapshot.Child("weaponName").Value.ToString();
                } else {
                    info.equippedCharacter = snapshot.Child("defaultChar").Value.ToString();
                    char g = InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender;
                    info.equippedPrimary = "M4A1";
                    info.equippedSecondary = "Glock23";
                    info.equippedSupport = "M67 Frag";
                    info.equippedTop = "Standard Fatigues Top (" + g + ")";
                    info.equippedBottom = "Standard Fatigues Bottom (" + g + ")";
                    info.equippedFootwear = "Standard Boots (" + g + ")";
                    info.equippedFacewear = "";
                    info.equippedHeadgear = "";
                    info.equippedArmor = "";

                    primaryModInfo.equippedSuppressor = "";
                    primaryModInfo.weaponName = "";

                    secondaryModInfo.equippedSuppressor = "";
                    secondaryModInfo.weaponName = "";

                    supportModInfo.equippedSuppressor = "";
                    supportModInfo.weaponName = "";
                    SavePlayerData();
                }
            }
        });
        FindBodyRef(info.equippedCharacter);
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        characterEquips.EquipCharacter(info.equippedCharacter, null);
        characterEquips.EquipHeadgear(info.equippedHeadgear, null);
        characterEquips.EquipFacewear(info.equippedFacewear, null);
        characterEquips.EquipTop(info.equippedTop, null);
        characterEquips.EquipBottom(info.equippedBottom, null);
        characterEquips.EquipFootwear(info.equippedFootwear, null);
        characterEquips.EquipArmor(info.equippedArmor, null);
        characterWeps.EquipWeapon(info.equippedPrimaryType, info.equippedPrimary, primaryModInfo.equippedSuppressor, null);
        characterWeps.EquipWeapon(info.equippedSecondaryType, info.equippedSecondary, secondaryModInfo.equippedSuppressor, null);
        characterWeps.EquipWeapon(info.equippedSupportType, info.equippedSupport, supportModInfo.equippedSuppressor, null);
        PhotonNetwork.NickName = playername;
    }

    public void FindBodyRef(string character)
    {
        if (bodyReference == null)
        {
            bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[character].prefabPath));
        }
        else
        {
            bodyReference = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void ChangeBodyRef(string character, GameObject shopItem)
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        WeaponScript weaponScrpt = bodyReference.GetComponent<WeaponScript>();
        PlayerData.playerdata.info.equippedPrimary = weaponScrpt.equippedPrimaryWeapon;
        PlayerData.playerdata.info.equippedSecondary = weaponScrpt.equippedSecondaryWeapon;
        PlayerData.playerdata.info.equippedSupport = weaponScrpt.equippedSupportWeapon;
        PlayerData.playerdata.info.equippedPrimaryType = weaponScrpt.equippedPrimaryType;
        PlayerData.playerdata.info.equippedSecondaryType = weaponScrpt.equippedSecondaryType;
        PlayerData.playerdata.info.equippedSupportType = weaponScrpt.equippedSupportType;
        Destroy(bodyReference);
        bodyReference = null;
        bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[character].prefabPath));
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
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
        WeaponScript myWeps = bodyReference.GetComponent<WeaponScript>();
        // Set mod data that was just saved
        if (weaponName == myWeps.equippedPrimaryWeapon)
        {
            PlayerData.playerdata.primaryModInfo = newModInfo;
        } else if (weaponName == myWeps.equippedSecondaryWeapon)
        {
            PlayerData.playerdata.secondaryModInfo = newModInfo;
        } else if (weaponName == myWeps.equippedSupportWeapon)
        {
            PlayerData.playerdata.supportModInfo = newModInfo;
        }
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
