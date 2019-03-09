﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public string name;
    public string prefabPath;
    public string category;
    public string thumbnailPath;
    public string description;
    public bool hideHairFlag;
    // Percent speed, stamina, and armor change
    public float speed;
    public float stamina;
    public float armor;

    public Equipment(string name, string category, string prefabPath, string thumbnailPath, string description, bool hideHairFlag, float speed, float stamina, float armor) {
        this.name = name;
        this.category = category;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.hideHairFlag = hideHairFlag;
        this.speed = speed;
        this.stamina = stamina;
        this.armor = armor;
    }

}
