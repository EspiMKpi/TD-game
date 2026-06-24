using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Trung tâm điều phối ván đấu (UC4–UC11) — Giai đoạn 2 kế hoạch.
//
// Refactor tại chỗ: GameSession tạm BỌC Level_Manager thay vì thay thế ngay.
// Currency vẫn do Level_Manager giữ (nguồn sự thật hiện tại) để không vỡ các
// tham chiếu Level_Manager.main đang dùng ở Plot / Health / Menu / EnemyMovement.
// GameSession ủy quyền qua các helper bên dưới; sẽ migrate dần các lời gọi trực
// tiếp sang GameSession ở những increment sau.
// DefaultExecutionOrder để Awake/Start chạy SAU Level_Manager/Base/EnemySpawner (order 0),
// đảm bảo ApplyLevel ghi đè được giá trị mặc định của chúng.
[DefaultExecutionOrder(100)]
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    private Level currentLevel;

    [Header("References")]
    public Base theBase;                 // hợp thành 1-1 (tự tìm qua Base.main nếu để trống)

    [Header("PowerUp")]
    public List<PowerUp> powerUps = new List<PowerUp>();   // kết tập 1-n
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private float powerUpEffectLifetime = 2f;   // VFX placeholder: thời gian sống của hiệu ứng kỹ năng

    [Header("Run state")]
    [SerializeField] private int currentWaveIndex;
    [SerializeField] private int score;
    [SerializeField] private int stars;

    [Header("Events")]
    public UnityEvent onStatusChanged = new UnityEvent();

    // 'status' theo thiết kế, đổi boolean -> enum (mục 2.3 kế hoạch).
    public GameStatus status { get; private set; } = GameStatus.Playing;

    public int CurrentWaveIndex => currentWaveIndex;
    public int Score => score;
    public int Stars => stars;

    // Ủy quyền tài nguyên cho Level_Manager (nguồn sự thật hiện tại).
    public int currentResources => Level_Manager.main != null ? Level_Manager.main.currency : 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Base.main đã được gán trong Base.Awake (chạy trước Start), nên không cần FindObjectOfType.
        if (theBase == null) theBase = Base.main;

        // Áp Level đã chọn từ menu (Giai đoạn 7). Null nếu chơi thẳng scene gameplay -> dùng mặc định.
        ApplyLevel(GameFlow.SelectedLevel);

        if (theBase != null)
        {
            theBase.onBaseDestroyed.AddListener(OnBaseDestroyed);
        }
        else
        {
            Debug.LogWarning("GameSession: chưa có Base trong scene — điều kiện thua đang bị tắt.");
        }
    }

    private void ApplyLevel(Level lvl)
    {
        currentLevel = lvl;
        if (lvl == null) return;
        if (Level_Manager.main != null) Level_Manager.main.currency = lvl.initialResources;
        if (theBase != null) theBase.Initialize(lvl.baseMaxHP);
        if (EnemySpawner.main != null) EnemySpawner.main.SetLevel(lvl);
    }

    // Số sao theo tỉ lệ máu căn cứ còn lại khi thắng.
    private int ComputeStars()
    {
        if (theBase == null || theBase.MaxHP <= 0) return 1;
        float r = (float)theBase.CurrentHP / theBase.MaxHP;
        if (r >= 0.99f) return 3;
        if (r >= 0.5f) return 2;
        return 1;
    }

    // UC11 — thua khi căn cứ bị phá.
    public void OnBaseDestroyed()
    {
        SetStatus(GameStatus.Lost);
        Debug.Log("GameSession: status = Lost");
        // TODO Giai đoạn 6: mở ResultView.
    }

    // Gọi khi qua wave cuối — sẽ nối ở Giai đoạn 6 khi EnemySpawner biết tổng số wave (từ Level).
    public void OnAllWavesCleared()
    {
        if (status != GameStatus.Playing) return;
        stars = ComputeStars();
        SetStatus(GameStatus.Won);
        Debug.Log("GameSession: status = Won (stars=" + stars + ")");
        if (currentLevel != null)
        {
            currentLevel.saveResult(score, stars);
            currentLevel.unlockNext();   // Giai đoạn 7: mở khóa màn kế khi thắng
        }
    }

    public void SetCurrentWaveIndex(int index)
    {
        currentWaveIndex = index;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    // Helper tài nguyên (ủy quyền Level_Manager) — để dần thay các chỗ gọi trực tiếp.
    public bool SpendResources(int amount)
    {
        return Level_Manager.main != null && Level_Manager.main.SpendCurrency(amount);
    }

    public void AddResources(int amount)
    {
        if (Level_Manager.main != null) Level_Manager.main.IncreaseCurrency(amount);
    }

    // UC5 — xây tháp: kiểm tra đủ tài nguyên rồi trừ tiền và đặt tháp.
    // Dùng đối tượng dữ liệu Tower (đã có prefab + cost) thay cho TowerType,
    // vì BuildManager đang giữ sẵn Tower data — sát thực tế hơn.
    public GameObject buildTower(Tower towerData, Vector3 position)
    {
        if (towerData == null || towerData.prefab == null) return null;
        if (currentResources < towerData.cost)
        {
            Debug.Log("Không đủ tài nguyên để xây tháp.");
            return null;
        }
        if (!SpendResources(towerData.cost)) return null;
        return Instantiate(towerData.prefab, position, Quaternion.identity);
    }

    // UC6 — nâng cấp tháp: chặn khi đạt cấp tối đa hoặc thiếu tiền.
    public bool upgradeTower(TurretScript tower)
    {
        if (tower == null || tower.IsMaxLevel) return false;
        int cost = tower.UpgradeCost;
        if (currentResources < cost) return false;
        if (!tower.upgrade()) return false;
        SpendResources(cost);
        return true;
    }

    // UC7 — bán tháp: hoàn sellPrice và gỡ tháp khỏi scene.
    public void sellTower(TurretScript tower)
    {
        if (tower == null) return;
        AddResources(tower.SellPrice);
        Destroy(tower.gameObject);
    }

    // UC9 — kích hoạt kỹ năng bổ trợ. Ngoại lệ: đang hồi chiêu / không đủ tài nguyên.
    public bool activatePowerUp(PowerUp powerUp, Vector3 targetPosition)
    {
        if (powerUp == null) return false;
        if (!powerUp.IsReady())
        {
            Debug.Log("Power-up đang hồi chiêu.");
            return false;
        }
        if (currentResources < powerUp.resourceCost)
        {
            Debug.Log("Không đủ tài nguyên cho power-up.");
            return false;
        }

        SpendResources(powerUp.resourceCost);
        powerUp.activate(targetPosition);
        SpawnPowerUpEffect(powerUp, targetPosition);

        switch (powerUp.type)
        {
            case PowerUpType.Portal:     ApplyPortal(powerUp, targetPosition); break;
            case PowerUpType.Airstrike:  ApplyAirstrike(powerUp, targetPosition); break;
            case PowerUpType.SpeedBoost: ApplySpeedBoost(powerUp, targetPosition); break;
        }
        return true;
    }

    // VFX: sinh hiệu ứng kỹ năng tại điểm nhắm (nếu PowerUp có gắn prefab) rồi tự hủy.
    private void SpawnPowerUpEffect(PowerUp powerUp, Vector3 pos)
    {
        if (powerUp.effectPrefab == null) return;
        GameObject fx = Instantiate(powerUp.effectPrefab, pos, Quaternion.identity);
        if (powerUpEffectLifetime > 0f) Destroy(fx, powerUpEffectLifetime);
    }

    // Portal — đẩy lùi địch trong bán kính trên đường đi (điểm nhấn Quantall).
    private void ApplyPortal(PowerUp p, Vector3 pos)
    {
        foreach (Collider2D h in Physics2D.OverlapCircleAll(pos, p.effectRadius, enemyMask))
        {
            EnemyMovement move = h.GetComponentInParent<EnemyMovement>();
            if (move != null) move.PushBack(p.effectPower);
        }
    }

    // Airstrike — sát thương diện rộng các địch trong bán kính.
    private void ApplyAirstrike(PowerUp p, Vector3 pos)
    {
        foreach (Collider2D h in Physics2D.OverlapCircleAll(pos, p.effectRadius, enemyMask))
        {
            Health health = h.GetComponentInParent<Health>();
            if (health != null) health.TakeDamage(p.effectPower);
        }
    }

    // SpeedBoost — buff tốc độ bắn các tháp trong bán kính trong effectDuration.
    // Tháp không có Collider2D (không cần physics) -> quét trực tiếp theo khoảng cách,
    // không phụ thuộc towerMask.
    private void ApplySpeedBoost(PowerUp p, Vector3 pos)
    {
        float sqr = p.effectRadius * p.effectRadius;
        foreach (TurretScript t in FindObjectsByType<TurretScript>(FindObjectsSortMode.None))
        {
            if (((Vector2)(t.transform.position - pos)).sqrMagnitude <= sqr)
                t.BoostFireRate(2f, p.effectDuration);
        }
    }

    // UC10 — tạm dừng / tiếp tục.
    public void pause() { Time.timeScale = 0f; }
    public void resume() { Time.timeScale = 1f; }

    private void SetStatus(GameStatus newStatus)
    {
        if (status == newStatus) return;
        status = newStatus;
        onStatusChanged.Invoke();
    }
}
