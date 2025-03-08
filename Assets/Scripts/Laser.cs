using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Laser Properties")]
    public float warmupTime = 0.5f;
    public float activeTime = 0.3f;
    public float damage = 100f;
    public Color warmupColor = new Color(1f, 0f, 0f, 0.5f);
    public Color activeColor = Color.red;
    
    [Header("Components")]
    public LineRenderer laserLine;
    public GameObject warningLine;
    
    private float timer;
    private bool isActive;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private BoxCollider2D laserCollider;

    private void Awake()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        if (laserCollider == null)
            laserCollider = gameObject.AddComponent<BoxCollider2D>();
        
        laserCollider.isTrigger = true;
        laserCollider.enabled = false;
        
        // Initialize line renderer if not set
        if (laserLine == null)
            laserLine = GetComponent<LineRenderer>();
        
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        laserLine.startWidth = 0.1f;
        laserLine.endWidth = 0.1f;
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = warmupColor;
        laserLine.endColor = warmupColor;
    }

    public void InitializeLaser(Vector2 start, Vector2 end)
    {
        startPoint = start;
        endPoint = end;
        transform.position = start;
        
        // Set up line renderer positions
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        
        // Set up collider
        Vector2 midPoint = (start + end) / 2f;
        transform.position = midPoint;
        
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        float length = Vector2.Distance(start, end);
        laserCollider.size = new Vector2(length, 0.1f);
        laserCollider.offset = Vector2.zero;
        
        // Show warning line
        if (warningLine != null)
        {
            warningLine.SetActive(true);
            warningLine.transform.position = midPoint;
            warningLine.transform.rotation = transform.rotation;
            warningLine.transform.localScale = new Vector3(length, 1f, 1f);
        }
        
        timer = 0f;
        isActive = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (!isActive && timer >= warmupTime)
        {
            ActivateLaser();
        }
        else if (isActive && timer >= warmupTime + activeTime)
        {
            Destroy(gameObject);
            return;
        }
        
        // Update warning line alpha during warmup
        if (!isActive && warningLine != null)
        {
            float alpha = Mathf.PingPong(timer * 4f, 1f);
            Color warningColor = warmupColor;
            warningColor.a = alpha;
            warningLine.GetComponent<SpriteRenderer>().color = warningColor;
        }
    }

    private void ActivateLaser()
    {
        isActive = true;
        laserCollider.enabled = true;
        laserLine.startColor = activeColor;
        laserLine.endColor = activeColor;
        
        if (warningLine != null)
            warningLine.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage * Time.deltaTime);
            }
        }
    }
}