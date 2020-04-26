using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableScript : MonoBehaviour
{
    public int deployableId;
    private const short MAX_FIRST_AID_KIT_USES = 6;
    private const short MAX_AMMO_BAG_USES = 8;
    public string deployableName;
    private short usesRemaining;
    // Determines if deployable can be stuck to any surface

    public int InstantiateDeployable() {
        if (deployableName.Equals("First Aid Kit")) {
            usesRemaining = MAX_FIRST_AID_KIT_USES;
        } else if (deployableName.Equals("Ammo Bag")) {
            usesRemaining = MAX_AMMO_BAG_USES;
        }
        deployableId = gameObject.GetInstanceID();
        return deployableId;
    }

    public void UseDeployableItem() {
        usesRemaining--;
    }

    public bool CheckOutOfUses() {
        return (usesRemaining == 0 ? true : false);
    }

}
