using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWeaponBehavior : StateMachineBehaviour
{
    WeaponActionScript was;
    WeaponScript ws;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       was = animator.GetComponentInParent<WeaponActionScript>();
       was.deployInProgress = false;
       was.isDrawing = true;
       ws = was.playerActionScript.weaponScript;
       if (ws.currentlyEquippedType == 1) {
          ws.EquipWeapon(ws.equippedPrimaryWeapon, PlayerData.playerdata.primaryModInfo.equippedSuppressor, PlayerData.playerdata.primaryModInfo.equippedSight, null);
       } else if (ws.currentlyEquippedType == 2) {
          ws.EquipWeapon(ws.equippedSecondaryWeapon, PlayerData.playerdata.secondaryModInfo.equippedSuppressor, PlayerData.playerdata.secondaryModInfo.equippedSight, null);
       } else if (ws.currentlyEquippedType == 4) {
          ws.EquipWeapon(ws.equippedSupportWeapon, PlayerData.playerdata.supportModInfo.equippedSuppressor, PlayerData.playerdata.supportModInfo.equippedSight, null);
       }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
   //  override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   //  {
       
   //  }

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
