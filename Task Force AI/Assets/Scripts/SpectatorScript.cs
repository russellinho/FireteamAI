using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	private int playerListIndex = 0;

	// Use this for initialization
	void Start () {
		camTransform = transform;
		cam = GetComponent<Camera> ();

		for (int i = 0; i < GameControllerScript.playerList.Length; i++) {
			if (GameControllerScript.playerList [i].GetComponent<PlayerScript> ().health > 0) {
				following = GameControllerScript.playerList [i];
				break;
			}
			if (i == GameControllerScript.playerList.Length - 1) {
				following = null;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
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
		
	}

}
