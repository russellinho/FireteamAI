using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LauncherScript : MonoBehaviour
{
    private const float LAUNCH_FORCE_MULTIPLIER = 30f;
    public Rigidbody rBody;
    public CapsuleCollider col;
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

    // Start is called before the first frame update
    void Awake()
    {
        playersHit = new ArrayList();
        explosionEffect.Stop();
        isLive = true;
    }

    void Update() {
        if (!isLive) {
            // Disable explosion collider
            col.enabled = false;
            if (!explosionEffect.IsAlive()) {
                DestroySelf();
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (PhotonNetwork.IsMasterClient) {
            if (collision.gameObject.layer != 4) {
                Explode();
            }
        }
    }

    [PunRPC]
    void RpcExplode() {
        // Freeze the physics
        isLive = false;
        rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
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
    }

    [PunRPC]
    void RpcSetPlayerLaunchedByReference(int playerViewId) {
        fromPlayerId = playerViewId;
    }
}
