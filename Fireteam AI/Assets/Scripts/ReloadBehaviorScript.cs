using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadBehaviorScript : StateMachineBehaviour {

	bool hasReloaded = false;
	WeaponActionScript was;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		was = animator.GetComponentInParent<WeaponActionScript> ();
		hasReloaded = false;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	// override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	// 	if (!hasReloaded && animator.GetCurrentAnimatorStateInfo(0).IsName("Reload")) {
	// 		hasReloaded = true;
	// 		was.FpcChangeMagazine();
	// 	}
	// }

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (stateInfo.normalizedTime >= 0.9f) {
			was.Reload ();
			was.isCocking = false;
			was.isReloading = false;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	// override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	// 	was = animator.GetComponentInParent<WeaponActionScript> ();
	// 	was.Reload ();
	// 	was.isCocking = false;
	// 	was.isReloading = false;
	// }

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
