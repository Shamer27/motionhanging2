using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndHandler : MonoBehaviour
{

    public LevelTimer timer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer.StopTimer();

            float completionTime = timer.GetElapsedTime();

            string sceneName = SceneManager.GetActiveScene().name;

            string key = sceneName + "Time";

            if (PlayerPrefs.HasKey(key))
            {
                float savedTime = PlayerPrefs.GetFloat(key);
                if (completionTime < savedTime)
                {
                    PlayerPrefs.SetFloat(key, completionTime);
                }
            }
            else
            {
                PlayerPrefs.SetFloat(key, completionTime);
            }
            
            PlayerPrefs.Save();
            Debug.Log("Level completed! Time: " + completionTime);
            // Here you can add logic to load the next level or show a completion screen
        }
    }
}
