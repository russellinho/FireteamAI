using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionTextAnimScript : MonoBehaviour {

	private Text missionText;
	private bool fadeIn;
	private bool peak;

	// Use this for initialization
	void Start () {
		missionText = GetComponent<Text> ();
    }

	void OnEnable() {
		fadeIn = true;
		peak = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (fadeIn) {
            if (missionText.color.a < 1f)
            {
                missionText.color = new Color(missionText.color.r, missionText.color.g, missionText.color.b, missionText.color.a + (0.4f * Time.deltaTime));
            }
			if (!peak && missionText.color.a >= 1f) {
				peak = true;
				StartCoroutine(TextDelay ());
			}
		} else {
            if (missionText.color.a > 0f)
            {
                missionText.color = new Color(missionText.color.r, missionText.color.g, missionText.color.b, missionText.color.a - (0.4f * Time.deltaTime));
            }
            if (missionText.color.a <= 0f) {
				gameObject.SetActive (false);
			}
		}
	}

	IEnumerator TextDelay() {
		yield return new WaitForSeconds (2f);
		fadeIn = false;
	}
}
