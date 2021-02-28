using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GiftEntryScript : MonoBehaviour
{
    private const float NINETY_DAYS_MINS = 129600f;
	private const float THIRTY_DAYS_MINS = 43200f;
	private const float SEVEN_DAYS_MINS = 10080f;
	private const float ONE_DAY_MINS = 1440f;

    public GiftInbox giftInbox;
    private string giftId;
    public RawImage itemPic;
    public TextMeshProUGUI senderTxt;
    public TextMeshProUGUI itemNameTxt;
    public TextMeshProUGUI durationTxt;
    public TextMeshProUGUI messageTxt;

    public void InitEntry(GiftInbox giftInbox, string giftId, string category, string sender, string itemName, float duration, string message)
    {
        this.giftInbox = giftInbox;
        this.giftId = giftId;
        this.itemNameTxt.text = itemName;
        this.durationTxt.text = ConvertDurationToText(duration);
        this.senderTxt.text = sender;
        this.messageTxt.text = message;
        if (category == "Mod") {
            this.itemPic.texture = (Texture)Resources.Load(InventoryScript.itemData.modCatalog[itemName].thumbnailPath);
        } else if (category == "Weapon") {
            this.itemPic.texture = (Texture)Resources.Load(InventoryScript.itemData.weaponCatalog[itemName].thumbnailPath);
        } else if (category == "Character") {
            this.itemPic.texture = (Texture)Resources.Load(InventoryScript.itemData.characterCatalog[itemName].thumbnailPath);
        } else if (category == "Armor") {
            this.itemPic.texture = (Texture)Resources.Load(InventoryScript.itemData.armorCatalog[itemName].thumbnailPath);
        } else {
            this.itemPic.texture = (Texture)Resources.Load(InventoryScript.itemData.equipmentCatalog[itemName].thumbnailPath);
        }
    }

    private string ConvertDurationToText(float duration)
    {
        string ret = null;
        if (duration == 1f) {
            ret = "PERMANENT";
        } else if (duration == NINETY_DAYS_MINS) {
            ret = "90 DAYS";
        } else if (duration == THIRTY_DAYS_MINS) {
            ret = "30 DAYS";
        } else if (duration == SEVEN_DAYS_MINS) {
            ret = "7 DAYS";
        } else if (duration == ONE_DAY_MINS) {
            ret = "1 DAY";
        }
        return ret;
    }

    public string GetGiftId()
    {
        return this.giftId;
    }

    public void OnEntryClick()
    {
        giftInbox.quickActionMenu.InitButtonsForGiftInbox();
        giftInbox.quickActionMenu.SetActingOnEntry(this);
        giftInbox.quickActionMenu.gameObject.SetActive(true);
        // Move menu to mouse position
        giftInbox.quickActionMenu.UpdatePosition();
    }
}
