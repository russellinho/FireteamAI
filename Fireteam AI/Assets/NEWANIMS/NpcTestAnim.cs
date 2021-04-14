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
    public NpcLook npcLook;
    public bool navMeshActiveAndEnabled;
    public bool navMeshIsOn;
    public bool navMeshStopped;
    public int currentBullets;
    public bool isCrouching;

	private Vector3 prevTargetPos;
    public Transform targetPos;
    public Transform spineTransform;
    private float rotationSpeed = 6f;
	private float rotationLerp;
    
	void Start()
	{
		StartCoroutine("DetermineResetRotLerp");
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            animator.SetTrigger("Fire");
        }
        DecideAnimation();
    }

	IEnumerator DetermineResetRotLerp()
	{
		if (!Vector3.Equals(prevTargetPos, targetPos.position)) {
			rotationLerp = 0f;
			prevTargetPos = targetPos.position;
		}
		yield return new WaitForSeconds(0.1f);
		StartCoroutine("DetermineResetRotLerp");
	}

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Period)) {
            ResetRot();
        }
        if (Input.GetKey(KeyCode.A)) {
            npcLook.LookRotation (gameObject.transform, spineTransform, Vector3.negativeInfinity, 0f, -0.2f);
        } else if (Input.GetKey(KeyCode.D)) {
            npcLook.LookRotation (gameObject.transform, spineTransform, Vector3.negativeInfinity, 0f, 0.2f);
        } else if (Input.GetKey(KeyCode.W)) {
            npcLook.LookRotation (gameObject.transform, spineTransform, Vector3.negativeInfinity, 0.2f, 0f);
            // spineTransform.rotation = Quaternion.Euler(50f, 0f, 0f);
        } else if (Input.GetKey(KeyCode.S)) {
            npcLook.LookRotation (gameObject.transform, spineTransform, Vector3.negativeInfinity, -0.2f, 0f);
        } else {
            npcLook.LookRotation (gameObject.transform, spineTransform, Vector3.negativeInfinity, 0f, 0f);
        }

        // if (Input.GetKeyDown(KeyCode.G)) {
            // RotateTowardsPlayer();
        // }
		RotateTowardsPlayer();
    }

    void RotateTowardsPlayer() {
		Vector3 rotDir = (targetPos.position - transform.position).normalized;
		Quaternion lookRot = Quaternion.LookRotation (rotDir);
		if (rotationLerp < 1f) {
			rotationLerp += 0.2f;
			if (rotationLerp > 1f) {
				rotationLerp = 1f;
			}
			Quaternion tempQuat = Quaternion.Slerp (transform.rotation, lookRot, rotationLerp);
			Vector3 tempRot = tempQuat.eulerAngles;
			npcLook.LookRotation (gameObject.transform, spineTransform, targetPos.position, tempRot.x, tempRot.y);
		}
	}

    void ResetRot()
    {
        npcLook.ResetRot();
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
