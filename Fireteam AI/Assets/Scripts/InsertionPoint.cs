using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InsertionPoint : MonoBehaviour
{
    public int index;
    private bool isSelected;
    public TextMeshProUGUI teamMembers;

    public void SetTeamMembers(string s)
    {
        teamMembers.text = s;
    }
    
    public void DeselectButton()
    {
        isSelected = false;
    }

    public void SelectButton()
    {
        isSelected = true;
        gameObject.GetComponent<Button>().Select();
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
