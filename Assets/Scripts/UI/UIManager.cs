using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI newHighScoreText;
    public TextMeshProUGUI userInfoText;
    public TextMeshProUGUI sessionTimeText;

    [Header("Buttons")]
    public Button startButton;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;

    private void Start()
    {
        SetupButtons();
        UpdateHighScore();
        UpdateUserInfo();
        ShowMainMenu();

        GameManager.instance.OnGameStateChanged += HandleGameStateChanged;
        GameManager.instance.OnScoreChanged += UpdateScore;

        // Start session time update
        InvokeRepeating(nameof(UpdateSessionTime), 0f, 1f);
    }

    private void SetupButtons()
    {
        startButton.onClick.AddListener(() => GameManager.instance.StartGame());
        resumeButton.onClick.AddListener(() => GameManager.instance.ResumeGame());
        restartButton.onClick.AddListener(() => GameManager.instance.StartGame());
        menuButton.onClick.AddListener(ShowMainMenu);
    }

    private void UpdateUserInfo()
    {
        string userLogin = GameManager.instance.currentUserLogin;
        userInfoText.text = $"User: {userLogin}";
    }

    private void UpdateSessionTime()
    {
        DateTime sessionStart = DateTime.Parse(GameManager.instance.sessionStartTime);
        TimeSpan elapsed = DateTime.UtcNow - sessionStart;
        sessionTimeText.text = $"Session Time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        mainMenuPanel.SetActive(newState == GameManager.GameState.Menu);
        gamePanel.SetActive(newState == GameManager.GameState.Playing);
        pausePanel.SetActive(newState == GameManager.GameState.Paused);
        gameOverPanel.SetActive(newState == GameManager.GameState.GameOver);

        if (newState == GameManager.GameState.GameOver)
        {
            ShowGameOver();
        }
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateHighScore()
    {
        string userLogin = GameManager.instance.currentUserLogin;
        highScoreText.text = $"High Score ({userLogin}): {GameManager.instance.highScore}";
    }

    private void ShowMainMenu()
    {
        GameManager.instance.currentState = GameManager.GameState.Menu;
        HandleGameStateChanged(GameManager.GameState.Menu);
    }

    private void ShowGameOver()
    {
        finalScoreText.text = $"Final Score: {GameManager.instance.currentScore}";
        bool isNewHighScore = GameManager.instance.currentScore > GameManager.instance.highScore;
        newHighScoreText.gameObject.SetActive(isNewHighScore);
        UpdateHighScore();
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.instance.OnScoreChanged -= UpdateScore;
        }
        CancelInvoke(nameof(UpdateSessionTime));
    }
}