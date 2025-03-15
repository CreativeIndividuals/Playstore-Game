using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class MenuSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI userLoginText;
    public TextMeshProUGUI sessionTimeText;
    public Button playButton;
    public Button settingsButton;
    public Button shopButton;
    public Button charactersButton;

    private void Start()
    {
        InitializeUI();
        SetupButtons();
    }

    private void InitializeUI()
    {
        userLoginText.text = $"User: {GameDataManager.instance.currentUserLogin}";
        InvokeRepeating(nameof(UpdateSessionTime), 0f, 1f);
    }

    private void UpdateSessionTime()
    {
        TimeSpan sessionTime = GameDataManager.instance.GetSessionDuration();
        sessionTimeText.text = $"Session: {sessionTime.Hours:D2}:{sessionTime.Minutes:D2}:{sessionTime.Seconds:D2}";
    }

    private void SetupButtons()
    {
        playButton.onClick.AddListener(() => SceneController.instance.LoadScene(SceneController.SceneType.Game));
        settingsButton.onClick.AddListener(OpenSettings);
        shopButton.onClick.AddListener(OpenShop);
        charactersButton.onClick.AddListener(OpenCharacters);
    }

    private void OpenSettings() => ShowTab("Settings");
    private void OpenShop() => ShowTab("Shop");
    private void OpenCharacters() => ShowTab("Characters");

    private void ShowTab(string tabName)
    {
        // Implement tab switching logic
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateSessionTime));
    }
}