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

    void Awake() {
        if (playerPreferences == null) {
            DontDestroyOnLoad(gameObject);
            this.preferenceData = new PreferenceData();
            playerPreferences = this;
            LoadPreferences();
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

    void SetDefaultPreferences() {
        playerPreferences.preferenceData.rememberLogin = false;
        playerPreferences.preferenceData.rememberUserId = null;
        playerPreferences.preferenceData.musicVolume = JukeboxScript.DEFAULT_MUSIC_VOLUME;
    }

    [Serializable]
    public class PreferenceData
    {
        public bool rememberLogin;
        public string rememberUserId;
        public int musicVolume;
    }
}
