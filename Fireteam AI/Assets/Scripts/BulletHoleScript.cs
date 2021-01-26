using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleScript : MonoBehaviour {
	public bool skipRicochetAmbient;
	public ParticleSystem hitParticles;
	public AudioClip[] ricochetSounds;
	public AudioClip[] ricochetAmbient;
	public AudioSource a;
	public AudioSource aAmbient;

	// Use this for initialization
	void Start () {
		hitParticles.Play();
		int r = Random.Range (0, ricochetSounds.Length);
		a.clip = ricochetSounds[r];
		a.Play ();

		if (!skipRicochetAmbient) {
			r = Random.Range (0, ricochetAmbient.Length * 2);
			if (r < ricochetAmbient.Length) {
				aAmbient.clip = ricochetAmbient[r];
				aAmbient.Play();
			}
		}
	}

}
