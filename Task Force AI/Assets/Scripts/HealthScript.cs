using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour {

	[SerializeField] private float health = 100f;

	public void ApplyDamage(float damage) {
		Debug.Log ("Damage Received on " + transform.name + ": " + damage);
		health -= damage;

		if (health <= 0) {
			Destroy (gameObject);
		}
	}
}
