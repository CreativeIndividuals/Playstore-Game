using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsTabContent : TabContent
{
    // Delegates for callbacks
    public delegate void AudioSettingChanged(float value);
    public delegate void ToggleSettingChanged(bool value);
    public delegate void QualitySettingChanged(int value);
    public delegate void AuthenticationAction();

    // Events
    public event AudioSettingChanged OnMusicVolumeChanged;
    public event AudioSettingChanged OnSfxVolumeChanged;
    public event ToggleSettingChanged OnMusicMuteChanged;
    public event ToggleSettingChanged OnSfxMuteChanged;
    public event ToggleSettingChanged OnVibrationChanged;
    public event ToggleSettingChanged OnPowerSavingChanged;
    public event QualitySettingChanged OnQualityChanged;
    public event AuthenticationAction OnSignInRequested;
    public event AuthenticationAction OnSignOutRequested;

    [Header("Audio Settings")]
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Toggle _muteMusicToggle;
    [SerializeField] private Toggle _muteSfxToggle;
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private Toggle _vibrationToggle;
    [SerializeField] private Toggle _powerSavingToggle;

    [Header("Account Settings")]
    [SerializeField] private Button _signInButton;
    [SerializeField] private Button _privacyPolicyButton;
    [SerializeField] private Button _termsButton;
    [SerializeField] private TextMeshProUGUI _versionText;

    private void Start()
    {
        InitializeSettings();
        SetupListeners();
    }

    private void InitializeSettings()
    {
        // Load saved settings
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        _muteMusicToggle.isOn = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        _muteSfxToggle.isOn = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        _vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        _powerSavingToggle.isOn = PlayerPrefs.GetInt("PowerSaving", 0) == 1;

        // Setup quality dropdown
        _qualityDropdown.ClearOptions();
        _qualityDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "Low", "Medium", "High"
        });
        _qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 1);

        // Set version
        _versionText.text = $"Version {Application.version}";
        
        // Update sign in button text
        UpdateSignInButtonText();
    }

    private void SetupListeners()
    {
        _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged_Internal);
        _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged_Internal);
        _muteMusicToggle.onValueChanged.AddListener(OnMuteMusicChanged_Internal);
        _muteSfxToggle.onValueChanged.AddListener(OnMuteSFXChanged_Internal);
        _qualityDropdown.onValueChanged.AddListener(OnQualityChanged_Internal);
        _vibrationToggle.onValueChanged.AddListener(OnVibrationChanged_Internal);
        _powerSavingToggle.onValueChanged.AddListener(OnPowerSavingChanged_Internal);
        
        _signInButton.onClick.AddListener(OnSignInButtonClicked);
        _privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyPressed);
        _termsButton.onClick.AddListener(OnTermsPressed);
    }

    public override void RefreshContent()
    {
        UpdateSignInButtonText();
    }

    private void UpdateSignInButtonText()
    {
        bool isSignedIn = PlayerPrefs.GetInt("IsSignedIn", 0) == 1;
        _signInButton.GetComponentInChildren<TextMeshProUGUI>().text = 
            isSignedIn ? "Sign Out" : "Sign In";
    }

    #region Callback Implementations
    private void OnMusicVolumeChanged_Internal(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        }
        OnMusicVolumeChanged?.Invoke(value);
    }

    private void OnSFXVolumeChanged_Internal(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        }
        OnSfxVolumeChanged?.Invoke(value);
    }

    private void OnMuteMusicChanged_Internal(bool value)
    {
        PlayerPrefs.SetInt("MusicMuted", value ? 1 : 0);
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MusicVolume", value ? -80f : Mathf.Log10(_musicVolumeSlider.value) * 20);
        }
        OnMusicMuteChanged?.Invoke(value);
    }

    private void OnMuteSFXChanged_Internal(bool value)
    {
        PlayerPrefs.SetInt("SFXMuted", value ? 1 : 0);
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("SFXVolume", value ? -80f : Mathf.Log10(_sfxVolumeSlider.value) * 20);
        }
        OnSfxMuteChanged?.Invoke(value);
    }

    private void OnQualityChanged_Internal(int value)
    {
        PlayerPrefs.SetInt("QualityLevel", value);
        QualitySettings.SetQualityLevel(value, true);
        OnQualityChanged?.Invoke(value);
    }

    private void OnVibrationChanged_Internal(bool value)
    {
        PlayerPrefs.SetInt("Vibration", value ? 1 : 0);
        OnVibrationChanged?.Invoke(value);
    }

    private void OnPowerSavingChanged_Internal(bool value)
    {
        PlayerPrefs.SetInt("PowerSaving", value ? 1 : 0);
        Application.targetFrameRate = value ? 30 : 60;
        OnPowerSavingChanged?.Invoke(value);
    }

    private void OnSignInButtonClicked()
    {
        bool isSignedIn = PlayerPrefs.GetInt("IsSignedIn", 0) == 1;
        if (isSignedIn)
        {
            OnSignOutRequested?.Invoke();
        }
        else
        {
            OnSignInRequested?.Invoke();
        }
    }

    private void OnPrivacyPolicyPressed()
    {
        Application.OpenURL("https://your-privacy-policy-url.com");
    }

    private void OnTermsPressed()
    {
        Application.OpenURL("https://your-terms-url.com");
    }
    #endregion

    private void OnDestroy()
    {
        // Clean up listeners
        if (_musicVolumeSlider != null) _musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (_muteMusicToggle != null) _muteMusicToggle.onValueChanged.RemoveAllListeners();
        if (_muteSfxToggle != null) _muteSfxToggle.onValueChanged.RemoveAllListeners();
        if (_qualityDropdown != null) _qualityDropdown.onValueChanged.RemoveAllListeners();
        if (_vibrationToggle != null) _vibrationToggle.onValueChanged.RemoveAllListeners();
        if (_powerSavingToggle != null) _powerSavingToggle.onValueChanged.RemoveAllListeners();
        if (_signInButton != null) _signInButton.onClick.RemoveAllListeners();
        if (_privacyPolicyButton != null) _privacyPolicyButton.onClick.RemoveAllListeners();
        if (_termsButton != null) _termsButton.onClick.RemoveAllListeners();
    }
}