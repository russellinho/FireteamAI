using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NpcScript : MonoBehaviour {

	public enum NpcType {Neutral, Friendly};
	public GameControllerScript gameController;
	public PhotonView pView;
	public int health;
	public int escortedByPlayerId;
	public bool isEscorting;
	public NpcType npcType;
	public AudioClip[] gruntSounds;
	public AudioSource audioSource;

	// Use this for initialization
	void Awake() {
		escortedByPlayerId = -1;
	}
		
	public void ToggleIsEscorting(bool b, Transform escorter, int escortedByPlayerId) {
		isEscorting = b;
		gameObject.transform.SetParent(escorter);
		this.escortedByPlayerId = escortedByPlayerId;
		// TODO: Change animation to escorting or relaxing animation depending on state here
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

}
