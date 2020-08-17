﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class LoginControllerScript : MonoBehaviour
{
    public bool developmentMode;
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
        if (popupMessage.Contains("password")) {
            popupMessage = "The password is invalid. Please try again.";
        } else if (popupMessage.Contains("logged")) {
            popupMessage = "This account is already logged in on another device! Please check again. If this issue is of error, please log out through the account dashboard on our website by clicking \"Log Out Of All Games\".";
        } else {
            popupMessage = "Couldn't establish connection to server. Please try again later.";
        }
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
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("checkUserIsSetup");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    popupMessage = ""+taskA.Exception;
                    activatePopupFlag = true;
                    loginBtn.interactable = true;
                    return;
                } else {
                    saveLoginPrefsFlag = true;
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() == "401") {
                        // Go to setup
                        Debug.Log("Success going to setup!");
                        signInFlag = 1;
                    } else if (results["status"].ToString() == "200") {
                        inputData.Clear();
                        inputData["callHash"] = DAOScript.functionsCallHash;
                        inputData["uid"] = AuthScript.authHandler.user.UserId;
                        inputData["loggedIn"] = "1";
                        func = DAOScript.dao.functions.GetHttpsCallable("setUserIsLoggedIn");
                        // Go to login
                        if (developmentMode) {
                            func.CallAsync(inputData).ContinueWith((taskB) => {
                                if (taskB.IsFaulted) {
                                    popupMessage = ""+taskB.Exception;
                                    activatePopupFlag = true;
                                    loginBtn.interactable = true;
                                    return;
                                } else {
                                    Dictionary<object, object> results2 = (Dictionary<object, object>)taskB.Result.Data;
                                    if (results2["status"].ToString() == "200") {
                                        Debug.Log("Success going to title!");
                                        signInFlag = 2;
                                    } else {
                                        popupMessage = "Database is currently unavailable. Please try again later.";
                                        activatePopupFlag = true;
                                        loginBtn.interactable = true;
                                        return;
                                    }
                                }
                            });
                        } else {
                            DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("loggedIn").GetValueAsync().ContinueWith(taskC => {
                                if (taskC.Result.Child(AuthScript.authHandler.user.UserId).Child("loggedIn").Value.ToString() == "0") {
                                    func.CallAsync(inputData).ContinueWith((taskB) => {
                                        if (taskB.IsFaulted) {
                                            popupMessage = ""+taskB.Exception;
                                            activatePopupFlag = true;
                                            loginBtn.interactable = true;
                                            return;
                                        } else {
                                            Dictionary<object, object> results2 = (Dictionary<object, object>)taskB.Result.Data;
                                            if (results2["status"].ToString() == "200") {
                                                Debug.Log("Success going to title!");
                                                signInFlag = 2;
                                            } else {
                                                popupMessage = "Database is currently unavailable. Please try again later.";
                                                activatePopupFlag = true;
                                                loginBtn.interactable = true;
                                                return;
                                            }
                                        }
                                    });
                                } else {
                                    popupMessage = "logged";
                                    activatePopupFlag = true;
                                    loginBtn.interactable = true;
                                    return;
                                }
                            });
                        }
                    } else {
                        // Error
                        popupMessage = "Database is currently unavailable. Please try again later.";
                        activatePopupFlag = true;
                        loginBtn.interactable = true;
                        return;
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
