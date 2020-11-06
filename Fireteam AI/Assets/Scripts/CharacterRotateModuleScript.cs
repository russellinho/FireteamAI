using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;

public class CharacterRotateModuleScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private bool mouseIn;
    private bool rotateActive;
    public GameObject weaponPreviewSlot;
    public char type;

    // Update is called once per frame
    void Update()
    {
        if (mouseIn && Input.GetMouseButtonDown(0)) {
            rotateActive = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            rotateActive = false;
        }

        if (rotateActive) {
            Cursor.visible = false;
            if (type == 'C') {
                float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * 12f;
                Vector3 rotationAmount = new Vector3(0f, -yRot, 0f);
                if (PlayerData.playerdata.bodyReference.activeInHierarchy) {
                    PlayerData.playerdata.bodyReference.transform.Rotate(rotationAmount);
                }
            } else if (type == 'W') {
                float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * 12f;
                float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * 12f;
                Vector3 rotationAmount = new Vector3(xRot, -yRot, 0f);
                if (weaponPreviewSlot.activeInHierarchy) {
                    weaponPreviewSlot.transform.Rotate(rotationAmount);
                }
            }
        } else {
            Cursor.visible = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        mouseIn = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        mouseIn = false;
    }

}
