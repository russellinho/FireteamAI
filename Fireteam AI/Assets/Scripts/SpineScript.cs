using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineScript : MonoBehaviour {

	private BetaEnemyScript parent;

	// Use this for initialization
	void Start () {
		parent = GetComponentInParent<BetaEnemyScript> ();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (!parent) {
			GetComponentInParent<BetaEnemyScript> ();
			return;
		}
		if (parent.playerTargeting != null && parent.health > 0) {
			RotateTowardsPlayer ();
		}
	}

	void RotateTowardsPlayer() {
		transform.LookAt(parent.playerTargeting.transform);
		transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, 0f, 0f));
	}
}
