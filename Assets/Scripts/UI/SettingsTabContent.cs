using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsTabContent : TabContent
{
    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private AudioSource audioSource; // For testing sounds
    [SerializeField] private AudioClip testSound;
    
    [Header("Graphics Settings")]
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Slider brightnessSlider;
    
    [Header("Gameplay Settings")]
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Dropdown difficultyDropdown;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Slider sensitivitySlider;
    
    [Header("Account")]
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Text versionText;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Text confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private bool isInitializing = false;
    private const string VERSION = "1.0.0";

    protected override void Awake()
    {
        base.Awake();
        SetupListeners();
        if (versionText != null)
            versionText.text = $"Version {VERSION}";
    }

    private void SetupListeners()
    {
        // Audio listeners
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteChanged);

        // Graphics listeners    
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        // Gameplay listeners
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        if (tutorialToggle != null)
            tutorialToggle.onValueChanged.AddListener(OnTutorialChanged);
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        // Button listeners
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveSettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ShowResetConfirmation);
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ResetSettings);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(HideConfirmationDialog);
            
        // Input field listeners
        if (playerNameInput != null)
        {
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
            playerNameInput.characterLimit = 16;
        }
    }

    protected override IEnumerator LoadContent()
    {
        isInitializing = true;
        
        // Initialize dropdowns
        InitializeQualityDropdown();
        InitializeDifficultyDropdown();
        
        // Load saved settings
        LoadSettings();
        
        yield return new WaitForSeconds(0.5f); // Give time for UI to update
        
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
            
        isInitializing = false;
    }

    private void InitializeQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string> 
            {
                "Low",
                "Medium",
                "High",
                "Ultra"
            });
        }
    }

    private void InitializeDifficultyDropdown()
    {
        if (difficultyDropdown != null)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string> 
            {
                "Easy",
                "Normal",
                "Hard",
                "Expert"
            });
        }
    }

    private void LoadSettings()
    {
        // Audio settings
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (muteToggle != null)
            muteToggle.isOn = PlayerPrefs.GetInt("Muted", 0) == 1;

        // Graphics settings
        if (qualityDropdown != null)
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (vsyncToggle != null)
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        if (brightnessSlider != null)
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);

        // Gameplay settings
        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        if (tutorialToggle != null)
            tutorialToggle.isOn = PlayerPrefs.GetInt("ShowTutorials", 1) == 1;
        if (difficultyDropdown != null)
            difficultyDropdown.value = PlayerPrefs.GetInt("Difficulty", 1);
        if (invertYToggle != null)
            invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
        if (sensitivitySlider != null)
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);

        // Account settings
        if (playerNameInput != null)
            playerNameInput.text = PlayerPrefs.GetString("PlayerName", "Player");
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (isInitializing) return;
        PlayerPrefs.SetFloat("MusicVolume", value);
        // Implement actual volume change through audio manager
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (isInitializing) return;
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        // Play test sound
        if (audioSource != null && testSound != null)
        {
            audioSource.PlayOneShot(testSound, value);
        }
    }

    private void OnMuteChanged(bool muted)
    {
        if (isInitializing) return;
        PlayerPrefs.SetInt("Muted", muted ? 1 : 0);
        
        // Update UI state
        musicSlider.interactable = !muted;
        sfxSlider.interactable = !muted;
    }

    private void OnQualityChanged(int index)
    {
        if (isInitializing) return;
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    private void OnFullscreenChanged(bool fullscreen)
    {
        if (isInitializing) return;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }

    private void OnVSyncChanged(bool enabled)
    {
        if (isInitializing) return;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
    }

    private void OnBrightnessChanged(float value)
    {
        if (isInitializing) return;
        PlayerPrefs.SetFloat("Brightness", value);
        // Implement brightness change through post-processing
    }

    private void OnVibrationChanged(bool enabled)
    {
        if (isInitializing) return;
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
        // Implement vibration toggle through device
    }

    private void OnTutorialChanged(bool enabled)
    {
        if (isInitializing) return;
        PlayerPrefs.SetInt("ShowTutorials", enabled ? 1 : 0);
    }

    private void OnDifficultyChanged(int index)
    {
        if (isInitializing) return;
        PlayerPrefs.SetInt("Difficulty", index);
    }

    private void OnInvertYChanged(bool enabled)
    {
        if (isInitializing) return;
        PlayerPrefs.SetInt("InvertY", enabled ? 1 : 0);
    }

    private void OnSensitivityChanged(float value)
    {
        if (isInitializing) return;
        PlayerPrefs.SetFloat("Sensitivity", value);
    }

    private void OnPlayerNameChanged(string newName)
    {
        if (isInitializing) return;
        saveButton.interactable = !string.IsNullOrEmpty(newName.Trim());
    }

    private void ShowResetConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            confirmationText.text = "Reset all settings to default values?";
        }
    }

    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    private void SaveSettings()
    {
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text.Trim()))
        {
            PlayerPrefs.SetString("PlayerName", playerNameInput.text.Trim());
        }
        
        PlayerPrefs.Save();
        StartCoroutine(ShowSaveConfirmation());
    }

    private IEnumerator ShowSaveConfirmation()
    {
        saveButton.interactable = false;
        Text buttonText = saveButton.GetComponentInChildren<Text>();
        string originalText = buttonText.text;
        buttonText.text = "Saved!";
        
        yield return new WaitForSeconds(1f);
        
        buttonText.text = originalText;
        saveButton.interactable = true;
    }

    private void ResetSettings()
    {
        isInitializing = true;
        
        // Reset Audio
        if (musicSlider != null) musicSlider.value = 1f;
        if (sfxSlider != null) sfxSlider.value = 1f;
        if (muteToggle != null) muteToggle.isOn = false;

        // Reset Graphics
        if (qualityDropdown != null) qualityDropdown.value = 2; // Medium quality
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (vsyncToggle != null) vsyncToggle.isOn = true;
        if (brightnessSlider != null) brightnessSlider.value = 1f;

        // Reset Gameplay
        if (vibrationToggle != null) vibrationToggle.isOn = true;
        if (tutorialToggle != null) tutorialToggle.isOn = true;
        if (difficultyDropdown != null) difficultyDropdown.value = 1; // Normal difficulty
        if (invertYToggle != null) invertYToggle.isOn = false;
        if (sensitivitySlider != null) sensitivitySlider.value = 1f;

        // Reset Account
        if (playerNameInput != null) playerNameInput.text = "Player";
        
        // Save all default values
        SaveSettings();
        
        // Hide confirmation dialog
        HideConfirmationDialog();
        
        isInitializing = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Remove all listeners
        if (musicSlider != null) musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveAllListeners();
        if (muteToggle != null) muteToggle.onValueChanged.RemoveAllListeners();
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveAllListeners();
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveAllListeners();
        if (vsyncToggle != null) vsyncToggle.onValueChanged.RemoveAllListeners();
        if (brightnessSlider != null) brightnessSlider.onValueChanged.RemoveAllListeners();
        if (vibrationToggle != null) vibrationToggle.onValueChanged.RemoveAllListeners();
        if (tutorialToggle != null) tutorialToggle.onValueChanged.RemoveAllListeners();
        if (difficultyDropdown != null) difficultyDropdown.onValueChanged.RemoveAllListeners();
        if (invertYToggle != null) invertYToggle.onValueChanged.RemoveAllListeners();
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.RemoveAllListeners();
        if (saveButton != null) saveButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        if (playerNameInput != null) playerNameInput.onValueChanged.RemoveAllListeners();
    }
}