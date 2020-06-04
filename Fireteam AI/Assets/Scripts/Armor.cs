using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor
{
    public string name;
    public int malePrefabPathTop;
    public int malePrefabPathBottom;
    public int femalePrefabPathTop;
    public int femalePrefabPathBottom;
    public string category;
    public string thumbnailPath;
    public string description;
    public float speed;
    public float stamina;
    public float armor;
    public bool purchasable;
    public int gpPrice;

    public Armor(string name, int malePrefabPathTop, int malePrefabPathBottom, int femalePrefabPathTop, int femalePrefabPathBottom, string thumbnailPath, string description, float speed, float stamina, float armor, int gpPrice, bool purchasable) {
        this.name = name;
        this.category = "Armor";
        this.malePrefabPathTop = malePrefabPathTop;
        this.malePrefabPathBottom = malePrefabPathBottom;
        this.femalePrefabPathTop = femalePrefabPathTop;
        this.femalePrefabPathBottom = femalePrefabPathBottom;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.speed = speed;
        this.stamina = stamina;
        this.armor = armor;
        this.gpPrice = gpPrice;
        this.purchasable = purchasable;
    }
}
