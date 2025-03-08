using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    public float speed = 8f;
    public float damage = 50f;
    public float lifetime = 5f;
    public bool isHoming = false;
    public float homingStrength = 2f;
    public float homingDuration = 1.5f;
    
    [Header("Visual Effects")]
    public TrailRenderer trail;
    public GameObject hitEffect;
    
    private Vector2 direction;
    private float spawnTime;
    private Transform target;
    private bool hasTarget;

    private void Start()
    {
        spawnTime = Time.time;
    }

    public void Initialize(Vector2 dir, bool homing = false)
    {
        direction = dir.normalized;
        isHoming = homing;
        
        if (isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                hasTarget = true;
            }
        }
        
        // Set initial rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 currentDirection = direction;
        
        // Apply homing behavior if enabled and within duration
        if (isHoming && hasTarget && Time.time - spawnTime < homingDuration)
        {
            Vector2 targetDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float homingFactor = homingStrength * Time.deltaTime;
            currentDirection = Vector2.Lerp(direction, targetDirection, homingFactor).normalized;
            direction = currentDirection;
            
            // Update rotation to face movement direction
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Move bullet
        transform.position += (Vector3)(currentDirection * speed * Time.deltaTime);
        
        // Check if bullet is off screen
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPoint.x < -0.1f || viewportPoint.x > 1.1f ||
            viewportPoint.y < -0.1f || viewportPoint.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }
    }
}