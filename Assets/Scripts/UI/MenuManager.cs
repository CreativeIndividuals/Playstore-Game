using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI finalScoreText;
    
    [Header("Tab Navigation")]
    public Button[] menuTabs;
    public GameObject[] menuPages;

    private float survivalTime;
    private bool isTracking;

    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged += HandleGameStateChanged;
            GameManager.instance.OnScoreChanged += UpdateScore;
        }

        ShowMainMenu();
        UpdateHighScore();
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.instance.OnScoreChanged -= UpdateScore;
        }
    }

    private void Update()
    {
        if (isTracking)
        {
            survivalTime += Time.deltaTime;
            UpdateSurvivalTimeDisplay();
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        
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
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        isTracking = false;
        
        finalScoreText.text = $"Final Score: {GameManager.instance.currentScore}\nSurvival Time: {GetFormattedTime(survivalTime)}";
        UpdateHighScore();
    }

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
                gameplayPanel.SetActive(true);
                break;
            case GameManager.GameState.Paused:
                PauseGame();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
        }
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateHighScore()
    {
        highScoreText.text = $"High Score: {GameManager.instance.highScore}";
    }

    private void UpdateSurvivalTimeDisplay()
    {
        survivalTimeText.text = $"Time: {GetFormattedTime(survivalTime)}";
    }

    private string GetFormattedTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return