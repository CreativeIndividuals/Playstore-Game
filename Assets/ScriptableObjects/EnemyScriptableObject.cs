using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/Enemy", order = 1)]
public class EnemyScriptableObject : ScriptableObject
{
    [System.Serializable]
    public class SessionStatistics
    {
        public string userLogin = "QAD6675";
        public string sessionDate = "2025-02-23 12:50:08";
        public int timesSpawned;
        public int timesDefeated;
        public float averageSurvivalTime;
        public float lastSpawnTime;
    }

    [Header("General Settings")]
    public string enemyName = "New Enemy";
    [Tooltip("Duration (in seconds) the enemy stays on screen before leaving.")]
    [Min(0.1f)]
    public float activeDuration = 3f;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the enemy moves. This multiplier affects how fast the enemy traverses the path.")]
    [Min(0.1f)]
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

    [Header("Visual Settings")]
    public Sprite enemySprite;
    public Color enemyColor = Color.white;
    public RuntimeAnimatorController animatorController;

    [Header("Session Statistics")]
    public SessionStatistics currentSessionStats = new SessionStatistics();

    private void OnEnable()
    {
        // Initialize session stats if needed
        if (currentSessionStats == null)
        {
            currentSessionStats = new SessionStatistics();
        }
    }

    public void UpdateSessionStats(bool wasDefeated = false, float survivalTime = 0f)
    {
        currentSessionStats.lastSpawnTime = Time.time;
        currentSessionStats.timesSpawned++;
        
        if (wasDefeated)
        {
            currentSessionStats.timesDefeated++;
            float totalTime = (currentSessionStats.averageSurvivalTime * (currentSessionStats.timesDefeated - 1)) + survivalTime;
            currentSessionStats.averageSurvivalTime = totalTime / currentSessionStats.timesDefeated;
        }

        // Log statistics
        Debug.Log($"Enemy {enemyName} Statistics:\n" +
                 $"User: {currentSessionStats.userLogin}\n" +
                 $"Session: {currentSessionStats.sessionDate}\n" +
                 $"Spawns: {currentSessionStats.timesSpawned}\n" +
                 $"Defeats: {currentSessionStats.timesDefeated}\n" +
                 $"Avg Survival: {currentSessionStats.averageSurvivalTime:F2}s");
    }

    private void OnValidate()
    {
        // Ensure minimum values
        activeDuration = Mathf.Max(0.1f, activeDuration);
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        
        // Validate curves
        if (movementPattern.length == 0)
        {
            movementPattern = AnimationCurve.Linear(0, 0, 1, 1);
        }
        if (scalePattern.length == 0)
        {
            scalePattern = AnimationCurve.Linear(0, 0, 1, 1);
        }
    }
}