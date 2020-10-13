using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;

public class UiTest : MonoBehaviour
{
    public HUDContainer container;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (container.pauseMenuManager.pauseActive)
            {
                container.pauseMenuManager.ClosePause();
            }
            else
            {
                container.pauseMenuManager.OpenPause();
            }
        }
    }
}
