using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCreatorScript : MonoBehaviour
{
    public AudioSource fireSource;
    public AudioSource suppressedFireSource;
    public AudioSource weaponSoundSource;
    public WeaponMeta weaponMetaDataRef;

    public void AssignWeaponStats() {
        weaponMetaDataRef.fireSound = fireSource;
        weaponMetaDataRef.suppressedFireSound = suppressedFireSource;
        weaponMetaDataRef.weaponSoundSource = weaponSoundSource;
    }
}
