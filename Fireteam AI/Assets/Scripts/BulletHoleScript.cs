using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleScript : MonoBehaviour {

	public ParticleSystem hitParticles;
	public AudioClip[] ricochetSounds;
	public AudioSource a;

	// Use this for initialization
	void Start () {
		hitParticles.Play();
		int r = Random.Range (0, ricochetSounds.Length);
		a.clip = ricochetSounds[r];
		a.Play ();
	}

}
