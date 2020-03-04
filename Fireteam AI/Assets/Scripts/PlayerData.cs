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
    private const string DEFAULT_PRIMARY = "M4A1";
    private const string DEFAULT_SECONDARY = "Glock23";
    private const string DEFAULT_SUPPORT = "M67 Frag";
    private const string DEFAULT_FOOTWEAR_MALE = "Standard Boots (M)";
    private const string DEFAULT_FOOTWEAR_FEMALE = "Standard Boots (F)";

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
    public Dictionary<string, EquipmentData> myHeadgear;
    public Dictionary<string, EquipmentData> myTops;
    public Dictionary<string, EquipmentData> myBottoms;
    public Dictionary<string, EquipmentData> myFacewear;
    public Dictionary<string, EquipmentData> myFootwear;
    public Dictionary<string, ArmorData> myArmor;
    public Dictionary<string, WeaponData> myWeapons;
    public Dictionary<string, CharacterData> myCharacters;
    public Dictionary<string, ModData> myMods;

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
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Codename Sayre")) {
            characterPrefabName = "SayreGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Hana")) {
            characterPrefabName = "HanaGamePrefab";
        } else if (PlayerData.playerdata.info.equippedCharacter.Equals("Jade")) {
            characterPrefabName = "JadeGamePrefab";
        }
        return characterPrefabName;
    }

    public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName.Equals("BetaLevelNetwork"))
        {
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
                if (PlayerData.playerdata.bodyReference == null)
                {
                    LoadPlayerData();
                }
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

        string saveJson = "{" +
            "\"equippedCharacter\":\"" + PlayerData.playerdata.info.equippedCharacter + "\"," +
            "\"equippedPrimary\":\"" + PlayerData.playerdata.info.equippedPrimary + "\"," +
            "\"equippedSecondary\":\"" + PlayerData.playerdata.info.equippedSecondary + "\"," +
            "\"equippedSupport\":\"" + PlayerData.playerdata.info.equippedSupport + "\"," +
            "\"equippedTop\":\"" + PlayerData.playerdata.info.equippedTop + "\"," +
            "\"equippedBottom\":\"" + PlayerData.playerdata.info.equippedBottom + "\"," +
            "\"equippedFootwear\":\"" + PlayerData.playerdata.info.equippedFootwear + "\"," +
            "\"equippedFacewear\":\"" + PlayerData.playerdata.info.equippedFacewear + "\"," +
            "\"equippedHeadgear\":\"" + PlayerData.playerdata.info.equippedHeadgear + "\"," +
            "\"equippedArmor\":\"" + PlayerData.playerdata.info.equippedArmor + "\"" +
        "}";
        DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("equipment")
            .SetRawJsonValueAsync(saveJson).ContinueWith(task => {
                if (task.IsCompleted) {
                    Debug.Log("Player data saved successfully.");
                }
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
                titleRef.CloseGameOnError();
            } else {
                info.defaultChar = snapshot.Child("defaultChar").Value.ToString();
                info.playername = snapshot.Child("username").Value.ToString();
                info.exp = float.Parse(snapshot.Child("exp").Value.ToString());
                info.gp = uint.Parse(snapshot.Child("gp").Value.ToString());
                info.kash = uint.Parse(snapshot.Child("kash").Value.ToString());
                // Equip previously equipped if available. Else, equip defaults and save it
                if (snapshot.HasChild("equipment")) {
                    DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).GetValueAsync().ContinueWith(taskA => {
                        DataSnapshot inventorySnapshot = taskA.Result;
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

                        DataSnapshot modsInventory = inventorySnapshot.Child("mods");

                        DataSnapshot modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedPrimary);
                        string modId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                        primaryModInfo.weaponName = info.equippedPrimary;
                        primaryModInfo.id = modId;
                        if (!"".Equals(modId)) {
                            primaryModInfo.equippedSuppressor = modsInventory.Child(modId).Child("name").Value.ToString();
                        }

                        modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedSecondary);
                        modId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                        secondaryModInfo.weaponName = info.equippedSecondary;
                        secondaryModInfo.id = modId;
                        if (!"".Equals(modId)) {
                            secondaryModInfo.equippedSuppressor = modsInventory.Child(modId).Child("name").Value.ToString();
                        }

                        modSnapshot = inventorySnapshot.Child("weapons").Child(info.equippedSupport);
                        modId = modSnapshot.Child("equippedSuppressor").Value.ToString();
                        supportModInfo.weaponName = info.equippedSupport;
                        supportModInfo.id = modId;
                        if (!"".Equals(modId)) {
                            supportModInfo.equippedSuppressor = modsInventory.Child(modId).Child("name").Value.ToString();
                        }
                        dataLoadedFlag = true;
                        updateCurrencyFlag = true;
                    });
                } else {
                    info.equippedCharacter = snapshot.Child("defaultChar").Value.ToString();
                    char g = InventoryScript.itemData.characterCatalog[info.equippedCharacter].gender;
                    info.equippedPrimary = DEFAULT_PRIMARY;
                    info.equippedSecondary = DEFAULT_SECONDARY;
                    info.equippedSupport = DEFAULT_SUPPORT;
                    info.equippedTop = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultTop;
                    info.equippedBottom = InventoryScript.itemData.characterCatalog[info.equippedCharacter].defaultBottom;
                    info.equippedFootwear = (g == 'M' ? DEFAULT_FOOTWEAR_MALE : DEFAULT_FOOTWEAR_FEMALE);
                    info.equippedFacewear = "";
                    info.equippedHeadgear = "";
                    info.equippedArmor = "";

                    primaryModInfo.equippedSuppressor = "";
                    primaryModInfo.weaponName = "";
                    primaryModInfo.id = "";

                    secondaryModInfo.equippedSuppressor = "";
                    secondaryModInfo.weaponName = "";
                    secondaryModInfo.id = "";

                    supportModInfo.equippedSuppressor = "";
                    supportModInfo.weaponName = "";
                    supportModInfo.id = "";
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
        characterWeps.EquipWeapon(info.equippedPrimary, primaryModInfo.equippedSuppressor, null);
        characterWeps.EquipWeapon(info.equippedSecondary, secondaryModInfo.equippedSuppressor, null);
        characterWeps.EquipWeapon(info.equippedSupport, supportModInfo.equippedSuppressor, null);
        PhotonNetwork.NickName = playername;
    }

    public void ReinstantiatePlayer()
    {
        InstantiatePlayer();
        saveDataFlag = true;
    }

    public void LoadInventory() {
        DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).GetValueAsync().ContinueWith(task => {
            DataSnapshot snapshot = task.Result;
            DataSnapshot subSnapshot = snapshot.Child("weapons");
            IEnumerator<DataSnapshot> dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load weapons
            while (dataLoaded.MoveNext()) {
                WeaponData w = new WeaponData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                w.name = key;
                w.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                w.duration = thisSnapshot.Child("duration").Value.ToString();
                w.equippedSuppressor = thisSnapshot.Child("equippedSuppressor").Value.ToString();
                w.equippedClip = thisSnapshot.Child("equippedClip").Value.ToString();
                w.equippedSight = thisSnapshot.Child("equippedSight").Value.ToString();
                // If item is expired, delete from database. Else, add it to inventory
                float dur = float.Parse(w.duration);
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
            
            subSnapshot = snapshot.Child("characters");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load characters
            while (dataLoaded.MoveNext()) {
                CharacterData c = new CharacterData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                c.name = key;
                c.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                c.duration = thisSnapshot.Child("duration").Value.ToString();
                // If item is expired, delete from database. Else, add it to inventory
                float dur = float.Parse(c.duration);
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

            subSnapshot = snapshot.Child("armor");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load armor
            while (dataLoaded.MoveNext()) {
                ArmorData a = new ArmorData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                a.name = key;
                a.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                a.duration = thisSnapshot.Child("duration").Value.ToString();
                // If item is expired, delete from database. Else, add it to inventory
                float dur = float.Parse(a.duration);
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

            subSnapshot = snapshot.Child("tops");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load tops
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                // If item is expired, delete from database. Else, add it to inventory
                float dur = float.Parse(d.duration);
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

            subSnapshot = snapshot.Child("bottoms");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load bottoms
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                float dur = float.Parse(d.duration);
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

            subSnapshot = snapshot.Child("footwear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load footwear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                float dur = float.Parse(d.duration);
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

            subSnapshot = snapshot.Child("headgear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load headgear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                float dur = float.Parse(d.duration);
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

            subSnapshot = snapshot.Child("facewear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load facewear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                float dur = float.Parse(d.duration);
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

            subSnapshot = snapshot.Child("mods");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load mods
            while (dataLoaded.MoveNext()) {
                ModData m = new ModData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current;
                m.id = key;
                m.name = thisSnapshot.Child("name").Value.ToString();
                m.acquireDate = thisSnapshot.Child("acquireDate").Value.ToString();
                m.duration = thisSnapshot.Child("duration").Value.ToString();
                m.equippedOn = thisSnapshot.Child("equippedOn").Value.ToString();
                myMods.Add(key, m);
            }
        });
    }

    public void FindBodyRef(string character)
    {
        if (bodyReference == null)
        {
            bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[character].prefabPath));
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
        bodyReference = Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[character].prefabPath));
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        if (!previewFlag) {
            bodyReference.GetComponent<EquipmentScript>().HighlightItemPrefab(shopItem);
            PlayerData.playerdata.info.equippedPrimary = weaponScrpt.equippedPrimaryWeapon;
            PlayerData.playerdata.info.equippedSecondary = weaponScrpt.equippedSecondaryWeapon;
            PlayerData.playerdata.info.equippedSupport = weaponScrpt.equippedSupportWeapon;
        }
        characterEquips.EquipCharacter(character, null);
    }

    // Saves mod data for given weapon. If ID is null, then that means there was no mod on that weapon to begin with when it was saved.
    // Therefore, don't do anything.
    // If the ID is not null but the equippedSuppressor is, then that means that a suppressor was unequipped from a weapon.
    // Therefore, set the equipped on for the mod to empty string and set the equippedSuppressor for the weapon to empty string.
    public void SaveModDataForWeapon(string weaponName, string equippedSuppressor, string id) {
        //Debug.Log("Data passed in: " + weaponName + ", " + equippedSuppressor + ", " + id);
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        ModInfo newModInfo = new ModInfo();

        // Mod was removed
        if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(equippedSuppressor))
        {
            newModInfo.equippedSuppressor = "";
            newModInfo.weaponName = weaponName;
            newModInfo.id = "";
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                .Child("mods").Child(id).Child("equippedOn").SetValueAsync("");
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                .Child(weaponName).Child("equippedSuppressor").SetValueAsync("");
            myMods[id].equippedOn = "";
        }
        else
        {
            // Mod was added/changed
            newModInfo.equippedSuppressor = equippedSuppressor;
            newModInfo.weaponName = weaponName;
            newModInfo.id = id;
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                .Child("mods").Child(id).Child("equippedOn").SetValueAsync(weaponName);
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                .Child(weaponName).Child("equippedSuppressor").SetValueAsync(id);
            myMods[id].equippedOn = weaponName;
        }

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
        ModInfo modInfo = new ModInfo();
        modInfo.weaponName = weaponName;

        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.myMods)
        {
            // If the mod is equipped on the given weapon, load it into the requested mod info
            ModData m = entry.Value;
            if (m.equippedOn.Equals(weaponName)) {
                Mod modDetails = InventoryScript.itemData.modCatalog[m.name];
                modInfo.id = m.id;
                if (modDetails.category.Equals("Suppressor")) {
                    modInfo.equippedSuppressor = m.name;
                } else if (modDetails.category.Equals("Sight")) {
                    modInfo.equippedSight = m.name;
                } else if (modDetails.category.Equals("Clip")) {
                    modInfo.equippedClip = m.name;
                }
            }
        }
        
        return modInfo;
    }

    public void AddItemToInventory(string itemName, string type, float duration, bool purchased, bool stacking, uint gpCost, uint kashCost) {
        string json = "";
        if (type.Equals("Weapon")) {
            // Only update duration if purchasing the item again
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myWeapons.ContainsKey(itemName))
                {
                    return;
                }
                WeaponData w = new WeaponData();
                w.name = itemName;
                w.acquireDate = DateTime.Now.ToString();
                w.duration = "" + duration;
                w.equippedClip = "";
                w.equippedSight = "";
                w.equippedSuppressor = "";
                json = "{" +
                    "\"acquireDate\":\"" + w.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"," +
                    "\"equippedSuppressor\":\"\"," +
                    "\"equippedSight\":\"\"," +
                    "\"equippedClip\":\"\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myWeapons.Add(itemName, w);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myWeapons.Add(itemName, w);
                            }
                        }
                    });
            } else
            {
                WeaponData w = myWeapons[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        w.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Character")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myCharacters.ContainsKey(itemName))
                {
                    return;
                }
                CharacterData c = new CharacterData();
                c.name = itemName;
                c.acquireDate = DateTime.Now.ToString();
                c.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + c.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myCharacters.Add(itemName, c);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                        addDefaultClothingFlag = itemName;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myCharacters.Add(itemName, c);
                            }
                        }
                    });
            } else
            {
                CharacterData c = myCharacters[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        c.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Top")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myTops.ContainsKey(itemName))
                {
                    return;
                }
                EquipmentData e = new EquipmentData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myTops.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myTops.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                EquipmentData e = myTops[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        e.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Bottom")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myBottoms.ContainsKey(itemName))
                {
                    return;
                }
                EquipmentData e = new EquipmentData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myBottoms.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myBottoms.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                EquipmentData e = myBottoms[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        e.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Armor")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myArmor.ContainsKey(itemName))
                {
                    return;
                }
                ArmorData e = new ArmorData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myArmor.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myArmor.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                ArmorData a = myArmor[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        a.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Footwear")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myFootwear.ContainsKey(itemName))
                {
                    return;
                }
                EquipmentData e = new EquipmentData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myFootwear.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myFootwear.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                EquipmentData e = myFootwear[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        e.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Headgear")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myHeadgear.ContainsKey(itemName))
                {
                    return;
                }
                EquipmentData e = new EquipmentData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myHeadgear.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myHeadgear.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                EquipmentData e = myHeadgear[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        e.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Facewear")) {
            if (!stacking)
            {
                // If user already has item, then don't do anything (if stacking extra time wasn't input)
                if (myFacewear.ContainsKey(itemName))
                {
                    return;
                }
                EquipmentData e = new EquipmentData();
                e.name = itemName;
                e.acquireDate = DateTime.Now.ToString();
                e.duration = "" + duration;
                json = "{" +
                    "\"acquireDate\":\"" + e.acquireDate + "\"," +
                    "\"duration\":\"" + duration + "\"" +
                "}";
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear")
                    .Child(itemName).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        myFacewear.Add(itemName, e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        } else
                        {
                            if (!task.IsCanceled && !task.IsFaulted)
                            {
                                myFacewear.Add(itemName, e);
                            }
                        }
                        
                    });
            } else
            {
                EquipmentData e = myFacewear[itemName];
                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear")
                    .Child(itemName).Child("duration").SetValueAsync(duration).ContinueWith(task =>
                    {
                        if (purchased)
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                purchaseFailFlag = true;
                            }
                            else
                            {
                                uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                                DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                                userInfoRef.Child("gp").SetValueAsync("" + gpDiff).ContinueWith(taskA =>
                                {
                                    userInfoRef.Child("kash").SetValueAsync("" + (PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB =>
                                    {
                                        e.duration = ""+duration;
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            }
        } else if (type.Equals("Mod")) {
            ModData m = new ModData();
            m.name = itemName;
            m.acquireDate = DateTime.Now.ToString();
            m.duration = ""+duration;
            m.equippedOn = "";
            json = "{" +
                "\"name\":\"" + itemName + "\"," +
                "\"equippedOn\":\"\"," +
                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                "\"duration\":\"-1\"" +
            "}";
            DatabaseReference d = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Push();
            string pushKey = d.Key;
            m.id = pushKey;
            d.SetRawJsonValueAsync(json).ContinueWith(task => {
                if (purchased) {
                    if (task.IsFaulted || task.IsCanceled) {
                        purchaseFailFlag = true;
                    } else {
                        uint gpDiff = PlayerData.playerdata.info.gp - gpCost;
                        DatabaseReference userInfoRef = DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId);
                            userInfoRef.Child("gp").SetValueAsync(""+gpDiff).ContinueWith(taskA => {
                                userInfoRef.Child("kash").SetValueAsync(""+(PlayerData.playerdata.info.kash - kashCost)).ContinueWith(taskB => {
                                    myMods.Add(pushKey, m);
                                    PlayerData.playerdata.info.gp = gpDiff;
                                    updateCurrencyFlag = true;
                                    purchaseSuccessfulFlag = true;
                                });
                            });
                    }
                } else
                {
                    if (!task.IsCanceled && !task.IsFaulted)
                    {
                        myMods.Add(pushKey, m);
                    }
                }
                
            });
        }
    }

    // Removes item from inventory in DB
    public void DeleteItemFromInventory(string itemName, string type, string modId, bool expiring)
    {
        if (type.Equals("Weapon"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.weaponCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
                        // Delete item locally
                        myWeapons.Remove(itemName);
                        if (expiring)
                        {
                            itemsExpired.Add(itemName);
                        }
                        Debug.Log(itemName + " has been deleted!");
                        if (PlayerData.playerdata.info.equippedPrimary == itemName)
                        {
                            PlayerData.playerdata.info.equippedPrimary = DEFAULT_PRIMARY;
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
                        }
                    }
                });
        }
        else if (type.Equals("Character"))
        {
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    } else
                    {
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
                            reloadPlayerFlag = true;
                        }
                        deleteDefaultClothingFlag = itemName;
                    }
                });
        }
        else if (type.Equals("Top"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Bottom"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Armor"))
        {
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("armor")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Footwear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("footwear")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Headgear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("headgear")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Facewear"))
        {
            // If item cannot be deleted, then skip
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable)
            {
                return;
            }
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("facewear")
                .Child(itemName).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
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
                    }
                });
        }
        else if (type.Equals("Mod"))
        {
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods")
                .Child(modId).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log(itemName + " could not be deleted!");
                    }
                    else
                    {
                        // Get weapon that the mod was equipped on
                        ModData m = myMods[modId];
                        string weaponName = m.equippedOn;
                        // If the mod was equipped to a weapon, unequip it from that weapon first and save
                        if (weaponName == null || !"".Equals(weaponName))
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
                            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                                .Child(weaponName).Child(childModType).SetValueAsync("").ContinueWith(taskA =>
                                {
                                    if (taskA.IsFaulted || taskA.IsCanceled)
                                    {
                                        Debug.Log(itemName + " could not be deleted when removing from weapon " + weaponName + "!");
                                    } else
                                    {
                                        Debug.Log(itemName + " was removed from " + weaponName + " since it was deleted from your inventory.");
                                        myMods.Remove(modId);
                                    }
                                });
                        } else
                        {
                            // Delete item locally
                            myMods.Remove(modId);
                            Debug.Log(itemName + " has been deleted!");
                        }
                    }
                });
        }
    }

}

public class PlayerInfo
{
    public string defaultChar;
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
    public float exp;
    public uint gp;
    public uint kash;
}

public class ModInfo
{
    public string id;
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
