using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [Header("App Configuration")]
    public string appVersion = "1.0.0";
    public bool enableAnalytics = true;
    public bool enableAds = true;
    
    [Header("Performance Settings")]
    public int targetFrameRate = 60;
    public bool enableVSync = true;
    public bool powerSavingMode = false;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        
        // Initialize system services
        InitializeAnalytics();
        InitializeAds();
        InitializeSaveData();
        
        // Log session start
        Debug.Log($"Game session started - User: {GameManager.instance.currentUserLogin}, Time: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    }

    private void InitializeAnalytics()
    {
        if (!enableAnalytics) return;
        
        // TODO: Initialize your analytics service here
        // Example: Firebase Analytics
    }

    private void InitializeAds()
    {
        if (!enableAds) return;
        
        // TODO: Initialize your ad service here
        // Example: AdMob initialization
    }

    private void InitializeSaveData()
    {
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            PlayerPrefs.SetInt("FirstLaunch", 1);
            PlayerPrefs.SetString("InstallDate", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.Save();
        }
    }
}