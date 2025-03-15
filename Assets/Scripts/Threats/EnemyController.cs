// Save as: Assets/Scripts/Enemy/EnemyController.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    
    private Vector2[] fullPath;
    private float spawnTime;
    private bool isDestroyed;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float nextShootTime;

    public float currentLifetime { get; private set; }
    public Vector3 currentVelocity { get; private set; }
    public bool isLeaving { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (enemyData != null && enemyData.animatorController != null)
        {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = enemyData.animatorController;
        }
    }

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy Data is missing!");
            Destroy(gameObject);
            return;
        }

        spawnTime = Time.time;
        InitializeEnemy();
        StartCoroutine(AnimateEnemy());

        Debug.Log($"[{GameManager.instance.currentUserLogin}] Enemy {enemyData.enemyName} spawned at {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    }

    private void Update()
    {
        if (enemyData.canShoot && Time.time >= nextShootTime)
        {
            TryShoot();
        }
    }

    private void InitializeEnemy()
    {
        // Setup path
        int pathCount = enemyData.pathPoints?.Length ?? 0;
        fullPath = new Vector2[pathCount + 2];
        fullPath[0] = enemyData.entryOffset;
        for (int i = 0; i < pathCount; i++)
        {
            if (enemyData.pathPoints != null)
                fullPath[i + 1] = enemyData.pathPoints[i];
        }
        fullPath[^1] = enemyData.exitOffset;

        // Setup visuals
        spriteRenderer.sprite = enemyData.enemySprite;
        spriteRenderer.color = enemyData.enemyColor;

        // Set initial position and scale
        transform.position = new Vector3(fullPath[0].x, fullPath[0].y, transform.position.z);
        transform.localScale = enemyData.startScale;
    }

    private void TryShoot()
    {
        if (!enemyData.canShoot || enemyData.projectilePrefab == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
            GameObject bullet = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
            
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * enemyData.bulletSpeed;
            }
        }

        nextShootTime = Time.time + enemyData.shootInterval;
    }

    private IEnumerator AnimateEnemy()
    {
        float elapsed = 0f;
        
        while (elapsed < enemyData.activeDuration && !isDestroyed)
        {
            if (GameManager.instance.currentState != GameManager.GameState.Playing)
            {
                yield return null;
                continue;
            }

            currentLifetime = elapsed;

            float t = (elapsed * enemyData.moveSpeed) / enemyData.activeDuration;
            t = Mathf.Clamp01(t);

            try
            {
                // Movement
                float curveT = enemyData.movementPattern.Evaluate(t);
                Vector2 newPosition2D = EvaluatePath(fullPath, curveT);
                Vector3 oldPos = transform.position;
                transform.position = new Vector3(newPosition2D.x, newPosition2D.y, oldPos.z);
                
                // Calculate velocity
                currentVelocity = transform.position - oldPos;

                // Scale
                float scaleT = enemyData.scalePattern.Evaluate(t);
                Vector3 newScale = Vector3.Lerp(enemyData.startScale, enemyData.endScale, scaleT);
                
                // Flip based on movement direction
                if (currentVelocity.x != 0)
                {
                    newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(currentVelocity.x);
                }
                
                transform.localScale = newScale;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{GameManager.instance.currentUserLogin}] Error animating enemy {enemyData.enemyName}: {e.Message}");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isDestroyed)
        {
            HandleEnemyExit();
        }
    }

    private void HandleEnemyExit()
    {
        Vector2 exitPos = fullPath[^1];
        transform.position = new Vector3(exitPos.x, exitPos.y, transform.position.z);
        transform.localScale = enemyData.endScale;
        isLeaving = true;
        
        float survivalTime = Time.time - spawnTime;
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Enemy {enemyData.enemyName} survived for {survivalTime:F2}s");
        
        Destroy(gameObject);
    }

    private Vector2 EvaluatePath(Vector2[] path, float t)
    {
        if (path == null || path.Length == 0) return Vector2.zero;
        if (path.Length == 1) return path[0];

        int segmentCount = path.Length - 1;
        float scaledT = t * segmentCount;
        int currentSegment = Mathf.Min(Mathf.FloorToInt(scaledT), segmentCount - 1);
        float segmentT = scaledT - currentSegment;

        return Vector2.Lerp(path[currentSegment], path[currentSegment + 1], segmentT);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed || isLeaving) return;

        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
    }

    private void HandlePlayerCollision()
    {
        isDestroyed = true;
        float survivalTime = Time.time - spawnTime;
        
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Enemy {enemyData.enemyName} destroyed by player after {survivalTime:F2}s");
        
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (fullPath == null || fullPath.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < fullPath.Length - 1; i++)
        {
            Vector3 start = new Vector3(fullPath[i].x, fullPath[i].y, 0);
            Vector3 end = new Vector3(fullPath[i + 1].x, fullPath[i + 1].y, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}