using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    // Storage for weapons, equipment, and characters in the game
    public static Dictionary<string, Weapon> weaponCatalog = new Dictionary<string, Weapon>();
    public static Dictionary<string, Character> characterCatalog = new Dictionary<string, Character>();
    public static Dictionary<string, Mod> modCatalog = new Dictionary<string, Mod>();
    public static Dictionary<string, Vector3> suppressorSizesByWeapon = new Dictionary<string, Vector3>();

    // Items that are in the player inventory/items they own
    public static ArrayList myCharacters = new ArrayList();
    public static ArrayList myTops = new ArrayList();
    public static ArrayList myBottoms = new ArrayList();
    public static ArrayList myHeadgear = new ArrayList();
    public static ArrayList myFacewear = new ArrayList();
    public static ArrayList myFootwear = new ArrayList();
    public static ArrayList myArmor = new ArrayList();
    public static ArrayList myWeapons = new ArrayList();
    public static ArrayList myMods = new ArrayList();
    public static Dictionary<string, Dictionary<string, Vector3>> rifleHandPositionsPerCharacter;
    public static Dictionary<string, Dictionary<string, Vector3>> rifleIdleHandPositionsPerCharacter;
    public static Dictionary<string, Dictionary<string, Vector3>> shotgunHandPositionsPerCharacter;
    public static Dictionary<string, Dictionary<string, Vector3>> shotgunIdleHandPositionsPerCharacter;
    public static Dictionary<string, Dictionary<string, Vector3>> sniperRifleHandPositionsPerCharacter;
    public static Dictionary<string, Dictionary<string, Vector3>> sniperRifleIdleHandPositionsPerCharacter;

    void Awake() {
        if (weaponCatalog.Count != 0) {
            return;
        }
        // Create all equipment data here
        Equipment casualShirtMale = new Equipment("Casual Shirt", "Top", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt", "Models/FirstPersonPrefabs/Tops/Lucas/Casual Shirt/male_cas_shirt", "Models/Pics/casual_shirt", "A classy yet casual button up.", false, 1, 0f, 0f, 0f);
        Equipment casualTShirtMale = new Equipment("Casual T-Shirt", "Top", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)", "Models/FirstPersonPrefabs/Tops/Lucas/Casual T-Shirt/male_vneck", "Models/Pics/v_neck_shirt", "A casual v neck t-shirt.", false, 2, 0f, 0f, 0f);
        Equipment standardFatiguesTopMale = new Equipment("Standard Fatigues Top", "Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop", "", "Models/Pics/standard_fatigue_shirt", "A standard issue shirt given to all solders upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment standardFatiguesTopFemale = new Equipment("Standard Fatigues Top", "Top", "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop", "", "Models/Pics/standard_fatigue_shirt_f", "A standard issue shirt given to all solders upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment casualTankTopFemale = new Equipment("Casual Tank Top", "Top", "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop", "", "Models/Pics/casual_tank_top_f", "A casual tank top.", false, 2, 0f, 0f, 0f);
        Equipment casualTShirtFemale = new Equipment("Casual T-Shirt", "Top", "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt", "Models/FirstPersonPrefabs/Tops/Hana/Casual T-Shirt/female_vneck", "Models/Pics/casual_t_shirt_f", "A casual t-shirt.", false, 1, 0f, 0f, 0f);
        Equipment darkWashDenimJeansMale = new Equipment("Dark Wash Denim Jeans", "Bottom", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans", "", "Models/Pics/dark_wash_jeans", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f);
        Equipment lightWashDenimJeansMale = new Equipment("Light Wash Denim Jeans", "Bottom", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans", "", "Models/Pics/light_wash_jeans", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f);
        Equipment standardFatiguesBottomMale = new Equipment("Standard Fatigues Bottom", "Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom", "", "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment darkWashDenimJeansFemale = new Equipment("Dark Wash Denim Jeans", "Bottom", "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans", "", "Models/Pics/dark_wash_jeans_f", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f);
        Equipment lightWashDenimJeansFemale = new Equipment("Light Wash Denim Jeans", "Bottom", "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans", "", "Models/Pics/light_wash_jeans_f", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f);
        Equipment standardFatiguesBottomFemale = new Equipment("Standard Fatigues Bottom", "Bottom", "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom", "", "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment michMale = new Equipment("MICH", "Headgear", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich", "", "Models/Pics/mich", "A helmet that can be used for protecting one's head from shrapnel and even bullets.", true, 0, 0f, 0f, 0.1f);
        Equipment michFemale = new Equipment("MICH", "Headgear", "Models/Equipment/Hana/Head/Standard Combat Helmet/hanamich", "", "Models/Pics/mich", "A helmet that can be used for protecting one's head from shrapnel and even bullets.", true, 0, 0f, 0f, 0.1f);
        Equipment combatBeanieMale = new Equipment("Combat Beanie", "Headgear", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie", "", "Models/Pics/combat_beanie", "A stylish beanie straight out of your local designer clothing store.", true, 0, 0.1f, 0f, 0f);
        Equipment combatBeanieFemale = new Equipment("Combat Beanie", "Headgear", "Models/Equipment/Hana/Head/Combat Beanie/hanabeanie", "", "Models/Pics/combat_beanie", "A stylish beanie straight out of your local designer clothing store.", true, 0, 0.1f, 0f, 0f);
        Equipment comHatMale = new Equipment("COM Hat", "Headgear", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat", "", "Models/Pics/com_hat", "A lightweight hat with a mic for optimal communication.", true, 0, 0f, 0.1f, 0f);
        Equipment comHatFemale = new Equipment("COM Hat", "Headgear", "Models/Equipment/Hana/Head/COM Hat/hanacomhat", "", "Models/Pics/com_hat", "A lightweight hat with a mic for optimal communication.", true, 0, 0f, 0.1f, 0f);
        Equipment saintLaurentMaskMale = new Equipment("Saint Laurent Mask", "Facewear", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask", "", "Models/Pics/saint_laurent_mask", "Eliminate your enemies in style with these expensive yet stylish glasses!", false, 0, 0f, 0.05f, 0f);
        Equipment saintLaurentMaskFemale = new Equipment("Saint Laurent Mask", "Facewear", "Models/Equipment/Hana/Face/Saint Laurent Mask/hanasaintlaurent", "", "Models/Pics/saint_laurent_mask", "Eliminate your enemies in style with these expensive yet stylish glasses!", false, 0, 0f, 0.05f, 0f);
        Equipment sportShadesMale = new Equipment("Sport Shades", "Facewear", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades", "", "Models/Pics/sport_shades", "Tinted shades with a sporty trim usually used for the shooting range.", false, 0, 0.05f, 0f, 0f);
        Equipment sportShadesFemale = new Equipment("Sport Shades", "Facewear", "Models/Equipment/Hana/Face/Sport Shades/hanasportglasses", "", "Models/Pics/sport_shades", "Tinted shades with a sporty trim usually used for the shooting range.", false, 0, 0.05f, 0f, 0f);
        Equipment standardGogglesMale = new Equipment("Standard Goggles", "Facewear", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles", "", "Models/Pics/standard_goggles", "Standard issue goggles given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0.05f);
        Equipment standardGogglesFemale = new Equipment("Standard Goggles", "Facewear", "Models/Equipment/Hana/Face/Standard Goggles/hanagoggles", "", "Models/Pics/standard_goggles", "Standard issue goggles given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0.05f);
        Equipment surgicalMaskMale = new Equipment("Surgical Mask", "Facewear", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask", "", "Models/Pics/surgical_mask", "A protective, lightweight mask used during medical surgeries.", false, 0, 0.02f, 0.02f, 0.02f);
        Equipment surgicalMaskFemale = new Equipment("Surgical Mask", "Facewear", "Models/Equipment/Hana/Face/Surgical Mask/hanasurgicalmask2", "", "Models/Pics/surgical_mask", "A protective, lightweight mask used during medical surgeries.", false, 0, 0.02f, 0.02f, 0.02f);
        Equipment redChucks = new Equipment("Red Chucks", "Footwear", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks", "", "Models/Pics/red_chucks", "These bright canvas shoes are stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f);
        Equipment whiteChucks = new Equipment("White Chucks", "Footwear", "Models/Clothing/Hana/Shoes/White Chucks/hanawhitechucks", "", "Models/Pics/white_chucks", "The white version of the red chucks; stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f);
        Equipment standardBootsMale = new Equipment("Standard Boots", "Footwear", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots", "", "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment standardBootsFemale = new Equipment("Standard Boots", "Footwear", "Models/Clothing/Hana/Shoes/Standard Boots/hanastandardboots", "", "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f);
        Equipment scrubsTopMale = new Equipment("Scrubs Top", "Top", "Models/Clothing/Sayre/Tops/scrubstop", "", "Models/Pics/scrubs_top", "A comfortable scrubs shirt commonly used in the medical field.", false, 2, 0f, 0f, 0f);
        Equipment scrubsBottomMale = new Equipment("Scrubs Bottom", "Bottom", "Models/Clothing/Sayre/Bottoms/scrubspants", "", "Models/Pics/scrubs_bottom", "A comfortable scrubs pants commonly used in the medical field.", false, 0, 0f, 0f, 0f);
        Armor standardVestMale = new Armor("Standard Vest", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom", "Models/Pics/standard_vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.", -0.08f, 0f, 0.2f);
        Armor standardVestFemale = new Armor("Standard Vest", "Models/Equipment/Hana/Armor/Standard Vest/Tops/hanastandardvesttop", "Models/Equipment/Hana/Armor/Standard Vest/Bottoms/hanastandardvestbottom", "Models/Pics/standard_vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.", -0.08f, 0f, 0.2f);
        Mod standardSuppressor = new Mod("Standard Suppressor", "Suppressor", "Models/Mods/Suppressors/Standard Suppressor/standardsuppressor", "Models/Pics/standardsuppressor-thumb", "A standard issue suppressor used to silence your weapon.", -3f, 2f, -4f, 0f, 0, 0);

        Dictionary<string, Equipment> lucasEquipment = new Dictionary<string, Equipment>();
        lucasEquipment.Add("Casual Shirt", casualShirtMale);
        lucasEquipment.Add("Casual T-Shirt", casualTShirtMale);
        lucasEquipment.Add("Standard Fatigues Top", standardFatiguesTopMale);
        lucasEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansMale);
        lucasEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansMale);
        lucasEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomMale);
        lucasEquipment.Add("MICH", michMale);
        lucasEquipment.Add("Combat Beanie", combatBeanieMale);
        lucasEquipment.Add("COM Hat", comHatMale);
        lucasEquipment.Add("Saint Laurent Mask", saintLaurentMaskMale);
        lucasEquipment.Add("Sport Shades", sportShadesMale);
        lucasEquipment.Add("Standard Goggles", standardGogglesMale);
        lucasEquipment.Add("Surgical Mask", surgicalMaskMale);
        lucasEquipment.Add("Red Chucks", redChucks);
        lucasEquipment.Add("Standard Boots", standardBootsMale);

        Dictionary<string, Equipment> darylEquipment = new Dictionary<string, Equipment>();
        darylEquipment.Add("Casual Shirt", casualShirtMale);
        darylEquipment.Add("Casual T-Shirt", casualTShirtMale);
        darylEquipment.Add("Standard Fatigues Top", standardFatiguesTopMale);
        darylEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansMale);
        darylEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansMale);
        darylEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomMale);
        darylEquipment.Add("MICH", michMale);
        darylEquipment.Add("Combat Beanie", combatBeanieMale);
        darylEquipment.Add("COM Hat", comHatMale);
        darylEquipment.Add("Saint Laurent Mask", saintLaurentMaskMale);
        darylEquipment.Add("Sport Shades", sportShadesMale);
        darylEquipment.Add("Standard Goggles", standardGogglesMale);
        darylEquipment.Add("Surgical Mask", surgicalMaskMale);
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
        sayreEquipment.Add("MICH", michMale);
        sayreEquipment.Add("Combat Beanie", combatBeanieMale);
        sayreEquipment.Add("COM Hat", comHatMale);
        sayreEquipment.Add("Saint Laurent Mask", saintLaurentMaskMale);
        sayreEquipment.Add("Sport Shades", sportShadesMale);
        sayreEquipment.Add("Standard Goggles", standardGogglesMale);
        sayreEquipment.Add("Surgical Mask", surgicalMaskMale);
        sayreEquipment.Add("Red Chucks", redChucks);
        sayreEquipment.Add("Standard Boots", standardBootsMale);

        Dictionary<string, Equipment> hanaEquipment = new Dictionary<string, Equipment>();
        hanaEquipment.Add("Casual Tank Top", casualTankTopFemale);
        hanaEquipment.Add("Casual T-Shirt", casualTShirtFemale);
        hanaEquipment.Add("Standard Fatigues Top", standardFatiguesTopFemale);
        hanaEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansFemale);
        hanaEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansFemale);
        hanaEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomFemale);
        hanaEquipment.Add("MICH", michFemale);
        hanaEquipment.Add("Combat Beanie", combatBeanieFemale);
        hanaEquipment.Add("COM Hat", comHatFemale);
        hanaEquipment.Add("Saint Laurent Mask", saintLaurentMaskFemale);
        hanaEquipment.Add("Sport Shades", sportShadesFemale);
        hanaEquipment.Add("Standard Goggles", standardGogglesFemale);
        hanaEquipment.Add("Surgical Mask", surgicalMaskFemale);
        hanaEquipment.Add("White Chucks", whiteChucks);
        hanaEquipment.Add("Standard Boots", standardBootsFemale);

        Dictionary<string, Equipment> jadeEquipment = new Dictionary<string, Equipment>();
        jadeEquipment.Add("Casual Tank Top", casualTankTopFemale);
        jadeEquipment.Add("Casual T-Shirt", casualTShirtFemale);
        jadeEquipment.Add("Standard Fatigues Top", standardFatiguesTopFemale);
        jadeEquipment.Add("Dark Wash Denim Jeans", darkWashDenimJeansFemale);
        jadeEquipment.Add("Light Wash Denim Jeans", lightWashDenimJeansFemale);
        jadeEquipment.Add("Standard Fatigues Bottom", standardFatiguesBottomFemale);
        jadeEquipment.Add("MICH", michFemale);
        jadeEquipment.Add("Combat Beanie", combatBeanieFemale);
        jadeEquipment.Add("COM Hat", comHatFemale);
        jadeEquipment.Add("Saint Laurent Mask", saintLaurentMaskFemale);
        jadeEquipment.Add("Sport Shades", sportShadesFemale);
        jadeEquipment.Add("Standard Goggles", standardGogglesFemale);
        jadeEquipment.Add("Surgical Mask", surgicalMaskFemale);
        jadeEquipment.Add("White Chucks", whiteChucks);
        jadeEquipment.Add("Standard Boots", standardBootsFemale);

        // Armor
        Dictionary<string, Armor> lucasArmor = new Dictionary<string, Armor>();
        lucasArmor.Add("Standard Vest", standardVestMale);

        Dictionary<string, Armor> hanaArmor = new Dictionary<string, Armor>();
        hanaArmor.Add("Standard Vest", standardVestFemale);

        // Weapons
        weaponCatalog.Add("AK-47", new Weapon("AK-47", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/AK-47", "Models/Pics/ak47-thumb", "A classic assault rifle developed in the Soviet Union during the World War II era. It's known for its unmatched stopping power and relatively light weight.", 48f, 90f, 68f, 90f, 70f, 3000f, 30, 120, true, true));
        weaponCatalog.Add("Glock23", new Weapon("Glock23", "Secondary", "Pistol", "Models/Weapons/Secondary/Pistols/Glock23", "Models/Pics/glock23-thumb", "The standard pistol used by United States police officers because of its reliability.", 33f, 100f, 45f, 60f, 56f, 3000f, 12, 60, true, true));
        weaponCatalog.Add("R870", new Weapon("R870", "Primary", "Shotgun","Models/Weapons/Primary/Shotguns/R870", "Models/Pics/r870-thumb", "Short for Remington 870, this shotgun is widely used for home defence due to its quick reload speed and reliability.", 75f, 95f, 17f, -1f, 60f, 1000f, 8, 56, true, false));
        weaponCatalog.Add("L96A1", new Weapon("L96A1", "Primary", "Sniper Rifle", "Models/Weapons/Primary/Sniper Rifles/L96A1", "Models/Pics/l96a1-thumb", "Developed in the 1980s by the British, this bolt-action sniper rifle is known for its deadly stopping power and quick operation speed.", 100f, 40f, 10f, 90f, 65f, 3000f, 5, 20, true, true));
        weaponCatalog.Add("M4A1", new Weapon("M4A1", "Primary", "Assault Rifle", "Models/Weapons/Primary/Assault Rifles/M4A1", "Models/Pics/m4a1-thumb", "As a successor of the M16A3 assault rifle, this weapon is one of the standard issue rifles in the United States military. It's known for being an all around ass kicker.", 38f, 92f, 74f, 80f, 64f, 3000f, 30, 120, true, true));
        weaponCatalog.Add("M67 Frag", new Weapon("M67 Frag", "Support", "Explosive", "Models/Weapons/Support/Explosives/M67 Frag/M67Frag", "Models/Pics/m67frag-thumb", "The standard issue high explosive anti-personnel grenade given to all mercenaries upon completion of basic training.", 135f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false));
        weaponCatalog.Add("XM84 Flashbang", new Weapon("XM84 Flashbang", "Support", "Explosive", "Models/Weapons/Support/Explosives/XM84 Flashbang/XM84Flash", "Models/Pics/xm84flash-thumb", "An explosive non-lethal device used to temporarily blind and disorient your enemies. The closer the enemy is and the more eye exposure given to the device, the longer the effect.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false));
        weaponCatalog.Add("Medkit", new Weapon("Medkit", "Support", "Booster", "Models/Weapons/Support/Boosters/Medkit/Medkit", "Models/Pics/medkit-thumb", "Emits a chemical into your body that expedites the coagulation and production of red blood cells. Replenishes 60 HP.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false));
        weaponCatalog.Add("Adrenaphine", new Weapon("Adrenaphine", "Support", "Booster", "Models/Weapons/Support/Boosters/Adrenaline Shot/AdrenalineShot", "Models/Pics/adrenalineshot-thumb", "Injects pure adrenaline straight into your blood stream, allowing you to experience unlimited stamina and faster movement speed for 10 seconds.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false));

        // Characters
        characterCatalog.Add("Lucas", new Character("Lucas", 'M', "Models/Characters/Lucas/PlayerPrefabLucas", "Models/FirstPersonPrefabs/Characters/Lucas/1/lucas_all_arms", "", "Models/Pics/character_lucas", "Nationality: British\nAs a reformed professional criminal, Lucas works swiftly and gets the job done.", new string[]{"Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid", "Models/Characters/Lucas/Extra Skins/Ankles Short Sleeves/lucasanklesshortsleeve"}, lucasEquipment, lucasArmor));
        characterCatalog.Add("Daryl", new Character("Daryl", 'M', "Models/Characters/Daryl/PlayerPrefabDaryl", "Models/FirstPersonPrefabs/Characters/Daryl/1/daryl_all_arms", "", "Models/Pics/character_daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him in combat.", new string[]{"Models/Characters/Daryl/1/skindonald1", "Models/Characters/Daryl/2/skindonald2", "Models/Characters/Daryl/3/skindonald3"}, darylEquipment, lucasArmor));
        characterCatalog.Add("Codename Sayre", new Character("Codename Sayre", 'M', "Models/Characters/Sayre/PlayerPrefabCodenameSayre", "Models/FirstPersonPrefabs/Characters/Sayre/1/sayre_all_arms", "", "Models/Pics/character_sayre", "Nationality: Mexican\nBeing fresh out of medical school at the top of his class, Codename Sayre is skilled in his healing abilities. His witty style of humor allows him to maneuver through sticky situations easily.", new string[]{"Models/Characters/Sayre/1/skinslayre1", "Models/Characters/Sayre/2/skinslayre2", "Models/Characters/Sayre/3/skinslayre3"}, sayreEquipment, lucasArmor));
        characterCatalog.Add("Hana", new Character("Hana", 'F', "Models/Characters/Hana/PlayerPrefabHana", "Models/FirstPersonPrefabs/Characters/Hana/1/hana_all_arms", "", "Models/Pics/character_hana", "Nationality: Japanese\nWhen her entire family was murdered as a kid, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, but don't be fooled by her size.", new string[]{"Models/Characters/Hana/1/skinhana1", "Models/Characters/Hana/2/skinhana2", "Models/Characters/Hana/3/skinhana3"}, hanaEquipment, hanaArmor));
        characterCatalog.Add("Jade", new Character("Jade", 'F', "Models/Characters/Jade/PlayerPrefabJade", "Models/FirstPersonPrefabs/Characters/Jade/1/jade_all_arms", "", "Models/Pics/character_jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.", new string[]{"Models/Characters/Jade/1/skinjade1", "Models/Characters/Jade/3/skinjade3", "Models/Characters/Jade/2/skinjade2"}, jadeEquipment, hanaArmor));

        // Mods
        modCatalog.Add("Standard Suppressor", standardSuppressor);

        // Add weapon hand positions
        Dictionary<string, Vector3> rifleHandPositions = new Dictionary<string, Vector3>();
        rifleHandPositions.Add("AK-47", new Vector3(-0.04f, 0.12f, 0.075f));
        rifleHandPositions.Add("M4A1", new Vector3(-0.007f, 0.111f, 0.04f));

        Dictionary<string, Vector3> shotgunHandPositions = new Dictionary<string, Vector3>();
        shotgunHandPositions.Add("R870", new Vector3(-0.071f, 0.15f, 0.11f));

        Dictionary<string, Vector3> sniperRifleHandPositions = new Dictionary<string, Vector3>();
        sniperRifleHandPositions.Add("L96A1", new Vector3(0.004f, 0.1f, 0.029f));

        Dictionary<string, Vector3> rifleHandPositionsF = new Dictionary<string, Vector3>();
        rifleHandPositionsF.Add("AK-47", new Vector3(-0.1f, 0.14f, 0.04f));
        rifleHandPositionsF.Add("M4A1", new Vector3(-0.06f, 0.12f, -0.01f));

        Dictionary<string, Vector3> shotgunHandPositionsF = new Dictionary<string, Vector3>();
        shotgunHandPositionsF.Add("R870", new Vector3(-0.13f, 0.15f, 0.084f));

        Dictionary<string, Vector3> sniperRifleHandPositionsF = new Dictionary<string, Vector3>();
        sniperRifleHandPositionsF.Add("L96A1", new Vector3(0.004f, 0.1f, 0.029f));

        rifleHandPositionsPerCharacter = new Dictionary<string, Dictionary<string, Vector3>>();
        rifleHandPositionsPerCharacter.Add("Lucas", rifleHandPositions);
        rifleHandPositionsPerCharacter.Add("Daryl", rifleHandPositions);
        rifleHandPositionsPerCharacter.Add("Codename Sayre", rifleHandPositions);
        rifleHandPositionsPerCharacter.Add("Hana", rifleHandPositionsF);
        rifleHandPositionsPerCharacter.Add("Jade", rifleHandPositionsF);

        shotgunHandPositionsPerCharacter = new Dictionary<string, Dictionary<string, Vector3>>();
        shotgunHandPositionsPerCharacter.Add("Lucas", shotgunHandPositions);
        shotgunHandPositionsPerCharacter.Add("Daryl", shotgunHandPositions);
        shotgunHandPositionsPerCharacter.Add("Codename Sayre", shotgunHandPositions);
        shotgunHandPositionsPerCharacter.Add("Hana", shotgunHandPositionsF);
        shotgunHandPositionsPerCharacter.Add("Jade", shotgunHandPositionsF);

        sniperRifleHandPositionsPerCharacter = new Dictionary<string, Dictionary<string, Vector3>>();
        sniperRifleHandPositionsPerCharacter.Add("Lucas", sniperRifleHandPositions);
        sniperRifleHandPositionsPerCharacter.Add("Daryl", sniperRifleHandPositions);
        sniperRifleHandPositionsPerCharacter.Add("Codename Sayre", sniperRifleHandPositions);
        sniperRifleHandPositionsPerCharacter.Add("Hana", sniperRifleHandPositionsF);
        sniperRifleHandPositionsPerCharacter.Add("Jade", sniperRifleHandPositionsF);

        collectCharacters();
        collectWeapons();
        collectMods();
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
        myWeapons.Add("M67 Frag");
        myWeapons.Add("XM84 Flashbang");
        myWeapons.Add("Medkit");
        myWeapons.Add("Adrenaphine");
        myWeapons.Add("L96A1");
    }

    public static void collectMods() {
        myMods.Add("Standard Suppressor");
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
