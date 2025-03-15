using UnityEngine;

public class TabSlideAnimation : TabAnimationBase
{
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Header("Slide Settings")]
    public SlideDirection slideDirection = SlideDirection.Right;
    public float slideDistance = 100f;
    public bool fadeWhileSliding = true;

    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private float startAlpha;
    private float targetAlpha;

    protected override void Awake()
    {
        base.Awake();
        originalPosition = rectTransform.anchoredPosition;
    }

    public override void PlayShowAnimation()
    {
        isAnimating = true;
        elapsedTime = 0f;
        
        startPosition = GetStartPosition();
        targetPosition = originalPosition;
        
        rectTransform.anchoredPosition = startPosition;
        
        if (fadeWhileSliding)
        {
            startAlpha = 0f;
            targetAlpha = 1f;
            canvasGroup.alpha = startAlpha;
        }
    }

    public override void PlayHideAnimation()
    {
        isAnimating = true;
        elapsedTime = 0f;
        
        startPosition = rectTransform.anchoredPosition;
        targetPosition = GetEndPosition();
        
        if (fadeWhileSliding)
        {
            startAlpha = canvasGroup.alpha;
            targetAlpha = 0f;
        }
    }

    public override void UpdateAnimation()
    {
        float progress = EaseInOut(elapsedTime / animationDuration);
        
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
        
        if (fadeWhileSliding)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
        }
    }

    public override void ResetState()
    {
        isAnimating = false;
        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 1f;
    }

    private Vector2 GetStartPosition()
    {
        switch (slideDirection)
        {
            case SlideDirection.Left:
                return originalPosition + Vector2.right * slideDistance;
            case SlideDirection.Right:
                return originalPosition + Vector2.left * slideDistance;
            case SlideDirection.Up:
                return originalPosition + Vector2.down * slideDistance;
            case SlideDirection.Down:
                return originalPosition + Vector2.up * slideDistance;
            default:
                return originalPosition;
        }
    }

    private Vector2 GetEndPosition()
    {
        switch (slideDirection)
        {
            case SlideDirection.Left:
                return originalPosition + Vector2.left * slideDistance;
            case SlideDirection.Right:
                return originalPosition + Vector2.right * slideDistance;
            case SlideDirection.Up:
                return originalPosition + Vector2.up * slideDistance;
            case SlideDirection.Down:
                return originalPosition + Vector2.down * slideDistance;
            default:
                return originalPosition;
        }
    }
}