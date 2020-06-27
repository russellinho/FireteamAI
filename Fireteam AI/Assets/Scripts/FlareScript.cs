using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareScript : MonoBehaviour {

	public bool popped;
	public int flareId;
	public ParticleSystem flareEffect;
	public MeshRenderer[] rends;
	public Material validMat;
	public Material originalMat;

	// Use this for initialization

	public void ToggleFlareTemplate(bool b) {
		if (b) {
			for (int i = 0; i < rends.Length; i++) {
				rends[i].material = validMat;
			}
        } else {
			for (int i = 0; i < rends.Length; i++) {
				rends[i].material = originalMat;
			}
        }
	}
		
	public void PopFlare() {
		popped = true;
		ToggleFlareTemplate(false);
		gameObject.layer = 0;
		flareEffect.gameObject.SetActive(true);
		flareEffect.Play();
	}

}
