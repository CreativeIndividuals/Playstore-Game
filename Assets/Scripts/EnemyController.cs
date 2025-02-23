using UnityEngine;
using System.Collections;
public class EnemyController : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    private Vector2[] _fullPath;

    private void Start()
    {
        // Build the full path: entry, then intermediate path points, then exit.
        int pathCount = enemyData.pathPoints?.Length ?? 0;
        _fullPath = new Vector2[pathCount + 2];
        _fullPath[0] = enemyData.entryOffset;
        for (int i = 0; i < pathCount; i++)
        {
            if (enemyData.pathPoints != null) _fullPath[i + 1] = enemyData.pathPoints[i];
        }
        _fullPath[^1] = enemyData.exitOffset;

        // Set initial position and scale.
        transform.position = new Vector3(_fullPath[0].x, _fullPath[0].y, transform.position.z);
        transform.localScale = enemyData.startScale;

        // Start the movement and scale animation.
        StartCoroutine(AnimateEnemy());
    }

    private IEnumerator AnimateEnemy()
    {
        float elapsed = 0f;
        while (elapsed < enemyData.activeDuration)
        {
            // Calculate normalized time (0 to 1) adjusted by moveSpeed.
            float t = (elapsed * enemyData.moveSpeed) / enemyData.activeDuration;
            t = Mathf.Clamp01(t);

            // Apply the movement AnimationCurve.
            float curveT = enemyData.movementPattern.Evaluate(t);
            Vector2 newPosition2D = EvaluatePath(_fullPath, curveT);
            transform.position = new Vector3(newPosition2D.x, newPosition2D.y, transform.position.z);

            // Apply the scale AnimationCurve.
            float scaleT = enemyData.scalePattern.Evaluate(t);
            transform.localScale = Vector3.Lerp(enemyData.startScale, enemyData.endScale, scaleT);

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the enemy ends exactly at the exit point and final scale.
        Vector2 exitPos = _fullPath[^1];
        transform.position = new Vector3(exitPos.x, exitPos.y, transform.position.z);
        transform.localScale = enemyData.endScale;

        // Optionally, destroy or deactivate the enemy after its animation is complete.
        Destroy(gameObject);
    }

    // Helper method: Given a path (array of Vector2) and normalized t (0 to 1), returns the interpolated position.
    private Vector2 EvaluatePath(Vector2[] path, float t)
    {
        if (path == null || path.Length == 0)
            return Vector2.zero;
        if (path.Length == 1)
            return path[0];

        int segmentCount = path.Length - 1;
        // Scale t to the number of segments.
        float scaledT = t * segmentCount;
        int currentSegment = Mathf.Min(Mathf.FloorToInt(scaledT), segmentCount - 1);
        float segmentT = scaledT - currentSegment;

        return Vector2.Lerp(path[currentSegment], path[currentSegment + 1], segmentT);
    }
}
