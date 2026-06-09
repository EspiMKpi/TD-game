using NUnit.Framework;
using UnityEngine;

// EditMode tests cho kinh tế & quản lý tháp (UC5/6/7).
// Lưu ý: edit mode KHÔNG gọi Awake/Start, nên ta gán Level_Manager.main + currency
// trực tiếp (đều public). Các method GameSession thao tác trên instance + Level_Manager.main
// nên không cần Awake. Field initializer (status=Playing, upgradeCost=30...) vẫn chạy khi AddComponent.
public class EconomyTests
{
    GameObject lmGo, gsGo, prefab;
    Level_Manager lm;
    GameSession gs;

    [SetUp]
    public void SetUp()
    {
        lmGo = new GameObject("LM");
        lm = lmGo.AddComponent<Level_Manager>();
        Level_Manager.main = lm;
        lm.currency = 100;

        gsGo = new GameObject("GS");
        gs = gsGo.AddComponent<GameSession>();

        prefab = new GameObject("TowerPrefab");
    }

    [TearDown]
    public void TearDown()
    {
        if (lmGo != null) Object.DestroyImmediate(lmGo);
        if (gsGo != null) Object.DestroyImmediate(gsGo);
        if (prefab != null) Object.DestroyImmediate(prefab);
        Level_Manager.main = null;
    }

    [Test]
    public void SpendCurrency_RejectsWhenInsufficient()
    {
        Assert.IsFalse(lm.SpendCurrency(150));
        Assert.AreEqual(100, lm.currency);
    }

    [Test]
    public void SpendCurrency_DeductsWhenEnough()
    {
        Assert.IsTrue(lm.SpendCurrency(40));
        Assert.AreEqual(60, lm.currency);
    }

    [Test]
    public void IncreaseCurrency_Adds()
    {
        lm.IncreaseCurrency(25);
        Assert.AreEqual(125, lm.currency);
    }

    [Test]
    public void GameSession_DefaultStatus_IsPlaying()
    {
        Assert.AreEqual(GameStatus.Playing, gs.status);
    }

    [Test]
    public void GameSession_CurrentResources_DelegatesToLevelManager()
    {
        Assert.AreEqual(100, gs.currentResources);
    }

    [Test]
    public void OnBaseDestroyed_SetsLost()
    {
        gs.OnBaseDestroyed();
        Assert.AreEqual(GameStatus.Lost, gs.status);
    }

    [Test]
    public void OnAllWavesCleared_CannotOverrideLost()
    {
        gs.OnBaseDestroyed();
        gs.OnAllWavesCleared();
        Assert.AreEqual(GameStatus.Lost, gs.status);
    }

    [Test]
    public void BuildTower_RejectsWhenInsufficient_NoDeduct()
    {
        var expensive = new Tower("Expensive", 1000, prefab);
        var built = gs.buildTower(expensive, Vector3.zero);
        Assert.IsNull(built);
        Assert.AreEqual(100, lm.currency);
    }

    [Test]
    public void BuildTower_DeductsWhenAffordable()
    {
        var cheap = new Tower("Cheap", 20, prefab);
        var built = gs.buildTower(cheap, Vector3.zero);
        Assert.IsNotNull(built);
        Assert.AreEqual(80, lm.currency);
        Object.DestroyImmediate(built);
    }

    [Test]
    public void UpgradeTower_SucceedsAndDeductsOldCost()
    {
        var turGo = new GameObject("Tur");
        var tur = turGo.AddComponent<TurretScript>();   // upgradeCost=30, level=1, max=3
        Assert.AreEqual(30, tur.UpgradeCost);

        bool ok = gs.upgradeTower(tur);
        Assert.IsTrue(ok);
        Assert.AreEqual(2, tur.CurrentLevel);
        Assert.AreEqual(70, lm.currency);                // 100 - 30
        Assert.Greater(tur.UpgradeCost, 30);             // cost tăng sau nâng cấp

        Object.DestroyImmediate(turGo);
    }

    [Test]
    public void UpgradeTower_RejectsWhenInsufficient()
    {
        lm.currency = 5;                                  // không đủ 30
        var turGo = new GameObject("Tur");
        var tur = turGo.AddComponent<TurretScript>();

        bool ok = gs.upgradeTower(tur);
        Assert.IsFalse(ok);
        Assert.AreEqual(1, tur.CurrentLevel);
        Assert.AreEqual(5, lm.currency);

        Object.DestroyImmediate(turGo);
    }
}
