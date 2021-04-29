using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class EquipmentScript : MonoBehaviour
{

    public TitleControllerScript ts;
    public PlayerScript playerScript;
    public PlayerActionScript playerActionScript;
    public WeaponScript tws;
    public GameObject fullBodyRef;
    public GameObject firstPersonRef;
    public Material camoMat;

    public string equippedCharacter;
    public string characterGender;
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
    private Material originalEyeMat;
    private Material originalEyelashMat;
    private Material originalGlovesMat;
    private Material originalTopMat;
    private Material originalBottomMat;
    private Material originalFootwearMat;
    private Material originalSkinMat;
    private Material originalHairMat;
    private Material originalFacewearMat;
    private Material originalHeadgearMat;
    private Material originalArmorTopMat;
    private Material originalArmorBottomMat;
    private Material originalFpcSkinMat;
    private Material originalFpcTopMat;
    private Material originalFpcGloveMat;

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

    private bool initialized;

    void Awake() {
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
    }

    void Start() {
        if (onTitle)
        {
            if (ts == null)
            {
                ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            }
        } else if (onSetup) {
            EquipDefaultsForSetup();
        }
    }

    public void PreInitialize()
    {
        onTitle = false;

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

    public void Initialize() {
        if (pView != null && !pView.IsMine) {
            initialized = true;
            return;
        }

        pView.RPC("RpcEquipCharacterInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedCharacter);
        pView.RPC("RpcEquipHeadgearInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedHeadgear);
        pView.RPC("RpcEquipFacewearInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedFacewear);
        pView.RPC("RpcEquipTopInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedTop);
        pView.RPC("RpcEquipBottomInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedBottom);
        pView.RPC("RpcEquipFootwearInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedFootwear);
        pView.RPC("RpcEquipArmorInGame", RpcTarget.All, PlayerData.playerdata.info.EquippedArmor);
        SetOriginalFpcMaterials();
        SetOriginalMaterials();

        initialized = true;
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

    public void CamouflageMesh(bool b)
    {
        if (b) {
            myEyesRenderer.GetComponent<SkinnedMeshRenderer>().material = camoMat;
            myEyelashRenderer.GetComponent<SkinnedMeshRenderer>().material = camoMat;
            myGlovesRenderer.GetComponent<SkinnedMeshRenderer>().material = camoMat;
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            if (renderHair) {
                myHairRenderer.GetComponent<SkinnedMeshRenderer>().material = camoMat;
            }
            if (equippedFacewearRef != null) {
                equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            }
            if (equippedHeadgearRef != null) {
                equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            }
            if (equippedArmorTopRef != null) {
                equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            }
            if (equippedArmorBottomRef != null) {
                equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            }
        } else {
            myEyesRenderer.GetComponent<SkinnedMeshRenderer>().material = originalEyeMat;
            myEyelashRenderer.GetComponent<SkinnedMeshRenderer>().material = originalEyelashMat;
            myGlovesRenderer.GetComponent<SkinnedMeshRenderer>().material = originalGlovesMat;
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalTopMat;
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalBottomMat;
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalFootwearMat;
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalSkinMat;
            if (renderHair) {
                myHairRenderer.GetComponent<SkinnedMeshRenderer>().material = originalHairMat;
            }
            if (equippedFacewearRef != null) {
                equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalFacewearMat;
            }
            if (equippedHeadgearRef != null) {
                equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalHeadgearMat;
            }
            if (equippedArmorTopRef != null) {
                equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalArmorTopMat;
            }
            if (equippedArmorBottomRef != null) {
                equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalArmorBottomMat;
            }
        }
    }

    public void CamouflageFpcMesh(bool b)
    {
        if (b) {
            if (equippedFpcSkinRef != null) {
                equippedFpcSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            }
            equippedFpcTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = camoMat;
            myFpcGlovesRenderer.GetComponent<SkinnedMeshRenderer>().material = camoMat;
        } else {
            if (equippedFpcSkinRef != null) {
                equippedFpcSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalFpcSkinMat;
            }
            equippedFpcTopRef.GetComponentInChildren<SkinnedMeshRenderer>().material = originalFpcTopMat;
            myFpcGlovesRenderer.GetComponent<SkinnedMeshRenderer>().material = originalFpcGloveMat;
        }
    }

    void SetOriginalMaterials()
    {
        originalEyeMat = myEyesRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        originalEyelashMat = myEyelashRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        originalGlovesMat = myGlovesRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        originalTopMat = equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        originalBottomMat = equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        originalFootwearMat = equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        originalSkinMat = equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        if (renderHair) {
            originalHairMat = myHairRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        }
        if (equippedFacewearRef != null) {
            originalFacewearMat = equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        }
        if (equippedHeadgearRef != null) {
            originalHeadgearMat = equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        }
        if (equippedArmorTopRef != null) {
            originalArmorTopMat = equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        }
        if (equippedArmorBottomRef != null) {
            originalArmorBottomMat = equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        }
    }

    void SetOriginalFpcMaterials()
    {
        if (equippedFpcSkinRef != null) {
            originalFpcSkinMat = equippedFpcSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        }
        originalFpcTopMat = equippedFpcTopRef.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        originalFpcGloveMat = myFpcGlovesRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
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

    public char GetGender() {
        return characterGender[0];
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
            ShopItemScript sis = shopItemRef.GetComponent<ShopItemScript>();
            sis.ToggleEquippedIndicator(true);
            if (sis.itemType == "Weapon") {
                if (ts.currentlyEquippedWeaponPrefab != null) {
                    ts.currentlyEquippedWeaponPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                }
                ts.currentlyEquippedWeaponPrefab = shopItemRef;
            } else if (sis.itemType == "Mod") {
                if (ts.currentlyEquippedModPrefab != null) {
                    ts.currentlyEquippedModPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                }
                ts.currentlyEquippedModPrefab = shopItemRef;
            } else {
                if (ts.currentlyEquippedEquipmentPrefab != null) {
                    ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                }
                ts.currentlyEquippedEquipmentPrefab = shopItemRef;
            }
        }
    }

    public void EquipCharacter(string name, GameObject shopItemRef) {
        if (PlayerData.playerdata.info.EquippedCharacter == name) return;
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && !ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemName.Equals(name)) {
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedCharacter"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewCharacter(string name) {
        if (name.Equals(equippedCharacter)) {
            return;
        }

        equippedCharacter = name;
        Character c = InventoryScript.itemData.characterCatalog[name];
        Destroy(PlayerData.playerdata.bodyReference);
        PlayerData.playerdata.bodyReference = null;
        PlayerData.playerdata.FindBodyRef(name);

        EquipmentScript previewCharEquips = PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>();
        previewCharEquips.PreviewTop(c.defaultTop);
        previewCharEquips.PreviewBottom(c.defaultBottom);
        previewCharEquips.PreviewFootwear("Standard Boots (" + c.gender + ")");

        // Reequip primary for preview
        Weapon w = InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary];
        string weaponType = w.category;
        GameObject wepEquipped = previewCharEquips.tws.weaponHolder.LoadWeapon(w.prefabPath);
        WeaponMeta wm = wepEquipped.GetComponent<WeaponMeta>();
        previewCharEquips.tws.equippedPrimaryWeapon = PlayerData.playerdata.info.EquippedPrimary;
        
        if (w.suppressorCompatible) {
            previewCharEquips.tws.EquipMod("Suppressor", PlayerData.playerdata.primaryModInfo.EquippedSuppressor, PlayerData.playerdata.info.EquippedPrimary, null);
        }
        if (w.sightCompatible) {
            previewCharEquips.tws.EquipMod("Sight", PlayerData.playerdata.primaryModInfo.EquippedSight, PlayerData.playerdata.info.EquippedPrimary, null);
        }

        if (c.gender == 'M') {
            previewCharEquips.tws.SetTitleWeaponPositions(wm.fullPosMale, wm.fullRotMale, 'M');
        } else {
            previewCharEquips.tws.SetTitleWeaponPositions(wm.fullPosFemale, wm.fullRotFemale, 'F');
        }
    }

    public void ReequipWeapons() {
        ModInfo primaryModInfo = PlayerData.playerdata.LoadModDataForWeapon(PlayerData.playerdata.info.EquippedPrimary);
        ModInfo secondaryModInfo = PlayerData.playerdata.LoadModDataForWeapon(PlayerData.playerdata.info.EquippedSecondary);
        tws.EquipWeapon(PlayerData.playerdata.info.EquippedPrimary, primaryModInfo.EquippedSuppressor, primaryModInfo.EquippedSight, null);
        tws.EquipWeapon(PlayerData.playerdata.info.EquippedSecondary, secondaryModInfo.EquippedSuppressor, secondaryModInfo.EquippedSight, null);
        tws.EquipWeapon(PlayerData.playerdata.info.EquippedSupport, null, null, null);
        tws.EquipWeapon(PlayerData.playerdata.info.EquippedMelee, null, null, null);
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
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        if (name.Equals(equippedTop)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Top")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedTop"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewTop(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        equippedTop = name;

        Destroy(equippedTopRef);
        equippedTopRef = null;

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedTopRef = (GameObject)Instantiate(p);
        equippedTopRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        EquipSkin(e.skinType);
    }

    void EquipTopForSetup() {
        equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(topPrefabSetupPath));
        equippedTopRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipSkin(int skinType) {
        if (equippedSkin == skinType) {
            return;
        }
        equippedSkin = skinType;
        if (equippedSkinRef != null) {
            Destroy(equippedSkinRef);
            equippedSkinRef = null;
        }
        equippedSkinRef = (GameObject)Instantiate(InventoryScript.itemData.itemReferences[InventoryScript.itemData.characterCatalog[equippedCharacter].skins[skinType]]);
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
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }

        if (name.Equals(equippedBottom)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Bottom")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedBottom"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewBottom(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        equippedBottom = name;

        Destroy(equippedBottomRef);
        equippedBottomRef = null;

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedBottomRef = (GameObject)Instantiate(p);
        equippedBottomRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
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
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }

        if (name.Equals(equippedFootwear)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Footwear")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedFootwear"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewFootwear(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        equippedFootwear = name;

        Destroy(equippedFootwearRef);
        equippedFootwearRef = null;

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedFootwearRef = (GameObject)Instantiate(p);
        equippedFootwearRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
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
        if (name.Equals(equippedFacewear)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Facewear")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedFacewear"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewFacewear(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != 'N' && e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        equippedFacewear = name;

        if (equippedFacewearRef != null) {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedFacewearRef = (GameObject)Instantiate(p);
        equippedFacewearRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipHeadgear(string name, GameObject shopItemRef) {
        if (name.Equals(equippedHeadgear)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Headgear")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedHeadgear"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewHeadgear(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Equipment e = InventoryScript.itemData.equipmentCatalog[name];
        if (e.gender != 'N' && e.gender != charGender) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\n" + e.gender + " gender only");
            return;
        }
        if (IsCharacterRestricted(e)) {
            ts.TriggerAlertPopup("You cannot equip this item due to the following restrictions:\nOnly equippable on these characters: " + string.Join(", ", e.characterRestrictions));
            return;
        }
        equippedHeadgear = name;

        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedHeadgearRef = (GameObject)Instantiate(p);
        equippedHeadgearRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
        m.target = myHeadgearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipArmor(string name, GameObject shopItemRef) {
        if (name.Equals(equippedArmor)) {
            return;
        }

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedEquipmentPrefab != null && ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().itemType.Equals("Armor")) {
            ts.currentlyEquippedEquipmentPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
            ts.currentlyEquippedEquipmentPrefab = shopItemRef;
        }

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equippedArmor"] = name;
        
        ts.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void PreviewArmor(string name) {
        // Don't preview if not same gender or if restricted to certain characers
        char charGender = GetGender();
        Armor a = InventoryScript.itemData.armorCatalog[name];
        if (name.Equals(equippedArmor)) {
            return;
        }

        if (equippedArmorTopRef != null) {
            Destroy(equippedArmorTopRef);
            equippedArmorTopRef = null;
        }
        
        if (equippedArmorBottomRef != null) {
            Destroy(equippedArmorBottomRef);
            equippedArmorBottomRef = null;
        }
        equippedArmor = name;

        GameObject p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathTop] : InventoryScript.itemData.itemReferences[a.femalePrefabPathTop]);
        equippedArmorTopRef = (GameObject)Instantiate(p);
        equippedArmorTopRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        MeshFixer m = equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();

        p = (InventoryScript.itemData.characterCatalog[PlayerData.playerdata.info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathBottom] : InventoryScript.itemData.itemReferences[a.femalePrefabPathBottom]);
        equippedArmorBottomRef = (GameObject)Instantiate(p);
        equippedArmorBottomRef.transform.SetParent(PlayerData.playerdata.bodyReference.transform);
        m = equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void UpdateStatsOnTitle() {
        StatBoosts newTotalStatBoosts = CalculateStatBoostsWithCurrentEquips();
        playerScript.stats.setStats(newTotalStatBoosts.speedBoost, newTotalStatBoosts.staminaBoost, newTotalStatBoosts.armorBoost, newTotalStatBoosts.avoidabilityBoost, newTotalStatBoosts.detection, 0);
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.avoidability - 1.0f) * 100.0f), playerScript.stats.detection);
    }

    public void ResetStats() {
        // Clear all equipment stats
        playerScript.stats.SetDefaults();
        playerScript.updateStats();
        ts.SetStatBoosts(Mathf.RoundToInt((playerScript.stats.armor - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.speed - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.stamina - 1.0f) * 100.0f), Mathf.RoundToInt((playerScript.stats.avoidability - 1.0f) * 100.0f), playerScript.stats.detection);
    }

    public void RemoveHeadgear() {
        EquipHeadgear("", null);
    }

    public void RemoveFacewear() {
        EquipFacewear("", null);
    }

    public void RemoveArmor() {
        EquipArmor("", null);
    }

    [PunRPC]
    private void RpcEquipCharacterInGame(string character) {
        equippedCharacter = character;
    }

    [PunRPC]
    private void RpcEquipTopInGame(string top) {
        equippedTop = top;
        EquipTopInGame();
    }

    void EquipTopInGame() {
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedTop];
        
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedTopRef = (GameObject)Instantiate(p);
        equippedTopRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        // Equip shirt on FPC model as well if it's the local player
        if (isFirstPerson()) {
            p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.maleFpcPrefabPath] : InventoryScript.itemData.itemReferences[e.femaleFpcPrefabPath]);
            equippedFpcTopRef = (GameObject)Instantiate(p);
            equippedFpcTopRef.transform.SetParent(firstPersonRef.transform);
            m = equippedFpcTopRef.GetComponentInChildren<MeshFixer>();
            m.target = myFpcTopRenderer.gameObject;
            m.rootBone = myFpcBones.transform;
            m.AdaptMesh();
        }

        //pView.RPC("RpcEquipSkinInGame", RpcTarget.AllBuffered, e.skinType);
        EquipSkinInGame(e.skinType);
    }

    private void EquipSkinInGame(int skin) {
        equippedSkin = skin;
        equippedSkinRef = (GameObject)Instantiate(InventoryScript.itemData.itemReferences[InventoryScript.itemData.characterCatalog[equippedCharacter].skins[skin]]);
        equippedSkinRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        // Equips skin on FPC if is local player
        if (isFirstPerson()) {
            int skinPath = (skin != 0) ? InventoryScript.itemData.characterCatalog[equippedCharacter].fpcFullSkinPath : InventoryScript.itemData.characterCatalog[equippedCharacter].fpcNoSkinPath;
            if (skinPath != -1) {
                equippedFpcSkinRef = (GameObject)Instantiate(InventoryScript.itemData.itemReferences[skinPath]);
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
        EquipBottomInGame();
    }

    void EquipBottomInGame() {
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedBottom];
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedBottomRef = (GameObject)Instantiate(p);
        equippedBottomRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);
    }

    [PunRPC]
    private void RpcEquipHeadgearInGame(string headgear) {
        if (headgear == null || headgear.Equals("")) {
            return;
        }
        equippedHeadgear = headgear;
        EquipHeadgearInGame();
    }

    void EquipHeadgearInGame() {
        if (string.IsNullOrEmpty(equippedHeadgear)) return;
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedHeadgear];
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
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedHeadgearRef = (GameObject)Instantiate(p);
        equippedHeadgearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
        m.target = myHeadgearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        StatBoosts newTotalStatBoosts = CalculateStatBoostsWithCurrentEquips();
        playerScript.stats.setStats(newTotalStatBoosts.speedBoost + playerActionScript.skillController.GetNinjaSpeedBoost(), newTotalStatBoosts.staminaBoost + playerActionScript.skillController.GetStaminaBoost(), (newTotalStatBoosts.armorBoost * (1f + playerActionScript.skillController.GetArmorAmplificationBoost())) + playerActionScript.skillController.GetOverallArmorBoost(), newTotalStatBoosts.avoidabilityBoost, newTotalStatBoosts.detection, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipFacewearInGame(string facewear) {
        if (facewear == null || facewear.Equals("")) {
            return;
        }
        equippedFacewear = facewear;
        EquipFacewearInGame();
    }

    void EquipFacewearInGame() {
        if (string.IsNullOrEmpty(equippedFacewear)) return;
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedFacewear];
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedFacewearRef = (GameObject)Instantiate(p);
        equippedFacewearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        StatBoosts newTotalStatBoosts = CalculateStatBoostsWithCurrentEquips();
        playerScript.stats.setStats(newTotalStatBoosts.speedBoost + playerActionScript.skillController.GetNinjaSpeedBoost(), newTotalStatBoosts.staminaBoost + playerActionScript.skillController.GetStaminaBoost(), (newTotalStatBoosts.armorBoost * (1f + playerActionScript.skillController.GetArmorAmplificationBoost())) + playerActionScript.skillController.GetOverallArmorBoost(), newTotalStatBoosts.avoidabilityBoost, newTotalStatBoosts.detection, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipArmorInGame(string armor) {
        if (armor == null || armor.Equals("")) {
            return;
        }
        equippedArmor = armor;
        EquipArmorInGame();
    }

    void EquipArmorInGame() {
        if (string.IsNullOrEmpty(equippedArmor)) return;
        Armor a = InventoryScript.itemData.armorCatalog[equippedArmor];
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathTop] : InventoryScript.itemData.itemReferences[a.femalePrefabPathTop]);
        equippedArmorTopRef = (GameObject)Instantiate(p);
        equippedArmorTopRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathBottom] : InventoryScript.itemData.itemReferences[a.femalePrefabPathBottom]);
        equippedArmorBottomRef = (GameObject)Instantiate(p);
        equippedArmorBottomRef.transform.SetParent(fullBodyRef.transform);
        m = equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myArmorBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);

        StatBoosts newTotalStatBoosts = CalculateStatBoostsWithCurrentEquips();
        playerScript.stats.setStats(newTotalStatBoosts.speedBoost + playerActionScript.skillController.GetNinjaSpeedBoost(), newTotalStatBoosts.staminaBoost + playerActionScript.skillController.GetStaminaBoost(), (newTotalStatBoosts.armorBoost * (1f + playerActionScript.skillController.GetArmorAmplificationBoost())) + playerActionScript.skillController.GetOverallArmorBoost(), newTotalStatBoosts.avoidabilityBoost, newTotalStatBoosts.detection, 0);
        playerScript.updateStats();
    }

    [PunRPC]
    private void RpcEquipFootwearInGame(string footwear) {
        equippedFootwear = footwear;
        EquipFootwearInGame();
    }

    void EquipFootwearInGame() {
        if (string.IsNullOrEmpty(equippedFootwear)) return;
        Equipment e = InventoryScript.itemData.equipmentCatalog[equippedFootwear];
        GameObject p = (GetGender() == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        equippedFootwearRef = (GameObject)Instantiate(p);
        equippedFootwearRef.transform.SetParent(fullBodyRef.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh(playerActionScript.objectCarrying != null && playerActionScript.objectCarrying.GetComponent<NpcScript>() != null ? playerActionScript.objectCarrying.GetComponent<NpcScript>().pelvisTransform.gameObject : null);
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
        if (!pView.IsMine) {
            ToggleFullBody(true);
        }
    }

    public void RespawnPlayer() {
        pView.RPC("RpcRespawnPlayer", RpcTarget.All);
    }

    // public void DespawnPlayer() {
    //     pView.RPC("RpcDespawnPlayer", RpcTarget.All);
    // }

    public StatBoosts CalculateStatBoostsWithCurrentEquips() {
        float totalArmorBoost = 0f;
        float totalSpeedBoost = 0f;
        float totalStaminaBoost = 0f;
        float totalAvoidabilityBoost = 0f;
        int totalDetection = 0;

        if (equippedArmor != null && equippedArmor != "") {
            totalArmorBoost += InventoryScript.itemData.armorCatalog[equippedArmor].armor;
            totalSpeedBoost += InventoryScript.itemData.armorCatalog[equippedArmor].speed;
            totalStaminaBoost += InventoryScript.itemData.armorCatalog[equippedArmor].stamina;
            totalAvoidabilityBoost += InventoryScript.itemData.armorCatalog[equippedArmor].avoidability;
            totalDetection += InventoryScript.itemData.armorCatalog[equippedArmor].detection;
        }
        if (equippedHeadgear != null && equippedHeadgear != "") {
            totalArmorBoost += InventoryScript.itemData.equipmentCatalog[equippedHeadgear].armor;
            totalSpeedBoost += InventoryScript.itemData.equipmentCatalog[equippedHeadgear].speed;
            totalStaminaBoost += InventoryScript.itemData.equipmentCatalog[equippedHeadgear].stamina;
            totalAvoidabilityBoost += InventoryScript.itemData.equipmentCatalog[equippedHeadgear].avoidability;
            totalDetection += InventoryScript.itemData.equipmentCatalog[equippedHeadgear].detection;
        }
        if (equippedFacewear != null && equippedFacewear != "") {
            totalArmorBoost += InventoryScript.itemData.equipmentCatalog[equippedFacewear].armor;
            totalSpeedBoost += InventoryScript.itemData.equipmentCatalog[equippedFacewear].speed;
            totalStaminaBoost += InventoryScript.itemData.equipmentCatalog[equippedFacewear].stamina;
            totalAvoidabilityBoost += InventoryScript.itemData.equipmentCatalog[equippedFacewear].avoidability;
            totalDetection += InventoryScript.itemData.equipmentCatalog[equippedFacewear].detection;
        }
        
        totalDetection += InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].detection;
        totalDetection += InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedSecondary].detection;
        totalDetection += InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedSupport].detection;
        totalDetection += InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedMelee].detection;

        if (PlayerData.playerdata.primaryModInfo.SightId != null && PlayerData.playerdata.primaryModInfo.SightId != "") {
            totalDetection += InventoryScript.itemData.modCatalog[PlayerData.playerdata.primaryModInfo.EquippedSight].detection;
        }
        if (PlayerData.playerdata.primaryModInfo.SuppressorId != null && PlayerData.playerdata.primaryModInfo.SuppressorId != "") {
            totalDetection += InventoryScript.itemData.modCatalog[PlayerData.playerdata.primaryModInfo.EquippedSuppressor].detection;
        }
        if (PlayerData.playerdata.secondaryModInfo.SightId != null && PlayerData.playerdata.secondaryModInfo.SightId != "") {
            totalDetection += InventoryScript.itemData.modCatalog[PlayerData.playerdata.secondaryModInfo.EquippedSight].detection;
        }
        if (PlayerData.playerdata.secondaryModInfo.SuppressorId != null && PlayerData.playerdata.secondaryModInfo.SuppressorId != "") {
            totalDetection += InventoryScript.itemData.modCatalog[PlayerData.playerdata.secondaryModInfo.EquippedSuppressor].detection;
        }

        return new StatBoosts(totalArmorBoost, totalSpeedBoost, totalStaminaBoost, totalAvoidabilityBoost, totalDetection);
    }

    public void SyncDataOnJoin() {
        pView.RPC("RpcAskServerForDataEquips", RpcTarget.Others);
    }

    [PunRPC]
	void RpcAskServerForDataEquips() {
        if (!pView.IsMine) return;
		pView.RPC("RpcSyncDataEquips", RpcTarget.Others, equippedArmor, equippedHeadgear, equippedFacewear, equippedFootwear, equippedSkin, equippedTop, equippedBottom, equippedCharacter);
	}

	[PunRPC]
	void RpcSyncDataEquips(string equippedArmor, string equippedHeadgear, string equippedFacewear, string equippedFootwear, int equippedSkin, string equippedTop, string equippedBottom, string equippedCharacter) {
        this.equippedArmor = equippedArmor;
        this.equippedHeadgear = equippedHeadgear;
        this.equippedFacewear = equippedFacewear;
        this.equippedFootwear = equippedFootwear;
        this.equippedSkin = equippedSkin;
        this.equippedTop = equippedTop;
        this.equippedBottom = equippedBottom;
        this.equippedCharacter = equippedCharacter;
        if (equippedSkinRef == null) {
            SyncEquips();
            SetOriginalMaterials();
        }
	}

    void SyncEquips() {
        EquipTopInGame();
        EquipBottomInGame();
        EquipFootwearInGame();
        EquipHeadgearInGame();
        EquipFacewearInGame();
        EquipArmorInGame();
    }

    public void ToggleUpdateWhenOffscreen(bool b)
	{
		if (equippedSkinRef != null)
        {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedHeadgearRef != null)
        {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedFacewearRef != null)
        {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedArmorTopRef != null)
        {
            equippedArmorTopRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedArmorBottomRef != null)
        {
            equippedArmorBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedTopRef != null)
        {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedBottomRef != null)
        {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedFootwearRef != null)
        {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myHairRenderer != null)
        {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myGlovesRenderer != null)
        {
            myGlovesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myEyesRenderer != null)
        {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myEyelashRenderer != null)
        {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
	}

    public class StatBoosts {
        public float armorBoost;
        public float speedBoost;
        public float staminaBoost;
        public float avoidabilityBoost;
        public int detection;

        public StatBoosts(float armorBoost, float speedBoost, float staminaBoost, float avoidabilityBoost, int detection) {
            this.armorBoost = armorBoost;
            this.speedBoost = speedBoost;
            this.staminaBoost = staminaBoost;
            this.avoidabilityBoost = avoidabilityBoost;
            this.detection = detection;
        }
    }

}
