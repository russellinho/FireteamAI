using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentScript : MonoBehaviour
{

	public GameObject primaryWeapon;
    public GameObject weaponSocket;
    
    public string equippedCharacter;
    public string equippedHeadgear;
    public string equippedFacewear;
    public string equippedTop;
    public string equippedBottom;
    public string equippedFootwear;
    public string equippedArmor;

    public GameObject equippedCharacterRef;
    public GameObject equippedHeadgearRef;
    public GameObject equippedFacewearRef;
    public GameObject equippedTopRef;
    public GameObject equippedBottomRef;
    public GameObject equippedFootwearRef;
    public GameObject equippedArmorTopRef;
    public GameObject equippedArmorBottomRef;

    //public GameObject myCharacterRenderer;
    public GameObject myHeadgearRenderer;
    public GameObject myFacewearRenderer;
    public GameObject myTopRenderer;
    public GameObject myBottomRenderer;
    public GameObject myFootwearRenderer;
    public GameObject myArmorTopRenderer;
    public GameObject myArmorBottomRenderer;
    public GameObject mySkinRenderer;

    public GameObject myBones;

    public void EquipDefaults() {
        RemoveFacewear();
        RemoveArmor();
        RemoveHeadgear();
        EquipTop("Standard Fatigues Top");
        EquipBottom("Standard Fatigues Bottom");
        EquipFootwear("Standard Boots");
    }

    public void EquipTop(string name) {
        equippedTop = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedTopRef != null) {
                Destroy(equippedTopRef);
                equippedTopRef = null;
            }
            equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedTopRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
            m.target = myTopRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void EquipBottom(string name) {
        equippedBottom = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedBottomRef != null) {
                Destroy(equippedBottomRef);
                equippedBottomRef = null;
            }
            equippedBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedBottomRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
            m.target = myBottomRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void EquipFootwear(string name) {
        equippedFootwear = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedFootwearRef != null) {
                Destroy(equippedFootwearRef);
                equippedFootwearRef = null;
            }
            equippedFootwearRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedFootwearRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
            m.target = myFootwearRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void EquipFacewear(string name) {
        equippedFacewear = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedFacewearRef != null) {
                Destroy(equippedFacewearRef);
                equippedFacewearRef = null;
            }
            equippedFacewearRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedFacewearRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
            m.target = myFacewearRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void EquipHeadgear(string name) {
        equippedHeadgear = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedHeadgearRef != null) {
                Destroy(equippedHeadgearRef);
                equippedHeadgearRef = null;
            }
            equippedHeadgearRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedHeadgearRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
            m.target = myHeadgearRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void EquipArmor(string name) {
        equippedArmor = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedArmorTopRef != null) {
                Destroy(equippedArmorTopRef);
                equippedArmorTopRef = null;
            }
            if (equippedArmorBottomRef != null) {
                Destroy(equippedArmorBottomRef);
                equippedArmorBottomRef = null;
            }
            equippedArmorTopRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name + " Top"]));
            equippedArmorTopRef.transform.SetParent(gameObject.transform);
            equippedArmorBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name + " Bottom"]));
            equippedArmorBottomRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
            m.target = myArmorTopRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();

            m = equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
            m.target = myArmorBottomRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

    public void RemoveHeadgear() {
        equippedHeadgear = "";
        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }
    }

    public void RemoveFacewear() {
        equippedFacewear = "";
        if (equippedFacewearRef != null) {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }
    }

    public void RemoveArmor() {
        equippedArmor = "";
        if (equippedArmorTopRef != null) {
            Destroy(equippedArmorTopRef);
            equippedArmorTopRef = null;
        }
        if (equippedArmorBottomRef != null) {
            Destroy(equippedArmorBottomRef);
            equippedArmorBottomRef = null;
        }
    }

}
