using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraScript : MonoBehaviour
{
    private const int WATER_LAYER = 4;
    public SphereCollider col;
    public Rigidbody rigid;
    //public Transform weaponHolderTrans;
    public FirstPersonController fpc;
    public AudioControllerScript audioController;
    public WeaponActionScript weaponActionScript;
    public WeaponScript weaponScript;
    private float unlockAnimationsTimer;

    // Update is called once per frame
    // void LateUpdate()
    // {
    //     // If dead, don't update the camera here
    //     if (fpc.playerActionScript.health <= 0) {
    //         return;
    //     }
    //     bool isReloading = fpc.weaponActionScript.isReloading;
    //     if (!fpc.m_IsRunning && !isReloading) {
    //         transform.forward = Vector3.Slerp(transform.forward, weaponHolderTrans.up, 5f * Time.deltaTime);
    //     } else {
    //         transform.forward = Vector3.Slerp(transform.forward, new Vector3(transform.forward.x, transform.forward.y, transform.forward.z), 5f * Time.deltaTime);
    //     }
    // }

    void Update()
    {
        if (unlockAnimationsTimer > 0f) {
            unlockAnimationsTimer -= Time.deltaTime;
            if (fpc.fpcAnimator.GetBool("Swimming")) {
                UnlockAnimations();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == WATER_LAYER) {
            fpc.SetIsSwimming(true);
            // Enable FPS swim animation (put away weapon), reset FPC animator, reset full body animator, reset weapon animator
            LockAnimations();
            // Player water enter sound
            audioController.PlayerWaterEnterSound();
            audioController.ToggleWaterAmbience(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == WATER_LAYER) {
            fpc.SetIsSwimming(false);
            // Disable FPS swim animation (draw weapon), reset FPC animator, reset full body animator, reset weapon animator
            SetUnlockAnimationsTimer();
            // Player water exit sound
            audioController.ToggleWaterAmbience(false);
        }
    }

    void UnlockAnimations()
    {
        fpc.SetSwimmingInAnimator(false);
        fpc.SetSwimmingInFPCAnimator(false);
        weaponActionScript.ResetMyActionStates();
        fpc.ResetAnimationState();
        fpc.ResetFPCAnimator(weaponScript.currentlyEquippedType);
    }

    void SetUnlockAnimationsTimer()
    {
        unlockAnimationsTimer = 1.75f;
    }

    void LockAnimations()
    {
        weaponActionScript.ResetMyActionStates();
        fpc.ResetAnimationState();
        fpc.ResetFPCAnimator(weaponScript.currentlyEquippedType);
        fpc.SetSwimmingInFPCAnimator(true);
        fpc.SetSwimmingInAnimator(true);
    }

    void OnPostRender() {
        if (fpc.playerActionScript.hud.screenGrab) {
            fpc.playerActionScript.hud.TriggerFlashbangEffect();
            fpc.playerActionScript.hud.screenGrab = false;
        }
    }
}
