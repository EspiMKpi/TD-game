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

    // Cấu hình màn chơi: Level "hợp thành" nhiều Wave.
    public List<Wave> waves = new List<Wave>();
    // TODO: List<TowerSlot> towerSlots — bổ sung khi tách lớp TowerSlot
    // (hiện các ô đặt tháp là Plot đặt trực tiếp trong scene).

    public static Level loadLevel(int levelId)
    {
        // TODO Giai đoạn 7: load asset theo id (Resources/Addressables) qua SaveSystem.
        return null;
    }

    public void saveResult(int score, int stars)
    {
        if (score > bestScore) bestScore = score;
        if (stars > bestStars) bestStars = stars;
        // TODO Giai đoạn 7: gọi SaveSystem.Save().
    }
}
