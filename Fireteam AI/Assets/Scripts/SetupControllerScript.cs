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
    private ArrayList starterCharacters = new ArrayList(){"Lucas", "Daryl", "Hana", "Jade"};
    private string selectedCharacter;
    public GameObject selectedPrefab;
    public GameObject contentPrefab;
    public GameObject characterDesc;
    public GameObject popupAlert;
    public GameObject confirmAlert;
    public Button confirmAlertConfirmBtn;
    public Button confirmAlertCancelBtn;
    public Button proceedBtn;
    public Button checkBtn;
    public Text popupAlertTxt;
    public Text confirmAlertTxt;
    private bool activatePopupFlag;
    private bool activateConfirmFlag;
    private string popupMessage;
    // Start is called before the first frame update
    void Start()
    {
        selectedCharacter = "Lucas";
        SpawnSelectedCharacter();
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
        }
    }

    void InitializeCharacterSelection() {
        // Populate into grid layout
		for (int i = 0; i < starterCharacters.Count; i++) {
			string thisCharacterName = (string)starterCharacters[i];
			Character c = InventoryScript.itemData.characterCatalog[thisCharacterName];
			GameObject o = Instantiate(contentPrefab);
            SetupItemScript s = o.GetComponent<SetupItemScript>();
			s.itemDescriptionPopupRef = characterDesc;
			s.characterDetails = c;
			s.itemName = thisCharacterName;
			s.itemDescription = c.description;
			o.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(c.thumbnailPath);
			o.GetComponentInChildren<RawImage>().SetNativeSize();
			RectTransform t = o.GetComponentsInChildren<RectTransform>()[3];
			t.sizeDelta = new Vector2(t.sizeDelta.x / 2f, t.sizeDelta.y / 2f);
			if (i == 0) {
				o.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
				s.equippedInd.enabled = true;
				selectedPrefab = o;
			}
			o.transform.SetParent(contentInventory.transform);
            s.setupController = this;
		}
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
    }

    void SpawnSelectedCharacter() {
        bodyRef = Instantiate((GameObject)Resources.Load(InventoryScript.itemData.characterCatalog[selectedCharacter].prefabPath), bodyPos, Quaternion.Euler(new Vector3(0f, 327f, 0f)));
    }

    void DespawnSelectedCharacter() {
        Destroy(bodyRef);
        bodyRef = null;
    }

    public short CheckCharacterName() {
        string potentialName = characterNameInput.text;
        if (potentialName.Length > 12 || potentialName.Length < 3) {
            QueuePopup("Your character name must be between 3 and 12 characters long!");
            return 0;
        }

        if (potentialName.Contains(" ")) {
            QueuePopup("Your character name must not contain spaces.");
            return 0;
        }

        foreach (string word in RegexLibrary.profanityList) {
            if (potentialName.Contains(word)) {
                QueuePopup("You cannot use this name. Please try another.");
                return 0;
            }
        }

        Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
        if (regex.Matches(potentialName).Count > 0) {
            QueuePopup("Your name must only consist of alphanumeric characters.");
            return 0;
        }

        // Check if username is taken
        short status = 0;
        DAOScript.dao.dbRef.Child("fteam_ai_takenUsernames").GetValueAsync().ContinueWith(taskA => {
            if (taskA.IsFaulted) {
                QueuePopup("Database is currently unavailable. Please try again later.\nError: " + taskA.Exception);
                status = 1;
            } else if (taskA.IsCompleted) {
                if (!taskA.Result.HasChild(potentialName)) {
                    QueuePopup("This username is taken! Please try another.");
                    status = 1;
                } else {
                    if (proceedBtn.interactable) {
                        QueuePopup("This name is available!");
                        status = 2;
                    }
                }
            }
        });

        while (status == 0);

        if (status == 1) {
            return 0;
        }

        return 1;
    }

    public void ConfirmCharacterCreation() {
        QueueConfirmPopup("Are you sure you wish to proceed with this name and character? It cannot be changed later.");
    }

    public void CompleteCharacterCreation() {
        string finalCharacterName = characterNameInput.text;
        proceedBtn.interactable = false;
        checkBtn.interactable = false;

        short finalNamePass = 2;
        finalNamePass = CheckCharacterName();
        bool semPhore = false;

        while (!semPhore) {
            if (finalNamePass != 2) {
                semPhore = true;
            }
        }

        if (finalNamePass == 0) {
            QueuePopup("Your name is ineligible. Please check your character name and try again.");
            proceedBtn.interactable = true;
            checkBtn.interactable = true;
        }

        // Everything is passed, create player data and mark username as taken
        DAOScript.dao.dbRef.Child("fteam_ai_takenUsernames").Child(finalCharacterName).SetValueAsync("true").ContinueWith(taskA => {
            if (taskA.IsFaulted) {
                QueuePopup("Database is currently unavailable. Please try again later.\nError: " + taskA.Exception);
                proceedBtn.interactable = true;
                checkBtn.interactable = true;
                return;
            } else if (taskA.IsCompleted) {
                string json = "{'username':'" + finalCharacterName + "','defaultChar':'" + selectedCharacter + "'}";
                DAOScript.dao.dbRef.Child("fteam_ai_users").Child(AuthScript.authHandler.user.UserId).SetRawJsonValueAsync(json).ContinueWith(taskB => {
                    if (taskB.IsFaulted) {
                        QueuePopup("Database is currently unavailable. Please try again later.\nError: " + taskA.Exception);
                        proceedBtn.interactable = true;
                        checkBtn.interactable = true;
                        
                        return;
                    } else if (taskB.IsCompleted) {
                        json = "{'weapons':{" +
                        "'M4A1': {" +
                            "'acquireDate':'" + DateTime.Now + "'," +
                            "'duration':'-1'," +
                            "'equippedSuppressor':''," +
                            "'equippedSight':''," +
                            "'equippedClip':''"
                        + "}," +
                        "'AK-47': {" +
                            "'acquireDate':'" + DateTime.Now + "'," +
                            "'duration':'-1'," +
                            "'equippedSuppressor':''," +
                            "'equippedSight':''," +
                            "'equippedClip':''"
                        + "}," + 
                        "'Glock23': {" +
                            "'acquireDate':'" + DateTime.Now + "'," +
                            "'duration':'-1'," +
                            "'equippedSuppressor':''," +
                            "'equippedSight':''," +
                            "'equippedClip':''"
                        + "}," + 
                        "'M67 Frag': {" +
                            "'acquireDate':'" + DateTime.Now + "'," +
                            "'duration':'-1'," +
                            "'equippedSuppressor':''," +
                            "'equippedSight':''," +
                            "'equippedClip':''"
                        + "}" + 
                        "}," +
                        "'armor':{" +
                            "'Standard Vest': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}," +
                        "'tops':{" +
                            "'Casual T-Shirt (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Casual T-Shirt (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Casual Shirt': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Fatigues Top (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Fatigues Top (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Casual Tank Top': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}," +
                        "'bottoms':{" +
                            "'Dark Wash Denim Jeans (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Dark Wash Denim Jeans (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Light Wash Denim Jeans (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Light Wash Denim Jeans (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Fatigues Bottom (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Fatigues Bottom (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}," +
                        "'footwear':{" +
                            "'White Chucks': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Red Chucks': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Boots (M)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Standard Boots (F)': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}," +
                        "'headgear':{" +
                            "'COM Hat': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'MICH': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Combat Beanie': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}," +
                        "'facewear':{" +
                            "'Standard Goggles': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Sport Shades': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}," +
                            "'Aviators': {" +
                                "'acquireDate':'" + DateTime.Now + "'," +
                                "'duration':'-1'"
                            + "}" +
                        "}" +
                        "}";
                        DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                            .SetRawJsonValueAsync(json).ContinueWith(taskC => {
                                string jsonTemp = "{" +
                                    "'name':'Standard Suppressor'," +
                                    "'equippedOn':''," +
                                    "'acquireDate':'" + DateTime.Now + "'," +
                                    "'duration':'-1'" +
                                "}";
                                DAOScript.dao.dbRef.Child("fteam_ai_inventory").Child(AuthScript.authHandler.user.UserId)
                                    .Child("mods").Push().SetRawJsonValueAsync(jsonTemp).ContinueWith(taskD => {
                                        // Continue to home screen
                                        // TODO: Uncomment once testing is done
                                        // SceneManager.LoadScene("Title");
                                    });
                            });
                    }
                });
            }
        });
    }

    public void ClosePopup() {
        confirmAlert.SetActive(false);
        popupAlert.SetActive(false);
        popupMessage = "";
    }

    public void OnConfirmButtonClicked() {
        ClosePopup();
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

    void QueuePopup(string message) {
        ClosePopup();
        activatePopupFlag = true;
        popupMessage = message;
    }

    void QueueConfirmPopup(string message) {
        ClosePopup();
        activateConfirmFlag = true;
        popupMessage = message;
    }

}
