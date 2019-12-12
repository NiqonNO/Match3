using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioConfig audioData;
    AudioSource[] audioSources;

    private void Awake()
    {
        audioSources = GetComponents<AudioSource>();
        audioSources[0].clip = audioData.backgroundMusic;
        audioSources[0].volume = audioData.backgroundMusicVolume;
        audioSources[1].clip = audioData.clearSound;
        audioSources[1].volume = audioData.clearSoundVolume;
        audioSources[2].clip = audioData.selectSound;
        audioSources[2].volume = audioData.selectSoundVolume;
        audioSources[3].clip = audioData.swapSound;
        audioSources[3].volume = audioData.swapSoundVolume;
        audioSources[0].Play();
    }

    public void PlayClear()
    {
        audioSources[1].Play();
    }
    public void PlaySelect()
    {
        audioSources[2].Play();
    }
    public void PlaySwap()
    {
        audioSources[3].Play();
    }
}
