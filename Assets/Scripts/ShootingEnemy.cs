using UnityEngine;

public class ShootingEnemy : EnemyBase
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float projectileSpeed = 5f;
    
    private float nextShootTime;

    protected override void Update()
    {
        base.Update();

        if (GameManager.instance.currentState == GameManager.GameState.Playing && 
            Time.time >= nextShootTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                Vector2 direction = (GameManager.instance.currentState == GameManager.GameState.Playing) 
                    ? (FindObjectOfType<PlayerController>().transform.position - transform.position).normalized 
                    : Vector2.left;
                
                rb.linearVelocity = direction * projectileSpeed;
            }

            Destroy(projectile, 5f);
        }

        nextShootTime = Time.time + shootInterval;
    }
}