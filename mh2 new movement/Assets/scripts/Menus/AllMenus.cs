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

    [Header("Sensitivity")]
    public Slider sensitivitySlider;
    public TMP_InputField sensitivityInput;

    [Header("Audio")]
    public Slider musicVolumeSlider;
    public TMP_InputField musicVolumeInput;
    public AudioManager audioManager;

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

        // Keybind listeners
        jumpRebindButton.onClick.AddListener(() => StartRebinding(jumpRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.jumpKey = key));
        slideRebindButton.onClick.AddListener(() => StartRebinding(slideRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.slideKey = key));
        sprintRebindButton.onClick.AddListener(() => StartRebinding(sprintRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.sprintKey = key));
        forwardRebindButton.onClick.AddListener(() => StartRebinding(forwardRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.forwardKey = key));
        backwardRebindButton.onClick.AddListener(() => StartRebinding(backwardRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.backwardKey = key));
        leftRebindButton.onClick.AddListener(() => StartRebinding(leftRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.leftKey = key));
        rightRebindButton.onClick.AddListener(() => StartRebinding(rightRebindButton, (key) => playerController.GetComponent<PlayerControllerScript>().bindings.rightKey = key));
        swingRebindButton.onClick.AddListener(() => StartCoroutine(ListenForMouseButton()));

        UpdateButtonLabels();

        // Sensitivity setup
        var player = playerController.GetComponent<PlayerControllerScript>();
        sensitivitySlider.value = player.lookSensitivity;
        sensitivityInput.text = player.lookSensitivity.ToString("F2");

        sensitivitySlider.onValueChanged.AddListener((value) =>
        {
            value = Mathf.Round(value * 100f) / 100f;
            player.lookSensitivity = value;
            sensitivityInput.text = value.ToString("F2");
        });

        sensitivityInput.onEndEdit.AddListener((string text) =>
        {
            if (float.TryParse(text, out float value))
            {
                value = Mathf.Clamp(Mathf.Round(value * 100f) / 100f, sensitivitySlider.minValue, sensitivitySlider.maxValue);
                player.lookSensitivity = value;
                sensitivitySlider.value = value;
            }
            else
            {
                sensitivityInput.text = player.lookSensitivity.ToString("F2");
            }
        });

        // Music volume setup
        if (audioManager != null && audioManager.audioSource != null)
        {
            float initialVolume = PlayerPrefs.HasKey("MusicVolume")
                ? PlayerPrefs.GetFloat("MusicVolume")
                : audioManager.audioSource.volume;

            audioManager.audioSource.volume = initialVolume;
            musicVolumeSlider.value = initialVolume;
            musicVolumeInput.text = initialVolume.ToString("F2");

            musicVolumeSlider.onValueChanged.AddListener((value) =>
            {
                value = Mathf.Round(value * 100f) / 100f;
                audioManager.audioSource.volume = value;
                musicVolumeInput.text = value.ToString("F2");
            });

            musicVolumeInput.onEndEdit.AddListener((string text) =>
            {
                if (float.TryParse(text, out float value))
                {
                    value = Mathf.Clamp(Mathf.Round(value * 100f) / 100f, musicVolumeSlider.minValue, musicVolumeSlider.maxValue);
                    audioManager.audioSource.volume = value;
                    musicVolumeSlider.value = value;
                }
                else
                {
                    musicVolumeInput.text = audioManager.audioSource.volume.ToString("F2");
                }
            });
        } // ✅ Closed this brace
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
        }

        if (onKeyRebind != null && Input.anyKeyDown)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    FinishRebind(key);
                    break;
                }
            }
        }


        if (audioManager == null)
        {
            // Try to find the AudioManager if it wasn't assigned in the inspector
            audioManager = FindObjectOfType<AudioManager>();
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

            PlayerPrefs.SetFloat("LookSensitivity", playerController.GetComponent<PlayerControllerScript>().lookSensitivity);

            if (audioManager != null && audioManager.audioSource != null)
                PlayerPrefs.SetFloat("MusicVolume", audioManager.audioSource.volume);
            

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

            if (PlayerPrefs.HasKey("LookSensitivity"))
            {
                playerController.GetComponent<PlayerControllerScript>().lookSensitivity = PlayerPrefs.GetFloat("LookSensitivity");
            }

            if (audioManager != null && audioManager.audioSource != null && PlayerPrefs.HasKey("MusicVolume"))
            {
                audioManager.audioSource.volume = PlayerPrefs.GetFloat("MusicVolume");
            }
                               
        }

        private void FinishRebind(KeyCode newKey)
        {
            onKeyRebind?.Invoke(newKey);
            SetButtonText(currentListeningButton, newKey.ToString());
            SaveSettings();
            currentListeningButton = null;
            onKeyRebind = null;
        }

    }

