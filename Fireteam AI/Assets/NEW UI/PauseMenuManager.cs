using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;

public class PauseMenuManager : MonoBehaviour
{
    public bool pauseActive;
    public Animator anim;
    public BlurManager blurManager;

    public void OpenPause() {
        anim.Play("Window In");
        blurManager.BlurInAnim();
        pauseActive = true;
    }

    public void ClosePause() {
        anim.Play("Window Out");
        blurManager.BlurOutAnim();
        pauseActive = false;
    }
}
