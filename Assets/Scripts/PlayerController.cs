using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;
    
    [Header("Boundaries")]
    public float horizontalBoundary = 8.5f;
    public float verticalBoundary = 4.5f;
    
    [Header("References")]
    public GameObject playerSprite;
    public ParticleSystem deathEffect;
    
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
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void Update()
    {
        if (isDead || GameManager.instance.currentState != GameManager.GameState.Playing)
            return;

        HandleInput();
        MovePlayer();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                targetPosition = mainCamera.ScreenToWorldPoint(touch.position);
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(targetPosition.x, -horizontalBoundary, horizontalBoundary),
            Mathf.Clamp(targetPosition.y, -verticalBoundary, verticalBoundary),
            0f
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            clampedPosition,
            ref velocity,
            smoothTime
        );
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Enemy") || other.CompareTag("Projectile"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        playerSprite.SetActive(false);
        
        if (deathEffect != null)
        {
            deathEffect.Play();
        }

        GameManager.instance.GameOver();
    }
}