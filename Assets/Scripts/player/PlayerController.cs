// Save as: Assets/Scripts/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float smoothTime = 0.05f; // Reduced for snappier controls
    
    [Header("Boundaries")]
    public float horizontalBoundary = 8.5f;
    public float verticalBoundary = 4.5f;
    
    [Header("References")]
    public GameObject playerSprite;
    public ParticleSystem deathEffect;
    public TrailRenderer trailEffect;
    
    private Vector3 velocity = Vector3.zero;
    private Vector2 targetPosition;
    private bool isDead = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;
        
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged += HandleGameStateChanged;
        }

        // Enable trail effect
        if (trailEffect != null)
        {
            trailEffect.enabled = true;
        }
    }

    private void Update()
    {
        if (isDead || GameManager.instance.currentState != GameManager.GameState.Playing)
            return;

        HandleInput();
        MovePlayer();
        ClampPosition();
    }

    private void HandleInput()
    {
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                targetPosition = mainCamera.ScreenToWorldPoint(touch.position);
            }
        }
        // Mouse input for testing
        else if (Input.GetMouseButton(0))
        {
            targetPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void MovePlayer()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            new Vector3(targetPosition.x, targetPosition.y, 0f),
            ref velocity,
            smoothTime
        );
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -horizontalBoundary, horizontalBoundary);
        pos.y = Mathf.Clamp(pos.y, -verticalBoundary, verticalBoundary);
        transform.position = pos;
    }

    public void TakeDamage(float damage)
    {
        Die();
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        playerSprite.SetActive(false);
        
        if (trailEffect != null)
            trailEffect.enabled = false;
            
        if (deathEffect != null)
        {
            deathEffect.Play();
        }

        // Camera shake effect
        CameraShake.Instance?.Shake(0.2f, 0.3f);

        GameManager.instance?.GameOver();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Enemy") || other.CompareTag("Projectile"))
        {
            Die();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            Die();
        }
    }
}