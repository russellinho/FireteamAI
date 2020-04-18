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
    public MeshRenderer projectileRenderer;
    public ParticleSystem explosionEffect;
    public ParticleSystem smokeTrail;
    public float blastRadius;
    public AudioSource explosionSound;
    public AudioSource projectileSound;
    public int fromPlayerId;
    private ArrayList playersHit;
    public PhotonView pView;

    // Start is called before the first frame update
    void Awake()
    {
        playersHit = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
