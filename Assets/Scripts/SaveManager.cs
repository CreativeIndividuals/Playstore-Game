using UnityEngine;
using System;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [System.Serializable]
    public class GameProgress
    {
        public float highScore;
        public int totalGamesPlayed;
        public int coinsCollected;
        public Dictionary<string, bool> achievements;
        public Dictionary<string, float> statistics;
        public string lastPlayedDate;
    }

    private GameProgress progress;
    private const string SAVE_KEY = "GameProgress";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadProgress()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        
        if (string.IsNullOrEmpty(json))
        {
            InitializeNewProgress();
        }
        else
        {
            try
            {
                progress = JsonUtility.FromJson<GameProgress>(json);
            }
            catch
            {
                InitializeNewProgress();
            }
        }
    }

    private void InitializeNewProgress()
    {
        progress = new GameProgress
        {
            highScore = 0,
            totalGamesPlayed = 0,
            coinsCollected = 0,
            achievements = new Dictionary<string, bool>(),
            statistics = new Dictionary<string, float>(),
            lastPlayedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };
        SaveProgress();
    }

    public void SaveProgress()
    {
        progress.lastPlayedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        string json = JsonUtility.ToJson(progress);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void UpdateHighScore(float newScore)
    {
        if (newScore > progress.highScore)
        {
            progress.highScore = newScore;
            SaveProgress();
        }
    }

    public void IncrementGamesPlayed()
    {
        progress.totalGamesPlayed++;
        SaveProgress();
    }

    public void AddCoins(int amount)
    {
        progress.coinsCollected += amount;
        SaveProgress();
    }

    public void UpdateStatistic(string statName, float value)
    {
        if (!progress.statistics.ContainsKey(statName))
        {
            progress.statistics[statName] = 0;
        }
        progress.statistics[statName] = value;
        SaveProgress();
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!progress.achievements.ContainsKey(achievementId))
        {
            progress.achievements[achievementId] = true;
            SaveProgress();
        }
    }

    public GameProgress GetProgress()
    {
        return progress;
    }
}