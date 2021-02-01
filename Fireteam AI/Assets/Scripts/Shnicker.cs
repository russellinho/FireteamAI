using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shnicker : MonoBehaviour
{
    public Animator animator;
    public Collider col;
    public Rigidbody[] ragdollBodies;
    public Transform headTransform;
	public Transform torsoTransform;
	public Transform leftArmTransform;
	public Transform leftForeArmTransform;
	public Transform rightArmTransform;
	public Transform rightForeArmTransform;
	public Transform pelvisTransform;
	public Transform leftUpperLegTransform;
	public Transform leftLowerLegTransform;
	public Transform rightUpperLegTransform;
	public Transform rightLowerLegTransform;
    public int lastBodyPartHit;
    public Vector3 lastHitFromPos;
    public int lastHitBy;


    void Awake()
    {
        Physics.IgnoreLayerCollision (14, 13);
        ToggleRagdoll(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) {
            animator.Play("Die");
            ToggleRagdoll(true);
			// Apply force modifiers
			ApplyForceModifiers();
        } else if (Input.GetKeyDown(KeyCode.X)) {
            animator.Play("Idle");
            ToggleRagdoll(false);
			transform.position = new Vector3(0f, 0.5f, 0f);
			transform.rotation = Quaternion.identity;
        } else if (Input.GetKeyDown(KeyCode.J)) {
			transform.Rotate(0f, 90f, 0f);
		}
    }

    void ToggleRagdoll(bool b)
	{
		animator.enabled = !b;
        col.enabled = !b;

		foreach (Rigidbody rb in ragdollBodies)
		{
			rb.isKinematic = !b;
            rb.useGravity = b;
		}

		// headTransform.GetComponent<Collider>().enabled = b;
		// torsoTransform.GetComponent<Collider>().enabled = b;
		// leftArmTransform.GetComponent<Collider>().enabled = b;
		// leftForeArmTransform.GetComponent<Collider>().enabled = b;
		// rightArmTransform.GetComponent<Collider>().enabled = b;
		// rightForeArmTransform.GetComponent<Collider>().enabled = b;
		// pelvisTransform.GetComponent<Collider>().enabled = b;
		// leftUpperLegTransform.GetComponent<Collider>().enabled = b;
		// leftLowerLegTransform.GetComponent<Collider>().enabled = b;
		// rightUpperLegTransform.GetComponent<Collider>().enabled = b;
		// rightLowerLegTransform.GetComponent<Collider>().enabled = b;
	}

    void ApplyForceModifiers()
	{
		// 0 = death from bullet, 1 = death from explosion, 2 = death from fire/etc.
		if (lastHitBy == 0) {
			Rigidbody rb = ragdollBodies[lastBodyPartHit - 1];
			if (lastBodyPartHit == WeaponActionScript.HEAD_TARGET) {
				Vector3 forceDir = Vector3.Normalize(headTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.TORSO_TARGET) {
				Vector3 forceDir = Vector3.Normalize(torsoTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftArmTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftForeArmTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightArmTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightForeArmTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.PELVIS_TARGET) {
				Vector3 forceDir = Vector3.Normalize(pelvisTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftUpperLegTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftLowerLegTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightUpperLegTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightLowerLegTransform.position - lastHitFromPos) * 50f;
				rb.AddForce(forceDir, ForceMode.Impulse);
			}
		} else if (lastHitBy == 1) {
			foreach (Rigidbody rb in ragdollBodies) {
				rb.AddExplosionForce(75f, lastHitFromPos, 7f, 0f, ForceMode.Impulse);
			}
		}
	}
}
