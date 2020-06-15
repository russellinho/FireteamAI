using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareScript : MonoBehaviour {

	public bool popped;
	public int flareId;
	public ParticleSystem flareEffect;

	// Use this for initialization
		
	public void PopFlare() {
		popped = true;
		gameObject.layer = 0;
		flareEffect.gameObject.SetActive(true);
		flareEffect.Play();
	}

}
