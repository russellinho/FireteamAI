using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthScript : MonoBehaviour
{
    public Text copyrightTxt;
    public RawImage titleLogoImg;
    public GameObject popupAlert;
    public Text popupAlertTxt;

    // Start is called before the first frame update
    void Start()
    {
        copyrightTxt.text = DateTime.Now.Year + " ©";
    }

    // Update is called once per frame
    // void Update()
    // {

    // }

    public void ClosePopup() {
        popupAlert.SetActive(false);
    }

    void TriggerPopup(string message) {
        popupAlertTxt.text = message;
        popupAlert.SetActive(true);
    }
}
