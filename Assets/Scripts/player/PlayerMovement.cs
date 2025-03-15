using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;
    public float rotationSpeed = 360f;
    
    [Header("Boundaries")]
    public float horizontalBoundary = 2.5f;
    public float verticalBoundary = 4.5f;
    
    [Header("Touch Settings")]
    public float touchRadius = 0.5f;
    public bool invertedControls = false;
    
    [Header("Visual Effects")]
    public TrailRenderer movementTrail;
    public ParticleSystem moveEffect;
    
    private Vector3 velocity;
    private Vector2 targetPosition;
    private Vector2 lastPosition;
    private float currentRotation;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;
        lastPosition = transform.position;
        
        if (movementTrail != null)
        {
            movementTrail.enabled = false;
        }
    }
    
    private void Update()
    {
        if (!GameManager.instance.isGameActive) return;
        
        HandleInput();
        UpdateMovement();
        UpdateRotation();
        UpdateEffects();
    }
    
    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(touch.position);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    EnableMovementEffects();
                    targetPosition = worldPoint;
                    break;
                    
                case TouchPhase.Moved:
                    targetPosition = worldPoint;
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    DisableMovementEffects();
                    break;
            }
        }
    }
    
    private void UpdateMovement()
    {
        // Clamp target position within boundaries
        targetPosition = new Vector2(
            Mathf.Clamp(targetPosition.x, -horizontalBoundary, horizontalBoundary),
            Mathf.Clamp(targetPosition.y, -verticalBoundary, verticalBoundary)
        );
        
        // Apply inverted controls if enabled
        if (invertedControls)
        {
            targetPosition.y = -targetPosition.y;
        }
        
        // Smooth movement
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
        
        // Update last position for rotation calculation
        lastPosition = transform.position;
    }
    
    private void UpdateRotation()
    {
        if (velocity.sqrMagnitude > 0.1f)
        {
            float targetRotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            currentRotation = Mathf.MoveTowardsAngle(
                currentRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }
    }
    
    private void EnableMovementEffects()
    {
        if (movementTrail != null)
        {
            movementTrail.enabled = true;
        }
        if (moveEffect != null)
        {
            moveEffect.Play();
        }
    }
    
    private void DisableMovementEffects()
    {
        if (movementTrail != null)
        {
            movementTrail.enabled = false;
        }
        if (moveEffect != null)
        {
            moveEffect.Stop();
        }
    }
    
    private void UpdateEffects()
    {
        if (moveEffect != null && velocity.sqrMagnitude > 0.1f)
        {
            var emission = moveEffect.emission;
            emission.rateOverTime = velocity.magnitude * 10f;
        }
    }
}