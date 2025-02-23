using UnityEngine;

public abstract class TabContent : MonoBehaviour
{
    protected GameDataManager gameData;
    protected string currentUserLogin;

    protected virtual void Awake()
    {
        gameData = GameDataManager.instance;
        currentUserLogin = gameData.currentUserLogin;
    }

    protected virtual void OnEnable()
    {
        RefreshContent();
    }

    public abstract void RefreshContent();

    protected string GetSessionTime()
    {
        var duration = gameData.GetSessionDuration();
        return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }
}