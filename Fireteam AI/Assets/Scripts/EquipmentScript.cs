using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class EquipmentScript : MonoBehaviour
{

    public TitleControllerScript ts;
    public PlayerScript playerScript;
    public PlayerActionScript playerActionScript;
    public WeaponScript tws;
    public GameObject fullBodyRef;
    public GameObject firstPersonRef;

    public string equippedCharacter;
    public string equippedHeadgear;
    public string equippedFacewear;
    public string equippedTop;
    public string equippedBottom;
    public string equippedFootwear;
    public string equippedArmor;
    public int equippedSkin;

    public GameObject equippedSkinRef;
    public GameObject equippedHeadgearRef;
    public GameObject equippedFacewearRef;
    public GameObject equippedTopRef;
    public GameObject equippedBottomRef;
    public GameObject equippedFootwearRef;
    public GameObject equippedArmorTopRef;
    public GameObject equippedArmorBottomRef;
    public GameObject equippedFpcSkinRef;
    public GameObject equippedFpcTopRef;

    public GameObject myHeadgearRenderer;
    public GameObject myFacewearRenderer;
    public GameObject myTopRenderer;
    public GameObject myBottomRenderer;
    public GameObject myFootwearRenderer;
    public GameObject myArmorTopRenderer;
    public GameObject myArmorBottomRenderer;
    public GameObject mySkinRenderer;
    public GameObject myGlovesRenderer;
    public GameObject myEyesRenderer;
    public GameObject myEyelashRenderer;
    public GameObject myHairRenderer;
    public GameObject myFpcSkinRenderer;
    public GameObject myFpcTopRenderer;
    public GameObject myFpcGlovesRenderer;

    public bool renderHair;

    public GameObject myBones;
    public GameObject myFpcBones;
    public PhotonView pView;

    private bool onTitle;
    private bool onSetup;
    public string topPrefabSetupPath;
    public string bottomPrefabSetupPath;
    public string footwearPrefabSetupPath;
    public string skinPrefabSetupPath;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name.Equals("Title"))
        {
            onTitle = true;
        }
        else
        {
            onTitle = false;
            if (SceneManager.GetActiveScene().name.Equals("Setup")) {
                onSetup = true;
            }
        }

        if (pView != null) {
            if (pView.IsMine) {
                ToggleFullBody(false);
                ToggleFirstPersonBody(true);
            } else {
                ToggleFullBody(true);
                ToggleFirstPersonBody(false);
            }
        }
    }

    void Start() {
        if (pView != null && !pView.IsMine) {
            return;
        }
        if (onTitle)
        {
            if (ts == null)
            {
                ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            }
        } else if (onSetup) {
            EquipDefaultsForSetup();
        } else {
            pView.RPC("RpcEquipCharacterInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedCharacter);
            pView.RPC("RpcEquipHeadgearInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedHeadgear);
            pView.RPC("RpcEquipFacewearInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedFacewear);
            pView.RPC("RpcEquipTopInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedTop);
            pView.RPC("RpcEquipBottomInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedBottom);
            pView.RPC("RpcEquipFootwearInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedFootwear);
            pView.RPC("RpcEquipArmorInGame", RpcTarget.AllBuffered, PlayerData.playerdata.info.equippedArmor);
        }
    }

    public void ToggleFirstPersonBody(bool b) {
        firstPersonRef.SetActive(b);
    }

    public void ToggleFullBody(bool b) {
        fullBodyRef.SetActive(b);
    }

    public void ToggleMesh(bool b) {
        myEyesRenderer.GetComponent<SkinnedMeshRenderer>().enabled = b;
        myEyelashRenderer.GetComponent<SkinnedMeshRenderer>().enabled = b;
        myGlovesRenderer.GetComponent<SkinnedMeshRenderer>().enabled = b;
        equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        if (renderHair) {
            myHairRenderer.GetComponent<SkinnedMeshRenderer>().enabled = b;
        }
        if (equippedFacewearRef != null) {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        }
        if (equippedHeadgearRef != null) {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        }
        if (equippedArmorTopRef != null) {
            equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        }
        if (equippedArmorBottomRef != null) {
            equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        }
    }

    public void ToggleFpcMesh(bool b) {
        if (equippedFpcSkinRef != null) {
            equippedFpcSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        }
        equippedFpcTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        myFpcGlovesRenderer.GetComponent<SkinnedMeshRenderer>().enabled = b;
    }

    public bool isFirstPerson() {
        return (firstPersonRef != null && firstPersonRef.activeInHierarchy);
    }

    public void EquipDefaults() {
        equippedSkin = -1;
        RemoveFacewear();
        RemoveArmor();
        RemoveHeadgear();
        Character c = InventoryScript.itemData.characterCatalog[equippedCharacter];
        EquipTop(c.defaultTop, null);
        EquipBottom(c.defaultBottom, null);
        EquipFootwear((c.gender == 'M' ? "Standard Boots (M)" : "Standard Boots (F)"), null);
    }

    public char GetGenderByCharacter(string characterName) {
        return InventoryScript.itemData.characterCatalog[characterName].gender;
    }

    public void EquipDefaultsForSetup() {
        EquipSkinForSetup();
        EquipTopForSetup();
        EquipBottomForSetup();
        EquipFootwearForSetup();
    }

    public void HighlightItemPrefab(GameObject shopItemRef) {
        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
            }
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }
    }

    public void EquipCharacter(string name, GameObject shopItemRef) {
        equippedCharacter = name;
        Character c = InventoryScript.itemData.characterCatalog[name];

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && !ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemName.Equals(name)) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedCharacterSlot.GetComponent<SlotScript>().ToggleThumbnail(true, c.thumbnailPath);
        ts.shopEquippedCharacterSlot.GetComponent<SlotScript>().ToggleThumbnail(true, c.thumbnailPath);
        ts.currentCharGender = InventoryScript.itemData.characterCatalog[name].gender;

        //if (name.Equals("Lucas") || name.Equals("Daryl") || name.Equals("Codename Sayre")) {
        //    ts.currentCharGender = 'M';
        //} else {
        //    ts.currentCharGender = 'F';
        //}

        // Clear all equipment stats
        playerScript.stats.SetDefaults();
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));

        // Change equipment back to default and re-equip weapons that were equipped beforehand
        EquipDefaults();
        if (PlayerData.playerdata.info.equippedPrimary == null)
        {
            PlayerData.playerdata.info.equippedPrimary = "AK-47";
        }
        if (PlayerData.playerdata.info.equippedSecondary == null)
        {
            PlayerData.playerdata.info.equippedSecondary = "Glock23";
        }
        if (PlayerData.playerdata.info.equippedSupport == null)
        {
            PlayerData.playerdata.info.equippedSupport = "M67 Frag";
        }
        ModInfo primaryModInfo = PlayerData.playerdata.LoadModDataForWeapon(PlayerData.playerdata.info.equippedPrimary);
        ModInfo secondaryModInfo = PlayerData.playerdata.LoadModDataForWeapon(PlayerData.playerdata.info.equippedSecondary);
        tws.EquipWeapon(PlayerData.playerdata.info.equippedPrimary, primaryModInfo.equippedSuppressor, primaryModInfo.equippedSight, null);
        tws.EquipWeapon(PlayerData.playerdata.info.equippedSecondary, secondaryModInfo.equippedSuppressor, secondaryModInfo.equippedSight, null);
        tws.EquipWeapon(PlayerData.playerdata.info.equippedSupport, null, null, null);

    }

    bool IsCharacterRestricted(Equipment e) {
        if (e.characterRestrictions.Length > 0) {
            for (int i = 0; i < e.characterRestrictions.Length; i++) {
                string c = e.characterRestrictions[i];
                if (equippedCharacter.Equals(c)) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public void EquipTop(string name, GameObject shopItemRef) {
        // Don't equip if not same gender or if restricted to certain characers
        char charGender = GetGenderByCharacter(equippedCharacter);
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            }
            return;
        }
        if (IsCharacterRestricted(e)) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            }
            return;
        }
        if (name.Equals(equippedTop)) {
            return;
        }
        equippedTop = name;
        if (equippedTopRef != null) {
            Destroy(equippedTopRef);
            equippedTopRef = null;
        }
        string absPrefabPath = (charGender == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedTopRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        EquipSkin(e.skinType);

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Top")) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        ts.shopEquippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
    }

    void EquipTopForSetup() {
        equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(topPrefabSetupPath));
        equippedTopRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    private void EquipSkin(int skinType) {
        if (equippedSkin == skinType) {
            return;
        }

        equippedSkin = skinType;
        if (equippedSkinRef != null) {
            Destroy(equippedSkinRef);
            equippedSkinRef = null;
        }
        equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[equippedCharacter].skins[skinType]));
        equippedSkinRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipSkinForSetup() {
        equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(skinPrefabSetupPath));
        equippedSkinRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipBottom(string name, GameObject shopItemRef) {
        // Don't equip if not same gender or if restricted to certain characers
        char charGender = GetGenderByCharacter(equippedCharacter);
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            }
            return;
        }
        if (IsCharacterRestricted(e)) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            }
            return;
        }
        if (name.Equals(equippedBottom)) {
            return;
        }
        equippedBottom = name;
        if (equippedBottomRef != null) {
            Destroy(equippedBottomRef);
            equippedBottomRef = null;
        }
        string absPrefabPath = (charGender == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedBottomRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Bottom")) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        ts.shopEquippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
    }

    void EquipBottomForSetup() {
        equippedBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(bottomPrefabSetupPath));
        equippedBottomRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipFootwear(string name, GameObject shopItemRef) {
        // Don't equip if not same gender or if restricted to certain characers
        char charGender = GetGenderByCharacter(equippedCharacter);
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            }
            return;
        }
        if (IsCharacterRestricted(e)) {
            if (shopItemRef != null) {
                ts.TriggerCustomizationPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            } else {
                ts.TriggerMarketplacePopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            }
            return;
        }
        if (name.Equals(equippedFootwear)) {
            return;
        }
        equippedFootwear = name;
        if (equippedFootwearRef != null) {
            Destroy(equippedFootwearRef);
            equippedFootwearRef = null;
        }
        string absPrefabPath = (charGender == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedFootwearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedFootwearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Footwear")) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        ts.shopEquippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        
    }

    void EquipFootwearForSetup() {
        equippedFootwearRef = (GameObject)Instantiate((GameObject)Resources.Load(footwearPrefabSetupPath));
        equippedFootwearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipFacewear(string name, GameObject shopItemRef) {
        RemoveFacewear();
        if (name.Equals(equippedFacewear)) {
            return;
        }
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        equippedFacewear = name;
        if (equippedFacewearRef != null) {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedFacewearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedFacewearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Facewear")) {
            ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        ts.shopEquippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        

        playerScript.stats.updateStats(e.speed, e.stamina, e.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));
    }

    public void EquipHeadgear(string name, GameObject shopItemRef) {
        RemoveHeadgear();
        if (name.Equals(equippedHeadgear)) {
            return;
        }
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        // Hide hair if has hair
        if (e.hideHairFlag) {
            if (myHairRenderer != null) {
                myHairRenderer.SetActive(false);
            }
        } else {
            if (myHairRenderer != null) {
                myHairRenderer.SetActive(true);
            }
        }
        equippedHeadgear = name;
        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedHeadgearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedHeadgearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
        m.target = myHeadgearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Headgear")) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        ts.shopEquippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        

        // Adds headgear stat to player
        playerScript.stats.updateStats(e.speed, e.stamina, e.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));
    }

    public void EquipArmor(string name, GameObject shopItemRef) {
        RemoveArmor();
        if (name.Equals(equippedArmor)) {
            return;
        }
        Armor a = InventoryScript.itemData.armorCatalog[name];
        equippedArmor = name;
        if (equippedArmorTopRef != null) {
            Destroy(equippedArmorTopRef);
            equippedArmorTopRef = null;
        }
        if (equippedArmorBottomRef != null) {
            Destroy(equippedArmorBottomRef);
            equippedArmorBottomRef = null;
        }
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? a.malePrefabPathTop : a.femalePrefabPathTop);
        equippedArmorTopRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedArmorTopRef.transform.SetParent(gameObject.transform);
        absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? a.malePrefabPathBottom : a.femalePrefabPathBottom);
        equippedArmorBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedArmorBottomRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        m = equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null && ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().itemType.Equals("Armor")) {
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, a.thumbnailPath);
        ts.shopEquippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, a.thumbnailPath);

        playerScript.stats.updateStats(a.speed, a.stamina, a.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));
    }

    public void RemoveHeadgear() {
        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }
        if (myHairRenderer != null) {
            myHairRenderer.SetActive(true);
        }
        ts.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        ts.shopEquippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        if (string.IsNullOrEmpty(equippedHeadgear)) {
            return;
        }
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Headgear")) {
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                ts.currentlyEquippedItemPrefab = null;
            }
        }
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedHeadgear];
        playerScript.stats.updateStats(-e.speed, -e.stamina, -e.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));

        equippedHeadgear = "";
    }

    public void RemoveFacewear() {
        if (equippedFacewearRef != null)
        {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }
        ts.equippedFaceSlot.GetComponentInChildren<RawImage>().texture = null;
        ts.equippedFaceSlot.GetComponentInChildren<RawImage>().enabled = false;
        ts.shopEquippedFaceSlot.GetComponentInChildren<RawImage>().texture = null;
        ts.shopEquippedFaceSlot.GetComponentInChildren<RawImage>().enabled = false;
        if (string.IsNullOrEmpty(equippedFacewear))
        {
            return;
        }
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Facewear")) {
                s.ToggleEquippedIndicator(false);
                ts.currentlyEquippedItemPrefab = null;
            }
        }

        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedFacewear];
        playerScript.stats.updateStats(-e.speed, -e.stamina, -e.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));

        equippedFacewear = "";
    }

    public void RemoveArmor() {
        if (equippedArmorTopRef != null) {
            Destroy(equippedArmorTopRef);
            equippedArmorTopRef = null;
        }
        if (equippedArmorBottomRef != null) {
            Destroy(equippedArmorBottomRef);
            equippedArmorBottomRef = null;
        }
        ts.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        ts.shopEquippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        if (string.IsNullOrEmpty(equippedArmor))
        {
            return;
        }
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Armor")) {
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                ts.currentlyEquippedItemPrefab = null;
            }
        }
        Armor e = InventoryScript.itemData.armorCatalog[equippedArmor];
        playerScript.stats.updateStats(-e.speed, -e.stamina, -e.armor, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f));

        equippedArmor = "";
    }

    [PunRPC]
    private void RpcEquipCharacterInGame(string character) {
        equippedCharacter = character;
    }

    [PunRPC]
    private void RpcEquipTopInGame(string top) {
        equippedTop = top;
        Equipment e = InventoryScript.itemData.equipmentCatalog[top];
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedTopRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Equip shirt on FPC model as well if it's the local player
        if (isFirstPerson()) {
            absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.maleFpcPrefabPath : e.femaleFpcPrefabPath);
            equippedFpcTopRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
            equippedFpcTopRef.transform.SetParent(firstPersonRef.transform);
            m = equippedFpcTopRef.GetComponentInChildren<MeshFixer>();
            m.target = myFpcTopRenderer.gameObject;
            m.rootBone = myFpcBones.transform;
            m.AdaptMesh();
        }

        //pView.RPC("RpcEquipSkinInGame", RpcTarget.AllBuffered, e.skinType);
        EquipSkinInGame(e.skinType);
    }

    // [PunRPC]
    // private void RpcEquipSkinInGame(int skin) {
    //     equippedSkin = skin;
    //     equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.characterCatalog[equippedCharacter].skins[skin]));
    //     equippedSkinRef.transform.SetParent(fullBodyRef.transform);
    //     MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
    //     m.target = mySkinRenderer.gameObject;
    //     m.rootBone = myBones.transform;
    //     m.AdaptMesh();
    // }

    private void EquipSkinInGame(int skin) {
        equippedSkin = skin;
        equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[equippedCharacter].skins[skin]));
        equippedSkinRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        // Equips skin on FPC if is local player
        if (isFirstPerson()) {
            string skinPath = (skin != 0) ? InventoryScript.itemData.characterCatalog[equippedCharacter].fpcFullSkinPath : InventoryScript.itemData.characterCatalog[equippedCharacter].fpcNoSkinPath;
            if (!"".Equals(skinPath)) {
                equippedFpcSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(skinPath));
                equippedFpcSkinRef.transform.SetParent(firstPersonRef.transform);
                m = equippedFpcSkinRef.GetComponentInChildren<MeshFixer>();
                m.target = myFpcSkinRenderer.gameObject;
                m.rootBone = myFpcBones.transform;
                m.AdaptMesh();
            }
        }
    }

    [PunRPC]
    private void RpcEquipBottomInGame(string bottom) {
        equippedBottom = bottom;
        Equipment e = InventoryScript.itemData.equipmentCatalog[bottom];
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedBottomRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    [PunRPC]
    private void RpcEquipHeadgearInGame(string headgear) {
        if (headgear == null || headgear.Equals("")) {
            return;
        }
        equippedHeadgear = headgear;
        Equipment e = InventoryScript.itemData.equipmentCatalog[headgear];
        // Hide hair if has hair
        if (e.hideHairFlag) {
            if (myHairRenderer != null) {
                myHairRenderer.SetActive(false);
            }
        } else {
            if (myHairRenderer != null) {
                myHairRenderer.SetActive(true);
            }
        }
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedHeadgearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedHeadgearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
        m.target = myHeadgearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        playerScript.stats.updateStats(e.speed, e.stamina, e.armor, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipFacewearInGame(string facewear) {
        if (facewear == null || facewear.Equals("")) {
            return;
        }
        equippedFacewear = facewear;
        Equipment e = InventoryScript.itemData.equipmentCatalog[facewear];
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedFacewearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedFacewearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        playerScript.stats.updateStats(e.speed, e.stamina, e.armor, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipArmorInGame(string armor) {
        if (armor == null || armor.Equals("")) {
            return;
        }
        equippedArmor = armor;
        Armor a = InventoryScript.itemData.armorCatalog[armor];
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? a.malePrefabPathTop : a.femalePrefabPathTop);
        equippedArmorTopRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedArmorTopRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? a.malePrefabPathBottom : a.femalePrefabPathBottom);
        equippedArmorBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedArmorBottomRef.transform.SetParent(fullBodyRef.transform);
        m = equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        playerScript.stats.updateStats(a.speed, a.stamina, a.armor, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipFootwearInGame(string footwear) {
        equippedFootwear = footwear;
        Equipment e = InventoryScript.itemData.equipmentCatalog[footwear];
        string absPrefabPath = (GetGenderByCharacter(equippedCharacter) == 'M' ? e.malePrefabPath : e.femalePrefabPath);
        equippedFootwearRef = (GameObject)Instantiate((GameObject)Resources.Load(absPrefabPath));
        equippedFootwearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void DespawnPlayer()
    {
        if (equippedSkinRef != null)
        {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedHeadgearRef != null)
        {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedFacewearRef != null)
        {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedArmorTopRef != null)
        {
            equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedArmorBottomRef != null)
        {
            equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedTopRef != null)
        {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedBottomRef != null)
        {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedFootwearRef != null)
        {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }

        if (myHairRenderer != null)
        {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myGlovesRenderer != null)
        {
            myGlovesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myEyesRenderer != null)
        {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myEyelashRenderer != null)
        {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
    }

    [PunRPC]
    void RpcRespawnPlayer()
    {
        if (playerActionScript.isNotOnTeamMap) return;
        if (equippedSkinRef != null)
        {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedHeadgearRef != null)
        {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedFacewearRef != null)
        {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedArmorTopRef != null)
        {
            equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedArmorBottomRef != null)
        {
            equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedTopRef != null)
        {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedBottomRef != null)
        {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedFootwearRef != null)
        {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myEyesRenderer != null)
        {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myEyelashRenderer != null)
        {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myGlovesRenderer != null)
        {
            myGlovesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myHairRenderer != null && renderHair)
        {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
    }

    public void RespawnPlayer() {
        pView.RPC("RpcRespawnPlayer", RpcTarget.All);
    }

    // public void DespawnPlayer() {
    //     pView.RPC("RpcDespawnPlayer", RpcTarget.All);
    // }

}
