using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;
using TMPro;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Fix bug where you can't use pause menu on levels
/// </summary>/
public class AllMenus : MonoBehaviour
{
    #region Settings
    [Header("Settings Vars")]
    public PlayerControllerScript PlayerControllerScript;
    public Button jumpRebindButton;
    public Button slideRebindButton;
    public Button sprintRebindButton;
    public Button swingRebindButton;
    public Button forwardRebindButton;
    public Button backwardRebindButton;
    public Button leftRebindButton;
    public Button rightRebindButton;
    public Button closeSettingsButton;
    public Button applyButton;

    private Button currentListeningButton;
    private Action<KeyCode> onKeyRebind;

    #endregion

    #region UIObjects
    [Header("UI Objects")]
    public GameObject pauseUI;
    public GameObject settingsUI;
    public GameObject mainUI;

    public GameObject playerUI;
    #endregion

    #region Menu States
    [Header("Menu Stats")]
    public static bool GameIsPaused = false;
    public static bool SettingsOpened = false;

    public static bool MainMenuOpen = true;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Show menu in all scenes except MenuBackground
        if (currentScene != "MenuBackground")
        {
            mainUI.SetActive(false);
            MainMenuOpen = false;
            playerUI.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {

            mainUI.SetActive(true); // on game start show the main menu
            playerUI.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        pauseUI.SetActive(false);
        settingsUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (currentScene == "MenuBackground")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        //
        // Load Conrtols
        //
        LoadSettings();

        // Bind buttons to rebind actions
        jumpRebindButton.onClick.AddListener(() => StartRebinding(jumpRebindButton, key => PlayerControllerScript.bindings.jumpKey = key));
        slideRebindButton.onClick.AddListener(() => StartRebinding(slideRebindButton, key => PlayerControllerScript.bindings.slideKey = key));
        sprintRebindButton.onClick.AddListener(() => StartRebinding(sprintRebindButton, key => PlayerControllerScript.bindings.sprintKey = key));
        swingRebindButton.onClick.AddListener(() => StartRebinding(swingRebindButton, key => PlayerControllerScript.bindings.swingMouseKey = (int)key));
        forwardRebindButton.onClick.AddListener(() => StartRebinding(forwardRebindButton, key => PlayerControllerScript.bindings.forwardKey = key));
        backwardRebindButton.onClick.AddListener(() => StartRebinding(backwardRebindButton, key => PlayerControllerScript.bindings.backwardKey = key));
        leftRebindButton.onClick.AddListener(() => StartRebinding(leftRebindButton, key => PlayerControllerScript.bindings.leftKey = key));
        rightRebindButton.onClick.AddListener(() => StartRebinding(rightRebindButton, key => PlayerControllerScript.bindings.rightKey = key));
        applyButton.onClick.AddListener(SaveSettings);

        closeSettingsButton.onClick.AddListener(SaveSettings);

        // Show correct key names on the buttons
        UpdateButtonLabels();
    }

    // Update is called once per frame
    void Update()
    {


        if (currentListeningButton != null && Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    onKeyRebind?.Invoke(keyCode);
                    SetButtonText(currentListeningButton, keyCode.ToString());
                    currentListeningButton = null;
                    onKeyRebind = null;
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !MainMenuOpen)
        {
            if (SettingsOpened)
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
        if (!MainMenuOpen && GameIsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    #region settings functions

    // change keybinds based on the input
    private void StartRebinding(Button button, Action<KeyCode> onRebind)
    {
        currentListeningButton = button;
        onKeyRebind = onRebind;
        SetButtonText(button, "Press a key...");
    }

    //sets the text of the button to what the keybind is
    void SetButtonText(Button button, string newText)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = newText;
        }
    }

    //save the keybinds
    public void SaveSettings()
    {
        var bindings = PlayerControllerScript.bindings;

        PlayerPrefs.SetString("JumpKey", bindings.jumpKey.ToString());
        PlayerPrefs.SetString("SlideKey", bindings.slideKey.ToString());
        PlayerPrefs.SetString("SprintKey", bindings.sprintKey.ToString());
        PlayerPrefs.SetInt("SwingMouseKey", bindings.swingMouseKey);
        PlayerPrefs.SetString("ForwardKey", bindings.forwardKey.ToString());
        PlayerPrefs.SetString("BackwardKey", bindings.backwardKey.ToString());
        PlayerPrefs.SetString("LeftKey", bindings.leftKey.ToString());
        PlayerPrefs.SetString("RightKey", bindings.rightKey.ToString());

        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }

    //load the keybind on start
    public void LoadSettings()
    {
        var bindings = PlayerControllerScript.bindings;

        if (PlayerPrefs.HasKey("JumpKey")) bindings.jumpKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey"));
        if (PlayerPrefs.HasKey("SlideKey")) bindings.slideKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlideKey"));
        if (PlayerPrefs.HasKey("SprintKey")) bindings.sprintKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SprintKey"));
        if (PlayerPrefs.HasKey("SwingMouseKey")) bindings.swingMouseKey = PlayerPrefs.GetInt("SwingMouseKey");
        if (PlayerPrefs.HasKey("ForwardKey")) bindings.forwardKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ForwardKey"));
        if (PlayerPrefs.HasKey("BackwardKey")) bindings.backwardKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BackwardKey"));
        if (PlayerPrefs.HasKey("LeftKey")) bindings.leftKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey"));
        if (PlayerPrefs.HasKey("RightKey")) bindings.rightKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey"));
    }

    //changes the text of the button if the keybind has been changed
    private void UpdateButtonLabels()
    {
        var b = PlayerControllerScript.bindings;

        SetButtonText(jumpRebindButton, b.jumpKey.ToString());
        SetButtonText(slideRebindButton, b.slideKey.ToString());
        SetButtonText(sprintRebindButton, b.sprintKey.ToString());
        SetButtonText(swingRebindButton, $"Mouse{b.swingMouseKey}");
        SetButtonText(forwardRebindButton, b.forwardKey.ToString());
        SetButtonText(backwardRebindButton, b.backwardKey.ToString());
        SetButtonText(leftRebindButton, b.leftKey.ToString());
        SetButtonText(rightRebindButton, b.rightKey.ToString());
    }

    //button to main menu
    public void BackButton()
    {
        settingsUI.SetActive(false);
        pauseUI.SetActive(true);
    }
    #endregion

    #region pauseMenu
    void Pause()
    {
        pauseUI.SetActive(true);
        playerUI.SetActive(false);
        settingsUI.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        SettingsOpened = false;
        GameIsPaused = true;
        // Show pause menu UI here
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        settingsUI.SetActive(false);
        SettingsOpened = false;
        playerUI.SetActive(true);
        MainMenuOpen = false;
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Hide pause menu UI here
    }

    public void Settings()
    {
        Debug.Log("Settings Menu...");
        pauseUI.SetActive(false); // 🔥 Hide the menu at game start
        playerUI.SetActive(false);
        settingsUI.SetActive(true);
        mainUI.SetActive(false);
        SettingsOpened = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSettings()
    {
        settingsUI.SetActive(false);
        pauseUI.SetActive(true);
        SettingsOpened = false;
    }

    public void Back2MainMenu()
    {
        // Load the main menu scene 
        SceneManager.LoadScene("MenuBackground");
        Debug.Log("Loading Menu...");
        MainMenuOpen = true;
        pauseUI.SetActive(false);
        playerUI.SetActive(false);
        settingsUI.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region mainMenu
    public void onPlayButton()
    {
        SceneManager.LoadScene("Level1Shaded");
        Cursor.visible = false;
        Resume();
        playerUI.SetActive(true);
    }

    public void onQuitButton()
    {
        Application.Quit();
    }

    #endregion
}
