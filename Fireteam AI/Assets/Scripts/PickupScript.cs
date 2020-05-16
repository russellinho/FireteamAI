using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour {

	public int pickupId;
	private bool done;
	private float spawnTime;
	public AudioClip pickupSound;

	void Start() {
		done = false;
		spawnTime = 0f;
	}

	// Update is called once per frame
	void Update () {
		if (done || GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
			done = true;
			transform.Rotate (new Vector3(0f, 2f, 0f));
		}
		spawnTime += Time.deltaTime;
		if (spawnTime >= 15f) {
			Destroy (gameObject);
		}
	}

	public AudioClip GetPickupSound() {
		return pickupSound;
	}

	public void DestroyPickup() {
		Destroy(gameObject);
	}



}
