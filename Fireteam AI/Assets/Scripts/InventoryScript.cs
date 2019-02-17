using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{

    // Storage for all characters in the game
    public static Dictionary<string, int> characterInventoryCatalog = new Dictionary<string, int>();
    public static Dictionary<string, string> characterSkinCatalog = new Dictionary<string, string>();
    public static Dictionary<string, string> characterRenderers = new Dictionary<string, string>();
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

    void Awake() {
        // Characters
        characterInventoryCatalog.Add("Lucas", 1);
        characterInventoryCatalog.Add("Daryl", 1);
        characterInventoryCatalog.Add("Codename Sayre", 1);
        characterInventoryCatalog.Add("Hana", 1);
        characterInventoryCatalog.Add("Jade", 1);

        characterRenderers.Add("Lucas", "Models/Characters/Lucas/lucas_preset");
        characterRenderers.Add("Daryl", "Models/Characters/Daryl/daryl_preset");
        characterRenderers.Add("Codename Sayre", "Models/Characters/Sayre/sayre_preset");
        characterRenderers.Add("Hana", "Models/Characters/Hana/hana_preset");
        characterRenderers.Add("Jade", "Models/Characters/Jade/jade_preset");

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
        characterSkinCatalog.Add("Hana1", "Models/Characters/Hana/2/skinhana2");
        characterSkinCatalog.Add("Hana2", "Models/Characters/Hana/3/skinhana3");
        characterSkinCatalog.Add("Jade0", "Models/Characters/Jade/1/skinjade1");
        characterSkinCatalog.Add("Jade1", "Models/Characters/Jade/2/skinjade2");
        characterSkinCatalog.Add("Jade2", "Models/Characters/Jade/3/skinjade3");

        thumbnailGallery.Add("Lucas", "Models/Pics/character_lucas");
        thumbnailGallery.Add("Daryl", "Models/Pics/character_donald");
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
        hanaInventoryCatalog.Add("Casual Tank Top", "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt");
        hanaInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop");
        hanaInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop");
        jadeInventoryCatalog.Add("Casual Tank Top", "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt");
        jadeInventoryCatalog.Add("Casual T-Shirt", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop");
        jadeInventoryCatalog.Add("Standard Fatigues Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop");

        thumbnailGallery.Add("Casual Shirt M", "Models/Pics/casual_shirt");
        thumbnailGallery.Add("Casual T-Shirt M", "Models/Pics/v_neck_shirt");
        thumbnailGallery.Add("Standard Fatigues Top M", "Models/Pics/standard_fatigue_shirt");
        thumbnailGallery.Add("Casual Tank Top F", "Models/Pics/casual_tank_top_f");
        thumbnailGallery.Add("Casual T-Shirt F", "Models/Pics/casual_tank_top_f");
        thumbnailGallery.Add("Standard Fatigues Top F", "Models/Pics/casual_tank_top_f");
        
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
        hanaInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans");
        hanaInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans");
        hanaInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom");
        jadeInventoryCatalog.Add("Dark Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans");
        jadeInventoryCatalog.Add("Light Wash Denim Jeans", "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans");
        jadeInventoryCatalog.Add("Standard Fatigues Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom");

        thumbnailGallery.Add("Dark Wash Denim Jeans M", "Models/Pics/dark_wash_jeans");
        thumbnailGallery.Add("Light Wash Denim Jeans M", "Models/Pics/light_wash_jeans");
        thumbnailGallery.Add("Standard Fatigues Bottom M", "Models/Pics/standard_fatigue_pants");
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
        darylInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        darylInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        darylInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        sayreInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        sayreInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        sayreInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        hanaInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        hanaInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        hanaInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");
        jadeInventoryCatalog.Add("Saint Laurent Mask", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask");
        jadeInventoryCatalog.Add("Sport Shades", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades");
        jadeInventoryCatalog.Add("Standard Goggles", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles");

        thumbnailGallery.Add("Saint Laurent Mask", "Models/Pics/saint_laurent_mask");
        thumbnailGallery.Add("Sport Shades", "Models/Pics/sport_shades");
        thumbnailGallery.Add("Standard Goggles", "Models/Pics/standard_goggles");

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
        myTops.Add("Casual Tank Top");
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
        myFootwear.Add("White Chucks");
    }

    void collectArmor() {
        // TODO: Supposed to load from database, but for now, will hard code acquired items
        myArmor.Add("Standard Vest");
    }

}
