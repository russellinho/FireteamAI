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
    public GameObject[] itemReferences;

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
        equipmentCatalog.Add("Casual Shirt", new Equipment("Casual Shirt", "Top", 0, -1, 69, -1, "Models/Pics/casual_shirt", "A classy yet casual button up.", false, 1, 0f, 0f, 0f, 'M', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Casual T-Shirt (M)", new Equipment("Casual T-Shirt (M)", "Top", 1, -1, 70, -1, "Models/Pics/v_neck_shirt", "A casual v neck t-shirt.", false, 2, 0f, 0f, 0f, 'M', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Standard Fatigues Top (M)", new Equipment("Standard Fatigues Top (M)", "Top", 2, -1, 71, -1, "Models/Pics/standard_fatigue_shirt", "A standard issue shirt given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("Standard Fatigues Top (F)", new Equipment("Standard Fatigues Top (F)", "Top", -1, 3, -1, 68, "Models/Pics/standard_fatigue_shirt_f", "A standard issue shirt given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("Casual Tank Top", new Equipment("Casual Tank Top", "Top", -1, 4, -1, 67, "Models/Pics/casual_tank_top_f", "A casual tank top.", false, 2, 0f, 0f, 0f, 'F', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Casual T-Shirt (F)", new Equipment("Casual T-Shirt (F)", "Top", -1, 5, -1, 67, "Models/Pics/casual_t_shirt_f", "A casual t-shirt.", false, 1, 0f, 0f, 0f, 'F', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Dark Wash Denim Jeans (M)", new Equipment("Dark Wash Denim Jeans (M)", "Bottom", 6, -1, -1, -1, "Models/Pics/dark_wash_jeans", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Light Wash Denim Jeans (M)", new Equipment("Light Wash Denim Jeans (M)", "Bottom", 7, -1, -1, -1, "Models/Pics/light_wash_jeans", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Standard Fatigues Bottom (M)", new Equipment("Standard Fatigues Bottom (M)", "Bottom", 8, -1, -1, -1, "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("Dark Wash Denim Jeans (F)", new Equipment("Dark Wash Denim Jeans (F)", "Bottom", -1, 9, -1, -1, "Models/Pics/dark_wash_jeans_f", "Slim fit dark wash jeans.", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Light Wash Denim Jeans (F)", new Equipment("Light Wash Denim Jeans (F)", "Bottom", -1, 10, -1, -1, "Models/Pics/light_wash_jeans_f", "Slim fit light wash jeans.", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Standard Fatigues Bottom (F)", new Equipment("Standard Fatigues Bottom (F)", "Bottom", -1, 11, -1, -1, "Models/Pics/standard_fatigue_pants", "A standard issue pants given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("MICH", new Equipment("MICH", "Headgear", 15, 14, -1, -1, "Models/Pics/mich", "A helmet that can be used for protecting one's head from shrapnel and even bullets.", true, 0, 0f, 0f, 0.1f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Combat Beanie", new Equipment("Combat Beanie", "Headgear", 89, 16, -1, -1, "Models/Pics/combat_beanie", "A stylish beanie straight out of your local designer clothing store.", true, 0, 0.1f, 0f, 0f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("COM Hat", new Equipment("COM Hat", "Headgear", 13, 12, -1, -1, "Models/Pics/com_hat", "A lightweight hat with a mic for optimal communication.", true, 0, 0f, 0.1f, 0f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Aviators", new Equipment("Aviators", "Facewear", 18, 17, -1, -1, "Models/Pics/saint_laurent_mask", "Eliminate your enemies in style with these expensive yet stylish glasses!", false, 0, 0f, 0.05f, 0f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Sport Shades", new Equipment("Sport Shades", "Facewear", 20, 19, -1, -1, "Models/Pics/sport_shades", "Tinted shades with a sporty trim usually used for the shooting range.", false, 0, 0.05f, 0f, 0f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Standard Goggles", new Equipment("Standard Goggles", "Facewear", 22, 21, -1, -1, "Models/Pics/standard_goggles", "Standard issue goggles given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0.05f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Surgical Mask", new Equipment("Surgical Mask", "Facewear", 24, 23, -1, -1, "Models/Pics/surgical_mask", "A protective, lightweight mask used during medical surgeries.", false, 0, 0.02f, 0.02f, 0.02f, 'N', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Red Chucks", new Equipment("Red Chucks", "Footwear", 31, -1, -1, -1, "Models/Pics/red_chucks", "These bright canvas shoes are stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("White Chucks", new Equipment("White Chucks", "Footwear", -1, 30, -1, -1, "Models/Pics/white_chucks", "The white version of the red chucks; stylish yet lightweight, durable, and comfortable!", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, true, true));
        equipmentCatalog.Add("Standard Boots (M)", new Equipment("Standard Boots (M)", "Footwear", 32, -1, -1, -1, "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'M', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("Standard Boots (F)", new Equipment("Standard Boots (F)", "Footwear", -1, 29, -1, -1, "Models/Pics/standard_boots", "Standard issue combat boots given to all soldiers upon completion of basic training.", false, 0, 0f, 0f, 0f, 'F', new string[0]{}, 100, false, false));
        equipmentCatalog.Add("Scrubs Top", new Equipment("Scrubs Top", "Top", 34, -1, 70, -1, "Models/Pics/scrubs_top", "A comfortable scrubs shirt commonly used in the medical field.", false, 2, 0f, 0f, 0f, 'M', new string[1]{"Codename Sayre"}, 100, false, true));
        equipmentCatalog.Add("Scrubs Bottom", new Equipment("Scrubs Bottom", "Bottom", 33, -1, -1, -1, "Models/Pics/scrubs_bottom", "A comfortable scrubs pants commonly used in the medical field.", false, 0, 0f, 0f, 0f, 'M', new string[1]{"Codename Sayre"}, 100, false, true));
        
        // Armor
        armorCatalog.Add("Standard Vest", new Armor("Standard Vest", 28, 27, 26, 25, "Models/Pics/standard_vest", "A first generation ballistic vest used to protect yourself in combat. Being first generation, it's a bit heavy, but offers great protection.", -0.08f, 0f, 0.2f, 100, true));

        // Weapons
        weaponCatalog.Add("AK-47", new Weapon("AK-47", "Primary", "Assault Rifle", 75, null, "Models/Pics/ak47-thumb", "A classic assault rifle developed in the Soviet Union during the World War II era. It's known for its unmatched stopping power and relatively light weight.", 65f, 90f, 68f, 90f, 70f, 3000f, 30, 120, true, true, true, 100, true, true));
        weaponCatalog.Add("MP5A3", new Weapon("MP5A3", "Primary", "SMG", 79, null, "Models/Pics/mp5a3-thumb", "The world renowned German engineered masterpiece of submachine guns, the MP5A3 is a worldwide go-to weapon for close quarter combat.", 52f, 98f, 79f, 65f, 55f, 3000f, 30, 120, true, true, true, 100, true, true));
        weaponCatalog.Add("M60", new Weapon("M60", "Primary", "LMG", 77, null, "Models/Pics/m60-thumb", "An all American classic machine gun manufactured during the Cold War era, this weapon has stood the test of time and is known for it's simple design, portability, and stopping power.", 70f, 75f, 66f, 88f, 67f, 3000f, 100, 300, true, false, false, 100, true, true));
        weaponCatalog.Add("RPG-7", new Weapon("RPG-7", "Secondary", "Launcher", 81, "Models/Weapons/Secondary/Launchers/RPG-7/RPG-7Projectile", "Models/Pics/rpg7-thumb", "Short for \"rocket-propelled grenade\", the RPG-7 is a portable anti-armor missile launcher that was manufactured in the Soviet Union during the Cold War era. Good for taking out enemy armor and groups.", 110, 80f, -1f, -1f, 64f, 10000f, 1, 4, false, false, false, 100, true, true));
        weaponCatalog.Add("Glock23", new Weapon("Glock23", "Secondary", "Pistol", 82, null, "Models/Pics/glock23-thumb", "The standard pistol used by United States police officers because of its reliability.", 48f, 100f, 45f, 60f, 56f, 3000f, 12, 60, true, true, false, 100, true, false));
        weaponCatalog.Add("R870", new Weapon("R870", "Primary", "Shotgun", 78, null, "Models/Pics/r870-thumb", "Short for Remington 870, this shotgun is widely used for home defense due to its quick reload speed and reliability.", 120f, 95f, 17f, -1f, 60f, 1000f, 8, 56, true, false, false, 100, true, true));
        weaponCatalog.Add("L96A1", new Weapon("L96A1", "Primary", "Sniper Rifle", 80, null, "Models/Pics/l96a1-thumb", "Developed in the 1980s by the British, this bolt-action sniper rifle is known for its deadly stopping power and quick operation speed.", 120f, 40f, 10f, 90f, 65f, 3000f, 5, 20, true, true, false, 100, true, true));
        weaponCatalog.Add("M4A1", new Weapon("M4A1", "Primary", "Assault Rifle", 76, null, "Models/Pics/m4a1-thumb", "As a successor of the M16A3 assault rifle, this weapon is one of the standard issue rifles in the United States military. It's well known for it's overall reliability and quality.", 57f, 92f, 74f, 80f, 64f, 3000f, 30, 120, true, true, true, 100, true, true));
        weaponCatalog.Add("M67 Frag", new Weapon("M67 Frag", "Support", "Explosive", 87, "Models/Weapons/Support/Explosives/M67 Frag/M67FragProjectile", "Models/Pics/m67frag-thumb", "The standard issue high explosive anti-personnel grenade given to all mercenaries upon completion of basic training.", 135f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false, false, 100, true, false));
        weaponCatalog.Add("XM84 Flashbang", new Weapon("XM84 Flashbang", "Support", "Explosive", 88, "Models/Weapons/Support/Explosives/XM84 Flashbang/XM84FlashProjectile", "Models/Pics/xm84flash-thumb", "An explosive non-lethal device used to temporarily blind and disorient your enemies. The closer the enemy is and the more eye exposure given to the device, the longer the effect.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false, false, 100, true, true));
        weaponCatalog.Add("Medkit", new Weapon("Medkit", "Support", "Booster", 84, null, "Models/Pics/medkit-thumb", "Emits a chemical into your body that expedites the coagulation and production of red blood cells. Replenishes 60 HP.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false, false, 100, true, true));
        weaponCatalog.Add("Adrenaphine", new Weapon("Adrenaphine", "Support", "Booster", 83, null, "Models/Pics/adrenalineshot-thumb", "Injects pure adrenaline straight into your blood stream, allowing you to experience unlimited stamina and faster movement speed for 10 seconds.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false, false, 100, true, true));
        weaponCatalog.Add("Ammo Bag", new Weapon("Ammo Bag", "Support", "Deployable", 85, null, "Models/Pics/ammobag-thumb", "A deployable ammo box that allows you and your team to replenish your ammo.", -1f, 90f, -1f, -1f, -1f, -1f, 1, 1, false, false, false, 100, true, true));
        weaponCatalog.Add("First Aid Kit", new Weapon("First Aid Kit", "Support", "Deployable", 86, null, "Models/Pics/firstaidkit-thumb", "A deployable medical kit that allows you and your team to replish your health.", -1f, 97f, -1f, -1f, -1f, -1f, 1, 1, false, false, false, 100, true, true));
        weaponCatalog.Add("Recon Knife", new Weapon("Recon Knife", "Melee", "Knife", 74, null, "Models/Pics/reconknife-thumb", "A lightweight, durable, low profile knife that is multipurpose.", 100f, -1f, -1f, -1f, -1f, 7f, 0, 0, false, false, false, 100, false, false));

        // Characters
        characterCatalog.Add("Lucas", new Character("Lucas", 'M', "Models/Characters/Lucas/PlayerPrefabLucas", 63, -1, "Models/Pics/character_lucas", "Nationality: British\nAs a reformed professional criminal, Lucas is a natural leader who works well with others and gets the job done efficiently.", new int[]{47, 48, 49}, 100, false, "Standard Fatigues Top (M)", "Standard Fatigues Bottom (M)"));
        characterCatalog.Add("Daryl", new Character("Daryl", 'M', "Models/Characters/Daryl/PlayerPrefabDaryl", 60, -1, "Models/Pics/character_daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him well in combat.", new int[]{38, 39, 40}, 100, false, "Standard Fatigues Top (M)", "Standard Fatigues Bottom (M)"));
        characterCatalog.Add("Codename Sayre", new Character("Codename Sayre", 'M', "Models/Characters/Sayre/PlayerPrefabCodenameSayre", 65, -1, "Models/Pics/character_sayre", "Nationality: Mexican\nBeing fresh out of medical school at the top of his class, Codename Sayre is skilled in his medical abilities. His wit and sense of humor allow him to maneuver through sticky situations with ease.", new int[]{53, 54, 55}, 100, true, "Scrubs Top", "Scrubs Bottom"));
        characterCatalog.Add("Hana", new Character("Hana", 'F', "Models/Characters/Hana/PlayerPrefabHana", 61, -1, "Models/Pics/character_hana", "Nationality: Japanese\nWhen her entire family was murdered during her childhood, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, however don't be fooled by her size.", new int[]{41, 42, 43}, 100, false, "Standard Fatigues Top (F)", "Standard Fatigues Bottom (F)"));
        characterCatalog.Add("Jade", new Character("Jade", 'F', "Models/Characters/Jade/PlayerPrefabJade", 62, -1, "Models/Pics/character_jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.", new int[]{44, 45, 46}, 100, false, "Standard Fatigues Top (F)", "Standard Fatigues Bottom (F)"));
        characterCatalog.Add("Yongjin", new Character("Yongjin", 'M', "Models/Characters/Yongjin/PlayerPrefabYongjin", 66, -1, "Models/Pics/yongjin-thumb", "Nationality: South Korean\nAs a former famous musician and artist with nothing left to achieve, Yongjin has decided to dedicate the remainder of his life and assets to the fight against evil.", new int[]{56, 57, 58}, 0, false, "Standard Fatigues Top (M)", "Standard Fatigues Bottom (M)"));
        characterCatalog.Add("Dani", new Character("Dani", 'F', "Models/Characters/Dani/PlayerPrefabDani", 59, -1, "Models/Pics/dani-thumb", "Nationality: American\nDani is an expert on demolitions and explosives and has served her time in the UGA. She is now looking to utilize her demolition skills to benefit the world as much as possible.", new int[]{35, 36, 37}, 0, false, "Standard Fatigues Top (F)", "Standard Fatigues Bottom (F)"));
        characterCatalog.Add("Rocko", new Character("Rocko", 'M', "Models/Characters/Rocko/PlayerPrefabRocko", 64, -1, "Models/Pics/rocko-thumb", "Nationality: Brazilian\nBefore committing his life to the fight against insurgency, Rocko was a gang member in one of Brazil's most notorious drug cartels. He is skilled in guerilla warfare and has a high tolerance for pain.", new int[]{50, 51, 52}, 0, false, "Standard Fatigues Top (M)", "Standard Fatigues Bottom (M)"));

        // Mods
        modCatalog.Add("Standard Suppressor", new Mod("Standard Suppressor", "Suppressor", 73, "Models/Pics/standardsuppressor-thumb", 0, null, "A standard issue suppressor used to silence your weapon.", -3f, 2f, -4f, 0f, 0, 0, 100, true));
        modCatalog.Add("GTF Red Dot A1", new Mod("GTF Red Dot A1", "Sight", 72, "Models/Pics/gtfreddota1-thumb", 0, "HUD/Crosshairs/reddotmark", "A first generation red dot sight designed by the Global Task Force that was intended for simplicity and accuracy.", 0f, 2f, 0f, 0f, 0, 0, 100, true));
    }

}
