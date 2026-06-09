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
}
