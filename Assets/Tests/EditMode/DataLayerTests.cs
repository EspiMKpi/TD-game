using NUnit.Framework;
using UnityEngine;

// EditMode tests cho lớp dữ liệu thuần (Giai đoạn 0): Tower, Settings, Level, Wave.
public class DataLayerTests
{
    [Test]
    public void Tower_Constructor_SetsFields()
    {
        var prefab = new GameObject("P");
        var t = new Tower("Sniper", 75, prefab);
        Assert.AreEqual("Sniper", t.name);
        Assert.AreEqual(75, t.cost);
        Assert.AreSame(prefab, t.prefab);
        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void Settings_SaveSettings_ReturnsTrueAndPersists()
    {
        var s = ScriptableObject.CreateInstance<Settings>();
        bool ok = s.saveSettings(0.5f, 0.8f, "High", "vi", "Fullscreen");
        Assert.IsTrue(ok);
        Assert.AreEqual(0.5f, s.musicVolume);
        Assert.AreEqual(0.8f, s.sfxVolume);
        Assert.AreEqual("High", s.graphicsQuality);
        Assert.AreEqual("vi", s.language);
        Assert.AreEqual("Fullscreen", s.displayMode);
        Object.DestroyImmediate(s);
    }

    [Test]
    public void Level_SaveResult_SetsInitialBest()
    {
        var lvl = ScriptableObject.CreateInstance<Level>();
        lvl.saveResult(100, 2);
        Assert.AreEqual(100, lvl.bestScore);
        Assert.AreEqual(2, lvl.bestStars);
        Object.DestroyImmediate(lvl);
    }

    [Test]
    public void Level_SaveResult_KeepsHigher_IgnoresLower()
    {
        var lvl = ScriptableObject.CreateInstance<Level>();
        lvl.saveResult(100, 2);
        lvl.saveResult(50, 1);
        Assert.AreEqual(100, lvl.bestScore);
        Assert.AreEqual(2, lvl.bestStars);
        Object.DestroyImmediate(lvl);
    }

    [Test]
    public void Level_SaveResult_UpdatesToHigher()
    {
        var lvl = ScriptableObject.CreateInstance<Level>();
        lvl.saveResult(100, 2);
        lvl.saveResult(120, 3);
        Assert.AreEqual(120, lvl.bestScore);
        Assert.AreEqual(3, lvl.bestStars);
        Object.DestroyImmediate(lvl);
    }

    [Test]
    public void Wave_SpawnEnemy_NullPrefab_ReturnsNull()
    {
        var w = new Wave { enemyPrefab = null };
        Assert.IsNull(w.spawnEnemy(Vector3.zero));
    }

    [Test]
    public void Wave_SpawnEnemy_WithPrefab_Instantiates()
    {
        var prefab = new GameObject("EnemyPrefab");
        var w = new Wave { enemyPrefab = prefab };
        var spawned = w.spawnEnemy(new Vector3(1, 2, 0));
        Assert.IsNotNull(spawned);
        Assert.AreEqual(new Vector3(1, 2, 0), spawned.transform.position);
        Object.DestroyImmediate(spawned);
        Object.DestroyImmediate(prefab);
    }
}
