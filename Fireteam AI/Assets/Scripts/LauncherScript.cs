using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LauncherScript : MonoBehaviour
{
    public string rootWeapon;
    private const int WATER_LAYER = 4;
    private const float LAUNCH_FORCE_MULTIPLIER = 45f;
    // RPG cannot be active for more than 12 seconds
    private const float MAX_ACTIVE_TIME = 12f;
    private const float EXPLOSION_ACTIVE_DELAY = 0.25f;
    public Rigidbody rBody;
    public SphereCollider col;
    public MeshRenderer[] projectileRenderer;
    public ParticleSystem explosionEffect;
    public ParticleSystem explosionWaterEffect;
    public ParticleSystem smokeTrail;
    public float blastRadius;
    public AudioSource explosionSound;
    public AudioSource projectileSound;
    public int fromPlayerId;
    public bool isLive;
    private ArrayList playersHit;
    public PhotonView pView;
    private float explosionActiveDelay;
    private float activeTime;
    private bool isInWater;
    private bool insideBubbleShield;

    // Start is called before the first frame update
    void Awake()
    {
        playersHit = new ArrayList();
        explosionEffect.Stop();
        explosionWaterEffect.Stop();
        isLive = true;
        activeTime = 0f;
    }

    void LateUpdate() {
        if (!isLive) {
            // Disable explosion collider
            explosionActiveDelay -= Time.deltaTime;
            if (explosionActiveDelay <= 0f) {
                col.enabled = false;
                if (!explosionEffect.IsAlive()) {
                    DestroySelf();
                }
            }
        } else {
            activeTime += Time.deltaTime;
            if (activeTime >= MAX_ACTIVE_TIME) {
                DestroySelf();
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (PhotonNetwork.IsMasterClient) {
            if (isLive && collision.gameObject.layer != 4 && !insideBubbleShield) {
                Explode();
            }
        }
    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject.layer == 22) {
            insideBubbleShield = false;
        }
    }

    [PunRPC]
    void RpcExplode() {
        // Freeze the physics
        isLive = false;
        rBody.velocity = Vector3.zero;
        rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rBody.isKinematic = true;
        // Create blast radius trigger collider - enemy will be affected if within this collider sphere during explosion
        EnableBlastCollider();
        // Make missile disappear
        foreach (MeshRenderer m in projectileRenderer) {
            m.enabled = false;
        }
        // Stop the projectile sound
        projectileSound.Stop();
        // Play the explosion sound
        explosionSound.Play();
        // Play the explosion particle effect
        if (isInWater) {
            explosionWaterEffect.Play();
        }
        explosionEffect.Play();
        // Set nearby enemies on alert from explosion sound
        GameControllerScript gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.SetLastGunshotHeardPos(false, transform.position);
    }

    public void Explode() {
      pView.RPC("RpcExplode", RpcTarget.All);
    }

    void EnableBlastCollider() {
        col.isTrigger = true;
        col.radius = blastRadius;
        explosionActiveDelay = EXPLOSION_ACTIVE_DELAY;
    }

    void DestroySelf() {
        Destroy(gameObject);
    }

    public void AddHitPlayer(int vId)
    {
        playersHit.Add(vId);
    }

    public bool PlayerHasBeenAffected(int vId)
    {
        return playersHit.Contains(vId);
    }

    public void Launch(int thrownByPlayerViewId, float xForce, float yForce, float zForce) {
        // Assign a reference to the player who launched this projectile
        // pView.RPC("RpcSetPlayerLaunchedByReference", RpcTarget.All, thrownByPlayer.GetComponent<PhotonView>().ViewID);
        SetPlayerLaunchedByReference(thrownByPlayerViewId);
        // Apply a force to the throwable that's equal to the forward position of the weapon holder
        rBody.velocity = new Vector3(xForce * LAUNCH_FORCE_MULTIPLIER, yForce * LAUNCH_FORCE_MULTIPLIER, zForce * LAUNCH_FORCE_MULTIPLIER);
        isLive = true;
        rBody.freezeRotation = true;
    }

    [PunRPC]
    void RpcSetPlayerLaunchedByReference(int playerViewId) {
        fromPlayerId = playerViewId;
    }

    void SetPlayerLaunchedByReference(int playerViewId) {
        fromPlayerId = playerViewId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == WATER_LAYER) {
            isInWater = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == WATER_LAYER) {
            isInWater = false;
        }
    }

}
