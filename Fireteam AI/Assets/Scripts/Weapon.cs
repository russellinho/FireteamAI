﻿using System.Collections;
using System.Collections.Generic;

public class Weapon
{
    public string name;
    public int prefabPath;
    public string projectilePath;
    public string type;
    public string category;
    public string thumbnailPath;
    public string description;
    public float damage;
    public float mobility;
    public float fireRate;
    public float accuracy;
    public float recoil;
    public float range;
    public int clipCapacity;
    public int maxAmmo;
    public bool canBeModded;
    public bool suppressorCompatible;
    public bool sightCompatible;
    public bool purchasable;
    public bool deleteable;
    public int gpPrice;

    public Weapon(string name, string type, string category, int prefabPath, string projectilePath, string thumbnailPath, string description, float damage, float mobility, float fireRate, float accuracy, float recoil, float range, int clipCapacity, int maxAmmo, bool canBeModded, bool suppressorCompatible, bool sightCompatible, int gpPrice, bool purchasable, bool deleteable) {
        this.name = name;
        this.type = type;
        this.category = category;
        this.prefabPath = prefabPath;
        this.projectilePath = projectilePath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.damage = damage;
        this.mobility = mobility;
        this.fireRate = fireRate;
        this.accuracy = accuracy;
        this.recoil = recoil;
        this.range = range;
        this.clipCapacity = clipCapacity;
        this.maxAmmo = maxAmmo;
        this.suppressorCompatible = suppressorCompatible;
        this.sightCompatible = sightCompatible;
        this.canBeModded = canBeModded;
        this.gpPrice = gpPrice;
        this.purchasable = purchasable;
        this.deleteable = deleteable;
    }

}
