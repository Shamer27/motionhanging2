using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextscene : MonoBehaviour
{
    public LevelTimer timer;
    public string scenename;

    [Header("levelSelectUI")]
    public string levelSaveKey = "Level1Shaded";
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer.StopTimer();

            float completionTime = timer.GetElapsedTime();

            if (string.IsNullOrEmpty(levelSaveKey))
            {
                Debug.LogWarning("No levelSaveKey assigned in LevelEndHandler.");
                return;
            }

            if (PlayerPrefs.HasKey(levelSaveKey))
            {
                float savedTime = PlayerPrefs.GetFloat(levelSaveKey);
                if (completionTime < savedTime)
                {
                    PlayerPrefs.SetFloat(levelSaveKey, completionTime);
                }
            }
            else
            {
                PlayerPrefs.SetFloat(levelSaveKey, completionTime);
            }

            PlayerPrefs.Save();
            Debug.Log("Level completed! Time: " + completionTime + " saved to key: " + levelSaveKey);
            SceneManager.LoadScene(scenename);
        }
        
        
    }
}
