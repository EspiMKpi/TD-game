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

    // Giai đoạn 6: số lính lấy theo Level.waves nếu có, nếu không theo công thức scaling.
    [Test]
    public void EnemySpawner_PlannedEnemies_UsesWaveConfigElseFormula()
    {
        var go = new GameObject("ES");
        var es = go.AddComponent<EnemySpawner>();

        // Chưa có Level -> công thức baseEnemies(8) * pow(wave, 0.75)
        Assert.AreEqual(8, es.PlannedEnemiesForWave(1));
        Assert.AreEqual(Mathf.RoundToInt(8f * Mathf.Pow(2f, 0.75f)), es.PlannedEnemiesForWave(2));

        // Có Level: wave[0].enemyCount=3 -> dùng cấu hình cho wave 1, công thức cho wave 2 (chưa cấu hình)
        var lvl = ScriptableObject.CreateInstance<Level>();
        lvl.waves.Add(new Wave { waveIndex = 1, enemyCount = 3, spawnRate = 1f });
        es.SetLevel(lvl);
        Assert.AreEqual(3, es.PlannedEnemiesForWave(1));
        Assert.AreEqual(Mathf.RoundToInt(8f * Mathf.Pow(2f, 0.75f)), es.PlannedEnemiesForWave(2));

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(lvl);
    }

    // Giai đoạn 8: máu lính nhân theo độ khó, dựa trên máu gốc (không cộng dồn).
    [Test]
    public void Health_ApplyDifficulty_ScalesFromBase()
    {
        var go = new GameObject("H");
        var h = go.AddComponent<Health>();   // hitPoints mặc định = 2
        Assert.AreEqual(2, h.Hp);
        h.ApplyDifficulty(2f);
        Assert.AreEqual(4, h.Hp);
        h.ApplyDifficulty(3f);               // theo gốc (2) -> 6, không phải 12
        Assert.AreEqual(6, h.Hp);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void EnemyMovement_ApplyDifficulty_ScalesDamage()
    {
        var go = new GameObject("E");
        var m = go.AddComponent<EnemyMovement>();   // damageToBase mặc định = 1
        Assert.AreEqual(1, m.DamageToBase);
        m.ApplyDifficulty(3f);
        Assert.AreEqual(3, m.DamageToBase);
        Object.DestroyImmediate(go);
    }
}
