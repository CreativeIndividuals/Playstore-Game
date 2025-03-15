using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayTabContent : TabContent
{
    [Header("Play Tab UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text currentLevelText;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text coinsText;
    
    [Header("Stats Display")]
    [SerializeField] private Text totalPlayTimeText;
    [SerializeField] private Text gamesPlayedText;
    [SerializeField] private Text averageScoreText;
    
    [Header("Level Selection")]
    [SerializeField] private Transform levelGrid;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private int totalLevels = 30;
    [SerializeField] private Color lockedLevelColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    [Header("Difficulty")]
    [SerializeField] private ToggleGroup difficultyGroup;
    [SerializeField] private Toggle[] difficultyToggles;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem selectionEffect;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip errorSound;

    private GameManager gameManager;
    private AudioSource audioSource;
    private int selectedLevel = 1;

    protected override void Awake()
    {
        base.Awake();
        gameManager = FindObjectOfType<GameManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
        SetupListeners();
    }

    private void SetupListeners()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        // Setup difficulty toggles
        if (difficultyToggles != null)
        {
            foreach (var toggle in difficultyToggles)
            {
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn) OnDifficultyChanged(toggle);
                });
            }
        }
    }

    protected override IEnumerator LoadContent()
    {
        ShowLoading();

        yield return LoadPlayerStats();
        CreateLevelButtons();
        UpdateUI();
        LoadDifficultySelection();

        HideLoading();
    }

    private IEnumerator LoadPlayerStats()
    {
        yield return new WaitForSeconds(0.5f); // Simulated loading time
        
        int gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
        float totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
        float averageScore = gamesPlayed > 0 ? 
            PlayerPrefs.GetFloat("TotalScore", 0) / gamesPlayed : 0;

        if (gamesPlayedText != null)
            gamesPlayedText.text = $"Games: {gamesPlayed}";
        
        if (totalPlayTimeText != null)
            totalPlayTimeText.text = $"Time: {FormatPlayTime(totalPlayTime)}";
        
        if (averageScoreText != null)
            averageScoreText.text = $"Avg: {averageScore:F0}";
    }

    private void CreateLevelButtons()
    {
        if (levelGrid == null || levelButtonPrefab == null) return;

        // Clear existing buttons
        foreach (Transform child in levelGrid)
        {
            Destroy(child.gameObject);
        }

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Create level buttons
        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelGrid);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            
            // Set level number
            if (buttonText != null)
                buttonText.text = i.ToString();

            // Set locked/unlocked state
            bool isLocked = i > unlockedLevel;
            button.interactable = !isLocked;
            if (buttonImage != null)
                buttonImage.color = isLocked ? lockedLevelColor : Color.white;

            // Add lock icon if locked
            Transform lockIcon = buttonObj.transform.Find("LockIcon");
            if (lockIcon != null)
                lockIcon.gameObject.SetActive(isLocked);

            // Add click listener
            int levelNumber = i; // Capture the level number
            button.onClick.AddListener(() => OnLevelSelected(levelNumber));

            // Show stars earned (if any)
            int stars = PlayerPrefs.GetInt($"Level_{i}_Stars", 0);
            ShowLevelStars(buttonObj, stars);
        }
    }

    private void ShowLevelStars(GameObject buttonObj, int stars)
    {
        Transform starsParent = buttonObj.transform.Find("Stars");
        if (starsParent == null) return;

        for (int i = 0; i < starsParent.childCount; i++)
        {
            Image starImage = starsParent.GetChild(i).GetComponent<Image>();
            if (starImage != null)
            {
                starImage.color = i < stars ? Color.yellow : Color.gray;
            }
        }
    }

    private void UpdateUI()
    {
        // Update high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = $"Best: {highScore:N0}";

        // Update level progress
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        float levelProgress = PlayerPrefs.GetFloat("LevelProgress", 0f);
        
        if (currentLevelText != null)
            currentLevelText.text = $"Level {currentLevel}";
        
        if (progressBar != null)
            progressBar.fillAmount = levelProgress;

        // Update coins
        int coins = PlayerPrefs.GetInt("Coins", 0);
        if (coinsText != null)
            coinsText.text = coins.ToString("N0");
    }

    private void LoadDifficultySelection()
    {
        if (difficultyToggles == null) return;

        int savedDifficulty = PlayerPrefs.GetInt("Difficulty", 1);
        if (savedDifficulty < difficultyToggles.Length)
        {
            difficultyToggles[savedDifficulty].isOn = true;
        }
    }

    private string FormatPlayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        
        if (hours > 0)
            return $"{hours}h {minutes}m";
        else
            return $"{minutes}m";
    }

    private void OnPlayButtonClicked()
    {
        if (gameManager != null)
        {
            PlaySound(selectSound);
            gameManager.StartLevel(selectedLevel);
        }
        else
        {
            HandleError("GameManager not found!");
        }
    }

    private void OnLevelSelected(int level)
    {
        selectedLevel = level;
        PlaySound(selectSound);
        
        if (selectionEffect != null)
            selectionEffect.Play();

        if (gameManager != null)
            gameManager.PreviewLevel(level);
    }

    private void OnDifficultyChanged(Toggle toggle)
    {
        if (gameManager == null) return;

        int difficultyIndex = System.Array.IndexOf(difficultyToggles, toggle);
        PlayerPrefs.SetInt("Difficulty", difficultyIndex);
        
        PlaySound(selectSound);
        gameManager.SetDifficulty(difficultyIndex);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    protected override IEnumerator RefreshContent()
    {
        yield return LoadPlayerStats();
        UpdateUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
            
        if (difficultyToggles != null)
        {
            foreach (var toggle in difficultyToggles)
            {
                if (toggle != null)
                    toggle.onValueChanged.RemoveAllListeners();
            }
        }

        // Clean up level button listeners
        if (levelGrid != null)
        {
            foreach (Transform child in levelGrid)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }
        }
    }
}