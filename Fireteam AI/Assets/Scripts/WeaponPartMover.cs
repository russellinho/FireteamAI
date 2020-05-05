using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPartMover : MonoBehaviour
{
    public Vector3 translation;
    public Transform[] parts;

    public void TranslateParts() {
        for (int i = 0; i < parts.Length; i++) {
            Vector3 oldPos = parts[i].localPosition;
            parts[i].localPosition = new Vector3(oldPos.x + translation.x, oldPos.y + translation.y, oldPos.z + translation.z);
        }
    }
}
