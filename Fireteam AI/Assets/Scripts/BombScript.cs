using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour {

	public bool defused;
	public int bombId;

	// Use this for initialization
	void Start () {
		defused = false;
	}
		
	public void Defuse() {
		defused = true;
	}

}
