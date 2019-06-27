using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;

public class CharacterRotateModuleScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private bool mouseIn;
    private bool rotateActive;

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
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * 12f;
            Vector3 rotationAmount = new Vector3(0f, -yRot, 0f);
            PlayerData.playerdata.bodyReference.transform.Rotate(rotationAmount);
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
