using UnityEngine;

// Xử lý input chung của người chơi (plan §4.3). Phần input gameplay (đặt tháp)
// hiện do Plot/TurretScript đảm nhận; lớp này giữ đúng thiết kế + lo việc thoát game.
public class Player : MonoBehaviour
{
    public string playerName;     // 'name' trong thiết kế
    public Settings settings;     // Player liên kết Settings (1-1)

    void Update()
    {
        checkClick();
    }

    public void checkClick()
    {
        // TODO: bắt input chuột/chạm cấp toàn cục nếu cần (hiện Plot/Turret tự xử lý).
    }

    // UC12 — thoát game.
    public void exit()
    {
        // Giai đoạn 7: lưu cài đặt trước khi thoát.
        if (settings != null) SaveSystem.SaveSettings(settings);
        Application.Quit();
    }
}
