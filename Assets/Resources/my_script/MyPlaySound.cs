using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlaySound : MonoBehaviour
{
    private static AudioSource myAudioSource;

    private void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    public static void MyPlayClip(AudioClip _clip)
    {
        float volumeScale = 1;
        if (myAudioSource.isPlaying)
            volumeScale = 0.5f;

        myAudioSource.PlayOneShot(_clip, volumeScale);
    }
}
