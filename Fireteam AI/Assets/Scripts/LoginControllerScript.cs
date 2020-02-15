using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool activatePopupFlag;
    private string popupMessage;
    private int signInFlag;

    // Start is called before the first frame update
    void Start()
    {
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
            SceneManager.LoadScene("Setup");
            signInFlag = 0;
        } else if (signInFlag == 2) {
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
                activatePopupFlag = true;
                popupMessage = ""+task.Exception;
                loginBtn.interactable = true;
                return;
            }
            if (task.IsFaulted) {
                activatePopupFlag = true;
                popupMessage = ""+task.Exception;
                loginBtn.interactable = true;
                return;
            }
            AuthScript.authHandler.user = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
            DAOScript.dao.dbRef.Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
                if (taskA.IsFaulted) {
                    Debug.Log("ridin");
                    activatePopupFlag = true;
                    popupMessage = ""+taskA.Exception;
                    loginBtn.interactable = true;
                    return;
                } else if (taskA.IsCompleted) {
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

}
