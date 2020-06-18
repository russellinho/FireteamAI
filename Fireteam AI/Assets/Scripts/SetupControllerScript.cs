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
        DatabaseReference d = DAOScript.dao.dbRef;
        d.RunTransaction(task => {
            if (task.Child("fteam_ai_takenUsernames").HasChild(potentialNameLower)) {
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
                    task.Child("fteam_ai_takenUsernames").Child(potentialNameLower).Value = "true";
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("username").Value = potentialName;
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("defaultChar").Value = selectedCharacter;
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("defaultWeapon").Value = ""+starterWeapons[wepSelectionIndex];
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("exp").Value = "0";
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("gp").Value = "100000";
                    task.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).Child("kash").Value = "0";
                    string iPrimary = ""+starterWeapons[wepSelectionIndex];
                    string iSecondary = "Glock23";
                    string iSupport = "M67 Frag";
                    string iMelee = "Recon Knife";
                    // Add primary
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iPrimary).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iPrimary).Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iPrimary).Child("equippedSuppressor").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iPrimary).Child("equippedSight").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iPrimary).Child("equippedClip").Value = "";
                    // Add secondary
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSecondary).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSecondary).Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSecondary).Child("equippedSuppressor").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSecondary).Child("equippedSight").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSecondary).Child("equippedClip").Value = "";
                    // Add support
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSupport).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSupport).Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSupport).Child("equippedSuppressor").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSupport).Child("equippedSight").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iSupport).Child("equippedClip").Value = "";
                    // Add melee
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iMelee).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("weapons").Child(iMelee).Child("duration").Value = "-1";
                    // Add characters
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters").Child(selectedCharacter).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("characters").Child(selectedCharacter).Child("duration").Value = "-1";
                    // Add tops
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child("Standard Fatigues Top (M)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child("Standard Fatigues Top (M)").Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child("Standard Fatigues Top (F)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("tops").Child("Standard Fatigues Top (F)").Child("duration").Value = "-1";
                    // Add bottoms
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Fatigues Bottom (M)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Fatigues Bottom (M)").Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Fatigues Bottom (F)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Fatigues Bottom (F)").Child("duration").Value = "-1";
                    // Add footwear
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Boots (M)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Boots (M)").Child("duration").Value = "-1";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Boots (F)").Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("bottoms").Child("Standard Boots (F)").Child("duration").Value = "-1";
                    // Add mods
                    string pushKey = DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Push().Key;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("name").Value = "Standard Suppressor";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("equippedOn").Value = "";
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("acquireDate").Value = ""+DateTime.Now;
                    task.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId).Child("mods").Child(pushKey).Child("duration").Value = "-1";
                    // Continue
                    finishedFlag = true;
                }
            }

            return TransactionResult.Success(task);
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
