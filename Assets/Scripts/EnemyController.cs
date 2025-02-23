using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    private Vector2[] _fullPath;
    private float _spawnTime;
    private bool _isDestroyed;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    [Header("Runtime Data")]
    public float currentLifetime;
    public Vector3 currentVelocity;
    public bool isLeaving;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (enemyData.animatorController != null)
        {
            _animator = gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = enemyData.animatorController;
        }
    }

    private void Start()
    {
        _spawnTime = Time.time;
        InitializeEnemy();
        StartCoroutine(AnimateEnemy());

        // Log spawn
        Debug.Log($"Enemy {enemyData.enemyName} spawned at {GameDataManager.instance.GetCurrentUtcTime()} " +
                 $"by user {GameDataManager.instance.currentUserLogin}");
    }

    private void InitializeEnemy()
    {
        // Build the full path
        int pathCount = enemyData.pathPoints?.Length ?? 0;
        _fullPath = new Vector2[pathCount + 2];
        _fullPath[0] = enemyData.entryOffset;
        for (int i = 0; i < pathCount; i++)
        {
            if (enemyData.pathPoints != null)
                _fullPath[i + 1] = enemyData.pathPoints[i];
        }
        _fullPath[^1] = enemyData.exitOffset;

        // Setup visuals
        _spriteRenderer.sprite = enemyData.enemySprite;
        _spriteRenderer.color = enemyData.enemyColor;

        // Set initial position and scale
        transform.position = new Vector3(_fullPath[0].x, _fullPath[0].y, transform.position.z);
        transform.localScale = enemyData.startScale;

        // Update enemy statistics
        enemyData.UpdateSessionStats();
    }

    private IEnumerator AnimateEnemy()
    {
        float elapsed = 0f;
        
        while (elapsed < enemyData.activeDuration && !_isDestroyed)
        {
            currentLifetime = elapsed;

            // Calculate normalized time (0 to 1) adjusted by moveSpeed
            float t = (elapsed * enemyData.moveSpeed) / enemyData.activeDuration;
            t = Mathf.Clamp01(t);

            try
            {
                // Apply the movement AnimationCurve
                float curveT = enemyData.movementPattern.Evaluate(t);
                Vector2 newPosition2D = EvaluatePath(_fullPath, curveT);
                transform.position = new Vector3(newPosition2D.x, newPosition2D.y, transform.position.z);
                
                // Calculate velocity
                currentVelocity = (Vector3)newPosition2D - transform.position;

                // Apply the scale AnimationCurve
                float scaleT = enemyData.scalePattern.Evaluate(t);
                transform.localScale = Vector3.Lerp(enemyData.startScale, enemyData.endScale, scaleT);

                // Set facing direction based on movement
                if (currentVelocity.x != 0)
                {
                    transform.localScale = new Vector3(
                        Mathf.Abs(transform.localScale.x) * Mathf.Sign(currentVelocity.x),
                        transform.localScale.y,
                        transform.localScale.z
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error animating enemy {enemyData.enemyName}: {e.Message}");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!_isDestroyed)
        {
            // Ensure the enemy ends exactly at the exit point and final scale
            Vector2 exitPos = _fullPath[^1];
            transform.position = new Vector3(exitPos.x, exitPos.y, transform.position.z);
            transform.localScale = enemyData.endScale;

            // Mark as leaving before destruction
            isLeaving = true;
            
            // Update statistics and destroy
            float survivalTime = Time.time - _spawnTime;
            enemyData.UpdateSessionStats(false, survivalTime);
            
            Destroy(gameObject);
        }
    }

    private Vector2 EvaluatePath(Vector2[] path, float t)
    {
        if (path == null || path.Length == 0)
            return Vector2.zero;
        if (path.Length == 1)
            return path[0];

        int segmentCount = path.Length - 1;
        float scaledT = t * segmentCount;
        int currentSegment = Mathf.Min(Mathf.FloorToInt(scaledT), segmentCount - 1);
        float segmentT = scaledT - currentSegment;

        return Vector2.Lerp(path[currentSegment], path[currentSegment + 1], segmentT);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDestroyed || isLeaving) return;

        if (other.CompareTag("Player"))
        {
            HandleCollisionWithPlayer();
        }
    }

    private void HandleCollisionWithPlayer()
    {
        _isDestroyed = true;
        float survivalTime = Time.time - _spawnTime;
        enemyData.UpdateSessionStats(true, survivalTime);

        // Log destruction
        Debug.Log($"Enemy {enemyData.enemyName} destroyed by player at {GameDataManager.instance.GetCurrentUtcTime()} " +
                 $"Survival time: {survivalTime:F2}s");

        // Optional: Spawn death effects here
        
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (_fullPath == null || _fullPath.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _fullPath.Length - 1; i++)
        {
            Vector3 start = new Vector3(_fullPath[i].x, _fullPath[i].y, 0);
            Vector3 end = new Vector3(_fullPath[i + 1].x, _fullPath[i + 1].y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void OnDestroy()
    {
        if (!_isDestroyed && !isLeaving)
        {
            float survivalTime = Time.time - _spawnTime;
            enemyData.UpdateSessionStats(false, survivalTime);
        }
    }
}