// Save as: Assets/Scripts/ScriptableObjects/EnemyScriptableObject.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Game/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public Color enemyColor = Color.white;
    public float health = 100f;
    public float moveSpeed = 5f;
    public float damage = 10f;
}