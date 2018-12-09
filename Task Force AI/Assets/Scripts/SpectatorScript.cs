using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpectatorScript : MonoBehaviour {

	private const float Y_ANGLE_MIN = 10F;
	private const float Y_ANGLE_MAX = 50F;

	public GameObject following;
	private Transform camTransform;
	private Camera cam;

	private float distance = 8f;
	private float currX = 0F;
	private float currY = 0f;
	private float sensitivityX = 4f;
	private float sensitivityY = 1f;

	private bool rotationLock = false;
	private int playerListKey = -1;
	private int lastPlayerListSize = 0;

	// Use this for initialization
	void Start () {
		camTransform = transform;
		cam = GetComponent<Camera> ();

		foreach (GameObject o in GameControllerScript.playerList.Values) {
            if (o.GetComponent<PlayerScript> ().health > 0 && !o.GetComponent<PhotonView>().IsMine) {
				following = o;
				playerListKey = o.GetComponent<PhotonView> ().OwnerActorNr;
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			SwitchFollowing ();
		}
		if (!following) {
			camTransform.position = Vector3.zero;
			camTransform.rotation = Quaternion.Euler (Vector3.zero);
		}
		if (!rotationLock) {
			currX += Input.GetAxis ("Mouse X");
			currY += Input.GetAxis ("Mouse Y");

			currY = Mathf.Clamp (currY, Y_ANGLE_MIN, Y_ANGLE_MAX);
		}
	}

	void LateUpdate() {
		if (following) {
			Vector3 dir = new Vector3 (0f, 0f, -distance);
			Quaternion rot = Quaternion.Euler (currY, currX, 0f);
			camTransform.position = following.transform.position + rot * dir;
			camTransform.LookAt (following.transform.position);
		}
	}

	void SwitchFollowing() {
		int numberSkipped = 0;
		GameObject first = null;
		bool currentIdFound = false;
		foreach (GameObject o in GameControllerScript.playerList.Values) {
			if (o.GetComponent<PlayerScript> ().health > 0) {
				if (!first) {
					first = o;
				} else {
					if (currentIdFound) {
						first = o;
						break;
					}
					if (o.GetComponent<PhotonView>().OwnerActorNr == playerListKey) {
						currentIdFound = true;
					}
				}
			}
		}
		following = first;
		if (following) {
			playerListKey = following.GetComponent<PhotonView> ().OwnerActorNr;
		} else {
			playerListKey = -1;
		}
	}

}
