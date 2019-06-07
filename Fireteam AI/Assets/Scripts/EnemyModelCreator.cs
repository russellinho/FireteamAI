using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class EnemyModelCreator : MonoBehaviour
{
    public string enemyName;
    public int modelNumber;

    private string equippedHeadgear;
    private string equippedFacewear;
    private string equippedEyewear;
    private string equippedTop;
    private string equippedBottom;
    private string equippedFootwear;
    public int equippedSkin;
    
    public GameObject equippedSkinRef;
    public GameObject equippedHeadgearRef;
    public GameObject equippedFacewearRef;
    public GameObject equippedEyewearRef;
    public GameObject equippedTopRef;
    public GameObject equippedBottomRef;
    public GameObject equippedFootwearRef;

    public GameObject myHeadgearRenderer;
    public GameObject myFacewearRenderer;
    public GameObject myTopRenderer;
    public GameObject myBottomRenderer;
    public GameObject myFootwearRenderer;
    public GameObject mySkinRenderer;
    public GameObject myEyewearRenderer;
    public GameObject myHairRenderer;
    public GameObject myBeardRenderer;

    public GameObject myBones;
    public PhotonView pView;

    void EquipRandomOutfitForEnemy() {
        if (enemyName.Equals("Cicadas")) {
            GenerateRandomOutfitForCicadas();
            
        }
    }

    void GenerateRandomOutfitForCicadas() {
        // Choose a shirt first as it will determine which skin to equip
        int r = Random.Range(0, 3);
        if (r == 0) {
            // Camo tank
            equippedTop = "Camo Tank";
            equippedSkin = 1;
        } else if (r == 1) {
            // Camo shirt/short sleeve shirt
            equippedTop = "Camo Shirt";
            equippedSkin = 2;
        } else {
            // Camo top/long sleeve shirt
            equippedTop = "Camo Top";
            equippedSkin = 3;
        }

        r = Random.Range(0, 3);
        // Now that we have the skin and shirt, we can pick out pants, which will determine shoes as well
        if (r == 0) {
            // Cargo pants
            equippedBottom = "Cargo Pants";
            equippedFootwear = "Combat Boots";
        } else if (r == 1) {
            // Cargo shorts
            equippedBottom = "Cargo Shorts";
            equippedFootwear = "Combat Shoes";
        } else {
            // Cargo jeans
            equippedBottom = "Cargo Jeans";
            equippedFootwear = "Combat Shoes";
        }

        // 20% chance of wearing a ski mask
        r = Random.Range(0, 5);
        if (r == 0) {
            equippedFacewear = "Ski Mask";
        }
        
        // If the ski mask wasn't equipped, maybe wear glasses and/or a hat
        if (r != 0) {
            // Maybe wear a hat - 20% chance
            r = Random.Range(0, 5);
            if (r == 0) {
                // Baseball cap
                equippedHeadgear = "Baseball Hat";
            } else {
                equippedHeadgear = null;
            }

            // Maybe wear glasses - 30% chance
            r = Random.Range(0, 4);
            if (r == 0) {
                equippedEyewear = "Sport Glasses";
            } else {
                equippedEyewear = null;
            }
        }
    }

    void EquipGeneratedOutfitForCicadas() {
        MeshFixer m = null;
        // Rules for guy with beard
        if (modelNumber == 1) {
            // First equip the correct skin
            if (equippedSkinRef != null) {
                Destroy(equippedSkinRef);
                equippedSkinRef = null;
            }
            equippedSkinRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/" + modelNumber + "/Skin" + equippedSkin + "/cicada" + modelNumber + "skin" + equippedSkin);
            equippedSkinRef.transform.SetParent(gameObject.transform);
            m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
            m.target = mySkinRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();

            // Second, equip the correct top
            if (equippedTopRef != null) {
                Destroy(equippedTopRef);
                equippedTopRef = null;
            }
            if (equippedTop.Equals("Camo Tank")) {
                equippedTopRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Tops/1/camotank");
            } else if (equippedTop.Equals("Camo Shirt")) {
                equippedTopRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Tops/2/camotop");
            } else if (equippedTop.Equals("Camo Top")) {
                equippedTopRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Tops/3/camoshirt");
            }
            equippedTopRef.transform.SetParent(gameObject.transform);
            m = equippedTopRef.GetComponentInChildren<MeshFixer>();
            m.target = myTopRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();

            // Third, equip the correct bottoms
            if (equippedBottomRef != null) {
                Destroy(equippedBottomRef);
                equippedBottomRef = null;
            }
            if (equippedBottom.Equals("Cargo Pants")) {
                equippedBottomRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Bottoms/1/cargos");
            } else if (equippedBottom.Equals("Cargo Shorts")) {
                equippedBottomRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Bottoms/3/cargoshorts");
            } else if (equippedBottom.Equals("Cargo Jeans")) {
                equippedBottomRef = (GameObject)Resources.Load("Models/Enemies/Cicadas/Clothing/Bottoms/2/cargojeans");
            }
            equippedBottomRef.transform.SetParent(gameObject.transform);
            m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
            m.target = myBottomRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();

            // Fourth, equip the correct shoes

            // Equip facewear

            // Equip eyewear

            // Equip hats
        } else if (modelNumber == 2) {
            // Rules for orange hair guy

        } else if (modelNumber == 3) {
            // Rules for blonde long hair guy

        } else if (modelNumber == 4) {
            // Rules for black dude

        } else {
            // Rules for long moustache guy

        }
    }

}
