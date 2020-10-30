using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Michsky.UI.Shift;
using TMPro;

public class SetupControllerScript : MonoBehaviour
{
    private Vector3 bodyPos = new Vector3(-0.45f, 4.87f, 2.13f);
    private GameObject bodyRef;
    public GameObject contentInventory;
    public TMP_InputField characterNameInput;
    private Dictionary<string, int> starterCharacters = new Dictionary<string, int>(){["Lucas"] = 0, ["Daryl"] = 1, ["Yongjin"] = 2, ["Rocko"] = 3, ["Hana"] = 4, ["Jade"] = 5, ["Dani"] = 6};
    public ArrayList starterWeapons = new ArrayList(){"N4A1", "KA-74"};
    private string selectedCharacter;
    private SetupItemScript selectedSlot;
    public GameObject contentPrefab;
    public GameObject selectionDesc;
    public CanvasGroup mainPanel;
    public ModalWindowManager popupAlert;
    public ModalWindowManager confirmAlert;
    public BlurManager blurManager;
    public Button confirmAlertConfirmBtn;
    public Button confirmAlertCancelBtn;
    public Button proceedBtn;
    public Button checkBtn;
    public HorizontalSelector weaponSelector;
    private bool activatePopupFlag;
    private bool activateConfirmFlag;
    private bool finishedFlag;
    private bool completeCharCreationFlag;
    private string popupMessage;
    public GameObject[] starterCharacterRefs;
    private int previousWeaponIndex;
    private bool blockScreenTrigger;
    public GameObject blockScreen;
    public GameObject blockBlur;
    // Start is called before the first frame update
    void Start()
    {
        mainPanel.GetComponent<Animator>().Play("Panel In");
        previousWeaponIndex = -1;
        InitializeWeaponSelection();
        selectedCharacter = "Lucas";
        SpawnSelectedCharacter();
        EquipSelectedWeapon();
        InitializeCharacterSelection();
    }

    void Update()
    {
        ToggleBlockScreen(blockScreenTrigger);
        if (activatePopupFlag) {
            TriggerPopup();
            activatePopupFlag = false;
        } else if (activateConfirmFlag) {
            TriggerConfirmPopup();
            activateConfirmFlag = false;
        } else if (finishedFlag) {
            SceneManager.LoadScene("Title");
            finishedFlag = false;
        }
    }

    void InitializeWeaponSelection() {
        weaponSelector.ClearItems();
        for (int i = 0; i < starterWeapons.Count; i++) {
            weaponSelector.CreateNewItem((string)starterWeapons[i]);
        }
        weaponSelector.UpdateUI();
    }

    void InitializeCharacterSelection() {
        // Populate into grid layout
		foreach (KeyValuePair<string, int> qq in starterCharacters) {
			string thisCharacterName = qq.Key;
			Character c = InventoryScript.itemData.characterCatalog[thisCharacterName];
			GameObject o = Instantiate(contentPrefab);
            SetupItemScript s = o.GetComponent<SetupItemScript>();
            s.setupItemType = SetupItemScript.SetupItemType.Character;
			s.itemDescriptionPopupRef = selectionDesc.GetComponent<ItemPopupScript>();
			s.characterDetails = c;
			s.itemName = thisCharacterName;
			s.itemDescription = c.description;
			s.thumbnailRef.texture = (Texture)Resources.Load(c.thumbnailPath);
			if (qq.Value == 0) {
				s.ToggleSelectedIndicator(true);
				selectedSlot = s;
			} else {
                s.ToggleSelectedIndicator(false);
            }
			o.transform.SetParent(contentInventory.transform);
            s.setupController = this;
		}
    }

    private void DeselectCharacter() {
        selectedCharacter = "";
        selectedSlot.ToggleSelectedIndicator(false);
        selectedSlot = null;
        DespawnSelectedCharacter();
    }

    public void SelectCharacter(SetupItemScript setupItem, string name) {
        DeselectCharacter();
        selectedCharacter = name;
        selectedSlot = setupItem;
        selectedSlot.ToggleSelectedIndicator(true);
        SpawnSelectedCharacter();
        EquipSelectedWeapon();
    }

    void SpawnSelectedCharacter() {
        bodyRef = Instantiate(starterCharacterRefs[starterCharacters[selectedCharacter]], bodyPos, Quaternion.Euler(new Vector3(0f, -65f, 0f)));
    }

    void DespawnSelectedCharacter() {
        Destroy(bodyRef);
        bodyRef = null;
    }

    public void CheckCharacterName() {
        string potentialName = characterNameInput.text;
        string potentialNameLower = potentialName.ToLower();
        if (potentialName.Length > 12 || potentialName.Length < 3) {
            activatePopupFlag = true;
            popupMessage = "Your character name must be between 3 and 12 characters long!";
            confirmAlertCancelBtn.interactable= true;
            confirmAlertConfirmBtn.interactable = true;
            TriggerBlockScreen(false);
            completeCharCreationFlag = false;
            return;
        }

        if (potentialName.Contains(" ")) {
            activatePopupFlag = true;
            popupMessage = "Your character name must not contain spaces.";
            confirmAlertCancelBtn.interactable = true;
            confirmAlertConfirmBtn.interactable = true;
            TriggerBlockScreen(false);
            completeCharCreationFlag = false;
            return;
        }

        foreach (string word in RegexLibrary.profanityList) {
            if (potentialNameLower.Contains(word)) {
                activatePopupFlag = true;
                popupMessage = "You cannot use this name. Please try another.";
                confirmAlertCancelBtn.interactable = true;
                confirmAlertConfirmBtn.interactable = true;
                TriggerBlockScreen(false);
                completeCharCreationFlag = false;
                return;
            }
        }

        Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
        if (regex.Matches(potentialName).Count == 0) {
            activatePopupFlag = true;
            popupMessage = "Your name must only consist of alphanumeric characters.";
            confirmAlertCancelBtn.interactable = true;
            confirmAlertConfirmBtn.interactable = true;
            TriggerBlockScreen(false);
            completeCharCreationFlag = false;
            return;
        }

        // Check if username is taken
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["usernameLower"] = potentialNameLower;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("checkUsernameTaken");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
			if (taskA.IsFaulted) {
                activatePopupFlag = true;
                TriggerBlockScreen(false);
                popupMessage = "Database is currently unavailable. Please try again later.";
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else if (results["status"].ToString() == "200") {
                activatePopupFlag = true;
                popupMessage = "This username is taken! Please try another.";
                confirmAlertCancelBtn.interactable = true;
                confirmAlertConfirmBtn.interactable = true;
                TriggerBlockScreen(false);
                completeCharCreationFlag = false;
            } else {
                if (!completeCharCreationFlag) {
                    activatePopupFlag = true;
                    popupMessage = "This name is available! You may use this name if you wish.";
                    confirmAlertCancelBtn.interactable = true;
                    confirmAlertConfirmBtn.interactable = true;
                    TriggerBlockScreen(false);
                    completeCharCreationFlag = false;
                } else {
                    func = DAOScript.dao.functions.GetHttpsCallable("setUsernameTaken");
                    inputData.Clear();
                    inputData["callHash"] = DAOScript.functionsCallHash;
                    inputData["usernameLower"] = potentialNameLower;
                    inputData["usernameExact"] = potentialName;
                    func.CallAsync(inputData).ContinueWith((taskB) => {
                        if (taskB.IsFaulted) {
                            activatePopupFlag = true;
                            // popupMessage = "Database is currently unavailable. Please try again later.";
                            TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                            confirmAlertCancelBtn.interactable = true;
                            confirmAlertConfirmBtn.interactable = true;
                            TriggerBlockScreen(false);
                            completeCharCreationFlag = false;
                            return;
                        } else {
                            Dictionary<object, object> results2 = (Dictionary<object, object>)taskB.Result.Data;
                            if (results2["status"].ToString() == "200") {
                                inputData.Clear();
                                inputData["callHash"] = DAOScript.functionsCallHash;
                                inputData["uid"] = AuthScript.authHandler.user.UserId;
                                inputData["username"] = potentialName;
                                inputData["selectedCharacter"] = selectedCharacter;
                                inputData["starterWeapon"] = weaponSelector.GetCurrentItem();
                                func = DAOScript.dao.functions.GetHttpsCallable("createCharacter");
                                func.CallAsync(inputData).ContinueWith((taskC) => {
                                    if (taskC.IsFaulted) {
                                        activatePopupFlag = true;
                                        // popupMessage = "Database is currently unavailable. Please try again later.";
                                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                        confirmAlertCancelBtn.interactable = true;
                                        confirmAlertConfirmBtn.interactable = true;
                                        TriggerBlockScreen(false);
                                        completeCharCreationFlag = false;
                                        return;
                                    } else {
                                        Dictionary<object, object> results3 = (Dictionary<object, object>)taskC.Result.Data;
                                        if (results3["status"].ToString() == "200") {
                                            inputData.Clear();
                                            inputData["callHash"] = DAOScript.functionsCallHash;
                                            inputData["uid"] = AuthScript.authHandler.user.UserId;
                                            Dictionary<string, string>[] starterItems = new Dictionary<string, string>[12];
                                            Dictionary<string, string> item1 = new Dictionary<string, string>();
                                            item1.Add("itemName", weaponSelector.GetCurrentItem());
                                            item1.Add("category", "weapons");
                                            item1.Add("duration", "-1");
                                            starterItems[0] = item1;
                                            Dictionary<string, string> item2 = new Dictionary<string, string>();
                                            item2.Add("itemName", "I32");
                                            item2.Add("category", "weapons");
                                            item2.Add("duration", "-1");
                                            starterItems[1] = item2;
                                            Dictionary<string, string> item3 = new Dictionary<string, string>();
                                            item3.Add("itemName", "N76 Fragmentation");
                                            item3.Add("category", "weapons");
                                            item3.Add("duration", "-1");
                                            starterItems[2] = item3;
                                            Dictionary<string, string> item4 = new Dictionary<string, string>();
                                            item4.Add("itemName", "Recon Knife");
                                            item4.Add("category", "weapons");
                                            item4.Add("duration", "-1");
                                            starterItems[3] = item4;
                                            Dictionary<string, string> item5 = new Dictionary<string, string>();
                                            item5.Add("itemName", selectedCharacter);
                                            item5.Add("category", "characters");
                                            item5.Add("duration", "-1");
                                            starterItems[4] = item5;
                                            Dictionary<string, string> item6 = new Dictionary<string, string>();
                                            item6.Add("itemName", "Standard Fatigues Top (M)");
                                            item6.Add("category", "tops");
                                            item6.Add("duration", "-1");
                                            starterItems[5] = item6;
                                            Dictionary<string, string> item7 = new Dictionary<string, string>();
                                            item7.Add("itemName", "Standard Fatigues Top (F)");
                                            item7.Add("category", "tops");
                                            item7.Add("duration", "-1");
                                            starterItems[6] = item7;
                                            Dictionary<string, string> item8 = new Dictionary<string, string>();
                                            item8.Add("itemName", "Standard Fatigues Bottom (M)");
                                            item8.Add("category", "bottoms");
                                            item8.Add("duration", "-1");
                                            starterItems[7] = item8;
                                            Dictionary<string, string> item9 = new Dictionary<string, string>();
                                            item9.Add("itemName", "Standard Fatigues Bottom (F)");
                                            item9.Add("category", "bottoms");
                                            item9.Add("duration", "-1");
                                            starterItems[8] = item9;
                                            Dictionary<string, string> item10 = new Dictionary<string, string>();
                                            item10.Add("itemName", "Standard Boots (M)");
                                            item10.Add("category", "footwear");
                                            item10.Add("duration", "-1");
                                            starterItems[9] = item10;
                                            Dictionary<string, string> item11 = new Dictionary<string, string>();
                                            item11.Add("itemName", "Standard Boots (F)");
                                            item11.Add("category", "footwear");
                                            item11.Add("duration", "-1");
                                            starterItems[10] = item11;
                                            Dictionary<string, string> item12 = new Dictionary<string, string>();
                                            item12.Add("itemName", "Standard Suppressor");
                                            item12.Add("category", "mods");
                                            starterItems[11] = item12;
                                            inputData["items"] = starterItems;
                                            func = DAOScript.dao.functions.GetHttpsCallable("giveItemsToUser");
                                            func.CallAsync(inputData).ContinueWith((taskD) => {
                                                if (taskD.IsFaulted) {
                                                    TriggerBlockScreen(false);
                                                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                                } else {
                                                    Dictionary<object, object> results4 = (Dictionary<object, object>)taskD.Result.Data;
                                                    if (results4["status"].ToString() == "200") {
                                                        finishedFlag = true;
                                                    } else {
                                                        TriggerBlockScreen(false);
                                                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                                    }
                                                }
                                            });
                                        } else {
                                            TriggerBlockScreen(false);
                                            TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                        }
                                    }
                                });
                            } else {
                                TriggerBlockScreen(false);
                                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                            }
                        }
                    });
                }
            }
		});
    }

    public void OnCheckCharacterNameClick() {
        CheckCharacterName();
    }

    public void ConfirmCharacterCreation() {
        completeCharCreationFlag = true;
        string finalCharacterName = characterNameInput.text;
        ToggleInputs(false);

        QueueConfirmPopup("Are you sure you wish to proceed with this name and character? It cannot be changed later.");
    }

    void ToggleInputs(bool b) {
        weaponSelector.ToggleSelectorButtons(b);
        characterNameInput.interactable = b;
        proceedBtn.interactable = b;
        checkBtn.interactable = b;
    }

    public void EnableInputs() {
        ToggleInputs(true);
    }

    public void CompleteCharacterCreation() {
        confirmAlertCancelBtn.interactable = false;
        confirmAlertConfirmBtn.interactable = false;
        TriggerBlockScreen(true);
        CheckCharacterName();
    }

    void ClosePopup() {
        popupMessage = "";
        ToggleInputs(true);
    }

    public void OnConfirmButtonClicked() {
        CompleteCharacterCreation();
    }

    public void OnCancelButtonClicked() {
        ClosePopup();
    }

    void TriggerPopup() {
        popupAlert.SetText(popupMessage);
        popupAlert.ModalWindowIn();
        blurManager.BlurInAnim();
    }

    void TriggerConfirmPopup() {
        confirmAlert.SetText(popupMessage);
        confirmAlert.ModalWindowIn();
        blurManager.BlurInAnim();
    }

    void QueueConfirmPopup(string message) {
        activateConfirmFlag = true;
        popupMessage = message;
    }

    public void QueueAlertPopup(string message) {
        activatePopupFlag = true;
        popupMessage = message;
    }

        // Only called in an emergency situation when the game needs to exit immediately (ex: database failure or user gets banned).
    public void TriggerEmergencyExit(string message) {
        // Freeze user mouse input
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Display emergency popup
        QueueAlertPopup("A fatal error has occurred:\n" + message + "\nThe game will now close.");

        StartCoroutine("EmergencyExitGame");
    }

    IEnumerator EmergencyExitGame() {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    void EquipSelectedWeapon() {
        WeaponScript ws = bodyRef.GetComponent<WeaponScript>();
        ws.EquipWeaponForSetup(weaponSelector.GetCurrentItem(), selectedCharacter);
    }

    public void SelectNextWeapon() {
        if (previousWeaponIndex == weaponSelector.index) {
            return;
        }
        previousWeaponIndex = weaponSelector.index;
        EquipSelectedWeapon();
    }

    public void SelectPreviousWeapon() {
        if (previousWeaponIndex == weaponSelector.index) {
            return;
        }
        previousWeaponIndex = weaponSelector.index;
        EquipSelectedWeapon();
    }

    public void TriggerBlockScreen(bool b) {
		blockScreenTrigger = b;
	}

	void ToggleBlockScreen(bool b) {
		blockScreen.SetActive(b);
		blockBlur.SetActive(b);
	}

}
