using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCreatorScript : MonoBehaviour
{
    public AudioSource fireSource;
    public AudioSource suppressedFireSource;
    public AudioSource weaponSoundSource;
    public WeaponStats weaponStatsRef;

    public void AssignWeaponStats() {
        weaponStatsRef.fireSound = fireSource;
        weaponStatsRef.suppressedFireSound = suppressedFireSource;
        weaponStatsRef.weaponSoundSource = weaponSoundSource;
    }
}
