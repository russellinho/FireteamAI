using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionPopupRef;
    public RawImage thumbnailRef;
    public Character characterDetails;
    public Equipment equipmentDetails;
    public Armor armorDetails;
    public Weapon weaponDetails;
    public string itemName;
    public string itemType;
    public string itemDescription;
    public string weaponCategory;
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
            EquipItem();
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
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipWeapon(weaponCategory, itemName, gameObject);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
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
            ips.SetEquipmentStats(equipmentDetails.armor, equipmentDetails.speed, equipmentDetails.stamina);
            ips.ToggleEquipmentStatDescriptor(true);
        } else if (itemType.Equals("Armor")) {
            ips.ToggleWeaponStatDescriptor(false);
            ips.SetEquipmentStats(armorDetails.armor, armorDetails.speed, armorDetails.stamina);
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

    public void OnPointerExit(PointerEventData eventData) {
        itemDescriptionPopupRef.SetActive(false);
    }
}
