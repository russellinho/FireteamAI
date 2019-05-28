using System.Collections;
using System.Collections.Generic;

public class Mod
{
    public string name;
    public string prefabPath;
    public string category;
    public string thumbnailPath;
    public string description;
    public float damageBoost;
    public float accuracyBoost;
    public float recoilBoost;
    public float rangeBoost;
    public int clipCapacityBoost;
    public int maxAmmoBoost;

    public Mod(string name, string category, string prefabPath, string thumbnailPath, string description, float damageBoost, float accuracyBoost, float recoilBoost, float rangeBoost, int clipCapacityBoost, int maxAmmoBoost) {
        this.name = name;
        this.category = category;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.damageBoost = damageBoost;
        this.accuracyBoost = accuracyBoost;
        this.recoilBoost = recoilBoost;
        this.rangeBoost = rangeBoost;
        this.clipCapacityBoost = clipCapacityBoost;
        this.maxAmmoBoost = maxAmmoBoost;
    }

}
