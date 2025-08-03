using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public TMP_Text timerText;
    private float elapsedTime;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
        timerText.text = time.ToString(@"mm\:ss\.ff"); // 02:34.56
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
