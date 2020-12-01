using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleScript : MonoBehaviour {

	public GameObject hitParticles;
	public AudioClip ricochet1;
	public AudioClip ricochet2;
	public AudioClip ricochet3;
	public AudioSource a;

	// Use this for initialization
	void Start () {
		int r = Random.Range (0, 3);
		if (r == 0) {
			a.clip = ricochet1;
		} else if (r == 1) {
			a.clip = ricochet2;
		} else {
			a.clip = ricochet3;
		}
		a.Play ();
	}

}
