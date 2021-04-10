﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;

public class WeaponMeta : MonoBehaviour
{
    public string weaponName;
    public bool steadyAim;
    public Vector3 aimDownSightPosMale;
    public Vector3 aimDownSightPosFemale;
    public Vector3 stableHandPosMale;
    public Vector3 stableHandPosFemale;
    public Vector3 defaultLeftCollarPosMale;
    public Vector3 defaultLeftCollarPosFemale;
    public Vector3 defaultRightCollarPosMale;
    public Vector3 defaultRightCollarPosFemale;
    public float aimDownSightSpeed;
    public float aimDownSightClipping;
    public float recoveryConstant;
    public AudioSource fireSound;
    public AudioSource suppressedFireSound;
    public AudioSource weaponSoundSource;
    public AudioClip[] reloadSounds;
    public AudioClip supportActionSound;
    public AudioClip meleeSwingSound;
    public AudioClip meleeLungeSound;
    public GameObject gunSmoke;
    public GameObject weaponShell;
    public Transform weaponShellPoint;
    public Transform weaponShootPoint;
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
    public Vector3 fullPosMale;
    public Vector3 fullRotMale;
    public Vector3 fullScaleMale;
    public Vector3 fullPosFemale;
    public Vector3 fullRotFemale;
    public Vector3 fullScaleFemale;
    public AnimatorOverrideController maleOverrideController;
    public AnimatorOverrideController femaleOverrideController;
    public AnimatorOverrideController maleOverrideControllerFullBody;
    public AnimatorOverrideController femaleOverrideControllerFullBody;
    public Animator weaponAnimator;
    public float defaultFpcReloadSpeed;
    public float defaultWeaponReloadSpeed;
    public float defaultWeaponCockingSpeed;
    public float defaultWeaponDrawSpeed;
    public float defaultFireSpeed;
    public float defaultMeleeSpeed;
    public float reloadTransitionSpeed;
    public float reloadSound1Time;
    public float reloadSound2Time;
    public float reloadSound3Time;
    public float reloadSound4Time;
    public float supportSoundTime;
    public bool switchToLeftDuringReload;
    public float switchToLeftTime;

    public MeshRenderer[] weaponParts;
    public MeshRenderer warheadRenderer;
    public GameObject deployPlanMesh;
    public bool isSticky;
    public GameObject suppressorSlot;
    public GameObject sightSlot;
    public float[] crosshairAimOffset;
    public GameObject deployRef;

}
