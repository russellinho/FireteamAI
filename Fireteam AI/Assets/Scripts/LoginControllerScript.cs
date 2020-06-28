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
        copyrightTxt.text = DateTime.Now.Year + " ©";
        emailField.text = PlayerPreferences.playerPreferences.preferenceData.rememberUserId;
        rememberLoginToggle.isOn = PlayerPreferences.playerPreferences.preferenceData.rememberLogin;
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
                PlayerPreferences.playerPreferences.preferenceData.rememberLogin = rememberLoginToggle.isOn;
                if (rememberLoginToggle.isOn) {
                    PlayerPreferences.playerPreferences.preferenceData.rememberUserId = emailField.text;
                } else {
                    PlayerPreferences.playerPreferences.preferenceData.rememberUserId = null;
                }
                PlayerPreferences.playerPreferences.SavePreferences();
                saveLoginPrefsFlag = false;
            }
            SceneManager.LoadScene("Setup");
            signInFlag = 0;
        } else if (signInFlag == 2) {
            if (saveLoginPrefsFlag) {
                PlayerPreferences.playerPreferences.preferenceData.rememberLogin = rememberLoginToggle.isOn;
                if (rememberLoginToggle.isOn) {
                    PlayerPreferences.playerPreferences.preferenceData.rememberUserId = emailField.text;
                } else {
                    PlayerPreferences.playerPreferences.preferenceData.rememberUserId = null;
                }
                PlayerPreferences.playerPreferences.SavePreferences();
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
            DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
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
                        if (taskA.Result.Child(AuthScript.authHandler.user.UserId).Child("loggedIn").Value.ToString() == "0") {
                            DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("loggedIn").SetValueAsync("1").ContinueWith(taskB => {
                                if (taskB.IsFaulted) {
                                    popupMessage = ""+taskB.Exception;
                                    activatePopupFlag = true;
                                    loginBtn.interactable = true;
                                    return;
                                } else if (taskB.IsCompleted) {
                                    Debug.Log("Success going to title!");
                                    signInFlag = 2;
                                }
                            });
                        } else {
                            popupMessage = "This account is already logged in on another device! Please check again. If this issue is of error, please log out through the account dashboard on our website by clicking \"Log Out Of All Games\".";
                            activatePopupFlag = true;
                            loginBtn.interactable = true;
                            return;
                        }
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

}
