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
    private const int DEFAULT_GAME_VOL = 90;
    private const int DEFAULT_AMBIENT_VOL = 90;
    public static int ASSAULT_TRACK_COUNT = 2;
    public static int STEALTH_TRACK_COUNT = 1;
    private const int DEFAULT_QUALITY_PRESET = 5;
    private const int DEFAULT_VSYNC = 1;
    private const int DEFAULT_LOD_BIAS = 2;
    private const int DEFAULT_ANTIALIASING = 2;
    private const int DEFAULT_ANISOTROPICFILTERING = 2;
    private const int DEFAULT_MASTER_TEXTURE_LIMIT = 0;
    private const int DEFAULT_SHADOW_CASCADES = 4;
    private const int DEFAULT_SHADOW_RESOLUTION = 1;
    private const int DEFAULT_SHADOWS = 2;
    private const float DEFAULT_BRIGHTNESS = 1f;
    public const int MIN_QUALITY_PRESET = 0;
    public const int MAX_QUALITY_PRESET = 5;
    public const int MIN_VSYNC = 0;
    public const int MAX_VSYNC = 2;
    public const int MIN_LOD_BIAS = 1;
    public const int MAX_LOD_BIAS = 10;
    public const int MIN_ANTIALIASING = 0;
    public const int MAX_ANTIALIASING = 3;
    public const int MIN_ANISOTROPICFILTERING = 0;
    public const int MAX_ANISOTROPICFILTERING = 2;
    public const int MIN_MASTER_TEXTURE_LIMIT = 0;
    public const int MAX_MASTER_TEXTURE_LIMIT = 2;
    public const int MIN_SHADOW_CASCADES = 0;
    public const int MAX_SHADOW_CASCADES = 2;
    public const int MIN_SHADOW_RESOLUTION = 0;
    public const int MAX_SHADOW_RESOLUTION = 3;
    public const int MIN_SHADOWS = 0;
    public const int MAX_SHADOWS = 2;
    public const float MIN_BRIGHTNESS = 0.1f;
    public const float MAX_BRIGHTNESS = 1f;
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
                playerPreferences.preferenceData.gameVolume = info.gameVolume == 0 ? 1 : info.gameVolume;
                if (info.gameVolume < 0 || info.gameVolume > 100) {
                    throw new InvalidDataException();
                }
                playerPreferences.preferenceData.ambientVolume = info.ambientVolume == 0 ? 1 : info.ambientVolume;
                if (info.ambientVolume < 0 || info.ambientVolume > 100) {
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
                playerPreferences.preferenceData.stealthTrack = info.stealthTrack;
                if (info.stealthTrack < 0 || info.stealthTrack >= STEALTH_TRACK_COUNT) {
                    playerPreferences.preferenceData.stealthTrack = 0;
                }
                playerPreferences.preferenceData.assaultTrack = info.assaultTrack;
                if (info.assaultTrack < 0 || info.assaultTrack >= ASSAULT_TRACK_COUNT) {
                    playerPreferences.preferenceData.assaultTrack = 0;
                }
                playerPreferences.preferenceData.qualityPreset = info.qualityPreset;
                if (info.qualityPreset < MIN_QUALITY_PRESET || info.qualityPreset > MAX_QUALITY_PRESET) {
                    playerPreferences.preferenceData.qualityPreset = DEFAULT_QUALITY_PRESET;
                }
                playerPreferences.preferenceData.vSyncCount = info.vSyncCount;
                if (info.vSyncCount < MIN_VSYNC || info.vSyncCount > MAX_VSYNC) {
                    playerPreferences.preferenceData.vSyncCount = DEFAULT_VSYNC;
                }
                playerPreferences.preferenceData.lodBias = info.lodBias;
                if (info.lodBias < MIN_LOD_BIAS || info.lodBias > MAX_LOD_BIAS) {
                    playerPreferences.preferenceData.lodBias = DEFAULT_LOD_BIAS;
                }
                playerPreferences.preferenceData.antiAliasing = info.antiAliasing;
                if (info.antiAliasing < MIN_ANTIALIASING || info.antiAliasing > MAX_ANTIALIASING) {
                    playerPreferences.preferenceData.antiAliasing = DEFAULT_ANTIALIASING;
                }
                playerPreferences.preferenceData.anisotropicFiltering = info.anisotropicFiltering;
                if (info.anisotropicFiltering < MIN_ANISOTROPICFILTERING || info.anisotropicFiltering > MAX_ANISOTROPICFILTERING) {
                    playerPreferences.preferenceData.anisotropicFiltering = DEFAULT_ANISOTROPICFILTERING;
                }
                playerPreferences.preferenceData.masterTextureLimit = info.masterTextureLimit;
                if (info.masterTextureLimit < MIN_MASTER_TEXTURE_LIMIT || info.masterTextureLimit > MAX_MASTER_TEXTURE_LIMIT) {
                    playerPreferences.preferenceData.masterTextureLimit = DEFAULT_MASTER_TEXTURE_LIMIT;
                }
                playerPreferences.preferenceData.shadowCascades = info.shadowCascades;
                if (info.shadowCascades < MIN_SHADOW_CASCADES || info.shadowCascades > MAX_SHADOW_CASCADES) {
                    playerPreferences.preferenceData.shadowCascades = DEFAULT_SHADOW_CASCADES;
                }
                playerPreferences.preferenceData.shadowResolution = info.shadowResolution;
                if (info.shadowResolution < MIN_SHADOW_RESOLUTION || info.shadowResolution > MAX_SHADOW_RESOLUTION) {
                    playerPreferences.preferenceData.shadowResolution = DEFAULT_SHADOW_RESOLUTION;
                }
                playerPreferences.preferenceData.shadows = info.shadows;
                if (info.shadows < MIN_SHADOWS || info.shadows > MAX_SHADOWS) {
                    playerPreferences.preferenceData.shadows = DEFAULT_SHADOWS;
                }
                playerPreferences.preferenceData.bloom = info.bloom;
                playerPreferences.preferenceData.motionBlur = info.motionBlur;
                playerPreferences.preferenceData.brightness = info.brightness;
                if (info.brightness < MIN_BRIGHTNESS || info.brightness > MAX_BRIGHTNESS) {
                    playerPreferences.preferenceData.brightness = DEFAULT_BRIGHTNESS;
                }

                SetGraphicsSettings();
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
        info.gameVolume = playerPreferences.preferenceData.gameVolume;
        info.ambientVolume = playerPreferences.preferenceData.ambientVolume;
        info.voiceInputVolume = playerPreferences.preferenceData.voiceInputVolume;
        info.voiceOutputVolume = playerPreferences.preferenceData.voiceOutputVolume;
        info.audioInputName = playerPreferences.preferenceData.audioInputName;
        info.stealthTrack = playerPreferences.preferenceData.stealthTrack;
        info.assaultTrack = playerPreferences.preferenceData.assaultTrack;
        info.qualityPreset = playerPreferences.preferenceData.qualityPreset;
        info.vSyncCount = playerPreferences.preferenceData.vSyncCount;
        info.lodBias = playerPreferences.preferenceData.lodBias;
        info.antiAliasing = playerPreferences.preferenceData.antiAliasing;
        info.anisotropicFiltering = playerPreferences.preferenceData.anisotropicFiltering;
        info.masterTextureLimit = playerPreferences.preferenceData.masterTextureLimit;
        info.bloom = playerPreferences.preferenceData.bloom;
        info.motionBlur = playerPreferences.preferenceData.motionBlur;
        info.shadowCascades = playerPreferences.preferenceData.shadowCascades;
        info.shadowResolution = playerPreferences.preferenceData.shadowResolution;
        info.shadows = playerPreferences.preferenceData.shadows;
        info.brightness = playerPreferences.preferenceData.brightness;
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
        playerPreferences.preferenceData.gameVolume = DEFAULT_GAME_VOL;
        playerPreferences.preferenceData.ambientVolume = DEFAULT_AMBIENT_VOL;
        playerPreferences.preferenceData.voiceInputVolume = 50;
        playerPreferences.preferenceData.voiceOutputVolume = 50;
        playerPreferences.preferenceData.audioInputName = "None";
        playerPreferences.preferenceData.stealthTrack = 0;
        playerPreferences.preferenceData.assaultTrack = 0;

        // Default graphics
        playerPreferences.preferenceData.qualityPreset = 5;
        playerPreferences.preferenceData.vSyncCount = 1;
        playerPreferences.preferenceData.lodBias = 2;
        playerPreferences.preferenceData.antiAliasing = 2;
        playerPreferences.preferenceData.anisotropicFiltering = 2;
        playerPreferences.preferenceData.masterTextureLimit = 0;
        playerPreferences.preferenceData.shadowCascades = 4;
        playerPreferences.preferenceData.shadowResolution = 1;
        playerPreferences.preferenceData.shadows = 2;
        playerPreferences.preferenceData.bloom = false;
        playerPreferences.preferenceData.motionBlur = false;
        playerPreferences.preferenceData.brightness = 1f;

        SetGraphicsSettings();
        JukeboxScript.jukebox.SetMusicVolume((float)JukeboxScript.DEFAULT_MUSIC_VOLUME / 100f);
    }

    public void SetGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(playerPreferences.preferenceData.qualityPreset);
        QualitySettings.vSyncCount = playerPreferences.preferenceData.vSyncCount;
        QualitySettings.lodBias = playerPreferences.preferenceData.lodBias;
        QualitySettings.antiAliasing = playerPreferences.preferenceData.antiAliasing;
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)playerPreferences.preferenceData.anisotropicFiltering;
        QualitySettings.masterTextureLimit = playerPreferences.preferenceData.masterTextureLimit;
        QualitySettings.shadowCascades = playerPreferences.preferenceData.shadowCascades;
        QualitySettings.shadowResolution = (ShadowResolution)playerPreferences.preferenceData.shadowResolution;
        QualitySettings.shadows = (ShadowQuality)playerPreferences.preferenceData.shadows;
        Screen.brightness = playerPreferences.preferenceData.brightness;
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
        public int gameVolume;
        public int ambientVolume;
        public int voiceInputVolume;
        public int voiceOutputVolume;
        public string audioInputName;
        public int stealthTrack;
        public int assaultTrack;
        public int qualityPreset;
        public int vSyncCount;
        public float lodBias;
        public int antiAliasing;
        public int anisotropicFiltering;
        public int masterTextureLimit;
        public float brightness;
        public int shadows;
        public int shadowCascades;
        public int shadowResolution;
        public bool bloom;
        public bool motionBlur;
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
