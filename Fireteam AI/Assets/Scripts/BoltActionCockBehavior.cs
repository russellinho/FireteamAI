using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltActionCockBehavior : StateMachineBehaviour
{
    private WeaponActionScript was;
    private WeaponScript ws;
    private bool shellCasingFired;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        shellCasingFired = false;
        ws = animator.GetComponentInParent<WeaponScript>();
        ws.weaponHolderFpc.SwitchWeaponToLeftHand();
        was = animator.GetComponentInParent<WeaponActionScript>();
        was.FpcCockBoltAction();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= 0.5f) {
           was.isCocking = false;
           if (was.isReloading) {
               was.Reload();
               was.isReloading = false;
           }
        }
       if (!shellCasingFired && stateInfo.normalizedTime >= 0.4f) {
           was.SpawnShellCasing();
           shellCasingFired = true;
       }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ws.weaponHolderFpc.SwitchWeaponToRightHand();
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
