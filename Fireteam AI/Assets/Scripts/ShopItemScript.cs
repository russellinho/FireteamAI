using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionPopupRef;
    public RawImage thumbnailRef;
    public string itemName;
    public string itemType;
    // 1 = long sleeves, 2 = mid sleeves, 3 = short sleeves
    public int skinType;
    public Text equippedInd;

    public void EquipItem()
    {
        switch (itemType)
        {
            case "Character":
                PlayerData.playerdata.ChangeBodyRef(itemName, gameObject);
                break;
            case "Top":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipTop(itemName, skinType, gameObject);
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
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        itemDescriptionPopupRef.SetActive(true);
        ItemPopupScript ips = itemDescriptionPopupRef.GetComponent<ItemPopupScript>();
        ips.SetTitle(itemName);
        ips.SetThumbnail(thumbnailRef);
        ips.SetDescription(InventoryScript.itemDescriptionCatalog[itemName]);
    }

    public void OnPointerExit(PointerEventData eventData) {
        itemDescriptionPopupRef.SetActive(false);
    }
}
