using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public string name;
    public string malePrefabPath;
    public string femalePrefabPath;
    public string maleFpcPrefabPath;
    public string femaleFpcPrefabPath;
    public string category;
    public string thumbnailPath;
    public string description;
    public bool hideHairFlag;
    public int skinType;
    // Percent speed, stamina, and armor change
    public float speed;
    public float stamina;
    public float armor;
    public char gender;
    public string[] characterRestrictions;

    public Equipment(string name, string category, string malePrefabPath, string femalePrefabPath, string maleFpcPrefabPath, string femaleFpcPrefabPath, string thumbnailPath, string description, bool hideHairFlag, int skinType, float speed, float stamina, float armor, char gender, string[] characterRestrictions) {
        this.name = name;
        this.category = category;
        this.malePrefabPath = malePrefabPath;
        this.femalePrefabPath = femalePrefabPath;
        this.maleFpcPrefabPath = maleFpcPrefabPath;
        this.femaleFpcPrefabPath = femaleFpcPrefabPath;
        this.thumbnailPath = thumbnailPath;
        this.description = description;
        this.hideHairFlag = hideHairFlag;
        this.skinType = skinType;
        this.speed = speed;
        this.stamina = stamina;
        this.armor = armor;
        this.gender = gender;
        this.characterRestrictions = characterRestrictions;
    }

    private int CheckSkinType(string clothingName, char gender) {
		if (gender == 'F') {
			if (clothingName.Equals("Casual T-Shirt")) {
				return 1;
			} else if (clothingName.Equals("Casual Tank Top")) {
				return 2;
			}
			return 0;
		} else {
			if (clothingName.Equals("Casual T-Shirt")) {
				return 2;
			} else if (clothingName.Equals("Casual Shirt")) {
				return 1;
			}
			return 0;
		}
	}

}
