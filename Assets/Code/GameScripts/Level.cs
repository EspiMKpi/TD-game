using System.Collections.Generic;
using UnityEngine;

// Cấu hình một màn chơi (ScriptableObject) + tiến trình mở khóa.
// Một Gameplay scene dùng chung, nạp asset Level đã chọn (mục 3 kế hoạch).
[CreateAssetMenu(menuName = "Quantall/Level")]
public class Level : ScriptableObject
{
    public int levelId;
    public int waveCount;
    public int initialResources;
    public int baseMaxHP;

    // Tiến trình (đọc/ghi qua SaveSystem lúc runtime — Giai đoạn 7).
    public bool isUnlocked;
    public int bestScore;
    public int bestStars;

    // Màn kế tiếp (gán trong asset; null nếu là màn cuối) — mở khóa khi thắng màn này.
    [SerializeField] private Level nextLevel;

    // Cấu hình màn chơi: Level "hợp thành" nhiều Wave.
    public List<Wave> waves = new List<Wave>();
    // TODO: List<TowerSlot> towerSlots — bổ sung khi tách lớp TowerSlot
    // (hiện các ô đặt tháp là Plot đặt trực tiếp trong scene).

    // Tải Level theo id từ Resources/Levels (đặt các Level asset trong thư mục Resources/Levels).
    public static Level loadLevel(int levelId)
    {
        foreach (var lvl in Resources.LoadAll<Level>("Levels"))
            if (lvl != null && lvl.levelId == levelId) return lvl;
        return null;
    }

    public void saveResult(int score, int stars)
    {
        if (score > bestScore) bestScore = score;
        if (stars > bestStars) bestStars = stars;
        SaveSystem.SaveLevel(this);
    }

    // Mở khóa màn kế khi thắng màn này (Giai đoạn 7). No-op nếu là màn cuối / đã mở.
    public void unlockNext()
    {
        if (nextLevel == null || nextLevel.isUnlocked) return;
        nextLevel.isUnlocked = true;
        SaveSystem.SaveLevel(nextLevel);
    }

    // Đọc tiến trình đã lưu vào asset này (gọi khi mở LevelSelect/Detail).
    public void LoadProgress()
    {
        SaveSystem.LoadLevel(this);
    }
}
