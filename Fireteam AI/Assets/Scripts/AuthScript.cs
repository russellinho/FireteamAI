using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class AuthScript : MonoBehaviour
{
    public Text copyrightTxt;
    public RawImage titleLogoImg;
    public GameObject popupAlert;
    public Text popupAlertTxt;
    public InputField emailField;
    public InputField passwordField;
    public Button loginBtn;

    private FirebaseAuth auth;
    private bool activatePopupFlag;
    private string popupMessage;

    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
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
        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
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

            Firebase.Auth.FirebaseUser newUser = task.Result;
            //QueuePopup("User signed in successfully: {" + newUser.DisplayName + "} ({" + newUser.UserId + "})");
            // Query DB to see if the user is set up yet. If not, go to setup. Else, go to title page.
        });
    }

    public void ProceedToTitle() {

    }

    public void ProceedToSetup() {

    }

    public void OnRegisterClick() {
        Application.OpenURL("https://www.koobando.com/signup");
    }

    public void OnForgotPasswordClick() {
        Application.OpenURL("https://www.koobando.com/forgotPassword");
    }
    
}
