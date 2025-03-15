// Save as: Assets/Scripts/Threats/ThreatManager.cs
using UnityEngine;

public class ThreatManager : MonoBehaviour
{
    [Header("Threat Prefabs")]
    [SerializeField] private GameObject laserBeamPrefab;
    [SerializeField] private GameObject homingMissilePrefab;

    [Header("Threat Properties")]
    [SerializeField] private float missileSpeed = 5f;
    [SerializeField] private float laserSpeed = 8f;

    public void SpawnLaserBeam(Vector3 position, Quaternion rotation)
    {
        GameObject laser = Instantiate(laserBeamPrefab, position, rotation);
        LaserBeam laserComponent = laser.GetComponent<LaserBeam>();
        if (laserComponent != null)
        {
            // Additional initialization if needed
        }
    }

    public void SpawnHomingMissile(Vector3 position)
    {
        GameObject missile = Instantiate(homingMissilePrefab, position, Quaternion.identity);
        HomingMissile missileComponent = missile.GetComponent<HomingMissile>();
        if (missileComponent != null)
        {
            missileComponent.Initialize(missileSpeed);
        }
    }
}