﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentScript : MonoBehaviour
{

    public TitleControllerScript ts;
	public GameObject primaryWeapon;
    public GameObject weaponSocket;
    
    public string equippedCharacter;
    public string equippedHeadgear;
    public string equippedFacewear;
    public string equippedTop;
    public string equippedBottom;
    public string equippedFootwear;
    public string equippedArmor;
    public string equippedSkin;

    public GameObject equippedSkinRef;
    public GameObject equippedHeadgearRef;
    public GameObject equippedFacewearRef;
    public GameObject equippedTopRef;
    public GameObject equippedBottomRef;
    public GameObject equippedFootwearRef;
    public GameObject equippedArmorTopRef;
    public GameObject equippedArmorBottomRef;

    public GameObject myHeadgearRenderer;
    public GameObject myFacewearRenderer;
    public GameObject myTopRenderer;
    public GameObject myBottomRenderer;
    public GameObject myFootwearRenderer;
    public GameObject myArmorTopRenderer;
    public GameObject myArmorBottomRenderer;
    public GameObject mySkinRenderer;
    public GameObject myGlovesRenderer;
    public GameObject myEyesRenderer;
    public GameObject myEyelashRenderer;
    public GameObject myHairRenderer;

    public GameObject myBones;

    public void EquipDefaults() {
        RemoveFacewear();
        RemoveArmor();
        RemoveHeadgear();
        EquipTop("Standard Fatigues Top", 0, null);
        EquipBottom("Standard Fatigues Bottom", null);
        EquipFootwear("Standard Boots", null);
    }

    public void EquipCharacter(string name, GameObject shopItemRef) {
        equippedCharacter = name;
        ChangeCharacter();

        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
        }

        // Sets item that you just equipped to orange in the shop
        if (shopItemRef != null) {
            shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
            shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
            ts.currentlyEquippedItemPrefab = shopItemRef;
        }

        ts.equippedCharacterSlot.enabled = true;
        ts.equippedCharacterSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name]);
    }

    public void EquipTop(string name, int skinType, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedTop)) {
                return;
            }
            equippedTop = name;
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
            
            EquipSkin("Lucas" + skinType);

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedTopSlot.enabled = true;
            ts.equippedTopSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name + " M"]);
        }
    }

    private void EquipSkin(string name) {
        if (name.Equals(equippedSkin)) {
            return;
        }
        equippedSkin = name;
        if (equippedSkinRef != null) {
            Destroy(equippedSkinRef);
            equippedSkinRef = null;
        }
        equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(InventoryScript.characterSkinCatalog[name]));
        equippedSkinRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    public void EquipBottom(string name, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedBottom)) {
                return;
            }
            equippedBottom = name;
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

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedBottomSlot.enabled = true;
            ts.equippedBottomSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name + " M"]);
        }
    }

    public void EquipFootwear(string name, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedFootwear)) {
                return;
            }
            equippedFootwear = name;
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

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedFootSlot.enabled = true;
            ts.equippedFootSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name]);
        }
    }

    public void EquipFacewear(string name, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedFacewear)) {
                return;
            }
            equippedFacewear = name;
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

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedFaceSlot.enabled = true;
            ts.equippedFaceSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name]);
        }
    }

    public void EquipHeadgear(string name, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedHeadgear)) {
                return;
            }
            equippedHeadgear = name;
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

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedHeadSlot.enabled = true;
            ts.equippedHeadSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name]);
        }
    }

    public void EquipArmor(string name, GameObject shopItemRef) {
        if (equippedCharacter.Equals("Lucas")) {
            if (name.Equals(equippedArmor)) {
                return;
            }
            equippedArmor = name;
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

            // Sets item that you unequipped to white
            if (ts.currentlyEquippedItemPrefab != null) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
            }

            // Sets item that you just equipped to orange in the shop
            if (shopItemRef != null) {
                shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                ts.currentlyEquippedItemPrefab = shopItemRef;
            }

            ts.equippedArmorSlot.enabled = true;
            ts.equippedArmorSlot.texture = (Texture)Resources.Load(InventoryScript.thumbnailGallery[name]);
        }
    }

    public void RemoveHeadgear() {
        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }
        ts.equippedHeadSlot.texture = null;
        ts.equippedHeadSlot.enabled = false;
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Headgear")) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
                ts.currentlyEquippedItemPrefab = null;
            }
        }
        equippedHeadgear = "";
    }

    public void RemoveFacewear() {
        if (equippedFacewearRef != null) {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }
        ts.equippedFaceSlot.texture = null;
        ts.equippedFaceSlot.enabled = false;
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Facewear")) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
                ts.currentlyEquippedItemPrefab = null;
            }
        }
        equippedFacewear = "";
    }

    public void RemoveArmor() {
        if (equippedArmorTopRef != null) {
            Destroy(equippedArmorTopRef);
            equippedArmorTopRef = null;
        }
        if (equippedArmorBottomRef != null) {
            Destroy(equippedArmorBottomRef);
            equippedArmorBottomRef = null;
        }
        ts.equippedArmorSlot.texture = null;
        ts.equippedArmorSlot.enabled = false;
        // Sets item that you unequipped to white
        if (ts.currentlyEquippedItemPrefab != null) {
            ShopItemScript s = ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>();
            if (s.itemType.Equals("Armor")) {
                ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
                ts.currentlyEquippedItemPrefab = null;
            }
        }
        equippedArmor = "";
    }

    // Changes renderers to new character renderers and sets clothes to default set
    void ChangeCharacter() {
        GameObject o = (GameObject)Resources.Load(InventoryScript.characterRenderers[equippedCharacter]);
        SkinnedMeshRenderer[] os = o.GetComponentsInChildren<SkinnedMeshRenderer>();
        // Change 
        Destroy(myHeadgearRenderer);
        myHeadgearRenderer = null;
        myHeadgearRenderer = Instantiate(os[0].gameObject);
        myHeadgearRenderer.transform.SetParent(gameObject.transform);

        Destroy(myFacewearRenderer);
        myFacewearRenderer = null;
        myFacewearRenderer = Instantiate(os[1].gameObject);
        myFacewearRenderer.transform.SetParent(gameObject.transform);

        Destroy(myTopRenderer);
        myTopRenderer = null;
        myTopRenderer = Instantiate(os[2].gameObject);
        myTopRenderer.transform.SetParent(gameObject.transform);

        Destroy(myBottomRenderer);
        myBottomRenderer = null;
        myBottomRenderer = Instantiate(os[3].gameObject);
        myBottomRenderer.transform.SetParent(gameObject.transform);

        Destroy(myFootwearRenderer);
        myFootwearRenderer = null;
        myFootwearRenderer = Instantiate(os[4].gameObject);
        myFootwearRenderer.transform.SetParent(gameObject.transform);

        myArmorTopRenderer = null;
        myArmorTopRenderer = myTopRenderer;

        myArmorBottomRenderer = null;
        myArmorBottomRenderer = myBottomRenderer;

        Destroy(mySkinRenderer);
        mySkinRenderer = null;
        mySkinRenderer = Instantiate(os[5].gameObject);
        mySkinRenderer.transform.SetParent(gameObject.transform);

        Destroy(myBones);
        myBones = null;
        myBones = Instantiate(os[6].gameObject);
        myBones.transform.SetParent(gameObject.transform);

        Destroy(myGlovesRenderer);
        myGlovesRenderer = null;
        myGlovesRenderer = Instantiate(os[7].gameObject);
        myGlovesRenderer.transform.SetParent(gameObject.transform);
        
        Destroy(myEyesRenderer);
        myEyesRenderer = null;
        myEyesRenderer = Instantiate(os[8].gameObject);
        myEyesRenderer.transform.SetParent(gameObject.transform);

        if (myEyelashRenderer != null) {
            Destroy(myEyelashRenderer);
        }
        myEyelashRenderer = null;
        if (os.Length > 9) {
            myEyelashRenderer = Instantiate(os[9].gameObject);
            myEyelashRenderer.transform.SetParent(gameObject.transform);
        }

        if (myHairRenderer != null) {
            Destroy(myHairRenderer);
        }
        myHairRenderer = null;
        if (os.Length > 10) {
            myHairRenderer = Instantiate(os[10].gameObject);

        }

        EquipDefaults();
    }

}
