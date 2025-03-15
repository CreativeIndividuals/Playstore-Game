// Save as: Assets/Scripts/Threats/LaserBeam.cs
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("Laser Properties")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * (moveSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}