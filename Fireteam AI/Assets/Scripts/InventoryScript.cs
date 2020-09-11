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
    }

    void Start() {
        DAOScript.dao.dbRef.Child("fteam_ai/shop").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                DataSnapshot itemsLoaded = null;
                IEnumerator<DataSnapshot> subSnap = null;
                // Get characters
                itemsLoaded = task.Result.Child("characters");
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                    equipmentCatalog.Add(itemName, null);
                }

                // Get bottoms
                itemsLoaded = task.Result.Child("bottoms");
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
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
                subSnap = itemsLoaded.Children.GetEnumerator();
                while (subSnap.MoveNext()) {
                    DataSnapshot d = subSnap.Current;
                    string itemName = d.Key.ToString();
                    object projectilePath = d.Child("projectilePath").Value;
                    string projectilePathString = (projectilePath == null ? null : projectilePath.ToString());
                    object damage = d.Child("damage").Value;
                    object mobility = d.Child("mobility").Value;
                    object fireRate = d.Child("fireRate").Value;
                    object accuracy = d.Child("accuracy").Value;
                    object recoil = d.Child("recoil").Value;
                    object range = d.Child("range").Value;
                    object clipCapacity = d.Child("clipCapacity").Value;
                    object maxAmmo = d.Child("maxAmmo").Value;
                    object canBeModded = d.Child("canBeModded").Value;
                    object suppressorCompatible = d.Child("suppressorCompatible").Value;
                    object sightCompatible = d.Child("sightCompatible").Value;
                    object purchasable = d.Child("purchasable").Value;
                    object deleteable = d.Child("deleteable").Value;
                    bool canBeModdedBool = canBeModded == null ? false : d.Child("canBeModded").Value.ToString() == "1";
                    bool suppressorCompatibleBool = suppressorCompatible == null ? false : d.Child("suppressorCompatible").Value.ToString() == "1";
                    bool sightCompatibleBool = sightCompatible == null ? false : d.Child("sightCompatible").Value.ToString() == "1";
                    bool purchasableBool = purchasable == null ? false : d.Child("purchasable").Value.ToString() == "1";
                    bool deleteableBool = deleteable == null ? false : d.Child("deleteable").Value.ToString() == "1";
                    object sway = d.Child("sway").Value;
                    object lungeRange = d.Child("lungeRange").Value;
                    object isSniper = d.Child("isSniper").Value;
                    float swayFloat = (sway == null ? -1f : float.Parse(sway.ToString()));
                    float lungeRangeFloat = (lungeRange == null ? -1f : float.Parse(lungeRange.ToString()));
                    bool isSniperBool = (isSniper == null ? false : (int.Parse(isSniper.ToString()) == 1));
                    weaponCatalog.Add(itemName, new Weapon(itemName, d.Child("type").Value.ToString(), d.Child("category").Value.ToString(), int.Parse(d.Child("prefabPath").Value.ToString()), projectilePathString, d.Child("thumbnailPath").Value.ToString(), d.Child("description").Value.ToString(), damage == null ? -1f : float.Parse(damage.ToString()), mobility == null ? -1f : float.Parse(mobility.ToString()), fireRate == null ? -1f : float.Parse(fireRate.ToString()), accuracy == null ? -1f : float.Parse(accuracy.ToString()), recoil == null ? -1f : float.Parse(recoil.ToString()), range == null ? -1f : float.Parse(range.ToString()), clipCapacity == null ? -1 : int.Parse(clipCapacity.ToString()), maxAmmo == null ? -1 : int.Parse(maxAmmo.ToString()), swayFloat, lungeRangeFloat, isSniperBool, canBeModdedBool, suppressorCompatibleBool, sightCompatibleBool, int.Parse(d.Child("gpPrice").Value.ToString()), purchasableBool, deleteableBool));
                }

                // Get mods
                itemsLoaded = task.Result.Child("mods");
                subSnap = itemsLoaded.Children.GetEnumerator();
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

            equipmentCatalog.CollectionChanged += OnCollectionChanged;
            equipmentCatalog.PropertyChanged += OnPropertyChanged;
            armorCatalog.CollectionChanged += OnCollectionChanged;
            armorCatalog.PropertyChanged += OnPropertyChanged;
            weaponCatalog.CollectionChanged += OnCollectionChanged;
            weaponCatalog.PropertyChanged += OnPropertyChanged;
            characterCatalog.CollectionChanged += OnCollectionChanged;
            characterCatalog.PropertyChanged += OnPropertyChanged;
            modCatalog.CollectionChanged += OnCollectionChanged;
            modCatalog.PropertyChanged += OnPropertyChanged;
        });
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
