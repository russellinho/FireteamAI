using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchAnimScript : StateMachineBehaviour {

    private WeaponScript tws;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		tws = animator.GetComponentInParent<WeaponScript>();
	}

	// // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	// override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //     if (doneSwitching) return;
	// 	// When the put away weapon animation ends, set the gun as ready to fire
    //     if (stateInfo.normalizedTime >= 0.2f) {
    //         Debug.Log("hi");
    //         doneSwitching = true;
    //         if (tws.currentlyEquippedType == 1) {
    //             animator.SetTrigger("ToAssaultRifle");
    //         } else if (tws.currentlyEquippedType == 2) {
    //             animator.SetTrigger("ToPistol");
    //         }
    //     }
	// }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        tws.weaponReady = true;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
