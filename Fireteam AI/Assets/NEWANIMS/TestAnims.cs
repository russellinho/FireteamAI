using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnims : MonoBehaviour
{
    public Animator anim;

    void Start() {
        anim.SetInteger("WeaponType", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            anim.SetTrigger("Jump");
            anim.SetBool("isSprinting", false);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            anim.SetBool("Crouching", !anim.GetBool("Crouching"));
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            anim.SetBool("isSprinting", true);
        } else {
            anim.SetBool("isSprinting", false);
        }
        if (Input.GetKey(KeyCode.W)) {
            anim.SetInteger("Moving", 1);
        } else if (Input.GetKey(KeyCode.S)) {
            anim.SetInteger("Moving", 4);
        } else if (Input.GetKey(KeyCode.A)) {
            anim.SetInteger("Moving", 2);
        } else if (Input.GetKey(KeyCode.D)) {
            anim.SetInteger("Moving", 3);
        } else {
            anim.SetInteger("Moving", 0);
        }
        if (Input.GetKey(KeyCode.C)) {
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            anim.SetBool("Incapacitated", true);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            anim.SetTrigger("Fire");
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            anim.SetTrigger("Cock");
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            anim.SetTrigger("Reload");
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            anim.SetTrigger("Melee");
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            anim.SetBool("onTitle", true);
        }
    }
}
