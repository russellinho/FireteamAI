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
    public float aimDownSightClipping;
    public Vector3 titleHandPositionsMale;
    public Vector3 titleHandPositionsFemale;
    public float recoveryConstant;
    public AudioSource fireSound;
    public AudioSource suppressedFireSound;
    public AudioSource reloadSound;
    public GameObject gunSmoke;
    public ParticleSystem muzzleFlash;
}
