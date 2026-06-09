using NUnit.Framework;
using UnityEngine;

// EditMode tests cho lưu trữ & tiến trình (Giai đoạn 7).
// Dùng levelId riêng (12345) + dọn PlayerPrefs để không đụng save thật.
public class SaveTests
{
    const int TestId = 12345;

    [SetUp] public void SetUp() { ClearKeys(); }
    [TearDown] public void TearDown() { ClearKeys(); }

    void ClearKeys()
    {
        PlayerPrefs.DeleteKey("lvl_" + TestId + "_unlocked");
        PlayerPrefs.DeleteKey("lvl_" + TestId + "_score");
        PlayerPrefs.DeleteKey("lvl_" + TestId + "_stars");
        PlayerPrefs.Save();
    }

    Level NewLevel()
    {
        var l = ScriptableObject.CreateInstance<Level>();
        l.levelId = TestId;
        return l;
    }

    [Test]
    public void SaveResult_Persists_AcrossInstances()
    {
        var a = NewLevel();
        a.saveResult(80, 2);
        var b = NewLevel();
        b.LoadProgress();
        Assert.AreEqual(80, b.bestScore);
        Assert.AreEqual(2, b.bestStars);
        Object.DestroyImmediate(a); Object.DestroyImmediate(b);
    }

    [Test]
    public void SaveResult_LowerResult_DoesNotOverwriteSaved()
    {
        var a = NewLevel();
        a.saveResult(80, 2);
        a.saveResult(50, 1);
        var b = NewLevel();
        b.LoadProgress();
        Assert.AreEqual(80, b.bestScore);
        Assert.AreEqual(2, b.bestStars);
        Object.DestroyImmediate(a); Object.DestroyImmediate(b);
    }

    [Test]
    public void SaveResult_HigherResult_UpdatesSaved()
    {
        var a = NewLevel();
        a.saveResult(80, 2);
        a.saveResult(200, 3);
        var b = NewLevel();
        b.LoadProgress();
        Assert.AreEqual(200, b.bestScore);
        Assert.AreEqual(3, b.bestStars);
        Object.DestroyImmediate(a); Object.DestroyImmediate(b);
    }

    [Test]
    public void Base_Initialize_SetsMaxAndCurrentHp()
    {
        var go = new GameObject("B");
        var b = go.AddComponent<Base>();
        b.Initialize(33);
        Assert.AreEqual(33, b.MaxHP);
        Assert.AreEqual(33, b.CurrentHP);
        Object.DestroyImmediate(go);
    }

    // Giai đoạn 7 (UC2): round-trip SaveSystem cho Settings qua PlayerPrefs.
    [Test]
    public void SaveSystem_Settings_RoundTrip_PersistsValues()
    {
        string[] keys = { "set_music", "set_sfx", "set_quality", "set_lang", "set_display" };
        foreach (var k in keys) PlayerPrefs.DeleteKey(k);
        PlayerPrefs.Save();

        var a = ScriptableObject.CreateInstance<Settings>();
        a.musicVolume = 0.42f;
        a.sfxVolume = 0.73f;
        a.graphicsQuality = "High";
        a.language = "vi";
        a.displayMode = "Windowed";
        SaveSystem.SaveSettings(a);

        var b = ScriptableObject.CreateInstance<Settings>();
        SaveSystem.LoadSettings(b);
        Assert.AreEqual(0.42f, b.musicVolume, 0.0001f);
        Assert.AreEqual(0.73f, b.sfxVolume, 0.0001f);
        Assert.AreEqual("High", b.graphicsQuality);
        Assert.AreEqual("vi", b.language);
        Assert.AreEqual("Windowed", b.displayMode);

        Object.DestroyImmediate(a); Object.DestroyImmediate(b);
        foreach (var k in keys) PlayerPrefs.DeleteKey(k);
        PlayerPrefs.Save();
    }

    // Giai đoạn 7: thắng -> mở khóa & lưu màn kế.
    [Test]
    public void Level_UnlockNext_UnlocksAndPersistsNextLevel()
    {
        const int nextId = 12346;
        foreach (var s in new[] { "unlocked", "score", "stars" }) PlayerPrefs.DeleteKey("lvl_" + nextId + "_" + s);
        PlayerPrefs.Save();

        var cur = NewLevel();
        var next = ScriptableObject.CreateInstance<Level>();
        next.levelId = nextId;
        next.isUnlocked = false;
        typeof(Level).GetField("nextLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cur, next);

        cur.unlockNext();
        Assert.IsTrue(next.isUnlocked, "nextLevel.isUnlocked in-memory");

        var reload = ScriptableObject.CreateInstance<Level>();
        reload.levelId = nextId;
        reload.LoadProgress();
        Assert.IsTrue(reload.isUnlocked, "unlock persisted via SaveSystem");

        Object.DestroyImmediate(cur); Object.DestroyImmediate(next); Object.DestroyImmediate(reload);
        foreach (var s in new[] { "unlocked", "score", "stars" }) PlayerPrefs.DeleteKey("lvl_" + nextId + "_" + s);
        PlayerPrefs.Save();
    }

    // Giai đoạn 7: loadLevel(id) tìm asset theo id trong Resources/Levels (Level_01 đã chuyển vào đó).
    [Test]
    public void Level_LoadLevel_FindsByIdFromResources()
    {
        var l = Level.loadLevel(1);
        Assert.IsNotNull(l, "Level.loadLevel(1) phải tìm thấy Level_01 trong Resources/Levels");
        Assert.AreEqual(1, l.levelId);
    }

    // Giai đoạn 8: Settings.Apply áp musicVolume vào AudioListener (master).
    [Test]
    public void Settings_Apply_SetsMasterVolume()
    {
        float prev = AudioListener.volume;
        var s = ScriptableObject.CreateInstance<Settings>();
        s.musicVolume = 0.33f;   // graphicsQuality/displayMode rỗng -> không đổi Quality/Screen
        s.Apply();
        Assert.AreEqual(0.33f, AudioListener.volume, 0.0001f);
        Object.DestroyImmediate(s);
        AudioListener.volume = prev;
    }
}
