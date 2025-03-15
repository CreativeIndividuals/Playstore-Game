// Save as: Assets/Scripts/EnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class DifficultyWave
    {
        public float startTime = 0f;
        public EnemyScriptableObject[] enemyTypes;
        public float spawnRate = 1f;
        public int maxSimultaneousEnemies = 5;
        public bool useRandomPosition = true;
    }

    [Header("Wave Settings")]
    public DifficultyWave[] waves;
    public float baseSpawnRate = 1f;
    public float spawnRateIncrease = 0.1f;
    public float maxSpawnRate = 3f;

    [Header("Boundaries")]
    public float horizontalBoundary = 8.5f;  // Screen width boundary
    public float verticalBoundary = 4.5f;    // Screen height boundary
    public float spawnHeight = 6f;           // Spawn above screen
    
    private float nextSpawnTime;
    private float currentSpawnRate;
    private DifficultyWave currentWave;
    private readonly List<GameObject> activeEnemies = new();
    private Transform enemyContainer;
    private bool isSpawning;
    private float gameTime;

    private void Start()
    {
        currentSpawnRate = baseSpawnRate;
        SetupContainer();
        SetInitialWave();
    }

    private void SetupContainer()
    {
        if (enemyContainer == null)
        {
            GameObject container = new GameObject("EnemyContainer");
            enemyContainer = container.transform;
            enemyContainer.SetParent(transform);
        }
    }

    private void SetInitialWave()
    {
        if (waves != null && waves.Length > 0)
        {
            currentWave = waves[0];
            Debug.Log($"[{GameManager.instance.currentUserLogin}] Initial wave set at {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            Debug.LogError($"[{GameManager.instance.currentUserLogin}] No waves configured in EnemySpawner!");
        }
    }

    private void Update()
    {
        if (!GameManager.instance.isGameActive || !isSpawning) return;

        gameTime += Time.deltaTime;
        
        if (Time.time >= nextSpawnTime && CanSpawnEnemy())
        {
            SpawnEnemy();
            UpdateSpawnRate();
            nextSpawnTime = Time.time + (1f / currentSpawnRate);
        }

        UpdateWave();
        CleanupEnemies();
    }

    private bool CanSpawnEnemy()
    {
        return currentWave != null && 
               activeEnemies.Count < currentWave.maxSimultaneousEnemies;
    }

    private void SpawnEnemy()
    {
        if (currentWave?.enemyTypes == null || currentWave.enemyTypes.Length == 0) return;

        // Select random enemy type
        int randomIndex = Random.Range(0, currentWave.enemyTypes.Length);
        EnemyScriptableObject enemyData = currentWave.enemyTypes[randomIndex];
        
        // Calculate spawn position
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Create enemy
        GameObject enemyObject = new GameObject($"Enemy_{enemyData.enemyName}");
        enemyObject.transform.SetParent(enemyContainer);
        enemyObject.transform.position = spawnPosition;

        // Add components
        var spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemyData.enemySprite;
        spriteRenderer.color = enemyData.enemyColor;

        var collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        var controller = enemyObject.AddComponent<EnemyController>();
        controller.enemyData = enemyData;

        activeEnemies.Add(enemyObject);
        
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Spawned {enemyData.enemyName} at {spawnPosition}");
    }

    private Vector3 GetSpawnPosition()
    {
        if (currentWave.useRandomPosition)
        {
            // Random position along top of screen
            float x = Random.Range(-horizontalBoundary, horizontalBoundary);
            return new Vector3(x, spawnHeight, 0f);
        }
        else
        {
            // Center position at spawn height
            return new Vector3(0f, spawnHeight, 0f);
        }
    }

    private void UpdateSpawnRate()
    {
        currentSpawnRate = Mathf.Min(
            baseSpawnRate + (spawnRateIncrease * (gameTime / 60f)), // Increase every minute
            maxSpawnRate
        );
    }

    private void UpdateWave()
    {
        for (int i = waves.Length - 1; i >= 0; i--)
        {
            if (gameTime >= waves[i].startTime)
            {
                if (currentWave != waves[i])
                {
                    currentWave = waves[i];
                    Debug.Log($"[{GameManager.instance.currentUserLogin}] Wave changed at {gameTime:F1}s");
                }
                break;
            }
        }
    }

    private void CleanupEnemies()
    {
        activeEnemies.RemoveAll(enemy => 
        {
            if (enemy == null) return true;
            
            // Check if enemy is below screen
            if (enemy.transform.position.y < -verticalBoundary)
            {
                GameManager.instance.AddScore(1);
                Destroy(enemy);
                return true;
            }
            
            // Check if enemy is far outside horizontal boundaries
            if (Mathf.Abs(enemy.transform.position.x) > horizontalBoundary * 1.5f)
            {
                Destroy(enemy);
                return true;
            }
            
            return false;
        });
    }

    public void StartSpawning()
    {
        isSpawning = true;
        gameTime = 0f;
        nextSpawnTime = Time.time + (1f / baseSpawnRate);
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Started spawning enemies");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Stopped spawning enemies");
    }

    public void ClearEnemies()
    {
        int count = activeEnemies.Count;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
        Debug.Log($"[{GameManager.instance.currentUserLogin}] Cleared {count} enemies");
    }

    private void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(-horizontalBoundary, spawnHeight, 0),
            new Vector3(horizontalBoundary, spawnHeight, 0)
        );

        // Draw game boundaries
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(horizontalBoundary * 2, verticalBoundary * 2, 0)
        );
    }

    private void OnValidate()
    {
        // Ensure boundaries are positive
        horizontalBoundary = Mathf.Max(0.1f, horizontalBoundary);
        verticalBoundary = Mathf.Max(0.1f, verticalBoundary);
        spawnHeight = Mathf.Max(verticalBoundary, spawnHeight);
    }
}