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

    public void EquipTop(string name) {
        equippedTop = name;

        if (equippedCharacter.Equals("Lucas")) {
            if (equippedTopRef != null) {
                Destroy(equippedTopRef);
                equippedTopRef = null;
            }
            equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.lucasInventoryCatalog[name]));
            equippedTopRef.transform.SetParent(gameObject.transform);
            MeshFixer m = equippedTopRef.GetComponent<MeshFixer>();
            m.target = myTopRenderer.gameObject;
            m.rootBone = myBones.transform;
            m.AdaptMesh();
        }
    }

}
