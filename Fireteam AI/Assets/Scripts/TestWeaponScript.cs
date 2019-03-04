using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestWeaponScript : MonoBehaviour
{
    public EquipmentScript equipmentScript;
    public WeaponHandlerScript weaponHolder;
    public Animator animator;
    public TitleControllerScript ts;
    public string equippedPrimaryWeapon;
    public string equippedPrimaryType;
    public string equippedSecondaryWeapon;
    public string equippedSecondaryType;
    public int currentlyEquippedType;
    private Dictionary<string, Vector3> rifleHandPositions;
    private Dictionary<string, Vector3> rifleIdleHandPositions;
    private Dictionary<string, Vector3> shotgunHandPositions;
    private Dictionary<string, Vector3> shotgunIdleHandPositions;
    private Dictionary<string, Vector3> sniperRifleHandPositions;
    private Dictionary<string, Vector3> sniperRifleIdleHandPositions;
    public bool weaponReady;

    void Awake() {
        if (SceneManager.GetActiveScene().name.Equals("Title")) {
            equippedPrimaryWeapon = "AK-47";
            equippedPrimaryType = "Assault Rifle";
            equippedSecondaryWeapon = "Glock23";
            equippedSecondaryType = "Pistol";
            animator.SetBool("onTitle", true);
        } else {
            animator.SetBool("onTitle", false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        equipmentScript = GetComponent<EquipmentScript>();
        weaponReady = false;
        // Populate hand positions
        if (ts.currentCharGender == 'M') {
            rifleHandPositions = new Dictionary<string, Vector3>();
            rifleHandPositions.Add("AK-47", new Vector3(-0.098f, 0.135f, 0.075f));
            rifleHandPositions.Add("M4A1", new Vector3(-0.065f, 0.126f, 0.04f));
        
            shotgunHandPositions = new Dictionary<string, Vector3>();
            shotgunHandPositions.Add("R870", new Vector3(-0.129f, 0.145f, 0.084f));

            sniperRifleHandPositions = new Dictionary<string, Vector3>();
            sniperRifleHandPositions.Add("L96A1", new Vector3(-0.054f, 0.115f, 0.029f));
        } else {
            rifleHandPositions = new Dictionary<string, Vector3>();
            rifleHandPositions.Add("AK-47", new Vector3(-0.098f, 0.135f, 0.075f));
            rifleHandPositions.Add("M4A1", new Vector3(-0.065f, 0.126f, 0.04f));
        
            shotgunHandPositions = new Dictionary<string, Vector3>();
            shotgunHandPositions.Add("R870", new Vector3(-0.129f, 0.145f, 0.084f));

            sniperRifleHandPositions = new Dictionary<string, Vector3>();
            sniperRifleHandPositions.Add("L96A1", new Vector3(-0.054f, 0.115f, 0.029f));
        }

        if (animator.GetBool("onTitle")) {
            EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon, null);
            EquipWeapon(equippedSecondaryType, equippedSecondaryWeapon, null);
        } else {
            DrawWeapon(1);
        }
    }

    void Update() {
        if (!animator.GetBool("onTitle")) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                DrawWeapon(1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                DrawWeapon(2);
            }
        }
    }

    void DrawWeapon(int weaponCat) {
        if (weaponCat == 1) {
            if (currentlyEquippedType == 1) return;
            weaponReady = false;
            currentlyEquippedType = 1;
            animator.SetInteger("WeaponType", 1);
            EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon, null);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        } else if (weaponCat == 2) {
            if (currentlyEquippedType == 2) return;
            weaponReady = false;
            animator.SetInteger("WeaponType", 2);
            currentlyEquippedType = 2;
            EquipWeapon(equippedSecondaryType, equippedSecondaryWeapon, null);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        }
    }

    void EquipAssaultRifle(string weaponName) {
        // Set animation and hand positions
        if (animator.GetBool("onTitle")) {
            weaponHolder.SetWeaponPosition(new Vector3(-0.02f, 0.05f, 0.03f));
        } else {
            weaponHolder.SetWeaponPosition();
            weaponHolder.SetSteadyHand(rifleHandPositions[weaponName]);
        }
    }

    void EquipShotgun(string weaponName) {
        if (animator.GetBool("onTitle")) {
            weaponHolder.SetWeaponPosition(new Vector3(-0.02f, 0.05f, 0.03f));
        } else {
            weaponHolder.SetWeaponPosition();
            weaponHolder.SetSteadyHand(shotgunHandPositions[weaponName]);
        }
    }

    public void EquipPistol(string weaponName) {
        // Set animation and hand positions
        weaponHolder.SetWeaponPosition();
        weaponHolder.ResetSteadyHand();
    }

    public void EquipSniperRifle(string weaponName) {
        if (animator.GetBool("onTitle")) {
            weaponHolder.SetWeaponPosition(new Vector3(-0.02f, 0.05f, 0.03f));
        } else {
            weaponHolder.SetWeaponPosition();
            weaponHolder.SetSteadyHand(sniperRifleHandPositions[weaponName]);
        }
    }

    public void EquipWeapon(string weaponType, string weaponName, GameObject shopItemRef) {
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.weaponCatalog[weaponName];
        switch (weaponType) {
            case "Assault Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipAssaultRifle(weaponName);
                break;
            case "Pistol":
                if (!animator.GetBool("onTitle")) {
                    currentlyEquippedType = 2;
                    weaponHolder.LoadWeapon(w.prefabPath);
                    EquipPistol(weaponName);
                }
                break;
            case "Shotgun":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipShotgun(weaponName);
                break;
            case "Sniper Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipSniperRifle(weaponName);
                break;
        }

        // Shop GUI stuff
        if (shopItemRef != null) {
            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }
        }

        // Puts the item that you just equipped in its proper slot
        if (w.type.Equals("Primary")) {
            ts.equippedPrimarySlot.GetComponentInChildren<RawImage>().enabled = true;
            ts.equippedPrimarySlot.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
        } else {
            ts.equippedSecondarySlot.GetComponentInChildren<RawImage>().enabled = true;
            ts.equippedSecondarySlot.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
        }
    }

}
