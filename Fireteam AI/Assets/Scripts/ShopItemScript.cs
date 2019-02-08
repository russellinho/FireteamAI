using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemScript : MonoBehaviour
{
    public string itemName;
    public string itemType;

    public void EquipItem()
    {
        switch (itemType)
        {
            case "Character":
                break;
            case "Top":
                PlayerData.playerdata.bodyReference.GetComponent<EquipmentScript>().EquipTop(itemName);
                break;
            case "Bottom":
                break;
            case "Footwear":
                break;
            case "Headgear":
                break;
            case "Facegear":
                break;
            case "Armor":
                break;
        }
    }
}
