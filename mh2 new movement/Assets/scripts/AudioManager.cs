using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> musicTracks;
    public AudioSource audioSource;
    private int currentTrackIndex;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (FindObjectsOfType<AudioManager>().Length > 1)
        {
            Destroy(gameObject);
        }
        
        if (musicTracks.Count == 0)
        {
            Debug.LogError("No music tracks assigned!");
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        PlayRandomTrack();
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (!audioSource.isPlaying && currentScene != "Menubackground")
        {
            PlayRandomTrack();
        }
    }

    void PlayRandomTrack()
    {
        int nextTrackIndex = Random.Range(0, musicTracks.Count);

        // Ensure the same track isn't played twice in a row.
        while (nextTrackIndex == currentTrackIndex && musicTracks.Count > 1)
        {
            nextTrackIndex = Random.Range(0, musicTracks.Count);
        }

        currentTrackIndex = nextTrackIndex;
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.Play();
    }
}