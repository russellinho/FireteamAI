using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    public static InventoryScript itemData;
    // Storage for weapons, equipment, and characters in the game
    public Dictionary<string, Equipment> equipmentCatalog = new Dictionary<string, Equipment>();
    public Dictionary<string, Armor> armorCatalog = new Dictionary<string, Armor>();
    public Dictionary<string, Weapon> weaponCatalog = new Dictionary<string, Weapon>();
    public Dictionary<string, Character> characterCatalog = new Dictionary<string, Character>();
    public Dictionary<string, Mod> modCatalog = new Dictionary<string, Mod>();
    public Dictionary<string, Vector3> suppressorSizesByWeapon = new Dictionary<string, Vector3>();

    // Items that are in the player inventory/items they own

    void Awake() {
        if (itemData == null)
        {
            DontDestroyOnLoad(gameObject);
            itemData = this;
        }
        else if (itemData != this)
        {
            Destroy(gameObject);
        }

        // Create all equipment data here
        equipmentCatalog.Add("Casual Shirt", new Equipment("Casual Shirt", "Top", "Models/Clothing/Lucas/Tops/Casual Shirt/lucascasualshirt", null,"Models/FirstPersonPrefabs/Tops/Lucas/Casual Shirt/male_cas_shirt", null, "Models/Pics/casual_shirt", "A classy yet casual button up.", false, 1, 0f, 0f, 0f));
        equipmentCatalog.Add("Casual T-Shirt (M)", new Equipment("Casual T-Shirt (M)", "Top", "Models/Clothing/Lucas/Tops/V Neck Tee/lucasvnecktee (1)", null, "Models/FirstPersonPrefabs/Tops/Lucas/Casual T-Shirt/male_vneck", null, "Models/Pics/v_neck_shirt", "A casual v neck t-shirt.", false, 2, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Fatigues Top (M)", new Equipment("Standard Fatigues Top (M)", "Top", "Models/Clothing/Lucas/Tops/Standard Fatigues/lucasstandardfatiguestop", null, "Models/FirstPersonPrefabs/Tops/Lucas/Standard Fatigues/male_stan_fatigues", null, "Models/Pics/standard_fatigue_shirt", "A standard issue shirt given to all solders upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Fatigues Top (F)", new Equipment("Standard Fatigues Top (F)", "Top", null, "Models/Clothing/Hana/Tops/Standard Fatigues/hanastandardfatiguestop", null, "Models/FirstPersonPrefabs/Tops/Hana/Standard Fatigues/female_stan_fatigues", "Models/Pics/standard_fatigue_shirt_f", "A standard issue shirt given to all solders upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Casual Tank Top", new Equipment("Casual Tank Top", "Top", null, "Models/Clothing/Hana/Tops/Casual Tank Top/hanatanktop", null, "Models/FirstPersonPrefabs/Tops/Hana/Casual T-Shirt/female_vneck", "Models/Pics/casual_tank_top_f", "A casual tank top.", false, 2, 0f, 0f, 0f));
        equipmentCatalog.Add("Casual T-Shirt (F)", new Equipment("Casual T-Shirt (F)", "Top", null, "Models/Clothing/Hana/Tops/Casual T-Shirt/skinhanatshirt", null, "Models/FirstPersonPrefabs/Tops/Hana/Casual T-Shirt/female_vneck", "Models/Pics/casual_t_shirt_f", "A casual t-shirt.", false, 1, 0f, 0f, 0f));
        equipmentCatalog.Add("Dark Wash Denim Jeans (M)", new Equipment("Dark Wash Denim Jeans (M)", "Bottom", "Models/Clothing/Lucas/Bottoms/Dark Wash Denim Jeans/lucasdarkwashjeans", null, "", null, "Models/Pics/dark_wash_jeans", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Light Wash Denim Jeans (M)", new Equipment("Light Wash Denim Jeans (M)", "Bottom", "Models/Clothing/Lucas/Bottoms/Light Wash Denim Jeans/lucaslightwashjeans", null, "", null, "Models/Pics/light_wash_jeans", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Fatigues Bottom (M)", new Equipment("Standard Fatigues Bottom (M)", "Bottom", "Models/Clothing/Lucas/Bottoms/Standard Fatigues/lucasstandardfatiguebottom", null, "", null, "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Dark Wash Denim Jeans (F)", new Equipment("Dark Wash Denim Jeans (F)", "Bottom", null, "Models/Clothing/Hana/Bottoms/Dark Wash Denim Jeans/hanadarkwashjeans", null, "", "Models/Pics/dark_wash_jeans_f", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Light Wash Denim Jeans (F)", new Equipment("Light Wash Denim Jeans (F)", "Bottom", null, "Models/Clothing/Hana/Bottoms/Light Wash Denim Jeans/hanalightwashjeans", null, "", "Models/Pics/light_wash_jeans_f", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Fatigues Bottom (F)", new Equipment("Standard Fatigues Bottom (F)", "Bottom", null, "Models/Clothing/Hana/Bottoms/Standard Fatigues/hanastandardfatiguesbottom", null, "", "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("MICH", new Equipment("MICH", "Headgear", "Models/Equipment/Lucas/Head/Standard Combat Helmet/lucasmich", "Models/Equipment/Hana/Head/Standard Combat Helmet/hanamich", "", null, "Models/Pics/mich", "A helmet that can be used for protecting one's head from shrapnel and even bullets.", true, 0, 0f, 0f, 0.1f));
        equipmentCatalog.Add("Combat Beanie", new Equipment("Combat Beanie", "Headgear", "Models/Equipment/Lucas/Head/Combat Beanie/lucascombatbeanie", "Models/Equipment/Hana/Head/Combat Beanie/hanabeanie", "", null, "Models/Pics/combat_beanie", "A stylish beanie straight out of your local designer clothing store.", true, 0, 0.1f, 0f, 0f));
        equipmentCatalog.Add("COM Hat", new Equipment("COM Hat", "Headgear", "Models/Equipment/Lucas/Head/COM Hat/lucascomhat", "Models/Equipment/Hana/Head/COM Hat/hanacomhat", "", null, "Models/Pics/com_hat", "A lightweight hat with a mic for optimal communication.", true, 0, 0f, 0.1f, 0f));
        equipmentCatalog.Add("Aviators", new Equipment("Aviators", "Facewear", "Models/Equipment/Lucas/Face/Saint Laurent Mask/lucassaintlaurentmask", "Models/Equipment/Hana/Face/Saint Laurent Mask/hanasaintlaurent", "", null, "Models/Pics/saint_laurent_mask", "Eliminate your enemies in style with these expensive yet stylish glasses!", false, 0, 0f, 0.05f, 0f));
        equipmentCatalog.Add("Sport Shades", new Equipment("Sport Shades", "Facewear", "Models/Equipment/Lucas/Face/Sport Shades/lucassportshades", "Models/Equipment/Hana/Face/Sport Shades/hanasportglasses", "", null, "Models/Pics/sport_shades", "Tinted shades with a sporty trim usually used for the shooting range.", false, 0, 0.05f, 0f, 0f));
        equipmentCatalog.Add("Standard Goggles", new Equipment("Standard Goggles", "Facewear", "Models/Equipment/Lucas/Face/Standard Goggles/lucasgoggles", "Models/Equipment/Hana/Face/Standard Goggles/hanagoggles", "", null, "Models/Pics/standard_goggles", "Standard issue goggles given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0.05f));
        equipmentCatalog.Add("Surgical Mask", new Equipment("Surgical Mask", "Facewear", "Models/Equipment/Lucas/Face/Surgical Mask/surgicalmask", "Models/Equipment/Hana/Face/Surgical Mask/hanasurgicalmask2", "", null, "Models/Pics/surgical_mask", "A protective, lightweight mask used during medical surgeries.", false, 0, 0.02f, 0.02f, 0.02f));
        equipmentCatalog.Add("Red Chucks", new Equipment("Red Chucks", "Footwear", "Models/Clothing/Lucas/Shoes/Chucks/lucasredchucks", null, "", null, "Models/Pics/red_chucks", "These bright canvas shoes are stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("White Chucks", new Equipment("White Chucks", "Footwear", null, "Models/Clothing/Hana/Shoes/White Chucks/hanawhitechucks", null, "", "Models/Pics/white_chucks", "The white version of the red chucks; stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Boots (M)", new Equipment("Standard Boots (M)", "Footwear", "Models/Clothing/Lucas/Shoes/Standard Boots/lucasstandardboots", null, "", null, "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Standard Boots (F)", new Equipment("Standard Boots (F)", "Footwear", null, "Models/Clothing/Hana/Shoes/Standard Boots/hanastandardboots", null, "", "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f));
        equipmentCatalog.Add("Scrubs Top", new Equipment("Scrubs Top", "Top", "Models/Clothing/Sayre/Tops/scrubstop", null, "Models/FirstPersonPrefabs/Tops/Lucas/Casual T-Shirt/male_vneck", null, "Models/Pics/scrubs_top", "A comfortable scrubs shirt commonly used in the medical field.", false, 2, 0f, 0f, 0f));
        equipmentCatalog.Add("Scrubs Bottom", new Equipment("Scrubs Bottom", "Bottom", "Models/Clothing/Sayre/Bottoms/scrubspants", null, "", null, "Models/Pics/scrubs_bottom", "A comfortable scrubs pants commonly used in the medical field.", false, 0, 0f, 0f, 0f));
        
        // Armor
        armorCatalog.Add("Standard Vest", new Armor("Standard Vest", "Models/Equipment/Lucas/Armor/Standard Vest/Tops/lucasstandardvesttop", "Models/Equipment/Lucas/Armor/Standard Vest/Bottoms/lucasstandardvestbottom", "Models/Equipment/Hana/Armor/Standard Vest/Tops/hanastandardvesttop", "Models/Equipment/Hana/Armor/Standard Vest/Bottoms/hanastandardvestbottom", "Models/Pics/standard_vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.", -0.08f, 0f, 0.2f));

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
        characterCatalog.Add("Lucas", new Character("Lucas", 'M', "Models/Characters/Lucas/PlayerPrefabLucas", "Models/FirstPersonPrefabs/Characters/Lucas/1/lucas_all_arms", "", "Models/Pics/character_lucas", "Nationality: British\nAs a reformed professional criminal, Lucas works swiftly and gets the job done.", new string[]{"Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid", "Models/Characters/Lucas/Extra Skins/Ankles Short Sleeves/lucasanklesshortsleeve"}));
        characterCatalog.Add("Daryl", new Character("Daryl", 'M', "Models/Characters/Daryl/PlayerPrefabDaryl", "Models/FirstPersonPrefabs/Characters/Daryl/1/daryl_all_arms", "", "Models/Pics/character_daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him in combat.", new string[]{"Models/Characters/Daryl/1/skindonald1", "Models/Characters/Daryl/2/skindonald2", "Models/Characters/Daryl/3/skindonald3"}));
        characterCatalog.Add("Codename Sayre", new Character("Codename Sayre", 'M', "Models/Characters/Sayre/PlayerPrefabCodenameSayre", "Models/FirstPersonPrefabs/Characters/Sayre/1/sayre_all_arms", "", "Models/Pics/character_sayre", "Nationality: Mexican\nBeing fresh out of medical school at the top of his class, Codename Sayre is skilled in his healing abilities. His wit and sense of humor allow him to maneuver through sticky situations with ease.", new string[]{"Models/Characters/Sayre/1/skinslayre1", "Models/Characters/Sayre/2/skinslayre2", "Models/Characters/Sayre/3/skinslayre3"}));
        characterCatalog.Add("Hana", new Character("Hana", 'F', "Models/Characters/Hana/PlayerPrefabHana", "Models/FirstPersonPrefabs/Characters/Hana/1/hana_all_arms", "", "Models/Pics/character_hana", "Nationality: Japanese\nWhen her entire family was murdered as a kid, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, but don't be fooled by her size.", new string[]{"Models/Characters/Hana/1/skinhana1", "Models/Characters/Hana/2/skinhana2", "Models/Characters/Hana/3/skinhana3"}));
        characterCatalog.Add("Jade", new Character("Jade", 'F', "Models/Characters/Jade/PlayerPrefabJade", "Models/FirstPersonPrefabs/Characters/Jade/1/jade_all_arms", "", "Models/Pics/character_jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.", new string[]{"Models/Characters/Jade/1/skinjade1", "Models/Characters/Jade/3/skinjade3", "Models/Characters/Jade/2/skinjade2"}));

        // Mods
        modCatalog.Add("Standard Suppressor", new Mod("Standard Suppressor", "Suppressor", "Models/Mods/Suppressors/Standard Suppressor/standardsuppressor", "Models/Pics/standardsuppressor-thumb", "A standard issue suppressor used to silence your weapon.", -3f, 2f, -4f, 0f, 0, 0));
    }

}
