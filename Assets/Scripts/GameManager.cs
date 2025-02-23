using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Session Info")]
    public string currentUserLogin = "QAD6675";
    public string sessionStartTime = "2025-02-23 13:30:41";
    
    [Header("Game State")]
    public GameState currentState;
    public bool isGameActive;
    public int currentScore;
    public int highScore;
    
    [Header("Mobile Settings")]
    public float doubleTapTime = 0.3f;
    public float minimumSwipeDistance = 50f;
    public int targetFrameRate = 60;

    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnScoreChanged;
    
    // Mobile input tracking
    private float _lastTapTime;
    private Vector2 _touchStartPosition;
    private float _lastPauseTime;
    private const float MinimumPauseInterval = 0.5f;

    public enum GameState 
    { 
        Menu, 
        Playing, 
        Paused, 
        GameOver 
    }

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

    private void Start()
    {
        // Configure mobile settings
        Application.targetFrameRate = targetFrameRate;
        Input.multiTouchEnabled = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void InitializeGame()
    {
        // Load user-specific high score
        highScore = PlayerPrefs.GetInt($"HighScore_{currentUserLogin}", 0);
        currentState = GameState.Menu;
        Time.timeScale = 1f;

        Debug.Log($"Game initialized - User: {currentUserLogin}, Session: {sessionStartTime}");
    }

    private void Update()
    {
        if (!isGameActive) return;

        HandleMobileInput();
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Ended:
                    HandleTouchEnded(touch);
                    break;
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        _touchStartPosition = touch.position;

        float timeSinceLastTap = Time.time - _lastTapTime;
        if (timeSinceLastTap <= doubleTapTime)
        {
            TryTogglePause();
        }
        _lastTapTime = Time.time;
    }

    private void HandleTouchEnded(Touch touch)
    {
        Vector2 swipeDelta = touch.position - _touchStartPosition;
        
        if (swipeDelta.magnitude >= minimumSwipeDistance)
        {
            if (swipeDelta.y < -minimumSwipeDistance && currentState == GameState.Playing)
            {
                TryTogglePause();
            }
        }
    }

    private void TryTogglePause()
    {
        float timeSinceLastPause = Time.time - _lastPauseTime;
        if (timeSinceLastPause < MinimumPauseInterval) return;

        if (currentState == GameState.Playing)
            PauseGame();
        else if (currentState == GameState.Paused)
            ResumeGame();

        _lastPauseTime = Time.time;
    }

    public void StartGame()
    {
        isGameActive = true;
        currentScore = 0;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        Debug.Log($"Game started by {currentUserLogin} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        
        OnGameStateChanged?.Invoke(currentState);
        OnScoreChanged?.Invoke(currentScore);
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            Time.timeScale = 0f;
            currentState = GameState.Paused;
            OnGameStateChanged?.Invoke(currentState);
            
            Debug.Log($"Game paused - User: {currentUserLogin}, Score: {currentScore}");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            currentState = GameState.Playing;
            OnGameStateChanged?.Invoke(currentState);
            
            Debug.Log($"Game resumed - User: {currentUserLogin}, Score: {currentScore}");
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt($"HighScore_{currentUserLogin}", highScore);
            Debug.Log($"New high score! User: {currentUserLogin}, Score: {highScore}");
        }
        
        Debug.Log($"Game over - User: {currentUserLogin}, Final Score: {currentScore}");
        OnGameStateChanged?.Invoke(currentState);
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;
        
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        if (currentScore % 100 == 0)
        {
            Debug.Log($"Score milestone - User: {currentUserLogin}, Score: {currentScore}");
        }
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
        Debug.Log($"Session ended - User: {currentUserLogin}, Final Score: {currentScore}");
        PlayerPrefs.Save();
    }
}