﻿using System.Collections;
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
        if ((titleController != null && titleController.isChangingKeyMapping) || (pauseMenuScript != null && pauseMenuScript.isChangingKeyMapping)) return;
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

    public KeyMappingInput GetKeyDescription(int i) {
        if (titleController != null) {
            return titleController.keyMappingInputs[i];
        } else if (pauseMenuScript != null) {
            return pauseMenuScript.keyMappingInputs[i];
        }
        return null;
    }

    void HandleKeyChange() {
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
            // Take the key out of changing mode
            currentlyChanging = false;
            if (titleController != null) {
                titleController.ToggleIsChangingKeyMapping(false, null);
            } else if (pauseMenuScript != null) {
                pauseMenuScript.ToggleIsChangingKeyMapping(false, null);
            }
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
            // Take the key out of changing mode
            currentlyChanging = false;
            if (titleController != null) {
                titleController.ToggleIsChangingKeyMapping(false, null);
            } else if (pauseMenuScript != null) {
                pauseMenuScript.ToggleIsChangingKeyMapping(false, null);
            }
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
                    // Take the key out of changing mode
                    currentlyChanging = false;
                    if (titleController != null) {
                        titleController.ToggleIsChangingKeyMapping(false, null);
                    } else if (pauseMenuScript != null) {
                        pauseMenuScript.ToggleIsChangingKeyMapping(false, null);
                    }
                    break;
                }
            }
        }        
    }

}
