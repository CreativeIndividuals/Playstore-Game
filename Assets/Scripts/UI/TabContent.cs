using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public abstract class TabContent : MonoBehaviour, ITab
{
    [Header("Tab Settings")]
    [SerializeField] protected string tabName;
    [SerializeField] protected Sprite tabIcon;
    
    [Header("Components")]
    [SerializeField] protected TabSlideAnimation slideAnimation;
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected RectTransform contentContainer;
    
    [Header("Loading")]
    [SerializeField] protected GameObject loadingIndicator;
    [SerializeField] protected float minimumLoadingTime = 0.5f;
    
    protected bool isInitialized;
    protected CanvasGroup canvasGroup;
    
    public string TabName => tabName;
    public bool IsInitialized => isInitialized;
    public Sprite TabIcon => tabIcon;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (slideAnimation == null)
            slideAnimation = GetComponent<TabSlideAnimation>();

        // Ensure tab starts deactivated
        gameObject.SetActive(false);
    }

    public virtual void OnTabSelected()
    {
        gameObject.SetActive(true);
        StartCoroutine(InitializeTabRoutine());
    }

    public virtual void OnTabDeselected()
    {
        if (slideAnimation != null)
        {
            slideAnimation.PlayHideAnimation();
            StartCoroutine(DeactivateAfterAnimation());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator InitializeTabRoutine()
    {
        if (!isInitialized)
        {
            ShowLoading();
            
            yield return LoadContent();
            
            // Ensure minimum loading time
            float startTime = Time.time;
            while (Time.time - startTime < minimumLoadingTime)
            {
                yield return null;
            }
            
            HideLoading();
            isInitialized = true;
        }
        else
        {
            yield return RefreshContent();
        }

        if (slideAnimation != null)
        {
            slideAnimation.PlayShowAnimation();
        }
    }

    protected virtual IEnumerator LoadContent()
    {
        // Override in derived classes to load initial content
        yield break;
    }

    protected virtual IEnumerator RefreshContent()
    {
        // Override in derived classes to refresh content when tab is reselected
        yield break;
    }

    protected virtual void ShowLoading()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
        
        if (contentContainer != null)
            contentContainer.gameObject.SetActive(false);
    }

    protected virtual void HideLoading()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        if (contentContainer != null)
            contentContainer.gameObject.SetActive(true);
    }

    private IEnumerator DeactivateAfterAnimation()
    {
        if (slideAnimation != null)
        {
            yield return new WaitForSeconds(slideAnimation.animationDuration);
        }
        gameObject.SetActive(false);
    }

    protected virtual void ResetScroll()
    {
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = Vector2.one;
            scrollRect.velocity = Vector2.zero;
        }
    }

    protected virtual void OnEnable()
    {
        // Override in derived classes for enable behavior
    }

    protected virtual void OnDisable()
    {
        // Override in derived classes for disable behavior
    }

    protected virtual void OnDestroy()
    {
        // Override in derived classes for cleanup
    }

    protected virtual void SaveTabState()
    {
        // Override in derived classes to save tab-specific state
    }

    protected virtual void LoadTabState()
    {
        // Override in derived classes to load tab-specific state
    }

    protected virtual void HandleError(string errorMessage)
    {
        Debug.LogError($"[{tabName}] {errorMessage}");
        // Override in derived classes for custom error handling
    }
}