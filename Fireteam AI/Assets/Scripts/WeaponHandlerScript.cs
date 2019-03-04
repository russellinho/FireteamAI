using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandlerScript : MonoBehaviour
{
    public Transform handle;
    public Transform weapon;
    public Transform leftShoulder;
    private Vector3 originalShoulderPos;

    void Awake() {
        originalShoulderPos = new Vector3(leftShoulder.localPosition.x, leftShoulder.localPosition.y, leftShoulder.localPosition.z);
    }

    // Update is called once per frame
    public void LoadWeapon(string weaponPath) {
        if (weapon != null) {
            Destroy(weapon.gameObject);
            weapon = null;
            handle = null;
        }
        GameObject o = Instantiate((GameObject)Resources.Load(weaponPath), gameObject.transform);
        weapon = o.transform;
        weapon.SetParent(gameObject.transform);
        handle = weapon.gameObject.GetComponentsInChildren<Transform>()[1];
    }
    public void SetWeaponPosition()
    {
        weapon.localPosition = new Vector3(weapon.localPosition.x + (-handle.localPosition.x), weapon.localPosition.y + (-handle.localPosition.y), weapon.localPosition.z + (-handle.localPosition.z));
    }

    public void SetWeaponPosition(Vector3 p) {
        weapon.localPosition = new Vector3(weapon.localPosition.x + (-handle.localPosition.x) + p.x, weapon.localPosition.y + (-handle.localPosition.y) + p.y, weapon.localPosition.z + (-handle.localPosition.z) + p.z);
        weapon.localRotation = Quaternion.Euler(13.891f, 177.759f, -92.145f);
    }

    public void SetSteadyHand(Vector3 shoulderPos) {
        leftShoulder.localPosition = new Vector3(shoulderPos.x, shoulderPos.y, shoulderPos.z);
    }

    public void ResetSteadyHand() {
        leftShoulder.localPosition = originalShoulderPos;
    }

}
