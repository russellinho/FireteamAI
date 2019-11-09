using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpActionCockBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state

    private WeaponActionScript was;
    private bool shellCasingFired;
    bool cockSoundPlayed;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        shellCasingFired = false;
        cockSoundPlayed = false;
        was = animator.GetComponentInParent<WeaponActionScript>();
        was.FpcCockShotgun();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!cockSoundPlayed && was.weaponStats.reloadSound2Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound2Time) {
            cockSoundPlayed = true;
            was.PlayReloadSound(1);
        }
       if (stateInfo.normalizedTime >= 0.5f) {
           was.isCocking = false;
       }
       if (!shellCasingFired && stateInfo.normalizedTime >= 0.2f) {
           was.SpawnShellCasing();
           shellCasingFired = true;
       }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        was.isCocking = false;
		was.isReloading = false;
    }

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
