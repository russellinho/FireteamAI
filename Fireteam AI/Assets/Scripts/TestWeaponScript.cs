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
    public bool weaponReady;

    // Start is called before the first frame update
    void Start()
    {
        weaponReady = false;
        // Populate hand positions
        rifleHandPositions = new Dictionary<string, Vector3>();
        rifleHandPositions.Add("AK-47", new Vector3(-0.098f, 0.135f, 0.075f));

        EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            if (currentlyEquippedType == 1) return;
            weaponReady = false;
            currentlyEquippedType = 1;
            animator.SetInteger("WeaponType", 1);
            EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (currentlyEquippedType == 2) return;
            weaponReady = false;
            animator.SetInteger("WeaponType", 2);
            currentlyEquippedType = 2;
            EquipWeapon(equippedSecondaryType, equippedSecondaryWeapon);
            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
        }
    }

    void EquipAssaultRifle(string weaponName) {
        // animator.CrossFadeInFixedTime("IdleAssaultRifle", 0.15f);
        if (weaponName.Equals("AK-47")) {
            // Set animation and hand positions
            weaponHolder.SetWeaponPosition();
            weaponHolder.SetSteadyHand(rifleHandPositions["AK-47"]);
        }
    }

    public void EquipPistol(string weaponName) {
        // animator.CrossFadeInFixedTime("IdlePistol", 0.1f);
        if (weaponName.Equals("Glock23")) {
            // Set animation and hand positions
            weaponHolder.SetWeaponPosition();
            weaponHolder.ResetSteadyHand();
        }
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
        }
    }

}
