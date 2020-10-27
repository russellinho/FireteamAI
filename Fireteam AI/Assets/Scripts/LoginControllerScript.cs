using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Michsky.UI.Shift;
using TMPro;

public class LoginControllerScript : MonoBehaviour
{
    public bool developmentMode;
    public Text copyrightTxt;
    public ModalWindowManager popupAlert;
    private string popupAlertMessage;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button loginBtn;
    public Button quitBtn;
    public Button forgotPasswordBtn;
    public Button registerBtn;
    public SwitchManager rememberLoginToggle;
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
        if (Input.GetKeyDown(KeyCode.Return)) {
            OnLoginClick();
        }
    }

    public void ClosePopup() {
        loginBtn.interactable = true;
        quitBtn.interactable = true;
        forgotPasswordBtn.interactable = true;
        registerBtn.interactable = true;
        popupAlert.ModalWindowOut();
    }

    void TriggerPopup(string popupMessage) {
        loginBtn.interactable = false;
        quitBtn.interactable = false;
        forgotPasswordBtn.interactable = false;
        registerBtn.interactable = false;
        popupAlert.SetText(popupMessage);
        popupAlert.ModalWindowIn();
    }

    public void OnLoginClick() {
        if (popupAlert.GetIsOn()) {
            return;
        }
        if (DAOScript.functionsCallHash == null) {
            popupAlertMessage = "Database is currently unavailable. Please try again later.";
            return;
        }
        loginBtn.interactable = false;
        quitBtn.interactable = false;
        forgotPasswordBtn.interactable = false;
        registerBtn.interactable = false;
        AuthScript.authHandler.auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                popupAlertMessage = "Invalid password! Please try again.";
                return;
            }
            if (task.IsFaulted) {
                popupAlertMessage = "Invalid password! Please try again.";
                return;
            }
            AuthScript.authHandler.user = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
            // Not banned, proceed
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("checkUserIsSetup");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    popupAlertMessage = ""+taskA.Exception;
                    return;
                } else {
                    saveLoginPrefsFlag = true;
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() == "401") {
                        // Go to setup
                        signInFlag = 1;
                    } else if (results["status"].ToString() == "200") {
                        // Check if banned
                        func = DAOScript.dao.functions.GetHttpsCallable("playerIsBanned");
                        func.CallAsync(inputData).ContinueWith((taskS) => {
                            Dictionary<object, object> resultsS = (Dictionary<object, object>)taskS.Result.Data;
                            if (taskS.IsFaulted) {
                                popupAlertMessage = ""+taskS.Exception;
                                return;
                            } else if (resultsS["status"].ToString() == "201") {
                                // Is banned
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
                                        if (taskC.Result.Value.ToString() == "0") {
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
                                popupAlertMessage = "Database is currently unavailable. Please try again later.";
                                return;
                            }
                        });
                    } else {
                        // Error
                        popupAlertMessage = "Database is currently unavailable. Please try again later.";
                        return;
                    }
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
