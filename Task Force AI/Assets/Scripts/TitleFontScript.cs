using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleFontScript : MonoBehaviour {

	private float fadeTimer;
	public Text text;

	void OnEnable() {
		fadeTimer = 0f;
		text.color = new Color (text.color.r, text.color.g, text.color.b, fadeTimer);
	}

	// Update is called once per frame
	void Update () {
		if (fadeTimer < 1f) {
			fadeTimer += (Time.deltaTime / 1.5f);
			text.color = text.color = new Color (text.color.r, text.color.g, text.color.b, fadeTimer);
		}
	}
}
