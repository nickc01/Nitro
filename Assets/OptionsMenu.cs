using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    AudioMixer mixer;

    [SerializeField]
    Slider masterSlider;

    [SerializeField]
    Slider musicSlider;

    [SerializeField]
    Slider sfxSlider;

    float master;
    float music;
    float sfx;

    bool dirty = false;

    private void Awake()
    {
        master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMasterVolume(master);
        SetMasterVolume(music);
        SetSFXVolume(sfx);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;
    }

    public void SetMasterVolume(float value)
    {
        master = value;
        dirty = true;
        mixer.SetFloat("MasterVolume",Mathf.Lerp(-80,0,value));
    }

    public void SetMusicVolume(float value)
    {
        music = value;
        dirty = true;
        mixer.SetFloat("MusicVolume", Mathf.Lerp(-80, 0, value));
    }

    public void SetSFXVolume(float value)
    {
        sfx = value;
        dirty = true;
        mixer.SetFloat("SFXVolume", Mathf.Lerp(-80, 0, value));
    }

    private void LateUpdate()
    {
        if (dirty)
        {
            dirty = false;
            PlayerPrefs.SetFloat("MasterVolume", master);
            PlayerPrefs.SetFloat("MusicVolume", music);
            PlayerPrefs.SetFloat("SFXVolume", sfx);
        }
    }
}
