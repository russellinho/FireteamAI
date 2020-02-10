using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginControllerScript : MonoBehaviour
{
    public Text copyrightTxt;
    public RawImage titleLogoImg;
    public GameObject popupAlert;
    public Text popupAlertTxt;
    public InputField emailField;
    public InputField passwordField;
    public Button loginBtn;
    private bool activatePopupFlag;
    private string popupMessage;

    // Start is called before the first frame update
    void Start()
    {
        copyrightTxt.text = DateTime.Now.Year + " ©";
    }

    // Update is called once per frame
    void Update()
    {
        if (activatePopupFlag) {
            TriggerPopup();
            activatePopupFlag = false;
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

    void QueuePopup(string message) {
        activatePopupFlag = true;
        popupMessage = message;
    }

    public void OnLoginClick() {
        loginBtn.interactable = false;
        AuthScript.authHandler.auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                QueuePopup("SignInWithEmailAndPasswordAsync was canceled.");
                loginBtn.interactable = true;
                return;
            }
            if (task.IsFaulted) {
                QueuePopup("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                loginBtn.interactable = true;
                return;
            }

            AuthScript.authHandler.user = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
            DAOScript.dao.dbRef.Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
                if (taskA.IsFaulted) {
                    QueuePopup("Database is currently unavailable. Please try again later.\nError: " + taskA.Exception);
                    loginBtn.interactable = true;
                    return;
                } else if (taskA.IsCompleted) {
                    if (!taskA.Result.HasChild(AuthScript.authHandler.user.UserId)) {
                        // DAOScript.dao.dbRef.Child("fteam_ai_users").Child(user.UserId).Child("test").SetValueAsync("testyman").ContinueWith(taskB => {
                        //     if (taskB.IsFaulted) {
                        //         QueuePopup("Database is currently unavailable. Please try again later.\nError: " + taskB.Exception);
                        //         loginBtn.interactable = false;
                        //         return;
                        //     } else if (taskB.IsCompleted) {
                        //         ProceedToSetup();
                        //     }
                        // });
                        ProceedToSetup();
                    } else {
                        ProceedToTitle();
                    }
                }
            });
        });
    }

    public void ProceedToTitle() {
        QueuePopup("Success going to title!");
    }

    public void ProceedToSetup() {
        QueuePopup("Success going to setup!");
    }

    public void OnRegisterClick() {
        Application.OpenURL("https://www.koobando.com/signup");
    }

    public void OnForgotPasswordClick() {
        Application.OpenURL("https://www.koobando.com/forgotPassword");
    }

}
