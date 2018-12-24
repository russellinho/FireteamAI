﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour {

	public bool defused;

	// Use this for initialization
	void Start () {
		defused = false;
	}
		
	public void Defuse() {
		defused = true;
		GetComponent<MeshRenderer> ().material.color = Color.white;
		GetComponentInChildren<SpriteRenderer> ().enabled = false;
	}

}
