using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageInfo : MonoBehaviour {

	public float timeRemaining;
	private short lengthOfString;

	// Use this for initialization
	void Start () {
		timeRemaining = 0f;
		lengthOfString = 0;
	}

	void Update() {
		timeRemaining -= Time.deltaTime;
	}
	
	public void SetLength(short s) {
		timeRemaining = 7f;
		lengthOfString = s;
	}

	public short GetLength() {
		return lengthOfString;
	}
}
