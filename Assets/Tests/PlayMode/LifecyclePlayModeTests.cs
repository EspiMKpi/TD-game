using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// PlayMode tests cho hành vi phụ thuộc lifecycle (Awake/Start) và Destroy —
// những thứ EditMode không kiểm được.
public class LifecyclePlayModeTests
{
    [Test]
    public void Base_Awake_InitsCurrentHpToMax()
    {
        var go = new GameObject("Base");
        var b = go.AddComponent<Base>();          // Awake chạy ngay trong play mode
        Assert.AreEqual(b.MaxHP, b.CurrentHP);
        Object.Destroy(go);
    }

    [Test]
    public void Base_TakeDamage_Reduces_Clamps_FiresOnce()
    {
        var go = new GameObject("Base");
        var b = go.AddComponent<Base>();
        int max = b.MaxHP;
        int fired = 0;
        b.onBaseDestroyed.AddListener(() => fired++);

        b.takeDamage(1);
        Assert.AreEqual(max - 1, b.CurrentHP, "giảm máu đúng từ maxHP");

        b.takeDamage(99999);
        Assert.AreEqual(0, b.CurrentHP, "máu kẹp tại 0, không âm");
        Assert.AreEqual(1, fired, "onBaseDestroyed bắn đúng 1 lần");

        b.takeDamage(5);
        Assert.AreEqual(1, fired, "takeDamage sau khi phá bị bỏ qua (không bắn lần 2)");

        Object.Destroy(go);
    }

    [Test]
    public void SellTower_RefundsSellPrice_AndDestroys()
    {
        var lmGo = new GameObject("LM");
        var lm = lmGo.AddComponent<Level_Manager>();
        Level_Manager.main = lm;
        lm.currency = 0;

        var gsGo = new GameObject("GS");
        var gs = gsGo.AddComponent<GameSession>();

        var turGo = new GameObject("Tur");
        var tur = turGo.AddComponent<TurretScript>();
        int sp = tur.SellPrice;

        gs.sellTower(tur);
        Assert.AreEqual(sp, lm.currency, "hoàn đúng sellPrice");

        Object.Destroy(lmGo);
        Object.Destroy(gsGo);
        Level_Manager.main = null;
    }

    [UnityTest]
    public IEnumerator BaseDestroyed_TriggersGameSessionLost()
    {
        var baseGo = new GameObject("Base");
        var b = baseGo.AddComponent<Base>();       // Awake set Base.main + currentHP=max
        var gsGo = new GameObject("GS");
        var gs = gsGo.AddComponent<GameSession>();

        yield return null;                          // để GameSession.Start subscribe onBaseDestroyed

        b.takeDamage(99999);
        Assert.AreEqual(GameStatus.Lost, gs.status, "căn cứ bị phá -> GameSession Lost");

        Object.Destroy(baseGo);
        Object.Destroy(gsGo);
        Level_Manager.main = null;
    }
}
