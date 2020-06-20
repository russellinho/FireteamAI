using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class SetupControllerScript : MonoBehaviour
{
    private Vector3 bodyPos = new Vector3(-6f, 2.74f, 13.2f);
    public GameObject bodyRef;
    public GameObject contentInventory;
    public InputField characterNameInput;
    private Dictionary<string, int> starterCharacters = new Dictionary<string, int>(){["Lucas"] = 0, ["Daryl"] = 1, ["Yongjin"] = 2, ["Rocko"] = 3, ["Hana"] = 4, ["Jade"] = 5, ["Dani"] = 6};
    public ArrayList starterWeapons = new ArrayList(){"M4A1", "AK-47"};
    private string selectedCharacter;
    public GameObject selectedPrefab;
    public GameObject contentPrefab;
    public GameObject selectionDesc;
    public GameObject popupAlert;
    public GameObject confirmAlert;
    public Button confirmAlertConfirmBtn;
    public Button confirmAlertCancelBtn;
    public Button proceedBtn;
    public Button checkBtn;
    public Button nextWepBtn;
    public Button prevWepBtn;
    public Image wepPnl;
    public Text wepTxt;
    public short wepSelectionIndex;
    public Text popupAlertTxt;
    public Text confirmAlertTxt;
    private bool activatePopupFlag;
    private bool activateConfirmFlag;
    private bool finishedFlag;
    private bool completeCharCreationFlag;
    private string popupMessage;
    public GameObject emergencyPopup;
    public Text emergencyPopupTxt;
    public GameObject[] starterCharacterRefs;
    // Start is called before the first frame update
    void Start()
    {
        wepSelectionIndex = 0;
        SetSelectedWeaponText();
        selectedCharacter = "Lucas";
        SpawnSelectedCharacter();
        EquipSelectedWeapon();
        InitializeCharacterSelection();
    }

    void Update()
    {
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
			s.thumbnailRef.SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (qq.Value == 0) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				s.equippedInd.enabled = true;
				selectedPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
            s.setupController = this;
		}
    }

    void SetSelectedWeaponText() {
        wepTxt.text = (string)starterWeapons[wepSelectionIndex];
    }

    private void DeselectCharacter() {
        selectedCharacter = "";
        selectedPrefab.GetComponentsInChildren<Image>()[0].color = Color.white;
        selectedPrefab.GetComponent<SetupItemScript>().equippedInd.enabled = false;
        selectedPrefab = null;
        DespawnSelectedCharacter();
    }

    public void SelectCharacter(GameObject setupItem, string name) {
        DeselectCharacter();
        selectedCharacter = name;
        setupItem.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
        setupItem.GetComponent<SetupItemScript>().equippedInd.enabled = true;
        selectedPrefab = setupItem;
        SpawnSelectedCharacter();
        EquipSelectedWeapon();
    }

    void SpawnSelectedCharacter() {
        bodyRef = Instantiate(starterCharacterRefs[starterCharacters[selectedCharacter]], bodyPos, Quaternion.Euler(new Vector3(0f, 327f, 0f)));
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
            completeCharCreationFlag = false;
            return;
        }

        if (potentialName.Contains(" ")) {
            activatePopupFlag = true;
            popupMessage = "Your character name must not contain spaces.";
            completeCharCreationFlag = false;
            return;
        }

        foreach (string word in RegexLibrary.profanityList) {
            if (potentialNameLower.Contains(word)) {
                activatePopupFlag = true;
                popupMessage = "You cannot use this name. Please try another.";
                completeCharCreationFlag = false;
                return;
            }
        }

        Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
        if (regex.Matches(potentialName).Count == 0) {
            activatePopupFlag = true;
            popupMessage = "Your name must only consist of alphanumeric characters.";
            completeCharCreationFlag = false;
            return;
        }

        // Check if username is taken
        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_takenUsernames").GetValueAsync().ContinueWith(taskA => {
            if (taskA.IsFaulted) {
                activatePopupFlag = true;
                popupMessage = "Database is currently unavailable. Please try again later.";
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else if (taskA.IsCompleted) {
                if (taskA.Result.HasChild(potentialNameLower)) {
                    activatePopupFlag = true;
                    popupMessage = "This username is taken! Please try another.";
                    completeCharCreationFlag = false;
                } else {
                    if (!completeCharCreationFlag) {
                        activatePopupFlag = true;
                        popupMessage = "This name is available! You may use this name if you wish.";
                        completeCharCreationFlag = false;
                    } else {
                        // Everything is passed, create player data and mark username as taken
                        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_takenUsernames").Child(potentialNameLower).SetValueAsync("true").ContinueWith(taskB => {
                            if (taskB.IsFaulted) {
                                activatePopupFlag = true;
                                // popupMessage = "Database is currently unavailable. Please try again later.";
                                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                completeCharCreationFlag = false;
                                return;
                            } else if (taskB.IsCompleted) {
                                string json = "{" +
                                    "\"username\":\"" + potentialName + "\"," +
                                    "\"defaultChar\":\"" + selectedCharacter + "\"," +
                                    "\"defaultWeapon\":\"" + starterWeapons[wepSelectionIndex] + "\"," +
                                    "\"exp\":\"0\"," +
                                    "\"gp\":\"100000\"," +
                                    "\"kash\":\"0\"" +
                                "}";
                                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).SetRawJsonValueAsync(json).ContinueWith(taskE => {
                                    if (taskE.IsFaulted) {
                                        activatePopupFlag = true;
                                        // popupMessage = "Database is currently unavailable. Please try again later.";
                                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                        completeCharCreationFlag = false;
                                        return;
                                    } else if (taskE.IsCompleted) {
                                        string jsonA = "{\"weapons\":{" +
                                        "\"" + starterWeapons[wepSelectionIndex] + "\": {" +
                                            "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            "\"duration\":\"-1\"," +
                                            "\"equippedSuppressor\":\"\"," +
                                            "\"equippedSight\":\"\"," +
                                            "\"equippedClip\":\"\""
                                        + "}," +
                                        "\"Glock23\": {" +
                                            "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            "\"duration\":\"-1\"," +
                                            "\"equippedSuppressor\":\"\"," +
                                            "\"equippedSight\":\"\"," +
                                            "\"equippedClip\":\"\""
                                        + "}," + 
                                        "\"M67 Frag\": {" +
                                            "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            "\"duration\":\"-1\"," +
                                            "\"equippedSuppressor\":\"\"," +
                                            "\"equippedSight\":\"\"," +
                                            "\"equippedClip\":\"\""
                                        + "}" + 
                                        "\"Recon Knife\": {" +
                                            "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            "\"duration\":\"-1\""
                                        + "}" + 
                                        "}," +
                                        "\"characters\":{" +
                                            "\"" + selectedCharacter + "\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}" +
                                        "}," +
                                        // "\"armor\":{" +
                                        //     "\"Standard Vest\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}" +
                                        // "}," +
                                        "\"tops\":{" +
                                            // "\"Casual T-Shirt (M)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Casual T-Shirt (F)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Casual Shirt\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Casual Tank Top\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            "\"Standard Fatigues Top (M)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}," +
                                            "\"Standard Fatigues Top (F)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}" +
                                        "}," +
                                        "\"bottoms\":{" +
                                            // "\"Dark Wash Denim Jeans (M)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Dark Wash Denim Jeans (F)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Light Wash Denim Jeans (M)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Light Wash Denim Jeans (F)\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            "\"Standard Fatigues Bottom (M)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}," +
                                            "\"Standard Fatigues Bottom (F)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}" +
                                        "}," +
                                        "\"footwear\":{" +
                                            // "\"White Chucks\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            // "\"Red Chucks\": {" +
                                            //     "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                            //     "\"duration\":\"-1\""
                                            // + "}," +
                                            "\"Standard Boots (M)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}," +
                                            "\"Standard Boots (F)\": {" +
                                                "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                "\"duration\":\"-1\""
                                            + "}" +
                                        "}," +
                                        // "\"headgear\":{" +
                                        //     "\"COM Hat\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}," +
                                        //     "\"MICH\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}," +
                                        //     "\"Combat Beanie\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}" +
                                        // "}," +
                                        // "\"facewear\":{" +
                                        //     "\"Standard Goggles\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}," +
                                        //     "\"Sport Shades\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}," +
                                        //     "\"Aviators\": {" +
                                        //         "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                        //         "\"duration\":\"-1\""
                                        //     + "}" +
                                        // "}" +
                                        "}";
                                        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).SetRawJsonValueAsync(jsonA).ContinueWith(taskC => {
                                            if (taskC.IsFaulted) {
                                                // Debug.Log(taskC.Exception.Message);
                                                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                            } else if (taskC.IsCompleted) {
                                                string jsonTemp = "{" +
                                                    "\"name\":\"Standard Suppressor\"," +
                                                    "\"equippedOn\":\"\"," +
                                                    "\"acquireDate\":\"" + DateTime.Now + "\"," +
                                                    "\"duration\":\"-1\"" +
                                                "}";
                                                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Push().SetRawJsonValueAsync(jsonTemp).ContinueWith(taskD => {
                                                    if (taskD.IsFaulted) {
                                                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                                                    } else if (taskD.IsCompleted) {
                                                        finishedFlag = true;
                                                    }
                                                });
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }
                }
            }
        });
    }

    public void OnCheckCharacterNameClick() {
        if (popupAlert.activeInHierarchy || confirmAlert.activeInHierarchy) {
            return;
        }

        CheckCharacterName();
    }

    public void ConfirmCharacterCreation() {
        if (popupAlert.activeInHierarchy || confirmAlert.activeInHierarchy) {
            return;
        }

        completeCharCreationFlag = true;
        string finalCharacterName = characterNameInput.text;
        ToggleInputs(false);

        QueueConfirmPopup("Are you sure you wish to proceed with this name and character? It cannot be changed later.");
    }

    void ToggleInputs(bool b) {
        nextWepBtn.interactable = b;
        prevWepBtn.interactable = b;
        characterNameInput.interactable = b;
        proceedBtn.interactable = b;
        checkBtn.interactable = b;
    }

    public void CompleteCharacterCreation() {
        confirmAlertCancelBtn.interactable = false;
        confirmAlertConfirmBtn.interactable = false;
        CheckCharacterName();
    }

    public void ClosePopup() {
        confirmAlert.SetActive(false);
        popupAlert.SetActive(false);
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
        popupAlertTxt.text = popupMessage;
        popupAlert.SetActive(true);
    }

    void TriggerConfirmPopup() {
        confirmAlertTxt.text = popupMessage;
        confirmAlert.SetActive(true);
    }

    void QueueConfirmPopup(string message) {
        activateConfirmFlag = true;
        popupMessage = message;
    }

    public void TriggerEmergencyPopup(string message) {
        emergencyPopupTxt.text = message;
        emergencyPopup.SetActive(true);
    }

    public void CloseEmergencyPopup() {
        emergencyPopup.SetActive(false);
    }

        // Only called in an emergency situation when the game needs to exit immediately (ex: database failure or user gets banned).
    public void TriggerEmergencyExit(string message) {
        // Freeze user mouse input
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Display emergency popup
        TriggerEmergencyPopup("A fatal error has occurred:\n" + message + "\nThe game will now close.");

        StartCoroutine("EmergencyExitGame");
    }

    IEnumerator EmergencyExitGame() {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    void EquipSelectedWeapon() {
        WeaponScript ws = bodyRef.GetComponent<WeaponScript>();
        ws.EquipWeaponForSetup((string)starterWeapons[wepSelectionIndex], selectedCharacter);
    }

    public void SelectNextWeapon() {
        wepSelectionIndex++;
        if (wepSelectionIndex >= starterWeapons.Count) {
            wepSelectionIndex = 0;
        }
        SetSelectedWeaponText();
        EquipSelectedWeapon();
    }

    public void SelectPreviousWeapon() {
        wepSelectionIndex--;
        if (wepSelectionIndex < 0) {
            wepSelectionIndex = (short)(starterWeapons.Count - 1);
        }
        SetSelectedWeaponText();
        EquipSelectedWeapon();
    }

}
