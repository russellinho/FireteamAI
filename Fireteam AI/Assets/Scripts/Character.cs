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
    public string[] skins;
    public Dictionary<string, Equipment> equipmentCatalog;
    public Dictionary<string, Armor> armorCatalog;
    public string fpcFullSkinPath;
    public string fpcNoSkinPath;

    public Character(string name, char gender, string prefabPath, string fpcFullSkinPath, string fpcNoSkinPath, string thumbnailPath, string description, string[] skins, Dictionary<string, Equipment> equipmentCatalog, Dictionary<string, Armor> armorCatalog) {
        this.skins = skins;
        this.name = name;
        this.gender = gender;
        this.prefabPath = prefabPath;
        this.fpcFullSkinPath = fpcFullSkinPath;
        this.fpcNoSkinPath = fpcNoSkinPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.equipmentCatalog = equipmentCatalog;
        this.armorCatalog = armorCatalog;
    }

}
