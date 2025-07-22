using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    // Update is called once per frame

    void start()
    {
        pauseMenuUI.SetActive(false); // 🔥 Hide the menu at game start
        Time.timeScale = 1f;          // Ensure game is unpaused
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Unlock the cursor
        Cursor.visible = false;
    }
    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
          if (GameIsPaused)
          {
              Resume();
          }
          else
          {
              Pause();
          }
      }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Hide pause menu UI here
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        // Show pause menu UI here
    }

    public void LoadMenu()
    {
        // Load the main menu scene
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Loading Menu...");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
