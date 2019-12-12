using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Audio Config", order = 2)]
public class AudioConfig : ScriptableObject
{
    public AudioClip backgroundMusic;
    [Range(0, 1)]
    public float backgroundMusicVolume;
    public AudioClip clearSound;
    [Range(0, 1)]
    public float clearSoundVolume;
    public AudioClip selectSound;
    [Range(0, 1)]
    public float selectSoundVolume;
    public AudioClip swapSound;
    [Range(0, 1)]
    public float swapSoundVolume;

    public AudioConfig()
    {
        backgroundMusicVolume = 1;
        clearSoundVolume = 1;
        selectSoundVolume = 1;
        swapSoundVolume = 1;
    }
}
