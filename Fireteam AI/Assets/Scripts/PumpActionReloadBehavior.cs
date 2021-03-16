using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpActionReloadBehavior : StateMachineBehaviour
{
    private const float BEGIN_TIME = 0.1f;
    private const float END_TIME = 0.7f;
    private WeaponActionScript weaponActionScript;
    private bool done;
    private bool reloadedThisFrame;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // reloadTimer = RELOAD_INTERVAL;
        animator.ResetTrigger("CockShotgun");
        done = false;
        weaponActionScript = animator.GetComponentInParent<WeaponActionScript>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        weaponActionScript.isReloading = true;
        float frameProgress = stateInfo.normalizedTime - (float)((int)stateInfo.normalizedTime);
        if (frameProgress <= BEGIN_TIME) {
            reloadedThisFrame = false;
        } else if (frameProgress >= END_TIME) {
            if (!reloadedThisFrame) {
                weaponActionScript.ReloadShotgun();
                weaponActionScript.PlayReloadSound(0);
                reloadedThisFrame = true;
            }
        }
        if (!done && weaponActionScript.currentAmmo > 0 && (weaponActionScript.currentAmmo >= weaponActionScript.weaponStats.clipCapacity || PlayerPreferences.playerPreferences.KeyWasPressed("Fire") || weaponActionScript.totalAmmoLeft <= 0)) {
            animator.SetTrigger("CockShotgun");
            done = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
