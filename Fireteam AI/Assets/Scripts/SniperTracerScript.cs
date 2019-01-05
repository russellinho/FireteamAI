using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTracerScript : MonoBehaviour {

	private float timer = 0f;
	public LineRenderer line;

	public void Reset () {
		SetAlpha (0.7f);
		timer = 1f;
	}

	void OnEnable() {
		Reset ();
	}

	public void SetDistance(float d) {
		transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, d + 10f);
	}

	void SetAlpha(float a) {
		line.startColor = new Color (line.startColor.r, line.startColor.g, line.startColor.b, a);
		line.endColor = new Color (line.endColor.r, line.endColor.g, line.endColor.b, a);
	}

	void FadeAlpha() {
		SetAlpha (line.startColor.a - Time.deltaTime);
	}

	private float GetAlpha() {
		return line.startColor.a;
	}
	
	void Update () {
		if (timer > 0f) {
			timer -= Time.deltaTime;
		} else {
			FadeAlpha ();
			if (GetAlpha() <= 0f) {
				line.enabled = false;
				this.enabled = false;
			}
		}
	}
}
