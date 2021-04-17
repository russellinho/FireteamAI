using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InsertionPoint : MonoBehaviour
{
    public int index;
    private bool isSelected;
    
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
