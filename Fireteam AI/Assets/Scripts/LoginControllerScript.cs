using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginControllerScript : MonoBehaviour
{
    public Text copyrightTxt;
    public RawImage titleLogoImg;
    public GameObject popupAlert;
    public Text popupAlertTxt;
    public InputField emailField;
    public InputField passwordField;
    public Button loginBtn;
    public Toggle rememberLoginToggle;
    private bool activatePopupFlag;
    private string popupMessage;
    private int signInFlag;
    private bool saveLoginPrefsFlag;

    // Start is called before the first frame update
    void Start()
    {
        LoadLoginPreferences();
        copyrightTxt.text = DateTime.Now.Year + " ©";
    }

    // Update is called once per frame
    void Update()
    {
        if (activatePopupFlag) {
            ConvertPopupMessage();
            TriggerPopup();
            activatePopupFlag = false;
        }
        if (signInFlag == 1) {
            if (saveLoginPrefsFlag) {
                SaveLoginPreferences();
                saveLoginPrefsFlag = false;
            }
            SceneManager.LoadScene("Setup");
            signInFlag = 0;
        } else if (signInFlag == 2) {
            if (saveLoginPrefsFlag) {
                SaveLoginPreferences();
                saveLoginPrefsFlag = false;
            }
            SceneManager.LoadScene("Title");
            signInFlag = 0;
        }
    }

    void ConvertPopupMessage() {
        popupMessage = (popupMessage.Contains("password") ? "The password is invalid. Please try again." : "Couldn't establish connection to server. Please try again later.");
    }

    public void ClosePopup() {
        popupAlert.SetActive(false);
        popupMessage = "";
    }

    void TriggerPopup() {
        popupAlertTxt.text = popupMessage;
        popupAlert.SetActive(true);
    }

    public void OnLoginClick() {
        if (popupAlert.activeInHierarchy) {
            return;
        }
        loginBtn.interactable = false;
        AuthScript.authHandler.auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                popupMessage = ""+task.Exception;
                activatePopupFlag = true;
                loginBtn.interactable = true;
                return;
            }
            if (task.IsFaulted) {
                popupMessage = ""+task.Exception;
                activatePopupFlag = true;
                loginBtn.interactable = true;
                return;
            }
            AuthScript.authHandler.user = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
            DAOScript.dao.dbRef.Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
                if (taskA.IsFaulted) {
                    popupMessage = ""+taskA.Exception;
                    activatePopupFlag = true;
                    loginBtn.interactable = true;
                    return;
                } else if (taskA.IsCompleted) {
                    saveLoginPrefsFlag = true;
                    if (!taskA.Result.HasChild(AuthScript.authHandler.user.UserId)) {
                        Debug.Log("Success going to setup!");
                        signInFlag = 1;
                    } else {
                        Debug.Log("Success going to title!");
                        signInFlag = 2;
                    }
                }
            });
        });
    }

    public void OnRegisterClick() {
        Application.OpenURL("https://www.koobando.com/signup");
    }

    public void OnForgotPasswordClick() {
        Application.OpenURL("https://www.koobando.com/forgotPassword");
    }

    public void OnQuitClick() {
        Application.Quit();
    }

    void LoadLoginPreferences() {
        // Load login prefs file. If file is corrupt or missing, then reset everything to default.
        string filePath = Application.persistentDataPath + "/loginPrefs.dat";
        FileStream file = null;
        if(File.Exists(filePath)) {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                file = File.Open(Application.persistentDataPath + "/loginPrefs.dat", FileMode.Open);
                LoginPreferences info = (LoginPreferences) bf.Deserialize(file);
                rememberLoginToggle.isOn = info.rememberLogin;
                emailField.text = info.rememberUserId;
                Debug.Log("Login prefs loaded successfully!");
            } catch (Exception e) {
                Debug.Log("Login prefs file was corrupted. Setting login prefs to default.");
                File.Delete(filePath);
                SetDefaultLoginPreferences();
            } finally {
                if (file != null) {
                    file.Close();
                }
            }
        } else {
            Debug.Log("Login prefs file not found. Setting defaults.");
            SetDefaultLoginPreferences();
        }
    }

    void SaveLoginPreferences() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/loginPrefs.dat");
        LoginPreferences info = new LoginPreferences();
        info.rememberLogin = rememberLoginToggle.isOn;
        if (rememberLoginToggle.isOn) {
            info.rememberUserId = emailField.text;
        } else {
            info.rememberUserId = "";
        }
        bf.Serialize(file, info);
        file.Close();
        Debug.Log("Login prefs saved.");
    }

    void SetDefaultLoginPreferences() {
        rememberLoginToggle.isOn = false;
        emailField.text = "";
    }

    [Serializable]
    class LoginPreferences
    {
        public bool rememberLogin;
        public string rememberUserId;
    }

}
