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
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletTracer;
    public Vector3 fpcPosMale;
    public Vector3 fpcRotMale;
    public Vector3 fpcScaleMale;
    public Vector3 fpcPosFemale;
    public Vector3 fpcRotFemale;
    public Vector3 fpcScaleFemale;
    public AnimatorOverrideController maleOverrideController;
    public AnimatorOverrideController femaleOverrideController;

}
