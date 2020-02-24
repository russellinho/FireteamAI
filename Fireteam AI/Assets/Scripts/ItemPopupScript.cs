using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupScript : MonoBehaviour
{

    public GameObject equipmentStatDescriptor;
    public GameObject weaponStatDescriptor;
    public GameObject modStatDescriptor;

    // Equipment stat labels
    public Text armorStatTxt;
    public Text speedStatTxt;
    public Text staminaStatTxt;
    public Text genderRestTxt;
    public Text characterRestTxt;

    // Weapon stat labels
    public Text damageStatTxt;
    public Text accuracyStatTxt;
    public Text recoilStatTxt;
    public Text fireRateTxt;
    public Text mobilityTxt;
    public Text rangeTxt;
    public Text clipCapacityTxt;

    // Mod stat labels
    public Text modDamageStatTxt;
    public Text modAccuracyStatTxt;
    public Text modRecoilStatTxt;
    public Text modRangeStatTxt;
    public Text modClipCapacityStatTxt;
    public Text modMaxAmmoStatTxt;
    public Text equippedOnTxt;
    public Text expirationDateTxt;

    public Text title;
    public RawImage thumbnail;
    public Text description;

    // Update is called once per frame
    void Update()
    {
        // Follow mouse position
        // if (gameObject.activeInHierarchy) {
        //     UpdatePosition();
        // }
    }

    void UpdatePosition() {
        transform.position = Input.mousePosition;
    }

    public void SetTitle(string s) {
        title.text = s;
    }

    public void SetThumbnail(RawImage r) {
        thumbnail.texture = r.texture;
    }

    public void SetDescription(string s) {
        description.text = s;
    }

    public void ToggleEquipmentStatDescriptor(bool b) {
        equipmentStatDescriptor.SetActive(b);
    }

    public void ToggleWeaponStatDescriptor(bool b) {
        weaponStatDescriptor.SetActive(b);
    }

    public void ToggleModStatDescriptor(bool b) {
        modStatDescriptor.SetActive(b);
    }

    public void SetEquipmentStats(float armor, float speed, float stamina, char gender, string[] characterRestrictions) {
        armorStatTxt.text = ConvertToPercent(armor) + "%";
        speedStatTxt.text = ConvertToPercent(speed) + "%";
        staminaStatTxt.text = ConvertToPercent(stamina) + "%";
        genderRestTxt.text = ""+gender;
        characterRestTxt.text = ""+characterRestrictions;
    }

    public void SetArmorStats(float armor, float speed, float stamina) {
        armorStatTxt.text = ConvertToPercent(armor) + "%";
        speedStatTxt.text = ConvertToPercent(speed) + "%";
        staminaStatTxt.text = ConvertToPercent(stamina) + "%";
    }

    public void SetWeaponStats(float damage, float accuracy, float recoil, float fireRate, float mobility, float range, float clipCapacity) {
        damageStatTxt.text = damage == -1f ? "-" : "" + (int)damage;
        accuracyStatTxt.text = accuracy == -1f ? "-" : "" + (int)accuracy;
        recoilStatTxt.text = recoil == -1f ? "-" : "" + (int)recoil;
        fireRateTxt.text = fireRate == -1f ? "-" : "" + (int)fireRate;
        mobilityTxt.text = mobility == -1f ? "-" : "" + (int)mobility;
        rangeTxt.text = range == -1f ? "-" : "" + (int)range;
        clipCapacityTxt.text = clipCapacity == -1f ? "-" : "" + (int)clipCapacity;
    }

    public void SetModStats(float damage, float accuracy, float recoil, float range, int clipCapacity, int maxAmmo, string equippedOn) {
        modDamageStatTxt.text = damage == -1f ? "-" : "" + (int)damage;
        modAccuracyStatTxt.text = accuracy == -1f ? "-" : "" + (int)accuracy;
        modRecoilStatTxt.text = recoil == -1f ? "-" : "" + (int)recoil;
        modRangeStatTxt.text = range == -1f ? "-" : "" + (int)range;
        modClipCapacityStatTxt.text = clipCapacity == -1f ? "-" : "" + clipCapacity;
        modMaxAmmoStatTxt.text = maxAmmo == -1f ? "-" : "" + maxAmmo;
        equippedOnTxt.text = ("".Equals(equippedOn) ? "-" : equippedOn);
    }

    private int ConvertToPercent(float f) {
        return Mathf.RoundToInt(f * 100.0f);
    }

    public void ClearAllStats() {
        armorStatTxt.text = "-";
        speedStatTxt.text = "-";
        staminaStatTxt.text = "-";
        damageStatTxt.text = "-";
        accuracyStatTxt.text = "-";
        recoilStatTxt.text = "-";
        fireRateTxt.text = "-";
        mobilityTxt.text = "-";
        rangeTxt.text = "-";
        clipCapacityTxt.text = "-";
        genderRestTxt.text = "";
        characterRestTxt.text = "";
    }

    public void SetExpirationDate(string expirationDate) {
        expirationDateTxt.text = expirationDate;
    }

}
