using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    
    [Header("Game Info")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI newHighScoreText;
    
    [Header("Session Info")]
    public TextMeshProUGUI sessionTimeText;
    public TextMeshProUGUI userLoginText;
    
    [Header("Navigation")]
    public Button[] menuTabs;
    public GameObject[] menuPages;
    
    [Header("Game Controls")]
    public Button startButton;
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button quitButton;

    private float survivalTime;
    private bool isTracking;
    private DateTime sessionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUI()
    {
        sessionStartTime = DateTime.Parse(GameManager.instance.sessionStartTime);
        SetupButtons();
        UpdateUserInfo();
        InvokeRepeating(nameof(UpdateSessionTime), 0f, 1f);
    }

    private void Start()
    {
        GameManager.instance.OnGameStateChanged += HandleGameStateChanged;
        GameManager.instance.OnScoreChanged += UpdateScore;
        ShowMainMenu();
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

    private void Update()
    {
        if (isTracking)
        {
            survivalTime += Time.deltaTime;
            UpdateSurvivalTimeDisplay();
        }
    }

    private void SetupButtons()
    {
        startButton?.onClick.AddListener(StartGame);
        resumeButton?.onClick.AddListener(ResumeGame);
        restartButton?.onClick.AddListener(RestartGame);
        settingsButton?.onClick.AddListener(ShowSettings);
        quitButton?.onClick.AddListener(QuitGame);
        
        // Setup menu tabs
        for (int i = 0; i < menuTabs.Length; i++)
        {
            int index = i;
            menuTabs[i]?.onClick.AddListener(() => SwitchTab(index));
        }
    }

    #region Panel Management
    public void ShowMainMenu()
    {
        SetPanelStates(true, false, false, false, false);
    }

    public void StartGame()
    {
        SetPanelStates(false, true, false, false, false);
        survivalTime = 0f;
        isTracking = true;
        GameManager.instance.StartGame();
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        isTracking = false;
        GameManager.instance.PauseGame();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        isTracking = true;
        GameManager.instance.ResumeGame();
    }

    public void ShowGameOver()
    {
        SetPanelStates(false, false, false, true, false);
        isTracking = false;
        UpdateFinalScore();
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void SetPanelStates(bool main, bool game, bool pause, bool over, bool settings)
    {
        mainMenuPanel.SetActive(main);
        gamePanel.SetActive(game);
        pausePanel.SetActive(pause);
        gameOverPanel.SetActive(over);
        settingsPanel.SetActive(settings);
    }
    #endregion

    #region UI Updates
    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateHighScore()
    {
        string userLogin = GameManager.instance.currentUserLogin;
        highScoreText.text = $"High Score ({userLogin}): {GameManager.instance.highScore}";
    }

    private void UpdateFinalScore()
    {
        finalScoreText.text = $"Final Score: {GameManager.instance.currentScore}\nSurvival Time: {GetFormattedTime(survivalTime)}";
        bool isNewHighScore = GameManager.instance.currentScore > GameManager.instance.highScore;
        newHighScoreText.gameObject.SetActive(isNewHighScore);
        UpdateHighScore();
    }

    private void UpdateSurvivalTimeDisplay()
    {
        survivalTimeText.text = $"Time: {GetFormattedTime(survivalTime)}";
    }

    private void UpdateSessionTime()
    {
        TimeSpan elapsed = DateTime.UtcNow - sessionStartTime;
        sessionTimeText.text = $"Session Time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    private void UpdateUserInfo()
    {
        userLoginText.text = $"User: {GameManager.instance.currentUserLogin}";
    }
    #endregion

    #region Navigation
    public void SwitchTab(int tabIndex)
    {
        for (int i = 0; i < menuPages.Length; i++)
        {
            menuPages[i].SetActive(i == tabIndex);
            if (menuTabs[i] != null)
            {
                menuTabs[i].interactable = i != tabIndex;
            }
        }
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.Menu:
                ShowMainMenu();
                break;
            case GameManager.GameState.Playing:
                gamePanel.SetActive(true);
                break;
            case GameManager.GameState.Paused:
                PauseGame();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
        }
    }
    #endregion

    #region Utility
    private string GetFormattedTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    public void RestartGame()
    {
        GameManager.instance.StartGame();
        StartGame();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion
}