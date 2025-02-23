using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GameSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI userLoginText;
    public TextMeshProUGUI sessionTimeText;
    public TextMeshProUGUI scoreText;
    public Button pauseButton;
    public Button returnToMenuButton;

    [Header("Game Elements")]
    public PlayerController player;
    public EnemySpawner enemySpawner;
    public BackgroundController background;

    private void Start()
    {
        InitializeUI();
        SetupButtons();
        StartGame();
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
        pauseButton.onClick.AddListener(PauseGame);
        returnToMenuButton.onClick.AddListener(() => SceneController.instance.LoadScene(SceneController.SceneType.Menu));
    }

    private void StartGame()
    {
        GameManager.instance.StartGame();
    }

    private void PauseGame()
    {
        GameManager.instance.PauseGame();
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateSessionTime));
    }
}