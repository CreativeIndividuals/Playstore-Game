using UnityEngine;

public class StandardEnemy : EnemyBase
{
    [Header("Standard Enemy Settings")]
    public float oscillationMagnitude = 2f;
    public float oscillationSpeed = 2f;

    protected override void CustomMovement()
    {
        float verticalMovement = Mathf.Sin(timeAlive * oscillationSpeed) * oscillationMagnitude;
        
        Vector3 newPosition = initialPosition;
        newPosition.x -= moveSpeed * timeAlive;
        newPosition.y += verticalMovement;
        
        transform.position = newPosition;
    }
}