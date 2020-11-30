using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;

public class PauseMenuManager : MonoBehaviour
{
    public bool pauseActive;
    public Animator anim;
    public BlurManager blurManager;
    public CanvasGroup mainPauseCanvas;

    public void OpenPause() {
        GetComponent<PauseMenuScript>().SetCurrentPanel("Main");
        anim.Play("Window In");
        blurManager.BlurInAnim();
        pauseActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePause() {
        if (mainPauseCanvas.alpha == 1f) {
            anim.Play("Window Out");
            blurManager.BlurOutAnim();
            pauseActive = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
