using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MeleeHandlerScript : MonoBehaviour
{
    public Transform weapon;
    public FirstPersonController fpc;

    // Update is called once per frame
    public GameObject LoadWeapon(int weaponPath) {
        if (weapon != null) {
            Destroy(weapon.gameObject);
            weapon = null;
        }
        GameObject o = Instantiate(InventoryScript.itemData.itemReferences[weaponPath]);
        weapon = o.transform;
        weapon.SetParent(gameObject.transform);
        return o;
    }

    public void SetWeapon(Transform wep, bool firstPerson) {
        weapon = wep;
        wep.SetParent(gameObject.transform);
        if (firstPerson) {
            SetWeaponPosition(true);
        } else {
            SetWeaponPosition(false);
        }
    }

    public void SetWeaponPosition(bool firstPersonMode)
    {
        WeaponStats wepStats = weapon.GetComponent<WeaponStats>();
        if (firstPersonMode) {
            if (fpc.equipmentScript.GetGender() == 'M') {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepStats.fpcPosMale;
                weapon.localRotation = Quaternion.Euler(wepStats.fpcRotMale);
                weapon.localScale = wepStats.fpcScaleMale;
            } else {
                // Set the weapon position for females, get from stats
                weapon.localPosition = wepStats.fpcPosFemale;
                weapon.localRotation = Quaternion.Euler(wepStats.fpcRotFemale);
                weapon.localScale = wepStats.fpcScaleFemale;
            }
        } else {
            if (fpc.equipmentScript.GetGender() == 'M') {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepStats.fullPosMale;
                weapon.localRotation = Quaternion.Euler(wepStats.fullRotMale);
                weapon.localScale = wepStats.fullScaleMale;
            } else {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepStats.fullPosFemale;
                weapon.localRotation = Quaternion.Euler(wepStats.fullRotFemale);
                weapon.localScale = wepStats.fullScaleFemale;
            }
        }
    }

}
