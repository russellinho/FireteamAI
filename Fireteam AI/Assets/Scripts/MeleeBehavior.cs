﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBehavior : StateMachineBehaviour
{
    WeaponActionScript was;
    bool attackDealt;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        was = animator.GetComponentInParent<WeaponActionScript>();
        if (was.isLunging) {
            was.SetMouseDynamicsForMelee(true);
        }
        was.isMeleeing = true;
        attackDealt = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (was.isLunging) {
            // was.UpdateMeleeDash(stateInfo.normalizedTime);
            was.UpdateMeleeDash();
            was.DealMeleeDamage();
        } else {
            if (stateInfo.normalizedTime <= 0.8f) {
                was.DealMeleeDamage();
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        was.isMeleeing = false;
        was.EndMeleeDash();
        was.meleeTargetPos = Vector3.negativeInfinity;
        was.SetMouseDynamicsForMelee(false);
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
