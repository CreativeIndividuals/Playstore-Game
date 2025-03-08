using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    // Add the GameState enum definition
    public enum GameState 
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    [Header("Session Info")]
    public string currentUserLogin = "QAD6675";
    public string sessionStartTime;
    
    [Header("Game State")]
    public GameState currentState;
    public bool isGameActive;
    public float survivalTime;
    public int currentScore;
    public float highScore;
    
    [Header("Game Settings")]
    public float difficultyMultiplier = 1f;
    public bool powerSavingMode;
    public bool soundEnabled = true;
    
    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<float> OnSurvivalTimeUpdated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        sessionStartTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        LoadSettings();
        ResetGameState();
    }

    private void LoadSettings()
    {
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        powerSavingMode = PlayerPrefs.GetInt("PowerSaving", 0) == 1;
        difficultyMultiplier = PlayerPrefs.GetFloat("Difficulty", 1f);
    }

    private void Update()
    {
        if (isGameActive)
        {
            survivalTime += Time.deltaTime;
            OnSurvivalTimeUpdated?.Invoke(survivalTime);
        }
    }

    public void StartGame()
    {
        ResetGameState();
        isGameActive = true;
        currentState = GameState.Playing;
        OnGameStateChanged?.Invoke(currentState);
        
        // Track game start
        SaveManager.Instance.IncrementGamesPlayed();
        AdManager.Instance.IncrementGamesPlayed();
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            Time.timeScale = 0f;
            currentState = GameState.Paused;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            currentState = GameState.Playing;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        currentState = GameState.GameOver;
        Time.timeScale = 1f;
        
        // Update high score if needed
        if (survivalTime > highScore)
        {
            highScore = survivalTime;
            SaveManager.Instance.UpdateHighScore(highScore);
        }
        
        // Show ad if conditions are met
        AdManager.Instance.ShowInterstitialAd();
        
        OnGameStateChanged?.Invoke(currentState);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void ResetGameState()
    {
        currentScore = 0;
        survivalTime = 0f;
        isGameActive = false;
        Time.timeScale = 1f;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentState == GameState.Playing)
        {
            PauseGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveManager.Instance.SaveProgress();
    }
}