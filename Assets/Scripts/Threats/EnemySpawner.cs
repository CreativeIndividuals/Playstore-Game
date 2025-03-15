// Save as: Assets/Scripts/EnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;
using System;

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
        [Range(0f, 1f)] public float bulletProbability = 0.3f;
        public float enemySpeed = 5f;
        public bool enableHoming = false;
    }

    [Header("Wave Configuration")]
    public EnemyWaveConfig waveConfig;
    public DifficultyWave[] waves;

    [Header("Spawn Settings")]
    public float baseSpawnDelay = 0.5f;
    public float minSpawnDelay = 0.2f;
    
    [Header("Spawn Boundaries")]
    public float horizontalSpawnWidth = 8f;
    public float verticalSpawnHeight = 6f;
    public float spawnYOffset = 1f;

    [Header("Debug Settings")]
    public bool showDebugGizmos = true;
    public bool logSpawnEvents = true;

    // Private variables
    private float gameTime;
    private float nextSpawnTime;
    private DifficultyWave currentWave;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private Transform enemyContainer;
    private string currentUserLogin;
    private DateTime sessionStartTime;

    private void Awake()
    {
        // Create enemy container if it doesn't exist
        if (enemyContainer == null)
        {
            enemyContainer = new GameObject("EnemyContainer").transform;
            enemyContainer.SetParent(transform);
        }

        // Load waves from config if available
        if (waveConfig != null)
        {
            waves = waveConfig.waves;
        }

        // Initialize session data
        currentUserLogin = GameManager.instance.currentUserLogin;
        sessionStartTime = DateTime.Parse(GameManager.instance.sessionStartTime);
    }

    private void Start()
    {
        ResetSpawner();
        LogEvent("Spawner initialized");
    }

    public void ResetSpawner()
    {
        gameTime = 0f;
        nextSpawnTime = 0f;
        currentWave = waves[0];
        ClearEnemies();
        isSpawning = false;
        LogEvent("Spawner reset");
    }

    private void Update()
    {
        if (!GameManager.instance.isGameActive || !isSpawning) return;

        gameTime += Time.deltaTime;
        UpdateCurrentWave();
        
        if (Time.time >= nextSpawnTime && CanSpawnEnemy())
        {
            SpawnEnemy();
            UpdateNextSpawnTime();
        }

        CleanupEnemies();
    }

    private void UpdateCurrentWave()
    {
        for (int i = waves.Length - 1; i >= 0; i--)
        {
            if (gameTime >= waves[i].startTime)
            {
                if (currentWave != waves[i])
                {
                    currentWave = waves[i];
                    LogEvent($"Wave changed to {i + 1}");
                }
                break;
            }
        }
    }

    private bool CanSpawnEnemy()
    {
        return currentWave != null && 
               activeEnemies.Count < currentWave.maxSimultaneousEnemies;
    }

    private void SpawnEnemy()
    {
        if (currentWave.enemyTypes == null || currentWave.enemyTypes.Length == 0)
        {
            LogEvent("Warning: No enemy types defined in current wave");
            return;
        }

        // Select random enemy type
        int enemyIndex = UnityEngine.Random.Range(0, currentWave.enemyTypes.Length);
        EnemyScriptableObject enemyData = currentWave.enemyTypes[enemyIndex];

        // Calculate spawn position
        Vector3 spawnPosition;
        if (currentWave.useRandomPosition)
        {
            spawnPosition = GetRandomSpawnPosition();
        }
        else
        {
            spawnPosition = GetPatternSpawnPosition();
        }

        // Create enemy game object
        GameObject enemy = CreateEnemy(enemyData, spawnPosition);
        
        // Initialize enemy behavior
        InitializeEnemyBehavior(enemy, enemyData);

        // Add to active enemies list
        activeEnemies.Add(enemy);

        LogEvent($"Spawned {enemyData.enemyName} at position {spawnPosition}");
    }

    private GameObject CreateEnemy(EnemyScriptableObject enemyData, Vector3 position)
    {
        GameObject enemy = new GameObject($"Enemy_{enemyData.enemyName}_{activeEnemies.Count}");
        enemy.transform.parent = enemyContainer;
        enemy.transform.position = position;

        // Add required components
        SpriteRenderer spriteRenderer = enemy.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemyData.enemySprite;
        spriteRenderer.color = enemyData.enemyColor;

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        return enemy;
    }

    private void InitializeEnemyBehavior(GameObject enemy, EnemyScriptableObject enemyData)
    {
        EnemyController controller = enemy.AddComponent<EnemyController>();
        controller.enemyData = enemyData;

        // Add shooting behavior if probability check passes
        if (UnityEngine.Random.value < currentWave.bulletProbability)
        {
            EnemyShooter shooter = enemy.AddComponent<EnemyShooter>();
            shooter.Initialize(currentWave.bulletProbability);
        }

        // Add homing behavior if enabled in wave
        if (currentWave.enableHoming)
        {
            EnemyHoming homing = enemy.AddComponent<EnemyHoming>();
            homing.Initialize(currentWave.enemySpeed);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float xPos = UnityEngine.Random.Range(-horizontalSpawnWidth, horizontalSpawnWidth);
        return new Vector3(xPos, verticalSpawnHeight + spawnYOffset, 0f);
    }

    private Vector3 GetPatternSpawnPosition()
    {
        // Calculate spawn position based on current game time
        float phase = (gameTime % 5f) / 5f; // 5-second pattern cycle
        float xPos = Mathf.Sin(phase * Mathf.PI * 2f) * horizontalSpawnWidth;
        return new Vector3(xPos, verticalSpawnHeight + spawnYOffset, 0f);
    }

    private void UpdateNextSpawnTime()
    {
        float currentSpawnDelay = Mathf.Max(minSpawnDelay, baseSpawnDelay / currentWave.spawnRate);
        nextSpawnTime = Time.time + currentSpawnDelay;
    }

    private void CleanupEnemies()
    {
        activeEnemies.RemoveAll(enemy =>
        {
            if (enemy == null) return true;

            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(enemy.transform.position);
            if (viewportPoint.y < -0.1f)
            {
                GameManager.instance.AddScore(1);
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
        nextSpawnTime = Time.time + baseSpawnDelay;
        LogEvent("Spawning started");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        LogEvent("Spawning stopped");
    }

    public void ClearEnemies()
    {
        int count = activeEnemies.Count;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        LogEvent($"Cleared {count} enemies");
    }

    private void LogEvent(string message)
    {
        if (!logSpawnEvents) return;

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        Debug.Log($"[{currentUserLogin}][{timestamp}] EnemySpawner: {message}");
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(0, verticalSpawnHeight + spawnYOffset, 0),
            new Vector3(horizontalSpawnWidth * 2, 1, 0)
        );

        // Draw game boundaries
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(horizontalSpawnWidth * 2, verticalSpawnHeight * 2, 0)
        );

        // Draw enemy paths if not using random positions
        if (currentWave != null && !currentWave.useRandomPosition)
        {
            Gizmos.color = Color.red;
            float phase = ((Time.time % 5f) / 5f) * Mathf.PI * 2f;
            Vector3 spawnPos = GetPatternSpawnPosition();
            Vector3 nextPos = spawnPos;
            for (int i = 0; i < 20; i++)
            {
                float t = i / 19f;
                float y = verticalSpawnHeight * (1f - t);
                float x = Mathf.Sin(phase + t * Mathf.PI * 2f) * horizontalSpawnWidth;
                Vector3 pos = new Vector3(x, y, 0);
                Gizmos.DrawLine(nextPos, pos);
                nextPos = pos;
            }
        }
    }
    #endif
}