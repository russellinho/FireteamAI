using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerScript : MonoBehaviour
{
    public Transform overheadCam;

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = overheadCam.localRotation;
    }
}
