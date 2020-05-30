using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareScript : MonoBehaviour {

	public bool popped;
	public int flareId;

	// Use this for initialization
		
	public void PopFlare() {
		popped = true;
		gameObject.layer = 0;
		// TODO: Start flare particle effect here
	}

}
