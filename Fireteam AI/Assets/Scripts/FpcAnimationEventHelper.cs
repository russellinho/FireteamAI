using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpcAnimationEventHelper : MonoBehaviour
{
    public void ReloadShotgun() {
        transform.GetComponentInParent<WeaponActionScript>().ReloadShotgun();
    }

}
