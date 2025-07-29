using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    // Update is called once per frame

    void Start()
    {
        pauseMenuUI.SetActive(false); // Hide the menu at game start
        Time.timeScale = 1f;          // Ensure game is unpaused
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Unlock the cursor
        Cursor.visible = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Check for Escape key press
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void onPlayButton()
    {
        SceneManager.LoadScene("Level1Shaded");
    }

    public void onQuitButton()
    {
        Application.Quit();
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
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        GameIsPaused = true;
        // Show pause menu UI here
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void LoadMenu() //main menu
    {
        // Load the main menu scene
        SceneManager.LoadScene("MenuBackground");
        Debug.Log("Loading Menu...");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        pauseMenuUI.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

}
