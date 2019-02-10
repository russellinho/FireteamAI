using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemScript : MonoBehaviour
{
    public string itemName;
    public string itemType;
    // 1 = long sleeves, 2 = mid sleeves, 3 = short sleeves
    public int skinType;

    public void EquipItem()
    {
        switch (itemType)
        {
            case "Character":
                //PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipCharacter(itemName);
                break;
            case "Top":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipTop(itemName, skinType);
                break;
            case "Bottom":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipBottom(itemName);
                break;
            case "Footwear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipFootwear(itemName);
                break;
            case "Headgear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipHeadgear(itemName);
                break;
            case "Facewear":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipFacewear(itemName);
                break;
            case "Armor":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipArmor(itemName);
                break;
        }
    }
}
