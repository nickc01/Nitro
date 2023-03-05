using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    float fadeTime = 0.25f;

    static MusicPlayer _instance;
    public static MusicPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MusicPlayer>();
            }
            return _instance;
        }
    }

    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.Play();
        audioSource.volume = 1f;
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(audioSource.volume,1f));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(audioSource.volume, 0f));
    }

    IEnumerator FadeRoutine(float from, float to)
    {
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }
        audioSource.volume = to;
    }
}
