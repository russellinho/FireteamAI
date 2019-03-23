using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupScript : MonoBehaviour
{

    public GameObject equipmentStatDescriptor;
    public GameObject weaponStatDescriptor;

    // Equipment stat labels
    public Text armorStatTxt;
    public Text speedStatTxt;
    public Text staminaStatTxt;

    // Weapon stat labels
    public Text damageStatTxt;
    public Text accuracyStatTxt;
    public Text recoilStatTxt;
    public Text fireRateTxt;
    public Text mobilityTxt;
    public Text rangeTxt;
    public Text clipCapacityTxt;

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

    public void SetEquipmentStats(float armor, float speed, float stamina) {
        armorStatTxt.text = ConvertToPercent(armor) + "%";
        speedStatTxt.text = ConvertToPercent(speed) + "%";
        staminaStatTxt.text = ConvertToPercent(stamina) + "%";
    }

    public void SetWeaponStats(float damage, float accuracy, float recoil, float fireRate, float mobility, float range, float clipCapacity) {
        damageStatTxt.text = "" + (int)damage;
        accuracyStatTxt.text = "" + (int)accuracy;
        recoilStatTxt.text = "" + (int)recoil;
        fireRateTxt.text = "" + (int)fireRate;
        mobilityTxt.text = "" + (int)mobility;
        rangeTxt.text = "" + (int)range;
        clipCapacityTxt.text = "" + (int)clipCapacity;
    }

    private int ConvertToPercent(float f) {
        return (int)(f * 100f);
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
    }

}
