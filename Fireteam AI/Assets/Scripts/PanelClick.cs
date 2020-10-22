using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class PanelClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItemScript shopItemScript;
    public void OnPointerEnter(PointerEventData eventData) {
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
            } else if (shopItemScript.itemType.Equals("Armor")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetArmorStats(shopItemScript.armorDetails.armor, shopItemScript.armorDetails.speed, shopItemScript.armorDetails.stamina);
                ips.ToggleEquipmentStatDescriptor(true);
            } else if (shopItemScript.itemType.Equals("Weapon")) {
                ips.ToggleEquipmentStatDescriptor(false);
                ips.SetWeaponStats(shopItemScript.weaponDetails.damage, shopItemScript.weaponDetails.accuracy, shopItemScript.weaponDetails.recoil, shopItemScript.weaponDetails.fireRate, shopItemScript.weaponDetails.mobility, shopItemScript.weaponDetails.range, shopItemScript.weaponDetails.clipCapacity);
                ips.ToggleWeaponStatDescriptor(true);
            } else {
                // For clothing and shoes
                ips.ToggleEquipmentStatDescriptor(false);
                ips.ToggleWeaponStatDescriptor(false);
                if (!shopItemScript.itemType.Equals("Character")) {
                    ips.SetRestrictions(shopItemScript.equipmentDetails.gender, shopItemScript.equipmentDetails.characterRestrictions);
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        shopItemScript.itemDescriptionPopupRef.SetActive(false);
    }
}
