using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] tracks;


    private void Update()
    {
        if (!audioSource.isPlaying)
            PlayTrack();
    }

    private void OnDisable()
    {
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    void PlayTrack()
    {
        audioSource.clip = tracks[UnityEngine.Random.Range(0, tracks.Length)];
        audioSource.Play();
    }
}
