using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveEntryScript : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;

    public void SetObjectiveText(string s) {
        objectiveText.text = s;
    }
}
