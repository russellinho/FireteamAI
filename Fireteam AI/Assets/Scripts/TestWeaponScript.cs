using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWeaponScript : MonoBehaviour
{
    public EquipmentScript equipmentScript;
    public WeaponHandlerScript weaponHolder;
    public Animator animator;
    public string equippedPrimaryWeapon;
    public string equippedPrimaryType;
    public string equippedSecondaryWeapon;
    public string equippedSecondaryType;
    public int currentlyEquippedType;
    private Dictionary<string, Vector3> rifleHandPositions;
    private Dictionary<string, Vector3> shotgunHandPositions;
    private Dictionary<string, Vector3> sniperRifleHandPositions;
    public bool weaponReady;

    // Start is called before the first frame update
    void Start()
    {
        weaponReady = false;
        // Populate hand positions
        rifleHandPositions = new Dictionary<string, Vector3>();
        rifleHandPositions.Add("AK-47", new Vector3(-0.098f, 0.135f, 0.075f));
        rifleHandPositions.Add("M4A1", new Vector3(-0.065f, 0.126f, 0.04f));
        
        shotgunHandPositions = new Dictionary<string, Vector3>();
        shotgunHandPositions.Add("R870", new Vector3(-0.129f, 0.145f, 0.084f));

        sniperRifleHandPositions = new Dictionary<string, Vector3>();
        sniperRifleHandPositions.Add("L96A1", new Vector3(-0.054f, 0.115f, 0.029f));

        DrawWeapon(1);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            DrawWeapon(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            DrawWeapon(2);
        }
    }

    void DrawWeapon(int weaponCat) {
        if (weaponCat == 1) {
            if (currentlyEquippedType == 1) return;
            weaponReady = false;
            currentlyEquippedType = 1;
            animator.SetInteger("WeaponType", 1);
            EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        } else if (weaponCat == 2) {
            if (currentlyEquippedType == 2) return;
            weaponReady = false;
            animator.SetInteger("WeaponType", 2);
            currentlyEquippedType = 2;
            EquipWeapon(equippedSecondaryType, equippedSecondaryWeapon);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        }
    }

    void EquipAssaultRifle(string weaponName) {
        // Set animation and hand positions
        weaponHolder.SetWeaponPosition();
        weaponHolder.SetSteadyHand(rifleHandPositions[weaponName]);
    }

    void EquipShotgun(string weaponName) {
        weaponHolder.SetWeaponPosition();
        weaponHolder.SetSteadyHand(shotgunHandPositions[weaponName]);
    }

    public void EquipPistol(string weaponName) {
        // Set animation and hand positions
        weaponHolder.SetWeaponPosition();
        weaponHolder.ResetSteadyHand();
    }

    public void EquipSniperRifle(string weaponName) {
        weaponHolder.SetWeaponPosition();
        weaponHolder.SetSteadyHand(sniperRifleHandPositions[weaponName]);
    }

    public void EquipWeapon(string weaponType, string weaponName) {
        switch (weaponType) {
            case "Assault Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(InventoryScript.weaponCatalog[weaponName]);
                EquipAssaultRifle(weaponName);
                break;
            case "Pistol":
                currentlyEquippedType = 2;
                weaponHolder.LoadWeapon(InventoryScript.weaponCatalog[weaponName]);
                EquipPistol(weaponName);
                break;
            case "Shotgun":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(InventoryScript.weaponCatalog[weaponName]);
                EquipShotgun(weaponName);
                break;
            case "Sniper Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(InventoryScript.weaponCatalog[weaponName]);
                EquipSniperRifle(weaponName);
                break;
        }
    }

}
