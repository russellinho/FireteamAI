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
            this.myHeadgear = new ArrayList();
            this.myTops = new ArrayList();
            this.myBottoms = new ArrayList();
            this.myFacewear = new ArrayList();
            this.myFootwear = new ArrayList();
            this.myArmor = new ArrayList();
            this.myWeapons = new ArrayList();
            this.myCharacters = new ArrayList();
            this.myMods = new ArrayList();
            playerdata = this;
            LoadPlayerData();
            LoadInventory();
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }
        else if (playerdata != this)
        {
            Destroy(gameObject);
        }

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
                info.playername = snapshot.Child("username").Value.ToString();
                info.exp = float.Parse(snapshot.Child("exp").Value.ToString());
                info.gp = uint.Parse(snapshot.Child("gp").Value.ToString());
                info.kcoin = uint.Parse(snapshot.Child("kcoin").Value.ToString());
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
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        characterEquips.EquipCharacter(info.equippedCharacter, null);
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
                myWeapons.Add(w);
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
                myCharacters.Add(c);
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
                myArmor.Add(a);
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
                myTops.Add(d);
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
                myBottoms.Add(d);
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
                myFootwear.Add(d);
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
                myHeadgear.Add(d);
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
                myFacewear.Add(d);
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
                myMods.Add(m);
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

    public void SaveModDataForWeapon(string weaponName, string equippedSuppressor, string id) {
        ModInfo newModInfo = new ModInfo();
        newModInfo.equippedSuppressor = equippedSuppressor;
        newModInfo.weaponName = weaponName;
        newModInfo.id = id;

        DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
            .Child("mods").Child(id).Child("equippedOn").SetValueAsync(weaponName);
        DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons")
            .Child(weaponName).Child("equippedSuppressor").SetValueAsync(id);

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
        
        for (int i = 0; i < myMods.Count; i++) {
            // If the mod is equipped on the given weapon, load it into the requested mod info
            ModData m = (ModData)myMods[i];
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

    public void AddItemToInventory(string itemName, string type, float duration, bool purchased, bool stacking, uint gpCost, uint kCoinCost) {
        string json = "";
        if (type.Equals("Weapon")) {
            // Only update duration if purchasing the item again
            if (!stacking)
            {
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myWeapons.Add(w);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                WeaponData w = GetExistingWeaponDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myCharacters.Add(c);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                CharacterData c = GetExistingCharacterDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myTops.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                EquipmentData e = GetExistingTopDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myBottoms.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                EquipmentData e = GetExistingBottomDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myArmor.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                ArmorData a = GetExistingArmorDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myFootwear.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                EquipmentData e = GetExistingFootwearDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myHeadgear.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                EquipmentData e = GetExistingHeadgearDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
                                    {
                                        myFacewear.Add(e);
                                        PlayerData.playerdata.info.gp = gpDiff;
                                        updateCurrencyFlag = true;
                                        purchaseSuccessfulFlag = true;
                                    });
                                });
                            }
                        }
                    });
            } else
            {
                EquipmentData e = GetExistingFacewearDataForName(itemName);
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
                                    userInfoRef.Child("kcoin").SetValueAsync("" + (PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB =>
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
                "\"duration\":\"" + duration + "\"" +
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
                                userInfoRef.Child("kcoin").SetValueAsync(""+(PlayerData.playerdata.info.kcoin - kCoinCost)).ContinueWith(taskB => {
                                    myMods.Add(m);
                                    PlayerData.playerdata.info.gp = gpDiff;
                                    updateCurrencyFlag = true;
                                    purchaseSuccessfulFlag = true;
                                });
                            });
                    }
                }
            });
        }
    }

    public WeaponData GetExistingWeaponDataForName(string name)
    {
        foreach (WeaponData w in myWeapons)
        {
            if (w.name == name)
            {
                return w;
            }
        }
        return null;
    }

    public CharacterData GetExistingCharacterDataForName(string name)
    {
        foreach (CharacterData c in myCharacters)
        {
            if (c.name == name)
            {
                return c;
            }
        }
        return null;
    }

    public EquipmentData GetExistingTopDataForName(string name)
    {
        foreach (EquipmentData e in myTops)
        {
            if (e.name == name)
            {
                return e;
            }
        }
        return null;
    }

    public EquipmentData GetExistingBottomDataForName(string name)
    {
        foreach (EquipmentData e in myBottoms)
        {
            if (e.name == name)
            {
                return e;
            }
        }
        return null;
    }

    public EquipmentData GetExistingFootwearDataForName(string name)
    {
        foreach (EquipmentData e in myFootwear)
        {
            if (e.name == name)
            {
                return e;
            }
        }
        return null;
    }

    public EquipmentData GetExistingFacewearDataForName(string name)
    {
        foreach (EquipmentData e in myFacewear)
        {
            if (e.name == name)
            {
                return e;
            }
        }
        return null;
    }

    public EquipmentData GetExistingHeadgearDataForName(string name)
    {
        foreach (EquipmentData e in myHeadgear)
        {
            if (e.name == name)
            {
                return e;
            }
        }
        return null;
    }

    public ArmorData GetExistingArmorDataForName(string name)
    {
        foreach (ArmorData a in myArmor)
        {
            if (a.name == name)
            {
                return a;
            }
        }
        return null;
    }

    public ModData GetExistingModDataForName(string name)
    {
        foreach (ModData m in myMods)
        {
            if (m.name == name)
            {
                return m;
            }
        }
        return null;
    }

}

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
    public string equippedSecondary;
    public string equippedSupport;
    public float exp;
    public uint gp;
    public uint kcoin;
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
