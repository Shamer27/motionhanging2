using System;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;
using TMPro;
using Menu;

public class SettingsMenu : MonoBehaviour
{
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


    void Start()
    {
        // Load saved bindings
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
    void Update()
    {
        if (currentListeningButton != null)
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
    }
    private void StartRebinding(Button button, Action<KeyCode> onRebind)
    {
        currentListeningButton = button;
        onKeyRebind = onRebind;
        SetButtonText(button, "Press a key...");
    }

    private void SetButtonText(Button button, string text)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = text;
    }

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

    public void BackButton()
    {
        Menu.settingsPanel.SetActive(false);
        Menu.pauseMenuUI.SetActive(true);
    }
}
