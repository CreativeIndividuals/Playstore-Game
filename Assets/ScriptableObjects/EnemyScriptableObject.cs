// Save as: Assets/Scripts/ScriptableObjects/EnemyScriptableObject.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Sprite enemySprite;
    public Color enemyColor = Color.white;
    public RuntimeAnimatorController animatorController;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float activeDuration = 5f;
    public bool useCustomPath = false;
    public AnimationCurve movementPattern = AnimationCurve.Linear(0, 0, 1, 0);
    
    [Header("Path")]
    public Vector2[] pathPoints;
    public Vector2 entryOffset = new Vector2(0, 6f);
    public Vector2 exitOffset = new Vector2(0, -6f);

    [Header("Combat")]
    public bool canShoot = false;
    public float shootInterval = 2f;
    public GameObject projectilePrefab;
    public float bulletSpeed = 10f; 
    public float projectileSpeed = 8f;
    [Range(0f, 1f)] public float shootProbability = 0.3f;

    [Header("Scaling")]
    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one;
    public AnimationCurve scalePattern = AnimationCurve.Linear(0, 1, 1, 1);
}