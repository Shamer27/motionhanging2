using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndHandler : MonoBehaviour
{

    public LevelTimer timer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer.StopTimer();

            float completionTime = timer.GetElapsedTime();
            PlayerPrefs.SetFloat("Level1Time", completionTime);
            PlayerPrefs.Save();

            Debug.Log("Level completed! Time: " + completionTime);
            // Here you can add logic to load the next level or show a completion screen
        }
    }
}
