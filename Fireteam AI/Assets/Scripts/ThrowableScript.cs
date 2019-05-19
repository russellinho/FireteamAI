using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ThrowableScript : MonoBehaviour
{
    private const float THROW_FORCE_MULTIPLIER = 3f;
    public Rigidbody rBody;
    public SphereCollider col;
    public float fuseTimer;
    public float blastRadius;
    private bool isLive;

    // Start is called before the first frame update
    void Awake()
    {
        isLive = false;
        TogglePhysics(false);
    }

    // Turns physics on/off so that when the user is holding the item, physics does not apply
    void TogglePhysics(bool b) {
        col.enabled = b;
        rBody.useGravity = b;
        rBody.isKinematic = !b;
    }

    public void Launch(float xForce, float yForce, float zForce) {
        // Turn physics on
        TogglePhysics(true);
        // Apply a force to the throwable that's equal to the forward position of the weapon holder
        rBody.AddForce(xForce * THROW_FORCE_MULTIPLIER, yForce * THROW_FORCE_MULTIPLIER, zForce * THROW_FORCE_MULTIPLIER);
        isLive = true;
    }

    void Update() {
        if (isLive) {
            fuseTimer -= Time.deltaTime;
            if (fuseTimer <= 0f) {
                Explode();
            }
        }
    }

    void Explode() {
        // TODO: Fill out - if frag, hurt enemies within blast radius scaled by distance from grenade
        // if flashbang, disorient the enemies within blast radius (go into disorientation animation)
        PhotonNetwork.Destroy(gameObject);
    }

}
