using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    // Storage for weapons, equipment, and characters in the game
    public static Dictionary<string, Weapon> weaponCatalog = new Dictionary<string, Weapon>();
    public static Dictionary<string, Character> characterCatalog = new Dictionary<string, Character>();

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
        // Create all equipment data here
        Equipment casualShirtMale = new Equipment("Casual Shirt", "Top", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt", "Models/Pics/casual_shirt", "A classy yet casual button up.", false);
        Equipment casualTShirtMale = new Equipment("Casual T-Shirt", "Top", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)", "Models/Pics/v_neck_shirt", "A casual v neck t-shirt.", false);
        Equipment standardFatiguesTopMale = new Equipment("Standard Fatigues Top", "Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop", "Models/Pics/standard_fatigue_shirt", "A standard issue shirt given to all solders upon completion of basic training.", false);
        Equipment standardFatiguesTopFemale = new Equipment("Standard Fatigues Top", "Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop", "Models/Pics/standard_fatigue_shirt_f", "A standard issue shirt given to all solders upon completion of basic training.", false);
        Equipment casualTankTopFemale = new Equipment("Casual Tank Top", "Top", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop", "Models/Pics/casual_tank_top_f", "A casual tank top.", false);
        Equipment casualTShirtFemale = new Equipment("Casual T-Shirt", "Top", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)", "Models/Pics/casual_t_shirt_f", "A casual t-shirt.", false);
        Equipment darkWashDenimJeansMale = new Equipment("Dark Wash Denim Jeans", "Bottom", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans", "Models/Pics/dark_wash_jeans", "Slim fit dark wash jeans.", false);
        Equipment lightWashDenimJeansMale = new Equipment("Light Wash Denim Jeans", "Bottom", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans", "Models/Pics/light_wash_jeans", "Slim fit light wash jeans.", false);
        Equipment standardFatiguesBottomMale = new Equipment("Standard Fatigues Bottom", "Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom", "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false);
        Equipment darkWashDenimJeansFemale = new Equipment("Dark Wash Denim Jeans", "Bottom", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans", "Models/Pics/dark_wash_jeans_f", "Slim fit dark wash jeans.", false);
        Equipment lightWashDenimJeansFemale = new Equipment("Light Wash Denim Jeans", "Bottom", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanalightwashjeans", "Models/Pics/light_wash_jeans_f", "Slim fit light wash jeans.", false);
        Equipment standardFatiguesBottomFemale = new Equipment("Standard Fatigues Bottom", "Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom", "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false);
        Equipment mich = new Equipment("MICH", "Headgear", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich", "Models/Pics/mich", "A helmet that can be used for protecting one's head from shrapnel and even bullets.", true);
        Equipment combatBeanie = new Equipment("Combat Beanie", "Headgear", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie", "Models/Pics/combat_beanie", "A stylish beanie straight out of your local designer clothing store.", true);
        Equipment comHat = new Equipment("COM Hat", "Headgear", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat", "Models/Pics/com_hat", "A lightweight hat with a mic for optimal communication.", false);
        Equipment saintLaurentMask = new Equipment("Saint Laurent Mask", "Facewear", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask", "Models/Pics/saint_laurent_mask", "Eliminate your enemies in style with these expensive yet stylish glasses!", false);
        Equipment sportShades = new Equipment("Sport Shades", "Facewear", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades", "Models/Pics/sport_shades", "Tinted shades with a sporty trim usually used for the shooting range.", false);
        Equipment standardGoggles = new Equipment("Standard Goggles", "Facewear", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles", "Models/Pics/standard_goggles", "Standard issue goggles given to all soldiers upon completion of basic training.", false);
        Equipment surgicalMask = new Equipment("Surgical Mask", "Facewear", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask", "Models/Pics/surgical_mask", "A protective, lightweight mask used during medical surgeries.", false);
        Equipment redChucks = new Equipment("Red Chucks", "Footwear", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks", "Models/Pics/red_chucks", "These bright canvas shoes are stylish yet lightweight, durable, and comfortable!", false);
        Equipment whiteChucks = new Equipment("White Chucks", "Footwear", "Models/Clothing/Hana/Shoes/White Chucks/hanawhitechucks", "Models/Pics/white_chucks", "The white version of the red chucks; stylish yet lightweight, durable, and comfortable!", false);
        Equipment standardBootsMale = new Equipment("Standard Boots", "Footwear", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots", "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false);
        Equipment standardBootsFemale = new Equipment("Standard Boots", "Footwear", "Models/Clothing/Hana/Shoes/Standard Boots/hanastandardboots", "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false);
        Equipment scrubsTopMale = new Equipment("Scrubs Top", "Top", "Models/Clothing/Sayre/Tops/scrubstop", "Models/Pics/scrubs_top", "A comfortable scrubs shirt commonly used in the medical field.", false);
        Equipment scrubsBottomMale = new Equipment("Scrubs Bottom", "Bottom", "Models/Clothing/Sayre/Bottoms/scrubspants", "Models/Pics/scrubs_bottom", "A comfortable scrubs pants commonly used in the medical field.", false);
        Armor standardVest = new Armor("Standard Vest", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom", "Models/Pics/standard_vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.");
        
        Dictionary<string, Equipment> lucasEquipment = new Dictionary<string, Equipment>();
        lucasEquipment.Add("Casual Shirt", casualShirtMale);
        lucasEquipment.Add("Casual T-Shirt", casualTShirtMale);
        lucasEquipment.Add("Standard Fatigues Top", standardFatiguesTopMale);
        lucasEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansMale);
        lucasEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansMale);
        lucasEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomMale);
        lucasEquipment.Add("MICH", mich);
        lucasEquipment.Add("Combat Beanie", combatBeanie);
        lucasEquipment.Add("COM Hat", comHat);
        lucasEquipment.Add("Saint Laurent Mask", saintLaurentMask);
        lucasEquipment.Add("Sport Shades", sportShades);
        lucasEquipment.Add("Standard Goggles", standardGoggles);
        lucasEquipment.Add("Surgical Mask", surgicalMask);
        lucasEquipment.Add("Red Chucks", redChucks);
        lucasEquipment.Add("Standard Boots", standardBootsMale);

        Dictionary<string, Equipment> darylEquipment = new Dictionary<string, Equipment>();
        darylEquipment.Add("Casual Shirt", casualShirtMale);
        darylEquipment.Add("Casual T-Shirt", casualTShirtMale);
        darylEquipment.Add("Standard Fatigues Top", standardFatiguesTopMale);
        darylEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansMale);
        darylEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansMale);
        darylEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomMale);
        darylEquipment.Add("MICH", mich);
        darylEquipment.Add("Combat Beanie", combatBeanie);
        darylEquipment.Add("COM Hat", comHat);
        darylEquipment.Add("Saint Laurent Mask", saintLaurentMask);
        darylEquipment.Add("Sport Shades", sportShades);
        darylEquipment.Add("Standard Goggles", standardGoggles);
        darylEquipment.Add("Surgical Mask", surgicalMask);
        darylEquipment.Add("Red Chucks", redChucks);
        darylEquipment.Add("Standard Boots", standardBootsMale);

        Dictionary<string, Equipment> sayreEquipment = new Dictionary<string, Equipment>();
        sayreEquipment.Add("Casual Shirt", casualShirtMale);
        sayreEquipment.Add("Casual T-Shirt", casualTShirtMale);
        sayreEquipment.Add("Standard Fatigues Top", standardFatiguesTopMale);
        sayreEquipment.Add("Scrubs Top", scrubsTopMale);
        sayreEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansMale);
        sayreEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansMale);
        sayreEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomMale);
        sayreEquipment.Add("Scrubs Bottom", scrubsBottomMale);
        sayreEquipment.Add("MICH", mich);
        sayreEquipment.Add("Combat Beanie", combatBeanie);
        sayreEquipment.Add("COM Hat", comHat);
        sayreEquipment.Add("Saint Laurent Mask", saintLaurentMask);
        sayreEquipment.Add("Sport Shades", sportShades);
        sayreEquipment.Add("Standard Goggles", standardGoggles);
        sayreEquipment.Add("Surgical Mask", surgicalMask);
        sayreEquipment.Add("Red Chucks", redChucks);
        sayreEquipment.Add("Standard Boots", standardBootsMale);

        Dictionary<string, Equipment> hanaEquipment = new Dictionary<string, Equipment>();
        hanaEquipment.Add("Casual Tank Top", casualTankTopFemale);
        hanaEquipment.Add("Casual T-Shirt", casualTShirtFemale);
        hanaEquipment.Add("Standard Fatigues Top", standardFatiguesTopFemale);
        hanaEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansFemale);
        hanaEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansFemale);
        hanaEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomFemale);
        hanaEquipment.Add("MICH", mich);
        hanaEquipment.Add("Combat Beanie", combatBeanie);
        hanaEquipment.Add("COM Hat", comHat);
        hanaEquipment.Add("Saint Laurent Mask", saintLaurentMask);
        hanaEquipment.Add("Sport Shades", sportShades);
        hanaEquipment.Add("Standard Goggles", standardGoggles);
        hanaEquipment.Add("Surgical Mask", surgicalMask);
        hanaEquipment.Add("White Chucks", whiteChucks);
        hanaEquipment.Add("Standard Boots", standardBootsFemale);
        
        Dictionary<string, Equipment> jadeEquipment = new Dictionary<string, Equipment>();
        jadeEquipment.Add("Casual Tank Top", casualTankTopFemale);
        jadeEquipment.Add("Casual T-Shirt", casualTShirtFemale);
        jadeEquipment.Add("Standard Fatigues Top", standardFatiguesTopFemale);
        jadeEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansFemale);
        jadeEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansFemale);
        jadeEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomFemale);
        jadeEquipment.Add("MICH", mich);
        jadeEquipment.Add("Combat Beanie", combatBeanie);
        jadeEquipment.Add("COM Hat", comHat);
        jadeEquipment.Add("Saint Laurent Mask", saintLaurentMask);
        jadeEquipment.Add("Sport Shades", sportShades);
        jadeEquipment.Add("Standard Goggles", standardGoggles);
        jadeEquipment.Add("Surgical Mask", surgicalMask);
        jadeEquipment.Add("White Chucks", whiteChucks);
        jadeEquipment.Add("Standard Boots", standardBootsFemale);

        // Armor
        Dictionary<string, Armor> lucasArmor = new Dictionary<string, Armor>();
        lucasArmor.Add("Standard Vest", standardVest);

        Dictionary<string, Armor> hanaArmor = new Dictionary<string, Armor>();
        hanaArmor.Add("Standard Vest", standardVest);

        // Weapons
        weaponCatalog.Add("AK-47", new Weapon("AK-47", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/AK-47", "Models/Pics/ak47-thumb", "A classic assault rifle developed in the Soviet Union during the World War II era. Known for its unmatched stopping power and relatively light weight."));
        weaponCatalog.Add("Glock23", new Weapon("Glock23", "Secondary", "Pistol", "Models/Weapons/Secondary/Pistols/Glock23", "Models/Pics/glock23-thumb", "The standard pistol used by United States police officers because of its reliability."));
        weaponCatalog.Add("R870", new Weapon("R870", "Primary", "Shotgun","Models/Weapons/Primary/Shotguns/R870", "Models/Pics/r870-thumb", "Short for Remington 870, this shotgun is widely used for home defence due to its quick reload speeds and reliability."));
        weaponCatalog.Add("L96A1", new Weapon("L96A1", "Primary", "Sniper Rifle", "Models/Weapons/Primary/Sniper Rifles/L96A1", "Models/Pics/l96a1-thumb", "Developed in the 1980s by the British, this bolt-action sniper rifle is known for its deadly stopping power and quick operation speed."));
        weaponCatalog.Add("M4A1", new Weapon("M4A1", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/M4A1", "Models/Pics/m4a1-thumb", "As a successor of the M16A3 assault rifle, this weapon is one of the standard issue rifles in the United States military. It's known for being an all around ass kicker."));

        // Characters
        characterCatalog.Add("Lucas", new Character("Lucas", 'M', "Models/Characters/Lucas/PlayerPrefabLucas", "Models/Pics/character_lucas", "Nationality: British\nAs a reformed professional criminal, Lucas works swiftly and gets the job done.", new string[]{"Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop"}, lucasEquipment, lucasArmor));
        characterCatalog.Add("Daryl", new Character("Daryl", 'M', "Models/Characters/Daryl/PlayerPrefabDaryl", "Models/Pics/character_daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him in combat.", new string[]{"Models/Characters/Daryl/1/skindonald1", "Models/Characters/Daryl/2/skindonald2", "Models/Characters/Daryl/3/skindonald3"}, darylEquipment, lucasArmor));
        characterCatalog.Add("Codename Sayre", new Character("Codename Sayre", 'M', "Models/Characters/Sayre/PlayerPrefabCodenameSayre", "Models/Pics/character_sayre", "Nationality: Mexican\nBeing fresh out of medical school at the top of his class, Codename Sayre is skilled in his healing abilities. His witty style of humor allows him to maneuver through sticky situations easily.", new string[]{"Models/Characters/Sayre/1/skinslayre1", "Models/Characters/Sayre/2/skinslayre2", "Models/Characters/Sayre/3/skinslayre3"}, sayreEquipment, lucasArmor));
        characterCatalog.Add("Hana", new Character("Hana", 'F', "Models/Characters/Hana/PlayerPrefabHana", "Models/Pics/character_hana", "Nationality: Japanese\nWhen her entire family was murdered as a kid, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, but don't be fooled by her size.", new string[]{"Models/Characters/Hana/1/skinhana1", "Models/Characters/Hana/2/skinhana2", "Models/Characters/Hana/3/skinhana3"}, hanaEquipment, lucasArmor));
        characterCatalog.Add("Jade", new Character("Jade", 'F', "Models/Characters/Jade/PlayerPrefabJade", "Models/Pics/character_jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.", new string[]{"Models/Characters/Jade/1/skinjade1", "Models/Characters/Jade/2/skinjade2", "Models/Characters/Jade/3/skinjade3"}, jadeEquipment, lucasArmor));

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
            if (character.Equals("Codename Sayre")) {
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
            if (character.Equals("Codename Sayre")) {
                myBottoms.Add("Scrubs Bottom");
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
