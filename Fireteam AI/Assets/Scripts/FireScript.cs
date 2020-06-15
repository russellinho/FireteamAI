using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour {

	public ParticleSystem fireEffect;
    public ParticleSystem sparkEffect;
    public BoxCollider col;
    public bool isPermanent;
    public float damage;
    public float initialStartDelay;

    void Awake() {
        col.enabled = false;
        StartCoroutine(StartFireAfterDelay());
    }

    IEnumerator StartFireAfterDelay() {
        yield return new WaitForSeconds(initialStartDelay);
        fireEffect.Play();
        sparkEffect.Play();
        col.enabled = true;
    }

}
