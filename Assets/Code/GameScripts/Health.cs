using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoints = 2;
    [SerializeField] private int currencyWorth = 10;

    [Header("VFX (placeholder — gắn prefab hiệu ứng chết thật vào đây sau)")]
    [SerializeField] private GameObject deathEffectPrefab;   // để trống = không có hiệu ứng, game chạy như cũ
    [SerializeField] private float deathEffectLifetime = 2f;

    [Header("Hồi máu (0 = không hồi)")]
    [SerializeField] private float regenPerSecond = 0f;   // máu/giây tự hồi, trần là HP đầy hiện tại

    private bool isDestroyed = false;
    private int maxHitPoints = -1;   // máu gốc (cache) cho cân bằng độ khó & tái dùng pool
    private int currentMaxHp = -1;   // trần hồi máu (= HP đầy sau khi áp độ khó)
    private float regenAccumulator = 0f;

    public int Hp => hitPoints;

    private void Awake()
    {
        if (maxHitPoints < 0) maxHitPoints = hitPoints;
    }

    // Giai đoạn 8: reset khi lấy lại từ pool (tái dùng).
    private void OnEnable()
    {
        if (maxHitPoints < 0) maxHitPoints = hitPoints;
        hitPoints = maxHitPoints;
        currentMaxHp = maxHitPoints;
        regenAccumulator = 0f;
        isDestroyed = false;
    }

    // Hồi máu dần (chỉ chạy nếu regenPerSecond > 0), không vượt HP đầy hiện tại.
    private void Update()
    {
        if (regenPerSecond <= 0f || isDestroyed) return;
        if (currentMaxHp < 0) currentMaxHp = maxHitPoints;
        if (hitPoints >= currentMaxHp) { regenAccumulator = 0f; return; }

        regenAccumulator += regenPerSecond * Time.deltaTime;
        if (regenAccumulator >= 1f)
        {
            int heal = Mathf.FloorToInt(regenAccumulator);
            regenAccumulator -= heal;
            hitPoints = Mathf.Min(currentMaxHp, hitPoints + heal);
        }
    }

    // Giai đoạn 8: nhân máu theo độ khó của wave (dựa trên máu gốc, không cộng dồn).
    public void ApplyDifficulty(float multiplier)
    {
        if (maxHitPoints < 0) maxHitPoints = hitPoints;
        hitPoints = Mathf.Max(1, Mathf.RoundToInt(maxHitPoints * multiplier));
        currentMaxHp = hitPoints;   // trần hồi máu = HP đầy sau khi áp độ khó
    }

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            Level_Manager.main.IncreaseCurrency(currencyWorth);
            if (GameSession.Instance != null) GameSession.Instance.AddScore(currencyWorth);
            isDestroyed = true;
            SpawnDeathEffect();
            SimplePool.Release(gameObject);   // Giai đoạn 8: trả về pool thay vì Destroy
        }
    }

    // VFX: sinh hiệu ứng chết (nếu có gắn prefab) rồi tự hủy sau deathEffectLifetime.
    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null) return;
        GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        if (deathEffectLifetime > 0f) Destroy(fx, deathEffectLifetime);
    }
}
