using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class TabData
    {
        public Button tabButton;
        public Image tabIcon;
        public Image tabIndicator;
        public TabContent content;
        public RectTransform buttonTransform;
        public Vector3 originalScale;
        public bool isAnimating;
        public float scaleProgress;
    }

    [Header("Tab Settings")]
    public List<TabData> tabs = new List<TabData>();
    public Color selectedIndicatorColor = Color.white;
    public Color unselectedIndicatorColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Tab Button Settings")]
    public Color selectedButtonColor = Color.white;
    public Color unselectedButtonColor = new Color(1f, 1f, 1f, 0.7f);
    
    [Header("Animation")]
    public bool useButtonScaleAnimation = true;
    public float selectedScale = 1.1f;
    public float scaleAnimationDuration = 0.2f;

    private TabData currentTab;
    public event Action<string> OnTabChanged;

    private void Start()
    {
        InitializeTabs();
        SelectDefaultTab();
    }

    private void Update()
    {
        if (useButtonScaleAnimation)
        {
            UpdateButtonAnimations();
        }
    }

    private void InitializeTabs()
    {
        foreach (var tab in tabs)
        {
            tab.buttonTransform = tab.tabButton.GetComponent<RectTransform>();
            tab.originalScale = tab.buttonTransform.localScale;
            tab.tabButton.onClick.AddListener(() => SelectTab(tab));
            SetTabButtonState(tab, false);
            
            if (tab.tabIcon != null && tab.content.TabIcon != null)
            {
                tab.tabIcon.sprite = tab.content.TabIcon;
            }
            
            tab.content.gameObject.SetActive(false);
        }
    }

    private void SelectDefaultTab()
    {
        if (tabs.Count > 0)
        {
            SelectTab(tabs[0]);
        }
    }

    public void SelectTab(TabData tab)
    {
        if (currentTab == tab)
            return;

        if (currentTab != null)
        {
            SetTabButtonState(currentTab, false);
            currentTab.content.OnTabDeselected();
        }

        currentTab = tab;
        SetTabButtonState(currentTab, true);
        currentTab.content.OnTabSelected();

        OnTabChanged?.Invoke(currentTab.content.TabName);
    }

    private void SetTabButtonState(TabData tab, bool selected)
    {
        tab.tabIndicator.color = selected ? selectedIndicatorColor : unselectedIndicatorColor;

        var buttonImage = tab.tabButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = selected ? selectedButtonColor : unselectedButtonColor;
        }

        if (useButtonScaleAnimation)
        {
            tab.isAnimating = true;
            tab.scaleProgress = 0f;
        }
    }

    private void UpdateButtonAnimations()
    {
        foreach (var tab in tabs)
        {
            if (tab.isAnimating)
            {
                tab.scaleProgress += Time.deltaTime / scaleAnimationDuration;
                if (tab.scaleProgress >= 1f)
                {
                    tab.scaleProgress = 1f;
                    tab.isAnimating = false;
                }

                float targetScale = tab == currentTab ? selectedScale : 1f;
                float currentScale = Mathf.Lerp(1f, targetScale, EaseInOut(tab.scaleProgress));
                tab.buttonTransform.localScale = tab.originalScale * currentScale;
            }
        }
    }

    private float EaseInOut(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    private void OnDestroy()
    {
        foreach (var tab in tabs)
        {
            tab.tabButton.onClick.RemoveAllListeners();
        }
    }
}