using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAnimCreator : MonoBehaviour
{
    public Vector3 initialPos;
    public Vector3 newInitialPos;

    public void DetermineDifference() {
        Vector3 diff = transform.localPosition - initialPos;
        diff += newInitialPos;
        Debug.Log("x: " + diff.x + ", y: " + diff.y + ", z: " + diff.z);
    }
}
