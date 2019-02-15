using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{

    // Storage for all characters in the game
    public static Dictionary<string, string> characterInventoryCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> characterSkinCatalog = new Dictionary<string, string>();
    // Mapping for all items in the database for Lucas - key is the item name and value is the
    // database path to load from
    public static Dictionary<string, string> lucasInventoryCatalog = new Dictionary<string, string>();

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

    void Awake() {
        // Characters
        characterSkinCatalog.Add("Lucas0", "Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly");
        characterSkinCatalog.Add("Lucas1", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid");
        characterSkinCatalog.Add("Lucas2", "Models/Characters/Lucas/Extra Skins/Ankles Short Sleeves/lucasanklesshortsleeve");
        thumbnailGallery.Add("Lucas", "Models/Pics/character_lucas");
        thumbnailGallery.Add("Daryl", "Models/Pics/character_donald");
        thumbnailGallery.Add("Codename Sayre", "Models/Pics/character_sayre");
        thumbnailGallery.Add("Hana", "Models/Pics/character_hana");
        thumbnailGallery.Add("Jade", "Models/Pics/character_jade");

        // Tops
        lucasInventoryCatalog.Add("Casual Shirt", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt");
        lucasInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)");
        lucasInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop");
        thumbnailGallery.Add("Casual Shirt", "Models/Pics/casual_shirt");
        thumbnailGallery.Add("Casual T-Shirt", "Models/Pics/v_neck_shirt");
        thumbnailGallery.Add("Standard Fatigues Top", "Models/Pics/standard_fatigue_shirt");
        
        // Bottoms
        lucasInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans");
        lucasInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans");
        lucasInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom");
        thumbnailGallery.Add("Dark Wash Denim Jeans", "Models/Pics/dark_wash_jeans");
        thumbnailGallery.Add("Light Wash Denim Jeans", "Models/Pics/light_wash_jeans");
        thumbnailGallery.Add("Standard Fatigues Bottom", "Models/Pics/standard_fatigue_pants");

        // Headgear
        lucasInventoryCatalog.Add("MICH", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich");
        lucasInventoryCatalog.Add("Combat Beanie", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie");
        lucasInventoryCatalog.Add("COM Hat", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat");
        thumbnailGallery.Add("MICH", "Models/Pics/mich");
        thumbnailGallery.Add("Combat Beanie", "Models/Pics/combat_beanie");
        thumbnailGallery.Add("COM Hat", "Models/Pics/com_hat");

        // Facewear
        lucasInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        lucasInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        lucasInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        thumbnailGallery.Add("Saint Laurent Mask", "Models/Pics/saint_laurent_mask");
        thumbnailGallery.Add("Sport Shades", "Models/Pics/sport_shades");
        thumbnailGallery.Add("Standard Goggles", "Models/Pics/standard_goggles");

        // Footwear
        lucasInventoryCatalog.Add("Red Chucks", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks");
        lucasInventoryCatalog.Add("Standard Boots", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots");
        thumbnailGallery.Add("Red Chucks", "Models/Pics/red_chucks");
        thumbnailGallery.Add("Standard Boots", "Models/Pics/standard_boots");

        // Armor
        lucasInventoryCatalog.Add("Standard Vest Top", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop");
        lucasInventoryCatalog.Add("Standard Vest Bottom", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom");
        thumbnailGallery.Add("Standard Vest", "Models/Pics/standard_vest");

        collectCharacters();
        collectTops();
        collectBottoms();
        collectHeadgear();
        collectFacewear();
        collectFootwear();
        collectArmor();
    }
    
    void collectCharacters() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myCharacters.Add("Lucas");
        myCharacters.Add("Daryl");
        myCharacters.Add("Hana");
        myCharacters.Add("Jade");
        myCharacters.Add("Codename Sayre");

    }

    void collectTops() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myTops.Add("Standard Fatigues Top");
        myTops.Add("Casual Shirt");
        myTops.Add("Casual T-Shirt");
    }

    void collectBottoms() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myBottoms.Add("Standard Fatigues Bottom");
        myBottoms.Add("Dark Wash Denim Jeans");
        myBottoms.Add("Light Wash Denim Jeans");
    }

    void collectHeadgear() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myHeadgear.Add("MICH");
        myHeadgear.Add("Combat Beanie");
        myHeadgear.Add("COM Hat");
    }

    void collectFacewear() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myFacewear.Add("Standard Goggles");
        myFacewear.Add("Sport Shades");
        myFacewear.Add("Saint Laurent Mask");
    }

    void collectFootwear() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myFootwear.Add("Standard Boots");
        myFootwear.Add("Red Chucks");
    }

    void collectArmor() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myArmor.Add("Standard Vest");
    }

}
