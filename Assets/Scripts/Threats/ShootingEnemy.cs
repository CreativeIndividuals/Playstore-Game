// Save as: Assets/Scripts/Enemies/ShootingEnemy.cs
using UnityEngine;

public class ShootingEnemy : StandardEnemy
{
    [Header("Shooting Configuration")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;  // Added explicit declaration
    [SerializeField] private bool trackPlayer = true;
    
    private float nextShootTime;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        
        // Find player reference
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"[{GameManager.instance.currentUserLogin}] ShootingEnemy found player target");
        }
        else
        {
            Debug.LogWarning($"[{GameManager.instance.currentUserLogin}] ShootingEnemy couldn't find player!");
        }

        // Randomize initial shoot time to prevent all enemies shooting at once
        nextShootTime = Time.time + Random.Range(0f, shootInterval);
    }

    protected override void Update()
    {
        base.Update();

        if (!isDestroyed && Time.time >= nextShootTime && 
            GameManager.instance.currentState == GameManager.GameState.Playing)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError($"[{GameManager.instance.currentUserLogin}] Bullet prefab not assigned to ShootingEnemy!");
            return;
        }

        // Calculate shooting direction
        Vector2 direction;
        if (trackPlayer && playerTransform != null)
        {
            direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        }
        else
        {
            direction = Vector2.left; // Default direction if no player or not tracking
        }

        // Create bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        
        // Set up bullet movement
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        else
        {
            Debug.LogWarning($"[{GameManager.instance.currentUserLogin}] Bullet prefab missing Rigidbody2D!");
        }

        // Update next shoot time
        nextShootTime = Time.time + shootInterval;

        // Log shooting event
        Debug.Log($"[{GameManager.instance.currentUserLogin}][{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                 $"Enemy fired bullet at {transform.position} with velocity {direction * bulletSpeed}");

        // Cleanup bullet after max lifetime
        Destroy(bullet, 5f);
    }

    // Utility method to set up bullet prefab at runtime if needed
    public void SetupBullet(GameObject prefab, float speed)
    {
        bulletPrefab = prefab;
        bulletSpeed = speed;
        Debug.Log($"[{GameManager.instance.currentUserLogin}] ShootingEnemy bullet configuration updated");
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate configuration in editor
        if (bulletPrefab != null && !bulletPrefab.GetComponent<Rigidbody2D>())
        {
            Debug.LogWarning("Bullet prefab should have a Rigidbody2D component!");
        }
        
        shootInterval = Mathf.Max(0.1f, shootInterval);
        bulletSpeed = Mathf.Max(1f, bulletSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerTransform == null) return;

        // Draw line to player when selected in editor
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, playerTransform.position);
    }
    #endif
}