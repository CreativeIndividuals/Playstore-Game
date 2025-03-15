using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float moveSpeed = 3f;
    public int scoreValue = 10;
    public float lifetime = 10f;
    
    [Header("Movement")]
    public bool useCustomMovement = false;
    public AnimationCurve movementPattern;
    
    protected Vector3 initialPosition;
    protected float timeAlive;
    protected bool isInitialized;

    protected virtual void Start()
    {
        initialPosition = transform.position;
        isInitialized = true;
    }

    protected virtual void Update()
    {
        if (!isInitialized || GameManager.instance.currentState != GameManager.GameState.Playing)
            return;

        timeAlive += Time.deltaTime;
        
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Movement();
    }

    protected virtual void Movement()
    {
        if (useCustomMovement)
        {
            CustomMovement();
        }
        else
        {
            DefaultMovement();
        }
    }

    protected virtual void DefaultMovement()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    protected virtual void CustomMovement()
    {
        float normalizedTime = timeAlive / lifetime;
        float verticalOffset = movementPattern.Evaluate(normalizedTime);
        
        Vector3 newPosition = initialPosition;
        newPosition.x -= moveSpeed * timeAlive;
        newPosition.y += verticalOffset;
        
        transform.position = newPosition;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnHitPlayer();
        }
    }

    protected virtual void OnHitPlayer()
    {
        Destroy(gameObject);
    }
}