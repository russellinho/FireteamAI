using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionTextAnimScript : MonoBehaviour {

	private TextMeshProUGUI missionText;
	private bool fadeIn;
	private bool peak;
	private bool started;
	private float t;
	private AudioSource soundEffect;

	// Use this for initialization
	void Start () {
		missionText = GetComponent<TextMeshProUGUI> ();
		soundEffect = GetComponent<AudioSource> ();
		Reset ();
    }

	public void SetStarted() {
		started = true;
		soundEffect.Play ();
	}

	public void Reset() {
		soundEffect.Stop ();
		soundEffect.time = 0f;
		t = 0f;
		fadeIn = true;
		peak = false;
		started = false;
		missionText.rectTransform.localScale = new Vector3 (15f, 1f, 1f);
		missionText.color = new Color(missionText.color.r, missionText.color.g, missionText.color.b, 0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (started) {
			if (fadeIn) {
	            if (missionText.color.a < 1f)
	            {
	                missionText.color = new Color(missionText.color.r, missionText.color.g, missionText.color.b, missionText.color.a + (0.4f * Time.deltaTime));
	            }
				if (!peak) {
					t += (Time.deltaTime * 1.5f);
					missionText.rectTransform.localScale = Vector3.Lerp (new Vector3 (15f, 1f, 1f), new Vector3 (1f, 1f, 1f), t);	
					if (missionText.color.a >= 1f) {
						peak = true;
						t = 0f;
					}
				}
				if (peak) {
					t += Time.deltaTime;
					if (t >= 2f) {
						fadeIn = false;
						t = 0f;
					}
				}
			} else {
	            if (missionText.color.a > 0f)
	            {
					t += (Time.deltaTime * 1.5f);
					missionText.rectTransform.localScale = Vector3.Lerp (new Vector3 (1f, 1f, 1f), new Vector3 (15f, 1f, 1f), t);
	                missionText.color = new Color(missionText.color.r, missionText.color.g, missionText.color.b, missionText.color.a - (1.5f * Time.deltaTime));
	            }
	            if (missionText.color.a <= 0f) {
					started = false;
				}
			}
		}
	}
}
