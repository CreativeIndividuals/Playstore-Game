using UnityEngine;
using UnityEngine.UI;

public class TabMenuController : MonoBehaviour
{
    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public GameObject contentPanel;
        public Image tabIcon;
        public Color selectedColor = new Color(1, 1, 1, 1);
        public Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1);
    }

    [Header("Tab Setup")]
    [SerializeField] private TabButton[] _tabs;
    [SerializeField] private int _defaultTabIndex;
    
    private int _currentTabIndex = -1;

    void Start()
    {
        InitializeTabs();
        SwitchToTab(_defaultTabIndex);
    }

    private void InitializeTabs()
    {
        for (int i = 0; i < _tabs.Length; i++)
        {
            int index = i;
            _tabs[i].button.onClick.AddListener(() => SwitchToTab(index));
            _tabs[i].contentPanel.SetActive(false);
        }
    }

    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex == _currentTabIndex) return;

        // Deactivate current tab if one is active
        if (_currentTabIndex >= 0 && _currentTabIndex < _tabs.Length)
        {
            _tabs[_currentTabIndex].contentPanel.SetActive(false);
            _tabs[_currentTabIndex].tabIcon.color = _tabs[_currentTabIndex].unselectedColor;
        }

        // Activate new tab
        _tabs[tabIndex].contentPanel.SetActive(true);
        _tabs[tabIndex].tabIcon.color = _tabs[tabIndex].selectedColor;
        _currentTabIndex = tabIndex;

        // Optional: Add animation or transition effects here
    }
}