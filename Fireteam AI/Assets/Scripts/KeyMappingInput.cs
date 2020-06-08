using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KeyMapping = PlayerPreferences.KeyMapping;

public class KeyMappingInput : MonoBehaviour
{
    public TitleControllerScript titleController;
    public PauseMenuScript pauseMenuScript;
    public string keyCode;
    public Text keyDescription;
    public Text actionDescription;
    private bool currentlyChanging;
    // Start is called before the first frame update
    void Start() {
        KeyMapping k = PlayerPreferences.playerPreferences.keyMappings[keyCode];
        if (k.scrollWheelFlag == 0) {
            keyDescription.text = k.key.ToString();
        } else if (k.scrollWheelFlag == 1) {
            keyDescription.text = "Mouse Wheel Up";
        } else {
            keyDescription.text = "Mouse Wheel Down";
        }
    }

    public void OnClick() {
        // Go into changing key mapping mode
        currentlyChanging = true;
        if (titleController != null) {
            titleController.ToggleIsChangingKeyMapping(true, actionDescription.text);
        } else if (pauseMenuScript != null) {
            pauseMenuScript.ToggleIsChangingKeyMapping(true, actionDescription.text);
        }

        // Set the text to ? temporarily
        keyDescription.text = "???";
    }

    void Update() {
        if (currentlyChanging) {
            HandleKeyChange();
        }
    }

    void HandleKeyChange() {
        KeyMapping k = PlayerPreferences.playerPreferences.keyMappings[keyCode];
        // Set the key to the new key in the preferences
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            k.key = KeyCode.None;
            k.scrollWheelFlag = -1;
            keyDescription.text = "Mouse Wheel Down";
        } else if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            k.key = KeyCode.None;
            k.scrollWheelFlag = 1;
            keyDescription.text = "Mouse Wheel Up";
        } else {
            foreach(KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
                if (Input.GetKey(vKey)) {
                    k.key = vKey;
                    keyDescription.text = vKey.ToString();
                    break;
                }
            }
            k.scrollWheelFlag = 0;
        }        

        // Take the key out of changing mode
        currentlyChanging = false;
    }

}
