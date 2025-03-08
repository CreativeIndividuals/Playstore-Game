using UnityEngine;
using System.Collections.Generic;

public class ThreatManager : MonoBehaviour
{
    [System.Serializable]
    public class ThreatPattern
    {
        public float duration = 10f;
        public bool useLasers = true;
        public bool useBullets = true;
        public bool useHoming = true;
        public float laserSpawnRate = 1f;
        public float bulletSpawnRate = 2f;
        public float homingSpawnRate = 3f;
        public int scoreThreshold;
    }

    [Header("Patterns")]
    public ThreatPattern[] patterns;
    public float difficultyMultiplier = 1f;
    
    [Header("Prefabs")]
    public GameObject laserPrefab;
    public GameObject bulletPrefab;
    public GameObject homingPrefab;
    
    private float nextLaserTime;
    private float nextBulletTime;
    private float nextHomingTime;
    private ThreatPattern currentPattern;
    private List<GameObject> activeThreats = new List<GameObject>();
    
    private void Start()
    {
        SetInitialPattern();
    }
    
    private void Update()
    {
        if (!GameManager.instance.isGameActive) return;
        
        UpdatePattern();
        SpawnThreats();
        CleanupThreats();
    }
    
    private void SetInitialPattern()
    {
        currentPattern = patterns[0];
        foreach (var pattern in patterns)
        {
            if (pattern.scoreThreshold <= GameManager.instance.currentScore)
            {
                currentPattern = pattern;
            }
        }
    }
    
    private void UpdatePattern()
    {
        foreach (var pattern in patterns)
        {
            if (pattern.scoreThreshold <= GameManager.instance.currentScore)
            {
                currentPattern = pattern;
            }
        }
    }
    
    private void SpawnThreats()
    {
        if (Time.time >= nextLaserTime && currentPattern.useLasers)
        {
            SpawnLaser();
            nextLaserTime = Time.time + (currentPattern.laserSpawnRate / difficultyMultiplier);
        }
        
        if (Time.time >= nextBulletTime && currentPattern.useBullets)
        {
            SpawnBulletPattern();
            nextBulletTime = Time.time + (currentPattern.bulletSpawnRate / difficultyMultiplier);
        }
        
        if (Time.time >= nextHomingTime && currentPattern.useHoming)
        {
            SpawnHomingMissile();
            nextHomingTime = Time.time + (currentPattern.homingSpawnRate / difficultyMultiplier);
        }
    }
    
    private void SpawnLaser()
    {
        Vector2 startPoint = GetRandomEdgePoint();
        Vector2 endPoint = GetRandomEdgePoint();
        
        GameObject laser = Instantiate(laserPrefab);
        laser.GetComponent<LaserBeam>()?.Initialize(startPoint, endPoint);
        activeThreats.Add(laser);
    }
    
    private void SpawnBulletPattern()
    {
        Vector2 spawnPoint = GetRandomSpawnPoint();
        int bulletCount = Random.Range(3, 8);
        float angleStep = 360f / bulletCount;
        
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
            float angle = i * angleStep;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            bullet.GetComponent<Bullet>()?.Initialize(direction);
            activeThreats.Add(bullet);
        }
    }
    
    private void SpawnHomingMissile()
    {
        Vector2 spawnPoint = GetRandomSpawnPoint();
        GameObject missile = Instantiate(homingPrefab, spawnPoint, Quaternion.identity);
        missile.GetComponent<HomingMissile>()?.Initialize();
        activeThreats.Add(missile);
    }
    
    private Vector2 GetRandomSpawnPoint()
    {
        float edge = Random.value;
        if (edge < 0.25f) // Top
            return new Vector2(Random.Range(-2.5f, 2.5f), 6f);
        else if (edge < 0.5f) // Right
            return new Vector2(2.5f, Random.Range(-4f, 4f));
        else if (edge < 0.75f) // Bottom
            return new Vector2(Random.Range(-2.5f, 2.5f), -6f);
        else // Left
            return new Vector2(-2.5f, Random.Range(-4f, 4f));
    }
    
    private Vector2 GetRandomEdgePoint()
    {
        return GetRandomSpawnPoint();
    }
    
    private void CleanupThreats()
    {
        activeThreats.RemoveAll(threat => threat == null);
        
        foreach (var threat in activeThreats.ToArray())
        {
            if (threat == null) continue;
            
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(threat.transform.position);
            if (viewportPoint.x < -0.2f || viewportPoint.x > 1.2f ||
                viewportPoint.y < -0.2f || viewportPoint.y > 1.2f)
            {
                activeThreats.Remove(threat);
                Destroy(threat);
            }
        }
    }
    
    public void ClearAllThreats()
    {
        foreach (var threat in activeThreats)
        {
            if (threat != null)
                Destroy(threat);
        }
        activeThreats.Clear();
    }
}