// Save as: Assets/Scripts/ScriptableObjects/EnemyScriptableObject.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy Data")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public Sprite enemySprite;
    public Color enemyColor = Color.white;
    public RuntimeAnimatorController animatorController;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float activeDuration = 5f;
    public AnimationCurve movementPattern = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 0)
    );
    
    [Header("Scale")]
    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one;
    public AnimationCurve scalePattern = new AnimationCurve(
        new Keyframe(0, 1),
        new Keyframe(1, 1)
    );
    
    [Header("Path")]
    public Vector2 entryOffset = new Vector2(0, 6f);
    public Vector2 exitOffset = new Vector2(0, -6f);
    public Vector2[] pathPoints;

    [Header("Combat")]
    public bool canShoot = false;
    public float shootInterval = 2f;
    public GameObject projectilePrefab;
    public float bulletSpeed = 5f;
}