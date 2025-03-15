using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingCoins = 100;
    [SerializeField] private float levelCompleteThreshold = 0.8f;
    [SerializeField] private int coinsPerStar = 50;
    
    [Header("Level Management")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private int maxLevel = 30;
    [SerializeField] private float[] difficultyMultipliers = { 0.8f, 1f, 1.2f, 1.5f };
    
    [Header("Audio")]
    [SerializeField] private AudioClip levelStartSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    
    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private UnityEngine.UI.Image loadingBar;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private int currentLevel = 1;
    private int currentDifficulty = 1; // 0=Easy, 1=Normal, 2=Hard, 3=Expert
    private bool isGamePaused = false;
    private float gameStartTime;
    private AudioSource audioSource;

    // Game state
    private int totalScore = 0;
    private int currentLevelScore = 0;
    private int coins;
    private bool isGameActive = false;
    public static GameManager instance { get; set; }
    public object currentState { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        audioSource = gameObject.AddComponent<AudioSource>();
        LoadPlayerData();
        
        // Initialize UI elements
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    private void LoadPlayerData()
    {
        coins = PlayerPrefs.GetInt("Coins", startingCoins);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        currentDifficulty = PlayerPrefs.GetInt("Difficulty", 1);
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
    }

    public void StartLevel(int level)
    {
        if (level < 1 || level > maxLevel) return;
        
        currentLevel = level;
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Save current state
        SavePlayerData();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (loadingBar != null)
                loadingBar.fillAmount = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f)
            {
                // Wait for any loading screen animations
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Initialize level
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        isGameActive = true;
        gameStartTime = Time.time;
        currentLevelScore = 0;
        
        // Play start sound
        PlaySound(levelStartSound);

        // Hide UI elements
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        // Apply difficulty settings
        ApplyDifficultySettings();
    }

    private void ApplyDifficultySettings()
    {
        if (currentDifficulty < 0 || currentDifficulty >= difficultyMultipliers.Length)
            currentDifficulty = 1;

        float multiplier = difficultyMultipliers[currentDifficulty];
        // Apply difficulty multiplier to game parameters
        // This will depend on your specific game mechanics
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentLevelScore += points;
        totalScore += points;

        // Check for level completion
        if (currentLevelScore >= GetLevelTarget())
        {
            CompleteLevelWithStars(CalculateStars());
        }
    }

    private int GetLevelTarget()
    {
        // Calculate target score based on level and difficulty
        return 1000 * currentLevel * (int)Mathf.Ceil(difficultyMultipliers[currentDifficulty]);
    }

    private int CalculateStars()
    {
        float completion = (float)currentLevelScore / GetLevelTarget();
        if (completion >= levelCompleteThreshold * 1.5f) return 3;
        if (completion >= levelCompleteThreshold * 1.25f) return 2;
        if (completion >= levelCompleteThreshold) return 1;
        return 0;
    }

    private void CompleteLevelWithStars(int stars)
    {
        isGameActive = false;
        PlaySound(levelCompleteSound);

        // Save level progress
        PlayerPrefs.SetInt($"Level_{currentLevel}_Stars", Mathf.Max(stars, 
            PlayerPrefs.GetInt($"Level_{currentLevel}_Stars", 0)));
        
        // Award coins based on stars
        AddCoins(stars * coinsPerStar);

        // Unlock next level if this was the highest level completed
        if (currentLevel >= PlayerPrefs.GetInt("UnlockedLevel", 1))
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
        }

        SavePlayerData();
        ShowLevelComplete(stars);
    }

    public void GameOver()
    {
        if (!isGameActive) return;

        isGameActive = false;
        PlaySound(gameOverSound);
        
        // Update stats
        int gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0) + 1;
        float totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0) + (Time.time - gameStartTime);
        
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed);
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
        
        SavePlayerData();
        ShowGameOver();
    }

    public void PauseGame()
    {
        if (!isGameActive) return;

        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0 : 1;
        
        if (pauseMenu != null)
            pauseMenu.SetActive(isGamePaused);
    }

    public void SetDifficulty(int difficulty)
    {
        if (difficulty >= 0 && difficulty < difficultyMultipliers.Length)
        {
            currentDifficulty = difficulty;
            PlayerPrefs.SetInt("Difficulty", difficulty);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            PlayerPrefs.SetInt("Coins", coins);
            return true;
        }
        return false;
    }

    public void PreviewLevel(int level)
    {
        // Implement level preview functionality
        // This could show a thumbnail, description, or requirements
    }

    private void ShowLevelComplete(int stars)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            // Update UI elements with stars and score
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Update UI elements with final score
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePlayerData();
        }
    }
}