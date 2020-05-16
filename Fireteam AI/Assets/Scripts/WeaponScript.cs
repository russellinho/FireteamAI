using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

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
    public string equippedWep;
    public int currentlyEquippedType;
    public int totalPrimaryAmmoLeft;
    public int totalSecondaryAmmoLeft;
    public int totalSupportAmmoLeft;
    public int currentAmmoPrimary;
    public int currentAmmoSecondary;
    public int currentAmmoSupport;

    private GameObject drawnWeaponReference;
    // private GameObject drawnSuppressorReference;
    // private GameObject drawnSightReference;

    public bool weaponReady;
    public PhotonView pView;

    private bool onTitle;
    private bool onSetup;
    public char setupGender;

    void Awake() {
        // If the photon view is null, then the player is not in-game
        if (!equipmentScript.isFirstPerson()) {
            if (pView == null) {
                if (SceneManager.GetActiveScene().name.Equals("Title")) {
                    onTitle = true;
                } else {
                    onTitle = false;
                    onSetup = true;
                }
                animator.SetBool("onTitle", true);
            } else {
                onTitle = false;
                if (animator != null) {
                    animator.SetBool("onTitle", false);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        if (onTitle)
        {
            ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            equipmentScript = GetComponent<EquipmentScript>();
            weaponReady = false;
        } else if (onSetup) {
            drawnWeaponReference = weaponHolder.LoadWeapon("Models/Weapons/Primary/Assault Rifles/M4A1");
            if (setupGender == 'M') {
                SetTitleWeaponPositions(drawnWeaponReference.GetComponent<WeaponStats>().titleHandPositionsMale);
            } else {
                SetTitleWeaponPositions(drawnWeaponReference.GetComponent<WeaponStats>().titleHandPositionsFemale);
            }
        } else {
            //EquipWeapon(PlayerData.playerdata.info.equippedPrimaryType, PlayerData.playerdata.info.equippedPrimary, null);
            //EquipWeapon(PlayerData.playerdata.info.equippedSecondaryType, PlayerData.playerdata.info.equippedSecondary, null);
            equippedPrimaryWeapon = PlayerData.playerdata.info.equippedPrimary;
            equippedSecondaryWeapon = PlayerData.playerdata.info.equippedSecondary;
            equippedSupportWeapon = PlayerData.playerdata.info.equippedSupport;
            equippedMeleeWeapon = PlayerData.playerdata.info.equippedMelee;
            currentAmmoPrimary = InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].clipCapacity;
            currentAmmoSecondary = InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].clipCapacity;
            currentAmmoSupport = InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].clipCapacity;
            totalPrimaryAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].maxAmmo - currentAmmoPrimary;
            totalSecondaryAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].maxAmmo - currentAmmoSecondary;
            totalSupportAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].maxAmmo - currentAmmoSupport;
            equippedWep = equippedPrimaryWeapon;
            //DrawWeapon(1);
            InitializeWeapon();
            InitializeMelee();
        }
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

        pView.RPC("RpcInitializeWeapon", RpcTarget.All, 1, equippedWep, modInfo.equippedSuppressor, modInfo.equippedSight);
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

    void DrawPrimary()
    {
        if (currentlyEquippedType == 1) return;
        DrawWeapon(1);
        currentlyEquippedType = 1;
        weaponActionScript.firingMode = WeaponActionScript.FireMode.Auto;
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

    void Update() {
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        // Debug.Log("isCockingGrenade: " + weaponActionScript.isCockingGrenade);
        // Debug.Log("isCocking: " + weaponActionScript.isCocking);
        // Debug.Log("isUsingBooster: " + weaponActionScript.isUsingBooster);
        if (CheckCanSwitchWeapon()) {
            if (Input.GetKeyDown(KeyCode.Alpha1) || (currentlyEquippedType == 4 && weaponActionScript.totalAmmoLeft <= 0 && weaponActionScript.currentAmmo <= 0)) {
                DrawPrimary();
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                DrawSecondary();
            } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                DrawSupport();
            }
        }
        HideMeleeWeapon();
    }

    // If the user has a grenade cocked or is currently loading a weapon, don't let him switch weapons
    bool CheckCanSwitchWeapon() {
        if (!equipmentScript.isFirstPerson()) {
            return false;
        } else if (weaponActionScript.isCockingGrenade) {
            return false;
        } else if (weaponActionScript.isUsingBooster) {
            return false;
        } else if (weaponActionScript.isUsingDeployable) {
            return false;
        } else if (weaponActionScript.deployInProgress) {
            return false;
        } else if (weaponActionScript.hudScript.container.pauseMenuGUI.activeInHierarchy) {
            return false;
        }
        return true;
    }

    public void HideWeapon(bool b) {
        if (weaponActionScript != null) {
            WeaponStats ws = weaponActionScript.weaponStats;
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
            WeaponStats ws = weaponActionScript.meleeStats;
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
            WeaponStats ws = weaponActionScript.weaponStats;
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
        }
        pView.RPC("RpcDrawWeapon", RpcTarget.All, weaponCat, equippedWep, modInfo.equippedSuppressor, modInfo.equippedSight);
    }

    [PunRPC]
    private void RpcDrawWeapon(int weaponCat, string equippedWep, string equippedSuppressor, string equippedSight) {
        weaponReady = false;
        currentlyEquippedType = weaponCat;
        if (!equipmentScript.isFirstPerson()) {
            animator.SetInteger("WeaponType", weaponCat);
            EquipWeapon(equippedWep, equippedSuppressor, equippedSight, null);
        } else {
            weaponActionScript.animatorFpc.SetInteger("WeaponType", weaponCat);
            weaponActionScript.animatorFpc.SetTrigger("HolsterWeapon");
        }
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
    }

    void EquipAssaultRifle(string weaponName) {
        // Set animation and hand positions
        equippedPrimaryWeapon = weaponName;
        if (!animator.GetBool("onTitle")) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipSmg(string weaponName) {
        equippedPrimaryWeapon = weaponName;
        if (!animator.GetBool("onTitle")) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipLmg(string weaponName) {
        equippedPrimaryWeapon = weaponName;
        if (!animator.GetBool("onTitle")) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipShotgun(string weaponName) {
        equippedPrimaryWeapon = weaponName;
        if (!animator.GetBool("onTitle")) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipPistol(string weaponName) {
        // Set animation and hand positions
        equippedSecondaryWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipLauncher(string weaponName) {
        equippedSecondaryWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipSniperRifle(string weaponName) {
        equippedPrimaryWeapon = weaponName;
        if (!animator.GetBool("onTitle")) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
            }
        }
    }

    void EquipExplosive(string weaponName) {
        equippedSupportWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
                weaponHolder.ResetSteadyHand();
            }
        }
    }

    void EquipBooster(string weaponName) {
        equippedSupportWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
                weaponHolder.ResetSteadyHand();
            }
        }
    }

    void EquipDeployable(string weaponName) {
        equippedSupportWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                weaponHolderFpc.SetWeaponPosition(true);
            } else {
                weaponHolder.SetWeaponPosition(false);
                weaponHolder.ResetSteadyHand();
            }
        }
    }

    void EquipKnife(string weaponName) {
        equippedMeleeWeapon = weaponName;
        if (!onTitle) {
            if (equipmentScript.isFirstPerson()) {
                meleeHolderFpc.SetWeaponPosition(true);
            } else {
                meleeHolder.SetWeaponPosition(false);
            }
        }
    }

    public void SwitchWeaponToFullBody() {
        weaponHolder.SetWeapon(drawnWeaponReference.transform, false);
    }

    public void SwitchWeaponToFpcBody() {
        weaponHolderFpc.SetWeapon(drawnWeaponReference.transform, true);
    }

    public void EquipWeapon(string weaponName, string suppressorName, string sightName, GameObject shopItemRef) {
        if (onTitle && (weaponName.Equals(equippedPrimaryWeapon) || weaponName.Equals(equippedSecondaryWeapon) || weaponName.Equals(equippedSupportWeapon) || weaponName.Equals(equippedMeleeWeapon))) return;
        // Get the weapon from the weapon catalog for its properties
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
                equippedWep = weaponName;
                EquipAssaultRifle(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
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
                equippedWep = weaponName;
                EquipSmg(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
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
                equippedWep = weaponName;
                EquipLmg(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
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
                equippedWep = weaponName;
                EquipShotgun(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
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
                equippedWep = weaponName;
                EquipSniperRifle(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Pistol":
                if (!onTitle) {
                    currentlyEquippedType = 2;
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                        weaponActionScript.animatorFpc.SetInteger("WeaponType", 2);
                        weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                        weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                    } else {
                        wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                    }
                }
                equippedWep = weaponName;
                EquipPistol(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (w.suppressorCompatible) {
                    EquipMod("Suppressor", suppressorName, weaponName, null);
                }
                if (w.sightCompatible) {
                    EquipMod("Sight", sightName, weaponName, null);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Launcher":
                if (!onTitle) {
                    currentlyEquippedType = 2;
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                        weaponActionScript.animatorFpc.SetInteger("WeaponType", 2);
                        weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                        weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                    } else {
                        wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                    }
                }
                equippedWep = weaponName;
                EquipLauncher(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Explosive":
                if (!onTitle) {
                    currentlyEquippedType = 4;
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                        weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                        weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                        weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                    } else {
                        wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                    }
                }
                equippedWep = weaponName;
                EquipExplosive(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Booster":
                if (!onTitle) {
                    currentlyEquippedType = 4;
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                        weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                        weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                        weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                    } else {
                        wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                    }
                }
                equippedWep = weaponName;
                EquipBooster(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Deployable":
                if (!onTitle) {
                    currentlyEquippedType = 4;
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = weaponHolderFpc.LoadWeapon(w.prefabPath);
                        weaponActionScript.animatorFpc.SetInteger("WeaponType", 4);
                        weaponActionScript.animatorFpc.SetBool("isShotgun", false);
                        weaponActionScript.animatorFpc.SetBool("isBoltAction", false);
                        SetWeaponCulling(wepEquipped);
                    } else {
                        wepEquipped = weaponHolder.LoadWeapon(w.prefabPath);
                    }
                }
                equippedWep = weaponName;
                EquipDeployable(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                    HideWeapon(currentAmmoSupport == 0 && totalSupportAmmoLeft == 0);
                    weaponActionScript.SetCurrentAimDownSightPos(sightName);
                    weaponActionScript.hudScript.EquipSightCrosshair(false);
                }
                if (!onTitle && equipmentScript.isFirstPerson()) {
                    SetWeaponCulling(wepEquipped);
                }
                break;
            case "Knife":
                if (!onTitle) {
                    if (equipmentScript.isFirstPerson()) {
                        wepEquipped = meleeHolderFpc.LoadWeapon(w.prefabPath);
                        SetWeaponCulling(wepEquipped);
                    } else {
                        wepEquipped = meleeHolder.LoadWeapon(w.prefabPath);
                    }
                }
                EquipKnife(weaponName);
                if (!onTitle) {
                    weaponActionScript.SetWeaponStats(wepEquipped.GetComponent<WeaponStats>());
                }
                HideMeleeWeapon();
                wepEquipped = null;
                break;
        }
        
        if (wepEquipped != null)
        {
            drawnWeaponReference = wepEquipped;
        }
        
        if (onTitle) {
            // Shop GUI stuff
            if (shopItemRef != null) {
                // Sets item that you unequipped to white
                if (ts.currentlyEquippedItemPrefab != null) {
                    ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                }

                // Sets item that you just equipped to orange in the shop
                if (shopItemRef != null) {
                    shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
                    ts.currentlyEquippedItemPrefab = shopItemRef;
                }
            }

            if (wepEquipped != null) {
                if (ts.currentCharGender == 'M') {
                    SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponStats>().titleHandPositionsMale);
                } else {
                    SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponStats>().titleHandPositionsFemale);
                }
            }

            // Puts the item that you just equipped in its proper slot
            if (w.type.Equals("Primary")) {
                ts.equippedPrimarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
                ts.shopEquippedPrimarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
            } else if (w.type.Equals("Secondary")) {
                ts.equippedSecondarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
                ts.shopEquippedSecondarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
            } else if (w.type.Equals("Support")) {
                ts.equippedSupportSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
                ts.shopEquippedSupportSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
            } else if (w.type.Equals("Melee")) {
                ts.equippedMeleeSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
                ts.shopEquippedMeleeSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
            }
        } else {
            SetCurrentAmmo(currentlyEquippedType);
        }
    }

    public void EquipMod(string modType, string modName, string equipOnWeapon, GameObject shopItemRef) {
        // If no mod equipped, don't equip anything
        if (modName == null || modName.Equals("") || equipOnWeapon == null) return;
        // Load mod from catalog
        Mod m = InventoryScript.itemData.modCatalog[modName];
        switch (modType) {
            case "Suppressor":
                EquipSuppressor(modName, equipOnWeapon);
                break;
            case "Sight":
                EquipSight(modName, equipOnWeapon);
                break;
        }
        // Change shop item highlight if in the mod area
        if (onTitle) {
            if (shopItemRef != null) {
                // Sets item that you unequipped to white
                if (ts.currentlyEquippedModPrefab != null) {
                    ts.currentlyEquippedModPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
                }

                // Sets item that you just equipped to orange in the shop
                if (shopItemRef != null) {
                    shopItemRef.GetComponent<ShopItemScript>().ToggleEquippedIndicator(true);
                    ts.currentlyEquippedModPrefab = shopItemRef;
                }
            }
        }
    }

    public void UnequipMod(string modType, string unequipFromWeapon) {
        // Remove the mod from the active gun
        switch(modType) {
            case "Suppressor":
                string equippedSuppressor = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSuppressor();
                if (equippedSuppressor == null || equippedSuppressor.Equals("")) return;
                UnequipSuppressor(unequipFromWeapon);
                break;
            case "Sight":
                string equippedSight = weaponHolder.GetComponentInChildren<WeaponMods>().GetEquippedSight();
                if (equippedSight == null || equippedSight.Equals("")) return;
                UnequipSight(unequipFromWeapon);
                break;
        }
        // Change shop item highlight if in the mod area
        if (onTitle) {
            // Sets item that you unequipped to white
            if (ts.currentlyEquippedModPrefab != null) {
                ts.currentlyEquippedModPrefab.GetComponent<ShopItemScript>().ToggleEquippedIndicator(false);
            }
        }
    }
    // Assuming recoil = 3 is 100, and 0 is min, multiply by .03 to get recoil modifier
    // If in-game, attach to weapon and affect weapon stats
    private void EquipSuppressor(string modName, string equipOnWeapon) {
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
            if (!onTitle) {
                Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
                weaponActionScript.ModifyWeaponStats(suppressorBoosts.damageBoost, suppressorBoosts.accuracyBoost, suppressorBoosts.recoilBoost*.03f, suppressorBoosts.rangeBoost, suppressorBoosts.clipCapacityBoost, suppressorBoosts.maxAmmoBoost);
            }
        } else if (equipOnWeapon.Equals(equippedSecondaryWeapon)) {
            // If secondary, only attach to weapon if in-game
            if (!onTitle) {
                WeaponMods wm = null;
                if (equipmentScript.isFirstPerson()) {
                    wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
                } else {
                    wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
                }
                wm.EquipSuppressor(modName);
                Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
                weaponActionScript.ModifyWeaponStats(suppressorBoosts.damageBoost, suppressorBoosts.accuracyBoost, suppressorBoosts.recoilBoost*.03f, suppressorBoosts.rangeBoost, suppressorBoosts.clipCapacityBoost, suppressorBoosts.maxAmmoBoost);
            }
        }
    }

    private void UnequipSuppressor(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            if (!onTitle) {
                Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
                weaponActionScript.ModifyWeaponStats(-suppressorBoosts.damageBoost, -suppressorBoosts.accuracyBoost, -suppressorBoosts.recoilBoost*.03f, -suppressorBoosts.rangeBoost, -suppressorBoosts.clipCapacityBoost, -suppressorBoosts.maxAmmoBoost);
            }
            wm.UnequipSuppressor();
        } else if (unequipFromWeapon.Equals(equippedSecondaryWeapon)) {
            if (!onTitle) {
                WeaponMods wm = null;
                if (equipmentScript.isFirstPerson()) {
                    wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
                } else {
                    wm = weaponHolder.GetComponentInChildren<WeaponMods>();
                }
                Mod suppressorBoosts = wm.GetEquippedSuppressorStats();
                weaponActionScript.ModifyWeaponStats(-suppressorBoosts.damageBoost, -suppressorBoosts.accuracyBoost, -suppressorBoosts.recoilBoost*.03f, -suppressorBoosts.rangeBoost, -suppressorBoosts.clipCapacityBoost, -suppressorBoosts.maxAmmoBoost);
                wm.UnequipSuppressor();
            }
        }
    }

    void EquipSight(string modName, string equipOnWeapon) {
        // If primary, attach to weapon on title screen and in-game
        if (equipOnWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
            }
            wm.EquipSight(modName);
            // drawnSuppressorReference = wm.suppressorRef;
            if (!onTitle) {
                Mod sightBoosts = wm.GetEquippedSightStats();
                weaponActionScript.ModifyWeaponStats(sightBoosts.damageBoost, sightBoosts.accuracyBoost, sightBoosts.recoilBoost, sightBoosts.rangeBoost, sightBoosts.clipCapacityBoost, sightBoosts.maxAmmoBoost);
                weaponActionScript.hudScript.EquipSightCrosshair(true);
                weaponActionScript.hudScript.SetSightCrosshairForSight(modName);
            }
        } else if (equipOnWeapon.Equals(equippedSecondaryWeapon)) {
            // If secondary, only attach to weapon if in-game
            if (!onTitle) {
                WeaponMods wm = null;
                if (equipmentScript.isFirstPerson()) {
                    wm = weaponHolderFpc.weapon.GetComponentInChildren<WeaponMods>();
                } else {
                    wm = weaponHolder.weapon.GetComponentInChildren<WeaponMods>();
                }
                wm.EquipSight(modName);
                Mod sightBoosts = wm.GetEquippedSightStats();
                weaponActionScript.ModifyWeaponStats(sightBoosts.damageBoost, sightBoosts.accuracyBoost, sightBoosts.recoilBoost*.03f, sightBoosts.rangeBoost, sightBoosts.clipCapacityBoost, sightBoosts.maxAmmoBoost);
                weaponActionScript.hudScript.EquipSightCrosshair(true);
                weaponActionScript.hudScript.SetSightCrosshairForSight(modName);
            }
        }
    }

    void UnequipSight(string unequipFromWeapon) {
        if (unequipFromWeapon.Equals(equippedPrimaryWeapon)) {
            WeaponMods wm = null;
            if (equipmentScript.isFirstPerson()) {
                wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
            } else {
                wm = weaponHolder.GetComponentInChildren<WeaponMods>();
            }
            if (!onTitle) {
                Mod sightBoosts = wm.GetEquippedSightStats();
                weaponActionScript.ModifyWeaponStats(-sightBoosts.damageBoost, -sightBoosts.accuracyBoost, -sightBoosts.recoilBoost*.03f, -sightBoosts.rangeBoost, -sightBoosts.clipCapacityBoost, -sightBoosts.maxAmmoBoost);
            }
            wm.UnequipSight();
        } else if (unequipFromWeapon.Equals(equippedSecondaryWeapon)) {
            if (!onTitle) {
                WeaponMods wm = null;
                if (equipmentScript.isFirstPerson()) {
                    wm = weaponHolderFpc.GetComponentInChildren<WeaponMods>();
                } else {
                    wm = weaponHolder.GetComponentInChildren<WeaponMods>();
                }
                Mod sightBoosts = wm.GetEquippedSightStats();
                weaponActionScript.ModifyWeaponStats(-sightBoosts.damageBoost, -sightBoosts.accuracyBoost, -sightBoosts.recoilBoost*.03f, -sightBoosts.rangeBoost, -sightBoosts.clipCapacityBoost, -sightBoosts.maxAmmoBoost);
                wm.UnequipSight();
            }
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

    public void EquipDefaultWeapons() {
        equippedPrimaryWeapon = "AK-47";
        equippedSecondaryWeapon = "Glock23";
        equippedSupportWeapon = "M67 Frag";
        equippedMeleeWeapon = "Recon Knife";
        EquipWeapon(equippedPrimaryWeapon, null, null, null);
        EquipWeapon(equippedSecondaryWeapon, null, null, null);
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
        WeaponStats ws = weapon.GetComponent<WeaponStats>();
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

    public void MaxRefillAllAmmo() {
        MaxRefillAmmoOnPrimary();
        MaxRefillAmmoOnSecondary();
        MaxRefillAmmoOnSupport();
        RefreshAmmoCounts();
    }

    public void MaxRefillAmmoOnPrimary() {
        currentAmmoPrimary = InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].clipCapacity;
        totalPrimaryAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedPrimaryWeapon].maxAmmo;
    }

    public void MaxRefillAmmoOnSecondary() {
        currentAmmoSecondary = InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].clipCapacity;
        totalSecondaryAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedSecondaryWeapon].maxAmmo;
    }

    public void MaxRefillAmmoOnSupport() {
        currentAmmoSupport = InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].clipCapacity;
        totalSupportAmmoLeft = InventoryScript.itemData.weaponCatalog[equippedSupportWeapon].maxAmmo;
    }

}
