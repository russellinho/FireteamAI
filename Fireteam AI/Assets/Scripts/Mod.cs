using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mod
{
    public string name;
    public int prefabPath;
    public string category;
    public string thumbnailPath;
    public int modIndex;
    public string crosshairPath;
    public string description;
    public float damageBoost;
    public float accuracyBoost;
    public float recoilBoost;
    public float rangeBoost;
    public int clipCapacityBoost;
    public int maxAmmoBoost;
    public bool purchasable;
    public int gpPrice;

    public Mod(string name, string category, int prefabPath, string thumbnailPath, int modIndex, string crosshairPath, string description, float damageBoost, float accuracyBoost, float recoilBoost, float rangeBoost, int clipCapacityBoost, int maxAmmoBoost, int gpPrice, bool purchasable) {
        this.name = name;
        this.category = category;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
        this.modIndex = modIndex;
        this.crosshairPath = crosshairPath;
        this.description = description;
        this.damageBoost = damageBoost;
        this.accuracyBoost = accuracyBoost;
        this.recoilBoost = recoilBoost;
        this.rangeBoost = rangeBoost;
        this.clipCapacityBoost = clipCapacityBoost;
        this.maxAmmoBoost = maxAmmoBoost;
        this.gpPrice = gpPrice;
        this.purchasable = purchasable;
    }

}
