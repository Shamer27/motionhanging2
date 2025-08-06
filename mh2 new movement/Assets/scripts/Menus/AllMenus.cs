using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject settingsPanel;
    public GameObject levelSelect;

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


    [Header("Player")]
    public GameObject playerController;
    public GameObject playerUI;

    [Header("Scene Names")]
    public List<string> levelSceneNames; // Level scene names (e.g., "level1", "level2", ...)

    public static bool GameIsPaused { get; private set; } = false;

    private bool inMainMenu = true;
    private bool inSettings = false;

    private void Start()
    {
        ShowMainMenu();

        jumpRebindButton.onClick.AddListener(() => StartRebinding(jumpRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.jumpKey = key));
        slideRebindButton.onClick.AddListener(() => StartRebinding(slideRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.slideKey = key));
        sprintRebindButton.onClick.AddListener(() => StartRebinding(sprintRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.sprintKey = key));
        forwardRebindButton.onClick.AddListener(() => StartRebinding(forwardRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.forwardKey = key));
        backwardRebindButton.onClick.AddListener(() => StartRebinding(backwardRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.backwardKey = key));
        leftRebindButton.onClick.AddListener(() => StartRebinding(leftRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.leftKey = key));
        rightRebindButton.onClick.AddListener(() => StartRebinding(rightRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.rightKey = key));
        swingRebindButton.onClick.AddListener(() => StartCoroutine(ListenForMouseButton()));

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inMainMenu) return;

            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }

            // Block escape from being picked up twice in one frame
            // EventSystem.current.SetSelectedGameObject(null);
        }

        if (onKeyRebind != null && Input.anyKeyDown)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    onKeyRebind.Invoke(key);
                    SetButtonText(currentListeningButton, key.ToString());
                    currentListeningButton = null;
                    onKeyRebind = null;
                    UpdateButtonLabels();
                    break;
                }
            }
        }
    }

    // ------------------ Main Menu ------------------

    public void ShowMainMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MenuBackground")
        {
            Time.timeScale = 0f;
            GameIsPaused = true;
            inMainMenu = true;

            mainMenu.SetActive(true);
            pauseMenu.SetActive(false);
            settingsPanel.SetActive(false);
            levelSelect.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            playerUI?.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            GameIsPaused = false;
            inMainMenu = false;

            mainMenu.SetActive(false);
            pauseMenu.SetActive(false);
            settingsPanel.SetActive(false);
            levelSelect.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            playerUI?.SetActive(true);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1Shaded");
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelSceneNames.Count) return;
        
    // Disable all UI buttons to block second input
        DisableAllMenus();

        Time.timeScale = 1f;
        GameIsPaused = false;
        inMainMenu = false;

        StartCoroutine(LoadSceneAndStart(index));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator<WaitForSeconds> LoadSceneAndStart(int index)
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        inMainMenu = false;

        yield return new WaitForSeconds(0.1f); // Optional buffer
        SceneManager.LoadScene(levelSceneNames[index]);
    }

    // ------------------ Pause Menu ------------------

    public void PauseGame()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);

        // Don’t disable the player
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MenuBackground");
        ShowMainMenu();
    }

    // ------------------ Settings ------------------

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsWithDelay());
    }

    private IEnumerator OpenSettingsWithDelay()
    {
        // Force UI reset
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // Wait 1 frame to flush UI input

        DisableAllMenus();
        settingsPanel.SetActive(true);
        inSettings = true;
    }



    public void CloseSettings()
    {
        if (!settingsPanel.activeSelf) return; // prevent double close

        settingsPanel.SetActive(false);
        inSettings = false;

        if (inMainMenu)
            mainMenu.SetActive(true);
        else
            pauseMenu.SetActive(true);
    }

    public void ShowLevelSelect()
    {
        levelSelect.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void closeLevelSelect()
    {
        levelSelect.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void DisableAllMenus()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsPanel.SetActive(false);
        levelSelect.SetActive(false);
    }

    // ------------------ Key Rebinding ------------------
    private void StartRebinding(Button button, Action<KeyCode> onRebind)
        {
            currentListeningButton = button;
            onKeyRebind = onRebind;
            SetButtonText(button, "Press a key...");
        }

        private IEnumerator ListenForMouseButton()
        {
            SetButtonText(swingRebindButton, "Click a mouse button...");
            yield return null;

            while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                yield return null;

            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    playerController.GetComponent<PlayerControllerScript>().bindings.swingMouseKey = i;
                    SetButtonText(swingRebindButton, $"Mouse{i}");
                    break;
                }
            }
        }

        private void SetButtonText(Button button, string newText)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = newText;
        }

        private void UpdateButtonLabels()
        {
            var b = playerController.GetComponent<PlayerControllerScript>().bindings;

            SetButtonText(jumpRebindButton, b.jumpKey.ToString());
            SetButtonText(slideRebindButton, b.slideKey.ToString());
            SetButtonText(sprintRebindButton, b.sprintKey.ToString());
            SetButtonText(swingRebindButton, $"Mouse{b.swingMouseKey}");
            SetButtonText(forwardRebindButton, b.forwardKey.ToString());
            SetButtonText(backwardRebindButton, b.backwardKey.ToString());
            SetButtonText(leftRebindButton, b.leftKey.ToString());
            SetButtonText(rightRebindButton, b.rightKey.ToString());
        }

        public void SaveSettings()
        {
            var bindings = playerController.GetComponent<PlayerControllerScript>().bindings;

            PlayerPrefs.SetString("JumpKey", bindings.jumpKey.ToString());
            PlayerPrefs.SetString("SlideKey", bindings.slideKey.ToString());
            PlayerPrefs.SetString("SprintKey", bindings.sprintKey.ToString());
            PlayerPrefs.SetInt("SwingMouseKey", bindings.swingMouseKey);
            PlayerPrefs.SetString("ForwardKey", bindings.forwardKey.ToString());
            PlayerPrefs.SetString("BackwardKey", bindings.backwardKey.ToString());
            PlayerPrefs.SetString("LeftKey", bindings.leftKey.ToString());
            PlayerPrefs.SetString("RightKey", bindings.rightKey.ToString());

            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            var bindings = playerController.GetComponent<PlayerControllerScript>().bindings;

            if (PlayerPrefs.HasKey("JumpKey")) bindings.jumpKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey"));
            if (PlayerPrefs.HasKey("SlideKey")) bindings.slideKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlideKey"));
            if (PlayerPrefs.HasKey("SprintKey")) bindings.sprintKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SprintKey"));
            if (PlayerPrefs.HasKey("SwingMouseKey")) bindings.swingMouseKey = PlayerPrefs.GetInt("SwingMouseKey");
            if (PlayerPrefs.HasKey("ForwardKey")) bindings.forwardKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ForwardKey"));
            if (PlayerPrefs.HasKey("BackwardKey")) bindings.backwardKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BackwardKey"));
            if (PlayerPrefs.HasKey("LeftKey")) bindings.leftKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey"));
            if (PlayerPrefs.HasKey("RightKey")) bindings.rightKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey"));
        }

}
