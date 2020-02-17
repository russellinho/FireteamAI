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
                playername = snapshot.Child("username").Value.ToString();
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
                }
            }
        });
    }

    void InstantiatePlayer() {
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
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                w.name = key;
                w.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
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
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                c.name = key;
                c.acquireData = thisSnapshot.Child("acquireData").Value.ToString();
                c.duration = thisSnapshot.Child("duration").Value.ToString();
                myCharacters.Add(c);
            }

            subSnapshot = snapshot.Child("armor");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load armor
            while (dataLoaded.MoveNext()) {
                ArmorData a = new ArmorData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                a.name = key;
                a.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                a.duration = thisSnapshot.Child("duration").Value.ToString();
                myArmor.Add(a);
            }

            subSnapshot = snapshot.Child("tops");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load tops
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                myTops.Add(d);
            }

            subSnapshot = snapshot.Child("bottoms");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load bottoms
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                myBottoms.Add(d);
            }

            subSnapshot = snapshot.Child("footwear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load footwear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                myFootwear.Add(d);
            }

            subSnapshot = snapshot.Child("headgear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load headgear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                myHeadgear.Add(d);
            }

            subSnapshot = snapshot.Child("facewear");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load facewear
            while (dataLoaded.MoveNext()) {
                EquipmentData d = new EquipmentData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                d.name = key;
                d.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                d.duration = thisSnapshot.Child("duration").Value.ToString();
                myFacewear.Add(d);
            }

            subSnapshot = snapshot.Child("mods");
            dataLoaded = subSnapshot.Children.GetEnumerator();
            // Load mods
            while (dataLoaded.MoveNext()) {
                ModData m = new ModData();
                string key = dataLoaded.Current.Key;
                DataSnapshot thisSnapshot = dataLoaded.Current.Child(key);
                m.id = key;
                m.name = thisSnapshot.Child("name").Value.ToString();
                m.acquireDate = thisSnapshot.Child("acquireData").Value.ToString();
                m.duration = thisSnapshot.Child("duration").Value.ToString();
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

    public void SaveModDataForWeapon(string weaponName, string equippedSuppressor, string id, bool removeFlag) {
        ModInfo newModInfo = new ModInfo();
        newModInfo.equippedSuppressor = equippedSuppressor;
        newModInfo.weaponName = weaponName;
        newModInfo.id = id;

        if (removeFlag) {
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                .Child("mods").Child(id).Child("equippedOn").SetValueAsync("");
            DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                .Child(weaponName).Child("equippedSuppressor").SetValueAsync("");
        } else {
            DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                .Child("mods").Child(id).Child("equippedOn").SetValueAsync(weaponName);
            DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("weapons")
                .Child(weaponName).Child("equippedSuppressor").SetValueAsync(id);
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
    public string acquireData;
    public string duration;
}
