using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceModifier : MonoBehaviour
{
    private AudioUnit[] audioUnits;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource[] a = GetComponentsInChildren<AudioSource>(true);
        if (a != null && a.Length > 0) {
            audioUnits = new AudioUnit[a.Length];
            for (int i = 0; i < a.Length; i++) {
                AudioSource ad = a[i];
                audioUnits[i] = new AudioUnit(ad, ad.volume);
                // If is 2D sound, set game sound volune. Else if 3D, set ambient sound volume
                if (ad.spatialBlend < 0.5f) {
                    ad.volume *= (float)PlayerPreferences.playerPreferences.preferenceData.gameVolume / 100f;
                } else {
                    ad.volume *= (float)PlayerPreferences.playerPreferences.preferenceData.ambientVolume / 100f;
                }
            }
        }
    }

    public void SetVolume() {
        if (audioUnits != null && audioUnits.Length > 0) {
            foreach (AudioUnit a in audioUnits) {
                if (a.aud.spatialBlend < 0.5f) {
                    a.aud.volume = a.originalVolume * (float)PlayerPreferences.playerPreferences.preferenceData.gameVolume / 100f;
                } else {
                    a.aud.volume = a.originalVolume * (float)PlayerPreferences.playerPreferences.preferenceData.ambientVolume / 100f;
                }
            }
        }
    }

    private class AudioUnit {
        public AudioSource aud;
        public float originalVolume;

        public AudioUnit(AudioSource aud, float originalVolume) {
            this.aud = aud;
            this.originalVolume = originalVolume;
        }
    }
}
