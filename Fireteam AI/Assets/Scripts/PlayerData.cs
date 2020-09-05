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
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class PlayerData : MonoBehaviour
{
    private const string DEFAULT_SECONDARY = "Glock23";
    private const string DEFAULT_SUPPORT = "M67 Frag";
    private const string DEFAULT_MELEE = "Recon Knife";
    private const string DEFAULT_FOOTWEAR_MALE = "Standard Boots (M)";
    private const string DEFAULT_FOOTWEAR_FEMALE = "Standard Boots (F)";
    public const uint MAX_EXP = 50000000;
    public const uint MAX_GP = uint.MaxValue;
    public const uint MAX_KASH = uint.MaxValue;

    public static PlayerData playerdata;
    public string playername;
    public bool disconnectedFromServer;
    public string disconnectReason;
    public bool testMode;
    private bool dataLoadedFlag;
    private bool playerDataModifyLegalFlag;
    private bool inventoryDataModifyLegalFlag;
    public PlayerInfo info;
    public PlayerInventory inventory;
    public ModInfo primaryModInfo;
    public ModInfo secondaryModInfo;
    public ModInfo supportModInfo;

    public GameObject bodyReference;
    public GameObject inGamePlayerReference;
    public TitleControllerScript titleRef;
    public GameOverController gameOverControllerRef;
    public Texture[] rankInsignias;

    void Awake()
    {
        if (playerdata == null)
        {
            DontDestroyOnLoad(gameObject);
            this.info = new PlayerInfo();
            this.inventory = new PlayerInventory();
            this.primaryModInfo = new ModInfo();
            this.secondaryModInfo = new ModInfo();
            this.supportModInfo = new ModInfo();
            playerdata = this;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "loggedIn").ValueChanged += HandleForceLogoutEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "gp").ValueChanged += HandleGpChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "kash").ValueChanged += HandleKashChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedArmor").ValueChanged += HandleArmorChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedBottom").ValueChanged += HandleBottomChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedCharacter").ValueChanged += HandleCharacterChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedFacewear").ValueChanged += HandleFacewearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedFootwear").ValueChanged += HandleFootwearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedHeadgear").ValueChanged += HandleHeadgearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedMelee").ValueChanged += HandleMeleeChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedPrimary").ValueChanged += HandlePrimaryChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedSecondary").ValueChanged += HandleSecondaryChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedSupport").ValueChanged += HandleSupportChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedTop").ValueChanged += HandleTopChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/ban").ChildAdded += HandleBanEvent;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId).ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId).ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId).ChildChanged += HandleInventoryChanged;
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
            PlayerData.playerdata.info.OnPlayerInfoChange += PlayerInfoChangeHandler;
            PlayerData.playerdata.inventory.OnInventoryChange += InventoryChangeHandler;
        }
        else if (playerdata != this)
        {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (dataLoadedFlag) {
            InstantiatePlayer();
            titleRef.SetPlayerStatsForTitle();
            dataLoadedFlag = false;
        }
    }

    string GetCharacterPrefabName() {
        string characterPrefabName = "";
        if (PlayerData.playerdata.info.equippedCharacter.Equals("Lucas")) {
            characterPrefabName = "LucasGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Daryl")) {
            characterPrefabName = "DarylGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Yongjin")) {
            characterPrefabName = "YongjinGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Rocko")) {
            characterPrefabName = "RockoGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Codename Sayre")) {
            characterPrefabName = "SayreGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Hana")) {
            characterPrefabName = "HanaGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Jade")) {
            characterPrefabName = "JadeGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Dani")) {
            characterPrefabName = "DaniGamePrefab";
        }
        return characterPrefabName;
    }

    void PlayerInfoChangeHandler() {
        // This should never be triggered unless called from the listeners. Therefore if it is, we need to ban the player
        if (!playerDataModifyLegalFlag) {
            // Ban player here
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of user data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

    void InventoryChangeHandler() {
        // This should never be triggered unless called from the listeners. Therefore if it is, we need to ban the player
        if (!inventoryDataModifyLegalFlag) {
            // Ban player here
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of user data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

    public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName.Equals("Badlands1") || levelName.Equals("Badlands1_Red") || levelName.Equals("Badlands1_Blue"))
        {
            string characterPrefabName = GetCharacterPrefabName();
            PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
                characterPrefabName,
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0],
                Quaternion.Euler(Vector3.zero));
        } else if (levelName.Equals("Badlands2") || levelName.Equals("Badlands2_Red") || levelName.Equals("Badlands2_Blue")) {
            string characterPrefabName = GetCharacterPrefabName();
            PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
                characterPrefabName,
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[1],
                Quaternion.Euler(0f, 180f, 0f));
        } else if (levelName.Equals("Test")) {
            string characterPrefabName = GetCharacterPrefabName();
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
                    // LoadInventory();
                }
                titleRef.SetPlayerStatsForTitle();
            }
        }

    }

    public void LoadPlayerData()
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        playerDataModifyLegalFlag = true;
        // Check if the DB has equipped data for the player. If not, then set default char and equips.
        // If error occurs, show error message on splash and quit the application
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("loadPlayerDataAndInventory");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Your data could not be loaded. Either your data is corrupted, or the service is unavailable. Please check the website for further details. If this issue persists, please create a ticket at koobando.com/support.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Dictionary<object, object> playerDataSnap = (Dictionary<object, object>)results["playerData"];
                    Dictionary<object, object> inventorySnap = (Dictionary<object, object>)results["inventory"];
                    info.defaultChar = playerDataSnap["defaultChar"].ToString();
                    info.defaultWeapon = playerDataSnap["defaultWeapon"].ToString();
                    info.playername = playerDataSnap["username"].ToString();
                    info.exp = uint.Parse(playerDataSnap["exp"].ToString());
                    info.gp = uint.Parse(playerDataSnap["gp"].ToString());
                    info.kash = uint.Parse(playerDataSnap["kash"].ToString());
                    if (playerDataSnap.ContainsKey("equipment")) {
                        Dictionary<object, object> equipmentSnap = (Dictionary<object, object>)playerDataSnap["equipment"];
                        info.equippedCharacter = equipmentSnap["equippedCharacter"].ToString();
                        info.equippedPrimary = equipmentSnap["equippedPrimary"].ToString();
                        info.equippedSecondary = equipmentSnap["equippedSecondary"].ToString();
                        info.equippedSupport = equipmentSnap["equippedSupport"].ToString();
                        info.equippedMelee = equipmentSnap["equippedMelee"].ToString();
                        info.equippedTop = equipmentSnap["equippedTop"].ToString();
                        info.equippedBottom = equipmentSnap["equippedBottom"].ToString();
                        info.equippedFootwear = equipmentSnap["equippedFootwear"].ToString();
                        info.equippedFacewear = equipmentSnap["equippedFacewear"].ToString();
                        info.equippedHeadgear = equipmentSnap["equippedHeadgear"].ToString();
                        info.equippedArmor = equipmentSnap["equippedArmor"].ToString();
                        Dictionary<object, object> weaponsInventorySnap = (Dictionary<object, object>)inventorySnap["weapons"];
                        Dictionary<object, object> thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.equippedPrimary];
                        Dictionary<object, object> modsInventorySnap = (Dictionary<object, object>)inventorySnap["mods"];
                        Dictionary<object, object> suppressorModSnap = null;
                        Dictionary<object, object> sightModSnap = null;
                        string suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                        string sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                        primaryModInfo.weaponName = info.equippedPrimary;
                        primaryModInfo.suppressorId = suppressorModId;
                        primaryModInfo.sightId = sightModId;
                        Debug.Log(suppressorModId);
                        Debug.Log(sightModId);
                        if (!"".Equals(suppressorModId)) {
                            suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                            primaryModInfo.equippedSuppressor = suppressorModSnap["name"].ToString();
                        }
                        if (!"".Equals(sightModId)) {
                            sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                            primaryModInfo.equippedSight = sightModSnap["name"].ToString();
                        }
                        thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.equippedSecondary];
                        suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                        sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                        secondaryModInfo.weaponName = info.equippedSecondary;
                        secondaryModInfo.suppressorId = suppressorModId;
                        secondaryModInfo.sightId = sightModId;
                        if (!"".Equals(suppressorModId)) {
                            suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                            secondaryModInfo.equippedSuppressor = suppressorModSnap["name"].ToString();
                        }
                        if (!"".Equals(sightModId)) {
                            sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                            secondaryModInfo.equippedSight = sightModSnap["name"].ToString();
                        }
                        thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.equippedSupport];
                        suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                        sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                        supportModInfo.weaponName = info.equippedSupport;
                        supportModInfo.suppressorId = suppressorModId;
                        supportModInfo.sightId = sightModId;
                        if (!"".Equals(suppressorModId)) {
                            suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                            supportModInfo.equippedSuppressor = suppressorModSnap["name"].ToString();
                        }
                        if (!"".Equals(sightModId)) {
                            sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                            supportModInfo.equippedSuppressor = sightModSnap["name"].ToString();
                        }
                    } else {
                        info.equippedCharacter = playerDataSnap["defaultChar"].ToString();
                        char g = InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender;
                        info.equippedPrimary = playerDataSnap["defaultWeapon"].ToString();
                        info.equippedSecondary = DEFAULT_SECONDARY;
                        info.equippedSupport = DEFAULT_SUPPORT;
                        info.equippedMelee = DEFAULT_MELEE;
                        info.equippedTop = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultTop;
                        info.equippedBottom = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultBottom;
                        info.equippedFootwear = (g == 'M' ? DEFAULT_FOOTWEAR_MALE : DEFAULT_FOOTWEAR_FEMALE);
                        info.equippedFacewear = "";
                        info.equippedHeadgear = "";
                        info.equippedArmor = "";
        
                        primaryModInfo.equippedSuppressor = "";
                        primaryModInfo.equippedSight = "";
                        primaryModInfo.weaponName = "";
                        primaryModInfo.suppressorId = "";
                        primaryModInfo.sightId = "";
        
                        secondaryModInfo.equippedSuppressor = "";
                        secondaryModInfo.equippedSight = "";
                        secondaryModInfo.weaponName = "";
                        secondaryModInfo.suppressorId = "";
                        secondaryModInfo.sightId = "";
        
                        supportModInfo.equippedSuppressor = "";
                        supportModInfo.equippedSight = "";
                        supportModInfo.weaponName = "";
                        supportModInfo.suppressorId = "";
                        supportModInfo.sightId = "";
                    }
                    LoadInventory(inventorySnap);
                    string[] itemsExpired = (string[])results["itemsExpired"];
                    if (itemsExpired.Length > 0) {
                        titleRef.TriggerExpirationPopup(itemsExpired);
                    }
                    dataLoadedFlag = true;
                    playerDataModifyLegalFlag = false;
                } else {
                    TriggerEmergencyExit("Your data could not be loaded. Either your data is corrupted, or the service is unavailable. Please check the website for further details. If this issue persists, please create a ticket at koobando.com/support.");
                }
            }
        });
    }

    public void InstantiatePlayer() {
        FindBodyRef(info.equippedCharacter);
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        this.primaryModInfo = LoadModDataForWeapon(info.equippedPrimary);
        this.secondaryModInfo = LoadModDataForWeapon(info.equippedSecondary);
        this.supportModInfo = LoadModDataForWeapon(info.equippedSupport);
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        OnCharacterChange(info.equippedCharacter);
        OnHeadgearChange(info.equippedHeadgear);
        OnFacewearChange(info.equippedFacewear);
        OnTopChange(info.equippedTop);
        OnBottomChange(info.equippedBottom);
        OnFootwearChange(info.equippedFootwear);
        OnArmorChange(info.equippedArmor);
        OnPrimaryChange(info.equippedPrimary);
        OnSecondaryChange(info.equippedSecondary);
        OnSupportChange(info.equippedSupport);
        OnMeleeChange(info.equippedMelee);
        PhotonNetwork.NickName = playername;
    }

    void RefreshInventory(DataSnapshot snapshot) {
        DataSnapshot headgearSnapshot = snapshot.Child("headgear");
        DataSnapshot topsSnapshot = snapshot.Child("tops");
        DataSnapshot bottomsSnapshot = snapshot.Child("bottoms");
        DataSnapshot facewearSnapshot = snapshot.Child("facewear");
        DataSnapshot footwearSnapshot = snapshot.Child("footwear");
        DataSnapshot armorSnapshot = snapshot.Child("armor");
        DataSnapshot weaponsSnapshot = snapshot.Child("weapons");
        DataSnapshot charactersSnapshot = snapshot.Child("characters");
        DataSnapshot modsSnapshot = snapshot.Child("mods");
        Dictionary<string, string> itemsModified = new Dictionary<string, string>();
        WeaponScript wepScript = bodyReference.GetComponent<WeaponScript>();

        IEnumerator<DataSnapshot> iter = headgearSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            EquipmentData e = null;
            if (!inventory.myHeadgear.ContainsKey(key)) {
                e = new EquipmentData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myHeadgear.Add(key, e);
            } else {
                e = inventory.myHeadgear[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = topsSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            EquipmentData e = null;
            if (!inventory.myTops.ContainsKey(key)) {
                e = new EquipmentData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myTops.Add(key, e);
            } else {
                e = inventory.myTops[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = bottomsSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            EquipmentData e = null;
            if (!inventory.myBottoms.ContainsKey(key)) {
                e = new EquipmentData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myBottoms.Add(key, e);
            } else {
                e = inventory.myBottoms[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = facewearSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            EquipmentData e = null;
            if (!inventory.myFacewear.ContainsKey(key)) {
                e = new EquipmentData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myFacewear.Add(key, e);
            } else {
                e = inventory.myFacewear[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = footwearSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            EquipmentData e = null;
            if (!inventory.myFootwear.ContainsKey(key)) {
                e = new EquipmentData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myFootwear.Add(key, e);
            } else {
                e = inventory.myFootwear[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = armorSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            ArmorData e = null;
            if (!inventory.myArmor.ContainsKey(key)) {
                e = new ArmorData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myArmor.Add(key, e);
            } else {
                e = inventory.myArmor[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = weaponsSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            WeaponData e = null;
            if (!inventory.myWeapons.ContainsKey(key)) {
                e = new WeaponData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                e.equippedSuppressor = curr.Child("equippedSuppressor").Value.ToString();
                e.equippedSight = curr.Child("equippedSight").Value.ToString();
                e.equippedClip = curr.Child("equippedClip").Value.ToString();
                inventory.myWeapons.Add(key, e);
            } else {
                e = inventory.myWeapons[key];
                string prevSuppId = e.equippedSuppressor;
                string prevSightId = e.equippedSight;
                string prevClipId = e.equippedClip;
                string newSuppId = curr.Child("equippedSuppressor").Value.ToString();
                string newSightId = curr.Child("equippedSight").Value.ToString();
                string newClipId = curr.Child("equippedClip").Value.ToString();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                e.equippedSuppressor = curr.Child("equippedSuppressor").Value.ToString();
                e.equippedSight = curr.Child("equippedSight").Value.ToString();
                e.equippedClip = curr.Child("equippedClip").Value.ToString();
                if (prevSuppId != newSuppId) {
                    wepScript.UnequipMod("Suppressor", key);
                    if (newSuppId != "") {
                        wepScript.EquipMod("Suppressor", newSuppId, key, null);
                    }
                }
                if (prevSightId != newSightId) {
                    wepScript.UnequipMod("Sight", key);
                    if (newSightId != "") {
                        wepScript.EquipMod("Sight", newSightId, key, null);
                    }
                }
                if (prevClipId != newClipId) {
                    wepScript.UnequipMod("Clip", key);
                    if (newClipId != "") {
                        wepScript.EquipMod("Clip", newClipId, key, null);
                    }
                }
            }
            itemsModified.Add(key, null);
        }

        iter = charactersSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            CharacterData e = null;
            if (!inventory.myCharacters.ContainsKey(key)) {
                e = new CharacterData();
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                inventory.myCharacters.Add(key, e);
            } else {
                e = inventory.myCharacters[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        iter = modsSnapshot.Children.GetEnumerator();
        while (iter.MoveNext()) {
            DataSnapshot curr = iter.Current;
            string key = curr.Key.ToString();
            ModData e = null;
            if (!inventory.myMods.ContainsKey(key)) {
                e = new ModData();
                e.id = key;
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                e.equippedOn = curr.Child("equippedOn").Value.ToString();
                inventory.myMods.Add(key, e);
            } else {
                e = inventory.myMods[key];
                e.duration = curr.Child("duration").Value.ToString();
                e.acquireDate = curr.Child("acquireDate").Value.ToString();
                e.equippedOn = curr.Child("equippedOn").Value.ToString();
            }
            itemsModified.Add(key, null);
        }

        // Delete items that weren't found in the update
        foreach(KeyValuePair<string, EquipmentData> entry in inventory.myHeadgear) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myHeadgear.Remove(key);
            }
        }
        foreach(KeyValuePair<string, EquipmentData> entry in inventory.myTops) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myTops.Remove(key);
            }
        }
        foreach(KeyValuePair<string, EquipmentData> entry in inventory.myBottoms) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myBottoms.Remove(key);
            }
        }
        foreach(KeyValuePair<string, EquipmentData> entry in inventory.myFacewear) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myFacewear.Remove(key);
            }
        }
        foreach(KeyValuePair<string, EquipmentData> entry in inventory.myFootwear) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myFootwear.Remove(key);
            }
        }
        foreach(KeyValuePair<string, ArmorData> entry in inventory.myArmor) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myArmor.Remove(key);
            }
        }
        foreach(KeyValuePair<string, WeaponData> entry in inventory.myWeapons) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myWeapons.Remove(key);
            }
        }
        foreach(KeyValuePair<string, CharacterData> entry in inventory.myCharacters) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                inventory.myCharacters.Remove(key);
            }
        }
        foreach(KeyValuePair<string, ModData> entry in inventory.myMods) {
            string key = entry.Key;
            if (!itemsModified.ContainsKey(key)) {
                wepScript.UnequipMod(InventoryScript.itemData.modCatalog[key].category, inventory.myMods[key].equippedOn);
                inventory.myMods.Remove(key);
            }
        }
    }

    public void LoadInventory(Dictionary<object, object> snapshot) {
        Dictionary<object, object> subSnapshot = (Dictionary<object, object>)snapshot["weapons"];
        IEnumerator dataLoaded = subSnapshot.GetEnumerator();
        // Load weapons
        foreach(KeyValuePair<object, object> entry in subSnapshot) {
            WeaponData w = new WeaponData();
            Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
            string key = entry.Key.ToString();
            w.name = key;
            w.acquireDate = thisSnapshot["acquireDate"].ToString();
            w.duration = thisSnapshot["duration"].ToString();
            object equippedSuppressor = null;
            if (thisSnapshot.ContainsKey("equippedSuppressor")) {
                equippedSuppressor = thisSnapshot["equippedSuppressor"];
            }
            object equippedClip = null;
            if (thisSnapshot.ContainsKey("equippedClip")) {
                equippedClip = thisSnapshot["equippedClip"];
            }
            object equippedSight = null;
            if (thisSnapshot.ContainsKey("equippedSight")) {
                equippedSight = thisSnapshot["equippedSight"];
            }
            w.equippedSuppressor = (equippedSuppressor == null ? "" : equippedSuppressor.ToString());
            w.equippedClip = (equippedClip == null ? "" : equippedClip.ToString());
            w.equippedSight = (equippedSight == null ? "" : equippedSight.ToString());
            inventory.myWeapons.Add(key, w);
        }
        subSnapshot = (Dictionary<object, object>)snapshot["characters"];
        dataLoaded = subSnapshot.GetEnumerator();
        // Load characters
        foreach(KeyValuePair<object, object> entry in subSnapshot) {
            CharacterData c = new CharacterData();
            Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
            string key = entry.Key.ToString();
            c.name = key;
            c.acquireDate = thisSnapshot["acquireDate"].ToString();
            c.duration = thisSnapshot["duration"].ToString();
            // If item is expired, delete from database. Else, add it to inventory
            inventory.myCharacters.Add(key, c);
        }
        if (snapshot.ContainsKey("armor")) {
            subSnapshot = (Dictionary<object, object>)snapshot["armor"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load armor
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                ArmorData a = new ArmorData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                a.name = key;
                a.acquireDate = thisSnapshot["acquireDate"].ToString();
                a.duration = thisSnapshot["duration"].ToString();
                inventory.myArmor.Add(key, a);
            }
        }
        if (snapshot.ContainsKey("tops")) {
            subSnapshot = (Dictionary<object, object>)snapshot["tops"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load tops
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.name = key;
                d.acquireDate = thisSnapshot["acquireDate"].ToString();
                d.duration = thisSnapshot["duration"].ToString();
                inventory.myTops.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("bottoms")) {
            subSnapshot = (Dictionary<object, object>)snapshot["bottoms"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load bottoms
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.name = key;
                d.acquireDate = thisSnapshot["acquireDate"].ToString();
                d.duration = thisSnapshot["duration"].ToString();
                inventory.myBottoms.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("footwear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["footwear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load footwear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.name = key;
                d.acquireDate = thisSnapshot["acquireDate"].ToString();
                d.duration = thisSnapshot["duration"].ToString();
                inventory.myFootwear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("headgear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["headgear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load headgear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.name = key;
                d.acquireDate = thisSnapshot["acquireDate"].ToString();
                d.duration = thisSnapshot["duration"].ToString();
                inventory.myHeadgear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("facewear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["facewear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load facewear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.name = key;
                d.acquireDate = thisSnapshot["acquireDate"].ToString();
                d.duration = thisSnapshot["duration"].ToString();
                inventory.myFacewear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("mods")) {
            subSnapshot = (Dictionary<object, object>)snapshot["mods"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load mods
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                ModData m = new ModData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                m.id = key;
                m.name = thisSnapshot["name"].ToString();
                m.acquireDate = thisSnapshot["acquireDate"].ToString();
                m.duration = thisSnapshot["duration"].ToString();
                m.equippedOn = thisSnapshot["equippedOn"].ToString();
                inventory.myMods.Add(key, m);
            }
        }
    }

    public void FindBodyRef(string character)
    {
        if (bodyReference == null)
        {
            bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[character]]);
        }
        // else
        // {
        //     bodyReference = GameObject.FindGameObjectWithTag("Player");
        // }
    }

    // Ensure that character changed listener re-equips weapons and sets equipment to defautl too
    public void UpdateBodyRef()
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        if (bodyReference.GetComponent<EquipmentScript>().equippedCharacter == PlayerData.playerdata.info.equippedCharacter)
        {
            return;
        }
        WeaponScript weaponScrpt = bodyReference.GetComponent<WeaponScript>();
        Destroy(bodyReference);
        bodyReference = null;
        bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[PlayerData.playerdata.info.equippedCharacter]]);
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
    }

    // Saves mod data for given weapon. If ID is null, then that means there was no mod on that weapon to begin with when it was saved.
    // Therefore, don't do anything.
    // If the ID is not null but the equippedSuppressor is, then that means that a suppressor was unequipped from a weapon.
    // Therefore, set the equipped on for the mod to empty string and set the equippedSuppressor for the weapon to empty string.
    public void SaveModDataForWeapon(string weaponName, string equippedSuppressor, string equippedSight, string suppressorId, string sightId) {
        //Debug.Log("Data passed in: " + weaponName + ", " + equippedSuppressor + ", " + id);
        if (string.IsNullOrEmpty(suppressorId) && string.IsNullOrEmpty(sightId))
        {
            return;
        }

        ModInfo newModInfo = new ModInfo();
        WeaponScript myWeps = bodyReference.GetComponent<WeaponScript>();
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;

        if (suppressorId != null && !"".Equals(suppressorId)) {
            if (suppressorId != null && !"".Equals(suppressorId) && string.IsNullOrEmpty(equippedSuppressor))
            {
                inputData["weaponName"] = weaponName;
                inputData["suppressorId"] = suppressorId;
                inputData["equippedSuppressor"] = "";
                inputData["suppressorEquippedOn"] = "";
            }
            else
            {
                inputData["weaponName"] = weaponName;
                inputData["suppressorId"] = suppressorId;
                inputData["equippedSuppressor"] = suppressorId;
                inputData["suppressorEquippedOn"] = weaponName;
            }
        }

        if (sightId != null && !"".Equals(sightId)) {
            if (sightId != null && !"".Equals(sightId) && string.IsNullOrEmpty(equippedSight))
            {
                inputData["weaponName"] = weaponName;
                inputData["sightId"] = sightId;
                inputData["equippedSight"] = "";
                inputData["sightEquippedOn"] = "";
            }
            else
            {
                // Mod was added/changed
                inputData["weaponName"] = weaponName;
                inputData["sightId"] = sightId;
                inputData["equippedSight"] = sightId;
                inputData["sightEquippedOn"] = weaponName;
            }
        }

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("saveModDataForWeapon");
        func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Mod saves successful.");
                } else {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public ModInfo LoadModDataForWeapon(string weaponName) {
        ModInfo modInfo = new ModInfo();
        modInfo.weaponName = weaponName;

        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.inventory.myMods)
        {
            // If the mod is equipped on the given weapon, load it into the requested mod info
            ModData m = entry.Value;
            if (m.equippedOn.Equals(weaponName)) {
                Mod modDetails = InventoryScript.itemData.modCatalog[m.name];
                if (modDetails.category.Equals("Suppressor")) {
                    modInfo.suppressorId = m.id;
                    modInfo.equippedSuppressor = m.name;
                } else if (modDetails.category.Equals("Sight")) {
                    modInfo.sightId = m.id;
                    modInfo.equippedSight = m.name;
                } else if (modDetails.category.Equals("Clip")) {
                    // modInfo.clipId = m.id;
                    modInfo.equippedClip = m.name;
                }
            }
        }
        
        return modInfo;
    }

    public void AddItemToInventory(string itemName, string type, float duration, bool purchased) {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["itemName"] = itemName;
        inputData["duration"] = duration;
        inputData["category"] = ConvertTypeToFirebaseType(type);
        if (purchased) {
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("transactItem");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                } else {
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() == "200") {
                        titleRef.TriggerMarketplacePopup("Purchase successful! The item has been added to your inventory.");
                    } else {
                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    }
                }
            });
        } else {
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("giveItemToUser");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                } else {
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() != "200") {
                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    }
                }
            });
        }
    }

    bool ItemIsDeletable(string itemName, string type) {
        if (type == "Armor") {
            if (!InventoryScript.itemData.armorCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Character") {
            if (!InventoryScript.itemData.characterCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Weapon") {
            if (!InventoryScript.itemData.weaponCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Mod") {
            if (!InventoryScript.itemData.modCatalog[itemName].deleteable) {
                return false;
            }
        } else {
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable) {
                return false;
            }
        }
        return true;
    }

    // Removes item from inventory in DB
    public void DeleteItemFromInventory(string itemName, string type, string modId)
    {
        // If item cannot be deleted, then skip
        if (!ItemIsDeletable(itemName, type))
        {
            return;
        }
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["itemId"] = itemName;
        inputData["category"] = ConvertTypeToFirebaseType(type);
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("deleteItemFromUser");
        func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void AddExpAndGpToPlayer(uint aExp, uint aGp) {
        // Save locally
        uint newExp = (uint)Mathf.Min(PlayerData.playerdata.info.exp + aExp, PlayerData.MAX_EXP);
        uint newGp = (uint)Mathf.Min(PlayerData.playerdata.info.gp + aGp, PlayerData.MAX_GP);
        // Save to DB
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["exp"] = newExp;
        inputData["gp"] = newGp;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public Texture GetRankInsigniaForRank(string rank) {
        switch (rank) {
            case "Trainee":
                return rankInsignias[0];
            case "Recruit":
                return rankInsignias[1];
            case "Private":
                return rankInsignias[2];
            case "Private First Class":
                return rankInsignias[3];
            case "Corporal":
                return rankInsignias[4];
            case "Sergeant":
                return rankInsignias[5];
            case "Staff Sergeant I":
                return rankInsignias[6];
            case "Staff Sergeant II":
                return rankInsignias[7];
            case "Staff Sergeant III":
                return rankInsignias[8];
            case "Sergeant First Class I":
                return rankInsignias[9];
            case "Sergeant First Class II":
                return rankInsignias[10];
            case "Sergeant First Class III":
                return rankInsignias[11];
            case "Master Sergeant I":
                return rankInsignias[12];
            case "Master Sergeant II":
                return rankInsignias[13];
            case "Master Sergeant III":
                return rankInsignias[14];
            case "Master Sergeant IV":
                return rankInsignias[15];
            case "Command Sergeant Major I":
                return rankInsignias[16];
            case "Command Sergeant Major II":
                return rankInsignias[17];
            case "Command Sergeant Major III":
                return rankInsignias[18];
            case "Command Sergeant Major IV":
                return rankInsignias[19];
            case "Command Sergeant Major V":
                return rankInsignias[20];
            case "Second Lieutenant I":
                return rankInsignias[21];
            case "Second Lieutenant II":
                return rankInsignias[22];
            case "Second Lieutenant III":
                return rankInsignias[23];
            case "Second Lieutenant IV":
                return rankInsignias[24];
            case "Second Lieutenant V":
                return rankInsignias[25];
            case "First Lieutenant I":
                return rankInsignias[26];
            case "First Lieutenant II":
                return rankInsignias[27];
            case "First Lieutenant III":
                return rankInsignias[28];
            case "First Lieutenant IV":
                return rankInsignias[29];
            case "First Lieutenant V":
                return rankInsignias[30];
            case "Captain I":
                return rankInsignias[31];
            case "Captain II":
                return rankInsignias[32];
            case "Captain III":
                return rankInsignias[33];
            case "Captain IV":
                return rankInsignias[34];
            case "Captain V":
                return rankInsignias[35];
            case "Major I":
                return rankInsignias[36];
            case "Major II":
                return rankInsignias[37];
            case "Major III":
                return rankInsignias[38];
            case "Major IV":
                return rankInsignias[39];
            case "Major V":
                return rankInsignias[40];
            case "Lieutenant Colonel I":
                return rankInsignias[41];
            case "Lieutenant Colonel II":
                return rankInsignias[42];
            case "Lieutenant Colonel III":
                return rankInsignias[43];
            case "Lieutenant Colonel IV":
                return rankInsignias[44];
            case "Lieutenant Colonel V":
                return rankInsignias[45];
            case "Colonel I":
                return rankInsignias[46];
            case "Colonel II":
                return rankInsignias[47];
            case "Colonel III":
                return rankInsignias[48];
            case "Colonel IV":
                return rankInsignias[49];
            case "Colonel V":
                return rankInsignias[50];
            case "Brigadier General":
                return rankInsignias[51];
            case "Major General":
                return rankInsignias[52];
            case "Lieutenant General":
                return rankInsignias[53];
            case "General":
                return rankInsignias[54];
            case "General of the Army":
                return rankInsignias[55];
            case "Commander in Chief I":
                return rankInsignias[56];
            case "Commander in Chief II":
                return rankInsignias[57];
            case "Commander in Chief III":
                return rankInsignias[58];
            case "Commander in Chief IV":
                return rankInsignias[59];
            case "Commander in Chief V":
                return rankInsignias[60];
            default:
                return rankInsignias[0];
        }
    }

    public Rank GetRankFromExp(uint exp) {
        if (exp >= 0 && exp <= 1999) {
            return new Rank("Trainee", 0, 1999);
        } else if (exp >= 2000 && exp <= 4499) {
            return new Rank("Recruit", 2000, 4499);
        } else if (exp >= 4500 && exp <= 5999) {
            return new Rank("Private", 4500, 5999);
        } else if (exp >= 6000 && exp <= 17999) {
            return new Rank("Private First Class", 6000, 17999);
        } else if (exp >= 18000 && exp <= 31999) {
            return new Rank("Corporal", 18000, 31999);
        } else if (exp >= 32000 && exp <= 53999) {
            return new Rank("Sergeant", 32000, 53999);
        } else if (exp >= 54000 && exp <= 78999) {
            return new Rank("Staff Sergeant I", 54000, 78999);
        } else if (exp >= 79000 && exp <= 108999) {
            return new Rank("Staff Sergeant II", 79000, 108999);
        } else if (exp >= 109000 && exp <= 144999) {
            return new Rank("Staff Sergeant III", 109000, 144999);
        } else if (exp >= 145000 && exp <= 185499) {
            return new Rank("Sergeant First Class I", 145000, 185499);
        } else if (exp >= 185500 && exp <= 232999) {
            return new Rank("Sergeant First Class II", 185500, 232999);
        } else if (exp >= 233000 && exp <= 291499) {
            return new Rank("Sergeant First Class III", 233000, 291499);
        } else if (exp >= 291500 && exp <= 353999) {
            return new Rank("Master Sergeant I", 291500, 353999);
        } else if (exp >= 354000 && exp <= 424999) {
            return new Rank("Master Sergeant II", 354000, 424999);
        } else if (exp >= 425000 && exp <= 503499) {
            return new Rank("Master Sergeant III", 425000, 503499);
        } else if (exp >= 503500 && exp <= 592999) {
            return new Rank("Master Sergeant IV", 503500, 592999);
        } else if (exp >= 593000 && exp <= 692999) {
            return new Rank("Command Sergeant Major I", 593000, 692999);
        } else if (exp >= 693000 && exp <= 803499) {
            return new Rank("Command Sergeant Major II", 693000, 803499);
        } else if (exp >= 803500 && exp <= 924999) {
            return new Rank("Command Sergeant Major III", 803500, 924999);
        } else if (exp >= 925000 && exp <= 1059999) {
            return new Rank("Command Sergeant Major IV", 925000, 1059999);
        } else if (exp >= 1060000 && exp <= 1199999) {
            return new Rank("Command Sergeant Major V", 1060000, 1199999);
        } else if (exp >= 1200000 && exp <= 1353499) {
            return new Rank("Second Lieutenant I", 1200000, 1353499);
        } else if (exp >= 1353500 && exp <= 1517999) {
            return new Rank("Second Lieutenant II", 1353500, 1517999);
        } else if (exp >= 1518000 && exp <= 1692999) {
            return new Rank("Second Lieutenant III", 1518000, 1692999);
        } else if (exp >= 1693000 && exp <= 1878499) {
            return new Rank("Second Lieutenant IV", 1693000, 1878499);
        } else if (exp >= 1878500 && exp <= 2071499) {
            return new Rank("Second Lieutenant V", 1878500, 2071499);
        } else if (exp >= 2071500 && exp <= 2278499) {
            return new Rank("First Lieutenant I", 2071500, 2278499);
        } else if (exp >= 2278500 && exp <= 2496999) {
            return new Rank("First Lieutenant II", 2278500, 2496999);
        } else if (exp >= 2497000 && exp <= 2724999) {
            return new Rank("First Lieutenant III", 2497000, 2724999);
        } else if (exp >= 2725000 && exp <= 2964999) {
            return new Rank("First Lieutenant IV", 2725000, 2964999);
        } else if (exp >= 2965000 && exp <= 3214499) {
            return new Rank("First Lieutenant V", 2965000, 3214499);
        } else if (exp >= 3214500 && exp <= 3510999) {
            return new Rank("Captain I", 3214500, 3510999);
        } else if (exp >= 3511000 && exp <= 3835999) {
            return new Rank("Captain II", 3511000, 3835999);
        } else if (exp >= 3836000 && exp <= 4189999) {
            return new Rank("Captain III", 3836000, 4189999);
        } else if (exp >= 4190000 && exp <= 4571499) {
            return new Rank("Captain IV", 4190000, 4571499);
        } else if (exp >= 4571500 && exp <= 4999999) {
            return new Rank("Captain V", 4571500, 4999999);
        } else if (exp >= 5000000 && exp <= 5474999) {
            return new Rank("Major I", 5000000, 5474999);
        } else if (exp >= 5475000 && exp <= 5996499) {
            return new Rank("Major II", 5475000, 5996499);
        } else if (exp >= 5996500 && exp <= 6564499) {
            return new Rank("Major III", 5996500, 6564499);
        } else if (exp >= 6564500 && exp <= 7178999) {
            return new Rank("Major IV", 6564500, 7178999);
        } else if (exp >= 7179000 && exp <= 7857999) {
            return new Rank("Major V", 7179000, 7857999);
        } else if (exp >= 7858000 && exp <= 8599999) {
            return new Rank("Lieutenant Colonel I", 7858000, 8599999);
        } else if (exp >= 8600000 && exp <= 9407499) {
            return new Rank("Lieutenant Colonel II", 8600000, 9407499);
        } else if (exp >= 9407500 && exp <= 10278999) {
            return new Rank("Lieutenant Colonel III", 9407500, 10278999);
        } else if (exp >= 10279000 && exp <= 11214999) {
            return new Rank("Lieutenant Colonel IV", 10279000, 11214999);
        } else if (exp >= 11215000 && exp <= 12214199) {
            return new Rank("Lieutenant Colonel V", 11215000, 12214199);
        } else if (exp >= 12215000 && exp <= 13278999) {
            return new Rank("Colonel I", 12215000, 13278999);
        } else if (exp >= 13279000 && exp <= 14406999) {
            return new Rank("Colonel II", 13279000, 14406999);
        } else if (exp >= 14407000 && exp <= 15599999) {
            return new Rank("Colonel III", 14407000, 15599999);
        } else if (exp >= 15600000 && exp <= 16857499) {
            return new Rank("Colonel IV", 15600000, 16857499);
        } else if (exp >= 16857500 && exp <= 18214999) {
            return new Rank("Colonel V", 16857500, 18214999);
        } else if (exp >= 18215000 && exp <= 19642999) {
            return new Rank("Brigadier General", 18215000, 19642999);
        } else if (exp >= 19643000 && exp <= 21429999) {
            return new Rank("Major General", 19643000, 21429999);
        } else if (exp >= 21430000 && exp <= 24285999) {
            return new Rank("Lieutenant General", 21430000, 24285999);
        } else if (exp >= 24286000 && exp <= 28571999) {
            return new Rank("General", 24286000, 28571999);
        } else if (exp >= 28572000 && exp <= 32856999) {
            return new Rank("General of the Army", 28572000, 32856999);
        } else if (exp >= 32857000 && exp <= 37142999) {
            return new Rank("Commander in Chief I", 32857000, 37142999);
        } else if (exp >= 37143000 && exp <= 41499999) {
            return new Rank("Commander in Chief II", 37143000, 41499999);
        } else if (exp >= 41500000 && exp <= 45719999) {
            return new Rank("Commander in Chief III", 41500000, 45719999);
        } else if (exp >= 45720000 && exp <= 49999999) {
            return new Rank("Commander in Chief IV", 45720000, 49999999);
        } else if (exp >= 50000000) {
            return new Rank("Commander in Chief V", 50000000, uint.MaxValue);
        }
        return new Rank("Trainee", 0, 1999);
    }

    // Only called in an emergency situation when the game needs to exit immediately (ex: database failure or user gets banned).
    public void TriggerEmergencyExit(string message) {
        // Freeze user mouse input
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Display emergency popup depending on which screen you're on
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Equals("GameOverSuccess") || currentScene.Equals("GameOverFail")) {
            gameOverControllerRef.TriggerEmergencyPopup("A fatal error has occurred:\n" + message + "\nThe game will now close.");
        } else if (currentScene.Equals("Title")) {
            titleRef.TriggerEmergencyPopup("A fatal error has occurred:\n" + message + "\nThe game will now close.");
        }

        StartCoroutine("EmergencyExitGame");
    }

    void HandleForceLogoutEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Key.ToString().Equals("loggedIn")) {
            if (args.Snapshot.Value != null) {
                if (args.Snapshot.Value.ToString() == "0") {
                    Application.Quit();
                }
            }
        }
    }

    void HandleGpChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            PlayerData.playerdata.info.gp = uint.Parse(args.Snapshot.Value.ToString());
            if (titleRef != null) {
                titleRef.myGpTxt.text = ""+PlayerData.playerdata.info.gp;
            }
            playerDataModifyLegalFlag = false;
        }
    }

    void HandleKashChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            PlayerData.playerdata.info.kash = uint.Parse(args.Snapshot.Value.ToString());
            if (titleRef != null) {
                titleRef.myKashTxt.text = ""+PlayerData.playerdata.info.kash;
            }
            playerDataModifyLegalFlag = false;
        }
    }

    void HandleArmorChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the armor is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedArmor = itemEquipped;
        OnArmorChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnArmorChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedArmor = PlayerData.playerdata.info.equippedArmor;
            if (thisEquipScript.equippedArmorTopRef != null) {
                Destroy(thisEquipScript.equippedArmorTopRef);
                thisEquipScript.equippedArmorTopRef = null;
            }
            if (thisEquipScript.equippedArmorBottomRef != null) {
                Destroy(thisEquipScript.equippedArmorBottomRef);
                thisEquipScript.equippedArmorBottomRef = null;
            }
        }

        if (itemEquipped != "") {
            Armor a = InventoryScript.itemData.armorCatalog[itemEquipped];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathTop] : InventoryScript.itemData.itemReferences[a.femalePrefabPathTop]);
            thisEquipScript.equippedArmorTopRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedArmorTopRef.transform.SetParent(bodyReference.transform);

            p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathBottom] : InventoryScript.itemData.itemReferences[a.femalePrefabPathBottom]);
            thisEquipScript.equippedArmorBottomRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedArmorBottomRef.transform.SetParent(bodyReference.transform);
            
            MeshFixer m = thisEquipScript.equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myArmorTopRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();

            m = thisEquipScript.equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myArmorBottomRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();

            titleRef.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, a.thumbnailPath);
            titleRef.shopEquippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, a.thumbnailPath);
        } else {
            titleRef.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
            titleRef.shopEquippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleTopChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the top is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedTop = itemEquipped;
        OnTopChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnTopChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedTop = PlayerData.playerdata.info.equippedTop;
            if (thisEquipScript.equippedTopRef != null) {
                Destroy(thisEquipScript.equippedTopRef);
                thisEquipScript.equippedTopRef = null;
            }
        }
        
        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedTopRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedTopRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myTopRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
            titleRef.shopEquippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }

        thisEquipScript.EquipSkin(e.skinType);
    }

    void HandleBottomChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the bottom is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedBottom = itemEquipped;
        OnBottomChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnBottomChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedBottom = PlayerData.playerdata.info.equippedBottom;
            if (thisEquipScript.equippedBottomRef != null) {
                Destroy(thisEquipScript.equippedBottomRef);
                thisEquipScript.equippedBottomRef = null;
            }
        }

        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedBottomRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedBottomRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myBottomRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
            titleRef.shopEquippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
    }

    void HandleCharacterChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the character is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedCharacter = itemEquipped;
        UpdateBodyRef();
        OnCharacterChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnCharacterChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedCharacter = PlayerData.playerdata.info.equippedCharacter;
            if (thisEquipScript.equippedSkinRef != null) {
                Destroy(thisEquipScript.equippedSkinRef);
                thisEquipScript.equippedSkinRef = null;
            }
        }

        Character c = InventoryScript.itemData.characterCatalog[itemEquipped];
        if (titleRef != null) {
            titleRef.equippedCharacterSlot.GetComponent<SlotScript>().ToggleThumbnail(true, c.thumbnailPath);
            titleRef.shopEquippedCharacterSlot.GetComponent<SlotScript>().ToggleThumbnail(true, c.thumbnailPath);
            titleRef.currentCharGender = InventoryScript.itemData.characterCatalog[name].gender;
            thisEquipScript.ResetStats();
        }

        // thisEquipScript.EquipTop(c.defaultTop, null);        
        Equipment e = InventoryScript.itemData.equipmentCatalog[c.defaultTop];
        GameObject p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedTopRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedTopRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myTopRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();
        thisEquipScript.EquipSkin(e.skinType);
        // thisEquipScript.EquipBottom(c.defaultBottom, null);
        e = InventoryScript.itemData.equipmentCatalog[c.defaultBottom];
        p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedBottomRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedBottomRef.transform.SetParent(bodyReference.transform);
        m = thisEquipScript.equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myBottomRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();
        // thisEquipScript.EquipFootwear((c.gender == 'M' ? "Standard Boots (M)" : "Standard Boots (F)"), null);
        e = InventoryScript.itemData.equipmentCatalog[c.gender == 'M' ? "Standard Boots (M)" : "Standard Boots (F)"];
        p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedFootwearRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedFootwearRef.transform.SetParent(bodyReference.transform);
        m = thisEquipScript.equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myFootwearRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        // Reequip primary
        Weapon w = InventoryScript.itemData.weaponCatalog[info.equippedPrimary];
        string weaponType = w.category;
        GameObject wepEquipped = thisWepScript.weaponHolder.LoadWeapon(w.prefabPath);
        
        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", primaryModInfo.equippedSuppressor, info.equippedPrimary, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", primaryModInfo.equippedSight, info.equippedPrimary, null);
        }

        if (titleRef.currentCharGender == 'M') {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }
    }

    void HandleFacewearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedFacewear = itemEquipped;
        OnFacewearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnFacewearChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedFacewear = itemEquipped;
            if (thisEquipScript.equippedFacewearRef != null) {
                Destroy(thisEquipScript.equippedFacewearRef);
                thisEquipScript.equippedFacewearRef = null;
            }
        }

        if (itemEquipped != "") {
            Equipment e = InventoryScript.itemData.equipmentCatalog[name];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
            thisEquipScript.equippedFacewearRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedFacewearRef.transform.SetParent(gameObject.transform);
            MeshFixer m = thisEquipScript.equippedFacewearRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myFacewearRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();
            titleRef.equippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
            titleRef.shopEquippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        } else {
            titleRef.equippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
            titleRef.shopEquippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleFootwearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the footwear is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedFootwear = itemEquipped;
        OnFootwearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnFootwearChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedFootwear = PlayerData.playerdata.info.equippedFootwear;
            if (thisEquipScript.equippedFootwearRef != null) {
                Destroy(thisEquipScript.equippedFootwearRef);
                thisEquipScript.equippedFootwearRef = null;
            }
        }

        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedFootwearRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedFootwearRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myFootwearRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
            titleRef.shopEquippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
    }

    void HandleHeadgearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedHeadgear = itemEquipped;
        OnHeadgearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnHeadgearChange(string itemEquipped) {
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedHeadgear = itemEquipped;
            if (thisEquipScript.equippedHeadgearRef != null) {
                Destroy(thisEquipScript.equippedHeadgearRef);
                thisEquipScript.equippedHeadgearRef = null;
            }
        }

        if (itemEquipped != "") {
            Equipment e = InventoryScript.itemData.equipmentCatalog[name];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
            thisEquipScript.equippedHeadgearRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedHeadgearRef.transform.SetParent(gameObject.transform);
            MeshFixer m = thisEquipScript.equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myHeadgearRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();
            titleRef.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
            titleRef.shopEquippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        } else {
            titleRef.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
            titleRef.shopEquippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleMeleeChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedMelee = itemEquipped;
        OnMeleeChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnMeleeChange(string itemEquipped) {
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedMeleeWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        titleRef.equippedMeleeSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
        titleRef.shopEquippedMeleeSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandlePrimaryChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedPrimary = itemEquipped;
        OnPrimaryChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnPrimaryChange(string itemEquipped) {
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedPrimaryWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.primaryModInfo = modInfo;
        GameObject wepEquipped = thisWepScript.weaponHolder.LoadWeapon(w.prefabPath);
        
        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", modInfo.equippedSuppressor, itemEquipped, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", modInfo.equippedSight, itemEquipped, null);
        }

        if (titleRef.currentCharGender == 'M') {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }

        // Puts the item that you just equipped in its proper slot
        titleRef.equippedPrimarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
        titleRef.shopEquippedPrimarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleSecondaryChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedPrimary = itemEquipped;
        OnSecondaryChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnSecondaryChange(string itemEquipped) {
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedSecondaryWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.secondaryModInfo = modInfo;

        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", modInfo.equippedSuppressor, itemEquipped, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", modInfo.equippedSight, itemEquipped, null);
        }

        titleRef.equippedSecondarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
        titleRef.shopEquippedSecondarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleSupportChangeEvent(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.equippedSupport = itemEquipped;
        OnSupportChange(itemEquipped);
        playerDataModifyLegalFlag = false;
    }

    void OnSupportChange(string itemEquipped) {
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedSupportWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.supportModInfo = modInfo;
        GameObject wepEquipped = thisWepScript.weaponHolder.LoadWeapon(w.prefabPath);

        titleRef.equippedSupportSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
        titleRef.shopEquippedSupportSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleBanEvent(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Child("ban").Value != null) {
            Dictionary<object, object> banValues = (Dictionary<object, object>)args.Snapshot.Value;
            string reason = banValues["reason"].ToString();
            TriggerEmergencyExit("You've been banned for the following reason:\n" + reason + "\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
        }
    }

    void HandleInventoryChanged(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When inventory item has been updated, find the item that has been updated and update it
        if (args.Snapshot.Value != null) {
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot);
            inventoryDataModifyLegalFlag = false;
        }
    }

    void HandleInventoryAdded(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When inventory item has been added, also add that item to this game session
        if (args.Snapshot.Value != null) {
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot);
            inventoryDataModifyLegalFlag = false;
        }
    }

    void HandleInventoryRemoved(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When inventory item has been removed, also remove that item from this game session
        if (args.Snapshot.Value != null) {
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot);
            inventoryDataModifyLegalFlag = false;
        }
    }

    IEnumerator EmergencyExitGame() {
        yield return new WaitForSeconds(5f);
        Dictionary<string, object> inputData = new Dictionary<string, object>();
		inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
		inputData["loggedIn"] = "0";

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("setUserIsLoggedIn");
		func.CallAsync(inputData).ContinueWith((task) => {
            Application.Quit();
        });
    }

    string ConvertTypeToFirebaseType(string type) {
        if (type.Equals("Weapon")) {
            return "weapons";
        } else if (type.Equals("Character")) {
            return "characters";
        } else if (type.Equals("Top")) {
            return "tops";
        } else if (type.Equals("Bottom")) {
            return "bottoms";
        } else if (type.Equals("Armor")) {
            return "armor";
        } else if (type.Equals("Footwear")) {
            return "footwear";
        } else if (type.Equals("Headgear")) {
            return "headgear";
        } else if (type.Equals("Facewear")) {
            return "facewear";
        } else if (type.Equals("Mod")) {
            return "mods";
        }
        return "";
    }
}

public class PlayerInfo
{
    public string defaultChar;
    public string defaultWeapon;
	public string playername;
    public string equippedCharacter;
    public string equippedHeadgear;
    public string equippedFacewear;
    public string equippedTop;
    public string equippedBottom;
    public string equippedFootwear;
    public string equippedArmor;
    public string equippedPrimary;
    public string equippedSecondary;
    public string equippedSupport;
    public string equippedMelee;
    public uint exp;
    public uint gp;
    public uint kash;

    public delegate void OnPlayerInfoChangeDelegate();
    public event OnPlayerInfoChangeDelegate OnPlayerInfoChange;
}

public class PlayerInventory {
    public PlayerInventory() {
        this.myHeadgear = new Dictionary<string, EquipmentData>();
        this.myTops = new Dictionary<string, EquipmentData>();
        this.myBottoms = new Dictionary<string, EquipmentData>();
        this.myFacewear = new Dictionary<string, EquipmentData>();
        this.myFootwear = new Dictionary<string, EquipmentData>();
        this.myArmor = new Dictionary<string, ArmorData>();
        this.myWeapons = new Dictionary<string, WeaponData>();
        this.myCharacters = new Dictionary<string, CharacterData>();
        this.myMods = new Dictionary<string, ModData>();
    }

    public Dictionary<string, EquipmentData> myHeadgear;
    public Dictionary<string, EquipmentData> myTops;
    public Dictionary<string, EquipmentData> myBottoms;
    public Dictionary<string, EquipmentData> myFacewear;
    public Dictionary<string, EquipmentData> myFootwear;
    public Dictionary<string, ArmorData> myArmor;
    public Dictionary<string, WeaponData> myWeapons;
    public Dictionary<string, CharacterData> myCharacters;
    public Dictionary<string, ModData> myMods;

    public delegate void OnInventoryChangeDelegate();
    public event OnInventoryChangeDelegate OnInventoryChange;
}

public class ModInfo
{
    public string suppressorId;
    public string sightId;
    public string weaponName;
    public string equippedSuppressor;
    public string equippedSight;
    public string equippedClip;
}

public class WeaponData {
    public string name;
    public string acquireDate;
    public string duration;
    public string equippedSuppressor;
    public string equippedSight;
    public string equippedClip;
}

public class EquipmentData {
    public string name;
    public string acquireDate;
    public string duration;
}

public class ModData {
    public string id;
    public string name;
    public string equippedOn;
    public string acquireDate;
    public string duration;
}

public class ArmorData {
    public string name;
    public string acquireDate;
    public string duration;
}

public class CharacterData {
    public string name;
    public string acquireDate;
    public string duration;
}

public class Rank {
    public string name;
    public uint minExp;
    public uint maxExp;
    public Rank(string name, uint minExp, uint maxExp) {
        this.name = name;
        this.minExp = minExp;
        this.maxExp = maxExp;
    }
}
