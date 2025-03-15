// Save as: Assets/Scripts/Enemies/StandardEnemy.cs
using UnityEngine;

public class StandardEnemy : EnemyBase
{
    [Header("Movement Settings")]
    public float horizontalSpeed = 5f;
    public float verticalSpeed = 2f;
    public float amplitude = 1f;
    
    protected override void HandleMovement()
    {
        // Basic sine wave movement
        float newY = initialPosition.y + Mathf.Sin(timeAlive * verticalSpeed) * amplitude;
        float newX = initialPosition.x - horizontalSpeed * Time.deltaTime;
        
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}