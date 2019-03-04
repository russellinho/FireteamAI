using System.Collections;
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

    public Equipment(string name, string category, string prefabPath, string thumbnailPath, string description, bool hideHairFlag) {
        this.name = name;
        this.category = category;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.hideHairFlag = hideHairFlag;
    }

}
