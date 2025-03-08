using UnityEngine;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    
    [Header("Ad Configuration")]
    public float minTimeBetweenAds = 180f; // 3 minutes
    public int gamesUntilFirstAd = 3;
    public bool showBannerAds = true;
    
    private float lastAdTime;
    private int gamesPlayed;
    private bool isInitialized;

    public event Action OnAdStarted;
    public event Action OnAdCompleted;
    public event Action OnAdFailed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        lastAdTime = -minTimeBetweenAds; // Allow first ad immediately after minimum games
        gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
        isInitialized = true;
        
        // TODO: Initialize your ad SDK here
        // Example: AdMob initialization
    }

    public void ShowInterstitialAd()
    {
        if (!isInitialized) return;
        
        if (gamesPlayed < gamesUntilFirstAd)
        {
            OnAdCompleted?.Invoke();
            return;
        }

        if (Time.time - lastAdTime < minTimeBetweenAds)
        {
            OnAdCompleted?.Invoke();
            return;
        }

        // TODO: Show interstitial ad through your ad SDK
        // For now, we'll simulate it
        SimulateAd();
    }

    private void SimulateAd()
    {
        OnAdStarted?.Invoke();
        lastAdTime = Time.time;
        
        // Simulate ad completion after 1 second
        Invoke(nameof(CompleteAd), 1f);
    }

    private void CompleteAd()
    {
        OnAdCompleted?.Invoke();
    }

    public void IncrementGamesPlayed()
    {
        gamesPlayed++;
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed);
        PlayerPrefs.Save();
    }
}