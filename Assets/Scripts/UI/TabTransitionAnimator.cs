using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TabTransitionAnimator : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private float _moveDistance = 50f;
    private Vector2 _originalPosition;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Store original position
        _originalPosition = _rectTransform.anchoredPosition;
        
        // Setup initial state
        _canvasGroup.alpha = 0f;
        _rectTransform.anchoredPosition = new Vector2(_originalPosition.x, _originalPosition.y - _moveDistance);

        // Start fade in animation
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(AnimateIn());
    }

    private void OnDisable()
    {
        // Reset for next time
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        _canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = _originalPosition;
    }

    private IEnumerator AnimateIn()
    {
        float elapsed = 0f;
        Vector2 startPosition = _rectTransform.anchoredPosition;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / _fadeDuration;
            
            // Smooth step interpolation for more pleasing animation
            float smoothProgress = Mathf.SmoothStep(0f, 1f, normalizedTime);
            
            // Update position
            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, _originalPosition, smoothProgress);
            
            // Update transparency
            _canvasGroup.alpha = smoothProgress;
            
            yield return null;
        }

        // Ensure we end up exactly at the target state
        _rectTransform.anchoredPosition = _originalPosition;
        _canvasGroup.alpha = 1f;
    }
}