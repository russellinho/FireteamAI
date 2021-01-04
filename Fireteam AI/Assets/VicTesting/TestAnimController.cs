using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimController : MonoBehaviour
{
    public Animator animator;
    public char supportTypeCarrying;
    // Start is called before the first frame update
    void Start()
    {
        ResetState();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            if (supportTypeCarrying == 'g') {
                supportTypeCarrying = 'i';
            } else {
                supportTypeCarrying = 'g';
            }
        }

        // Title preview mode
        if (Input.GetKeyDown(KeyCode.Comma)) {
            TitleMode(true);
        }

        // Reset
        if (Input.GetKeyDown(KeyCode.Period)) {
            TitleMode(false);
        }

        // Die
        if (Input.GetKeyDown(KeyCode.Z)) {
            animator.SetBool("isDead", true);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            animator.SetTrigger("Reloading");
        }

        // Movement
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)) {
            animator.SetInteger("Moving", 5);
        } else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)) {
            animator.SetInteger("Moving", 6);
        } else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A)) {
            animator.SetInteger("Moving", 7);
        } else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D)) {
            animator.SetInteger("Moving", 8);
        } else if (Input.GetKey(KeyCode.W)) {
            animator.SetInteger("Moving", 1);
        } else if (Input.GetKey(KeyCode.S)) {
            animator.SetInteger("Moving", 4);
        } else if (Input.GetKey(KeyCode.A)) {
            animator.SetInteger("Moving", 2);
        } else if (Input.GetKey(KeyCode.D)) {
            animator.SetInteger("Moving", 3);
        } else {
            animator.SetInteger("Moving", 0);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space)) {
            animator.SetTrigger("Jump");
        }

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            animator.SetBool("Crouching", !animator.GetBool("Crouching"));
        }

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift)) {
            animator.SetBool("isSprinting", true);
        } else {
            animator.SetBool("isSprinting", false);
        }

        // Walk
        if (Input.GetKey(KeyCode.C)) {
            animator.SetBool("isWalking", true);
        } else {
            animator.SetBool("isWalking", false);
        }

        // Melee
        if (Input.GetKeyDown(KeyCode.V)) {
            animator.SetTrigger("Melee");
        }

        // Fire
        int currWepType = animator.GetInteger("WeaponType");
        if (currWepType == 4) {
            if (supportTypeCarrying == 'i') {
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    animator.SetTrigger("useBooster");
                }
            } else {
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    animator.SetBool("isCockingGrenade", true);
                }
                if (Input.GetKeyUp(KeyCode.Mouse0)) {
                    animator.SetBool("isCockingGrenade", false);
                }
            }
        }

        // Draw primary
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            animator.SetInteger("WeaponType", 1);
        }

        // Draw secondary
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            animator.SetInteger("WeaponType", 2);
        }

        // Draw support
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            animator.SetInteger("WeaponType", 4);
        }
    }

    void ResetState()
    {
        animator.SetInteger("WeaponType", 1);
        animator.SetInteger("Moving", 0);
        animator.SetBool("weaponReady", false);
        animator.SetBool("Crouching", false);
        animator.SetBool("isSprinting", false);
        animator.SetBool("isDead", false);
        animator.SetBool("isWalking", false);
        animator.Play("IdleAssaultRifle", 0);
        animator.Play("IdleAssaultRifle", 1);
    }

    void TitleMode(bool b)
    {
        animator.SetBool("onTitle", b);
        ResetState();
    }
}
