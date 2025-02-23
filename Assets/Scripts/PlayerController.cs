using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [FormerlySerializedAs("dragSensitivity")]
    [Header("Movement Settings")]
    [SerializeField] private float _dragSensitivity = 1f;
    [FormerlySerializedAs("smoothTime")] [SerializeField] private float _smoothTime = 0.1f;
    [FormerlySerializedAs("minMoveThreshold")] [SerializeField] private float _minMoveThreshold = 0.001f;
    
    [FormerlySerializedAs("horizontalBoundary")]
    [Header("Boundaries")]
    [SerializeField] private float _horizontalBoundary = 2.5f;
    [FormerlySerializedAs("verticalBoundary")] [SerializeField] private float _verticalBoundary = 4.5f;
    
    [FormerlySerializedAs("trail")]
    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer _trail;
    [FormerlySerializedAs("moveEffect")] [SerializeField] private ParticleSystem _moveEffect;
    [FormerlySerializedAs("tiltAngle")] [SerializeField] private float _tiltAngle = 15f;
    [FormerlySerializedAs("tiltSmoothTime")] [SerializeField] private float _tiltSmoothTime = 0.1f;

    [FormerlySerializedAs("showDebugGizmos")]
    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmos = true;

    // Components
    private Camera _mainCamera;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    // Movement
    private Vector3 _velocity;
    private Vector2 _lastTouchPos;
    private Vector3 _targetPosition;
    private float _currentTilt;
    private float _tiltVelocity;

    // State
    private bool _isDragging;
    private bool _isDead;

    // Session tracking
    private string _sessionStartTime;
    private string _userLogin;
    private float _totalDistanceMoved;
    private int _touchCount;
    private float _sessionStartedAt;

    private void Start()
    {
        InitializeComponents();
        InitializeSession();
    }

    private void InitializeComponents()
    {
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        
        _targetPosition = transform.position;
    }

    private void InitializeSession()
    {
        _sessionStartTime = "2025-02-23 13:30:41";
        _userLogin = "QAD6675";
        _sessionStartedAt = Time.time;
        
        Debug.Log($"Player initialized - User: {_userLogin}, Session: {_sessionStartTime}");
    }

    private void Update()
    {
        if (_isDead || GameManager.instance.currentState != GameManager.GameState.Playing) 
            return;

        HandleTouchInput();
        UpdateMovement();
        UpdateVisuals();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchWorldPos = _mainCamera.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touchWorldPos);
                    break;

                case TouchPhase.Moved:
                    HandleTouchMoved(touchWorldPos);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandleTouchEnded();
                    break;
            }
        }
    }

    private void HandleTouchBegan(Vector2 touchWorldPos)
    {
        _isDragging = true;
        _lastTouchPos = touchWorldPos;
        _touchCount++;
    }

    private void HandleTouchMoved(Vector2 touchWorldPos)
    {
        if (!_isDragging) return;

        Vector2 delta = touchWorldPos - _lastTouchPos;
        Vector3 newTargetPosition = _targetPosition + new Vector3(
            delta.x * _dragSensitivity,
            delta.y * _dragSensitivity,
            0
        );

        _targetPosition = ClampPosition(newTargetPosition);
        _lastTouchPos = touchWorldPos;

        // Track movement
        _totalDistanceMoved += delta.magnitude;
    }

    private void HandleTouchEnded()
    {
        _isDragging = false;
    }

    private void UpdateMovement()
    {
        Vector3 previousPosition = transform.position;
        
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _velocity,
            _smoothTime
        );

        // Update tilt based on movement
        float moveDirection = transform.position.x - previousPosition.x;
        float targetTilt = -moveDirection * _tiltAngle;
        _currentTilt = Mathf.SmoothDamp(_currentTilt, targetTilt, ref _tiltVelocity, _tiltSmoothTime);
        transform.rotation = Quaternion.Euler(0, 0, _currentTilt);
    }

    private void UpdateVisuals()
    {
        bool isMoving = _velocity.magnitude > _minMoveThreshold;

        // Update trail
        if (_trail != null)
            _trail.emitting = isMoving;

        // Update movement effect
        if (_moveEffect != null)
        {
            var emission = _moveEffect.emission;
            emission.enabled = isMoving;
        }
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, -_horizontalBoundary, _horizontalBoundary),
            Mathf.Clamp(position.y, -_verticalBoundary, _verticalBoundary),
            position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;

        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        LogSessionStats();

        // Disable visuals
        _spriteRenderer.enabled = false;
        if (_trail) _trail.enabled = false;
        if (_moveEffect) _moveEffect.Stop();

        // Disable collision
        _boxCollider.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Optional: Add death effect here
        yield return new WaitForSecondsRealtime(0.5f);
        GameManager.instance.GameOver();
    }

    private void LogSessionStats()
    {
        float sessionDuration = Time.time - _sessionStartedAt;
        
        Debug.Log($"Player Session Stats:\n" +
                 $"User: {_userLogin}\n" +
                 $"Session Start: {_sessionStartTime}\n" +
                 $"Duration: {sessionDuration:F2}s\n" +
                 $"Distance Moved: {_totalDistanceMoved:F2}\n" +
                 $"Touch Count: {_touchCount}");

        // Save stats
        string statsKey = $"PlayerStats_{_userLogin}_{System.DateTime.Now:yyyyMMdd}";
        PlayerPrefs.SetFloat($"{statsKey}_Duration", sessionDuration);
        PlayerPrefs.SetFloat($"{statsKey}_Distance", _totalDistanceMoved);
        PlayerPrefs.SetInt($"{statsKey}_Touches", _touchCount);
        PlayerPrefs.Save();
    }

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || !Application.isPlaying) return;

        // Draw movement boundaries
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, 
            new Vector3(_horizontalBoundary * 2, _verticalBoundary * 2, 0));

        // Draw movement vector
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetPosition);
        }
    }
}