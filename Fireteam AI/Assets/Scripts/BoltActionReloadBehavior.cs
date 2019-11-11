using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltActionReloadBehavior : StateMachineBehaviour
{

    private WeaponActionScript was;
    private bool reload1Played;
    private bool reload2Played;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("CockBoltAction");
        was = animator.GetComponentInParent<WeaponActionScript>();
        was.isReloading = true;
        was.FpcLoadBoltAction();
        reload1Played = false;
        reload2Played = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
		if (!reload1Played && was.weaponStats.reloadSound1Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound1Time) {
			reload1Played = true;
			was.PlayReloadSound(0);
		}
		if (!reload2Played && was.weaponStats.reloadSound2Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound2Time) {
			reload2Played = true;
			was.PlayReloadSound(1);
		}
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    // }

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
