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

    public Character(string name, char gender, string prefabPath, string thumbnailPath, string description, string[] skins, Dictionary<string, Equipment> equipmentCatalog, Dictionary<string, Armor> armorCatalog) {
        this.skins = skins;
        this.name = name;
        this.gender = gender;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.equipmentCatalog = equipmentCatalog;
        this.armorCatalog = armorCatalog;
    }

}
