using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PickupScript : MonoBehaviour {

	private bool done;
	private float spawnTime;
	private bool destroying;
	private AudioSource aud;

	void Start() {
		done = false;
		spawnTime = 0f;
		destroying = false;
		aud = GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update () {
		if (destroying) {
			if (!aud.isPlaying) {
				PhotonNetwork.Destroy (gameObject);
			} else {
				Debug.Log (aud.clip.name);
			}
			return;
		}
		if (done || GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
			done = true;
			transform.Rotate (new Vector3(0f, 2f, 0f));
		}
		spawnTime += Time.deltaTime;
		if (spawnTime >= 15f) {
			if (PhotonNetwork.IsMasterClient) {
				PhotonNetwork.Destroy (gameObject);
			}
		}
	}

	public void DestroyPickup() {
		aud.Play ();
		destroying = true;
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<BoxCollider> ().enabled = false;
		GetComponent<Animator> ().enabled = false;
	}

}
