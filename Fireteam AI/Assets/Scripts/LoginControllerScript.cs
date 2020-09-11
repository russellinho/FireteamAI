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
    private string popupAlertMessage;
    public InputField emailField;
    public InputField passwordField;
    public Button loginBtn;
    public Toggle rememberLoginToggle;
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
        if (popupAlertMessage != null) {
            TriggerPopup(popupAlertMessage);
            popupAlertMessage = null;
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

    public void ClosePopup() {
        loginBtn.interactable = true;
        popupAlert.SetActive(false);
    }

    void TriggerPopup(string popupMessage) {
        loginBtn.interactable = false;
        popupAlertTxt.text = popupMessage;
        popupAlert.SetActive(true);
    }

    public void OnLoginClick() {
        if (popupAlert.activeInHierarchy) {
            return;
        }
        if (DAOScript.functionsCallHash == null) {
            popupAlertMessage = "Database is currently unavailable. Please try again later.";
            return;
        }
        loginBtn.interactable = false;
        AuthScript.authHandler.auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                popupAlertMessage = ""+task.Exception;
                return;
            }
            if (task.IsFaulted) {
                popupAlertMessage = ""+task.Exception;
                return;
            }
            AuthScript.authHandler.user = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("playerIsBanned");
            func.CallAsync(inputData).ContinueWith((taskS) => {
                Dictionary<object, object> resultsS = (Dictionary<object, object>)taskS.Result.Data;
                if (taskS.IsFaulted) {
                    popupAlertMessage = ""+taskS.Exception;
                    return;
                } else if (resultsS["status"].ToString() == "201") {
                    string duration = resultsS["duration"].ToString();
                    string dateBanned = resultsS["dateBanned"].ToString();
                    string reason = resultsS["reason"].ToString();
                    string banString = "You have been banned for the following reason:\n" + reason + "\n";
                    if (duration == "-1") {
                        banString += "This ban is permanent, so you will no longer be able to use this account.\n";
                    } else {
                        banString += "Date the ban will be lifted:" + CalculateBannedUntilDate(float.Parse(duration), DateTime.Parse(dateBanned)) + "\n";
                    }
                    banString += "If you think this is a mistake, please open a support ticket at \"www.koobando.com/support\"";
                    popupAlertMessage = banString;
                    return;
                } else if (resultsS["status"].ToString() == "200") {
                    // Not banned, proceed
                    func = DAOScript.dao.functions.GetHttpsCallable("checkUserIsSetup");
                    func.CallAsync(inputData).ContinueWith((taskA) => {
                        if (taskA.IsFaulted) {
                            popupAlertMessage = ""+taskA.Exception;
                            return;
                        } else {
                            saveLoginPrefsFlag = true;
                            Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                            // Debug.Log(results["status"].ToString());
                            if (results["status"].ToString() == "401") {
                                // Go to setup
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
                                            popupAlertMessage = ""+taskB.Exception;
                                            return;
                                        } else {
                                            Dictionary<object, object> results2 = (Dictionary<object, object>)taskB.Result.Data;
                                            if (results2["status"].ToString() == "200") {
                                                signInFlag = 2;
                                            } else {
                                                popupAlertMessage = "Database is currently unavailable. Please try again later.";
                                                return;
                                            }
                                        }
                                    });
                                } else {
                                    DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("loggedIn").GetValueAsync().ContinueWith(taskC => {
                                        if (taskC.Result.Child(AuthScript.authHandler.user.UserId).Child("loggedIn").Value.ToString() == "0") {
                                            func.CallAsync(inputData).ContinueWith((taskB) => {
                                                if (taskB.IsFaulted) {
                                                    popupAlertMessage = ""+taskB.Exception;
                                                    return;
                                                } else {
                                                    Dictionary<object, object> results2 = (Dictionary<object, object>)taskB.Result.Data;
                                                    if (results2["status"].ToString() == "200") {
                                                        signInFlag = 2;
                                                    } else {
                                                        popupAlertMessage = "Database is currently unavailable. Please try again later.";
                                                        return;
                                                    }
                                                }
                                            });
                                        } else {
                                            popupAlertMessage = "This account is already logged in on another device! Please check again. If this issue is of error, please log out through the account dashboard on our website by clicking \"Log Out Of All Games\".";
                                            return;
                                        }
                                    });
                                }
                            } else {
                                // Error
                                popupAlertMessage = "Database is currently unavailable. Please try again later.";
                                return;
                            }
                        }
                    });
                }
            });
        });
    }

    private DateTime CalculateBannedUntilDate(float duration, DateTime dateBanned) {
        return dateBanned.AddMinutes(duration);
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
