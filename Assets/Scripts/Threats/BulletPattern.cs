using UnityEngine;
using System.Collections;

public class BulletPattern : MonoBehaviour
{
    [System.Serializable]
    public class Pattern
    {
        public enum PatternType
        {
            Single,
            Spread,
            Circle,
            Spiral,
            Grid
        }
        
        public PatternType type;
        public int bulletCount = 1;
        public float spreadAngle = 45f;
        public float spiralSpeed = 120f;
        public float delayBetweenBullets = 0.1f;
        public bool isHoming = false;
    }

    public GameObject bulletPrefab;
    public Pattern[] patterns;
    public float patternRepeatDelay = 2f;
    
    private void Start()
    {
        StartCoroutine(ShootPatterns());
    }

    private IEnumerator ShootPatterns()
    {
        while (true)
        {
            foreach (Pattern pattern in patterns)
            {
                yield return StartCoroutine(ExecutePattern(pattern));
                yield return new WaitForSeconds(patternRepeatDelay);
            }
        }
    }

    private IEnumerator ExecutePattern(Pattern pattern)
    {
        switch (pattern.type)
        {
            case Pattern.PatternType.Single:
                ShootSingle(pattern.isHoming);
                break;
                
            case Pattern.PatternType.Spread:
                ShootSpread(pattern.bulletCount, pattern.spreadAngle, pattern.isHoming);
                break;
                
            case Pattern.PatternType.Circle:
                ShootCircle(pattern.bulletCount, pattern.isHoming);
                break;
                
            case Pattern.PatternType.Spiral:
                yield return StartCoroutine(ShootSpiral(pattern.bulletCount, 
                    pattern.spiralSpeed, pattern.delayBetweenBullets, pattern.isHoming));
                break;
                
            case Pattern.PatternType.Grid:
                ShootGrid(pattern.bulletCount, pattern.spreadAngle, pattern.isHoming);
                break;
        }
    }

    private void ShootSingle(bool homing)
    {
        Vector2 direction = GetDirectionToPlayer();
        SpawnBullet(direction, homing);
    }

    private void ShootSpread(int count, float totalSpreadAngle, bool homing)
    {
        Vector2 centerDirection = GetDirectionToPlayer();
        float angleStep = totalSpreadAngle / (count - 1);
        float startAngle = -totalSpreadAngle / 2;
        
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = RotateVector(centerDirection, angle);
            SpawnBullet(direction, homing);
        }
    }

    private void ShootCircle(int count, bool homing)
    {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = RotateVector(Vector2.right, angle);
            SpawnBullet(direction, homing);
        }
    }

    private IEnumerator ShootSpiral(int count, float rotationSpeed, float delay, bool homing)
    {
        float angle = 0f;
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = RotateVector(Vector2.right, angle);
            SpawnBullet(direction, homing);
            angle += rotationSpeed * delay;
            yield return new WaitForSeconds(delay);
        }
    }

    private void ShootGrid(int size, float spacing, bool homing)
    {
        Vector2 startPos = transform.position;
        float offset = (size - 1) * spacing / 2;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 direction = new Vector2(
                    (x * spacing) - offset,
                    (y * spacing) - offset
                ).normalized;
                SpawnBullet(direction, homing);
            }
        }
    }

    private void SpawnBullet(Vector2 direction, bool homing)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, homing);
        }
    }

    private Vector2 GetDirectionToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        }
        return Vector2.right;
    }

    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
}