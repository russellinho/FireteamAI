using System.Collections;
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
    private bool saveDataFlag;
    private bool purchaseSuccessfulFlag;
    private bool purchaseFailFlag;
    private bool updateCurrencyFlag;
    private bool reloadPlayerFlag;
    private bool reinstantiatePlayerFlag;
    private bool skipReloadCharacterFlag;
    private string addDefaultClothingFlag;
    private string deleteDefaultClothingFlag;
    private ArrayList itemsExpired;
    public PlayerInfo info;
    public ModInfo primaryModInfo;
    public ModInfo secondaryModInfo;
    public ModInfo supportModInfo;

    public GameObject bodyReference;
    public GameObject inGamePlayerReference;
    public TitleControllerScript titleRef;
    public GameOverController gameOverControllerRef;
    public Dictionary<string, EquipmentData> myHeadgear;
    public Dictionary<string, EquipmentData> myTops;
    public Dictionary<string, EquipmentData> myBottoms;
    public Dictionary<string, EquipmentData> myFacewear;
    public Dictionary<string, EquipmentData> myFootwear;
    public Dictionary<string, ArmorData> myArmor;
    public Dictionary<string, WeaponData> myWeapons;
    public Dictionary<string, CharacterData> myCharacters;
    public Dictionary<string, ModData> myMods;
    public Texture[] rankInsignias;

    void Awake()
    {
        if (playerdata == null)
        {
            DontDestroyOnLoad(gameObject);
            this.info = new PlayerInfo();
            this.primaryModInfo = new ModInfo();
            this.secondaryModInfo = new ModInfo();
            this.supportModInfo = new ModInfo();
            this.myHeadgear = new Dictionary<string, EquipmentData>();
            this.myTops = new Dictionary<string, EquipmentData>();
            this.myBottoms = new Dictionary<string, EquipmentData>();
            this.myFacewear = new Dictionary<string, EquipmentData>();
            this.myFootwear = new Dictionary<string, EquipmentData>();
            this.myArmor = new Dictionary<string, ArmorData>();
            this.myWeapons = new Dictionary<string, WeaponData>();
            this.myCharacters = new Dictionary<string, CharacterData>();
            this.myMods = new Dictionary<string, ModData>();
            playerdata = this;
            LoadPlayerData();
            LoadInventory();
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }
        else if (playerdata != this)
        {
            Destroy(gameObject);
        }
        itemsExpired = new ArrayList();
    }

    void Update() {
        // Handle async calls
        if (dataLoadedFlag) {
            InstantiatePlayer();
            titleRef.SetPlayerStatsForTitle();
            dataLoadedFlag = false;
        }
        if (saveDataFlag) {
            SavePlayerData();
            saveDataFlag = false;
        }
        if (purchaseSuccessfulFlag) {
            titleRef.TriggerMarketplacePopup("Purchase successful! The item has been added to your inventory.");
            purchaseSuccessfulFlag = false;
        }
        if (purchaseFailFlag) {
            titleRef.TriggerMarketplacePopup("Purchase failed. Please try again later.");
            purchaseFailFlag = false;
        }
        if (updateCurrencyFlag) {
            titleRef.UpdateCurrency();
            updateCurrencyFlag = false;
        }
        if (itemsExpired.Count > 0)
        {
            titleRef.TriggerExpirationPopup(itemsExpired);
            itemsExpired.Clear();
        }
        if (reloadPlayerFlag)
        {
            ReinstantiatePlayer();
            skipReloadCharacterFlag = false;
            reloadPlayerFlag = false;
            reinstantiatePlayerFlag = false;
        }
        if (addDefaultClothingFlag != null)
        {
            string dTop = InventoryScript.itemData.characterCatalog[addDefaultClothingFlag].defaultTop;
            string dBottom = InventoryScript.itemData.characterCatalog[addDefaultClothingFlag].defaultBottom;
            AddItemToInventory(dTop, "Top", -1f, false, false, 0, 0);
            AddItemToInventory(dBottom, "Bottom", -1f, false, false, 0, 0);
            addDefaultClothingFlag = null;
        }
        if (deleteDefaultClothingFlag != null)
        {
            string dTop = InventoryScript.itemData.characterCatalog[deleteDefaultClothingFlag].defaultTop;
            string dBottom = InventoryScript.itemData.characterCatalog[deleteDefaultClothingFlag].defaultBottom;
            DeleteItemFromInventory(dTop, "Top", null, false);
            DeleteItemFromInventory(dBottom, "Bottom", null, false);
            deleteDefaultClothingFlag = null;
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
                Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0],
                Quaternion.Euler(Vector3.zero));
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
                if (PlayerData.playerdata.bodyReference == null && !dataLoadedFlag)
                {
                    LoadPlayerData();
                }
                titleRef.SetPlayerStatsForTitle();
            }
        }

    }

    public void SavePlayerData()
    {
        EquipmentScript myEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript myWeps = bodyReference.GetComponent<WeaponScript>();
        PlayerData.playerdata.info.equippedCharacter = myEquips.equippedCharacter;
        PlayerData.playerdata.info.equippedHeadgear = myEquips.equippedHeadgear;
        PlayerData.playerdata.info.equippedFacewear = myEquips.equippedFacewear;
        PlayerData.playerdata.info.equippedTop = myEquips.equippedTop;
        PlayerData.playerdata.info.equippedBottom = myEquips.equippedBottom;
        PlayerData.playerdata.info.equippedFootwear = myEquips.equippedFootwear;
        PlayerData.playerdata.info.equippedArmor = myEquips.equippedArmor;
        PlayerData.playerdata.info.equippedPrimary = myWeps.equippedPrimaryWeapon;
        PlayerData.playerdata.info.equippedSecondary = myWeps.equippedSecondaryWeapon;
        PlayerData.playerdata.info.equippedSupport = myWeps.equippedSupportWeapon;
        PlayerData.playerdata.info.equippedMelee = myWeps.equippedMeleeWeapon;

        DatabaseReference d = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("equipment");
        d.RunTransaction(task => {
            task.Child("equippedCharacter").Value = PlayerData.playerdata.info.equippedCharacter;
            task.Child("equippedPrimary").Value = PlayerData.playerdata.info.equippedPrimary;
            task.Child("equippedSecondary").Value = PlayerData.playerdata.info.equippedSecondary;
            task.Child("equippedSupport").Value = PlayerData.playerdata.info.equippedSupport;
            task.Child("equippedMelee").Value = PlayerData.playerdata.info.equippedMelee;
            task.Child("equippedTop").Value = PlayerData.playerdata.info.equippedTop;
            task.Child("equippedBottom").Value = PlayerData.playerdata.info.equippedBottom;
            task.Child("equippedFootwear").Value = PlayerData.playerdata.info.equippedFootwear;
            task.Child("equippedFacewear").Value = PlayerData.playerdata.info.equippedFacewear;
            task.Child("equippedHeadgear").Value = PlayerData.playerdata.info.equippedHeadgear;
            task.Child("equippedArmor").Value = PlayerData.playerdata.info.equippedArmor;
            return TransactionResult.Success(task);
        });
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
                TriggerEmergencyExit("Your data could not be loaded. Either your data is corrupted, or the service is unavailable. Please check the website for further details. If this issue persists, please create a ticket at koobando.com/support.");
            } else {
                info.defaultChar = snapshot.Child("defaultChar").Value.ToString();
                info.defaultWeapon = snapshot.Child("defaultWeapon").Value.ToString();
                info.playername = snapshot.Child("username").Value.ToString();
                info.exp = uint.Parse(snapshot.Child("exp").Value.ToString());
                info.gp = uint.Parse(snapshot.Child("gp").Value.ToString());
                info.kash = uint.Parse(snapshot.Child("kash").Value.ToString());
                // Equip previously equipped if available. Else, equip defaults and save it
                if (snapshot.HasChild("equipment")) {
                    DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).GetValueAsync().ContinueWith(taskA => {
                        if (taskA.IsCompleted) {
                            DataSnapshot inventorySnapshot = taskA.Result;
                            DataSnapshot equipSnapshot = snapshot.Child("equipment");
                            info.equippedCharacter = equipSnapshot.Child("equippedCharacter").Value.ToString();
                            info.equippedPrimary = equipSnapshot.Child("equippedPrimary").Value.ToString();
                            info.equippedSecondary = equipSnapshot.Child("equippedSecondary").Value.ToString();
                            info.equippedSupport = equipSnapshot.Child("equippedSupport").Value.ToString();
                            info.equippedMelee = equipSnapshot.Child("equippedMelee").Value.ToString();
                            info.equippedTop = equipSnapshot.Child("equippedTop").Value.ToString();
                            info.equippedBottom = equipSnapshot.Child("equippedBottom").Value.ToString();
                            info.equippedFootwear = equipSnapshot.Child("equippedFootwear").Value.ToString();
                            info.equippedFacewear = equipSnapshot.Child("equippedFacewear").Value.ToString();
                            info.equippedHeadgear = equipSnapshot.Child("equippedHeadgear").Value.ToString();
                            info.equippedArmor = equipSnapshot.Child("equippedArmor").Value.ToString();
    
                            DataSnapshot modsInventory = inventorySnapshot.Child("mods");
    
                            DataSnapshot modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedPrimary);
                            string suppressorModId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                            string sightModId = modSnapshot.Child("equippedSight").Value.ToString();
                            primaryModInfo.weaponName = info.equippedPrimary;
                            primaryModInfo.suppressorId = suppressorModId;
                            primaryModInfo.sightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                primaryModInfo.equippedSuppressor = modsInventory.Child(suppressorModId).Child("name").Value.ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                primaryModInfo.equippedSight = modsInventory.Child(sightModId).Child("name").Value.ToString();
                            }
    
                            modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedSecondary);
                            suppressorModId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                            sightModId = modSnapshot.Child("equippedSight").Value.ToString();
                            secondaryModInfo.weaponName = info.equippedSecondary;
                            secondaryModInfo.suppressorId = suppressorModId;
                            secondaryModInfo.sightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                secondaryModInfo.equippedSuppressor = modsInventory.Child(suppressorModId).Child("name").Value.ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                secondaryModInfo.equippedSight = modsInventory.Child(sightModId).Child("name").Value.ToString();
                            }
    
                            modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedSupport);
                            suppressorModId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                            sightModId = modSnapshot.Child("equippedSight").Value.ToString();
                            supportModInfo.weaponName = info.equippedSupport;
                            supportModInfo.suppressorId = suppressorModId;
                            supportModInfo.sightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                supportModInfo.equippedSuppressor = modsInventory.Child(suppressorModId).Child("name").Value.ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                supportModInfo.equippedSuppressor = modsInventory.Child(sightModId).Child("name").Value.ToString();
                            }
                            dataLoadedFlag = true;
                            updateCurrencyFlag = true;
                        } else {
                            TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                        }
                    });
                } else {
                    info.equippedCharacter = snapshot.Child("defaultChar").Value.ToString();
                    char g = InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender;
                    info.equippedPrimary = snapshot.Child("defaultWeapon").Value.ToString();
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
                    dataLoadedFlag = true;
                    saveDataFlag = true;
                    updateCurrencyFlag = true;
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
        if (!skipReloadCharacterFlag)
        {
            characterEquips.EquipCharacter(info.equippedCharacter, null);
        }
        characterEquips.EquipHeadgear(info.equippedHeadgear, null);
        characterEquips.EquipFacewear(info.equippedFacewear, null);
        characterEquips.EquipTop(info.equippedTop, null);
        characterEquips.EquipBottom(info.equippedBottom, null);
        characterEquips.EquipFootwear(info.equippedFootwear, null);
        characterEquips.EquipArmor(info.equippedArmor, null);
        characterWeps.EquipWeapon(info.equippedPrimary, primaryModInfo.equippedSuppressor, primaryModInfo.equippedSight, null);
        characterWeps.EquipWeapon(info.equippedSecondary, secondaryModInfo.equippedSuppressor, secondaryModInfo.equippedSight, null);
        characterWeps.EquipWeapon(info.equippedSupport, supportModInfo.equippedSuppressor, supportModInfo.equippedSight, null);
        characterWeps.EquipWeapon(info.equippedMelee, null, null, null);
        PhotonNetwork.NickName = playername;
    }

    public void ReinstantiatePlayer()
    {
        InstantiatePlayer();
        saveDataFlag = true;
    }

    public void LoadInventory() {
        DatabaseReference dr = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId);
        dr.RunTransaction(task => {
            IEnumerator<MutableData> dataLoaded = task.Child("weapons").Children.GetEnumerator();
            // Load weapons
            while (dataLoaded.MoveNext()) {
                WeaponData w = new WeaponData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                w.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    w.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    w.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(w.duration);
                }
                object equippedSuppressor = thisSnapshot.Child("equippedSuppressor").Value;
                object equippedClip = thisSnapshot.Child("equippedClip").Value;
                object equippedSight = thisSnapshot.Child("equippedSight").Value;
                w.equippedSuppressor = (equippedSuppressor == null ? "" : equippedSuppressor.ToString());
                w.equippedClip = (equippedClip == null ? "" : equippedClip.ToString());
                w.equippedSight = (equippedSight == null ? "" : equippedSight.ToString());
                // If item is expired, delete from database. Else, add it to inventory
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(w.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(w.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Weapon", null, true);
                        } else
                        {
                            myWeapons.Add(key, w);
                        }
                    } else
                    {
                        myWeapons.Add(key, w);
                    }
                }
            }
            
            dataLoaded = task.Child("characters").Children.GetEnumerator();
            // Load characters
            while (dataLoaded.MoveNext()) {
                CharacterData c = new CharacterData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                c.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    c.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    c.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(c.duration);
                }
                // If item is expired, delete from database. Else, add it to inventory
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(c.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(c.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Character", null, true);
                        }
                        else
                        {
                            myCharacters.Add(key, c);
                        }
                    }
                    else
                    {
                        myCharacters.Add(key, c);
                    }
                }
            }

            dataLoaded = task.Child("armor").Children.GetEnumerator();
            // Load armor
            while (dataLoaded.MoveNext()) {
                ArmorData a = new ArmorData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                a.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    a.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    a.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(a.duration);
                }
                // If item is expired, delete from database. Else, add it to inventory
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(a.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(a.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Armor", null, true);
                        }
                        else
                        {
                            myArmor.Add(key, a);
                        }
                    }
                    else
                    {
                        myArmor.Add(key, a);
                    }
                }
            }

            dataLoaded = task.Child("tops").Children.GetEnumerator();
            // Load tops
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                d.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    d.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(d.duration);
                }
                // If item is expired, delete from database. Else, add it to inventory
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(d.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(d.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Top", null, true);
                        }
                        else
                        {
                            myTops.Add(key, d);
                        }
                    }
                    else
                    {
                        myTops.Add(key, d);
                    }
                }
            }

            dataLoaded = task.Child("bottoms").Children.GetEnumerator();
            // Load bottoms
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                d.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    d.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(d.duration);
                }
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(d.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(d.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Bottom", null, true);
                        }
                        else
                        {
                            myBottoms.Add(key, d);
                        }
                    }
                    else
                    {
                        myBottoms.Add(key, d);
                    }
                }
            }

            dataLoaded = task.Child("footwear").Children.GetEnumerator();
            // Load footwear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                d.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    d.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(d.duration);
                }
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(d.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(d.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Footwear", null, true);
                        }
                        else
                        {
                            myFootwear.Add(key, d);
                        }
                    }
                    else
                    {
                        myFootwear.Add(key, d);
                    }
                }
            }

            dataLoaded = task.Child("headgear").Children.GetEnumerator();
            // Load headgear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                d.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    d.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(d.duration);
                }
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(d.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(d.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Headgear", null, true);
                        }
                        else
                        {
                            myHeadgear.Add(key, d);
                        }
                    }
                    else
                    {
                        myHeadgear.Add(key, d);
                    }
                }
            }

            dataLoaded = task.Child("facewear").Children.GetEnumerator();
            // Load facewear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                d.name = key;
                float dur = -10f;
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    d.duration = thisSnapshot.Child("duration").Value.ToString();
                    dur = float.Parse(d.duration);
                }
                if (dur != -10f) {
                    if (dur >= 0f)
                    {
                        DateTime acquireDate = DateTime.Parse(d.acquireDate);
                        acquireDate = acquireDate.AddMinutes((double)float.Parse(d.duration));
                        int result = DateTime.Compare(DateTime.Now, acquireDate);
                        if (result >= 0)
                        {
                            DeleteItemFromInventory(key, "Facewear", null, true);
                        }
                        else
                        {
                            myFacewear.Add(key, d);
                        }
                    }
                    else
                    {
                        myFacewear.Add(key, d);
                    }
                }
            }

            dataLoaded = task.Child("mods").Children.GetEnumerator();
            // Load mods
            while (dataLoaded.MoveNext()) {
                ModData m = new ModData();
                string key = dataLoaded.Current.Key;
                MutableData thisSnapshot = dataLoaded.Current;
                m.id = key;
                if (thisSnapshot.Child("name").Value != null) {
                    m.name = thisSnapshot.Child("name").Value.ToString();
                }
                if (thisSnapshot.Child("acquireDate").Value != null) {
                    m.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                }
                if (thisSnapshot.Child("duration").Value != null) {
                    m.duration = thisSnapshot.Child("duration").Value.ToString();
                }
                if (thisSnapshot.Child("equippedOn").Value != null) {
                    m.equippedOn = thisSnapshot.Child("equippedOn").Value.ToString();
                    myMods.Add(key, m);
                }
                // myMods.Add(key, m);
            }
            
            return TransactionResult.Success(task);
        });
    }

    public void FindBodyRef(string character)
    {
        if (reinstantiatePlayerFlag) {
            Destroy(bodyReference);
            bodyReference = null;
        }
        if (bodyReference == null)
        {
            bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[character]]);
        }
        // else
        // {
        //     bodyReference = GameObject.FindGameObjectWithTag("Player");
        // }
    }

    public void ChangeBodyRef(string character, GameObject shopItem, bool previewFlag)
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        if (bodyReference.GetComponent<EquipmentScript>().equippedCharacter == character)
        {
            return;
        }
        WeaponScript weaponScrpt = bodyReference.GetComponent<WeaponScript>();
        Destroy(bodyReference);
        bodyReference = null;
        bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[character]]);
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        if (!previewFlag) {
            bodyReference.GetComponent<EquipmentScript>().HighlightItemPrefab(shopItem);
            PlayerData.playerdata.info.equippedPrimary = weaponScrpt.equippedPrimaryWeapon;
            PlayerData.playerdata.info.equippedSecondary = weaponScrpt.equippedSecondaryWeapon;
            PlayerData.playerdata.info.equippedSupport = weaponScrpt.equippedSupportWeapon;
            PlayerData.playerdata.info.equippedMelee = weaponScrpt.equippedMeleeWeapon;
        }
        characterEquips.EquipCharacter(character, null);
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

        DatabaseReference d = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId);
        d.RunTransaction(task => {
            if (suppressorId != null && !"".Equals(suppressorId)) {
                // Mod was removed
                if (suppressorId != null && !"".Equals(suppressorId) && string.IsNullOrEmpty(equippedSuppressor))
                {
                    newModInfo.equippedSuppressor = "";
                    newModInfo.weaponName = weaponName;
                    newModInfo.suppressorId = "";
                    task.Child("mods").Child(suppressorId).Child("equippedOn").Value = "";
                    task.Child("weapons").Child(weaponName).Child("equippedSuppressor").Value = "";
                    myMods[suppressorId].equippedOn = "";
                }
                else
                {
                    // Mod was added/changed
                    newModInfo.equippedSuppressor = equippedSuppressor;
                    newModInfo.weaponName = weaponName;
                    newModInfo.suppressorId = suppressorId;
                    task.Child("mods").Child(suppressorId).Child("equippedOn").Value = weaponName;
                    task.Child("weapons").Child(weaponName).Child("equippedSuppressor").Value = suppressorId;
                    myMods[suppressorId].equippedOn = weaponName;
                }

                // Set mod data that was just saved
                if (weaponName == myWeps.equippedPrimaryWeapon)
                {
                    PlayerData.playerdata.primaryModInfo.equippedSuppressor = newModInfo.equippedSuppressor;
                    PlayerData.playerdata.primaryModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.primaryModInfo.suppressorId = newModInfo.suppressorId;
                } else if (weaponName == myWeps.equippedSecondaryWeapon)
                {
                    PlayerData.playerdata.secondaryModInfo.equippedSuppressor = newModInfo.equippedSuppressor;
                    PlayerData.playerdata.secondaryModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.secondaryModInfo.suppressorId = newModInfo.suppressorId;
                } else if (weaponName == myWeps.equippedSupportWeapon)
                {
                    PlayerData.playerdata.supportModInfo.equippedSuppressor = newModInfo.equippedSuppressor;
                    PlayerData.playerdata.supportModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.supportModInfo.suppressorId = newModInfo.suppressorId;
                }
            }

            if (sightId != null && !"".Equals(sightId)) {
                if (sightId != null && !"".Equals(sightId) && string.IsNullOrEmpty(equippedSight))
                {
                    newModInfo.equippedSight = "";
                    newModInfo.weaponName = weaponName;
                    newModInfo.sightId = "";
                    task.Child("mods").Child(sightId).Child("equippedOn").Value = "";
                    task.Child("weapons").Child(weaponName).Child("equippedSight").Value = "";
                    myMods[sightId].equippedOn = "";
                }
                else
                {
                    // Mod was added/changed
                    newModInfo.equippedSight = equippedSight;
                    newModInfo.weaponName = weaponName;
                    newModInfo.sightId = sightId;
                    task.Child("mods").Child(sightId).Child("equippedOn").Value = weaponName;
                    task.Child("weapons").Child(weaponName).Child("equippedSight").Value = sightId;
                    myMods[sightId].equippedOn = weaponName;
                }

                // Set mod data that was just saved
                if (weaponName == myWeps.equippedPrimaryWeapon)
                {
                    PlayerData.playerdata.primaryModInfo.equippedSight = newModInfo.equippedSight;
                    PlayerData.playerdata.primaryModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.primaryModInfo.sightId = newModInfo.sightId;
                } else if (weaponName == myWeps.equippedSecondaryWeapon)
                {
                    PlayerData.playerdata.secondaryModInfo.equippedSight = newModInfo.equippedSight;
                    PlayerData.playerdata.secondaryModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.secondaryModInfo.sightId = newModInfo.sightId;
                } else if (weaponName == myWeps.equippedSupportWeapon)
                {
                    PlayerData.playerdata.supportModInfo.equippedSight = newModInfo.equippedSight;
                    PlayerData.playerdata.supportModInfo.weaponName = newModInfo.weaponName;
                    PlayerData.playerdata.supportModInfo.sightId = newModInfo.sightId;
                }
            }

            return TransactionResult.Success(task);
        });
    }

    public ModInfo LoadModDataForWeapon(string weaponName) {
        ModInfo modInfo = new ModInfo();
        modInfo.weaponName = weaponName;

        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.myMods)
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

    public void AddItemToInventory(string itemName, string type, float duration, bool purchased, bool stacking, uint gpCost, uint kashCost) {
        DatabaseReference d = DAOScript.dao.dbRef;
        d.RunTransaction(task => {
            if (type.Equals("Weapon")) {
                // Only update duration if purchasing the item again
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input, bought new)
                    if (myWeapons.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    WeaponData w = new WeaponData();
                    w.name = itemName;
                    w.acquireDate = DateTime.Now.ToString();
                    w.duration = "" + duration;
                    w.equippedClip = "";
                    w.equippedSight = "";
                    w.equippedSuppressor = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("acquireDate").Value = w.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("duration").Value = w.duration;
                    if (type != "Melee") {
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("equippedSuppressor").Value = "";
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("equippedSight").Value = "";
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("equippedClip").Value = "";
                    }
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myWeapons.Add(itemName, w);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myWeapons.Add(itemName, w);
                    }
                } else {
                    WeaponData w = myWeapons[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        w.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Character")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myCharacters.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    CharacterData c = new CharacterData();
                    c.name = itemName;
                    c.acquireDate = DateTime.Now.ToString();
                    c.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters").Child(itemName).Child("acquireDate").Value = c.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters").Child(itemName).Child("duration").Value = c.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myCharacters.Add(itemName, c);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                        addDefaultClothingFlag = itemName;
                    } else
                    {
                        myCharacters.Add(itemName, c);
                    }
                } else
                {
                    CharacterData c = myCharacters[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        c.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Top")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myTops.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    EquipmentData e = new EquipmentData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myTops.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myTops.Add(itemName, e);
                    }
                } else
                {
                    EquipmentData e = myTops[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        e.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Bottom")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myBottoms.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    EquipmentData e = new EquipmentData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myBottoms.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myBottoms.Add(itemName, e);
                    }
                } else
                {
                    EquipmentData e = myBottoms[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        e.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Armor")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myArmor.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    ArmorData e = new ArmorData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myArmor.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myArmor.Add(itemName, e);
                    }
                } else
                {
                    ArmorData a = myArmor[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        a.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Footwear")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myFootwear.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    EquipmentData e = new EquipmentData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myFootwear.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myFootwear.Add(itemName, e);
                    }
                } else
                {
                    EquipmentData e = myFootwear[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        e.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Headgear")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myHeadgear.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    EquipmentData e = new EquipmentData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myHeadgear.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myHeadgear.Add(itemName, e);
                    }
                } else
                {
                    EquipmentData e = myHeadgear[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        e.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Facewear")) {
                if (!stacking)
                {
                    // If user already has item, then don't do anything (if stacking extra time wasn't input)
                    if (myFacewear.ContainsKey(itemName))
                    {
                        return TransactionResult.Success(task);
                    }
                    EquipmentData e = new EquipmentData();
                    e.name = itemName;
                    e.acquireDate = DateTime.Now.ToString();
                    e.duration = "" + duration;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear").Child(itemName).Child("acquireDate").Value = e.acquireDate;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear").Child(itemName).Child("duration").Value = e.duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        myFacewear.Add(itemName, e);
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    } else
                    {
                        myFacewear.Add(itemName, e);
                    }
                } else
                {
                    EquipmentData e = myFacewear[itemName];
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear").Child(itemName).Child("duration").Value = ""+duration;
                    if (purchased)
                    {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                        task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                        e.duration = ""+duration;
                        PlayerData.playerdata.info.gp = gpDiff;
                        updateCurrencyFlag = true;
                        purchaseSuccessfulFlag = true;
                    }
                }
            } else if (type.Equals("Mod")) {
                ModData m = new ModData();
                m.name = itemName;
                m.acquireDate = DateTime.Now.ToString();
                m.duration = ""+duration;
                m.equippedOn = "";
                DatabaseReference de = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Push();
                if (de == null) {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    return TransactionResult.Success(task);
                }
                string pushKey = de.Key;
                m.id = pushKey;
                task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("name").Value = m.name;
                task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("equippedOn").Value = m.equippedOn;
                task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("acquireDate").Value = m.acquireDate;
                task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("duration").Value = m.duration;
                if (purchased) {
                    uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "" + gpDiff;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "" + (PlayerData.playerdata.info.kash - kashCost);
                    myMods.Add(pushKey, m);
                    PlayerData.playerdata.info.gp = gpDiff;
                    updateCurrencyFlag = true;
                    purchaseSuccessfulFlag = true;
                } else
                {
                    myMods.Add(pushKey, m);
                }
            }

            return TransactionResult.Success(task);
        });
    }

    // Removes item from inventory in DB
    public void DeleteItemFromInventory(string itemName, string type, string modId, bool expiring)
    {
        DatabaseReference d = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId);
        if (type.Equals("Weapon"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.weaponCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                // Get any mods that are attached to it and unattach those as well
                object equippedSuppressorId = task.Child("weapons").Child(itemName).Child("equippedSuppressor").Value;
                object equippedSightId = task.Child("weapons").Child(itemName).Child("equippedSight").Value;
                // Deletes item in DB
                task.Child("weapons").Child(itemName).Value = null;
                // Delete item locally
                // Unattach locally, and then save
                if (equippedSuppressorId != null) {
                    string equippedSuppressorIdText = equippedSuppressorId.ToString();
                    try {
                        myMods[equippedSuppressorIdText].equippedOn = "";
                    } catch (KeyNotFoundException e) {
                        Debug.Log("Mod " + equippedSuppressorIdText + " was not loaded locally yet... skipping");
                    }
                    // Save in DB
                    Debug.Log("Deleting: " + equippedSuppressorIdText + " off of weapon " + itemName);
                    task.Child("mods").Child(equippedSuppressorIdText).Child("equippedOn").Value = "";
                    // Debug.Log("Suppressor removed.");
                }
                if (equippedSightId != null) {
                    string equippedSightIdText = equippedSightId.ToString();
                    try {
                        myMods[equippedSightIdText].equippedOn = "";
                    } catch (KeyNotFoundException e) {
                        Debug.Log("Mod " + equippedSightIdText + " was not loaded locally yet... skipping");
                    }
                    // Save in DB
                    Debug.Log("Deleting: " + equippedSightIdText + " off of weapon " + itemName);
                    task.Child("mods").Child(equippedSightIdText).Child("equippedOn").Value = "";
                    // Debug.Log("Sight removed.");
                }
                // Debug.Log("Mods handled.");
                myWeapons.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                if (PlayerData.playerdata.info.equippedPrimary == itemName)
                {
                    PlayerData.playerdata.info.equippedPrimary = PlayerData.playerdata.info.defaultWeapon;
                    //PlayerData.playerdata.primaryModInfo = LoadModDataForWeapon(DEFAULT_PRIMARY);
                    reloadPlayerFlag = true;
                } else if (PlayerData.playerdata.info.equippedSecondary == itemName)
                {
                    PlayerData.playerdata.info.equippedSecondary = DEFAULT_SECONDARY;
                    //PlayerData.playerdata.secondaryModInfo = LoadModDataForWeapon(DEFAULT_SECONDARY);
                    reloadPlayerFlag = true;
                } else if (PlayerData.playerdata.info.equippedSupport == itemName)
                {
                    PlayerData.playerdata.info.equippedSupport = DEFAULT_SUPPORT;
                    //PlayerData.playerdata.supportModInfo = LoadModDataForWeapon(DEFAULT_SUPPORT);
                    reloadPlayerFlag = true;
                } else if (PlayerData.playerdata.info.equippedMelee == itemName)
                {
                    PlayerData.playerdata.info.equippedMelee = DEFAULT_MELEE;
                    //PlayerData.playerdata.supportModInfo = LoadModDataForWeapon(DEFAULT_SUPPORT);
                    reloadPlayerFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Character"))
        {
            d.RunTransaction(task => {
                task.Child("characters").Child(itemName).Value = null;
                // Delete item locally
                myCharacters.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedCharacter == itemName)
                {
                    PlayerData.playerdata.info.equippedCharacter = PlayerData.playerdata.info.defaultChar;
                    PlayerData.playerdata.info.equippedTop = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultTop;
                    PlayerData.playerdata.info.equippedBottom = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultBottom;
                    reinstantiatePlayerFlag = true;
                    reloadPlayerFlag = true;
                }
                deleteDefaultClothingFlag = itemName;
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Top"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                task.Child("tops").Child(itemName).Value = null;
                // Delete item locally
                myTops.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedTop == itemName)
                {
                    PlayerData.playerdata.info.equippedTop = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultTop;
                    reloadPlayerFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Bottom"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                task.Child("bottoms").Child(itemName).Value = null;
                // Delete item locally
                myBottoms.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedBottom == itemName)
                {
                    PlayerData.playerdata.info.equippedBottom = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultBottom;
                    reloadPlayerFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Armor"))
        {
            d.RunTransaction(task => {
                task.Child("armor").Child(itemName).Value = null;
                // Delete item locally
                myArmor.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedArmor == itemName)
                {
                    PlayerData.playerdata.info.equippedArmor = "";
                    reloadPlayerFlag = true;
                    skipReloadCharacterFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Footwear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                task.Child("footwear").Child(itemName).Value = null;
                // Delete item locally
                myFootwear.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedFootwear == itemName)
                {
                    char g = InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender;
                    PlayerData.playerdata.info.equippedFootwear = (g == 'M' ? DEFAULT_FOOTWEAR_MALE : DEFAULT_FOOTWEAR_FEMALE); 
                    reloadPlayerFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Headgear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                task.Child("headgear").Child(itemName).Value = null;
                // Delete item locally
                myHeadgear.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedHeadgear == itemName)
                {
                    PlayerData.playerdata.info.equippedHeadgear = "";
                    reloadPlayerFlag = true;
                    skipReloadCharacterFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Facewear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            d.RunTransaction(task => {
                task.Child("facewear").Child(itemName).Value = null;
                // Delete item locally
                myFacewear.Remove(itemName);
                if (expiring)
                {
                    itemsExpired.Add(itemName);
                }
                Debug.Log(itemName + " has been deleted!");
                // Equip default if item currently equipped
                if (PlayerData.playerdata.info.equippedFacewear == itemName)
                {
                    PlayerData.playerdata.info.equippedFacewear = "";
                    reloadPlayerFlag = true;
                    skipReloadCharacterFlag = true;
                }
                return TransactionResult.Success(task);
            });
        }
        else if (type.Equals("Mod"))
        {
            d.RunTransaction(task => {
                task.Child("mods").Child(modId).Value = null;
                // Get weapon that the mod was equipped on
                ModData m = myMods[modId];
                string weaponName = m.equippedOn;
                // If the mod was equipped to a weapon, unequip it from that weapon first and save
                if (weaponName != null && !"".Equals(weaponName))
                {
                    // Delete locally and in DB
                    string childModType = "";
                    if (InventoryScript.itemData.modCatalog[m.name].category == "Suppressor")
                    {
                        childModType = "equippedSuppressor";
                        myWeapons[weaponName].equippedSuppressor = "";
                    } else if (InventoryScript.itemData.modCatalog[m.name].category == "Sight")
                    {
                        childModType = "equippedSight";
                        myWeapons[weaponName].equippedSight = "";
                    } else if (InventoryScript.itemData.modCatalog[m.name].category == "Clip")
                    {
                        childModType = "equippedClip";
                        myWeapons[weaponName].equippedClip = "";
                    }
                    // Remove attachment from weapon
                    task.Child("weapons").Child(weaponName).Child(childModType).Value = "";
                    Debug.Log(itemName + " was removed from " + weaponName + " since it was deleted from your inventory.");
                }
                // Delete item locally
                myMods.Remove(modId);
                Debug.Log(itemName + " has been deleted!");
                return TransactionResult.Success(task);
            });
        }
    }

    public void AddExpAndGpToPlayer(uint aExp, uint aGp) {
        // Save locally
        PlayerData.playerdata.info.exp = (uint)Mathf.Min(PlayerData.playerdata.info.exp + aExp, PlayerData.MAX_EXP);
        PlayerData.playerdata.info.gp = (uint)Mathf.Min(PlayerData.playerdata.info.gp + aGp, PlayerData.MAX_GP);
        // Save to DB
        DatabaseReference d = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
        d.RunTransaction(task => {
            task.Child("exp").Value = "" + PlayerData.playerdata.info.exp;
            task.Child("gp").Value = "" + PlayerData.playerdata.info.gp;
            return TransactionResult.Success(task);
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

    IEnumerator EmergencyExitGame() {
        yield return new WaitForSeconds(5f);
        Application.Quit();
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
