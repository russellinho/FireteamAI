using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehavior : StateMachineBehaviour
{
    WeaponActionScript was;
    bool entered;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        was = animator.GetComponentInParent<WeaponActionScript>();
        was.isCockingGrenade = false;
        entered = true;
       animator.ResetTrigger("ThrowGrenade");
       animator.ResetTrigger("isCockingGrenade");
       animator.ResetTrigger("UseBooster");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (entered && stateInfo.normalizedTime >= 0.2f) {
            entered = false;
            was.isCocking = false;
            //was.isCockingGrenade = false;
            was.isReloading = false;
            was.isUsingBooster = false;
            was.isFiring = false;
        }
        if (was.isWieldingSupportItem && (Input.GetButtonDown("Fire1") || Input.GetButton("Fire1"))) {
            was.isCockingGrenade = true;
        }
        if (was.isWieldingSupportItem && (Input.GetButtonUp("Fire1"))) {
            was.ConfirmGrenadeThrow();
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
