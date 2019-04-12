using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform weaponHolderTrans;

    // Update is called once per frame
    void Update()
    {
        transform.forward = weaponHolderTrans.up;
    }
}
