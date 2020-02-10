using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionPopupRef;
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
        if (itemDescriptionPopupRef.activeInHierarchy) {
            return;
        }
        itemDescriptionPopupRef.SetActive(true);
        ItemPopupScript ips = itemDescriptionPopupRef.GetComponent<ItemPopupScript>();
        ips.SetTitle(itemName);
        ips.SetThumbnail(thumbnailRef);
        ips.SetDescription(itemDescription);
    }

    public void OnPointerExit(PointerEventData eventData) {
        itemDescriptionPopupRef.SetActive(false);
    }

}
