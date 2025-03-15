// Save as: Assets/Scripts/ScriptableObjects/EnemyWaveConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/Wave Configuration")]
public class EnemyWaveConfig : ScriptableObject
{
    public DifficultyWave[] waves;
}

[System.Serializable]
public class DifficultyWave
{
    public float startTime = 0f;
    public EnemyScriptableObject[] enemyTypes;
    public float spawnRate = 1f;
    public int maxSimultaneousEnemies = 5;
    public bool useRandomPosition = true;
    [Range(0f, 1f)] public float bulletProbability = 0.3f;
}