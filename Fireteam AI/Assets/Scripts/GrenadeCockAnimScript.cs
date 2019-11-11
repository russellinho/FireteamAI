using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GrenadeCockAnimScript : StateMachineBehaviour {

	private WeaponScript weaponScript;
	private bool hidden;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		weaponScript = animator.GetComponentInParent<WeaponScript>();
		if (animator.GetComponentInParent<PhotonView>().IsMine) {
			// Throw grenade/use item when animation is done
			WeaponActionScript was = animator.GetComponentInParent<WeaponActionScript> ();
			was.UseSupportItem();
			hidden = false;
		} else {
			weaponScript.ToggleWeaponVisible(false);
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!hidden && stateInfo.normalizedTime >= 0.3f) {
			weaponScript.ToggleWeaponVisible(false);
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		weaponScript.ToggleWeaponVisible(true);
		animator.ResetTrigger("isCockingGrenade");
		animator.ResetTrigger("ThrowGrenade");
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
