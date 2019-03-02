using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    // Storage for weapons in the game
    public static Dictionary<string, Weapon> weaponCatalog = new Dictionary<string, Weapon>();

    // Storage for all characters in the game
    public static Dictionary<string, string> itemDescriptionCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> characterSkinCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> characterPrefabs = new Dictionary<string, string>();
    // Mapping for all items in the database for Lucas - key is the item name and value is the
    // database path to load from
    public static Dictionary<string, string> lucasInventoryCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> darylInventoryCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> sayreInventoryCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> hanaInventoryCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> jadeInventoryCatalog = new Dictionary<string, string>();

    // Inventory thumbnails
    public static Dictionary<string, string> thumbnailGallery = new Dictionary<string, string>();

    // Items that are in the player inventory/items they own
    public static ArrayList myCharacters = new ArrayList();
    public static ArrayList myTops = new ArrayList();
    public static ArrayList myBottoms = new ArrayList();
    public static ArrayList myHeadgear = new ArrayList();
    public static ArrayList myFacewear = new ArrayList();
    public static ArrayList myFootwear = new ArrayList();
    public static ArrayList myArmor = new ArrayList();
    public static ArrayList myWeapons = new ArrayList();

    void Awake() {
        // Characters
        itemDescriptionCatalog.Add("Lucas", "Nationality: British\nAs a reformed professional criminal, Lucas works swiftly and gets the job done.");
        itemDescriptionCatalog.Add("Daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him in combat.");
        itemDescriptionCatalog.Add("Codename Sayre", "Nationality: Mexican\nBeing fresh out of medical school at the top of his class, Codename Sayre is skilled in his healing abilities. His witty style of humor allows him to maneuver through sticky situations easily.");
        itemDescriptionCatalog.Add("Hana", "Nationality: Japanese\nWhen her entire family was murdered as a kid, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, but don't be fooled by her size.");
        itemDescriptionCatalog.Add("Jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.");
        itemDescriptionCatalog.Add("Casual T-Shirt", "A casual t-shirt to wear on the street.");
        itemDescriptionCatalog.Add("Casual Tank Top", "A casual tank top to wear on the street.");
        itemDescriptionCatalog.Add("Standard Fatigues Top", "A standard issue shirt given to all solders upon completion of basic training.");
        itemDescriptionCatalog.Add("Standard Fatigues Bottom", "A standard issue pants given to all soldiers upon completion of basic training.");
        itemDescriptionCatalog.Add("Light Wash Denim Jeans", "Light wash jeans for casual wear.");
        itemDescriptionCatalog.Add("Dark Wash Denim Jeans", "Dark wash jeans for casual wear.");
        itemDescriptionCatalog.Add("MICH", "An overarching helmet that can be used for protecting one's head from shrapnel and even bullets.");
        itemDescriptionCatalog.Add("COM Hat", "A lightweight hat with a mic for optimal communication.");
        itemDescriptionCatalog.Add("Combat Beanie", "A stylish hat straight out of your local designer clothing store.");
        itemDescriptionCatalog.Add("Saint Laurent Mask", "Eliminate your enemies in style with these expensive yet stylish glasses!");
        itemDescriptionCatalog.Add("Sport Shades", "Tinted shades with a sporty trim usually used for the shooting range.");
        itemDescriptionCatalog.Add("Standard Goggles", "Standard issue goggles given to all soldiers upon completion of basic training.");
        itemDescriptionCatalog.Add("Surgical Mask", "A protective, lightweight mask used during medical surgeries.");
        itemDescriptionCatalog.Add("Red Chucks", "These bright and stylish canvas shoes are stylish yet lightweight, durable, and comfortable!");
        itemDescriptionCatalog.Add("White Chucks", "The white version of the red chucks; stylish yet lightweight, durable, and comfortable!");
        itemDescriptionCatalog.Add("Standard Boots", "Standard issue combat boots given to all soldiers upon completion of basic training.");
        itemDescriptionCatalog.Add("Standard Vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.");
        itemDescriptionCatalog.Add("Scrubs Top", "A comfortable scrubs shirt commonly used in the medical field.");
        itemDescriptionCatalog.Add("Scrubs Bottom", "A comfortable scrubs pants commonly used in the medical field.");

        characterPrefabs.Add("Lucas", "Models/Characters/Lucas/PlayerPrefabLucas");
        characterPrefabs.Add("Daryl", "Models/Characters/Daryl/PlayerPrefabDaryl");
        characterPrefabs.Add("Codename Sayre", "Models/Characters/Sayre/PlayerPrefabCodenameSayre");
        characterPrefabs.Add("Hana", "Models/Characters/Hana/PlayerPrefabHana");
        characterPrefabs.Add("Jade", "Models/Characters/Jade/PlayerPrefabJade");

        characterSkinCatalog.Add("Lucas0", "Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly");
        characterSkinCatalog.Add("Lucas1", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid");
        characterSkinCatalog.Add("Lucas2", "Models/Characters/Lucas/Extra Skins/Ankles Short Sleeves/lucasanklesshortsleeve");
        characterSkinCatalog.Add("Daryl0", "Models/Characters/Daryl/1/skindonald1");
        characterSkinCatalog.Add("Daryl1", "Models/Characters/Daryl/2/skindonald2");
        characterSkinCatalog.Add("Daryl2", "Models/Characters/Daryl/3/skindonald3");
        characterSkinCatalog.Add("Sayre0", "Models/Characters/Sayre/1/skinslayre1");
        characterSkinCatalog.Add("Sayre1", "Models/Characters/Sayre/2/skinslayre2");
        characterSkinCatalog.Add("Sayre2", "Models/Characters/Sayre/3/skinslayre3");
        characterSkinCatalog.Add("Hana0", "Models/Characters/Hana/1/skinhana1");
        characterSkinCatalog.Add("Hana2", "Models/Characters/Hana/2/skinhana2");
        characterSkinCatalog.Add("Hana1", "Models/Characters/Hana/3/skinhana3");
        characterSkinCatalog.Add("Jade0", "Models/Characters/Jade/1/skinjade1");
        characterSkinCatalog.Add("Jade1", "Models/Characters/Jade/2/skinjade2");
        characterSkinCatalog.Add("Jade2", "Models/Characters/Jade/3/skinjade3");

        thumbnailGallery.Add("Lucas", "Models/Pics/character_lucas");
        thumbnailGallery.Add("Daryl", "Models/Pics/character_daryl");
        thumbnailGallery.Add("Codename Sayre", "Models/Pics/character_sayre");
        thumbnailGallery.Add("Hana", "Models/Pics/character_hana");
        thumbnailGallery.Add("Jade", "Models/Pics/character_jade");

        // Tops
        lucasInventoryCatalog.Add("Casual Shirt", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt");
        lucasInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)");
        lucasInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop");
        darylInventoryCatalog.Add("Casual Shirt", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt");
        darylInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)");
        darylInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop");
        sayreInventoryCatalog.Add("Casual Shirt", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt");
        sayreInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)");
        sayreInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop");
        sayreInventoryCatalog.Add("Scrubs Top", "Models/Clothing/Sayre/Tops/scrubstop");
        hanaInventoryCatalog.Add("Casual Tank Top", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop");
        hanaInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt");
        hanaInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop");
        jadeInventoryCatalog.Add("Casual Tank Top", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop");
        jadeInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt");
        jadeInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop");

        thumbnailGallery.Add("Casual Shirt M", "Models/Pics/casual_shirt");
        thumbnailGallery.Add("Casual T-Shirt M", "Models/Pics/v_neck_shirt");
        thumbnailGallery.Add("Standard Fatigues Top M", "Models/Pics/standard_fatigue_shirt");
        thumbnailGallery.Add("Scrubs Top M", "Models/Pics/scrubs_top");
        thumbnailGallery.Add("Casual Tank Top F", "Models/Pics/casual_tank_top_f");
        thumbnailGallery.Add("Casual T-Shirt F", "Models/Pics/casual_t_shirt_f");
        thumbnailGallery.Add("Standard Fatigues Top F", "Models/Pics/standard_fatigue_shirt_f");
        
        // Bottoms
        lucasInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans");
        lucasInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans");
        lucasInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom");
        darylInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans");
        darylInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans");
        darylInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom");
        sayreInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans");
        sayreInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans");
        sayreInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom");
        sayreInventoryCatalog.Add("Scrubs Bottom", "Models/Clothing/Sayre/Bottoms/scrubspants");
        hanaInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans");
        hanaInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans");
        hanaInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom");
        jadeInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans");
        jadeInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans");
        jadeInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom");

        thumbnailGallery.Add("Dark Wash Denim Jeans M", "Models/Pics/dark_wash_jeans");
        thumbnailGallery.Add("Light Wash Denim Jeans M", "Models/Pics/light_wash_jeans");
        thumbnailGallery.Add("Standard Fatigues Bottom M", "Models/Pics/standard_fatigue_pants");
        thumbnailGallery.Add("Scrubs Bottom M", "Models/Pics/scrubs_bottom");
        thumbnailGallery.Add("Dark Wash Denim Jeans F", "Models/Pics/dark_wash_jeans_f");
        thumbnailGallery.Add("Light Wash Denim Jeans F", "Models/Pics/light_wash_jeans_f");
        thumbnailGallery.Add("Standard Fatigues Bottom F", "Models/Pics/standard_fatigue_pants");

        // Headgear
        lucasInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        lucasInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        lucasInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        darylInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        darylInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        darylInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        sayreInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        sayreInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        sayreInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        hanaInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        hanaInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        hanaInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        jadeInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        jadeInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        jadeInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        
        thumbnailGallery.Add("MICH", "Models/Pics/mich");
        thumbnailGallery.Add("Combat Beanie", "Models/Pics/combat_beanie");
        thumbnailGallery.Add("COM Hat", "Models/Pics/com_hat");

        // Facewear
        lucasInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        lucasInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        lucasInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        lucasInventoryCatalog.Add("Surgical Mask", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask");
        darylInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        darylInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        darylInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        darylInventoryCatalog.Add("Surgical Mask", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask");
        sayreInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        sayreInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        sayreInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        sayreInventoryCatalog.Add("Surgical Mask", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask");
        hanaInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        hanaInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        hanaInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        hanaInventoryCatalog.Add("Surgical Mask", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask");
        jadeInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        jadeInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        jadeInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        jadeInventoryCatalog.Add("Surgical Mask", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask");

        thumbnailGallery.Add("Saint Laurent Mask", "Models/Pics/saint_laurent_mask");
        thumbnailGallery.Add("Sport Shades", "Models/Pics/sport_shades");
        thumbnailGallery.Add("Standard Goggles", "Models/Pics/standard_goggles");
        thumbnailGallery.Add("Surgical Mask", "Models/Pics/surgical_mask");

        // Footwear
        lucasInventoryCatalog.Add("Red Chucks", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks");
        lucasInventoryCatalog.Add("Standard Boots", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots");
        darylInventoryCatalog.Add("Red Chucks", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks");
        darylInventoryCatalog.Add("Standard Boots", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots");
        sayreInventoryCatalog.Add("Red Chucks", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks");
        sayreInventoryCatalog.Add("Standard Boots", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots");
        hanaInventoryCatalog.Add("White Chucks", "Models/Clothing/Hana/Shoes/White Chucks/hanawhitechucks");
        hanaInventoryCatalog.Add("Standard Boots", "Models/Clothing/Hana/Shoes/Standard Boots/hanastandardboots");
        jadeInventoryCatalog.Add("White Chucks", "Models/Clothing/Hana/Shoes/White Chucks/hanawhitechucks");
        jadeInventoryCatalog.Add("Standard Boots", "Models/Clothing/Hana/Shoes/Standard Boots/hanastandardboots");

        thumbnailGallery.Add("Red Chucks", "Models/Pics/red_chucks");
        thumbnailGallery.Add("Standard Boots", "Models/Pics/standard_boots");
        thumbnailGallery.Add("White Chucks", "Models/Pics/white_chucks");

        // Armor
        lucasInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        lucasInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        darylInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        darylInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        sayreInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        sayreInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        hanaInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        hanaInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        jadeInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        jadeInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        
        thumbnailGallery.Add("Standard Vest", "Models/Pics/standard_vest");

        // Weapons
        weaponCatalog.Add("AK-47", new Weapon("AK-47", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/AK-47", "Models/Pics/ak47-thumb"));
        weaponCatalog.Add("Glock23", new Weapon("Glock23", "Secondary", "Pistol", "Models/Weapons/Secondary/Pistols/Glock23", "Models/Pics/glock23-thumb"));
        weaponCatalog.Add("R870", new Weapon("R870", "Primary", "Shotgun","Models/Weapons/Primary/Shotguns/R870", "Models/Pics/r870-thumb"));
        weaponCatalog.Add("L96A1", new Weapon("L96A1", "Primary", "Sniper Rifle", "Models/Weapons/Primary/Sniper Rifles/L96A1", "Models/Pics/l96a1-thumb"));
        weaponCatalog.Add("M4A1", new Weapon("M4A1", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/M4A1", "Models/Pics/m4a1-thumb"));

        collectCharacters();
        collectWeapons();
    }
    
    public static void collectCharacters() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myCharacters.Add("Lucas");
        myCharacters.Add("Daryl");
        myCharacters.Add("Jade");
        myCharacters.Add("Hana");
        myCharacters.Add("Codename Sayre");

    }

    public static void collectWeapons() {
        myWeapons.Add("AK-47");
        myWeapons.Add("Glock23");
        myWeapons.Add("R870");
        myWeapons.Add("M4A1");
        myWeapons.Add("L96A1");
    }

    public static void collectTops(string character) {
        myTops.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        if (character.Equals("Lucas") || character.Equals("Daryl") || character.Equals("Codename Sayre")) {
            if (character.Equals("Codename Slayre")) {
                myTops.Add("Scrubs Top");
            }
            myTops.Add("Standard Fatigues Top");
            myTops.Add("Casual Shirt");
            myTops.Add("Casual T-Shirt");
        } else {
            myTops.Add("Standard Fatigues Top");
            myTops.Add("Casual T-Shirt");
            myTops.Add("Casual Tank Top");
        }
    }

    public static void collectBottoms(string character) {
        myBottoms.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        if (character.Equals("Lucas") || character.Equals("Daryl") || character.Equals("Codename Sayre")) {
            if (character.Equals("Codename Slayre")) {
                myTops.Add("Scrubs Bottom");
            }
            myBottoms.Add("Standard Fatigues Bottom");
            myBottoms.Add("Dark Wash Denim Jeans");
            myBottoms.Add("Light Wash Denim Jeans");
        } else {
            myBottoms.Add("Standard Fatigues Bottom");
            myBottoms.Add("Dark Wash Denim Jeans");
            myBottoms.Add("Light Wash Denim Jeans");
        }
    }

    public static void collectHeadgear(string character) {
        myHeadgear.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myHeadgear.Add("MICH");
        myHeadgear.Add("Combat Beanie");
        myHeadgear.Add("COM Hat");
    }

    public static void collectFacewear(string character) {
        myFacewear.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myFacewear.Add("Standard Goggles");
        myFacewear.Add("Saint Laurent Mask");
        myFacewear.Add("Sport Shades");
        myFacewear.Add("Surgical Mask");
    }

    public static void collectFootwear(string character) {
        myFootwear.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        if (character.Equals("Lucas") || character.Equals("Daryl") || character.Equals("Codename Sayre")) {
            myFootwear.Add("Standard Boots");
            myFootwear.Add("Red Chucks");
        } else {
            myFootwear.Add("Standard Boots");
            myFootwear.Add("White Chucks");
        }
    }

    public static void collectArmor(string character) {
        myArmor.Clear();
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myArmor.Add("Standard Vest");
    }

}
