using NUnit.Framework;
using UnityEngine;

// EditMode tests cho kỹ năng bổ trợ (UC9) phần logic thuần.
public class PowerUpTests
{
    [Test]
    public void PowerUp_Cooldown_ReadyInitially_NotReadyAfterUse()
    {
        var p = ScriptableObject.CreateInstance<PowerUp>();
        p.cooldown = 100f;
        Assert.IsTrue(p.IsReady(), "chưa dùng -> sẵn sàng");
        p.activate(Vector3.zero);
        Assert.IsFalse(p.IsReady(), "vừa dùng -> đang hồi chiêu");
        Object.DestroyImmediate(p);
    }

    [Test]
    public void Turret_BoostFireRate_TakesStrongestMultiplier()
    {
        var go = new GameObject("Tur");
        var t = go.AddComponent<TurretScript>();
        Assert.AreEqual(1f, t.FireRateMultiplier, "mặc định không buff");
        t.BoostFireRate(2f, 5f);
        Assert.AreEqual(2f, t.FireRateMultiplier, "buff x2");
        t.BoostFireRate(1.5f, 5f);
        Assert.AreEqual(2f, t.FireRateMultiplier, "buff yếu hơn không hạ multiplier hiện tại");
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Enemy_PushBack_ClampsAtZero_NoThrow()
    {
        var go = new GameObject("E");
        var m = go.AddComponent<EnemyMovement>();
        Assert.AreEqual(0, m.PathIndex);
        m.PushBack(3);                       // pathIndex=0 -> max(0,-3)=0, không lỗi
        Assert.AreEqual(0, m.PathIndex);
        Object.DestroyImmediate(go);
    }
}
