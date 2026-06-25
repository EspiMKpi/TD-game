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

    [Header("Swarm")]
    [Tooltip("Độ tản ngang tối đa hai bên đường đi để di chuyển như bầy (0 = đi thẳng hàng như cũ).")]
    [SerializeField] private float swarmSpread = 0.5f;
    private float laneAmount = 0f;                // độ lệch ngang có dấu, riêng mỗi địch (|.| <= swarmSpread)
    private Vector2 laneOffset = Vector2.zero;    // độ lệch vuông góc với đoạn đường hiện tại

    // Điểm nhắm thực tế = waypoint hiện tại cộng độ lệch ngang (vuông góc hướng đi).
    private Vector2 AimPoint => (Vector2)target.position + laneOffset;

    // Tính lại độ lệch sao cho LUÔN vuông góc với đoạn đường hiện tại -> bầy tản ngang
    // theo bề rộng đường, không lấn sang ô đặt tháp (Plot) khi đường rẽ.
    private void RecomputeLaneOffset()
    {
        if (target == null) { laneOffset = Vector2.zero; return; }
        Vector2 dir = (Vector2)target.position - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.0001f) { laneOffset = Vector2.zero; return; }
        dir.Normalize();
        laneOffset = new Vector2(-dir.y, dir.x) * laneAmount;   // pháp tuyến * biên độ
    }

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
        laneAmount = Random.Range(-swarmSpread, swarmSpread);   // biên độ lệch ngang mới mỗi lần lấy từ pool
        if (Level_Manager.main != null && Level_Manager.main.path != null && Level_Manager.main.path.Length > 0)
            target = Level_Manager.main.path[0];
        RecomputeLaneOffset();
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
            RecomputeLaneOffset();
        }
    }

    private void Start()
    {
        // BUG-06: null-guard cho path.
        if (Level_Manager.main != null && Level_Manager.main.path != null
            && pathIndex < Level_Manager.main.path.Length)
            target = Level_Manager.main.path[pathIndex];
        RecomputeLaneOffset();
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

        // So với điểm nhắm có độ lệch bầy để waypoint cuối vẫn kích hoạt tấn công căn cứ.
        if (Vector2.Distance(AimPoint, transform.position) <= 0.1f)
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
                RecomputeLaneOffset();
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;   // BUG-06
        Vector2 direction = (AimPoint - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * moveSpeed * slowMultiplier;
    }
}
