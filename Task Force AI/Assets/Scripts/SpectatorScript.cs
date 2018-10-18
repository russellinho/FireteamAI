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
	private int playerListIndex = -1;
	private int lastPlayerListSize = 0;

	// Use this for initialization
	void Start () {
		camTransform = transform;
		cam = GetComponent<Camera> ();

		for (int i = 0; i < GameControllerScript.playerList.Count; i++) {
			Debug.Log (i);
            Debug.Log (((GameObject)GameControllerScript.playerList[i]).GetComponent<PlayerScript>().health);
            Debug.Log(((GameObject)GameControllerScript.playerList[i]).GetComponent<PhotonView>().IsMine);
            if (((GameObject)GameControllerScript.playerList [i]).GetComponent<PlayerScript> ().health > 0 && !((GameObject)GameControllerScript.playerList [i]).GetComponent<PhotonView>().IsMine) {
				following = (GameObject)GameControllerScript.playerList [i];
				Debug.Log ("found one");
				playerListIndex = i;
				break;
			}
			if (i == GameControllerScript.playerList.Count - 1) {
				Debug.Log ("is null");
				following = null;
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
		if (lastPlayerListSize != GameControllerScript.playerList.Count) {
			lastPlayerListSize = GameControllerScript.playerList.Count;
			playerListIndex = -1;
		}
		int numberSkipped = 0;
		while (((GameObject)GameControllerScript.playerList [++playerListIndex]).GetComponent<PlayerScript> ().health <= 0 || ((GameObject)GameControllerScript.playerList [playerListIndex]).GetComponent<PhotonView> ().IsMine) {
			numberSkipped++;
			// Checks if there are any players available to spectate
			if (numberSkipped >= GameControllerScript.playerList.Count) {
				// If there aren't, set the following index back to -1 for default position
				playerListIndex = -1;
				following = null;
				return;
			}
		}
		following = (GameObject)GameControllerScript.playerList [playerListIndex];
	}

}
