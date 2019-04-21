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
        if (!fpc.m_IsRunning) {
            transform.forward = weaponHolderTrans.up;
        } else {
            transform.forward = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
        }
    }
}
