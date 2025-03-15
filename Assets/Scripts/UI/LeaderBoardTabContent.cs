using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LeaderboardTabContent : TabContent
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public int rank;
    }

    [Header("Leaderboard UI")]
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Text playerRankText;
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Toggle[] filterToggles; // Daily, Weekly, All-time
    [SerializeField] private Text noEntriesText;

    private List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    private LeaderboardEntry playerEntry;
    private bool isRefreshing;

    protected override void Awake()
    {
        base.Awake();
        
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClicked);

        if (filterToggles != null)
        {
            foreach (var toggle in filterToggles)
            {
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn) OnFilterChanged(toggle);
                });
            }
        }
    }

    protected override IEnumerator LoadContent()
    {
        yield return FetchLeaderboardData();
        PopulateLeaderboard();
    }

    private IEnumerator FetchLeaderboardData()
    {
        isRefreshing = true;
        
        if (refreshButton != null)
            refreshButton.interactable = false;

        // Clear existing entries
        entries.Clear();

        // Simulate network delay
        yield return new WaitForSeconds(1f);

        // TODO: Replace with actual leaderboard data fetching
        // For now, generate sample data
        GenerateSampleData();

        if (refreshButton != null)
            refreshButton.interactable = true;

        isRefreshing = false;
    }

    private void GenerateSampleData()
    {
        // Generate sample leaderboard entries
        for (int i = 0; i < 20; i++)
        {
            entries.Add(new LeaderboardEntry
            {
                playerName = $"Player_{i}",
                score = Random.Range(1000, 10000),
                rank = i + 1
            });
        }

        // Sort by score
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Update ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].rank = i + 1;
        }

        // Generate player entry
        playerEntry = new LeaderboardEntry
        {
            playerName = PlayerPrefs.GetString("PlayerName", "Player"),
            score = PlayerPrefs.GetInt("HighScore", 0),
            rank = Random.Range(1, 100) // Replace with actual rank calculation
        };
    }

    private void PopulateLeaderboard()
    {
        // Clear existing entries
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }

        if (entries.Count == 0)
        {
            if (noEntriesText != null)
            {
                noEntriesText.gameObject.SetActive(true);
            }
            return;
        }

        if (noEntriesText != null)
        {
            noEntriesText.gameObject.SetActive(false);
        }

        // Create entries
        foreach (var entry in entries)
        {
            GameObject entryObj = Instantiate(
                entry.playerName == playerEntry.playerName ? playerEntryPrefab : entryPrefab, 
                entriesContainer
            );

            SetupEntryUI(entryObj, entry);
        }

        // Update player stats
        if (playerRankText != null)
            playerRankText.text = $"Rank: #{playerEntry.rank}";
        
        if (playerScoreText != null)
            playerScoreText.text = $"Score: {playerEntry.score:N0}";
    }

    private void SetupEntryUI(GameObject entryObj, LeaderboardEntry entry)
    {
        // Find UI components
        Text rankText = entryObj.transform.Find("RankText")?.GetComponent<Text>();
        Text nameText = entryObj.transform.Find("NameText")?.GetComponent<Text>();
        Text scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<Text>();

        // Set values
        if (rankText != null)
            rankText.text = $"#{entry.rank}";
        
        if (nameText != null)
            nameText.text = entry.playerName;
        
        if (scoreText != null)
            scoreText.text = entry.score.ToString("N0");
    }

    private void OnRefreshClicked()
    {
        if (!isRefreshing)
        {
            StartCoroutine(RefreshContent());
        }
    }

    private void OnFilterChanged(Toggle toggle)
    {
        if (isRefreshing) return;
        
        int filterIndex = System.Array.IndexOf(filterToggles, toggle);
        StartCoroutine(RefreshContent());
    }

    protected override IEnumerator RefreshContent()
    {
        yield return FetchLeaderboardData();
        PopulateLeaderboard();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (refreshButton != null)
            refreshButton.onClick.RemoveAllListeners();

        if (filterToggles != null)
        {
            foreach (var toggle in filterToggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }
}