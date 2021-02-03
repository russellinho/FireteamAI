using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NpcScript : MonoBehaviourPunCallbacks {

	private const float ENV_DAMAGE_DELAY = 0.5f;
	private const float EXPLOSION_FORCE = 75f;
	private const float BULLET_FORCE = 50f;
	public enum NpcType {Neutral, Friendly};
	public GameControllerScript gameController;
	public PhotonView pView;
	public int health;
	// public bool godMode;
	public int carriedByPlayerId;
	public enum ActionStates {Idle, Wander, Firing, Moving, Dead, Reloading, Melee, Pursue, TakingCover, InCover, Seeking, Disoriented, Carried, Escorted, Injured, Incapacitated};
	// FSM used for determining movement while attacking and not in cover
	enum FiringStates {StandingStill, StrafeLeft, StrafeRight, Backpedal, Forward};
	public ActionStates actionState;
	private FiringStates firingState;
	// If true, the player can sprint, crouch, jump, walk while carrying
	public bool immobileWhileCarrying;
	// Amount of speed to reduce while carrying
	public float weightSpeedReduction;
	public NpcType npcType;
	public AudioClip[] gruntSounds;
	public AudioSource audioSource;
	private Transform carriedByTransform;
	public SkinnedMeshRenderer[] rends;
	public Animator animator;
	public Collider mainCol;
	public Rigidbody mainRigid;
	public Transform headTransform;
	public Transform torsoTransform;
	public Transform leftArmTransform;
	public Transform leftForeArmTransform;
	public Transform rightArmTransform;
	public Transform rightForeArmTransform;
	public Transform pelvisTransform;
	public Transform leftUpperLegTransform;
	public Transform leftLowerLegTransform;
	public Transform rightUpperLegTransform;
	public Transform rightLowerLegTransform;
	public Rigidbody[] ragdollBodies;
	// Timers
	private float envDamageTimer;
	private float disorientationTime;
	private Vector3 lastHitFromPos;
	private int lastHitBy;
	private int lastBodyPartHit;

	// Use this for initialization
	void Awake() {
		ToggleRagdoll(false);
		carriedByPlayerId = -1;
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
		if (!PhotonNetwork.IsMasterClient && !gameController.isVersusHostForThisTeam()) {
			pView.RPC("RpcAskServerForDataNpc", RpcTarget.All);
		}
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
			SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
		}
    }

	public void OnPlayerLeftGame(int actorNo) {
		pView.RPC("RpcOnPlayerLeftGame", RpcTarget.All, actorNo);
	}

	[PunRPC]
	void RpcOnPlayerLeftGame(int actorNo) {
		if (actorNo == carriedByPlayerId) {
			ToggleIsCarrying(false, -1);
			SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
		}
	}
		
	public void ToggleIsCarrying(bool b, int carriedByPlayerId) {
		this.carriedByPlayerId = carriedByPlayerId;
		if (carriedByPlayerId == -1) {
			actionState = ActionStates.Incapacitated;
			carriedByTransform = null;
			// ToggleRenderers(true);
			// ToggleRagdoll(true);
			gameObject.transform.SetParent(null);
			transform.rotation = Quaternion.identity;
			mainRigid.isKinematic = false;
			mainRigid.useGravity = true;
		} else {
			actionState = ActionStates.Carried;
			carriedByTransform = GameControllerScript.playerList[carriedByPlayerId].objRef.GetComponent<PlayerActionScript>().carryingSlot;
			// ToggleRagdoll(false);
			mainRigid.useGravity = false;
			mainRigid.isKinematic = true;
			gameObject.transform.SetParent(carriedByTransform);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			// if (carriedByPlayerId == PhotonNetwork.LocalPlayer.ActorNumber) {
			// 	ToggleRenderers(false);
			// }
		}
	}

	public void TakeDamage(int d, Vector3 hitFromPos, int hitBy, int bodyPartHit) {
		// if (godMode) return;
		pView.RPC ("RpcTakeDamage", RpcTarget.All, d, hitFromPos.x, hitFromPos.y, hitFromPos.z, hitBy, bodyPartHit, gameController.teamMap);
	}

	[PunRPC]
	public void RpcTakeDamage(int d, float hitFromX, float hitFromY, float hitFromZ, int hitBy, int bodyPartHit, string team) {
        if (team != gameController.teamMap) return;
        health -= d;
		lastHitFromPos = new Vector3(hitFromX, hitFromY, hitFromZ);
		lastHitBy = hitBy;
		lastBodyPartHit = bodyPartHit;
	}

	public void PlayGruntSound(string team) {
		pView.RPC("RpcPlayGruntSound", RpcTarget.All, team);
	}

	[PunRPC]
	public void RpcPlayGruntSound(string team) {
		if (team != gameController.teamMap) return;
		if (gruntSounds.Length == 0) return;
		int r = Random.Range(0, gruntSounds.Length);
		audioSource.clip = gruntSounds [r];
		audioSource.Play ();
	}

	void ToggleRagdoll(bool b) {
		animator.enabled = !b;
		mainCol.enabled = !b;

		foreach (Rigidbody rb in ragdollBodies)
		{
			rb.isKinematic = !b;
			rb.useGravity = b;
		}

		// headTransform.GetComponent<Collider>().enabled = b;
		// torsoTransform.GetComponent<Collider>().enabled = b;
		// leftArmTransform.GetComponent<Collider>().enabled = b;
		// leftForeArmTransform.GetComponent<Collider>().enabled = b;
		// rightArmTransform.GetComponent<Collider>().enabled = b;
		// rightForeArmTransform.GetComponent<Collider>().enabled = b;
		// pelvisTransform.GetComponent<Collider>().enabled = b;
		// leftUpperLegTransform.GetComponent<Collider>().enabled = b;
		// leftLowerLegTransform.GetComponent<Collider>().enabled = b;
		// rightUpperLegTransform.GetComponent<Collider>().enabled = b;
		// rightLowerLegTransform.GetComponent<Collider>().enabled = b;
	}

	void ToggleHumanCollision(bool b)
	{
		if (!b) {
			headTransform.gameObject.layer = 12;
			torsoTransform.gameObject.layer = 12;
			leftArmTransform.gameObject.layer = 12;
			leftForeArmTransform.gameObject.layer = 12;
			rightArmTransform.gameObject.layer = 12;
			rightForeArmTransform.gameObject.layer = 12;
			pelvisTransform.gameObject.layer = 12;
			leftUpperLegTransform.gameObject.layer = 12;
			leftLowerLegTransform.gameObject.layer = 12;
			rightUpperLegTransform.gameObject.layer = 12;
			rightLowerLegTransform.gameObject.layer = 12;
		} else {
			headTransform.gameObject.layer = 15;
			torsoTransform.gameObject.layer = 15;
			leftArmTransform.gameObject.layer = 15;
			leftForeArmTransform.gameObject.layer = 15;
			rightArmTransform.gameObject.layer = 15;
			rightForeArmTransform.gameObject.layer = 15;
			pelvisTransform.gameObject.layer = 15;
			leftUpperLegTransform.gameObject.layer = 15;
			leftLowerLegTransform.gameObject.layer = 15;
			rightUpperLegTransform.gameObject.layer = 15;
			rightLowerLegTransform.gameObject.layer = 15;
		}
	}

	void ApplyForceModifiers()
	{
		// 0 = death from bullet, 1 = death from explosion, 2 = death from fire/etc.
		if (lastHitBy == 0) {
			Rigidbody rb = ragdollBodies[lastBodyPartHit - 1];
			if (lastBodyPartHit == WeaponActionScript.HEAD_TARGET) {
				Vector3 forceDir = Vector3.Normalize(headTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.TORSO_TARGET) {
				Vector3 forceDir = Vector3.Normalize(torsoTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftForeArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_ARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_FOREARM_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightForeArmTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.PELVIS_TARGET) {
				Vector3 forceDir = Vector3.Normalize(pelvisTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftUpperLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.LEFT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(leftLowerLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_UPPER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightUpperLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			} else if (lastBodyPartHit == WeaponActionScript.RIGHT_LOWER_LEG_TARGET) {
				Vector3 forceDir = Vector3.Normalize(rightLowerLegTransform.position - lastHitFromPos) * BULLET_FORCE;
				rb.AddForce(forceDir, ForceMode.Impulse);
			}
		} else if (lastHitBy == 1) {
			foreach (Rigidbody rb in ragdollBodies) {
				rb.AddExplosionForce(EXPLOSION_FORCE, lastHitFromPos, 7f, 0f, ForceMode.Impulse);
			}
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

		if (actionState == ActionStates.Incapacitated) {
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
		}
	}

	void OnTriggerEnterForVersus(Collider other) {
		/** Explosive trigger functionality below - only operate on master client/server to avoid duplicate effects */
		if (gameController.isVersusHostForThisTeam()) {
			HandleExplosiveEffectTriggers(other);
		}
	}

	void OnTriggerStay(Collider other) {
		if (gameController.matchType == 'V') {
			OnTriggerStayForVersus(other);
		} else {
			OnTriggerStayForCampaign(other);
		}
	}

	void OnTriggerStayForCampaign(Collider other) {
		if (PhotonNetwork.IsMasterClient) {
			HandleEnvironmentEffectTriggers(other);
		}
	}

	void OnTriggerStayForVersus(Collider other) {
		if (gameController.isVersusHostForThisTeam()) {
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
			TakeDamage(damageReceived, other.gameObject.transform.position, 2, 0);
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
					Weapon grenadeStats = InventoryScript.itemData.weaponCatalog[t.rootWeapon];
					int damageReceived = (int)(grenadeStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived, other.gameObject.transform.position, 1, 0);
					// Validate that this enemy has already been affected
					t.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
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
					Weapon projectileStats = InventoryScript.itemData.weaponCatalog[l.rootWeapon];
					int damageReceived = (int)(projectileStats.damage * scale);
					// Deal damage to the enemy
					TakeDamage(damageReceived, other.gameObject.transform.position, 1, 0);
					// Validate that this enemy has already been affected
					l.AddHitPlayer(pView.ViewID);
					if (health <= 0) {
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
			PlayGruntSound(gameController.teamMap);
		}
		if (action == ActionStates.Disoriented) {
			PlayGruntSound(gameController.teamMap);
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
			StartCoroutine(DelayToggleRagdoll(0.2f, true));
			// ToggleHumanCollision(false);
		}
	}

	[PunRPC]
	void RpcAskServerForDataNpc() {
		if (PhotonNetwork.IsMasterClient || gameController.isVersusHostForThisTeam()) {
			pView.RPC("RpcSyncDataNpc", RpcTarget.Others, health, carriedByPlayerId, actionState, firingState, envDamageTimer, disorientationTime, 
					transform.position.x, transform.position.y, transform.position.z, gameController.teamMap);
		}
	}

	[PunRPC]
	void RpcSyncDataNpc(int health, int carriedByPlayerId, ActionStates acState, FiringStates firingState, float envDamageTimer, float disorientationTimer, 
					float posX, float posY, float posZ, string team) {
		if (team != gameController.teamMap) return;
		this.health = health;
		this.carriedByPlayerId = carriedByPlayerId;
		this.actionState = acState;
		if (this.actionState == ActionStates.Dead) {
			ToggleHumanCollision(false);
		} else {
			ToggleHumanCollision(true);
		}
		this.firingState = firingState;
		this.envDamageTimer = envDamageTimer;
		this.disorientationTime = disorientationTimer;
		transform.position = new Vector3(posX, posY, posZ);
		if (carriedByPlayerId == -1) {
			ToggleIsCarrying(false, -1);
		} else {
			ToggleIsCarrying(true, carriedByPlayerId);
		}
	}

	IEnumerator DelayToggleRagdoll(float seconds, bool b)
    {
        yield return new WaitForSeconds(seconds);
        pView.RPC("RpcToggleRagdollNpc", RpcTarget.MasterClient, b);
    }

    [PunRPC]
    void RpcToggleRagdollNpc(bool b)
    {
        ToggleRagdoll(b);
		ToggleHumanCollision(!b);
        if (b) {
            ApplyForceModifiers();
        }
    }

}
