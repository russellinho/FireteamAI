using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceCommandScript : MonoBehaviour
{
    public AudioClip commandAudioMale;
    public AudioClip commandAudioFemale;
    public string commandString;

    public AudioClip GetCommandAudio(char gender) {
        if (gender == 'M') {
            return commandAudioMale;
        }
        return commandAudioFemale;
    }
}
