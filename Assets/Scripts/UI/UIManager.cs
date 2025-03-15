using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Tab Panels")]
    public GameObject playPanel;
    public GameObject shopPanel;
    public GameObject charactersPanel;
    public GameObject settingsPanel;
    public GameObject leaderboardPanel;
    
    [Header("Tab Buttons")]
    public Button playButton;
    public Button shopButton;
    public Button charactersButton;
    public Button settingsButton;
    public Button leaderboardButton;
    
    [Header("Tab Indicators")]
    public Image playIndicator;
    public Image shopIndicator;
    public Image charactersIndicator;
    public Image settingsIndicator;
    public Image leaderboardIndicator;

    private void Start()
    {
        // Set up button listeners
        playButton.onClick.AddListener(() => SwitchTab(0));
        shopButton.onClick.AddListener(() => SwitchTab(1));
        charactersButton.onClick.AddListener(() => SwitchTab(2));
        settingsButton.onClick.AddListener(() => SwitchTab(3));
        leaderboardButton.onClick.AddListener(() => SwitchTab(4));

        // Show play tab by default
        SwitchTab(0);
    }

    public void SwitchTab(int tabIndex)
    {
        // Deactivate all panels
        playPanel.SetActive(false);
        shopPanel.SetActive(false);
        charactersPanel.SetActive(false);
        settingsPanel.SetActive(false);
        leaderboardPanel.SetActive(false);

        // Reset all indicators
        playIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        shopIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        charactersIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        settingsIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        leaderboardIndicator.color = new Color(1f, 1f, 1f, 0.5f);

        // Activate selected panel and indicator
        switch (tabIndex)
        {
            case 0:
                playPanel.SetActive(true);
                playIndicator.color = Color.white;
                break;
            case 1:
                shopPanel.SetActive(true);
                shopIndicator.color = Color.white;
                break;
            case 2:
                charactersPanel.SetActive(true);
                charactersIndicator.color = Color.white;
                break;
            case 3:
                settingsPanel.SetActive(true);
                settingsIndicator.color = Color.white;
                break;
            case 4:
                leaderboardPanel.SetActive(true);
                leaderboardIndicator.color = Color.white;
                break;
        }
    }
}