using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    public string weaponName;
    public string type;
    public string category;
    public float damage;
    public float mobility;
    public float fireRate;
    public float accuracy;
    public float recoil;
    public float range;
    public int clipCapacity;
    public int maxAmmo;
    public Vector3 aimDownSightPosMale;
    public Vector3 aimDownSightPosFemale;
    public float aimDownSightSpeed;
    public float aimDownSightClipping;
    public Vector3 titleHandPositionsMale;
    public Vector3 titleHandPositionsFemale;
    public float recoveryConstant;
    public AudioSource fireSound;
    public AudioSource suppressedFireSound;
    public AudioSource reloadSound;
    public GameObject gunSmoke;
    public GameObject weaponShell;
    public Transform weaponShellPoint;
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletTracer;
    public Vector3 fpcPosMale;
    public Vector3 fpcLeftHandPosMale;
    public Vector3 fpcLeftHandRotMale;
    public Vector3 fpcRotMale;
    public Vector3 fpcScaleMale;
    public Vector3 fpcPosFemale;
    public Vector3 fpcLeftHandPosFemale;
    public Vector3 fpcLeftHandRotFemale;
    public Vector3 fpcRotFemale;
    public Vector3 fpcScaleFemale;
    public AnimatorOverrideController maleOverrideController;
    public AnimatorOverrideController femaleOverrideController;
    public Animator weaponAnimator;
    public float defaultFpcReloadSpeed;
    public float defaultWeaponReloadSpeed;
    public float defaultWeaponCockingSpeed;
    public float defaultFireSpeed;
    public float reloadTransitionSpeed;
    public float cockStartTime;

    public MeshRenderer[] weaponParts;
    public GameObject suppressorSlot;

}
