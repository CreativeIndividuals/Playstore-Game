// Save as: Assets/Scripts/Enemies/EnemyBase.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Components")]
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rb;
    protected Animator animator;

    [Header("State")]
    protected bool isInitialized;
    protected bool isDestroyed;
    protected Vector3 initialPosition;
    protected float timeAlive;
    protected float spawnTime;

    protected virtual void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        // Set up physics
        rb.gravityScale = 0f;
        rb.isKinematic = true;
        boxCollider.isTrigger = true;
    }

    protected virtual void Start()
    {
        spawnTime = Time.time;
        initialPosition = transform.position;
        isInitialized = true;
        
        LogSpawn();
    }

    protected virtual void Update()
    {
        if (!isInitialized || isDestroyed || 
            GameManager.instance.currentState != GameManager.GameState.Playing)
            return;

        timeAlive += Time.deltaTime;
        HandleMovement();
        CheckBounds();
    }

    protected virtual void HandleMovement()
    {
        // Override in derived classes
    }

    protected virtual void CheckBounds()
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPoint.x < -0.1f || viewportPoint.x > 1.1f ||
            viewportPoint.y < -0.1f || viewportPoint.y > 1.1f)
        {
            OnOutOfBounds();
        }
    }

    protected virtual void OnOutOfBounds()
    {
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        if (other.CompareTag("Player"))
        {
            OnHitPlayer();
        }
    }

    protected virtual void OnHitPlayer()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(1);  // Default damage value
        }
        DestroyEnemy();
    }

    protected virtual void DestroyEnemy()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        LogDestruction();
        Destroy(gameObject);
    }

    protected virtual void LogSpawn()
    {
        Debug.Log($"[{GameManager.instance.currentUserLogin}][{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                 $"Enemy spawned at {transform.position}");
    }

    protected virtual void LogDestruction()
    {
        float survivalTime = Time.time - spawnTime;
        Debug.Log($"[{GameManager.instance.currentUserLogin}][{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                 $"Enemy destroyed after {survivalTime:F2}s");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw movement preview
        Gizmos.color = Color.yellow;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + Vector3.left * 5f;
        Gizmos.DrawLine(startPos, endPos);
    }
}