using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NpcScript : MonoBehaviourPunCallbacks {

	public enum NpcType {Neutral, Friendly};
	public GameControllerScript gameController;
	public PhotonView pView;
	public int health;
	public int carriedByPlayerId;
	public bool isCarrying;
	// If true, the player can sprint, crouch, jump, walk while carrying
	public bool immobileWhileCarrying;
	// Amount of speed to reduce while carrying
	public float weightSpeedReduction;
	public NpcType npcType;
	public AudioClip[] gruntSounds;
	public AudioSource audioSource;
	private Transform carriedByTransform;
	public SkinnedMeshRenderer[] rends;
	public CapsuleCollider col;

	// Use this for initialization
	void Awake() {
		carriedByPlayerId = -1;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
    {
		if (otherPlayer.ActorNumber == carriedByPlayerId) {
			ToggleIsCarrying(false, -1);
		}
    }
		
	public void ToggleIsCarrying(bool b, int carriedByPlayerId) {
		isCarrying = b;
		this.carriedByPlayerId = carriedByPlayerId;
		if (carriedByPlayerId == -1) {
			carriedByTransform = null;
			ToggleRenderers(true);
			ToggleCollider(true);
		} else {
			carriedByTransform = GameControllerScript.playerList[carriedByPlayerId].carryingSlotRef;
			ToggleCollider(false);
			if (carriedByPlayerId == PhotonNetwork.LocalPlayer.ActorNumber) {
				ToggleRenderers(false);
			}
		}
		// TODO: Change animation to carrying or relaxing animation depending on state here
	}
	
	void LateUpdate() {
		if (isCarrying) {
			if (carriedByTransform != null) {
				transform.position = carriedByTransform.position;
			}
		}
	}

	public void TakeDamage(int d) {
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
	}

	void ToggleRenderers(bool b) {
		foreach (SkinnedMeshRenderer m in rends) {
			m.enabled = b;
		}
	}

}
