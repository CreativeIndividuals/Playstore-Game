using UnityEngine;

public abstract class TabAnimationBase : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.3f;

    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    protected float elapsedTime;
    protected bool isAnimating;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public abstract void PlayShowAnimation();
    public abstract void PlayHideAnimation();
    public abstract void UpdateAnimation();
    public abstract void ResetState();

    protected virtual void Update()
    {
        if (isAnimating)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / animationDuration);
            
            UpdateAnimation();

            if (progress >= 1f)
            {
                isAnimating = false;
            }
        }
    }

    protected float EaseInOut(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}