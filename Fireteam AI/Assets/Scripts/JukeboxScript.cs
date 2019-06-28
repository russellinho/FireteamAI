using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JukeboxScript : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    private int audio1Index;
    private int audio2Index;
    private float audio1FadeTime;
    private float audio2FadeTime;
    public AudioClip[] trackList;
    private const float SONG_FADE_DELAY = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        audio1FadeTime = 0f;
        audio2FadeTime = 0f;
        int r = Random.Range(0, trackList.Length);
        audio1Index = r;
        audioSource1.clip = trackList[r];
        audioSource1.volume = 0f;
        audioSource1.Play();
        StartCoroutine(QueueNextSong(2));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (audioSource1.isPlaying) {
            if (audio1FadeTime < SONG_FADE_DELAY) {
                audio1FadeTime += Time.deltaTime;
                audioSource1.volume = audio1FadeTime / SONG_FADE_DELAY;
            }

            if (audioSource1.time >= (audioSource1.clip.length - SONG_FADE_DELAY)) {
                audio1FadeTime -= Time.deltaTime;
                audioSource1.volume = audio1FadeTime / SONG_FADE_DELAY;
            }

            if (audio1FadeTime <= 0f) {
                audioSource1.Stop();
                audio1FadeTime = 0f;
            }
        }
        if (audioSource2.isPlaying) {
            if (audio2FadeTime < SONG_FADE_DELAY) {
                audio2FadeTime += Time.deltaTime;
                audioSource2.volume = audio2FadeTime / SONG_FADE_DELAY;
            }

            if (audioSource2.time >= (audioSource2.clip.length - SONG_FADE_DELAY)) {
                audio2FadeTime -= Time.deltaTime;
                audioSource2.volume = audio2FadeTime / SONG_FADE_DELAY;
            }

            if (audio2FadeTime <= 0f) {
                audioSource2.Stop();
                audio2FadeTime = 0f;
            }
        }
    }

    IEnumerator QueueNextSong(int nextSource) {
        int r = Random.Range(0, trackList.Length);
        float waitTime = 0f;

        if (nextSource == 2) {
            // Ensure same song isn't played
            if (audio1Index == r) {
                if (r == (trackList.Length - 1)) {
                    r = 0;
                } else {
                    r++;
                }
            }

            audioSource2.clip = trackList[r];
            waitTime = audioSource1.clip.length;
        } else if (nextSource == 1) {
            // Ensure same song isn't played
            if (audio2Index == r) {
                if (r == (trackList.Length - 1)) {
                    r = 0;
                } else {
                    r++;
                }
            }

            audioSource1.clip = trackList[r];
            waitTime = audioSource2.clip.length;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, waitTime - SONG_FADE_DELAY));

        if (nextSource == 2) {
            audioSource2.Play();
            nextSource = 1;
        } else if (nextSource == 1) {
            audioSource1.Play();
            nextSource = 2;
        }

        StartCoroutine(QueueNextSong(nextSource));
    }

}
