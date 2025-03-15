// Save as: Assets/Scripts/CameraShake.cs
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    private Vector3 originalPosition;
    private float traumaExponent = 1;
    private float trauma;
    private float recoverySpeed = 1f;

    private void Awake()
    {
        Instance = this;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (trauma > 0)
        {
            float shake = Mathf.Pow(trauma, traumaExponent);
            transform.localPosition = originalPosition + Random.insideUnitSphere * shake * 0.5f;
            trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    public void Shake(float amount, float length)
    {
        trauma = Mathf.Clamp01(trauma + amount);
        recoverySpeed = 1f / length;
    }
}