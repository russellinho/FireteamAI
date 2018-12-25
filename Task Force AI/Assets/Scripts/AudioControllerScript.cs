﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class AudioControllerScript : MonoBehaviour {

	private GameControllerScript gameControllerRef;

	// Audio stuff
	public AudioSource bgm;
	public GameObject fxRef;
	private AudioSource fxSound1;
	private AudioSource fxSound2;
	private AudioSource fxSound3;
	private AudioSource fxSound4;
	private AudioSource fxSound5;
	private AudioSource fxSound6;

	// Clips
	public AudioClip stealthMusic;
	public AudioClip assaultMusic;
	public AudioClip headshotSound;
	public AudioClip playerHitSound;
	public AudioClip playerGruntSound1;
	public AudioClip playerGruntSound2;
	public AudioClip hitmarkerSound;
	public AudioClip sirenSound;
	public AudioClip missionStartSound;

	private bool wasRunning;

	// Use this for initialization
	void Start () {
		if (!GetComponent<PhotonView> ().IsMine) {
			this.enabled = false;
			return;
		}
		wasRunning = false;
		fxSound1 = fxRef.GetComponents<AudioSource>() [0];
		fxSound2 = fxRef.GetComponents<AudioSource>() [1];
		fxSound3 = fxRef.GetComponents<AudioSource>() [2];
		fxSound4 = fxRef.GetComponents<AudioSource>() [3];
		fxSound5 = fxRef.GetComponents<AudioSource> () [4];
		fxSound6 = fxRef.GetComponents<AudioSource> () [5];
		PlayMissionStartSound ();

	}

	// Update is called once per frame
	void Update () {
		// Ensure we can access game controller before we begin
		if (!GetComponent<PhotonView> ().IsMine) {
			return;
		}
		if (!gameControllerRef) {
			gameControllerRef = GetComponent<PlayerScript> ().gameController.GetComponent<GameControllerScript>();
			return;
		}

		if (gameControllerRef.currentMap == 1)
		{
			// Control BGM
			if (gameControllerRef.assaultMode) {
				if (!bgm.isPlaying || !bgm.clip.name.Equals(assaultMusic.name)) {
					bgm.clip = assaultMusic;
					bgm.Play ();
					fxSound5.clip = sirenSound;
					fxSound5.loop = true;
					fxSound5.Play ();
					StartCoroutine (RestartBgmTimer(assaultMusic.length - bgm.time));
				}
			} else {
				if (!bgm.isPlaying || !bgm.clip.name.Equals (stealthMusic.name)) {
					bgm.clip = stealthMusic;
					bgm.Play ();
				}
			}

		}

		if (!fxSound2.isPlaying && wasRunning) {
			if (!fxSound6.isPlaying) {
				fxSound6.Play ();
			}
		}
	}

	IEnumerator RestartBgmTimer(float secs) {
		yield return new WaitForSeconds (secs);
		bgm.Stop ();
		bgm.time = 1.15f;
		bgm.Play ();
		StartCoroutine (RestartBgmTimer(assaultMusic.length - bgm.time));
	}

	public void PlayHeadshotSound() {
		fxSound1.clip = headshotSound;
		fxSound1.Play ();
	}

	void PlayMissionStartSound() {
		fxSound1.clip = missionStartSound;
		fxSound1.Play ();
	}

	public void PlayGruntSound() {
		if (fxSound6.isPlaying) {
			fxSound6.Stop ();
		}

		int r = Random.Range (1, 3);
		if (r == 1) {
			fxSound2.clip = playerGruntSound1;
		} else {
			fxSound2.clip = playerGruntSound2;
		}
		fxSound2.Play ();
	}

	public void PlaySprintSound(bool play) {
		if (play) {
			wasRunning = true;
			if (!fxSound6.isPlaying) {
				fxSound6.loop = true;
				fxSound6.Play ();
			}
		} else {
			wasRunning = false;
			fxSound6.loop = false;
		}
	}

	public void PlayHitmarkerSound() {
		fxSound3.clip = hitmarkerSound;
		fxSound3.Play ();
	}

	public void PlayHitSound() {
		fxSound4.clip = playerHitSound;
		fxSound4.Play ();
	}

}
