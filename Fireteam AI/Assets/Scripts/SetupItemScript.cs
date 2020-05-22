using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum SetupItemType {Character, Weapon};
    public SetupItemType setupItemType;
    public ItemPopupScript itemDescriptionPopupRef;
    public RawImage thumbnailRef;
    public Character characterDetails;
    public string itemName;
    public string itemDescription;
    private int clickCount;
    private float clickTimer;
    public Text equippedInd;
    public SetupControllerScript setupController;

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
        setupController.SelectCharacter(gameObject, characterDetails.name);
    }

    public void OnPointerEnter(PointerEventData eventData) {

        if (itemDescriptionPopupRef.gameObject.activeInHierarchy) {
            return;
        }
        itemDescriptionPopupRef.gameObject.SetActive(true);
        if (setupItemType == SetupItemType.Character) {
            itemDescriptionPopupRef.weaponStatDescriptor.SetActive(false);
            itemDescriptionPopupRef.SetTitle(itemName);
            itemDescriptionPopupRef.SetThumbnail(thumbnailRef);
            itemDescriptionPopupRef.SetDescription(itemDescription);
        } else if (setupItemType == SetupItemType.Weapon) {
            Weapon w = InventoryScript.itemData.weaponCatalog[(string)setupController.starterWeapons[setupController.wepSelectionIndex]];
            itemDescriptionPopupRef.SetTitle(w.name);
            itemDescriptionPopupRef.SetThumbnail((Texture)Resources.Load(w.thumbnailPath));
            itemDescriptionPopupRef.SetDescription(w.description);
            itemDescriptionPopupRef.SetWeaponStats(w.damage, w.accuracy, w.recoil, w.fireRate, w.mobility, w.range, w.clipCapacity);
            itemDescriptionPopupRef.weaponStatDescriptor.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        itemDescriptionPopupRef.gameObject.SetActive(false);
        itemDescriptionPopupRef.weaponStatDescriptor.SetActive(false);
    }

}
