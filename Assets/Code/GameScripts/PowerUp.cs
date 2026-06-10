using System.Collections.Generic;
using UnityEngine;

// Kỹ năng bổ trợ (UC9) — ScriptableObject dữ liệu. Hiệu ứng spatial do
// GameSession.activatePowerUp thực thi (cần truy cập Physics2D scene), đúng thiết kế.
[CreateAssetMenu(menuName = "Quantall/PowerUp")]
public class PowerUp : ScriptableObject
{
    public string powerUpName;     // 'name' trong thiết kế
    public PowerUpType type;       // Portal / Airstrike / SpeedBoost
    public int resourceCost;
    public float cooldown;
    public float effectRadius;
    public float effectDuration;
    public int effectPower = 1;    // Airstrike: sát thương; Portal: số waypoint lùi lại
    public Color tintColor = Color.white;   // màu chỉ báo tầm (vòng tròn trong suốt)

    // Theo thiết kế: kết tập các đối tượng chịu tác động (Tower data class -> TurretScript thực thể;
    // Enemy -> EnemyMovement, do lớp Enemy chưa tách riêng).
    [System.NonSerialized] public List<TurretScript> affectedTowers = new List<TurretScript>();
    [System.NonSerialized] public List<EnemyMovement> affectedEnemies = new List<EnemyMovement>();

    [System.NonSerialized] private float lastUsedTime = -9999f;

    public bool IsReady() => Time.time - lastUsedTime >= cooldown;

    // Giữ tên 'activate' theo thiết kế; tại đây chỉ ghi nhận thời điểm dùng (cooldown).
    // GameSession.activatePowerUp thực thi hiệu ứng theo 'type'.
    public void activate(Vector3 targetPosition)
    {
        lastUsedTime = Time.time;
    }
}
