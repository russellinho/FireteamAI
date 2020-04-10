using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSwitchScript : MonoBehaviour
{
    public Transform obj;
    public Transform myTrans;

    public void SwitchTransform()
    {
        Transform[] childTrans = obj.gameObject.GetComponentsInChildren<Transform>();
        Transform[] theseTrans = myTrans.gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childTrans.Length; i++) {
            theseTrans[i].position = childTrans[i].position;
            theseTrans[i].rotation = childTrans[i].rotation;
            theseTrans[i].localScale = childTrans[i].localScale;
        }
    }
}
