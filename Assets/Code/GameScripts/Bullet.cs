using UnityEngine;

public class Bullet : MonoBehaviour
{
   [Header("References")]
   [SerializeField] private Rigidbody2D rb;

   [Header("Attribute")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Projectile Type")]
    [SerializeField] private ProjectileType projectileType = ProjectileType.Single;
    [SerializeField] private float explosionRadius = 1.5f;   // chỉ dùng cho loại Explosive
    [SerializeField] private LayerMask enemyMask;            // lớp địch để quét AoE

    [Header("Slow (tuỳ chọn — cho đạn tháp Slow)")]
    [SerializeField] private bool appliesSlow = false;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 2f;

   private Transform target;
   private float releaseTime;   // Giai đoạn 8: thời điểm tự trả về pool (thay Destroy hẹn giờ)
   private Vector3 lastTargetPos;   // điểm tới gần nhất (homing) — dùng khi mục tiêu chết giữa đường
   private bool hasLastPos;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    // Giai đoạn 8: reset khi lấy từ pool. target được SetTarget gán ngay sau Get.
    private void OnEnable()
    {
        target = null;
        hasLastPos = false;
        releaseTime = Time.time + bulletLifetime;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void FixedUpdate()
    {
        if (Time.time >= releaseTime)
        {
            SimplePool.Release(gameObject);
            return;
        }

        // Địch còn sống? Pooling chỉ SetActive(false) -> phải kiểm activeInHierarchy,
        // không chỉ null, nếu không đạn sẽ bám mãi vào Transform "đóng băng" của địch đã chết.
        bool alive = target != null && target.gameObject.activeInHierarchy;
        if (alive)
        {
            lastTargetPos = target.position;   // homing: luôn cập nhật điểm tới
            hasLastPos = true;
        }
        else if (!hasLastPos)
        {
            SimplePool.Release(gameObject);    // chưa từng có mục tiêu hợp lệ -> biến mất
            return;
        }
        else if (Vector2.Distance(transform.position, lastTargetPos) <= 0.15f)
        {
            SimplePool.Release(gameObject);    // mục tiêu đã chết: bay tới điểm va chạm dự kiến -> biến mất
            return;
        }

        Vector2 direction = ((Vector2)lastTargetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        TryHitTarget(other.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitTarget(other);
    }

    private void TryHitTarget(Collider2D other)
    {
        if (!IsTargetCollider(other))
        {
            return;
        }

        Detonate();
        SimplePool.Release(gameObject);
    }

    // Single/Slow: chỉ trúng mục tiêu. Explosive: quét OverlapCircle gây sát thương diện rộng.
    private void Detonate()
    {
        if (projectileType == ProjectileType.Explosive)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyMask);
            foreach (Collider2D hit in hits)
            {
                ApplyHit(hit.transform);
            }
        }
        else
        {
            ApplyHit(target);
        }
    }

    private void ApplyHit(Transform enemy)
    {
        if (enemy == null) return;

        Health health = enemy.GetComponentInParent<Health>();
        if (health != null) health.TakeDamage(bulletDamage);

        if (appliesSlow)
        {
            EnemyMovement move = enemy.GetComponentInParent<EnemyMovement>();
            if (move != null) move.ApplySlow(slowMultiplier, slowDuration);
        }
    }

    private bool IsTargetCollider(Collider2D other)
    {
        if (!target || other == null)
        {
            return false;
        }

        if (other.transform == target)
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.transform == target)
        {
            return true;
        }

        return other.transform.root == target;
    }

}
