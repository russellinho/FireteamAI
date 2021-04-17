using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InsertionPoint : MonoBehaviour
{
    public int index;
    private bool isSelected;
    
    public void DeselectButton()
    {
        isSelected = false;
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
    }

    public void SelectButton(bool highlight)
    {
        isSelected = true;
        if (highlight) EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(gameObject);
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
