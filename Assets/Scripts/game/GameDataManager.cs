using UnityEngine;
using System;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance { get; private set; }

    public string currentUserLogin { get; private set; } = "QAD6675";
    private DateTime sessionStartTime { get; set; }
    private string formattedSessionStartTime => sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss");

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSession()
    {
        sessionStartTime = DateTime.Parse("2025-02-23 12:47:21");
        Debug.Log($"Session initialized - User: {currentUserLogin}, Time: {formattedSessionStartTime}");
    }

    public string GetCurrentUtcTime()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public TimeSpan GetSessionDuration()
    {
        return DateTime.UtcNow - sessionStartTime;
    }
}