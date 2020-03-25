using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour {

	public int pickupId;
	private bool done;
	private float spawnTime;
	private float destroyTimer;
	private bool destroyed;
	private AudioSource aud;

	void Start() {
		done = false;
		spawnTime = 0f;
		destroyTimer = 0f;
		destroyed = false;
		aud = GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update () {
		if (destroyed) {
			if (destroyTimer <= 0f) {
				Destroy (gameObject);
			} else {
				destroyTimer -= Time.deltaTime;
			}
			return;
		}
		if (done || GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
			done = true;
			transform.Rotate (new Vector3(0f, 2f, 0f));
		}
		spawnTime += Time.deltaTime;
		if (spawnTime >= 15f) {
			Destroy (gameObject);
		}
	}

	public void PlayPickupSound() {
		aud.Play ();
	}

	public void DestroyPickup() {
		destroyTimer = 3f;
		destroyed = true;
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<BoxCollider> ().enabled = false;
		GetComponent<Animator> ().enabled = false;
	}



}
