using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionStates = BetaEnemyScript.ActionStates;
using FiringStates = BetaEnemyScript.FiringStates;
using AlertStatus = BetaEnemyScript.AlertStatus;

public class NpcTestAnim : MonoBehaviour
{
    public Animator animator;
    public AlertStatus alertStatus;
    public ActionStates actionState;
	public FiringStates firingState;
    public bool navMeshActiveAndEnabled;
    public bool navMeshIsOn;
    public bool navMeshStopped;
    public int currentBullets;
    public bool isCrouching;

    public Transform targetPos;
    public Transform spineTransform;
    private float rotationSpeed = 6f;

	void Start()
	{
		WeaponMeta w = GetComponentInChildren<WeaponMeta>();
		animator.runtimeAnimatorController = w.maleNpcOverrideController as RuntimeAnimatorController;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            animator.SetTrigger("Fire");
        }
        DecideAnimation();
    }

    void LateUpdate()
    {
		RotateTowardsPlayer();
    }

    void RotateTowardsPlayer() {
		transform.LookAt(targetPos.position);
		transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

		spineTransform.forward = (targetPos.position - spineTransform.position).normalized;
		spineTransform.localRotation = Quaternion.Euler(spineTransform.localRotation.eulerAngles.x, 0f, 0f);
	}

    void DecideAnimation() {
		if (actionState == ActionStates.Seeking) {
			if (animator.GetInteger("Moving") == 0) {
				int r = Random.Range (1, 4);
				if (r >= 1 && r <= 2) {
					animator.SetInteger("Moving", 1);
					animator.SetBool("isSprinting", false);
				} else {
					animator.SetBool("isSprinting", true);
				}
			}
		}

		if (actionState == ActionStates.Wander) {
			if (navMeshActiveAndEnabled && navMeshIsOn && navMeshStopped) {
				animator.SetBool ("onTitle", true);
				animator.SetInteger("Moving", 0);
				animator.SetBool("Patrol", false);
			} else {
				animator.SetBool ("onTitle", false);
				if (alertStatus != AlertStatus.Alert && !animator.GetBool("Patrol")) {
					animator.SetInteger("Moving", 0);
					animator.SetBool ("Patrol", true);
				} else if (alertStatus == AlertStatus.Alert && animator.GetInteger("Moving") == 0) {
					animator.SetBool ("Patrol", false);
					animator.SetInteger("Moving", 1);
				}
			}
		} else {
			animator.SetBool("Patrol", false);
		}

		if (actionState == ActionStates.TakingCover) {
			if (!animator.GetBool("isSprinting"))
				animator.SetBool ("isSprinting", true);
		}

		if (actionState == ActionStates.Pursue) {
			if (!animator.GetBool("isSprinting"))
				animator.SetBool("isSprinting", true);
		}

		if (actionState == ActionStates.Idle) {
			animator.SetInteger("Moving", 0);
			if (alertStatus == AlertStatus.Alert) {
				animator.SetBool("onTitle", false);
			} else {
				animator.SetBool("onTitle", true);
			}
		}

		if (actionState == ActionStates.Firing || actionState == ActionStates.Reloading || actionState == ActionStates.InCover) {
			// Set proper animation
			animator.SetBool("onTitle", false);
			animator.SetBool("Patrol", false);
			animator.SetBool("isSprinting", false);
			if (actionState == ActionStates.Firing && currentBullets > 0) {
				if (firingState == FiringStates.StandingStill) {
					animator.SetInteger("Moving", 0);
				} else if (firingState == FiringStates.Forward) {
					animator.SetInteger("Moving", 1);
				} else if (firingState == FiringStates.Backpedal) {
					animator.SetInteger("Moving", 4);
				} else if (firingState == FiringStates.StrafeLeft) {
					animator.SetInteger("Moving", 2);
				} else if (firingState == FiringStates.StrafeRight) {
					animator.SetInteger("Moving", 3);
				}
			} else if (currentBullets <= 0) {
				animator.SetTrigger("Reload");
			}
		}

		animator.SetBool("Crouching", isCrouching);

		if (actionState == ActionStates.Melee) {
			if (!animator.GetCurrentAnimatorStateInfo (1).IsName ("Melee")) {
				animator.SetTrigger("Melee");
			}
		}

		animator.SetBool("Disoriented", actionState == ActionStates.Disoriented);
	}
}
