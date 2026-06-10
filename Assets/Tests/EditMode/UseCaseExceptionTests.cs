using NUnit.Framework;
using UnityEngine;
using UnityEditor;   // SerializedObject (test EditMode là editor-only nên dùng được)

// Kiểm thử luồng NGOẠI LỆ các use case (phục vụ Chương 5).
// EditMode: không gọi Awake; gán Level_Manager.main + currency trực tiếp.
public class UseCaseExceptionTests
{
    GameObject lmGo, gsGo;
    Level_Manager lm;
    GameSession gs;

    [SetUp]
    public void SetUp()
    {
        lmGo = new GameObject("LM");
        lm = lmGo.AddComponent<Level_Manager>();
        Level_Manager.main = lm;
        gsGo = new GameObject("GS");
        gs = gsGo.AddComponent<GameSession>();
    }

    [TearDown]
    public void TearDown()
    {
        if (lmGo != null) Object.DestroyImmediate(lmGo);
        if (gsGo != null) Object.DestroyImmediate(gsGo);
        Level_Manager.main = null;
    }

    // UC6 — ngoại lệ: tháp đạt cấp tối đa thì không nâng cấp được.
    [Test]
    public void UC6_UpgradeTower_RejectedAtMaxLevel()
    {
        lm.currency = 1000000;
        var turGo = new GameObject("Tur");
        var tur = turGo.AddComponent<TurretScript>();   // level 1, maxLevel 3

        Assert.IsTrue(gs.upgradeTower(tur));   // -> 2
        Assert.IsTrue(gs.upgradeTower(tur));   // -> 3 (max)
        Assert.AreEqual(3, tur.CurrentLevel);
        Assert.IsTrue(tur.IsMaxLevel);

        int before = lm.currency;
        Assert.IsFalse(gs.upgradeTower(tur), "đã max cấp -> từ chối");
        Assert.AreEqual(before, lm.currency, "không trừ tiền khi từ chối");

        Object.DestroyImmediate(turGo);
    }

    // UC9 — ngoại lệ: power-up đang hồi chiêu thì không kích hoạt được.
    [Test]
    public void UC9_ActivatePowerUp_RejectedOnCooldown()
    {
        lm.currency = 1000;
        var pu = ScriptableObject.CreateInstance<PowerUp>();
        pu.type = PowerUpType.Airstrike; pu.resourceCost = 10; pu.cooldown = 100f; pu.effectRadius = 1f;

        Assert.IsTrue(gs.activatePowerUp(pu, Vector3.zero), "lần đầu: kích hoạt được");
        Assert.IsFalse(gs.activatePowerUp(pu, Vector3.zero), "đang hồi chiêu -> từ chối");

        Object.DestroyImmediate(pu);
    }

    // UC9 — ngoại lệ: không đủ tài nguyên.
    [Test]
    public void UC9_ActivatePowerUp_RejectedInsufficientFunds()
    {
        lm.currency = 5;
        var pu = ScriptableObject.CreateInstance<PowerUp>();
        pu.type = PowerUpType.Airstrike; pu.resourceCost = 50; pu.cooldown = 0f; pu.effectRadius = 1f;

        Assert.IsFalse(gs.activatePowerUp(pu, Vector3.zero), "thiếu tiền -> từ chối");
        Assert.AreEqual(5, lm.currency, "không trừ tiền khi từ chối");

        Object.DestroyImmediate(pu);
    }

    // UC11/Giai đoạn 7 — mở khóa màn kế khi thắng (Level.unlockNext) + lưu lại.
    [Test]
    public void Level_UnlockNext_UnlocksAndPersists()
    {
        const int idB = 90002;
        PlayerPrefs.DeleteKey("lvl_" + idB + "_unlocked");

        var a = ScriptableObject.CreateInstance<Level>(); a.levelId = 90001;
        var b = ScriptableObject.CreateInstance<Level>(); b.levelId = idB; b.isUnlocked = false;
        var so = new SerializedObject(a);
        so.FindProperty("nextLevel").objectReferenceValue = b;
        so.ApplyModifiedProperties();

        a.unlockNext();
        Assert.IsTrue(b.isUnlocked, "màn kế được mở khóa");

        var b2 = ScriptableObject.CreateInstance<Level>(); b2.levelId = idB;
        b2.LoadProgress();
        Assert.IsTrue(b2.isUnlocked, "mở khóa được lưu (đọc lại từ instance khác)");

        PlayerPrefs.DeleteKey("lvl_" + idB + "_unlocked");
        Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(b2);
    }
}
