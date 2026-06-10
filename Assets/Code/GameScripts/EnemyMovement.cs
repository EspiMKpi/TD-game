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

    private int baseDamageToBase = -1;   // sát thương gốc (cache) cho cân bằng độ khó
    public int DamageToBase => damageToBase;

    private void Awake()
    {
        if (baseDamageToBase < 0) baseDamageToBase = damageToBase;
    }

    // Giai đoạn 8: reset trạng thái khi lấy lại từ pool.
    private void OnEnable()
    {
        pathIndex = 0;
        slowMultiplier = 1f;
        slowTimer = 0f;
        if (Level_Manager.main != null && Level_Manager.main.path != null && Level_Manager.main.path.Length > 0)
            target = Level_Manager.main.path[0];
    }

    // Giai đoạn 8: nhân sát thương lên căn cứ theo độ khó (dựa trên giá trị gốc).
    public void ApplyDifficulty(float multiplier)
    {
        if (baseDamageToBase < 0) baseDamageToBase = damageToBase;
        damageToBase = Mathf.Max(1, Mathf.RoundToInt(baseDamageToBase * multiplier));
    }

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
        // BUG-06: null-guard cho path.
        if (Level_Manager.main != null && Level_Manager.main.path != null
            && pathIndex < Level_Manager.main.path.Length)
            target = Level_Manager.main.path[pathIndex];
    }

    private void Update()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) slowMultiplier = 1f;
        }

        // BUG-06: null-guard cho target/path (tránh NRE nếu chưa khởi tạo đường đi).
        if (target == null || Level_Manager.main == null || Level_Manager.main.path == null) return;

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
                SimplePool.Release(gameObject);   // Giai đoạn 8: trả về pool
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
        if (target == null) return;   // BUG-06
        Vector2 direction = (target.position - transform.position). normalized;

        rb.linearVelocity = direction * moveSpeed * slowMultiplier;
    }
}
