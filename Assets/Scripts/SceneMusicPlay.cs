using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicPlay : SingletonMonoBehaviour<SceneMusicPlay>
{

    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioClip gameMusicClip;
    [SerializeField] private AudioClip menuMusicClip;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadFadeInEvent += PlaySceneMusic;
        EventHandler.BeforeSceneUnloadFadeOutEvent += StopSceneMusic;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadFadeInEvent -= PlaySceneMusic;
        EventHandler.BeforeSceneUnloadFadeOutEvent -= StopSceneMusic;
    }


    private void StopSceneMusic(string oldSceneName, string newSceneName)
    {
        if (oldSceneName == newSceneName)
        {
            return;
        }
        audioSource.Stop();
    }

    private void PlaySceneMusic(string oldSceneName, string newSceneName)
    {
        // only play if the scene changes
        // e.g., menu -> game.
        // continue music through level changes.
        if (oldSceneName ==  newSceneName)
        {
            return;
        }

        if (newSceneName == "HomeScreen")
        {
            audioSource.clip = menuMusicClip;
        } else {
            audioSource.clip = gameMusicClip;
        }
        audioSource.Play();
    }
}
