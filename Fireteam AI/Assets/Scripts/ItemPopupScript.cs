﻿using System.Collections;
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
    public Text expirationDateEquipTxt;
    public GameObject expirationDateEquip;

    // Weapon stat labels
    public Text damageStatTxt;
    public Text accuracyStatTxt;
    public Text recoilStatTxt;
    public Text fireRateTxt;
    public Text mobilityTxt;
    public Text rangeTxt;
    public Text clipCapacityTxt;
    public Text expirationDateWeaponTxt;
    public GameObject expirationDateWeapon;

    // Mod stat labels
    public Text modDamageStatTxt;
    public Text modAccuracyStatTxt;
    public Text modRecoilStatTxt;
    public Text modRangeStatTxt;
    public Text modClipCapacityStatTxt;
    public Text modMaxAmmoStatTxt;
    public Text equippedOnTxt;

    public Text title;
    public RawImage thumbnail;
    public Text description;
    public Canvas parentCanvas;
    public RectTransform rectTransform;
    private int maxScreenRight;
    private int maxScreenLeft;
    private int maxScreenTop;
    private int maxScreenBottom;

    void Start() {
        maxScreenBottom = 0;
        maxScreenLeft = 0;
        Vector2 maxes;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            new Vector3(Screen.width, Screen.height, 0), parentCanvas.worldCamera,
            out maxes);
        maxScreenTop = (int)maxes.x - (int)rectTransform.rect.height;
        maxScreenRight = (int)maxes.y - (int)rectTransform.rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        // Follow mouse position
        if (gameObject.activeInHierarchy) {
            UpdatePosition();
        }
    }

    void OnEnable() {
        UpdatePosition();
    }

    void UpdatePosition() {
        Vector2 movePos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition, parentCanvas.worldCamera,
            out movePos);

        // movePos = KeepPopupInScreenBounds(movePos);
        transform.position = parentCanvas.transform.TransformPoint(movePos);
    }

    Vector2 KeepPopupInScreenBounds(Vector2 movePos) {
        float newX = movePos.x;
        float newY = movePos.y;

        if (newX < maxScreenLeft) {
            newX = maxScreenLeft;
        }
        if (newX > maxScreenRight) {
            newX = maxScreenRight;
        }
        if (newY < maxScreenBottom) {
            newY = maxScreenBottom;
        }
        if (newY > maxScreenTop) {
            newY = maxScreenTop;
        }
        
        return new Vector2(newX, newY);
    }

    public void SetTitle(string s) {
        title.text = s;
    }

    public void SetThumbnail(RawImage r) {
        thumbnail.texture = r.texture;
    }

    public void SetThumbnail(Texture t) {
        thumbnail.texture = t;
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
    }

    public void SetRestrictions(char gender, string[] characterRestrictions) {
        if (gender == 'M') {
            genderRestTxt.text = "Male";
        } else if (gender == 'F') {
            genderRestTxt.text = "Female";
        } else {
            genderRestTxt.text = "None";
        }
        characterRestTxt.text = characterRestrictions.Length == 0 ? "None" : string.Join(", ", characterRestrictions);
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
        expirationDateWeaponTxt.text = expirationDate;
        expirationDateEquipTxt.text = expirationDate;
        if (expirationDate != "Permanent") {
            expirationDateWeaponTxt.text += " EST";
            expirationDateEquipTxt.text += " EST";
        }
    }

    public void ToggleExpirationDateText(bool b) {
        expirationDateEquip.SetActive(b);
        expirationDateWeapon.SetActive(b);
    }

}
