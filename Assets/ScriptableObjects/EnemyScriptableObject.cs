using UnityEngine;

[CreateAssetMenu(fileName = "New2DEnemy", menuName = "ScriptableObjects/2DEnemy", order = 1)]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("General Settings")]
    public string enemyName = "New Enemy";
    [Tooltip("Duration (in seconds) the enemy stays on screen before leaving.")]
    public float activeDuration = 3f;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the enemy moves. This multiplier affects how fast the enemy traverses the path.")]
    public float moveSpeed = 1f;
    [Tooltip("Starting position offset when entering the screen.")]
    public Vector2 entryOffset;
    [Tooltip("Exit position offset for when the enemy leaves.")]
    public Vector2 exitOffset;

    [Header("Path Settings")]
    [Tooltip("Points (in 2D space) the enemy will follow between entry and exit.")]
    public Vector2[] pathPoints;
    
    [Header("Animation Settings")]
    [Tooltip("Curve to control the enemy's movement progression. X-axis: normalized time (0 to 1), Y-axis: progression factor.")]
    public AnimationCurve movementPattern = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Scale Settings")]
    [Tooltip("Starting scale of the enemy.")]
    public Vector3 startScale = Vector3.one;
    [Tooltip("Ending scale of the enemy when leaving.")]
    public Vector3 endScale = Vector3.one;
    [Tooltip("Curve to control the enemy's scale progression. X-axis: normalized time (0 to 1), Y-axis: progression factor.")]
    public AnimationCurve scalePattern = AnimationCurve.Linear(0, 0, 1, 1);
}