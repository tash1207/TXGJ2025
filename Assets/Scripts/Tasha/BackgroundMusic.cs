using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}
