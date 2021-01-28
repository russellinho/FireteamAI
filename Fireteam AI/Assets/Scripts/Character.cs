using System.Collections;
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
    public int kashPrice;
    public string defaultTop;
    public string defaultBottom;
    public bool deleteable;

    public Character(string name, char gender, string prefabPath, int fpcFullSkinPath, int fpcNoSkinPath, string thumbnailPath, string description, int[] skins, int gpPrice, int kashPrice, bool purchasable, string defaultTop, string defaultBottom, bool deleteable) {
        this.skins = skins;
        this.name = name;
        this.gender = gender;
        this.prefabPath = prefabPath;
        this.fpcFullSkinPath = fpcFullSkinPath;
        this.fpcNoSkinPath = fpcNoSkinPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.gpPrice = gpPrice;
        this.kashPrice = kashPrice;
        this.purchasable = purchasable;
        this.defaultTop = defaultTop;
        this.defaultBottom = defaultBottom;
        this.deleteable = deleteable;
    }

}
