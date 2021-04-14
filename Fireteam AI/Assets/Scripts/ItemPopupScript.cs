using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupScript : MonoBehaviour
{
    public GameObject clothingStatDescriptor;
    public GameObject equipmentStatDescriptor;
    public GameObject weaponStatDescriptor;
    public GameObject modStatDescriptor;
    public GameObject skillDescriptor;

    // Clothing stat labels
    public Text genderRestTxtClothing;
    public Text characterRestTxtClothing;
    public Text expirationDateTxtClothing;
    public GameObject expirationDateClothing;

    // Equipment stat labels
    public Text armorStatTxt;
    public Text speedStatTxt;
    public Text staminaStatTxt;
    public Text avoidabilityStatTxt;
    public Text detectionEquipmentTxt;
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
    public Text detectionWeaponTxt;
    public Text expirationDateWeaponTxt;
    public GameObject expirationDateWeapon;

    // Mod stat labels
    public Text modDamageStatTxt;
    public Text modAccuracyStatTxt;
    public Text modRecoilStatTxt;
    public Text modRangeStatTxt;
    public Text modClipCapacityStatTxt;
    public Text modMaxAmmoStatTxt;
    public Text detectionModTxt;
    public Text equippedOnTxt;

    // Skill stat labels
    public Text currentLevelTxt;
    public Text maxLevelTxt;
    public Text prerequisitesTxt;

    public Text title;
    public RawImage thumbnail;
    public Text description;
    public Canvas parentCanvas;
    public RectTransform rectCanvas;
    public RectTransform rectTransform;

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

        transform.position = parentCanvas.transform.TransformPoint(movePos);
        KeepPopupInScreenBounds();
    }

    void KeepPopupInScreenBounds()
    {
        var sizeDelta = rectCanvas.sizeDelta - rectTransform.sizeDelta;
        var panelPivot = rectTransform.pivot;
        var position = rectTransform.anchoredPosition;
        position.x = Mathf.Clamp(position.x, 0f, sizeDelta.x * (1.035f));
        position.y = Mathf.Clamp(position.y, -sizeDelta.y * (1.035f), 0f);
        rectTransform.anchoredPosition = position;
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

    public void ToggleClothingStatDescriptor(bool b) {
        clothingStatDescriptor.SetActive(b);
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

    public void ToggleSkillStatDescriptor(bool b)
    {
        skillDescriptor.SetActive(b);
    }

    public void SetEquipmentStats(float armor, float speed, float stamina, float avoidability, int detection, char gender, string[] characterRestrictions) {
        armorStatTxt.text = ConvertToPercent(armor) + "%";
        speedStatTxt.text = ConvertToPercent(speed) + "%";
        staminaStatTxt.text = ConvertToPercent(stamina) + "%";
        avoidabilityStatTxt.text = ConvertToPercent(avoidability) + "%";
        detectionEquipmentTxt.text = "" + detection;
    }

    public void SetRestrictions(char gender, string[] characterRestrictions) {
        if (gender == 'M') {
            genderRestTxt.text = "Male";
            genderRestTxtClothing.text = "Male";
        } else if (gender == 'F') {
            genderRestTxt.text = "Female";
            genderRestTxtClothing.text = "Female";
        } else {
            genderRestTxt.text = "None";
            genderRestTxtClothing.text = "None";
        }
        if (characterRestrictions.Length == 0) {
            characterRestTxt.text = "None";
            characterRestTxtClothing.text = "None";
        } else {
            string rests = string.Join(", ", characterRestrictions);
            characterRestTxt.text = rests;
            characterRestTxtClothing.text = rests;
        }
    }

    public void SetArmorStats(float armor, float speed, float stamina, float avoidability, int detection) {
        armorStatTxt.text = ConvertToPercent(armor) + "%";
        speedStatTxt.text = ConvertToPercent(speed) + "%";
        staminaStatTxt.text = ConvertToPercent(stamina) + "%";
        avoidabilityStatTxt.text = ConvertToPercent(avoidability) + "%";
        detectionEquipmentTxt.text = "" + detection;
    }

    public void SetWeaponStats(float damage, float accuracy, float recoil, float fireRate, float mobility, float range, float clipCapacity, int detection) {
        damageStatTxt.text = damage == -1f ? "-" : "" + (int)damage;
        accuracyStatTxt.text = accuracy == -1f ? "-" : "" + (int)accuracy;
        recoilStatTxt.text = recoil == -1f ? "-" : "" + (int)recoil;
        fireRateTxt.text = fireRate == -1f ? "-" : "" + (int)fireRate;
        mobilityTxt.text = mobility == -1f ? "-" : "" + (int)mobility;
        rangeTxt.text = range == -1f ? "-" : "" + (int)range;
        clipCapacityTxt.text = clipCapacity == -1f ? "-" : "" + (int)clipCapacity;
        detectionWeaponTxt.text = "" + detection;
    }

    public void SetModStats(float damage, float accuracy, float recoil, float range, int clipCapacity, int maxAmmo, int detection, string equippedOn) {
        modDamageStatTxt.text = damage == -1f ? "-" : "" + (int)damage;
        modAccuracyStatTxt.text = accuracy == -1f ? "-" : "" + (int)accuracy;
        modRecoilStatTxt.text = recoil == -1f ? "-" : "" + (int)recoil;
        modRangeStatTxt.text = range == -1f ? "-" : "" + (int)range;
        modClipCapacityStatTxt.text = clipCapacity == -1f ? "-" : "" + clipCapacity;
        modMaxAmmoStatTxt.text = maxAmmo == -1f ? "-" : "" + maxAmmo;
        equippedOnTxt.text = ("".Equals(equippedOn) ? "-" : equippedOn);
        detectionModTxt.text = "" + detection;
    }

    public void SetSkillStats(int currLevel, int maxLevel, string prerequisites)
    {
        currentLevelTxt.text = ""+currLevel;
        maxLevelTxt.text = ""+maxLevel;
        prerequisitesTxt.text = prerequisites;
    }

    private int ConvertToPercent(float f) {
        return Mathf.RoundToInt(f * 100.0f);
    }

    public void ClearAllStats() {
        armorStatTxt.text = "-";
        speedStatTxt.text = "-";
        staminaStatTxt.text = "-";
        avoidabilityStatTxt.text = "-";
        damageStatTxt.text = "-";
        accuracyStatTxt.text = "-";
        recoilStatTxt.text = "-";
        fireRateTxt.text = "-";
        mobilityTxt.text = "-";
        rangeTxt.text = "-";
        detectionEquipmentTxt.text = "-";
        detectionWeaponTxt.text = "-";
        detectionModTxt.text = "-";
        clipCapacityTxt.text = "-";
        genderRestTxt.text = "";
        characterRestTxt.text = "";
        genderRestTxtClothing.text = "";
        characterRestTxtClothing.text = "";
        expirationDateTxtClothing.text = "";
    }

    public void SetExpirationDate(string expirationDate) {
        expirationDateWeaponTxt.text = expirationDate;
        expirationDateEquipTxt.text = expirationDate;
        expirationDateTxtClothing.text = expirationDate;
        if (expirationDate != "Permanent") {
            expirationDateWeaponTxt.text += " EST";
            expirationDateEquipTxt.text += " EST";
            expirationDateTxtClothing.text += " EST";
        }
    }

    public void ToggleExpirationDateText(bool b) {
        expirationDateEquip.SetActive(b);
        expirationDateWeapon.SetActive(b);
        expirationDateClothing.SetActive(b);
    }

}
