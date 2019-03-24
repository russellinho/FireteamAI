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
        // Vector3 offset = new Vector3(transform.position.x - handle.position.x, transform.position.y - handle.position.y, transform.position.z - handle.position.z);
        //Vector3 offset = new Vector3();
        //weapon.localPosition = new Vector3(weapon.localPosition.x + (-handle.localPosition.x) + offset.x, weapon.localPosition.y + (-handle.localPosition.y) + offset.y, weapon.localPosition.z + (-handle.localPosition.z) + offset.z);
        //weapon.position = new Vector3(transform.localPosition.x + handle.localPosition.x, transform.localPosition.y + handle.localPosition.y, transform.localPosition.z + handle.localPosition.z);
        Vector3 oldHandlePos = handle.localPosition;
        handle.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        oldHandlePos = new Vector3(oldHandlePos.x - handle.localPosition.x, oldHandlePos.y - handle.localPosition.y, oldHandlePos.z - handle.localPosition.z);
        weapon.localPosition = new Vector3(weapon.localPosition.x - oldHandlePos.x - 0.01f, weapon.localPosition.y - oldHandlePos.y + 0.06f, weapon.localPosition.z - oldHandlePos.z + 0.02f);
    }

    public void SetWeaponPositionForTitle(Vector3 offset) {
        weapon.localPosition = new Vector3(weapon.localPosition.x + (-handle.localPosition.x) + offset.x, weapon.localPosition.y + (-handle.localPosition.y) + offset.y, weapon.localPosition.z + (-handle.localPosition.z) + offset.z);
        weapon.localRotation = Quaternion.Euler(13.891f, 177.759f, -92.145f);
    }

    public void SetSteadyHand(Vector3 shoulderPos) {
        leftShoulder.localPosition = new Vector3(shoulderPos.x, shoulderPos.y, shoulderPos.z);
    }

    public void ResetSteadyHand() {
        leftShoulder.localPosition = originalShoulderPos;
    }

}
