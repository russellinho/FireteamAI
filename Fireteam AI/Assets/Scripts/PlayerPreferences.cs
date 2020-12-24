using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Koobando.AntiCheat;

public class PlayerPreferences : MonoBehaviour
{
    private const float KEY_COUNT = 25;
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
                if (info.musicVolume < 0 || info.musicVolume > 100) {
                    throw new InvalidDataException();
                }
                playerPreferences.preferenceData.voiceInputVolume = info.voiceInputVolume == 0 ? 50 : info.voiceInputVolume;
                if (info.voiceInputVolume < 0 || info.voiceInputVolume > 75) {
                    throw new InvalidDataException();
                }
                playerPreferences.preferenceData.voiceOutputVolume = info.voiceOutputVolume == 0 ? 50 : info.voiceOutputVolume;
                if (info.voiceOutputVolume < 0 || info.voiceOutputVolume > 75) {
                    throw new InvalidDataException();
                }
                playerPreferences.preferenceData.audioInputName = string.IsNullOrEmpty(info.audioInputName) ? "None" : info.audioInputName;
                JukeboxScript.jukebox.SetMusicVolume((float)playerPreferences.preferenceData.musicVolume / 100f);
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
                if (keyMappings.Count != 25) {
                    // For when new keys are added
                    Debug.Log("More keys were added since last update, setting all to default.");
                    SetDefaultKeyMappings();
                }
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
        info.voiceInputVolume = playerPreferences.preferenceData.voiceInputVolume;
        info.voiceOutputVolume = playerPreferences.preferenceData.voiceOutputVolume;
        info.audioInputName = playerPreferences.preferenceData.audioInputName;
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
        playerPreferences.preferenceData.voiceInputVolume = 50;
        playerPreferences.preferenceData.voiceOutputVolume = 50;
        playerPreferences.preferenceData.audioInputName = "None";
        JukeboxScript.jukebox.SetMusicVolume((float)JukeboxScript.DEFAULT_MUSIC_VOLUME / 100f);
    }

    void SetDefaultKeyMappings() {
        keyMappings = new Dictionary<string, KeyMapping>();
        keyMappings.Add("Forward", new KeyMapping(KeyCode.W, 0));
        keyMappings.Add("Backward", new KeyMapping(KeyCode.S, 1));
        keyMappings.Add("Left", new KeyMapping(KeyCode.A, 2));
        keyMappings.Add("Right", new KeyMapping(KeyCode.D, 3));
        keyMappings.Add("Sprint", new KeyMapping(KeyCode.LeftShift, 5));
        keyMappings.Add("Crouch", new KeyMapping(KeyCode.LeftControl, 7));
        keyMappings.Add("Jump", new KeyMapping(KeyCode.Space, 4));
        keyMappings.Add("Walk", new KeyMapping(KeyCode.C, 6));
        keyMappings.Add("Interact", new KeyMapping(KeyCode.F, 8));
        keyMappings.Add("Drop", new KeyMapping(KeyCode.G, 9));
        keyMappings.Add("FireMode", new KeyMapping(KeyCode.Q, 12));
        keyMappings.Add("Reload", new KeyMapping(KeyCode.R, 14));
        keyMappings.Add("Melee", new KeyMapping(KeyCode.None, 13, -1));
        keyMappings.Add("AllChat", new KeyMapping(KeyCode.T, 18));
        keyMappings.Add("VoiceChat", new KeyMapping(KeyCode.Period, 21));
        keyMappings.Add("VCReport", new KeyMapping(KeyCode.V, 22));
        keyMappings.Add("VCTactical", new KeyMapping(KeyCode.B, 23));
        keyMappings.Add("VCSocial", new KeyMapping(KeyCode.N, 24));
        keyMappings.Add("Primary", new KeyMapping(KeyCode.Alpha1, 15));
        keyMappings.Add("Secondary", new KeyMapping(KeyCode.Alpha2, 16));
        keyMappings.Add("Support", new KeyMapping(KeyCode.Alpha4, 17));
        keyMappings.Add("Scoreboard", new KeyMapping(KeyCode.Tab, 20));
        keyMappings.Add("Pause", new KeyMapping(KeyCode.Escape, 19));
        keyMappings.Add("Fire", new KeyMapping(KeyCode.Mouse0, 10));
        keyMappings.Add("Aim", new KeyMapping(KeyCode.Mouse1, 11));
    }

    public void ResetKeyMappings()
    {
        keyMappings["Forward"].key = KeyCode.W;
        keyMappings["Forward"].scrollWheelFlag = 0;
        keyMappings["Backward"].key = KeyCode.S;
        keyMappings["Backward"].scrollWheelFlag = 0;
        keyMappings["Left"].key = KeyCode.A;
        keyMappings["Left"].scrollWheelFlag = 0;
        keyMappings["Right"].key = KeyCode.D;
        keyMappings["Right"].scrollWheelFlag = 0;
        keyMappings["Sprint"].key = KeyCode.LeftShift;
        keyMappings["Sprint"].scrollWheelFlag = 0;
        keyMappings["Crouch"].key = KeyCode.LeftControl;
        keyMappings["Crouch"].scrollWheelFlag = 0;
        keyMappings["Jump"].key = KeyCode.Space;
        keyMappings["Jump"].scrollWheelFlag = 0;
        keyMappings["Walk"].key = KeyCode.C;
        keyMappings["Walk"].scrollWheelFlag = 0;
        keyMappings["Interact"].key = KeyCode.F;
        keyMappings["Interact"].scrollWheelFlag = 0;
        keyMappings["Drop"].key = KeyCode.G;
        keyMappings["Drop"].scrollWheelFlag = 0;
        keyMappings["FireMode"].key = KeyCode.Q;
        keyMappings["FireMode"].scrollWheelFlag = 0;
        keyMappings["Reload"].key = KeyCode.R;
        keyMappings["Reload"].scrollWheelFlag = 0;
        keyMappings["Melee"].key = KeyCode.None;
        keyMappings["Melee"].scrollWheelFlag = -1;
        keyMappings["AllChat"].key = KeyCode.T;
        keyMappings["AllChat"].scrollWheelFlag = 0;
        keyMappings["VoiceChat"].key = KeyCode.Period;
        keyMappings["VoiceChat"].scrollWheelFlag = 0;
        keyMappings["VCReport"].key = KeyCode.V;
        keyMappings["VCReport"].scrollWheelFlag = 0;
        keyMappings["VCTactical"].key = KeyCode.B;
        keyMappings["VCTactical"].scrollWheelFlag = 0;
        keyMappings["VCSocial"].key = KeyCode.N;
        keyMappings["VCSocial"].scrollWheelFlag = 0;
        keyMappings["Primary"].key = KeyCode.Alpha1;
        keyMappings["Primary"].scrollWheelFlag = 0;
        keyMappings["Secondary"].key = KeyCode.Alpha2;
        keyMappings["Secondary"].scrollWheelFlag = 0;
        keyMappings["Support"].key = KeyCode.Alpha4;
        keyMappings["Support"].scrollWheelFlag = 0;
        keyMappings["Scoreboard"].key = KeyCode.Tab;
        keyMappings["Scoreboard"].scrollWheelFlag = 0;
        keyMappings["Pause"].key = KeyCode.Escape;
        keyMappings["Pause"].scrollWheelFlag = 0;
        keyMappings["Fire"].key = KeyCode.Mouse0;
        keyMappings["Fire"].scrollWheelFlag = 0;
        keyMappings["Aim"].key = KeyCode.Mouse1;
        keyMappings["Aim"].scrollWheelFlag = 0;
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
            } else if (up) {
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

    public string KeyIsMappedOn(KeyCode key, int scrollWheelFlag) {
        foreach (KeyValuePair<string, KeyMapping> k in keyMappings) {
            if (scrollWheelFlag != 0) {
                if (k.Value.scrollWheelFlag == scrollWheelFlag) {
                    return k.Key;
                }
            } else {
                if (k.Value.key == key) {
                    return k.Key;
                }
            }
        }

        return null;
    }

    [Serializable]
    public class PreferenceData
    {
        public bool rememberLogin;
        public string rememberUserId;
        public int musicVolume;
        public int voiceInputVolume;
        public int voiceOutputVolume;
        public string audioInputName;
    }

    [Serializable]
    public class KeyMapping {
        public KeyCode key;
        public int scrollWheelFlag;
        public int keyDescriptionIndex;

        public KeyMapping(KeyCode key, int keyDescriptionIndex, int scrollWheelFlag = 0) {
            this.key = key;
            this.keyDescriptionIndex = keyDescriptionIndex;
            this.scrollWheelFlag = scrollWheelFlag;
        }
    }
}
