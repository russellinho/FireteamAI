using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BombScript : MonoBehaviour {

	public bool defused;
	private PhotonView pView;

	// Use this for initialization
	void Start () {
		defused = false;
	}
		
	public void Defuse() {
		pView.RPC ("RpcDefuse", RpcTarget.All);
	}

	[PunRPC]
	void RpcDefuse() {
		defused = true;
		GetComponent<MeshRenderer> ().material.color = Color.white;
		GetComponentInChildren<SpriteRenderer> ().enabled = false;
	}

}
