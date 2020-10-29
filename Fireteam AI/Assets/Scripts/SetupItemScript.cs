using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupItemScript : MonoBehaviour
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
    public Image outline;
    public Button selectBtn;
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

    public void EquipItem()
    {
        setupController.SelectCharacter(this, characterDetails.name);
    }

    public void ToggleSelectedIndicator(bool b) {
        if (b) {
            outline.color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
            selectBtn.gameObject.SetActive(false);
        } else {
            outline.color = new Color(99f / 255f, 198f / 255f, 255f / 255f, 255f / 255f);
            selectBtn.gameObject.SetActive(true);
        }
    }

}
