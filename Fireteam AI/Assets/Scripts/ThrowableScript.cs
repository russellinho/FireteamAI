using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ThrowableScript : MonoBehaviour
{
    private const float THROW_FORCE_MULTIPLIER = 25f;
    public static float MAX_FLASHBANG_TIME = 9f; // 8 seconds max flashbang time
    private const float EXPLOSION_ACTIVE_DELAY = 1.5f;
    public Rigidbody rBody;
    public SphereCollider col;
    public MeshRenderer[] renderers;
    public ParticleSystem explosionEffect;
    private float explosionActiveDelay;
    public float fuseTimer;
    public bool explosionDelay;
    private float explosionDelayTimer;
    public float blastRadius;
    public bool isLive;
    public AudioSource explosionSound;
    public AudioSource pinSound;
    public int fromPlayerId;
    private ArrayList playersHit;
    public PhotonView pView;


    // Start is called before the first frame update
    void Awake()
    {
        playersHit = new ArrayList();
        explosionEffect.Stop();
        isLive = true;
        explosionActiveDelay = EXPLOSION_ACTIVE_DELAY;
    }

    public void AddHitPlayer(int vId)
    {
        playersHit.Add(vId);
    }

    public bool PlayerHasBeenAffected(int vId)
    {
        return playersHit.Contains(vId);
    }

    public void Launch(GameObject thrownByPlayer, float xForce, float yForce, float zForce) {
        // Assign a reference to the player who threw this projectile
        pView.RPC("RpcSetPlayerThrownByReference", RpcTarget.All, thrownByPlayer.GetComponent<PhotonView>().ViewID);
        // Apply a force to the throwable that's equal to the forward position of the weapon holder
        rBody.velocity = new Vector3(xForce * THROW_FORCE_MULTIPLIER, yForce * THROW_FORCE_MULTIPLIER, zForce * THROW_FORCE_MULTIPLIER);
        //rBody.AddForce(xForce * THROW_FORCE_MULTIPLIER, yForce * THROW_FORCE_MULTIPLIER, zForce * THROW_FORCE_MULTIPLIER);
        isLive = true;
    }

    [PunRPC]
    void RpcSetPlayerThrownByReference(int playerViewId) {
        fromPlayerId = playerViewId;
    }

    void LateUpdate() {
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
            explosionActiveDelay -= Time.deltaTime;
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
                    if (explosionActiveDelay <= 0f) {
                        col.enabled = false;
                        if (!explosionEffect.IsAlive()) {
                            DestroySelf();
                        }
                    }
                }
            } else {
                if (fuseTimer <= 0f) {
                    // Disable explosion collider
                    if (explosionActiveDelay <= 0f) {
                        col.enabled = false;
                        if (!explosionEffect.IsAlive()) {
                            DestroySelf();
                        }
                    }
                }
            }
        }
    }
    [PunRPC]
    void RpcExplode() {
        // Freeze the physics
        isLive = false;
        rBody.useGravity = false;
        rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
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
        // Set nearby enemies on alert from explosion sound
        GameControllerScript gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
    }

    // Same method as above except this one has a delay on it for the collision to register
    [PunRPC]
    void RpcExplodeDelay() {
        // Freeze the physics
        rBody.useGravity = false;
        rBody.isKinematic = true;
        // Set the time to enable collision after 0.15 seconds
        explosionDelayTimer = 0.15f;
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

    public void ExplodeDelay() {
      pView.RPC("RpcExplodeDelay", RpcTarget.All);
    }

    public void Explode() {
      pView.RPC("RpcExplode", RpcTarget.All);
    }

    void EnableBlastCollider() {
        col.isTrigger = true;
        col.radius = blastRadius;
    }

    void DestroySelf() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void PlayPinSound() {
        pinSound.Play();
    }

}
