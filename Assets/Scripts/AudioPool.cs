using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPool : MonoBehaviour
{
    [SerializeField]
    AudioMixerGroup defaultMixerGroup;

    static AudioPool _instance;

    static AudioPool Instance
    {
        get
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<GameObject>("Audio Pool");
                _instance = GameObject.Instantiate(prefab).GetComponent<AudioPool>();
            }
            return _instance;
        }
    }

    [NonSerialized]
    AudioSource _source;

    public AudioSource Source
    {
        get
        {
            if (_source == null)
            {
                _source = GetComponent<AudioSource>();
            }
            return _source;
        }
    }

    static Queue<AudioPool> pooledObjects = new Queue<AudioPool>();

    public static AudioPool PlaySoundTillDone(AudioClip clip, Vector3 position, float volume = 1, AudioMixerGroup mixerGroup = default)
    {
        var instance = PlaySound(clip, position, volume, mixerGroup);
        ReturnToPool(instance, clip.length);
        return instance;
    }

    public static AudioPool PlaySound(AudioClip clip, Vector3 position, float volume = 1, AudioMixerGroup mixerGroup = default)
    {
        Debug.Log("Position = " + position);
        if (pooledObjects.TryDequeue(out var instance))
        {
            instance.transform.position = position;
            instance.gameObject.SetActive(true);
        }
        else
        {
            instance = GameObject.Instantiate(Instance, position, Quaternion.identity);
        }
        instance.Source.clip = clip;
        instance.Source.outputAudioMixerGroup = mixerGroup ?? Instance.defaultMixerGroup;
        instance.Source.volume = volume;
        instance.Source.spatialBlend = 1;
        instance.Source.Play();
        return instance;
    }

    static IEnumerator ReturnToPoolRoutine(AudioPool audio, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            yield return null;
        }
        ReturnToPool(audio);
    }

    public static void ReturnToPool(AudioPool audio, float time)
    {
        if (time <= 0)
        {
            ReturnToPool(audio);
        }
        else
        {
            Instance.StartCoroutine(ReturnToPoolRoutine(audio, time));
        }
    }

    public static void ReturnToPool(AudioPool audio)
    {
        audio.Source.Stop();
        audio.Source.clip = null;
        audio.Source.outputAudioMixerGroup = null;
        audio.Source.volume = 1f;
        audio.gameObject.SetActive(false);
        pooledObjects.Enqueue(audio);
    }
}
