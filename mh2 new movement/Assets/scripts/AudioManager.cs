// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class AudioManager : MonoBehaviour
// {

//     public AudioSource BGM;
//     // Start is called before the first frame update
    // void Start()
    // {
    //     DontDestroyOnLoad(gameObject);

    //     if (FindObjectsOfType<AudioManager>().Length > 1)
    //     {
    //         Destroy(gameObject);
    //     }

    // }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     public void ChangeBGM(AudioClip music)
//     {
//         if (BGM.clip.name == music.name)
//             return;

//         BGM.Stop();
//         BGM.clip = music;
//         BGM.Play();
//     }
// }

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        if (!audioSource.isPlaying)
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