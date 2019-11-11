using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpActionReloadBehavior : StateMachineBehaviour
{
    private WeaponActionScript weaponActionScript;
    bool loadSoundPlayed;
    private bool done;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("CockShotgun");
        done = false;
        weaponActionScript = animator.GetComponentInParent<WeaponActionScript>();
        weaponActionScript.isReloading = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= weaponActionScript.weaponStats.reloadSound1Time - 0.1f && stateInfo.normalizedTime <= weaponActionScript.weaponStats.reloadSound1Time + 0.1f) {
            if (!loadSoundPlayed) {
                weaponActionScript.PlayReloadSound(0);
                loadSoundPlayed = true;
            }
        } else {
            loadSoundPlayed = false;
        }
        if (!done && (weaponActionScript.currentAmmo >= weaponActionScript.weaponStats.clipCapacity || Input.GetButtonDown("Fire1"))) {
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
