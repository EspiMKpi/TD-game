using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int damageToBase = 1;

    private Transform target;
    private int pathIndex = 0;

    private float slowMultiplier = 1f;
    private float slowTimer = 0f;
    public float SlowMultiplier => slowMultiplier;

    // Làm chậm (tháp Slow / đạn làm chậm): áp hệ số mạnh nhất, làm mới thời gian hiệu lực.
    public void ApplySlow(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp01(multiplier);
        if (slowTimer <= 0f || multiplier < slowMultiplier) slowMultiplier = multiplier;
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    public int PathIndex => pathIndex;

    // UC9 Portal — đẩy lùi địch 'steps' waypoint trên đường đi.
    public void PushBack(int steps)
    {
        pathIndex = Mathf.Max(0, pathIndex - steps);
        if (Level_Manager.main != null && Level_Manager.main.path != null
            && pathIndex < Level_Manager.main.path.Length)
        {
            target = Level_Manager.main.path[pathIndex];
        }
    }

    private void Start()
    {
        target = Level_Manager.main.path[pathIndex];
    }

    private void Update()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) slowMultiplier = 1f;
        }

        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (pathIndex >= Level_Manager.main.path.Length)
            {
                // Địch tới cuối đường: tấn công căn cứ (Enemy.attackBase -> Base.takeDamage).
                // Null-guard để scene chưa đặt Base vẫn chạy như cũ.
                if (Base.main != null)
                {
                    Base.main.takeDamage(damageToBase);
                }
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = Level_Manager.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position). normalized;

        rb.linearVelocity = direction * moveSpeed * slowMultiplier;
    }
}
