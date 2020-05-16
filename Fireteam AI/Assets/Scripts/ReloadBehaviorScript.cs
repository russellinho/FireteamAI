﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadBehaviorScript : StateMachineBehaviour {

	bool hasReloaded = false;
	bool reload1Played;
	bool reload2Played;
	bool reload3Played;
	bool reload4Played;
	bool supportSoundPlayed;
	bool inLeftHand;
	WeaponActionScript was;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		was = animator.GetComponentInParent<WeaponActionScript> ();
		hasReloaded = false;
		reload1Played = false;
		reload2Played = false;
		reload3Played = false;
		reload4Played = false;
		inLeftHand = false;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	// override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	// 	if (!hasReloaded && animator.GetCurrentAnimatorStateInfo(0).IsName("Reload")) {
	// 		hasReloaded = true;
	// 		was.FpcChangeMagazine();
	// 	}
	// }

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (stateInfo.normalizedTime >= 0.15f) {
			was.playerActionScript.weaponScript.ToggleWarhead(true);
		}
		if (!reload1Played && was.weaponStats.reloadSound1Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound1Time) {
			reload1Played = true;
			was.PlayReloadSound(0);
		}
		if (!reload2Played && was.weaponStats.reloadSound2Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound2Time) {
			reload2Played = true;
			was.PlayReloadSound(1);
		}
		if (!reload3Played && was.weaponStats.reloadSound3Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound3Time) {
			reload3Played = true;
			was.PlayReloadSound(2);
		}
		if (!reload4Played && was.weaponStats.reloadSound4Time != -1f && stateInfo.normalizedTime >= was.weaponStats.reloadSound4Time) {
			reload4Played = true;
			was.PlayReloadSound(3);
		}
		if (!supportSoundPlayed && was.weaponStats.supportSoundTime != -1f && stateInfo.normalizedTime >= was.weaponStats.supportSoundTime) {
			supportSoundPlayed = true;
			was.PlaySupportActionSound();
		}
		if (!inLeftHand && was.weaponStats.switchToLeftDuringReload && stateInfo.normalizedTime >= was.weaponStats.switchToLeftTime) {
			inLeftHand = true;
			was.playerActionScript.weaponScript.weaponHolderFpc.SwitchWeaponToLeftHand();
		}
		if (stateInfo.normalizedTime >= 0.9f && !hasReloaded) {
			was.Reload ();
			hasReloaded = true;
			//was.isCocking = false;
			was.isReloading = false;
		}
	}

	//OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//was = animator.GetComponentInParent<WeaponActionScript> ();
		//was.Reload ();
		if (inLeftHand) {
			was.playerActionScript.weaponScript.weaponHolderFpc.SwitchWeaponToRightHand();
		}
		was.playerActionScript.weaponScript.ToggleWarhead(true);
		was.isCocking = false;
		was.isReloading = false;
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
