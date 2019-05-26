﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ThrowableScript : MonoBehaviour
{
    private const float THROW_FORCE_MULTIPLIER = 25f;
    public static float FLASHBANG_TIME = 10f; // 10 seconds max flashbang time
    public Rigidbody rBody;
    public SphereCollider col;
    public MeshRenderer[] renderers;
    public ParticleSystem explosionEffect;
    public float fuseTimer;
    public bool explosionDelay;
    private float explosionDelayTimer;
    public float blastRadius;
    private bool isLive;
    private float explosionDuration;
    public AudioSource explosionSound;
    public AudioSource pinSound;

    // Start is called before the first frame update
    void Awake()
    {
        explosionEffect.Stop();
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
        rBody.velocity = new Vector3(xForce * THROW_FORCE_MULTIPLIER, yForce * THROW_FORCE_MULTIPLIER, zForce * THROW_FORCE_MULTIPLIER);
        //rBody.AddForce(xForce * THROW_FORCE_MULTIPLIER, yForce * THROW_FORCE_MULTIPLIER, zForce * THROW_FORCE_MULTIPLIER);
        isLive = true;
    }

    void Update() {
        if (isLive) {
            fuseTimer -= Time.deltaTime;
            if (fuseTimer <= 0f) {
                if (explosionDelay) {
                    ExplodeDelay();
                } else {
                    Explode();
                }
            }
        } else {
            if (explosionDelay) {
                if (explosionDelayTimer > 0f) {
                    explosionDelayTimer -= Time.deltaTime;
                    if (explosionDelayTimer <= 0f) {
                        EnableBlastCollider();
                        return;
                    }
                }
                if (fuseTimer <= 0f && explosionDelayTimer <= 0f) {
                    // Disable explosion collider 
                    col.enabled = false;
                    if (!explosionEffect.IsAlive()) {
                        DestroySelf();
                    }
                }
            } else {
                if (fuseTimer <= 0f) {
                    // Disable explosion collider 
                    col.enabled = false;
                    if (!explosionEffect.IsAlive()) {
                        DestroySelf();
                    }
                }
            }
        }
    }

    void Explode() {
        // Freeze the physics
        rBody.useGravity = false;
        rBody.isKinematic = true;
        // Create blast radius trigger collider - enemy will be affected if within this collider sphere during explosion
        EnableBlastCollider();
        // Make grenade disappear
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].enabled = false;
        }
        // Play the explosion sound
        explosionSound.Play();
        // Play the explosion particle effect
        explosionEffect.Play();
        isLive = false;
        // Set nearby enemies on alert from explosion sound
        GameControllerScript gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
    }

    // Same method as above except this one has a delay on it for the collision to register
    void ExplodeDelay() {
        // Freeze the physics
        rBody.useGravity = false;
        rBody.isKinematic = true;
        // Set the time to enable collision after 1.5 seconds
        explosionDelayTimer = 1.5f;
        // Make grenade disappear
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].enabled = false;
        }
        // Play the explosion sound
        explosionSound.Play();
        // Play the explosion particle effect
        explosionEffect.Play();
        isLive = false;
        // Set nearby enemies on alert from explosion sound
        GameControllerScript gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
    }

    void EnableBlastCollider() {
        col.isTrigger = true;
        col.radius = blastRadius;
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    public void PlayPinSound() {
        pinSound.Play();
    }

}
