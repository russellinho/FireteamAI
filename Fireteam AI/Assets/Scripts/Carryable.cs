using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Carryable : MonoBehaviourPunCallbacks
{
    public float throwForceMultiplier;
    public float weightSpeedReduction;
	public bool immobileWhileCarrying;
    public float detectionRatePenalty;
    public string carryableName;
    public int carryableId;
    public int carriedByPlayerId;
    public Transform carriedByTransform;
    public Rigidbody mainRigid;
	public Vector3 carryPosition;
	public Vector3 carryRotation;

	// void Awake()
	// {
	// 	carriedByPlayerId = -1;
	// 	SceneManager.sceneLoaded += OnSceneFinishedLoading;
	// }

    public void ToggleIsCarrying(bool b, int carriedByPlayerId) {
		this.carriedByPlayerId = carriedByPlayerId;
		if (carriedByPlayerId == -1) {
			carriedByTransform = null;
			gameObject.transform.SetParent(null);
			transform.rotation = Quaternion.identity;
			mainRigid.isKinematic = false;
			mainRigid.useGravity = true;
		} else {
			carriedByTransform = GameControllerScript.playerList[carriedByPlayerId].objRef.GetComponent<PlayerActionScript>().carryingSlot;
			mainRigid.useGravity = false;
			mainRigid.isKinematic = true;
			gameObject.transform.SetParent(carriedByTransform);
			transform.localPosition = carryPosition;
			transform.localRotation = Quaternion.Euler(carryRotation);
		}
	}

    public void Launch(float xForce, float yForce, float zForce) {
        // Apply a force to the throwable that's equal to the forward position of the weapon holder
        mainRigid.velocity = new Vector3(xForce * throwForceMultiplier, yForce * throwForceMultiplier, zForce * throwForceMultiplier);
    }

}
