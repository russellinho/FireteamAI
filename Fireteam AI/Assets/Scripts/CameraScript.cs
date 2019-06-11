using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraScript : MonoBehaviour
{
    public Transform weaponHolderTrans;
    public FirstPersonController fpc;

    // Update is called once per frame
    void LateUpdate()
    {
        bool isReloading = fpc.weaponActionScript.isReloading;
        if (!fpc.m_IsRunning && !isReloading) {
            transform.forward = Vector3.Slerp(transform.forward, weaponHolderTrans.up, 5f * Time.deltaTime);
        } else {
            transform.forward = Vector3.Slerp(transform.forward, new Vector3(transform.forward.x, transform.forward.y, transform.forward.z), 5f * Time.deltaTime);
        }
    }

    void OnPostRender() {
        if (fpc.playerActionScript.hud.screenGrab) {
            fpc.playerActionScript.hud.TriggerFlashbangEffect();
            fpc.playerActionScript.hud.screenGrab = false;
        }
    }
}
