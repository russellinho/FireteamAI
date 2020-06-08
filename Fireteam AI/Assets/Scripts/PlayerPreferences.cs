using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerPreferences : MonoBehaviour
{
    public static PlayerPreferences playerPreferences;
    public PreferenceData preferenceData;
    public Dictionary<string, KeyMapping> keyMappings;

    void Awake() {
        if (playerPreferences == null) {
            DontDestroyOnLoad(gameObject);
            this.preferenceData = new PreferenceData();
            this.keyMappings = new Dictionary<string, KeyMapping>();
            playerPreferences = this;
            LoadPreferences();
            LoadKeyMappings();
        } else if (playerPreferences != this) {
            Destroy(this);
        }
    }

    public void LoadPreferences() {
        // Load login prefs file. If file is corrupt or missing, then reset everything to default.
        string filePath = Application.persistentDataPath + "/loginPrefs.dat";
        FileStream file = null;
        if(File.Exists(filePath)) {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                file = File.Open(Application.persistentDataPath + "/loginPrefs.dat", FileMode.Open);
                PreferenceData info = (PreferenceData) bf.Deserialize(file);
                playerPreferences.preferenceData.rememberLogin = info.rememberLogin;
                playerPreferences.preferenceData.rememberUserId = info.rememberUserId;
                playerPreferences.preferenceData.musicVolume = info.musicVolume;
                Debug.Log("Login prefs loaded successfully!");
            } catch (Exception e) {
                Debug.Log("Login prefs file was corrupted. Setting login prefs to default.");
                File.Delete(filePath);
                SetDefaultPreferences();
            } finally {
                if (file != null) {
                    file.Close();
                }
            }
        } else {
            Debug.Log("Login prefs file not found. Setting defaults.");
            SetDefaultPreferences();
        }
    }

    public void LoadKeyMappings() {
        // Load key mappings file. If file is corrupt or missing, then reset everything to default.
        string filePath = Application.persistentDataPath + "/keyMappings.dat";
        FileStream file = null;
        if(File.Exists(filePath)) {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                file = File.Open(Application.persistentDataPath + "/keyMappings.dat", FileMode.Open);
                keyMappings = (Dictionary<string, KeyMapping>) bf.Deserialize(file);
                Debug.Log("Key mappings loaded successfully!");
            } catch (Exception e) {
                Debug.Log("Key mappings file was corrupted. Setting key mappings to default.");
                File.Delete(filePath);
                SetDefaultKeyMappings();
            } finally {
                if (file != null) {
                    file.Close();
                }
            }
        } else {
            Debug.Log("Key mappings file not found. Setting defaults.");
            SetDefaultKeyMappings();
        }
    }

    public void SavePreferences() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/loginPrefs.dat");
        PreferenceData info = new PreferenceData();
        info.rememberLogin = playerPreferences.preferenceData.rememberLogin;
        info.rememberUserId = playerPreferences.preferenceData.rememberUserId;
        info.musicVolume = playerPreferences.preferenceData.musicVolume;
        bf.Serialize(file, info);
        file.Close();
        Debug.Log("Prefs saved.");
    }

    public void SaveKeyMappings() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/keyMappings.dat");
        bf.Serialize(file, keyMappings);
        file.Close();
        Debug.Log("Key mappings saved.");
    }

    void SetDefaultPreferences() {
        playerPreferences.preferenceData.rememberLogin = false;
        playerPreferences.preferenceData.rememberUserId = null;
        playerPreferences.preferenceData.musicVolume = JukeboxScript.DEFAULT_MUSIC_VOLUME;
    }

    void SetDefaultKeyMappings() {
        keyMappings = new Dictionary<string, KeyMapping>();
        keyMappings.Add("Forward", new KeyMapping(KeyCode.W));
        keyMappings.Add("Backward", new KeyMapping(KeyCode.S));
        keyMappings.Add("Left", new KeyMapping(KeyCode.A));
        keyMappings.Add("Right", new KeyMapping(KeyCode.D));
        keyMappings.Add("Sprint", new KeyMapping(KeyCode.LeftShift));
        keyMappings.Add("Crouch", new KeyMapping(KeyCode.LeftControl));
        keyMappings.Add("Jump", new KeyMapping(KeyCode.Space));
        keyMappings.Add("Walk", new KeyMapping(KeyCode.C));
        keyMappings.Add("Interact", new KeyMapping(KeyCode.F));
        keyMappings.Add("Drop", new KeyMapping(KeyCode.G));
        keyMappings.Add("FireMode", new KeyMapping(KeyCode.Q));
        keyMappings.Add("Reload", new KeyMapping(KeyCode.R));
        keyMappings.Add("Melee", new KeyMapping(KeyCode.None, -1));
        keyMappings.Add("AllChat", new KeyMapping(KeyCode.T));
        keyMappings.Add("Primary", new KeyMapping(KeyCode.Alpha1));
        keyMappings.Add("Secondary", new KeyMapping(KeyCode.Alpha2));
        keyMappings.Add("Support", new KeyMapping(KeyCode.Alpha4));
        keyMappings.Add("Scoreboard", new KeyMapping(KeyCode.Tab));
        keyMappings.Add("Pause", new KeyMapping(KeyCode.Escape));
        keyMappings.Add("Fire", new KeyMapping(KeyCode.Mouse0));
        keyMappings.Add("Aim", new KeyMapping(KeyCode.Mouse1));
    }

    public bool KeyWasPressed(string key, bool hold = false, bool up = false) {
        KeyMapping k = keyMappings[key];
        if (k.scrollWheelFlag == 1) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
                return true;
            }
        } else if (k.scrollWheelFlag == -1) {
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
                return true;
            }
        } else {
            if (hold) {
                if (Input.GetKey(k.key)) {
                    return true;
                }
            } else if (Input.GetKeyUp(k.key)) {
                if (Input.GetKeyUp(k.key)) {
                    return true;
                }
            } else {
                if (Input.GetKeyDown(k.key)) {
                    return true;
                }
            }
        }
        return false;
    }

    [Serializable]
    public class PreferenceData
    {
        public bool rememberLogin;
        public string rememberUserId;
        public int musicVolume;
    }

    [Serializable]
    public class KeyMapping {
        public KeyCode key;
        public int scrollWheelFlag;

        public KeyMapping(KeyCode key, int scrollWheelFlag = 0) {
            this.key = key;
            this.scrollWheelFlag = scrollWheelFlag;
        }
    }
}
