using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour {

	public ParticleSystem fireEffect;
    public bool isPermanent;
    public float damage;
    public float initialStartDelay;

    void Awake() {
        StartCoroutine(StartFireAfterDelay());
    }

    IEnumerator StartFireAfterDelay() {
        yield return new WaitForSeconds(initialStartDelay);
        fireEffect.Play();
    }

}
