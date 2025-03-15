// Save as: Assets/Scripts/Enemies/EnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnWave
    {
        public float startTime = 0f;
        public GameObject[] enemyPrefabs;
        public float spawnRate = 1f;
        public int maxEnemies = 5;
        public bool useRandomSpawn = true;
        [Range(0f, 1f)] public float shooterProbability = 0.3f;
    }

    [Header("Wave Settings")]
    public SpawnWave[] waves;
    public float baseSpawnDelay = 0.5f;
    public float minSpawnDelay = 0.2f;

    [Header("Spawn Area")]
    public float horizontalSpawnWidth = 8.5f;
    public float verticalSpawnHeight = 4.5f;
    public float spawnYOffset = 1f;

    private float gameTime;
    private float nextSpawnTime;
    private SpawnWave currentWave;
    private readonly List<GameObject> activeEnemies = new();
    private Transform enemyContainer;
    private bool isSpawning;

    private void Start()
    {
        SetupContainer();
        ResetSpawner();
    }

    private void SetupContainer()
    {
        enemyContainer = new GameObject("EnemyContainer").transform;
        enemyContainer.SetParent(transform);
    }

    private void Update()
    {
        if (!GameManager.instance.isGameActive || !isSpawning) return;

        gameTime += Time.deltaTime;
        UpdateWave();
        
        if (Time.time >= nextSpawnTime && CanSpawnEnemy())
        {
            SpawnEnemy();
            UpdateNextSpawnTime();
        }

        CleanupEnemies();
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
                    LogWaveChange(i);
                }
                break;
            }
        }
    }

    private bool CanSpawnEnemy()
    {
        return currentWave != null && activeEnemies.Count < currentWave.maxEnemies;
    }

    private void SpawnEnemy()
    {
        if (currentWave?.enemyPrefabs == null || currentWave.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"[{GameManager.instance.currentUserLogin}] No enemy prefabs configured!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        GameObject prefab = currentWave.enemyPrefabs[Random.Range(0, currentWave.enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity, enemyContainer);
        
        activeEnemies.Add(enemy);
    }

    private Vector3 GetSpawnPosition()
    {
        if (currentWave.useRandomSpawn)
        {
            float x = Random.Range(-horizontalSpawnWidth, horizontalSpawnWidth);
            return new Vector3(x, verticalSpawnHeight + spawnYOffset, 0f);
        }
        return new Vector3(0f, verticalSpawnHeight + spawnYOffset, 0f);
    }

    private void UpdateNextSpawnTime()
    {
        float delay = Mathf.Max(minSpawnDelay, baseSpawnDelay / currentWave.spawnRate);
        nextSpawnTime = Time.time + delay;
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
        LogEvent("Started spawning");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        LogEvent("Stopped spawning");
    }

    public void ResetSpawner()
    {
        gameTime = 0f;
        nextSpawnTime = 0f;
        currentWave = waves.Length > 0 ? waves[0] : null;
        ClearEnemies();
        isSpawning = false;
        LogEvent("Reset spawner");
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
        LogEvent($"Cleared {count} enemies");
    }

    private void LogEvent(string message)
    {
        Debug.Log($"[{GameManager.instance.currentUserLogin}][{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                 $"EnemySpawner: {message}");
    }

    private void LogWaveChange(int waveIndex)
    {
        Debug.Log($"[{GameManager.instance.currentUserLogin}][{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                 $"Wave changed to {waveIndex + 1} at {gameTime:F1}s");
    }

    private void OnDrawGizmos()
    {
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
    }
}