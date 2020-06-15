using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NpcScript : MonoBehaviourPunCallbacks {

	private const float ENV_DAMAGE_DELAY = 0.5f;
	public enum NpcType {Neutral, Friendly};
	public GameControllerScript gameController;
	public PhotonView pView;
	public int health;
	public bool godMode;
	public int carriedByPlayerId;
	public enum ActionStates {Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking, Disoriented, Carried, Escorted, Injured, FatallyInjured};
	// FSM used for determining movement while attacking and not in cover
	enum FiringStates {StandingStill, StrafeLeft, StrafeRight, Backpedal, Forward};
	public ActionStates actionState;
	private FiringStates firingState;
	// If true, the player can sprint, crouch, jump, walk while carrying
	public bool immobileWhileCarrying;
	// Amount of speed to reduce while carrying
	public float weightSpeedReduction;
	private int deathBy;
	public NpcType npcType;
	public AudioClip[] gruntSounds;
	public AudioSource audioSource;
	private Transform carriedByTransform;
	public SkinnedMeshRenderer[] rends;
	public CapsuleCollider col;
	public Animator animator;
	public Rigidbody rBody;
	public Transform headTransform;
	// Timers
	private float envDamageTimer;
	private float disorientationTime;

	// Use this for initialization
	void Awake() {
		carriedByPlayerId = -1;
	}

	void Update()
    {
        if (gameController.matchType == 'C')
        {
            UpdateForCampaign();
        } else if (gameController.matchType == 'V')
        {
            UpdateForVersus();
        }
    }

	void UpdateForCampaign() {
		UpdateEnvDamageTimer();

		if (!PhotonNetwork.IsMasterClient) {
			return;
		}

		// If disoriented, don't have the ability to do anything else except die
		if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
			// StopVoices();
			return;
		}

		DecideAction();
	}

	void UpdateForVersus() {
		UpdateEnvDamageTimer();

		if (!gameController.isVersusHostForThisTeam()) {
			return;
		}

		// If disoriented, don't have the ability to do anything else except die
		if (actionState == ActionStates.Disoriented || actionState == ActionStates.Dead) {
			// StopVoices();
			return;
		}

		DecideAction();
	}

	void FixedUpdate() {
		if (gameController.matchType == 'C') {
			FixedUpdateForCampaign();
		} else if (gameController.matchType == 'V') {
			FixedUpdateForVersus();
		}
	}

	void FixedUpdateForCampaign() {
		DecideAnimation();
	}

	void FixedUpdateForVersus() {
		DecideAnimation();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
    {
		if (otherPlayer.ActorNumber == carriedByPlayerId) {
			ToggleIsCarrying(false, -1);
		}
    }
		
	public void ToggleIsCarrying(bool b, int carriedByPlayerId) {
		this.carriedByPlayerId = carriedByPlayerId;
		if (carriedByPlayerId == -1) {
			actionState = ActionStates.Carried;
			carriedByTransform = null;
			ToggleRenderers(true);
			ToggleCollider(true);
		} else {
			actionState = ActionStates.FatallyInjured;
			carriedByTransform = GameControllerScript.playerList[carriedByPlayerId].carryingSlotRef;
			ToggleCollider(false);
			if (carriedByPlayerId == PhotonNetwork.LocalPlayer.ActorNumber) {
				ToggleRenderers(false);
			}
		}
	}
	
	void LateUpdate() {
		if (actionState == ActionStates.Carried) {
			if (carriedByTransform != null) {
				transform.position = carriedByTransform.position;
			}
		}
	}

	public void TakeDamage(int d) {
		if (godMode) return;
		pView.RPC ("RpcTakeDamage", RpcTarget.All, d, gameController.teamMap);
	}

	[PunRPC]
	public void RpcTakeDamage(int d, string team) {
        if (team != gameController.teamMap) return;
        health -= d;
	}

	public void PlayGruntSound() {
		pView.RPC("RpcPlayGruntSound", RpcTarget.All);
	}

	[PunRPC]
	public void RpcPlayGruntSound() {
		if (gruntSounds.Length == 0) return;
		int r = Random.Range(0, gruntSounds.Length);
		audioSource.clip = gruntSounds [r];
		audioSource.Play ();
	}

	void ToggleCollider(bool b) {
		col.enabled = b;
		if (b) {
			rBody.isKinematic = false;
			rBody.useGravity = true;
		} else {
			rBody.isKinematic = true;
			rBody.useGravity = false;
		}
	}

	void ToggleRenderers(bool b) {
		foreach (SkinnedMeshRenderer m in rends) {
			m.enabled = b;
		}
	}

	void DecideAnimation() {
		if (actionState == ActionStates.Dead) {
			animator.SetBool("Dead", true);
		} else {
			animator.SetBool("Dead", false);
		}

		if (actionState == ActionStates.Carried) {
			animator.SetBool("isBeingCarried", true);
		} else {
			animator.SetBool("isBeingCarried", false);
		}

		if (actionState == ActionStates.FatallyInjured) {
			animator.SetBool("isFatallyInjured", true);
		} else {
			animator.SetBool("isFatallyInjured", false);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (gameController.matchType == 'V') {
			OnTriggerEnterForVersus(other);
		} else {
			OnTriggerEnterForCampaign(other);
		}
	}

	void OnTriggerEnterForCampaign(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (PhotonNetwork.IsMasterClient) {
			HandleExplosiveEffectTriggers(other);
			HandleEnvironmentEffectTriggers(other);
		}
	}

	void OnTriggerEnterForVersus(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (gameController.isVersusHostForThisTeam()) {
			HandleExplosiveEffectTriggers(other);
			HandleEnvironmentEffectTriggers(other);
		}
	}

	void HandleEnvironmentEffectTriggers(Collider other) {
		if (health <= 0 || envDamageTimer < ENV_DAMAGE_DELAY) {
			return;
		}

		if (other.gameObject.tag.Equals("Fire")) {
			FireScript f = other.gameObject.GetComponent<FireScript>();
			int damageReceived = (int)(f.damage);
			TakeDamage(damageReceived);
			ResetEnvDamageTimer();
		}
	}

	void HandleExplosiveEffectTriggers(Collider other) {
		// First priority is to handle possible explosion damage
		if (health <= 0) {
			return;
		}
		if (other.gameObject.tag.Equals("Explosive")) {
            // If the grenade is still active or if the grenade has already affected the enemy, ignore it
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            // If a ray caszed from the enemy head to the grenade position is obscured, then the explosion is blocked
            if (t != null) {
				if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID)) {
					// Determine how far from the explosion the enemy was
					float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
					float blastRadius = other.gameObject.GetComponent<ThrowableScript>().blastRadius;
					distanceFromGrenade = Mathf.Min(distanceFromGrenade, blastRadius);
					float scale = 1f - (distanceFromGrenade / blastRadius);

					// Scale damage done to enemy by the distance from the explosion
					WeaponStats grenadeStats = other.gameObject.GetComponent<WeaponStats>();
					int damageReceived = (int)(grenadeStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived);
					// Validate that this enemy has already been affected
					t.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
						deathBy = 1;
						KilledByGrenade(t.fromPlayerId);
					}
				}
			} else {
				LauncherScript l = other.gameObject.GetComponent<LauncherScript>();
				if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !l.isLive && !l.PlayerHasBeenAffected(pView.ViewID)) {
					// Determine how far from the explosion the enemy was
					float distanceFromProjectile = Vector3.Distance(transform.position, other.gameObject.transform.position);
					float blastRadius = other.gameObject.GetComponent<LauncherScript>().blastRadius;
					distanceFromProjectile = Mathf.Min(distanceFromProjectile, blastRadius);
					float scale = 1f - (distanceFromProjectile / blastRadius);

					// Scale damage done to enemy by the distance from the explosion
					WeaponStats projectileStats = other.gameObject.GetComponent<WeaponStats>();
					int damageReceived = (int)(projectileStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived);
					// Validate that this enemy has already been affected
					l.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
						deathBy = 1;
						KilledByGrenade(l.fromPlayerId);
					}
				}
			}

			// // Make enemy alerted by the explosion if he's not dead
			// if (health > 0) {
			// 	SetAlertStatus(AlertStatus.Alert);
			// }
			// return;
		}

		if (other.gameObject.tag.Equals("Flashbang")) {
            ThrowableScript t = other.gameObject.GetComponent<ThrowableScript>();
            if (!EnvObstructionExists(headTransform.position, other.gameObject.transform.position) && !t.isLive && !t.PlayerHasBeenAffected(pView.ViewID)) {
				float totalDisorientationTime = ThrowableScript.MAX_FLASHBANG_TIME;

				// Determine how far from the explosion the enemy was
				float distanceFromGrenade = Vector3.Distance(transform.position, other.gameObject.transform.position);
				float blastRadius = t.blastRadius;

				// Determine rotation away from the flashbang - if more pointed away, less the duration
				Vector3 toPosition = Vector3.Normalize(other.gameObject.transform.position - transform.position);
				float angleToPosition = Vector3.Angle(transform.forward, toPosition);

				// Modify total disorientation time dependent on distance from grenade and rotation away from grenade
				float distanceMultiplier = Mathf.Clamp(1f - (distanceFromGrenade / blastRadius) + 0.6f, 0f, 1f);
				float rotationMultiplier = Mathf.Clamp(1f - (angleToPosition / 180f) + 0.1f, 0f, 1f);

				// Set enemy disorientation time
				totalDisorientationTime *= distanceMultiplier * rotationMultiplier;
				disorientationTime += totalDisorientationTime;
				disorientationTime = Mathf.Min(disorientationTime, ThrowableScript.MAX_FLASHBANG_TIME);
				if (disorientationTime > 0f) {
					UpdateActionState(ActionStates.Disoriented);
				}
                // Validate that this enemy has already been affected
                t.AddHitPlayer(pView.ViewID);
                // // Make enemy alerted by the disorientation if he's not dead
                // if (health > 0) {
				// 	SetAlertStatus(AlertStatus.Alert);
				// }
				return;
			}
		}
	}

	void ResetEnvDamageTimer() {
		envDamageTimer = 0f;
	}

	void UpdateEnvDamageTimer() {
		if (envDamageTimer < ENV_DAMAGE_DELAY) {
			envDamageTimer += Time.deltaTime;
		}
	}

	bool EnvObstructionExists(Vector3 a, Vector3 b) {
		// Ignore other enemy/player colliders
		// Layer mask (layers/objects to ignore in explosion that don't count as defensive)
		int ignoreLayers = (1 << 9) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15) | (1 << 17) | (1 << 18);
		ignoreLayers = ~ignoreLayers;
		RaycastHit hitInfo;
		bool t = Physics.Linecast(a, b, out hitInfo, ignoreLayers, QueryTriggerInteraction.Ignore);
		if (t) {
			t = (hitInfo.transform.tag == "Human") ? false : true;
			if (hitInfo.transform.gameObject.layer == 18) {
				if (hitInfo.transform.gameObject.GetComponent<BombScript>() == null) {
					t = false;
				}
			}
		}
		return t;
	}

	void KilledByGrenade(int killedByViewId) {
		pView.RPC("RpcRegisterGrenadeKill", RpcTarget.All, killedByViewId, gameController.teamMap);
	}

	[PunRPC]
	void RpcRegisterGrenadeKill(int playerNetworkId, string team) {
        if (team != gameController.teamMap) return;
        // TODO: For friendly fire stuff
	}

	void UpdateActionState(ActionStates action) {
		if (actionState != action) {
			pView.RPC("RpcUpdateActionState", RpcTarget.All, action, gameController.teamMap);
		}
	}

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action, string team) {
        if (team != gameController.teamMap) return;
        //{Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking}
        // if (action == ActionStates.Firing || action == ActionStates.Moving || action == ActionStates.Reloading || action == ActionStates.Pursue || action == ActionStates.TakingCover || action == ActionStates.InCover) {
		// 	int r = Random.Range (0, 3);
		// 	if (r == 1) {
		// 		StartCoroutine (PlayVoiceClipDelayed(Random.Range (1, 5), Random.Range(2f, 50f)));
		// 	} else if (r != 0) {
		// 		StartCoroutine (PlayVoiceClipDelayed(Random.Range (6, 13), Random.Range(2f, 50f)));
		// 	}
		// }
		// Play grunt when enemy dies or hit by flashbang
		if (action == ActionStates.Dead) {
			PlayGruntSound();
			headTransform.gameObject.layer = 15;
		}
		if (action == ActionStates.Disoriented) {
			PlayGruntSound();
		}
		actionState = action;
	}

	void DecideAction() {
		// Check for death first
		if (health <= 0 && actionState != ActionStates.Dead)
		{	
			UpdateActionState(ActionStates.Dead);

			// float respawnTime = Random.Range(0f, gameControllerScript.aIController.enemyRespawnSecs);
			// pView.RPC ("StartDespawn", RpcTarget.All, respawnTime, gameControllerScript.teamMap);
		}
	}

}
