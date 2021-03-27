using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class AudioControllerScript : MonoBehaviour {

	private const int MUFFLE_START_VALUE = 900;
	private const int MUFFLE_END_VALUE = 15000;
	private const int ECHO_START_VALUE = 836;
	private const int ECHO_END_VALUE = -10000;
	private const int MUFFLE_DIFFERENCE = MUFFLE_START_VALUE - MUFFLE_END_VALUE;
	private const int ECHO_DIFFERENCE = ECHO_START_VALUE - ECHO_END_VALUE;

	private GameControllerScript gameControllerRef;
	public PlayerHUDScript playerHUDScript;

	// Audio stuff
	public AudioLowPassFilter audioMuffle;
	public AudioReverbFilter audioEcho;
	public GameObject fxRef;
	private AudioSource fxSound1;
	private AudioSource fxSound2;
	private AudioSource fxSound3;
	private AudioSource fxSound4;
	private AudioSource fxSound5;
	private AudioSource fxSound6;
	private AudioSource fxSound7;
	private AudioSource fxSound8;
	private AudioSource fxSound9;
	private float flashbangRingTimer;
	private float flashbangRingTotalTime;
	private float flashbangRingThird;

	// Clips
	public AudioClip headshotSound;
	public AudioClip killSound;
	public AudioClip playerHitSound;
	public AudioClip overshieldHitSound;
	public AudioClip overshieldPopSound;
	public AudioClip overshieldWarningSound;
	public AudioClip overshieldRecoverSound;
	public AudioClip playerGruntSound1;
	public AudioClip playerGruntSound2;
	public AudioClip hitmarkerSound;
	public AudioClip sirenSound;
	public AudioClip missionStartSound;
	public AudioClip cautionSound;
	public AudioClip detectedSound;
	public AudioClip enterWater;
	public AudioClip swimSound1;
	public AudioClip swimSound2;
	public AudioClip waterFootstep1;
	public AudioClip waterFootstep2;
	public AudioClip waterFootstep3;

	private bool wasRunning;
	private PhotonView pView;

	private bool initialized;

	// Use this for initialization
	public void Initialize () {
		wasRunning = false;
		fxSound2 = fxRef.GetComponents<AudioSource>() [1];
		pView = GetComponent<PhotonView> ();
		if (pView.IsMine) {
			AudioSource[] fxRefs = fxRef.GetComponents<AudioSource>();
			fxSound1 = fxRefs [0];
			fxSound3 = fxRefs [2];
			fxSound4 = fxRefs [3];
			fxSound5 = fxRefs [4];
			fxSound6 = fxRefs [5];
			fxSound7 = fxRefs [6];
			fxSound8 = fxRefs [7];
			fxSound9 = fxRefs [8];
			PlayMissionStartSound ();
		}
		initialized = true;
	}

	// Update is called once per frame
	void Update () {
		if (!initialized) {
			return;
		}
		// Ensure we can access game controller before we begin
		if (!pView.IsMine) {
			return;
		}
		if (!gameControllerRef) {
			gameControllerRef = GetComponent<PlayerActionScript> ().gameController;
			return;
		}

		// Flashbang ring - muffles all other sounds and fades out towards the end while fading in the other sounds
		if (flashbangRingTimer > 0f) {
			// If the flashbang just hit, play the sound, muffle all the other sounds, echo all the other sounds
			if (flashbangRingTimer == flashbangRingTotalTime) {
				flashbangRingThird = flashbangRingTotalTime / 3f;
				if (!fxSound7.isPlaying) {
					audioEcho.reverbPreset = AudioReverbPreset.User;
					audioMuffle.cutoffFrequency = (float)MUFFLE_START_VALUE;
					audioEcho.reverbLevel = (float)ECHO_START_VALUE;
					audioMuffle.enabled = true;
					audioEcho.enabled = true;
					fxSound7.volume = 1f;
					fxSound7.Play();
				}
			}

			// If the flashbang effect is in its last third of total time, start restoring audio back to normal
			if (flashbangRingTimer <= flashbangRingThird) {
				float fractionRemaining = flashbangRingTimer / flashbangRingThird;
				
				// Ringing volume reduction
				fxSound7.volume = fractionRemaining;
				// Audio muffle reduction
				audioMuffle.cutoffFrequency = (float)MUFFLE_START_VALUE + ((1f - fractionRemaining) * MUFFLE_DIFFERENCE);
				// Audio echo reduction
				audioEcho.reverbLevel = (float)ECHO_START_VALUE - ((1f - fractionRemaining) * ECHO_DIFFERENCE);
			}

			flashbangRingTimer -= Time.deltaTime;
		} else {
			fxSound7.Stop();
			if (audioMuffle.enabled) {
				audioMuffle.enabled = false;
				audioEcho.enabled = false;
			}
		}

		// Control BGM
		if (gameControllerRef.assaultMode) {
			JukeboxScript.jukebox.PlayAssaultMusic();
		} else {
			JukeboxScript.jukebox.PlayStealthMusic();
		}


		if (!fxSound2.isPlaying && wasRunning) {
			if (!fxSound6.isPlaying) {
				fxSound6.Play ();
			}
		}

		if (fxSound8.isPlaying) {
			audioEcho.enabled = true;
		} else {
			audioEcho.enabled = false;
		}
	}

	public void PlayHeadshotSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = headshotSound;
		fxSound1.Play ();
	}

	public void PlayKillSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = killSound;
		fxSound1.Play();
	}

	public void PlayCautionSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = cautionSound;
		fxSound1.Play();
	}

	public void PlayDetectedSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = detectedSound;
		fxSound1.Play();
	}

	void PlayMissionStartSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = missionStartSound;
		fxSound1.Play ();
	}

	public void PlayGruntSound() {
		if (fxSound6 != null && fxSound6.isPlaying) {
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
		if (!pView.IsMine) {
			return;
		}
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

	public void PlayOvershieldWarningSound(bool b)
	{
		if (!pView.IsMine) {
			return;
		}
		fxSound9.clip = overshieldWarningSound;
		if (b) {
			if (!fxSound9.isPlaying) {
				fxSound9.Play ();
			}
		} else {
			fxSound9.Stop();
		}
	}

	public void PlayOvershieldRecoverSound(bool b)
	{
		if (!pView.IsMine) {
			return;
		}
		fxSound9.clip = overshieldRecoverSound;
		if (b) {
			if (!fxSound9.isPlaying) {
				fxSound9.Play ();
			}
		} else {
			fxSound9.Stop();
		}
	}

	public void PlayHitmarkerSound() {
		if (!pView.IsMine) {
			return;
		}
		fxSound3.clip = hitmarkerSound;
		fxSound3.Play ();
	}

	public void PlayHitSound(bool overshield) {
		if (!pView.IsMine) {
			return;
		}
		if (overshield) {
			fxSound4.clip = overshieldHitSound;
		} else {
			fxSound4.clip = playerHitSound;
		}
		fxSound4.Play ();
	}

	public void PlayOvershieldPopSound()
	{
		if (!pView.IsMine) {
			return;
		}
		fxSound4.clip = overshieldPopSound;
		fxSound4.Play ();
	}

	public void PlayFlashbangEarRingSound(float flashbangTime) {
		if (!pView.IsMine) {
			return;
		}
		
		flashbangRingTotalTime = flashbangTime;
		flashbangRingTimer = flashbangTime;
	}

	public void PlayerWaterEnterSound()
	{
		if (!pView.IsMine) {
			return;
		}
		fxSound1.clip = enterWater;
		fxSound1.Play ();
	}

	public void ToggleWaterAmbience(bool b)
	{
		if (!pView.IsMine) {
			return;
		}
		if (b) {
			fxSound8.Play();
			audioEcho.reverbPreset = AudioReverbPreset.Underwater;
			audioEcho.enabled = true;
		} else {
			fxSound8.Stop();
			audioEcho.enabled = false;
		}
	}

}
