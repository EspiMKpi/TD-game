using NUnit.Framework;
using UnityEngine;

// EditMode tests cho cơ chế làm chậm của địch (tháp Slow / đạn làm chậm).
// ApplySlow không cần Awake; field init (slowMultiplier=1, slowTimer=0) chạy khi AddComponent.
// Update không chạy trong edit mode nên slowTimer giữ nguyên -> kiểm được luật ghi đè.
public class EnemyTests
{
    [Test]
    public void ApplySlow_SetsMultiplier()
    {
        var go = new GameObject("E");
        var m = go.AddComponent<EnemyMovement>();
        Assert.AreEqual(1f, m.SlowMultiplier);
        m.ApplySlow(0.5f, 2f);
        Assert.AreEqual(0.5f, m.SlowMultiplier);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ApplySlow_StrongerOverrides_WeakerIgnoredWhileActive()
    {
        var go = new GameObject("E");
        var m = go.AddComponent<EnemyMovement>();
        m.ApplySlow(0.6f, 2f);
        m.ApplySlow(0.3f, 2f);   // mạnh hơn (0.3 < 0.6) -> ghi đè
        Assert.AreEqual(0.3f, m.SlowMultiplier);
        m.ApplySlow(0.8f, 2f);   // yếu hơn trong khi đang slow -> giữ nguyên
        Assert.AreEqual(0.3f, m.SlowMultiplier);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ApplySlow_ClampsToZeroOne()
    {
        var go = new GameObject("E");
        var m = go.AddComponent<EnemyMovement>();
        m.ApplySlow(-5f, 2f);    // clamp về 0
        Assert.AreEqual(0f, m.SlowMultiplier);
        Object.DestroyImmediate(go);
    }
}
