using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JukeboxScript : MonoBehaviour
{
    private const float TRACK_PREVIEW_TIME = 7f;
    private enum MusicMode {Title, InGame, GameOver};
    private MusicMode currentMode;
    public static JukeboxScript jukebox;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;
    private int audio1Index;
    private int audio2Index;
    private float audio1FadeTime;
    private float audio2FadeTime;
    private bool assaultMode;
    public AudioClip[] titleTrackList;
    public AudioClip[] stealthTracks;
    public AudioClip[] assaultTracks;
    private const float SONG_FADE_DELAY = 3f;
    public const int DEFAULT_MUSIC_VOLUME = 70;
    private bool audioSource1FullyQueued;
    private bool audioSource2FullyQueued;
    private float previewSongTimer;

    void Awake() {
        if (jukebox == null)
        {
            DontDestroyOnLoad(gameObject);
            jukebox = this;
            StartTitleMusic();
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }
        else if (jukebox != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode) {
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName.Equals("GameOverFail") || levelName.Equals("GameOverSuccess")) {
            currentMode = MusicMode.GameOver;
            StopMusic();
        } else if (levelName.Equals("Login") || levelName.Equals("Title") || levelName.Equals("Setup")) {
            if (currentMode != MusicMode.Title) {
                StopMusic();
                StartTitleMusic();
            }
        } else {
            if (currentMode != MusicMode.InGame) {
                StopMusic();
                if (levelName.EndsWith("_Red")) {
                    levelName = levelName.Substring(0, levelName.Length - 4);
                } else if (levelName.EndsWith("_Blue")) {
                    levelName = levelName.Substring(0, levelName.Length - 5);
                }
                StartInGameMusic(levelName);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentMode == MusicMode.Title) {
            HandleUpdateForTitle();
        } else if (currentMode == MusicMode.InGame) {
            HandleUpdateInGame();
        }
    }

    void HandleUpdateForTitle() {
        if (previewSongTimer > 0f) {
            previewSongTimer -= Time.deltaTime;
            if (previewSongTimer <= 0f) {
                audioSource3.Stop();
                JukeboxScript.jukebox.SetMusicVolume((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
            } else {
                SetMusicVolume(0f);
            }
        }

        if (audioSource1.isPlaying) {
            if (audioSource1.time >= audioSource1.clip.length) {
                audioSource1.Stop();
            }
        }
        if (audioSource2.isPlaying) {
            if (audioSource2.time >= audioSource2.clip.length) {
                audioSource2.Stop();
            }
        }
    }

    void HandleUpdateInGame() {
        if (audioSource1.isPlaying) {
            if (assaultMode) {
                audioSource1.Stop();
            }
        }
        if (audioSource2.isPlaying) {
            if (!assaultMode) {
                audioSource2.Stop();
            }
        }
    }

    // void HandleUpdateForTitle() {
    //     if (audioSource1.isPlaying) {
    //         if (!audioSource1FullyQueued && audio1FadeTime < SONG_FADE_DELAY) {
    //             audio1FadeTime += Time.deltaTime;
    //             if (audio1FadeTime >= SONG_FADE_DELAY) {
    //                 audioSource1FullyQueued = true;
    //             }
    //             audioSource1.volume = (audio1FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         }

    //         if (audioSource1.time >= (audioSource1.clip.length - SONG_FADE_DELAY)) {
    //             audio1FadeTime -= Time.deltaTime;
    //             audioSource1.volume = (audio1FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         }

    //         if (audio1FadeTime <= 0f) {
    //             audioSource1.Stop();
    //             audio1FadeTime = 0f;
    //             audioSource1FullyQueued = false;
    //         }
    //     }
    //     if (audioSource2.isPlaying) {
    //         if (!audioSource2FullyQueued && audio2FadeTime < SONG_FADE_DELAY) {
    //             audio2FadeTime += Time.deltaTime;
    //             if (audio2FadeTime >= SONG_FADE_DELAY) {
    //                 audioSource2FullyQueued = true;
    //             }
    //             audioSource2.volume = (audio2FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         }

    //         if (audioSource2.time >= (audioSource2.clip.length - SONG_FADE_DELAY)) {
    //             audio2FadeTime -= Time.deltaTime;
    //             audioSource2.volume = (audio2FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         }

    //         if (audio2FadeTime <= 0f) {
    //             audioSource2.Stop();
    //             audio2FadeTime = 0f;
    //             audioSource2FullyQueued = false;
    //         }
    //     }
    // }

    // void HandleUpdateInGame() {
    //     if (audioSource1.isPlaying) {
    //         if (assaultMode) {
    //             audio1FadeTime -= Time.deltaTime;
    //             audioSource1.volume = (audio1FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         } else {
    //             if (audio1FadeTime < SONG_FADE_DELAY) {
    //                 audio1FadeTime += Time.deltaTime;
    //                 audioSource1.volume = (audio1FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //             }
    //         }

    //         if (audio1FadeTime <= 0f) {
    //             audioSource1.Stop();
    //             audio1FadeTime = 0f;
    //         }
    //     }
    //     if (audioSource2.isPlaying) {
    //         if (!assaultMode) {
    //             audio2FadeTime -= Time.deltaTime;
    //             audioSource2.volume = (audio2FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //         } else {
    //             if (audio2FadeTime < SONG_FADE_DELAY) {
    //                 audio2FadeTime += Time.deltaTime;
    //                 audioSource2.volume = (audio2FadeTime / SONG_FADE_DELAY) * ((float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f);
    //             }
    //         }

    //         if (audio2FadeTime <= 0f) {
    //             audioSource2.Stop();
    //             audio2FadeTime = 0f;
    //         }
    //     }
    // }

    IEnumerator QueueNextSongOnTitle(int nextSource) {
        int r = Random.Range(0, titleTrackList.Length);
        float waitTime = 0f;

        if (nextSource == 2) {
            // Ensure same song isn't played
            if (audio1Index == r) {
                if (r == (titleTrackList.Length - 1)) {
                    r = 0;
                } else {
                    r++;
                }
            }

            audio2Index = r;
            audioSource2.clip = titleTrackList[r];
            waitTime = audioSource1.clip.length;
        } else if (nextSource == 1) {
            // Ensure same song isn't played
            if (audio2Index == r) {
                if (r == (titleTrackList.Length - 1)) {
                    r = 0;
                } else {
                    r++;
                }
            }

            audio1Index = r;
            audioSource1.clip = titleTrackList[r];
            waitTime = audioSource2.clip.length;
        }

        // yield return new WaitForSeconds(Mathf.Max(0f, waitTime - SONG_FADE_DELAY));
        yield return new WaitForSeconds(Mathf.Max(0f, waitTime));

        if (nextSource == 2) {
            audioSource2.Play();
            nextSource = 1;
        } else if (nextSource == 1) {
            audioSource1.Play();
            nextSource = 2;
        }

        StartCoroutine(QueueNextSongOnTitle(nextSource));
    }

    public void StopMusic() {
        audioSource1.Stop();
        audioSource2.Stop();
        StopAllCoroutines();
    }

    public void StartTitleMusic() {
        if (!audioSource1.isPlaying && !audioSource2.isPlaying) {
            currentMode = MusicMode.Title;
            audioSource1.loop = false;
            audioSource2.loop = false;
            audio1FadeTime = 0f;
            audio2FadeTime = 0f;
            int r = Random.Range(0, titleTrackList.Length);
            audio1Index = r;
            audioSource1.clip = titleTrackList[r];
            // audioSource1.volume = 0f;
            audioSource1.Play();
            StartCoroutine(QueueNextSongOnTitle(2));
        }
    }

    void StartInGameMusic(string sceneName) {
        StopAllCoroutines();
        currentMode = MusicMode.InGame;
        audioSource1.loop = true;
        audioSource2.loop = true;
        audio1FadeTime = 0f;
        audio2FadeTime = 0f;
        LoadMusicForScene(sceneName);
        // audioSource1.volume = 0f;
    }

    public void PlayStealthMusic() {
        assaultMode = false;
        if (!audioSource1.isPlaying) {
            audioSource1.Play();
        }
    }

    public void PlayAssaultMusic() {
        assaultMode = true;
        if (!audioSource2.isPlaying) {
            audioSource2.Play();
        }
    }

    void LoadMusicForScene(string sceneName) {
        audioSource1.clip = GetCurrentStealthTrack();
        audioSource2.clip = GetCurrentAssaultTrack();
    }

    public void SetMusicVolume(float v) {
        audioSource1.volume = v;
        audioSource2.volume = v;
    }

    public AudioClip GetCurrentStealthTrack()
    {
        return stealthTracks[PlayerPreferences.playerPreferences.preferenceData.stealthTrack];
    }

    public AudioClip GetCurrentAssaultTrack()
    {
        return assaultTracks[PlayerPreferences.playerPreferences.preferenceData.assaultTrack];
    }

    public void PreviewTrack(char mode, int i)
    {
        audioSource3.volume = (float)PlayerPreferences.playerPreferences.preferenceData.musicVolume / 100f;
        previewSongTimer = TRACK_PREVIEW_TIME;
        if (mode == 'A') {
            audioSource3.clip = assaultTracks[i];
        } else if (mode == 'S') {
            audioSource3.clip = stealthTracks[i];
        }
        audioSource3.Play();
    }

}
