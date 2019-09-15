using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformDebugger : MonoBehaviour
{
    public bool logPosition;
    public bool logRotation;
    public bool logScale;

    // Update is called once per frame
    void Update()
    {
        if (logPosition) {
            Debug.Log("pos[" + gameObject.name + "] = {" + transform.localPosition + "}");
        }

        if (logRotation) {
            Debug.Log("pos[" + gameObject.name + "] = {" + transform.localRotation.eulerAngles + "}");
        }

        if (logScale) {
            Debug.Log("pos[" + gameObject.name + "] = {" + transform.localScale + "}");
        }
    }
}
