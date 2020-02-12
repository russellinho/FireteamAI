using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMods : MonoBehaviour
{
    // Suppressor attachment
    public Vector3 suppressorScaler;
    public Transform suppressorPos;
    public GameObject suppressorRef;
    private Mod suppressorStats;
    private string suppressorName;
    // Size to scale the suppressor by for this weapon

    public void EquipSuppressor(string suppressorName) {
        if (suppressorName.Equals(this.suppressorName)) return;
        // Load the prefab/game object for the suppressor
        Mod newSuppressorStats = InventoryScript.itemData.modCatalog[suppressorName];
        // If error occurred and suppressor wasn't found, cancel the procedure
        if (newSuppressorStats == null) {
            return;
        }
        // Unequip the previous suppressor just in case
        UnequipSuppressor();
        // Set the suppressor name
        this.suppressorName = suppressorName;
        suppressorStats = newSuppressorStats;
        suppressorRef = (GameObject)Instantiate(Resources.Load(suppressorStats.prefabPath));
        // Equip it and place it in the correct position
        suppressorRef.transform.SetParent(suppressorPos);
        suppressorRef.transform.localPosition = Vector3.zero;
        suppressorRef.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
        suppressorRef.transform.localScale = suppressorScaler;
    }

    public void UnequipSuppressor() {
        // Set the suppressor name
        this.suppressorName = null;
        // Destroy the previous suppressor game object
        if (suppressorRef != null) {
            Destroy(suppressorRef);
        }
        suppressorRef = null;
        suppressorStats = null;
    }

    public string GetEquippedSuppressor() {
        return this.suppressorName;
    }

    public Mod GetEquippedSuppressorStats() {
        return this.suppressorStats;
    }

}
