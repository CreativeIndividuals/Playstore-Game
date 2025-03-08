using UnityEngine;
using System.Collections;

public class BackgroundThreatManager : MonoBehaviour
{
    [System.Serializable]
    public class ThreatType
    {
        public GameObject prefab;
        public float spawnInterval = 3f;
        public float minSpeed = 5f;
        public float maxSpeed = 10f;
        public bool isHoming = false;
    }

    [Header("Threat Types")]
    public ThreatType laser;
    public ThreatType bullet;
    public ThreatType homingMissile;

    [Header("Spawn Settings")]
    public float initialDelay = 2f;
    public float difficultyIncreaseInterval = 30f;
    public float spawnIntervalReduction = 0.1f;

    private void Start()
    {
        StartCoroutine(SpawnThreats(laser));
        StartCoroutine(SpawnThreats(bullet));
        StartCoroutine(SpawnThreats(homingMissile));
        StartCoroutine(IncreaseDifficulty());
    }

    private IEnumerator SpawnThreats(ThreatType threat)
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (GameManager.instance.currentState == GameManager.GameState.Playing)
            {
                SpawnThreat(threat);
            }
            yield return new WaitForSeconds(threat.spawnInterval);
        }
    }

    private void SpawnThreat(ThreatType threat)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject threatObj = Instantiate(threat.prefab, spawnPosition, Quaternion.identity);
        
        float speed = Random.Range(threat.minSpeed, threat.maxSpeed);
        
        if (threat.isHoming)
        {
            HomingMissile homing = threatObj.GetComponent<HomingMissile>();
            if (homing != null)
            {
                homing.Initialize(speed);
            }
        }
        else
        {
            Rigidbody2D rb = threatObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.left * speed;
            }
        }

        Destroy(threatObj, 10f);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float yPos = Random.Range(-4.5f, 4.5f);
        return new Vector3(12f, yPos, 0f);
    }

    private IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);
            
            laser.spawnInterval = Mathf.Max(laser.spawnInterval - spawnIntervalReduction, 1f);
            bullet.spawnInterval = Mathf.Max(bullet.spawnInterval - spawnIntervalReduction, 1f);
            homingMissile.spawnInterval = Mathf.Max(homingMissile.spawnInterval - spawnIntervalReduction, 2f);
        }
    }
}