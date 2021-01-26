using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class PanelClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItemScript shopItemScript;
    public SetupItemScript setupItemScript;

    public void OnPointerEnter(PointerEventData eventData) {
        if (shopItemScript != null) {
            ShowShopItemData();
        } else if (setupItemScript != null) {
            ShowSetupItemData();
        }
    }
    public void ShowShopItemData() {
        ItemPopupScript ips = shopItemScript.itemDescriptionPopupRef.GetComponent<ItemPopupScript>();
        ips.SetTitle(shopItemScript.itemName);
        ips.SetThumbnail(shopItemScript.thumbnailRef);
        ips.SetDescription(shopItemScript.itemDescription);
        if (shopItemScript.titleType == 'l') {
            shopItemScript.CalculateExpirationDate();
            ips.SetExpirationDate(shopItemScript.expirationDate);
            ips.ToggleExpirationDateText(true);
        } else {
            ips.ToggleExpirationDateText(false);
        }
        if (shopItemScript.itemType.Equals("Mod")) {
            if (shopItemScript.itemDescriptionPopupRef.activeInHierarchy) {
                return;
            }
            shopItemScript.itemDescriptionPopupRef.SetActive(true);
            ips.SetModStats(shopItemScript.modDetails.damageBoost, shopItemScript.modDetails.accuracyBoost, shopItemScript.modDetails.recoilBoost, shopItemScript.modDetails.rangeBoost, shopItemScript.modDetails.clipCapacityBoost, shopItemScript.modDetails.maxAmmoBoost, shopItemScript.equippedOn);
            ips.ToggleModStatDescriptor(true);
            ips.ToggleEquipmentStatDescriptor(false);
            ips.ToggleWeaponStatDescriptor(false);
            ips.ToggleClothingStatDescriptor(false);
        } else {
            ips.ToggleModStatDescriptor(false);
            if (shopItemScript.itemDescriptionPopupRef.activeInHierarchy) {
                return;
            }
            shopItemScript.itemDescriptionPopupRef.SetActive(true);
            if (shopItemScript.itemType.Equals("Headgear") || shopItemScript.itemType.Equals("Facewear")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetEquipmentStats(shopItemScript.equipmentDetails.armor, shopItemScript.equipmentDetails.speed, shopItemScript.equipmentDetails.stamina, shopItemScript.equipmentDetails.gender, shopItemScript.equipmentDetails.characterRestrictions);
                ips.ToggleEquipmentStatDescriptor(true);
                ips.SetRestrictions(shopItemScript.equipmentDetails.gender, shopItemScript.equipmentDetails.characterRestrictions);
                ips.ToggleClothingStatDescriptor(false);
            } else if (shopItemScript.itemType.Equals("Armor")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetArmorStats(shopItemScript.armorDetails.armor, shopItemScript.armorDetails.speed, shopItemScript.armorDetails.stamina);
                ips.ToggleEquipmentStatDescriptor(true);
                ips.ToggleClothingStatDescriptor(false);
            } else if (shopItemScript.itemType.Equals("Weapon")) {
                ips.ToggleEquipmentStatDescriptor(false);
                ips.SetWeaponStats(shopItemScript.weaponDetails.damage, shopItemScript.weaponDetails.accuracy, shopItemScript.weaponDetails.recoil, shopItemScript.weaponDetails.fireRate, shopItemScript.weaponDetails.mobility, shopItemScript.weaponDetails.range, shopItemScript.weaponDetails.clipCapacity);
                ips.ToggleWeaponStatDescriptor(true);
                ips.ToggleClothingStatDescriptor(false);
            } else {
                // For clothing and shoes
                ips.ToggleEquipmentStatDescriptor(false);
                ips.ToggleWeaponStatDescriptor(false);
                if (!shopItemScript.itemType.Equals("Character")) {
                    ips.SetRestrictions(shopItemScript.equipmentDetails.gender, shopItemScript.equipmentDetails.characterRestrictions);
                }
                ips.ToggleClothingStatDescriptor(true);
            }
        }
    }

    public void ShowSetupItemData() {
        setupItemScript.itemDescriptionPopupRef.gameObject.SetActive(true);
        if (setupItemScript.setupItemType == SetupItemScript.SetupItemType.Character) {
            setupItemScript.itemDescriptionPopupRef.weaponStatDescriptor.SetActive(false);
            setupItemScript.itemDescriptionPopupRef.SetTitle(setupItemScript.itemName);
            setupItemScript.itemDescriptionPopupRef.SetThumbnail(setupItemScript.thumbnailRef);
            setupItemScript.itemDescriptionPopupRef.SetDescription(setupItemScript.itemDescription);
        } else if (setupItemScript.setupItemType == SetupItemScript.SetupItemType.Weapon) {
            Weapon w = InventoryScript.itemData.weaponCatalog[setupItemScript.setupController.weaponSelector.GetCurrentItem()];
            setupItemScript.itemDescriptionPopupRef.SetTitle(w.name);
            setupItemScript.itemDescriptionPopupRef.SetThumbnail((Texture)Resources.Load(w.thumbnailPath));
            setupItemScript.itemDescriptionPopupRef.SetDescription(w.description);
            setupItemScript.itemDescriptionPopupRef.SetWeaponStats(w.damage, w.accuracy, w.recoil, w.fireRate, w.mobility, w.range, w.clipCapacity);
            setupItemScript.itemDescriptionPopupRef.weaponStatDescriptor.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (shopItemScript != null) {
            shopItemScript.itemDescriptionPopupRef.SetActive(false);
        } else if (setupItemScript != null) {
            setupItemScript.itemDescriptionPopupRef.gameObject.SetActive(false);
        }
    }
}
