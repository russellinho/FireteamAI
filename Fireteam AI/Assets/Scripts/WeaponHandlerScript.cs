using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class WeaponHandlerScript : MonoBehaviour
{
    public Transform weapon;
    public Transform leftShoulder;
    public Transform leftHand;
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
        WeaponMeta wepMetaData = weapon.GetComponent<WeaponMeta>();
        if (firstPersonMode) {
            if (fpc.equipmentScript.GetGender() == 'M') {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepMetaData.fpcPosMale;
                weapon.localRotation = Quaternion.Euler(wepMetaData.fpcRotMale);
                weapon.localScale = wepMetaData.fpcScaleMale;
            } else {
                // Set the weapon position for females, get from stats
                weapon.localPosition = wepMetaData.fpcPosFemale;
                weapon.localRotation = Quaternion.Euler(wepMetaData.fpcRotFemale);
                weapon.localScale = wepMetaData.fpcScaleFemale;
            }
        } else {
            if (fpc.equipmentScript.GetGender() == 'M') {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepMetaData.fullPosMale;
                weapon.localRotation = Quaternion.Euler(wepMetaData.fullRotMale);
                weapon.localScale = wepMetaData.fullScaleMale;
            } else {
                // Set the weapon position for males, get from stats
                weapon.localPosition = wepMetaData.fullPosFemale;
                weapon.localRotation = Quaternion.Euler(wepMetaData.fullRotFemale);
                weapon.localScale = wepMetaData.fullScaleFemale;
            }
        }
    }

    public void SetWeaponPositionForTitle(Vector3 pos, Vector3 rot) {
        weapon.localPosition = new Vector3(pos.x, pos.y, pos.z);
        weapon.localRotation = Quaternion.Euler(rot);
    }

    public void SwitchWeaponToLeftHand() {
        if (weapon != null) {
            // WeaponMeta wepMetaData = weapon.GetComponent<WeaponMeta>();
            weapon.SetParent(leftHand);
            // if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
            //     weapon.localPosition = wepStats.fpcLeftHandPosMale;
            //     weapon.localRotation = Quaternion.Euler(wepStats.fpcLeftHandRotMale);            
            // } else if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'F') {
            //     weapon.localPosition = wepStats.fpcLeftHandPosFemale;
            //     weapon.localRotation = Quaternion.Euler(wepStats.fpcLeftHandRotFemale);
            // }
        }
    }

    public void SwitchWeaponToRightHand() {
        if (weapon != null) {
            weapon.SetParent(gameObject.transform);
            SetWeaponPosition(true);
        }
    }

}
