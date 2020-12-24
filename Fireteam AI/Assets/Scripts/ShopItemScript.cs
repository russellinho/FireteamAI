﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemScript : MonoBehaviour
{
    public GameObject itemDescriptionPopupRef;
    public TitleControllerScript ts;
    public RawImage thumbnailRef;
    public Image outline;
    public Character characterDetails;
    public Equipment equipmentDetails;
    public Armor armorDetails;
    public Weapon weaponDetails;
    public Mod modDetails;
    public string id;
    public string equippedOn;
    public string acquireDate;
    public string expirationDate;
    public string duration;
    public string itemName;
    public string itemType;
    public string itemDescription;
    public string weaponCategory;
    public string modCategory;
    // 0 = long sleeves, 1 = mid sleeves, 2 = short sleeves
    private int clickCount;
    private float clickTimer;
    public TextMeshProUGUI gpPriceTxt;
    public Button previewBtn;
    public Button purchaseBtn;
    public Button equipBtn;
    public Button modWeaponBtn;
    public Button modEquipBtn;
    public char titleType;

    void Start() {
        clickCount = 0;
        clickTimer = 0f;
        ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
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
            if (titleType == 'm') {
                if (ItemCanBePreviewed()) {
                    PreviewItem();
                }
            } else if (titleType == 'l') {
                EquipItem();
            } else if (titleType == 's') {
                if (modDetails == null) {
                    LoadWeaponForModding();
                } else {
                    EquipMod();
                }
            }
            clickTimer = 0f;
            clickCount = 0;
        }
    }

    bool ItemCanBePreviewed() {
        if (itemType == "Mod" || (itemType == "Weapon" && weaponDetails.type != "Primary")) {
            return false;
        }
        return true;
    }

    public void LoadWeaponForModding() {
        if (ts.weaponPreviewShopSlot.itemName == this.itemName) {
            return;
        }
        ts.LoadWeaponForModding(this);
    }

    public void OnPreviewBtnClicked() {
        PreviewItem();
    }

    public void OnPurchaseBtnClicked() {
        ts.PreparePurchase(itemName, itemType, thumbnailRef.texture);
    }

    private void PreviewItem() {
        switch (itemType)
        {
            case "Character":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewCharacter(itemName);
                break;
            case "Top":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewTop(itemName);
                break;
            case "Bottom":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewBottom(itemName);
                break;
            case "Footwear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewFootwear(itemName);
                break;
            case "Headgear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewHeadgear(itemName);
                break;
            case "Facewear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewFacewear(itemName);
                break;
            case "Armor":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().PreviewArmor(itemName);
                break;
            case "Weapon":
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().PreviewWeapon(itemName);
                break;
        }
    }

    public void EquipItem()
    {
        switch (itemType)
        {
            case "Character":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipCharacter(itemName, gameObject);
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
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipWeapon(itemName, null, null, gameObject);
                break;
        }
    }

    public void EquipMod() {
        switch (modCategory)
        {
            case "Suppressor":
                // Check if weapon is suppressor compatible before continuing
                if (!ts.WeaponIsSuppressorCompatible(ts.weaponPreviewShopSlot.itemName)) {
                    ts.TriggerAlertPopup("Suppressors cannot be equipped on this weapon!");
                    return;
                }

                // Check if the weapon already has this mod equipped
                if (PlayerData.playerdata.LoadModDataForWeapon(ts.weaponPreviewShopSlot.itemName).SuppressorId == this.id) {
                    return;
                }

                PlayerData.playerdata.SaveModDataForWeapon(ts.weaponPreviewShopSlot.itemName, id, null);
                break;
            case "Sight":
                // Check if weapon is sight compatible before continuing
                if (!ts.WeaponIsSightCompatible(ts.weaponPreviewShopSlot.itemName)) {
                    ts.TriggerAlertPopup("Sights cannot be equipped on this weapon!");
                    return;
                }

                // Check if the weapon already has this mod equipped
                if (PlayerData.playerdata.LoadModDataForWeapon(ts.weaponPreviewShopSlot.itemName).SightId == this.id) {
                    return;
                }

                PlayerData.playerdata.SaveModDataForWeapon(ts.weaponPreviewShopSlot.itemName, null, id);
                break;
        }
    }

    public void CalculateExpirationDate() {
        if (duration.Equals("-1")) {
            expirationDate = "Permanent";
        } else {
            // Calculate expiration date - add duration to acquire date and convert to DateTime
            DateTime acquireDateDate = DateTime.Parse(acquireDate);
            float dur = float.Parse(duration);
            acquireDateDate = acquireDateDate.AddMinutes((double)dur);
            // Set the calculated expiration date
            expirationDate = acquireDateDate.ToString();
        }
    }

    public void ToggleEquippedIndicator(bool b) {
        if (b) {
            outline.color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
            equipBtn?.gameObject.SetActive(false);
        } else {
            outline.color = new Color(99f / 255f, 198f / 255f, 255f / 255f, 255f / 255f);
            equipBtn?.gameObject.SetActive(true);
        }
    }

    public void ToggleWeaponPreviewIndicator(bool b) {
        if (b) {
            outline.color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
            modWeaponBtn?.gameObject.SetActive(false);
        } else {
            outline.color = new Color(99f / 255f, 198f / 255f, 255f / 255f, 255f / 255f);
            modWeaponBtn?.gameObject.SetActive(true);
        }
    }

    public void ToggleModEquippedIndicator(bool b) {
        if (b) {
            outline.color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
            modEquipBtn?.gameObject.SetActive(false);
        } else {
            outline.color = new Color(99f / 255f, 198f / 255f, 255f / 255f, 255f / 255f);
            modEquipBtn?.gameObject.SetActive(true);
        }
    }

    public void SetItemForMarket() {
        gpPriceTxt.gameObject.SetActive(true);
        previewBtn.gameObject.SetActive(ItemCanBePreviewed());
        purchaseBtn.gameObject.SetActive(true);
        equipBtn.gameObject.SetActive(false);
        modWeaponBtn.gameObject.SetActive(false);
        modEquipBtn.gameObject.SetActive(false);
        titleType = 'm';
    }

    public void SetItemForLoadout() {
        gpPriceTxt.gameObject.SetActive(false);
        previewBtn.gameObject.SetActive(false);
        purchaseBtn.gameObject.SetActive(false);
        equipBtn.gameObject.SetActive(true);
        modWeaponBtn.gameObject.SetActive(false);
        modEquipBtn.gameObject.SetActive(false);
        titleType = 'l';
    }

    public void SetItemForModShop() {
        if (modDetails == null) {
            modWeaponBtn.gameObject.SetActive(true);
            modEquipBtn.gameObject.SetActive(false);
        } else {
            modWeaponBtn.gameObject.SetActive(false);
            modEquipBtn.gameObject.SetActive(true);
        }
        gpPriceTxt.gameObject.SetActive(false);
        previewBtn.gameObject.SetActive(false);
        purchaseBtn.gameObject.SetActive(false);
        equipBtn.gameObject.SetActive(false);
        titleType = 's';
    }

    public void ClearEquippedOn() {
        equippedOn = "";
    }

}
