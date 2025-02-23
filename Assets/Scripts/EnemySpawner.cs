using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPattern
    {
        public EnemyScriptableObject[] enemies;
        public float spawnInterval = 1f;
        public int scoreThreshold;
    }

    public SpawnPattern[] patterns;
    public float baseSpawnRate = 1f;
    public float spawnRateIncrease = 0.1f;
    public float maxSpawnRate = 3f;
    
    private float _nextSpawnTime;
    private float _currentSpawnRate;
    private SpawnPattern _currentPattern;
    private readonly List<GameObject> _activeEnemies = new();
    private GameObject _enemyContainer;

    private void Start()
    {
        _currentSpawnRate = baseSpawnRate;
        SetInitialPattern();
        _enemyContainer = new GameObject("EnemyContainer");
    }

    private void Update()
    {
        if (!GameManager.instance.isGameActive) return;

        if (Time.time >= _nextSpawnTime)
        {
            SpawnEnemy();
            UpdateSpawnRate();
            _nextSpawnTime = Time.time + (1f / _currentSpawnRate);
        }

        UpdatePattern();
        CleanupEnemies();
    }

    private void SetInitialPattern()
    {
        _currentPattern = patterns[0];
        foreach (var pattern in patterns)
        {
            if (pattern.scoreThreshold <= GameManager.instance.currentScore)
            {
                _currentPattern = pattern;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (_currentPattern.enemies.Length == 0) return;

        int randomIndex = Random.Range(0, _currentPattern.enemies.Length);
        EnemyScriptableObject enemyData = _currentPattern.enemies[randomIndex];
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        GameObject enemyObject = new GameObject($"Enemy_{enemyData.enemyName}");
        enemyObject.transform.parent = _enemyContainer.transform;
        enemyObject.transform.position = spawnPosition;

        enemyObject.AddComponent<SpriteRenderer>();
        // Assuming you have a sprite assigned in the enemy prefab
        // spriteRenderer.sprite = enemyData.sprite;

        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
        enemyController.enemyData = enemyData;

        _activeEnemies.Add(enemyObject);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-2.5f, 2.5f);
        return new Vector3(x, 6f, 0f);
    }

    private void UpdateSpawnRate()
    {
        _currentSpawnRate = Mathf.Min(
            baseSpawnRate + (spawnRateIncrease * (GameManager.instance.currentScore / 100f)),
            maxSpawnRate
        );
    }

    private void UpdatePattern()
    {
        foreach (var pattern in patterns)
        {
            if (pattern.scoreThreshold <= GameManager.instance.currentScore)
            {
                _currentPattern = pattern;
            }
        }
    }

    private void CleanupEnemies()
    {
        _activeEnemies.RemoveAll(enemy => 
        {
            if (enemy == null) return true;
            
            if (enemy.transform.position.y < -6f)
            {
                GameManager.instance.AddScore(1);
                Destroy(enemy);
                return true;
            }
            return false;
        });
    }

    public void ClearEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        _activeEnemies.Clear();
    }
}