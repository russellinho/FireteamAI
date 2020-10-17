using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionPopupRef;
    public GameObject modDescriptionPopupRef;
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
    private string expirationDate;
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

    void Start() {
        clickCount = 0;
        clickTimer = 0f;
        ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        // Only allow previewing for characters, equipment, and primary weapons
        if (previewBtn != null) {
            if (modDetails != null) {
                previewBtn.gameObject.SetActive(false);
            }
            if (weaponDetails != null && (weaponDetails.type != "Primary")) {
                previewBtn.gameObject.SetActive(false);
            }
        } else
        {
            if (modDetails != null)
            {
                ts.removeSuppressorBtn.onClick.AddListener(() => OnRemoveSuppressorClicked());
                ts.removeSightBtn.onClick.AddListener(() => OnRemoveSightClicked());
            }
        }
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
                if (gpPriceTxt == null) {
                    EquipItem();
                } else {
                    PreviewItem();
                }
            } else {
                EquipMod();
            }
            clickTimer = 0f;
            clickCount = 0;
        }
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

    private void EquipItem()
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

    private void EquipMod() {
        // if (equippedOn == ts.modWeaponLbl.text)
        // {
        //     return;
        // }
        switch (modCategory)
        {
            case "Suppressor":
                // If this weapon already has a suppressor on it, unequip it first
                OnRemoveSuppressorClicked();

                // If this mod is equipped to another weapon, unequip it from that weapon as well
                if (equippedOn != null && !"".Equals(equippedOn))
                {
                    ts.RemoveSuppressorFromWeapon(equippedOn, false);
                    // Ensure that it gets saved in the DB
                    PlayerData.playerdata.SaveModDataForWeapon(equippedOn, "", null, id, null);
                }

                // Attach to player weapon and attach to weapon mod template as well
                string weaponNameAttachedTo = ts.EquipModOnWeaponTemplate(itemName, modCategory, id);
                equippedOn = weaponNameAttachedTo;
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipMod(modCategory, itemName, weaponNameAttachedTo, gameObject);
                break;
            case "Sight":
                // If this weapon already has a sight on it, unequip it first
                OnRemoveSightClicked();

                // If this mod is equipped to another weapon, unequip it from that weapon as well
                if (equippedOn != null && !"".Equals(equippedOn))
                {
                    ts.RemoveSightFromWeapon(equippedOn, false);
                    // Ensure that it gets saved in the DB
                    PlayerData.playerdata.SaveModDataForWeapon(equippedOn, null, "", null, id);
                }

                // Attach to player weapon and attach to weapon mod template as well
                weaponNameAttachedTo = ts.EquipModOnWeaponTemplate(itemName, modCategory, id);
                equippedOn = weaponNameAttachedTo;
                PlayerData.playerdata.bodyReference.GetComponent<WeaponScript>().EquipMod(modCategory, itemName, weaponNameAttachedTo, gameObject);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (modDetails == null && purchaseBtn == null && (expirationDate == null || "".Equals(expirationDate))) {
            CalculateExpirationDate();
        }
        if (itemType.Equals("Mod")) {
            if (modDescriptionPopupRef.activeInHierarchy) {
                return;
            }
            modDescriptionPopupRef.SetActive(true);
            ItemPopupScript ips = modDescriptionPopupRef.GetComponent<ItemPopupScript>();
            ips.SetExpirationDate(expirationDate);
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
            ips.SetExpirationDate(expirationDate);
            ips.SetTitle(itemName);
            ips.SetThumbnail(thumbnailRef);
            ips.SetDescription(itemDescription);
            if (itemType.Equals("Headgear") || itemType.Equals("Facewear")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetEquipmentStats(equipmentDetails.armor, equipmentDetails.speed, equipmentDetails.stamina, equipmentDetails.gender, equipmentDetails.characterRestrictions);
                ips.ToggleEquipmentStatDescriptor(true);
                ips.SetRestrictions(equipmentDetails.gender, equipmentDetails.characterRestrictions);
            } else if (itemType.Equals("Armor")) {
                ips.ToggleWeaponStatDescriptor(false);
                ips.SetArmorStats(armorDetails.armor, armorDetails.speed, armorDetails.stamina);
                ips.ToggleEquipmentStatDescriptor(true);
            } else if (itemType.Equals("Weapon")) {
                ips.ToggleEquipmentStatDescriptor(false);
                ips.SetWeaponStats(weaponDetails.damage, weaponDetails.accuracy, weaponDetails.recoil, weaponDetails.fireRate, weaponDetails.mobility, weaponDetails.range, weaponDetails.clipCapacity);
                ips.ToggleWeaponStatDescriptor(true);
            } else {
                // For clothing and shoes
                ips.ToggleEquipmentStatDescriptor(false);
                ips.ToggleWeaponStatDescriptor(false);
                if (!itemType.Equals("Character")) {
                    ips.SetRestrictions(equipmentDetails.gender, equipmentDetails.characterRestrictions);
                }
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

    private void CalculateExpirationDate() {
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
        } else {
            outline.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
        }
    }

    public void OnRemoveSuppressorClicked()
    {
        // if (equippedOn != ts.modWeaponLbl.text)
        // {
        //     return;
        // }
        // Remove suppressor model from the player's weapon and the template weapon
        ts.RemoveSuppressorFromWeapon(equippedOn, true);
        ToggleEquippedIndicator(false);
        equippedOn = "";
    }

    public void OnRemoveSightClicked() {
        // if (equippedOn != ts.modWeaponLbl.text) {
        //     return;
        // }
        // Remove sight model from the player's weapon and the template weapon
        ts.RemoveSightFromWeapon(equippedOn, true);
        ToggleEquippedIndicator(false);
        equippedOn = "";
    }

    public void SetItemForMarket() {
        gpPriceTxt.gameObject.SetActive(true);
        previewBtn.gameObject.SetActive(true);
        purchaseBtn.gameObject.SetActive(true);
        equipBtn.gameObject.SetActive(false);
    }

    public void SetItemForLoadout() {
        gpPriceTxt.gameObject.SetActive(false);
        previewBtn.gameObject.SetActive(false);
        purchaseBtn.gameObject.SetActive(false);
        equipBtn.gameObject.SetActive(true);
    }

}
