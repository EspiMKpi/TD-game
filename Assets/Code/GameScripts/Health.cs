using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoints = 2;
    [SerializeField] private int currencyWorth = 10;

    [Header("VFX (placeholder — gắn prefab hiệu ứng chết thật vào đây sau)")]
    [SerializeField] private GameObject deathEffectPrefab;   // để trống = không có hiệu ứng, game chạy như cũ
    [SerializeField] private float deathEffectLifetime = 2f;

    private bool isDestroyed = false;
    private int maxHitPoints = -1;   // máu gốc (cache) cho cân bằng độ khó & tái dùng pool

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
        isDestroyed = false;
    }

    // Giai đoạn 8: nhân máu theo độ khó của wave (dựa trên máu gốc, không cộng dồn).
    public void ApplyDifficulty(float multiplier)
    {
        if (maxHitPoints < 0) maxHitPoints = hitPoints;
        hitPoints = Mathf.Max(1, Mathf.RoundToInt(maxHitPoints * multiplier));
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
