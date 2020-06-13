using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadCameraScript : MonoBehaviour
{
    public Transform mainCamTrans;

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(90f, 0f, -mainCamTrans.rotation.eulerAngles.y);
    }
}
