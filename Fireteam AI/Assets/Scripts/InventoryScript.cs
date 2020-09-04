using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;
using Firebase.Database;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class InventoryScript : MonoBehaviour
{
    public static InventoryScript itemData;
    // Storage for weapons, equipment, and characters in the game
    public ObservableDict<string, Equipment> equipmentCatalog = new ObservableDict<string, Equipment>();
    public ObservableDict<string, Armor> armorCatalog = new ObservableDict<string, Armor>();
    public ObservableDict<string, Weapon> weaponCatalog = new ObservableDict<string, Weapon>();
    public ObservableDict<string, Character> characterCatalog = new ObservableDict<string, Character>();
    public ObservableDict<string, Mod> modCatalog = new ObservableDict<string, Mod>();
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

        DAOScript.dao.dbRef.Child("fteam_ai/items").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                DataSnapshot itemsLoaded = null;
                IEnumerator<DataSnapshot> subSnap = null;
                // Get characters
                itemsLoaded = task.Result.Child("characters");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    string[] skinsString = d.Child("skins").Value.ToString().Split(',');
                    int[] skinsInt = new int[skinsString.Length];
                    for (int i = 0; i < skinsString.Length; i++) {
                        skinsInt[i] = int.Parse(skinsString[i]);
                    }
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    characterCatalog.Add(itemName, new Character(itemName, char.Parse(d.Child("gender").Value.ToString()), d.Child("prefabPath").Value.ToString(), int.Parse(d.Child("fpcFullSkinPath").Value.ToString()), int.Parse(d.Child("fpcNoSkinPath").Value.ToString()), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), skinsInt, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, d.Child("defaultTop").Value.ToString(), d.Child("defaultBottom").Value.ToString(), deleteable));
                }

                // Get tops
                itemsLoaded = task.Result.Child("tops");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPath = d.Child("malePrefabPath").Value;
                    object femalePrefabPath = d.Child("femalePrefabPath").Value;
                    object maleFpcPrefabPath = d.Child("maleFpcPrefabPath").Value;
                    object femaleFpcPrefabPath = d.Child("femaleFpcPrefabPath").Value;
                    object hideHairFlag = d.Child("hideHairFlag").Value;
                    bool hideHairFlagBool = (hideHairFlag == null ? false : (int.Parse(hideHairFlag.ToString()) == 1));
                    object skinType = d.Child("skinType").Value;
                    int skinTypeInt = (skinType == null ? 0 : int.Parse(skinType.ToString()));
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    object gender = d.Child("gender").Value;
                    char genderChar = (gender == null ? 'N' : char.Parse(gender.ToString()));
                    object characterRestrictions = d.Child("characterRestrictions").Value;
                    string[] characterRestrictionsString = (characterRestrictions == null ? new string[0]{} : characterRestrictions.ToString().Split(','));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    equipmentCatalog.Add(itemName, new Equipment(itemName, "Top", (malePrefabPath == null ? -1 : int.Parse(malePrefabPath.ToString())), (femalePrefabPath == null ? -1 : int.Parse(femalePrefabPath.ToString())), (maleFpcPrefabPath == null ? -1 : int.Parse(maleFpcPrefabPath.ToString())), (femaleFpcPrefabPath == null ? -1 : int.Parse(femaleFpcPrefabPath.ToString())), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), hideHairFlagBool, skinTypeInt, speedFloat, staminaFloat, armorFloat, genderChar, characterRestrictionsString, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get bottoms
                itemsLoaded = task.Result.Child("bottoms");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPath = d.Child("malePrefabPath").Value;
                    object femalePrefabPath = d.Child("femalePrefabPath").Value;
                    object maleFpcPrefabPath = d.Child("maleFpcPrefabPath").Value;
                    object femaleFpcPrefabPath = d.Child("femaleFpcPrefabPath").Value;
                    object hideHairFlag = d.Child("hideHairFlag").Value;
                    bool hideHairFlagBool = (hideHairFlag == null ? false : (int.Parse(hideHairFlag.ToString()) == 1));
                    object skinType = d.Child("skinType").Value;
                    int skinTypeInt = (skinType == null ? 0 : int.Parse(skinType.ToString()));
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    object gender = d.Child("gender").Value;
                    char genderChar = (gender == null ? 'N' : char.Parse(gender.ToString()));
                    object characterRestrictions = d.Child("characterRestrictions").Value;
                    string[] characterRestrictionsString = (characterRestrictions == null ? new string[0]{} : characterRestrictions.ToString().Split(','));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    equipmentCatalog.Add(itemName, new Equipment(itemName, "Bottom", (malePrefabPath == null ? -1 : int.Parse(malePrefabPath.ToString())), (femalePrefabPath == null ? -1 : int.Parse(femalePrefabPath.ToString())), (maleFpcPrefabPath == null ? -1 : int.Parse(maleFpcPrefabPath.ToString())), (femaleFpcPrefabPath == null ? -1 : int.Parse(femaleFpcPrefabPath.ToString())), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), hideHairFlagBool, skinTypeInt, speedFloat, staminaFloat, armorFloat, genderChar, characterRestrictionsString, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get footwear
                itemsLoaded = task.Result.Child("footwear");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPath = d.Child("malePrefabPath").Value;
                    object femalePrefabPath = d.Child("femalePrefabPath").Value;
                    object maleFpcPrefabPath = d.Child("maleFpcPrefabPath").Value;
                    object femaleFpcPrefabPath = d.Child("femaleFpcPrefabPath").Value;
                    object hideHairFlag = d.Child("hideHairFlag").Value;
                    bool hideHairFlagBool = (hideHairFlag == null ? false : (int.Parse(hideHairFlag.ToString()) == 1));
                    object skinType = d.Child("skinType").Value;
                    int skinTypeInt = (skinType == null ? 0 : int.Parse(skinType.ToString()));
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    object gender = d.Child("gender").Value;
                    char genderChar = (gender == null ? 'N' : char.Parse(gender.ToString()));
                    object characterRestrictions = d.Child("characterRestrictions").Value;
                    string[] characterRestrictionsString = (characterRestrictions == null ? new string[0]{} : characterRestrictions.ToString().Split(','));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    equipmentCatalog.Add(itemName, new Equipment(itemName, "Footwear", (malePrefabPath == null ? -1 : int.Parse(malePrefabPath.ToString())), (femalePrefabPath == null ? -1 : int.Parse(femalePrefabPath.ToString())), (maleFpcPrefabPath == null ? -1 : int.Parse(maleFpcPrefabPath.ToString())), (femaleFpcPrefabPath == null ? -1 : int.Parse(femaleFpcPrefabPath.ToString())), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), hideHairFlagBool, skinTypeInt, speedFloat, staminaFloat, armorFloat, genderChar, characterRestrictionsString, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get facewear
                itemsLoaded = task.Result.Child("facewear");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPath = d.Child("malePrefabPath").Value;
                    object femalePrefabPath = d.Child("femalePrefabPath").Value;
                    object maleFpcPrefabPath = d.Child("maleFpcPrefabPath").Value;
                    object femaleFpcPrefabPath = d.Child("femaleFpcPrefabPath").Value;
                    object hideHairFlag = d.Child("hideHairFlag").Value;
                    bool hideHairFlagBool = (hideHairFlag == null ? false : (int.Parse(hideHairFlag.ToString()) == 1));
                    object skinType = d.Child("skinType").Value;
                    int skinTypeInt = (skinType == null ? 0 : int.Parse(skinType.ToString()));
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    object gender = d.Child("gender").Value;
                    char genderChar = (gender == null ? 'N' : char.Parse(gender.ToString()));
                    object characterRestrictions = d.Child("characterRestrictions").Value;
                    string[] characterRestrictionsString = (characterRestrictions == null ? new string[0]{} : characterRestrictions.ToString().Split(','));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    equipmentCatalog.Add(itemName, new Equipment(itemName, "Facewear", (malePrefabPath == null ? -1 : int.Parse(malePrefabPath.ToString())), (femalePrefabPath == null ? -1 : int.Parse(femalePrefabPath.ToString())), (maleFpcPrefabPath == null ? -1 : int.Parse(maleFpcPrefabPath.ToString())), (femaleFpcPrefabPath == null ? -1 : int.Parse(femaleFpcPrefabPath.ToString())), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), hideHairFlagBool, skinTypeInt, speedFloat, staminaFloat, armorFloat, genderChar, characterRestrictionsString, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get headgear
                itemsLoaded = task.Result.Child("headgear");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPath = d.Child("malePrefabPath").Value;
                    object femalePrefabPath = d.Child("femalePrefabPath").Value;
                    object maleFpcPrefabPath = d.Child("maleFpcPrefabPath").Value;
                    object femaleFpcPrefabPath = d.Child("femaleFpcPrefabPath").Value;
                    object hideHairFlag = d.Child("hideHairFlag").Value;
                    bool hideHairFlagBool = (hideHairFlag == null ? false : (int.Parse(hideHairFlag.ToString()) == 1));
                    object skinType = d.Child("skinType").Value;
                    int skinTypeInt = (skinType == null ? 0 : int.Parse(skinType.ToString()));
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    object gender = d.Child("gender").Value;
                    char genderChar = (gender == null ? 'N' : char.Parse(gender.ToString()));
                    object characterRestrictions = d.Child("characterRestrictions").Value;
                    string[] characterRestrictionsString = (characterRestrictions == null ? new string[0]{} : characterRestrictions.ToString().Split(','));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    equipmentCatalog.Add(itemName, new Equipment(itemName, "Headgear", (malePrefabPath == null ? -1 : int.Parse(malePrefabPath.ToString())), (femalePrefabPath == null ? -1 : int.Parse(femalePrefabPath.ToString())), (maleFpcPrefabPath == null ? -1 : int.Parse(maleFpcPrefabPath.ToString())), (femaleFpcPrefabPath == null ? -1 : int.Parse(femaleFpcPrefabPath.ToString())), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), hideHairFlagBool, skinTypeInt, speedFloat, staminaFloat, armorFloat, genderChar, characterRestrictionsString, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get armor
                itemsLoaded = task.Result.Child("armor");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object malePrefabPathTop = d.Child("malePrefabPathTop").Value;
                    object malePrefabPathBottom = d.Child("malePrefabPathBottom").Value;
                    object femalePrefabPathTop = d.Child("femalePrefabPathTop").Value;
                    object femalePrefabPathBottom = d.Child("femalePrefabPathBottom").Value;
                    object speed = d.Child("speed").Value;
                    object stamina = d.Child("stamina").Value;
                    object armor = d.Child("armor").Value;
                    float speedFloat = (speed == null ? 0f : float.Parse(speed.ToString()));
                    float staminaFloat = (stamina == null ? 0f : float.Parse(stamina.ToString()));
                    float armorFloat = (armor == null ? 0f : float.Parse(armor.ToString()));
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    armorCatalog.Add(itemName, new Armor(itemName, int.Parse(malePrefabPathTop.ToString()), int.Parse(malePrefabPathBottom.ToString()), int.Parse(femalePrefabPathTop.ToString()), int.Parse(femalePrefabPathBottom.ToString()), d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), speedFloat, staminaFloat, armorFloat, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get weapons
                itemsLoaded = task.Result.Child("weapons");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object projectilePath = d.Child("projectilePath").Value.ToString();
                    string projectilePathString = (projectilePath == null ? null : projectilePath.ToString());
                    object damage = d.Child("damage").Value;
                    object mobility = d.Child("mobility").Value;
                    object fireRate = d.Child("fireRate").Value;
                    object accuracy = d.Child("accuracy").Value;
                    object recoil = d.Child("recoil").Value;
                    object range = d.Child("range").Value;
                    object clipCapacity = d.Child("clipCapacity").Value;
                    object maxAmmo = d.Child("maxAmmo").Value;
                    bool canBeModded = d.Child("canBeModded").Value.ToString() == "1";
                    bool suppressorCompatible = d.Child("suppressorCompatible").Value.ToString() == "1";
                    bool sightCompatible = d.Child("sightCompatible").Value.ToString() == "1";
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    weaponCatalog.Add(itemName, new Weapon(itemName, d.Child("type").Value.ToString(), d.Child("category").Value.ToString(), int.Parse(d.Child("category").Value.ToString()), projectilePathString, d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), float.Parse(damage.ToString()), float.Parse(mobility.ToString()), float.Parse(fireRate.ToString()), float.Parse(accuracy.ToString()), float.Parse(recoil.ToString()), float.Parse(range.ToString()), int.Parse(clipCapacity.ToString()), int.Parse(maxAmmo.ToString()), canBeModded, suppressorCompatible, sightCompatible, int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }

                // Get mods
                itemsLoaded = task.Result.Child("mods");
                subSnap = task.Result.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object crosshairPath = d.Child("crosshairPath").Value;
                    string crosshairPathString = (crosshairPath == null ? null : crosshairPath.ToString());
                    object damageBoost = d.Child("damageBoost").Value;
                    object accuracyBoost = d.Child("accuracyBoost").Value;
                    object recoilBoost = d.Child("recoilBoost").Value;
                    object rangeBoost = d.Child("rangeBoost").Value;
                    object clipCapacityBoost = d.Child("clipCapacityBoost").Value;
                    object maxAmmoBoost = d.Child("maxAmmoBoost").Value;
                    bool purchasable = d.Child("purchasable").Value.ToString() == "1";
                    bool deleteable = d.Child("deleteable").Value.ToString() == "1";
                    modCatalog.Add(itemName, new Mod(itemName, d.Child("category").Value.ToString(), int.Parse(d.Child("prefabPath").Value.ToString()), d.Child("thumbnailPath").Value.ToString(), int.Parse(d.Child("modIndex").Value.ToString()), crosshairPathString, d.Child("description").Value.ToString(), float.Parse(damageBoost.ToString()), float.Parse(accuracyBoost.ToString()), float.Parse(recoilBoost.ToString()), float.Parse(rangeBoost.ToString()), int.Parse(clipCapacityBoost.ToString()), int.Parse(maxAmmoBoost.ToString()), int.Parse(d.Child("gpPrice").Value.ToString()), purchasable, deleteable));
                }
            }
        });

        equipmentCatalog.CollectionChanged += OnCollectionChanged;
        equipmentCatalog.PropertyChanged += OnPropertyChanged;

        // Weapons
        // weaponCatalog.Add("AK-47", new Weapon("AK-47", "Primary", "Assault Rifle", 75, null, "Models/Pics/ak47-thumb", "A classic assault rifle developed in the Soviet Union during the World War II era. It's known for its unmatched stopping power and relatively light weight.", 65f, 90f, 68f, 90f, 70f, 3000f, 30, 120, true, true, true, 100, true, true));
        // weaponCatalog.Add("MP5A3", new Weapon("MP5A3", "Primary", "SMG", 79, null, "Models/Pics/mp5a3-thumb", "The world renowned German engineered masterpiece of submachine guns, the MP5A3 is a worldwide go-to weapon for close quarter combat.", 52f, 98f, 79f, 65f, 55f, 3000f, 30, 120, true, true, true, 100, true, true));
        // weaponCatalog.Add("M60", new Weapon("M60", "Primary", "LMG", 77, null, "Models/Pics/m60-thumb", "An all American classic machine gun manufactured during the Cold War era, this weapon has stood the test of time and is known for it's simple design, portability, and stopping power.", 70f, 75f, 66f, 88f, 67f, 3000f, 100, 300, true, false, false, 100, true, true));
        // weaponCatalog.Add("RPG-7", new Weapon("RPG-7", "Secondary", "Launcher", 81, "Models/Weapons/Secondary/Launchers/RPG-7/RPG-7Projectile", "Models/Pics/rpg7-thumb", "Short for \"rocket-propelled grenade\", the RPG-7 is a portable anti-armor missile launcher that was manufactured in the Soviet Union during the Cold War era. Good for taking out enemy armor and groups.", 110, 80f, -1f, -1f, 64f, 10000f, 1, 4, false, false, false, 100, true, true));
        // weaponCatalog.Add("Glock23", new Weapon("Glock23", "Secondary", "Pistol", 82, null, "Models/Pics/glock23-thumb", "The standard pistol used by United States police officers because of its reliability.", 48f, 100f, 45f, 60f, 56f, 3000f, 12, 60, true, true, false, 100, true, false));
        // weaponCatalog.Add("R870", new Weapon("R870", "Primary", "Shotgun", 78, null, "Models/Pics/r870-thumb", "Short for Remington 870, this shotgun is widely used for home defense due to its quick reload speed and reliability.", 120f, 95f, 17f, -1f, 60f, 1000f, 8, 56, true, false, false, 100, true, true));
        // weaponCatalog.Add("L96A1", new Weapon("L96A1", "Primary", "Sniper Rifle", 80, null, "Models/Pics/l96a1-thumb", "Developed in the 1980s by the British, this bolt-action sniper rifle is known for its deadly stopping power and quick operation speed.", 120f, 40f, 10f, 90f, 65f, 3000f, 5, 20, true, true, false, 100, true, true));
        // weaponCatalog.Add("M4A1", new Weapon("M4A1", "Primary", "Assault Rifle", 76, null, "Models/Pics/m4a1-thumb", "As a successor of the M16A3 assault rifle, this weapon is one of the standard issue rifles in the United States military. It's well known for it's overall reliability and quality.", 57f, 92f, 74f, 80f, 64f, 3000f, 30, 120, true, true, true, 100, true, true));
        // weaponCatalog.Add("M67 Frag", new Weapon("M67 Frag", "Support", "Explosive", 87, "Models/Weapons/Support/Explosives/M67 Frag/M67FragProjectile", "Models/Pics/m67frag-thumb", "The standard issue high explosive anti-personnel grenade given to all mercenaries upon completion of basic training.", 135f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false, false, 100, true, false));
        // weaponCatalog.Add("XM84 Flashbang", new Weapon("XM84 Flashbang", "Support", "Explosive", 88, "Models/Weapons/Support/Explosives/XM84 Flashbang/XM84FlashProjectile", "Models/Pics/xm84flash-thumb", "An explosive non-lethal device used to temporarily blind and disorient your enemies. The closer the enemy is and the more eye exposure given to the device, the longer the effect.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 3, false, false, false, 100, true, true));
        // weaponCatalog.Add("Medkit", new Weapon("Medkit", "Support", "Booster", 84, null, "Models/Pics/medkit-thumb", "Emits a chemical into your body that expedites the coagulation and production of red blood cells. Replenishes 60 HP.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false, false, 100, true, true));
        // weaponCatalog.Add("Adrenaphine", new Weapon("Adrenaphine", "Support", "Booster", 83, null, "Models/Pics/adrenalineshot-thumb", "Injects pure adrenaline straight into your blood stream, allowing you to experience unlimited stamina and faster movement speed for 10 seconds.", -1f, 100f, -1f, -1f, -1f, -1f, 1, 2, false, false, false, 100, true, true));
        // weaponCatalog.Add("Ammo Bag", new Weapon("Ammo Bag", "Support", "Deployable", 85, null, "Models/Pics/ammobag-thumb", "A deployable ammo box that allows you and your team to replenish your ammo.", -1f, 90f, -1f, -1f, -1f, -1f, 1, 1, false, false, false, 100, true, true));
        // weaponCatalog.Add("First Aid Kit", new Weapon("First Aid Kit", "Support", "Deployable", 86, null, "Models/Pics/firstaidkit-thumb", "A deployable medical kit that allows you and your team to replish your health.", -1f, 97f, -1f, -1f, -1f, -1f, 1, 1, false, false, false, 100, true, true));
        // weaponCatalog.Add("Recon Knife", new Weapon("Recon Knife", "Melee", "Knife", 74, null, "Models/Pics/reconknife-thumb", "A lightweight, durable, low profile knife that is multipurpose.", 100f, -1f, -1f, -1f, -1f, 7f, 0, 0, false, false, false, 100, false, false));
    }

    protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (PlayerData.playerdata == null) {
            Application.Quit();
        } else {
            // Ban player here for modifying item data
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                PlayerData.playerdata.TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of game data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (PlayerData.playerdata == null) {
            Application.Quit();
        } else {
            // Ban player here for modifying item data
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                PlayerData.playerdata.TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of game data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

}
