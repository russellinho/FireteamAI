using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SetupControllerScript : MonoBehaviour
{
    private Vector3 bodyPos = new Vector3(-6f, 2.74f, 13.2f);
    public GameObject bodyRef;
    public GameObject contentInventory;
    private ArrayList starterCharacters = new ArrayList(){"Lucas", "Daryl", "Hana", "Jade"};
    private Dictionary<string, Character> characterCatalog = new Dictionary<string, Character>();
    private string selectedCharacter;
    public GameObject selectedPrefab;
    public GameObject contentPrefab;
    public GameObject characterDesc;
    // Start is called before the first frame update
    void Start()
    {
        characterCatalog.Add("Lucas", new Character("Lucas", 'M', "Models/Characters/Lucas/PlayerPrefabLucas", "Models/FirstPersonPrefabs/Characters/Lucas/1/lucas_all_arms", "", "Models/Pics/character_lucas", "Nationality: British\nAs a reformed professional criminal, Lucas works swiftly and gets the job done.", new string[]{"Models/Characters/Lucas/Extra Skins/Ankles Long Sleeves/lucasskinanklesonly", "Models/Characters/Lucas/Extra Skins/Ankles Mid Sleeves/lucasanklesmid", "Models/Characters/Lucas/Extra Skins/Ankles Short Sleeves/lucasanklesshortsleeve"}, null, null));
        characterCatalog.Add("Daryl", new Character("Daryl", 'M', "Models/Characters/Daryl/PlayerPrefabDaryl", "Models/FirstPersonPrefabs/Characters/Daryl/1/daryl_all_arms", "", "Models/Pics/character_daryl", "Nationality: American\nDaryl was an ex professional college football player whose career ended abruptly after an unsustainable knee injury. His tenacity, size, and strength all serve him in combat.", new string[]{"Models/Characters/Daryl/1/skindonald1", "Models/Characters/Daryl/2/skindonald2", "Models/Characters/Daryl/3/skindonald3"}, null, null));
        characterCatalog.Add("Hana", new Character("Hana", 'F', "Models/Characters/Hana/PlayerPrefabHana", "Models/FirstPersonPrefabs/Characters/Hana/1/hana_all_arms", "", "Models/Pics/character_hana", "Nationality: Japanese\nWhen her entire family was murdered as a kid, Hana swore to fight for justice to avenge her family. She is an ex police officer who many underestimate, but don't be fooled by her size.", new string[]{"Models/Characters/Hana/1/skinhana1", "Models/Characters/Hana/2/skinhana2", "Models/Characters/Hana/3/skinhana3"}, null, null));
        characterCatalog.Add("Jade", new Character("Jade", 'F', "Models/Characters/Jade/PlayerPrefabJade", "Models/FirstPersonPrefabs/Characters/Jade/1/jade_all_arms", "", "Models/Pics/character_jade", "Nationality: American\nNot much is known about Jade's past besides the fact that she likes to work alone and was previously a firefighter.", new string[]{"Models/Characters/Jade/1/skinjade1", "Models/Characters/Jade/3/skinjade3", "Models/Characters/Jade/2/skinjade2"}, null, null));
        selectedCharacter = "Lucas";
        SpawnSelectedCharacter();
        InitializeCharacterSelection();
    }

    void InitializeCharacterSelection() {
        // Populate into grid layout
		for (int i = 0; i < starterCharacters.Count; i++) {
			string thisCharacterName = (string)starterCharacters[i];
			Character c = characterCatalog[thisCharacterName];
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
        bodyRef = Instantiate((GameObject)Resources.Load(characterCatalog[selectedCharacter].prefabPath), bodyPos, Quaternion.Euler(new Vector3(0f, 327f, 0f)));
    }

    void DespawnSelectedCharacter() {
        Destroy(bodyRef);
        bodyRef = null;
    }

    public void CheckCharacterName() {
        string regex = @"";
    }

    public void CompleteCharacterCreation() {

    }

}
