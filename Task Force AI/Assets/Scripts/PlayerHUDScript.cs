using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDScript : MonoBehaviour {

	private const string HEALTH_TEXT = "Health: ";
	private Text healthText;
	private GameObject hitFlare;
	private GameObject hitDir;

	private Text weaponLabelTxt;
	private Text ammoTxt;

	// Use this for initialization
	void Start () {
		/**hitFlare = GameObject.Find ("HitFlare");
		hitDir = GameObject.Find ("HitDir");
		hitFlare.GetComponent<RawImage> ().enabled = false;
		hitDir.GetComponent<RawImage> ().enabled = false;*/

		healthText = GameObject.Find ("HealthBar").GetComponent<Text>();
		weaponLabelTxt = GameObject.Find("WeaponLabel").GetComponent<Text>();
		ammoTxt = GameObject.Find ("AmmoCount").GetComponent<Text>();
		//gameController.GetComponent<GameControllerScript> ().hudMap.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		//		healthText.text = HEALTH_TEXT + health;

		// Update UI
		//		weaponLabelTxt.text = currWep;
		//ammoTxt.text = "" + wepScript.currentBullets + '/' + wepScript.totalBulletsLeft;
	}

	void FixedUpdate() {
		UpdateHitFlare();
	}

	void UpdateHitFlare() {
		// Hit timer is set to 0 every time the player is hit, if player has been hit recently, make sure the hit flare and dir is set
		/**if (hitTimer < 1f) {
			//hitFlare.GetComponent<RawImage> ().enabled = true;
			//hitDir.GetComponent<RawImage> ().enabled = true;
			hitTimer += Time.deltaTime;
		} else {
			//hitFlare.GetComponent<RawImage> ().enabled = false;
			//hitDir.GetComponent<RawImage> ().enabled = false;
			float a = Vector3.Angle (transform.forward,hitLocation);
			//Vector3 temp = hitDir.GetComponent<RectTransform> ().rotation.eulerAngles;
			//hitDir.GetComponent<RectTransform> ().rotation = Quaternion.Euler (new Vector3(temp.x,temp.y,a));
		}*/
	}
}
