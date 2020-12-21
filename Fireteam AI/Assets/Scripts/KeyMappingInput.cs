using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KeyMapping = PlayerPreferences.KeyMapping;
using TMPro;
using Michsky.UI.Shift;

public class KeyMappingInput : MonoBehaviour
{
    public TitleControllerScript titleController;
    public PressKeyEvent pressKeyEvent;
    public PauseMenuScript pauseMenuScript;
    public string keyCode;
    public TextMeshProUGUI keyDescription;
    // public Text actionDescription;
    // Start is called before the first frame update
    void Start() {
        ResetKeyDisplay();
    }

    public void ResetKeyDisplay()
    {
        KeyMapping k = PlayerPreferences.playerPreferences.keyMappings[keyCode];
        if (k.scrollWheelFlag == 0) {
            keyDescription.text = k.key.ToString();
        } else if (k.scrollWheelFlag == 1) {
            keyDescription.text = "Mouse Wheel Up";
        } else {
            keyDescription.text = "Mouse Wheel Down";
        }
    }

    public KeyMappingInput GetKeyDescription(int i) {
        if (titleController != null) {
            return titleController.keyMappingInputs[i];
        } else if (pauseMenuScript != null) {
            return pauseMenuScript.keyMappingInputs[i];
        }
        return null;
    }

    public void SetKeyMappingInputOnPopup() {
        pressKeyEvent.keyMappingInput = this;
    }

    public void HandleKeyChange() {
        KeyMapping k = PlayerPreferences.playerPreferences.keyMappings[keyCode];
        // Set the key to the new key in the preferences
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            // If another action is already mapped to this key, then swap the keys
            string alreadyMappedAction = PlayerPreferences.playerPreferences.KeyIsMappedOn(KeyCode.None, -1);
            if (alreadyMappedAction != null) {
                KeyMapping alreadyMappedKeyMapping = PlayerPreferences.playerPreferences.keyMappings[alreadyMappedAction];
                KeyMappingInput alreadyMappedInput = GetKeyDescription(alreadyMappedKeyMapping.keyDescriptionIndex);
                alreadyMappedKeyMapping.scrollWheelFlag = k.scrollWheelFlag;
                alreadyMappedKeyMapping.key = k.key;
                if (alreadyMappedKeyMapping.scrollWheelFlag == 1) {
                    alreadyMappedInput.keyDescription.text = "Mouse Wheel Up";
                } else if (alreadyMappedKeyMapping.scrollWheelFlag == -1) {
                    alreadyMappedInput.keyDescription.text = "Mouse Wheel Down";
                } else {
                    alreadyMappedInput.keyDescription.text = alreadyMappedKeyMapping.key.ToString();
                }
            }
            k.key = KeyCode.None;
            k.scrollWheelFlag = -1;
            keyDescription.text = "Mouse Wheel Down";
        } else if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            // If another action is already mapped to this key, then swap the keys
            string alreadyMappedAction = PlayerPreferences.playerPreferences.KeyIsMappedOn(KeyCode.None, 1);
            if (alreadyMappedAction != null) {
                KeyMapping alreadyMappedKeyMapping = PlayerPreferences.playerPreferences.keyMappings[alreadyMappedAction];
                KeyMappingInput alreadyMappedInput = GetKeyDescription(alreadyMappedKeyMapping.keyDescriptionIndex);
                alreadyMappedKeyMapping.scrollWheelFlag = k.scrollWheelFlag;
                alreadyMappedKeyMapping.key = k.key;
                if (alreadyMappedKeyMapping.scrollWheelFlag == 1) {
                    alreadyMappedInput.keyDescription.text = "Mouse Wheel Up";
                } else if (alreadyMappedKeyMapping.scrollWheelFlag == -1) {
                    alreadyMappedInput.keyDescription.text = "Mouse Wheel Down";
                } else {
                    alreadyMappedInput.keyDescription.text = alreadyMappedKeyMapping.key.ToString();
                }
            }
            k.key = KeyCode.None;
            k.scrollWheelFlag = 1;
            keyDescription.text = "Mouse Wheel Up";
        } else {
            foreach(KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
                if (Input.GetKey(vKey)) {
                    // If another action is already mapped to this key, then swap the keys
                    string alreadyMappedAction = PlayerPreferences.playerPreferences.KeyIsMappedOn(vKey, 0);
                    if (alreadyMappedAction != null) {
                        KeyMapping alreadyMappedKeyMapping = PlayerPreferences.playerPreferences.keyMappings[alreadyMappedAction];
                        KeyMappingInput alreadyMappedInput = GetKeyDescription(alreadyMappedKeyMapping.keyDescriptionIndex);
                        alreadyMappedKeyMapping.scrollWheelFlag = k.scrollWheelFlag;
                        alreadyMappedKeyMapping.key = k.key;
                        if (alreadyMappedKeyMapping.scrollWheelFlag == 1) {
                            alreadyMappedInput.keyDescription.text = "Mouse Wheel Up";
                        } else if (alreadyMappedKeyMapping.scrollWheelFlag == -1) {
                            alreadyMappedInput.keyDescription.text = "Mouse Wheel Down";
                        } else {
                            alreadyMappedInput.keyDescription.text = alreadyMappedKeyMapping.key.ToString();
                        }
                    }
                    
                    k.scrollWheelFlag = 0;
                    k.key = vKey;
                    keyDescription.text = vKey.ToString();
                    break;
                }
            }
        }        
    }

}
