using UnityEngine;

namespace ParkourFPS
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        [Header("Sounds")]
        public bool soundEnabled = true; // enable or disable sound
        public AudioClip walkingSound;
        public AudioClip runningSound;
        public AudioClip jumpSound;
        public AudioClip landingSound;
        public AudioClip slidingSound;

        private AudioSource audioSource;

        public bool isPlaying { get { return audioSource.isPlaying; } } // if a sound is currently being played

        public AudioClip clip { get { return audioSource.clip; } } // current audio clip being played

        // Start is called before the first frame update
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // play a selected audio clip with parameters
        public void PlaySound(AudioClip clip, bool loop = false, float volume = 1, float pitch = 1)
        {
                // set parameters
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;

            if (!soundEnabled) return; // if sound is disabled, do not play
            // play audio clip
                audioSource.Play();
        }

        // stop playing the current audio clip
        public void Stop() => audioSource.Stop();
    }
}