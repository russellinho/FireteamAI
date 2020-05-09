using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LauncherScript : MonoBehaviour
{
    private const float LAUNCH_FORCE_MULTIPLIER = 45f;
    // RPG cannot be active for more than 12 seconds
    private const float MAX_ACTIVE_TIME = 12f;
    private const float EXPLOSION_ACTIVE_DELAY = 1.5f;
    public Rigidbody rBody;
    public SphereCollider col;
    public MeshRenderer[] projectileRenderer;
    public ParticleSystem explosionEffect;
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

    // Start is called before the first frame update
    void Awake()
    {
        playersHit = new ArrayList();
        explosionEffect.Stop();
        isLive = true;
        explosionActiveDelay = EXPLOSION_ACTIVE_DELAY;
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
            if (isLive && collision.gameObject.layer != 4) {
                Explode();
            }
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
        explosionEffect.Play();
        // Set nearby enemies on alert from explosion sound
        GameControllerScript gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
        gameController.SetLastGunshotHeardPos(transform.position.x, transform.position.y, transform.position.z);
    }

    public void Explode() {
      pView.RPC("RpcExplode", RpcTarget.All);
    }

    void EnableBlastCollider() {
        col.isTrigger = true;
        col.radius = blastRadius;
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
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
        // Assign a reference to the player who launched this projectile
        pView.RPC("RpcSetPlayerLaunchedByReference", RpcTarget.All, thrownByPlayer.GetComponent<PhotonView>().ViewID);
        // Apply a force to the throwable that's equal to the forward position of the weapon holder
        rBody.velocity = new Vector3(xForce * LAUNCH_FORCE_MULTIPLIER, yForce * LAUNCH_FORCE_MULTIPLIER, zForce * LAUNCH_FORCE_MULTIPLIER);
        isLive = true;
        rBody.freezeRotation = true;
    }

    [PunRPC]
    void RpcSetPlayerLaunchedByReference(int playerViewId) {
        fromPlayerId = playerViewId;
    }
}
