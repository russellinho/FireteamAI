using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpectatorScript : MonoBehaviour {

	private const float Y_ANGLE_MIN = 10f;
	private const float Y_ANGLE_MAX = 50f;
	private Vector3 gameOverCamPos = new Vector3(81f, 30f, 5f);
	private Vector3 gameOverCamRot = new Vector3(0f, -40f, 0f);

	public GameObject following;
	public Camera cam;

	private float distance = 8f;
	private float currX = 0f;
	private float currY = 0f;
	private float sensitivityX = 4f;
	private float sensitivityY = 1f;

	private bool rotationLock = false;
	private int playerListKey = -1;
	private bool gameOverLock = false;

	// Use this for initialization
	void Start () {
		foreach (PlayerStat playerStat in GameControllerScript.playerList.Values) {
			GameObject o = playerStat.objRef;
            if (o.GetComponent<PlayerActionScript> ().health > 0 && !o.GetComponent<PhotonView>().IsMine) {
				following = o;
				playerListKey = o.GetComponent<PhotonView> ().OwnerActorNr;
				break;
			}
		}

		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.Euler (Vector3.zero);
	}
	
	// Update is called once per frame
	void Update () {
		if (!gameOverLock) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				SwitchFollowing ();
			}
			if (!following) {
				rotationLock = true;
				transform.position = new Vector3 (0f, 10f, 0f);
				transform.rotation = Quaternion.Euler (new Vector3 (0f, 45f, 0f));
			}
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
			transform.position = following.transform.position + rot * dir;
			transform.LookAt (following.transform.position);
		}
	}

	void SwitchFollowing() {
		int numberSkipped = 0;
		GameObject first = null;
		bool currentIdFound = false;
		foreach (PlayerStat playerStat in GameControllerScript.playerList.Values) {
			GameObject o = playerStat.objRef;
			if (o.GetComponent<PlayerActionScript> ().health > 0) {
				// If we haven't defined first person in the collection yet, define it
				if (!first) {
					first = o;
				}
				// Else, if we come across the player who we are currently spectating, then move onto the next player or the first if it's the last player
				if (currentIdFound) {
					first = o;
					break;
				}
				if (o.GetComponent<PhotonView>().OwnerActorNr == playerListKey) {
					currentIdFound = true;
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

	public void GameOverCam() {
		gameOverLock = true;
		rotationLock = true;
		transform.position = gameOverCamPos;
		transform.rotation = Quaternion.Euler (gameOverCamRot);
	}

}
