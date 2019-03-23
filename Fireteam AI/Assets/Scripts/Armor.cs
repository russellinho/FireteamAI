using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor
{
    public string name;
    public string prefabPathTop;
    public string prefabPathBottom;
    public string category;
    public string thumbnailPath;
    public string description;
    public float speed;
    public float stamina;
    public float armor;

    public Armor(string name, string prefabPathTop, string prefabPathBottom, string thumbnailPath, string description, float speed, float stamina, float armor) {
        this.name = name;
        this.category = "Armor";
        this.prefabPathTop = prefabPathTop;
        this.prefabPathBottom = prefabPathBottom;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.speed = speed;
        this.stamina = stamina;
        this.armor = armor;
    }
}
