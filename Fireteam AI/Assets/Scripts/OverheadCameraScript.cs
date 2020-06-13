using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadCameraScript : MonoBehaviour
{
    public Transform mainCamTrans;

    // Update is called once per frame
    void Update()
    {
        transform.up = mainCamTrans.forward;
    }
}
