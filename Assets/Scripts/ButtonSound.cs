using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    [SerializeField]
    AudioClip sound;

    [SerializeField]
    AudioMixerGroup mixerGroup;

    [SerializeField]
    float volume = 1f;

    private void Awake()
    {
        if (!TryGetComponent<AudioSource>(out var audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.clip = sound;
        audioSource.outputAudioMixerGroup = mixerGroup;

        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(() =>
            {
                audioSource.Play();
            });
        }
    }
}
