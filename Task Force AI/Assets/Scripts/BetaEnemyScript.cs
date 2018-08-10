using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class BetaEnemyScript : NetworkBehaviour {

	enum ActionStates {Idle, Firing, Moving, Dead, Reloading, Melee, Pursue};
	public enum EnemyType {Patrol, Scout};

	// Gun stuff
	private AudioSource audioSource;

	public float range;
	public int bulletsPerMag = 30;
	public int currentBullets;
	public AudioClip shootSound;

	public Transform shootPoint;
	public ParticleSystem muzzleFlash;
	private bool isReloading = false;

	public GameObject hitParticles;
	public GameObject bulletImpact;
	public GameObject bloodEffect;

	public float fireRate = 0.4f;
	public float damage = 10f;

	[SyncVar] public EnemyType enemyType;

	// Once it equals fireRate, it will allow us to shoot
	float fireTimer = 0.0f;

	//-----------------------------------------------------

	// TODO: Player detection system needs to be refactored for networking
	private GameObject player;
	private Transform lastSeenPlayerPos;
	private Animator animator;
	[SyncVar] public int health;
	private Rigidbody rigid;

	private float rotationSpeed = 6f;

	// All patrol pathfinding points for an enemy
	public GameObject[] navPoints;
	[SyncVar] public int currNavPointIndex;
	[SyncVar] private ActionStates actionState;
	private bool isCrouching;

	private Transform spine;

	private GameObject[] players;

	private NavMeshAgent navMesh;
	private AudioSource aud;

	private float alertTimer = 0f;
	// Time in cover
	private float coverTimer = 0f;
	// Time to wait to be in cover again
	private float coverWaitTimer = 0f;
	private bool inCover;
	private int crouchMode = 2;

	// Use this for initialization
	void Start () {

		player = null;
		spine = GetComponentInChildren<SpineScript> ().gameObject.transform;
		animator = GetComponent<Animator> ();
		players = new GameObject[8];
		health = 100;
		currentBullets = bulletsPerMag;
		audioSource = GetComponent<AudioSource> ();
		rigid = GetComponent<Rigidbody> ();
		isCrouching = false;
		// Get nav points
		navMesh = GetComponent<NavMeshAgent>();
		navPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
		GetComponent<Rigidbody> ().freezeRotation = true;
		aud = GetComponent<AudioSource> ();
		coverTimer = 0f;
		coverWaitTimer = Random.Range (4f, 15f);
		inCover = false;
	}

	void PlayerScan() {
		// If we do not have a target player, try to find one
		if (player == null) {
			ArrayList indicesNearBy = new ArrayList ();
			for (int i = 0; i < GameControllerScript.playerList.Count; i++) {
				GameObject p = (GameObject)GameControllerScript.playerList [i];
				if (Vector3.Distance (transform.position, p.transform.position) < range + 30f) {
					Vector3 toPlayer = p.transform.position - transform.position;
					float angleBetween = Vector3.Angle (transform.forward, toPlayer);
					if (angleBetween <= 60f) {
						indicesNearBy.Add (i);
					}
				}
			}
			if (indicesNearBy.Count != 0) player = (GameObject) GameControllerScript.playerList [Random.Range (0, indicesNearBy.Count)];
		} else {
			// If we do, check if it's still in range
			if (Vector3.Distance (transform.position, player.transform.position) >= range + 30f) {
				lastSeenPlayerPos = player.transform;
				player = null;

			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Die")) {
			return;
		}
			
		if (inCover)
			isCrouching = true;
		else
			isCrouching = false;
		//Debug.DrawRay (transform.position, transform.forward * range, Color.blue);

		if (enemyType == EnemyType.Patrol) {
			DecideActionPatrol ();
			Movement ();
		} else {
			DecideActionScout ();
		}

		// Shoot at player
		if (actionState == ActionStates.Firing) {
			if (currentBullets > 0) {
				Fire ();
			}
		}

		if (fireTimer < fireRate) {
			fireTimer += Time.deltaTime;
		}

		//Debug.Log ("Spine: " + spine.transform.rotation.x + "," + spine.transform.rotation.y + "," + spine.transform.rotation.z);

		if (alertTimer > 0f) {
			alertTimer -= Time.deltaTime;
		}

		if (coverTimer > 0f && actionState != ActionStates.Idle) {
			coverTimer -= Time.deltaTime;
		}

		if (coverWaitTimer > 0f && actionState != ActionStates.Idle) {
			coverWaitTimer -= Time.deltaTime;
		}
		//Debug.Log (coverWaitTimer + " " + inCover);
	}

	void FixedUpdate() {
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
			return;
		DecideAnimation ();
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
		isReloading = (info.IsName ("Reloading") || info.IsName("CrouchReload"));
	}

	void LateUpdate() {
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
			return;
		// If the enemy sees the player, rotate the enemy towards the player
		if (player != null) {
			Vector3 rotDir = (player.transform.position - transform.position).normalized;
			Quaternion lookRot = Quaternion.LookRotation (rotDir);
			Quaternion tempQuat = Quaternion.Slerp (transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
			Vector3 tempRot = tempQuat.eulerAngles;
			//tempRot = new Vector3 (0f, tempRot.y, 0f);
			transform.rotation = Quaternion.Euler (new Vector3 (0f, tempRot.y, 0f));
			//spine.transform.localRotation = Quaternion.Euler (new Vector3 (tempRot.x, 0f, 0f));
			spine.transform.forward = new Vector3(spine.transform.forward.x, player.transform.position.y - spine.transform.position.y + 0.3f, spine.transform.forward.z);
		}

	}

	void Movement() {
		// If the player is not in sight, move between waypoints
		if (actionState == ActionStates.Pursue) {
			navMesh.isStopped = false;
			navMesh.SetDestination (lastSeenPlayerPos.position);
			return;
		}
		if (player == null) {
			if (!navMesh.hasPath) {
				int r = Random.Range (0, navPoints.Length);
				RotateTowards (navPoints [r].transform.position);
				navMesh.SetDestination (navPoints [r].transform.position);
				navMesh.isStopped = false;
			}
		} else {
			// If the player has been spotted
			// If the player is too far, move closer
			if (Vector3.Distance (transform.position, player.transform.position) > range) {
				navMesh.isStopped = false;
				navMesh.SetDestination (player.transform.position);
			} else {
				// Else, stop and shoot at player
				navMesh.isStopped = true;
			}
		}
	}

	// Decision tree for scout type enemy
	void DecideActionScout() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead) {
			actionState = ActionStates.Dead;
			// Choose a death sound
			int r = Random.Range (0, 3);
			if (r == 0) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt1");
			} else if (r == 1) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt2");
			} else {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt4");
			}
			aud.Play ();

			GetComponentInChildren<SpriteRenderer> ().enabled = false;
			//GetComponent<CapsuleCollider> ().isTrigger = true;
			StartCoroutine(Despawn ());
			return;
		}

		// Continue with decision tree
		// Scan for a target player
		PlayerScan();
		// Sees a player?
		if (player != null) {
			alertTimer = 10f;

			if (Vector3.Distance (player.transform.position, transform.position) <= 2.3f) {
				actionState = ActionStates.Melee;
			} else {
				if (currentBullets > 0) {
					actionState = ActionStates.Firing;
					if (crouchMode == 0)
						crouchMode = 1;
					else if (crouchMode == 1)
						crouchMode = 2;
					TakeCover ();
				} else {
					actionState = ActionStates.Reloading;
					crouchMode = 0;
					TakeCover ();
				}
			}
		} else {
			if (alertTimer > 0f) {
				crouchMode = 0;
				TakeCover ();
			} else {
				crouchMode = 1;
				TakeCover ();
			}
			actionState = ActionStates.Idle;
		}
	}

	// Decision tree for patrol type enemy
	void DecideActionPatrol() {
		if (actionState != ActionStates.Dead) {
			bool wasInSight = player == null ? false : true;
			PlayerScan();
			if (player != null) {
				if (Vector3.Distance (transform.position, player.transform.position) <= 2.3f) {
					actionState = ActionStates.Melee;
				} else if (currentBullets > 0) {
					actionState = ActionStates.Firing;
				} else {
					actionState = ActionStates.Reloading;
				}
			} else {
				// If the player was in sight but is now too far, then pursue the player
				if (wasInSight) {
					actionState = ActionStates.Pursue;
				} else {
					actionState = ActionStates.Idle;
				}
			}

			if (navMesh.hasPath) {
				if (actionState != ActionStates.Firing && actionState != ActionStates.Reloading && actionState != ActionStates.Melee) {
					actionState = ActionStates.Moving;
				}
			}

			if (!navMesh.hasPath) {
				if (actionState != ActionStates.Firing && actionState != ActionStates.Reloading && actionState != ActionStates.Melee) {
					actionState = ActionStates.Idle;
				}
			}
		}

		if (health <= 0 && actionState != ActionStates.Dead) {
			actionState = ActionStates.Dead;
			// Choose a death sound
			int r = Random.Range (0, 3);
			if (r == 0) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt1");
			} else if (r == 1) {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt2");
			} else {
				aud.clip = (AudioClip)Resources.Load ("Grunts/grunt4");
			}
			aud.Play ();

			GetComponentInChildren<SpriteRenderer> ().enabled = false;
			//GetComponent<CapsuleCollider> ().isTrigger = true;
			StartCoroutine(Despawn ());
		}
	}

	void DecideAnimation() {
		if (actionState == ActionStates.Dead) {
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die")) animator.Play ("Die");
		}

		if (actionState == ActionStates.Pursue) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Sprint"))
				animator.Play ("Sprint");
		}

		if (actionState == ActionStates.Moving) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Moving"))
				animator.Play ("Moving");
		}

		if (actionState == ActionStates.Idle) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
				animator.Play ("Idle");
		}

		if (actionState == ActionStates.Firing || actionState == ActionStates.Reloading) {

			// Set proper animation
			if (currentBullets > 0) {
				if (isCrouching) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Crouching"))
						animator.Play ("Crouching");
				} else {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Firing"))
						animator.Play ("Firing");
				}
			} else {
				if (isCrouching) {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("CrouchReload"))
						animator.Play ("CrouchReload");
				} else {
					if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Reloading"))
						animator.Play ("Reloading");
				}
			}

		}

		if (actionState == ActionStates.Melee) {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("Melee")) {
				animator.Play ("Melee");
			}
		}
	}

	private void Fire() {
		if (fireTimer < fireRate || currentBullets < 0 || isReloading)
			return;

		if (player != null) {
			RaycastHit hit;
			// Locks onto the player and shoots at him
			Vector3 dir = player.GetComponentsInChildren<Transform>()[0].position - shootPoint.position;

			// Adding artificial stupidity - ensures that the player isn't hit every time by offsetting
			// the shooting direction in x and y by two random numbers
			float xOffset = Random.Range (-2.5f, 2.5f);
			float yOffset = Random.Range (-2.5f, 2.5f);
			dir = new Vector3 (dir.x + xOffset, dir.y + yOffset, dir.z);
			Debug.DrawRay (shootPoint.position, dir * range, Color.red);
			if (Physics.Raycast (shootPoint.position, dir, out hit)) {
				GameObject bloodSpill = null;
				if (hit.transform.tag.Equals ("Player") || hit.transform.tag.Equals ("Human")) {
					bloodSpill = Instantiate (bloodEffect, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
					bloodSpill.transform.Rotate (180f, 0f, 0f);
					Debug.Log (transform.name + " has hit you");
					if (hit.transform.tag.Equals ("Player")) {
						hit.transform.GetComponent<PlayerScript> ().health -= (int)damage;
						hit.transform.GetComponent<PlayerScript> ().hitTimer = 0f;
						hit.transform.GetComponent<PlayerScript> ().hitLocation = transform.position;
					} else {
						hit.transform.GetComponent<BetaEnemyScript> ().health -= (int)damage;
					}
				} else {
					GameObject hitParticleEffect = Instantiate (hitParticles, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
					GameObject bulletHoleEffect = Instantiate (bulletImpact, hit.point, Quaternion.FromToRotation (Vector3.forward, hit.normal));
					bulletHoleEffect.transform.SetParent (hit.transform);
					Destroy (hitParticleEffect, 1f);
					Destroy (bulletHoleEffect, 3f);
				}
				if (bloodSpill != null)
					Destroy (bloodSpill, 1.5f);
			}
		}

		//animator.CrossFadeInFixedTime ("Firing", 0.01f);
		muzzleFlash.Play ();
		PlayShootSound ();
		currentBullets--;
		// Reset fire timer
		fireTimer = 0.0f;
	}

	private void PlayShootSound() {
		audioSource.PlayOneShot (shootSound);
	}

	public void Reload() {
		int bulletsToLoad = bulletsPerMag - currentBullets;
		currentBullets += bulletsPerMag;
	}

	public void MeleeAttack() {
		if (player != null) {
			player.GetComponent<PlayerScript> ().health -= 50;
			player.transform.GetComponent<PlayerScript> ().hitTimer = 0f;
		}
	}

	/**private void ReloadAction() {
		if (isReloading)
			return;
		//animator.CrossFadeInFixedTime ("Reloading", 0.1f);
		animator.Play("Reloading");
	}*/

	private void RotateTowards(Vector3 r) {
		Vector3 rotDir = (r - transform.position).normalized;
		Quaternion lookRot = Quaternion.LookRotation (rotDir);
		Quaternion tempQuat = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
		Vector3 tempRot = tempQuat.eulerAngles;
		tempRot = new Vector3 (0f, tempRot.y, 0f);
		transform.rotation = Quaternion.Euler(tempRot);
	}

	IEnumerator Despawn() {
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
	}

	// b is the mode the AI is in. 0 means override everything and take cover, 1 is override everything and leave cover
	// 2 is use the natural timer to decide
	void TakeCover() {
		if (crouchMode == 0) {
			coverWaitTimer = 0f;
			inCover = true;
		} else if (crouchMode == 1) {
			if (coverWaitTimer <= 0f) coverWaitTimer = Random.Range (4f, 15f);
			inCover = false;
		} else {
			if (coverWaitTimer <= 0f && !inCover) {
				coverTimer = Random.Range (3f, 7f);
				inCover = true;
			} else if (coverTimer <= 0f && inCover) {
				coverWaitTimer = Random.Range (4f,15f);
				inCover = false;
			}
		}
	}

}
