using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public float rotationSpeed = 200f;
    private float speed;
    private Transform target;
    private Rigidbody2D rb;

    public void Initialize(float missileSpeed)
    {
        speed = missileSpeed;
        rb = GetComponent<Rigidbody2D>();
        target = FindObjectOfType<PlayerController>()?.transform;
    }

    private void FixedUpdate()
    {
        if (target == null || GameManager.instance.currentState != GameManager.GameState.Playing)
            return;

        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.right).z;
        rb.angularVelocity = -rotateAmount * rotationSpeed;
        rb.velocity = transform.right * speed;
    }
}