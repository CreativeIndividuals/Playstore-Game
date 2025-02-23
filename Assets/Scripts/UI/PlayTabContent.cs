using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayTabContent : TabContent
{
    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public TextMeshProUGUI levelNumber;
        public Image starRating;
        public Image lockIcon;
    }

    [FormerlySerializedAs("levelButtons")]
    [Header("UI References")]
    [SerializeField] private LevelButton[] _levelButtons;
    [FormerlySerializedAs("playerScoreText")] [SerializeField] private TextMeshProUGUI _playerScoreText;
    [FormerlySerializedAs("playerRankText")] [SerializeField] private TextMeshProUGUI _playerRankText;
    [FormerlySerializedAs("quickPlayButton")] [SerializeField] private Button _quickPlayButton;
    [FormerlySerializedAs("continueButton")] [SerializeField] private Button _continueButton;

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        _quickPlayButton.onClick.AddListener(OnQuickPlayPressed);
        _continueButton.onClick.AddListener(OnContinuePressed);

        for (int i = 0; i < _levelButtons.Length; i++)
        {
            int levelIndex = i;
            _levelButtons[i].button.onClick.AddListener(() => OnLevelSelected(levelIndex));
        }
    }

    private void OnQuickPlayPressed()
    {
        //TODO:implement quick play functionality
    }

    public override void RefreshContent()
    {
        UpdatePlayerStats();
        UpdateLevelButtons();
        UpdateContinueButton();
    }

    private void UpdatePlayerStats()
    {
        // TODO: Implement with your game's progression system
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        int playerRank = PlayerPrefs.GetInt("PlayerRank", 1);
        
        _playerScoreText.text = $"Score: {totalScore:N0}";
        _playerRankText.text = $"Rank {playerRank}";
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            bool isUnlocked = PlayerPrefs.GetInt($"Level_{levelNum}_Unlocked", 0) == 1;
            _levelButtons[i].levelNumber.text = levelNum.ToString();
            _levelButtons[i].lockIcon.gameObject.SetActive(!isUnlocked);
            _levelButtons[i].button.interactable = isUnlocked;
            
            // Update star rating (implement based on your star sprite system)
            // levelButtons[i].starRating.sprite = GetStarSprite(stars);
        }
    }

    private void UpdateContinueButton()
    {
        int lastPlayedLevel = PlayerPrefs.GetInt("LastPlayedLevel", 1);
        _continueButton.gameObject.SetActive(lastPlayedLevel > 1);
    }

    private void OnLevelSelected(int levelIndex)
    {
        // TODO: Implement level loading
        Debug.Log($"Loading level {levelIndex + 1}");
    }
    private void OnContinuePressed()
    {
        int lastLevel = PlayerPrefs.GetInt("LastPlayedLevel", 1);
        // TODO: Implement continue functionality
        Debug.Log($"Continuing from level {lastLevel}");
    }
}