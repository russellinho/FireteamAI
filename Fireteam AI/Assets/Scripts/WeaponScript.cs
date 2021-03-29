using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Koobando.AntiCheat;

public class WeaponScript : MonoBehaviour
{
    public EquipmentScript equipmentScript;
    public WeaponActionScript weaponActionScript;
    public PlayerActionScript playerActionScript;
    public WeaponHandlerScript weaponHolder;
    public WeaponHandlerScript weaponHolderFpc;
    public MeleeHandlerScript meleeHolder;
    public MeleeHandlerScript meleeHolderFpc;
    public Animator animator;
    public TitleControllerScript ts;
    public Animation titleAnimFemale;
    public Animation titleAnimMale;
    
    public string equippedPrimaryWeapon;
    public string equippedSecondaryWeapon;
    public string equippedSupportWeapon;
    public string equippedMeleeWeapon;
    public string equippedWepInGame;
    public int currentlyEquippedType;
    public EncryptedInt totalPrimaryAmmoLeft;
    public EncryptedInt totalSecondaryAmmoLeft;
    public EncryptedInt totalSupportAmmoLeft;
    public EncryptedInt currentAmmoPrimary;
    public EncryptedInt currentAmmoSecondary;
    public EncryptedInt currentAmmoSupport;

    private GameObject drawnWeaponReference;
    // private GameObject drawnSuppressorReference;
    // private GameObject drawnSightReference;

    public bool weaponReady;
    public PhotonView pView;

    private bool onTitle;
    private bool onSetup;
    public char setupGender;

    private bool initialized;

    void Awake() {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName.Equals("Title")) {
            onTitle = true;
        } else if (activeSceneName.Equals("Setup")) {
            onTitle = false;
            onSetup = true;
        }
        animator.SetBool("onTitle", true);
    }

    void Start() {
        if (onTitle) {
            ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            equipmentScript = GetComponent<EquipmentScript>();
            weaponReady = false;
        }
    }

    public void PreInitialize() {
        // If the photon view is null, then the player is not in-game
        if (!equipmentScript.isFirstPerson()) {
            onTitle = false;
            if (animator != null) {
                animator.SetBool("onTitle", false);
            }
        }
    }

    public void SyncDataOnJoin() {
        pView.RPC("RpcAskServerForDataWeps", RpcTarget.Others);
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        if (pView != null && !pView.IsMine)
        {
            initialized = true;
            return;
        }
        
        //EquipWeapon(PlayerData.playerdata.info.equippedPrimaryType, PlayerData.playerdata.info.equippedPrimary, null);
        //EquipWeapon(PlayerData.playerdata.info.equippedSecondaryType, PlayerData.playerdata.info.equippedSecondary, null);
        equippedPrimaryWeapon = PlayerData.playerdata.info.EquippedPrimary;
        equippedSecondaryWeapon = PlayerData.playerdata.info.EquippedSecondary;
        equippedSupportWeapon = PlayerData.playerdata.info.EquippedSupport;
        equippedMeleeWeapon = PlayerData.playerdata.info.EquippedMelee;
        currentAmmoPrimary = InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].clipCapacity;
        currentAmmoSecondary = InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].clipCapacity;
        currentAmmoSupport = InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].clipCapacity;
        totalPrimaryAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].maxAmmo + (currentAmmoPrimary * playerActionScript.skillController.GetProviderBoost())) - currentAmmoPrimary;
        totalSecondaryAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].maxAmmo + (currentAmmoSecondary * playerActionScript.skillController.GetProviderBoost())) - currentAmmoSecondary;
        int supportAmmoBoost = 0;
        if (PlayerData.playerdata.info.EquippedSupport == "First Aid Kit") {
            supportAmmoBoost = playerActionScript.skillController.GetHealthCaddyBoost();
        }
        if (PlayerData.playerdata.info.EquippedSupport == "Ammo Bag") {
            supportAmmoBoost = playerActionScript.skillController.GetAmmoCaddyBoost();
        }
        if (PlayerData.playerdata.info.EquippedSupport == "Bubble Shield") {
            supportAmmoBoost = playerActionScript.skillController.GetDigitalNomadBoost();
        }
        totalSupportAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].maxAmmo + (currentAmmoSupport * playerActionScript.skillController.GetProviderBoost())) + supportAmmoBoost - currentAmmoSupport;
        equippedWepInGame = equippedPrimaryWeapon;
        //DrawWeapon(1);
        InitializeWeapon();
        InitializeMelee();

        initialized = true;
    }

    void InitializeMelee() {
        pView.RPC("RpcInitializeMelee", RpcTarget.All, equippedMeleeWeapon);
    }

    [PunRPC]
    void RpcInitializeMelee(string equippedMelee) {
        EquipWeapon(equippedMelee, null, null, null);
    }

    // Use when spawning/respawning
    void InitializeWeapon() {
        string equippedWep = "";
        ModInfo modInfo = null;
        
        weaponActionScript.hudScript.ToggleCrosshair(false);
        equippedWep = equippedPrimaryWeapon;
        weaponActionScript.currentAmmo = currentAmmoPrimary;
        weaponActionScript.totalAmmoLeft = totalPrimaryAmmoLeft;
        modInfo = PlayerData.playerdata.primaryModInfo;

        pView.RPC("RpcInitializeWeapon", RpcTarget.All, 1, equippedWep, modInfo.EquippedSuppressor, modInfo.EquippedSight);
    }

    [PunRPC]
    void RpcInitializeWeapon(int weaponCat, string equippedWep, string equippedSuppressor, string equippedSight) {
        weaponReady = false;
        currentlyEquippedType = weaponCat;
        if (!equipmentScript.isFirstPerson()) {
            animator.SetInteger("WeaponType", weaponCat);
        } else {
            weaponActionScript.animatorFpc.SetInteger("WeaponType", weaponCat);
        }
        EquipWeapon(equippedWep, equippedSuppressor, equippedSight, null);
    }

    public void DrawPrimary()
    {
        if (currentlyEquippedType == 1) return;
        DrawWeapon(1);
        currentlyEquippedType = 1;
    }

    void DrawSecondary()
    {
        if (currentlyEquippedType == 2) return;
        DrawWeapon(2);
        currentlyEquippedType = 2;
    }

    void DrawSupport()
    {
        if (currentlyEquippedType == 4 || (totalSupportAmmoLeft <= 0 && currentAmmoSupport <= 0)) return;
        DrawWeapon(4);
        currentlyEquippedType = 4;
    }

    public void DrawBubbleShieldSkill()
    {
        if (CheckCanSwitchWeapon()) {
            if (currentlyEquippedType == -1) return;
            DrawWeapon(-1);
            currentlyEquippedType = -1;
        }
    }

    void Update() {
        if (!initialized) {
            return;
        }
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        // Debug.Log("isCockingGrenade: " + weaponActionScript.isCockingGrenade);
        // Debug.Log("isCocking: " + weaponActionScript.isCocking);
        // Debug.Log("isUsingBooster: " + weaponActionScript.isUsingBooster);
        UpdateMunitionsEngineering();

        if (CheckCanSwitchWeapon()) {
            if (PlayerPreferences.playerPreferences.KeyWasPressed("Primary") || (currentlyEquippedType == 4 && weaponActionScript.totalAmmoLeft <= 0 && weaponActionScript.currentAmmo <= 0)) {
                DrawPrimary();
            } else if (PlayerPreferences.playerPreferences.KeyWasPressed("Secondary")) {
                DrawSecondary();
            } else if (PlayerPreferences.playerPreferences.KeyWasPressed("Support")) {
                DrawSupport();
            }
        }
        HideMeleeWeapon();
    }

    void UpdateMunitionsEngineering()
    {
        if (playerActionScript.skillController.MunitionsEngineeringFlag()) {
            int lvl = playerActionScript.skillController.GetMunitionsEngineeringLevel();
            if (lvl == 1) {
                RegenerateAmmo(1);
            } else if (lvl == 2) {
                RegenerateAmmo(2);
            } else if (lvl == 3) {
                RegenerateAmmo(3);
            }
            playerActionScript.skillController.MunitionsEngineeringReset();
        }
    }

    void RegenerateAmmo(int ammo)
    {
        if (currentlyEquippedType == 1) {
            int maxAmmo = GetLoadedMaxAmmoForCurrentWep();
            weaponActionScript.totalAmmoLeft += ammo;
            if (weaponActionScript.totalAmmoLeft > maxAmmo) {
                weaponActionScript.totalAmmoLeft = maxAmmo;
            }
            SyncAmmoCounts();
        } else if (currentlyEquippedType == 2) {
            if (weaponActionScript.weaponStats.category != "Launcher") {
                int maxAmmo = GetLoadedMaxAmmoForCurrentWep();
                weaponActionScript.totalAmmoLeft += ammo;
                if (weaponActionScript.totalAmmoLeft > maxAmmo) {
                    weaponActionScript.totalAmmoLeft = maxAmmo;
                }
                SyncAmmoCounts();
            }
        }
    }

    // If the user has a grenade cocked or is currently loading a weapon, don't let him switch weapons
    bool CheckCanSwitchWeapon() {
        if (!equipmentScript.isFirstPerson()) {
            return false;
        } else if (playerActionScript.fpc.GetIsSwimming()) {
            return false;
        } else if (weaponActionScript.isCockingGrenade) {
            return false;
        } else if (weaponActionScript.isUsingBooster) {
            return false;
        } else if (weaponActionScript.isUsingDeployable) {
            return false;
        } else if (weaponActionScript.deployInProgress) {
            return false;
        } else if (weaponActionScript.hudScript.container.pauseMenuGUI.pauseActive) {
            return false;
        } else if (weaponActionScript.hudScript.commandDelay > 0f) {
            return false;
        } else if (weaponActionScript.hudScript.skillDelay > 0f) {
            return false;
        } else if (weaponActionScript.hudScript.guardianAngelDelay > 0f) {
            return false;
        }
        return true;
    }

    public void HideWeapon(bool b) {
        if (weaponActionScript != null) {
            WeaponMeta ws = weaponActionScript.weaponMetaData;
            if (ws == null) return;
            if (!b) {
                if (!ws.weaponParts[0].enabled) {
                    for (int i = 0; i < ws.weaponParts.Length; i++) {
                        ws.weaponParts[i].enabled = true;
                    }
                }
            } else {
                if (ws.weaponParts[0].enabled) {
                    for (int i = 0; i < ws.weaponParts.Length; i++) {
                        ws.weaponParts[i].enabled = false;
                    }
                }
            }
        }
    }

    void HideMeleeWeapon() {
        if (weaponActionScript != null) {
            WeaponMeta ws = weaponActionScript.meleeMetaData;
            if (ws == null) return;
            if (weaponActionScript.isMeleeing) {
                if (!ws.weaponParts[0].enabled) {
                    for (int i = 0; i < ws.weaponParts.Length; i++) {
                        ws.weaponParts[i].enabled = true;
                    }
                }
            } else {
                if (ws.weaponParts[0].enabled) {
                    for (int i = 0; i < ws.weaponParts.Length; i++) {
                        ws.weaponParts[i].enabled = false;
                    }
                }
            }
        }
    }

    public void ToggleWarhead(bool b) {
        if (weaponActionScript != null) {
            WeaponMeta ws = weaponActionScript.weaponMetaData;
            if (ws == null) return;
            if (ws.warheadRenderer != null) {
                ws.warheadRenderer.gameObject.SetActive(b);
            }
        }
    }

    public void DrawWeapon(int weaponCat) {
        string equippedWep = "";
        ModInfo modInfo = null;
        
        if (weaponCat == 1)
        {
            weaponActionScript.hudScript.ToggleCrosshair(false);
            equippedWep = equippedPrimaryWeapon;
            modInfo = PlayerData.playerdata.primaryModInfo;
        }
        else if (weaponCat == 2)
        {
            weaponActionScript.hudScript.ToggleCrosshair(false);
            equippedWep = equippedSecondaryWeapon;
            modInfo = PlayerData.playerdata.secondaryModInfo;
        }
        else if (weaponCat == 4)
        {
            if (InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].category.Equals("Explosive")) {
                weaponActionScript.hudScript.ToggleCrosshair(true);
            }
            HideWeapon(false);
            equippedWep = equippedSupportWeapon;
            modInfo = PlayerData.playerdata.supportModInfo;
        } else if (weaponCat == -1)
        {
            weaponActionScript.hudScript.ToggleCrosshair(false);
            equippedWep = "Bubble Shield (Skill)";
            modInfo = PlayerData.playerdata.supportModInfo;
        }
        pView.RPC("RpcDrawWeapon", RpcTarget.All, weaponCat, equippedWep, modInfo.EquippedSuppressor, modInfo.EquippedSight);
    }

    [PunRPC]
    private void RpcDrawWeapon(int weaponCat, string equippedWep, string equippedSuppressor, string equippedSight) {
        weaponReady = false;
        currentlyEquippedType = weaponCat;
        if (weaponCat == -1) weaponCat = 4;
        if (!equipmentScript.isFirstPerson()) {
            animator.SetInteger("WeaponType", weaponCat);
            EquipWeapon(equippedWep, equippedSuppressor, equippedSight, null);
        } else {
            weaponActionScript.animatorFpc.SetInteger("WeaponType", weaponCat);
            weaponActionScript.animatorFpc.SetTrigger("HolsterWeapon");
        }
    }

    void WeaponRefresh(string equippedSuppressor, string equippedSight) {
        animator.SetInteger("WeaponType", currentlyEquippedType);
        Weapon w = InventoryScript.itemData.weaponCatalog[equippedWepInGame];
        GameObject wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
        weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[equippedWepInGame]);
        weaponHolder.SetWeaponPosition(false);
        WeaponMods wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
        if (w.suppressorCompatible) {
            if (equippedSuppressor != null && equippedSuppressor != "") {
                wm.EquipSuppressor(equippedSuppressor);
            }
        }
        if (w.sightCompatible) {
            if (equippedSight != null && equippedSight != "") {
                wm.EquipSight(equippedSight);
            }
        }
        drawnWeaponReference = wepEquipped;
    }

    void SetCurrentAmmo(int weaponCat) {
        if (weaponCat == 1)
        {
            weaponActionScript.currentAmmo = currentAmmoPrimary;
            weaponActionScript.totalAmmoLeft = totalPrimaryAmmoLeft;
        }
        else if (weaponCat == 2)
        {
            weaponActionScript.currentAmmo = currentAmmoSecondary;
            weaponActionScript.totalAmmoLeft = totalSecondaryAmmoLeft;
        }
        else if (weaponCat == 4)
        {
            weaponActionScript.currentAmmo = currentAmmoSupport;
            weaponActionScript.totalAmmoLeft = totalSupportAmmoLeft;
        }
        else if (weaponCat == -1)
        {
            weaponActionScript.currentAmmo = 1;
            weaponActionScript.totalAmmoLeft = 0;
        }
    }

    void EquipAssaultRifleInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipSmgInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipLmgInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipShotgunInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipPistolInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipLauncherInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipSniperRifleInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipExplosiveInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipBoosterInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipDeployableInGame() {
        if (equipmentScript.isFirstPerson()) {
            weaponHolderFpc.SetWeaponPosition(true);
        } else {
            weaponHolder.SetWeaponPosition(false);
        }
    }

    void EquipKnifeInGame() {
        if (equipmentScript.isFirstPerson()) {
            meleeHolderFpc.SetWeaponPosition(true);
        } else {
            meleeHolder.SetWeaponPosition(false);
        }
    }

    public void SwitchWeaponToFullBody() {
        weaponHolder.SetWeapon(drawnWeaponReference.transform, false);
    }

    public void SwitchWeaponToFpcBody() {
        weaponHolderFpc.SetWeapon(drawnWeaponReference.transform, true);
    }

    public void PreviewWeapon(string name) {
        if (equippedPrimaryWeapon == name) return;
        Character c = InventoryScript.itemData.characterCatalog[equipmentScript.equippedCharacter];

        Weapon w = InventoryScript.itemData.weaponCatalog[name];
        string weaponType = w.category;
        GameObject wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
        if (w.type == "Primary") {
            equippedPrimaryWeapon = name;
        }

        if (c.gender == 'M') {
            SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }
    }

    public void EquipWeapon(string weaponName, string suppressorName, string sightName, GameObject shopItemRef) {
        if (onTitle) {
            EquipWeaponOnTitle(weaponName, suppressorName, sightName, shopItemRef);
        } else {
            EquipWeaponInGame(weaponName, suppressorName, sightName);
        }
    }

    void EquipWeaponInGame(string weaponName, string suppressorName, string sightName) {
        Weapon w = InventoryScript.itemData.weaponCatalog[weaponName];
        GameObject wepEquipped = null;
        string weaponType = InventoryScript.itemData.weaponCatalog[weaponName].category;
        switch (weaponType) {
            case "Assault Rifle":
                currentlyEquippedType = 1;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 1);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipAssaultRifleInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(0);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "SMG":
                currentlyEquippedType = 1;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 1);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipSmgInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(1);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "LMG":
                currentlyEquippedType = 1;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 1);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipLmgInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(2);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "Shotgun":
                currentlyEquippedType = 1;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 1);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", true);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipShotgunInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(3);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "Sniper Rifle":
                currentlyEquippedType = 1;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 1);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", true);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipSniperRifleInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(4);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "Pistol":
                currentlyEquippedType = 2;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 2);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipPistolInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(5);
                    SetWeaponCulling(wepEquipped);
                    weaponActionScript.SetSpread(weaponActionScript.weaponStats.accuracy);
                }
                break;
            case "Launcher":
                currentlyEquippedType = 2;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 2);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipLauncherInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(6);
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Explosive":
                currentlyEquippedType = 4;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipExplosiveInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(7);
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Booster":
                currentlyEquippedType = 4;
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipBoosterInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(8);
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Deployable":
                if (weaponName.EndsWith("(Skill)")) {
                    currentlyEquippedType = -1;
                } else {
                    currentlyEquippedType = 4;
                }
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                    weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                    weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                    weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                    SetWeaponCulling(wepEquipped);
                } else {
                    wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                }
                equippedWepInGame = weaponName;
                EquipDeployableInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                HideWeapon(currentAmmoSupport == 0 && totalSupportAmmoLeft == 0);
                weaponActionScript.SetCurrentAimDownSightPos(sightName);
                weaponActionScript.hudScript.EquipSightCrosshair(false);
                if (equipmentScript.isFirstPerson()) {
                    playerActionScript.skillController.InitializePassiveSkills(9);
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Knife":
                if (equipmentScript.isFirstPerson()) {
                    wepEquipped = meleeHolderFpc.LoadWeapon(w.prefabPath);
                    SetWeaponCulling(wepEquipped);
                } else {
                    wepEquipped = meleeHolder.LoadWeapon(w.prefabPath);
                }
                EquipKnifeInGame();
                weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponMeta>(), InventoryScript.itemData.weaponCatalog[weaponName]);
                HideMeleeWeapon();
                wepEquipped = null;
                break;
        }

        if (wepEquipped != null)
        {
            drawnWeaponReference = wepEquipped;
        }

        SetCurrentAmmo(currentlyEquippedType);
    }

    void EquipWeaponOnTitle(string weaponName, string suppressorName, string sightName, GameObject shopItemRef) {
        if (weaponName.Equals(equippedPrimaryWeapon) || weaponName.Equals(equippedSecondaryWeapon) || weaponName.Equals(equippedSupportWeapon) || weaponName.Equals(equippedMeleeWeapon)) return;
        // Shop GUI stuff
        if (shopItemRef != null) {
            // Sets item that you unequipped to white
            if (ts.currentlyEquippedWeaponPrefab != null) {
                ts.currentlyEquippedWeaponPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
                ts.currentlyEquippedWeaponPrefab = shopItemRef;
            }
        }

        string type = InventoryScript.itemData.weaponCatalog[weaponName].type;

        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["equipped" + type] = weaponName;
        
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

    public void EquipMod(string modType, string modName, string equipOnWeapon, GameObject shopItemRef) {
        // If no mod equipped, don't equip anything
        if (modName == null || modName.Equals("") || equipOnWeapon == null) return;
        if (onTitle) {
            EquipModOnTitle(modType, modName, equipOnWeapon, shopItemRef);
        } else {
            EquipModInGame(modType, modName, equipOnWeapon);
        }
    }

    void EquipModOnTitle(string modType, string modName, string equipOnWeapon, GameObject shopItemRef) {
        switch (modType) {
            case "Suppressor":
                EquipSuppressorOnTitle(modName, equipOnWeapon);
                break;
            case "Sight":
                EquipSightOnTitle(modName, equipOnWeapon);
                break;
        }
        if (shopItemRef != null) {
            // Sets item that you unequipped to white
            if (ts.currentlyEquippedModPrefab != null) {
                ts.currentlyEquippedModPrefab.GetComponent<ShopItemScript>().ToggleModEquippedIndicator(false);
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponent<ShopItemScript>().ToggleModEquippedIndicator(true);
                ts.currentlyEquippedModPrefab = shopItemRef;
            }
        }
    }

    void EquipModInGame(string modType, string modName, string equipOnWeapon) {
        switch (modType) {
            case "Suppressor":
                EquipSuppressorInGame(modName, equipOnWeapon);
                break;
            case "Sight":
                EquipSightInGame(modName, equipOnWeapon);
                break;
        }
    }

    public void UnequipMod(string modType, string unequipFromWeapon) {
        if (onTitle) {
            UnequipModOnTitle(modType, unequipFromWeapon);
        } else {
            UnequipModInGame(modType, unequipFromWeapon);
        }
    }

    void UnequipModOnTitle(string modType, string unequipFromWeapon) {
        // Remove the mod from the active gun
        switch(modType) {
            case "Suppressor":
                string equippedSuppressor = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSuppressor();
                if (equippedSuppressor == null || equippedSuppressor.Equals("")) return;
                UnequipSuppressorOnTitle(unequipFromWeapon);
                break;
            case "Sight":
                string equippedSight = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSight();
                if (equippedSight == null || equippedSight.Equals("")) return;
                UnequipSightOnTitle(unequipFromWeapon);
                break;
        }
    }

    void UnequipModInGame(string modType, string unequipFromWeapon) {
        // Remove the mod from the active gun
        switch(modType) {
            case "Suppressor":
                string equippedSuppressor = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSuppressor();
                if (equippedSuppressor == null || equippedSuppressor.Equals("")) return;
                UnequipSuppressorInGame(unequipFromWeapon);
                break;
            case "Sight":
                string equippedSight = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSight();
                if (equippedSight == null || equippedSight.Equals("")) return;
                UnequipSightInGame(unequipFromWeapon);
                break;
        }
    }

    void EquipSuppressorOnTitle(string modName, string equipOnWeapon) {
        if (equipOnWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            wm.EquipSuppressor(modName);
        }
    }

    void EquipSuppressorInGame(string modName, string equipOnWeapon) {
        // If primary, attach to weapon on title screen and in-game
        if (equipOnWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            }
            wm.EquipSuppressor(modName);
            // drawnSuppressorReference = wm.suppressorRef;
            Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
            weaponActionScript.ModifyWeaponStats(suppressorBoosts.damageBoost, suppressorBoosts.accuracyBoost, suppressorBoosts.recoilBoost*.03f, suppressorBoosts.rangeBoost, suppressorBoosts.clipCapacityBoost);
        } else if (equipOnWeapon.Equals(equippedSecondaryWeapon)) {
            // If secondary, only attach to weapon if in-game
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            }
            wm.EquipSuppressor(modName);
            Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
            weaponActionScript.ModifyWeaponStats(suppressorBoosts.damageBoost, suppressorBoosts.accuracyBoost, suppressorBoosts.recoilBoost*.03f, suppressorBoosts.rangeBoost, suppressorBoosts.clipCapacityBoost);
        }
    }

    void UnequipSuppressorOnTitle(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            wm.UnequipSuppressor();
        }
    }

    void UnequipSuppressorInGame(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
            weaponActionScript.ModifyWeaponStats(-suppressorBoosts.damageBoost, -suppressorBoosts.accuracyBoost, -suppressorBoosts.recoilBoost*.03f, -suppressorBoosts.rangeBoost, -suppressorBoosts.clipCapacityBoost);
            wm.UnequipSuppressor();
        } else if (unequipFromWeapon.Equals(equippedSecondaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
            weaponActionScript.ModifyWeaponStats(-suppressorBoosts.damageBoost, -suppressorBoosts.accuracyBoost, -suppressorBoosts.recoilBoost*.03f, -suppressorBoosts.rangeBoost, -suppressorBoosts.clipCapacityBoost);
            wm.UnequipSuppressor();
        }
    }

    void EquipSightOnTitle(string modName, string equipOnWeapon) {
        if (equipOnWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            wm.EquipSight(modName);
        }
    }

    void EquipSightInGame(string modName, string equipOnWeapon) {
        if (equipOnWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            }
            wm.EquipSight(modName);
            Mod sightBoosts = wm.GetEquippedSightStats();
            weaponActionScript.ModifyWeaponStats(sightBoosts.damageBoost, sightBoosts.accuracyBoost, sightBoosts.recoilBoost, sightBoosts.rangeBoost, sightBoosts.clipCapacityBoost);
            weaponActionScript.hudScript.EquipSightCrosshair(true);
            weaponActionScript.hudScript.SetSightCrosshairForSight(modName);
        } else if (equipOnWeapon.Equals(equippedSecondaryWeapon)) {
            // If secondary, only attach to weapon if in-game
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            }
            wm.EquipSight(modName);
            Mod sightBoosts = wm.GetEquippedSightStats();
            weaponActionScript.ModifyWeaponStats(sightBoosts.damageBoost, sightBoosts.accuracyBoost, sightBoosts.recoilBoost*.03f, sightBoosts.rangeBoost, sightBoosts.clipCapacityBoost);
            weaponActionScript.hudScript.EquipSightCrosshair(true);
            weaponActionScript.hudScript.SetSightCrosshairForSight(modName);
        }
    }

    void UnequipSightOnTitle(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            wm.UnequipSight();
        }
    }

    void UnequipSightInGame(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            Mod sightBoosts = wm.GetEquippedSightStats();
            weaponActionScript.ModifyWeaponStats(-sightBoosts.damageBoost, -sightBoosts.accuracyBoost, -sightBoosts.recoilBoost*.03f, -sightBoosts.rangeBoost, -sightBoosts.clipCapacityBoost);
            wm.UnequipSight();
        } else if (unequipFromWeapon.Equals(equippedSecondaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            Mod sightBoosts = wm.GetEquippedSightStats();
            weaponActionScript.ModifyWeaponStats(-sightBoosts.damageBoost, -sightBoosts.accuracyBoost, -sightBoosts.recoilBoost*.03f, -sightBoosts.rangeBoost, -sightBoosts.clipCapacityBoost);
            wm.UnequipSight();
        }
    }

    public void SetTitleWeaponPositions(Vector3 p) {
        weaponHolder.SetWeaponPositionForTitle(p);
        // if (ts != null) {
        //     if (ts.currentCharGender == 'M') {
        //         weaponHolder.SetWeaponPositionForTitle(new Vector3(-0.02f, 0.05f, 0.03f));
        //     } else {
        //         weaponHolder.SetWeaponPositionForTitle(new Vector3(-0.01f, 0.02f, 0.02f));
        //     }
        // }
    }

    public void DespawnPlayer()
    {
        ToggleWeaponVisible(false);
    }

    public void RespawnPlayer()
    {
        pView.RPC("RpcToggleWeaponVisible", RpcTarget.All, true);
        weaponHolderFpc.SwitchWeaponToRightHand();
        //drawnSuppressorRenderer.enabled = true;
        // DrawPrimary();
        MaxRefillAllAmmo(true);
        InitializeWeapon();
    }

    [PunRPC]
    void RpcToggleWeaponVisible(bool b) {
        if (playerActionScript.isNotOnTeamMap) return;
        drawnWeaponReference.SetActive(b);
    }

    public void ToggleWeaponVisible(bool b) {
        drawnWeaponReference.SetActive(b);
    }

    void SetWeaponCulling(GameObject weapon) {
        WeaponMeta ws = weapon.GetComponent<WeaponMeta>();
        WeaponMods wsm = weapon.GetComponent<WeaponMods>();
        foreach (MeshRenderer part in ws.weaponParts) {
            part.gameObject.layer = 16;
        }
        if (wsm != null) {
            if (wsm.suppressorRef != null) {
                MeshRenderer[] suppressorRenderers = wsm.suppressorRef.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer part in suppressorRenderers) {
                    part.gameObject.layer = 16;
                }
            }
            if (wsm.sightRef != null) {
                MeshRenderer[] sightRenderers = wsm.sightRef.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer part in sightRenderers) {
                    part.gameObject.layer = 16;
                }
            }
            if (wsm.sightMountRef != null) {
                MeshRenderer[] mountRenderers = wsm.sightMountRef.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer part in mountRenderers) {
                    part.gameObject.layer = 16;
                }
            }
        }
    }

    public void SyncAmmoCounts() {
        if (currentlyEquippedType == 1) {
            currentAmmoPrimary = weaponActionScript.currentAmmo;
            totalPrimaryAmmoLeft = weaponActionScript.totalAmmoLeft;
        } else if (currentlyEquippedType == 2) {
            currentAmmoSecondary = weaponActionScript.currentAmmo;
            totalSecondaryAmmoLeft = weaponActionScript.totalAmmoLeft;
        } else if (currentlyEquippedType == 4) {
            currentAmmoSupport = weaponActionScript.currentAmmo;
            totalSupportAmmoLeft = weaponActionScript.totalAmmoLeft;
        }
    }

    public void RefreshAmmoCounts() {
        if (currentlyEquippedType == 1) {
            weaponActionScript.currentAmmo = currentAmmoPrimary;
            weaponActionScript.totalAmmoLeft = totalPrimaryAmmoLeft;
        } else if (currentlyEquippedType == 2) {
            weaponActionScript.currentAmmo = currentAmmoSecondary;
            weaponActionScript.totalAmmoLeft = totalSecondaryAmmoLeft;
        } else if (currentlyEquippedType == 4) {
            weaponActionScript.currentAmmo = currentAmmoSupport;
            weaponActionScript.totalAmmoLeft = totalSupportAmmoLeft;
        }
    }

    public void MaxRefillAllAmmo(bool includeSupport = false) {
        MaxRefillAmmoOnPrimary();
        MaxRefillAmmoOnSecondary();
        if (includeSupport) {
            MaxRefillAmmoOnSupport();
        }
        RefreshAmmoCounts();
    }

    public void MaxRefillAmmoOnPrimary() {
        totalPrimaryAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].maxAmmo + (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].clipCapacity * playerActionScript.skillController.GetProviderBoost())) - currentAmmoPrimary;
    }

    public void RefillAmmoOnPrimary(int amt)
    {
        totalPrimaryAmmoLeft += amt;
        totalPrimaryAmmoLeft = Mathf.Min((InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].maxAmmo + (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedPrimary].clipCapacity * playerActionScript.skillController.GetProviderBoost())) - currentAmmoPrimary, totalPrimaryAmmoLeft);
    }

    public void MaxRefillAmmoOnSecondary() {
        totalSecondaryAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].maxAmmo + (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedSecondary].clipCapacity * playerActionScript.skillController.GetProviderBoost())) - currentAmmoSecondary;
    }

    public void MaxRefillAmmoOnSupport() {
        totalSupportAmmoLeft = (InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].maxAmmo + (InventoryScript.itemData.weaponCatalog[PlayerData.playerdata.info.EquippedSupport].clipCapacity * playerActionScript.skillController.GetProviderBoost())) - currentAmmoSupport;
    }

    public void EquipWeaponForSetup(string weaponName, string characterSelected) {
        // Get the weapon from the weapon catalog for its properties
        char chararacterGender = InventoryScript.itemData.characterCatalog[characterSelected].gender;
        Weapon w = InventoryScript.itemData.weaponCatalog[weaponName];
        GameObject wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);

        if (chararacterGender == 'M') {
            SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }
    }

    [PunRPC]
	void RpcAskServerForDataWeps() {
        if (!pView.IsMine) return;
        int primaryAmmoLeft = totalPrimaryAmmoLeft;
        int secondaryAmmoLeft = totalSecondaryAmmoLeft;
        int supportAmmoLeft = totalSupportAmmoLeft;
        int currentAmmoP = currentAmmoPrimary;
        int currentAmmoSec = currentAmmoSecondary;
        int currentAmmoSupp = currentAmmoSupport;
        string suppEquipped = null;
        string sightEquipped = null;
        if (currentlyEquippedType == 1) {
            suppEquipped = PlayerData.playerdata.primaryModInfo.EquippedSuppressor;
            sightEquipped = PlayerData.playerdata.primaryModInfo.EquippedSight;
        } else if (currentlyEquippedType == 2) {
            suppEquipped = PlayerData.playerdata.secondaryModInfo.EquippedSuppressor;
            sightEquipped = PlayerData.playerdata.secondaryModInfo.EquippedSight;
        } else if (currentlyEquippedType == 4) {
            suppEquipped = PlayerData.playerdata.supportModInfo.EquippedSuppressor;
            sightEquipped = PlayerData.playerdata.supportModInfo.EquippedSight;
        }
		pView.RPC("RpcSyncDataWeps", RpcTarget.Others, equippedPrimaryWeapon, equippedSecondaryWeapon, equippedSupportWeapon, equippedMeleeWeapon, primaryAmmoLeft,
            secondaryAmmoLeft, supportAmmoLeft, currentAmmoP, currentAmmoSec, currentAmmoSupp, currentlyEquippedType, weaponReady, equippedWepInGame,
            suppEquipped, sightEquipped);
	}

	[PunRPC]
	void RpcSyncDataWeps(string equippedPrimaryWeapon, string equippedSecondaryWeapon, string equippedSupportWeapon, string equippedMeleeWeapon, int totalPrimaryAmmoLeft,
        int totalSecondaryAmmoLeft, int totalSupportAmmoLeft, int currentAmmoPrimary, int currentAmmoSecondary, int currentAmmoSupport, int currentlyEquippedType, bool weaponReady, string equippedWepInGame,
        string currentlyEquippedSupp, string currentlyEquippedSight) {
            this.equippedPrimaryWeapon = equippedPrimaryWeapon;
            this.equippedSecondaryWeapon = equippedSecondaryWeapon;
            this.equippedSupportWeapon = equippedSupportWeapon;
            this.equippedMeleeWeapon = equippedMeleeWeapon;
            this.weaponReady = weaponReady;
            this.currentlyEquippedType = currentlyEquippedType;
            this.totalPrimaryAmmoLeft = totalPrimaryAmmoLeft;
            this.totalSecondaryAmmoLeft = totalSecondaryAmmoLeft;
            this.totalSupportAmmoLeft = totalSupportAmmoLeft;
            this.currentAmmoPrimary = currentAmmoPrimary;
            this.currentAmmoSecondary = currentAmmoSecondary;
            this.currentAmmoSupport = currentAmmoSupport;
            this.equippedWepInGame = equippedWepInGame;
            if (weaponHolder.weapon == null) {
                SyncWeps(currentlyEquippedSupp, currentlyEquippedSight);
            }
	}

    void SyncWeps(string equippedSuppressor, string equippedSight) {
        WeaponRefresh(equippedSuppressor, equippedSight);
    }

    public int GetLoadedMaxAmmoForCurrentWep()
    {
        return (weaponActionScript.weaponStats.maxAmmo + (weaponActionScript.weaponStats.clipCapacity * playerActionScript.skillController.GetProviderBoost())) - weaponActionScript.weaponStats.clipCapacity;
    }

}
