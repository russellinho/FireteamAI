﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public string name;
    public string prefabPath;
    public char gender;
    public string thumbnailPath;
    public string description;
    public int[] skins;
    public int fpcFullSkinPath;
    public int fpcNoSkinPath;
    public bool purchasable;
    public int gpPrice;
    public string defaultTop;
    public string defaultBottom;

    public Character(string name, char gender, string prefabPath, int fpcFullSkinPath, int fpcNoSkinPath, string thumbnailPath, string description, int[] skins, int gpPrice, bool purchasable, string defaultTop, string defaultBottom) {
        this.skins = skins;
        this.name = name;
        this.gender = gender;
        this.prefabPath = prefabPath;
        this.fpcFullSkinPath = fpcFullSkinPath;
        this.fpcNoSkinPath = fpcNoSkinPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.gpPrice = gpPrice;
        this.purchasable = purchasable;
        this.defaultTop = defaultTop;
        this.defaultBottom = defaultBottom;
    }

}
