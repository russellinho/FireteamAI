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
    public GameObject LoadWeapon(string weaponPath) {
        if (weapon != null) {
            Destroy(weapon.gameObject);
            weapon = null;
        }
        GameObject o = Instantiate((GameObject)Resources.Load(weaponPath));
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

    public void SetWeaponPositionForTitle(Vector3 pos) {
        //weapon.localPosition = new Vector3(weapon.localPosition.x + (-handle.localPosition.x) + offset.x, weapon.localPosition.y + (-handle.localPosition.y) + offset.y, weapon.localPosition.z + (-handle.localPosition.z) + offset.z);
        weapon.localPosition = new Vector3(pos.x, pos.y, pos.z);
        weapon.localRotation = Quaternion.Euler(13.891f, 177.759f, -92.145f);
    }

    public void SwitchWeaponToLeftHand() {
        WeaponStats wepStats = weapon.GetComponent<WeaponStats>();
        weapon.SetParent(leftHand);
        // if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'M') {
        //     weapon.localPosition = wepStats.fpcLeftHandPosMale;
        //     weapon.localRotation = Quaternion.Euler(wepStats.fpcLeftHandRotMale);            
        // } else if (fpc.equipmentScript.GetGenderByCharacter(PlayerData.playerdata.info.equippedCharacter) == 'F') {
        //     weapon.localPosition = wepStats.fpcLeftHandPosFemale;
        //     weapon.localRotation = Quaternion.Euler(wepStats.fpcLeftHandRotFemale);
        // }
    }

    public void SwitchWeaponToRightHand() {
        weapon.SetParent(gameObject.transform);
        SetWeaponPosition(true);
    }

}
