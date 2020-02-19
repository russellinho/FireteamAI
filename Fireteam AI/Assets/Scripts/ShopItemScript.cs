﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionPopupRef;
    public GameObject modDescriptionPopupRef;
    public RawImage thumbnailRef;
    public Character characterDetails;
    public Equipment equipmentDetails;
    public Armor armorDetails;
    public Weapon weaponDetails;
    public Mod modDetails;
    public string id;
    public string equippedOn;
    public string itemName;
    public string itemType;
    public string itemDescription;
    public string weaponCategory;
    public string modCategory;
    // 0 = long sleeves, 1 = mid sleeves, 2 = short sleeves
    public Text equippedInd;
    private int clickCount;
    private float clickTimer;

    void Start() {
        clickCount = 0;
        clickTimer = 0f;
    }

    void FixedUpdate() {
        if (clickCount == 1) {
            clickTimer += Time.deltaTime;
            if (clickTimer > 0.5f) {
                clickTimer = 0f;
                clickCount = 0;
            }
        }
    }

    // Makes sure that the user double clicks on the item to equip it
    public void OnItemClick() {
        clickCount++;
        if (clickCount == 2) {
            if (modDetails == null) {
                EquipItem();
            } else {
                EquipMod();
            }
            clickTimer = 0f;
            clickCount = 0;
        }
    }

    private void EquipItem()
    {
        switch (itemType)
        {
            case "Character":
                PlayerData.playerdata.ChangeBodyRef(itemName, gameObject);
                break;
            case "Top":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipTop(itemName, gameObject);
                break;
            case "Bottom":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipBottom(itemName, gameObject);
                break;
            case "Footwear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipFootwear(itemName, gameObject);
                break;
            case "Headgear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipHeadgear(itemName, gameObject);
                break;
            case "Facewear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipFacewear(itemName, gameObject);
                break;
            case "Armor":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipArmor(itemName, gameObject);
                break;
            case "Weapon":
                ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemName);
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipWeapon(itemName, modInfo.equippedSuppressor, gameObject);
                SetModInfo(modInfo);
                break;
        }
    }

    private void EquipMod() {
        switch (modCategory)
        {
            case "Suppressor":
                // If this weapon already has a suppressor on it, unequip it first

                // If this mod is equipped to another weapon, unequip it from that weapon as well

                // Attach to player weapon and attach to weapon mod template as well
                TitleControllerScript ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
                string weaponNameAttachedTo = ts.EquipModOnWeaponTemplate(itemName, modCategory);
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipMod(modCategory, itemName, weaponNameAttachedTo, gameObject);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (itemType.Equals("Mod")) {
            if (modDescriptionPopupRef.activeInHierarchy) {
                return;
            }
            modDescriptionPopupRef.SetActive(true);
            ItemPopupScript ips = modDescriptionPopupRef.GetComponent<ItemPopupScript>();
            ips.SetTitle(itemName);
            ips.SetThumbnail(thumbnailRef);
            ips.SetDescription(itemDescription);
            ips.SetModStats(modDetails.damageBoost, modDetails.accuracyBoost, modDetails.recoilBoost, modDetails.rangeBoost, modDetails.clipCapacityBoost, modDetails.maxAmmoBoost, equippedOn);
            ips.ToggleModStatDescriptor(true);
        } else {
            if (itemDescriptionPopupRef.activeInHierarchy) {
                return;
            }
            itemDescriptionPopupRef.SetActive(true);
            ItemPopupScript ips = itemDescriptionPopupRef.GetComponent<ItemPopupScript>();
            ips.SetTitle(itemName);
            ips.SetThumbnail(thumbnailRef);
            ips.SetDescription(itemDescription);
            if (itemType.Equals("Headgear") || itemType.Equals("Facewear")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetEquipmentStats(equipmentDetails.armor, equipmentDetails.speed, equipmentDetails.stamina, equipmentDetails.gender, equipmentDetails.characterRestrictions);
                ips.ToggleEquipmentStatDescriptor(true);
            } else if (itemType.Equals("Armor")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetArmorStats(armorDetails.armor, armorDetails.speed, armorDetails.stamina);
                ips.ToggleEquipmentStatDescriptor(true);
            } else if (itemType.Equals("Weapon")) {
                ips.ToggleEquipmentStatDescriptor(false);
                ips.SetWeaponStats(weaponDetails.damage, weaponDetails.accuracy, weaponDetails.recoil, weaponDetails.fireRate, weaponDetails.mobility, weaponDetails.range, weaponDetails.clipCapacity);
                ips.ToggleWeaponStatDescriptor(true);
            } else {
                ips.ToggleEquipmentStatDescriptor(false);
                ips.ToggleWeaponStatDescriptor(false);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (itemType.Equals("Mod")) {
            modDescriptionPopupRef.SetActive(false);
        } else {
            itemDescriptionPopupRef.SetActive(false);
        }
    }

    void SetModInfo(ModInfo modInfo) {
        if (weaponDetails.type.Equals("Primary")) {
            PlayerData.playerdata.primaryModInfo = modInfo;
        }
        if (weaponDetails.type.Equals("Secondary")) {
            PlayerData.playerdata.secondaryModInfo = modInfo;
        }
        if (weaponDetails.type.Equals("Support")) {
            PlayerData.playerdata.supportModInfo = modInfo;
        }
    }

}
