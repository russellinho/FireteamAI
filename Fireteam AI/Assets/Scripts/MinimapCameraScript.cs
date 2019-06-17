using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraScript : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        transform.localPosition = new Vector3(0f, 24.88f, 0f);
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
