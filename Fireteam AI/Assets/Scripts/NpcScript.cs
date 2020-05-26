using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcScript : MonoBehaviour {

	public int escortedByPlayerId;
	public bool isEscorting;

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

}
