using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloadCreatorScript : MonoBehaviour
{
    public int frameIndex;
    public Vector3[] framePositions;
    public Transform weaponTransform;
    public Transform attachToTransform;
    public Transform partTransform;

    public void RecordPosition() {
        // Set back to weapon parent
        partTransform.SetParent(weaponTransform);

        // Record the local transform
        framePositions[frameIndex] = partTransform.localPosition;

        // Set back to body part transform
        partTransform.SetParent(attachToTransform);
    }

}
