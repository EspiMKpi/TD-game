using UnityEngine;

public class TurretScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;

    [Header("Attribute")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float bps = 1f; //BulletPerSecond

    [Header("Tower Info")]
    [SerializeField] private TowerType towerType = TowerType.Single;
    [SerializeField] private int buildCost = 50;
    [SerializeField] private int upgradeCost = 30;
    [SerializeField] private int sellPrice = 25;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private int maxTargets = 3;   // số mục tiêu mỗi loạt bắn cho tháp Multi

    public TowerType Type => towerType;
    public int BuildCost => buildCost;
    public int UpgradeCost => upgradeCost;
    public int SellPrice => sellPrice;
    public int CurrentLevel => currentLevel;
    public bool IsMaxLevel => currentLevel >= maxLevel;

    private Transform target;
    private float timeUntilFire;

    private float fireRateMultiplier = 1f;
    private float boostTimer = 0f;
    public float FireRateMultiplier => fireRateMultiplier;

    // UC9 SpeedBoost — tăng tốc độ bắn tạm thời (lấy hệ số mạnh nhất, làm mới thời gian).
    public void BoostFireRate(float multiplier, float duration)
    {
        fireRateMultiplier = Mathf.Max(fireRateMultiplier, multiplier);
        boostTimer = Mathf.Max(boostTimer, duration);
    }

    // UC6 — nâng cấp tháp: tăng tầm bắn & tốc độ bắn, cập nhật chi phí nâng cấp/bán.
    public bool upgrade()
    {
        if (IsMaxLevel) return false;
        currentLevel++;
        targetingRange *= 1.15f;
        bps *= 1.2f;
        sellPrice += Mathf.RoundToInt(upgradeCost * 0.4f);
        upgradeCost = Mathf.RoundToInt(upgradeCost * 1.5f);
        return true;
    }

    private void Update()
    {
        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f) fireRateMultiplier = 1f;
        }

        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;

            if (timeUntilFire >= 1f / (bps * fireRateMultiplier))
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    private void Shoot()
    {
        if (towerType == TowerType.Multi)
        {
            // Bắn vào nhiều địch trong tầm (tối đa maxTargets).
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
            int count = 0;
            foreach (Collider2D e in enemies)
            {
                if (count >= maxTargets) break;
                FireBulletAt(e.transform);
                count++;
            }
        }
        else
        {
            // Single / Explosive / Slow: một viên vào mục tiêu chính (hiệu ứng do đạn quyết định).
            FireBulletAt(target);
        }
    }

    private void FireBulletAt(Transform t)
    {
        if (t == null) return;
        GameObject bulletObj = SimplePool.Get(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null) bulletScript.SetTarget(t);
    }
    private void FindTarget()
    {
        // BUG-05: dùng OverlapCircleAll (đúng ngữ nghĩa) và chọn địch GẦN NHẤT trong tầm,
        // thay cho CircleCastAll với hướng = vị trí + lấy hits[0] tùy ý.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
        float best = float.MaxValue;
        Transform nearest = null;
        foreach (Collider2D h in hits)
        {
            float d = ((Vector2)(h.transform.position - transform.position)).sqrMagnitude;
            if (d < best) { best = d; nearest = h.transform; }
        }
        target = nearest;
    }

    private bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // Dùng Gizmos (UnityEngine) thay cho Handles (UnityEditor) để không phụ thuộc
        // assembly editor — cho phép đưa code vào asmdef runtime sạch.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
