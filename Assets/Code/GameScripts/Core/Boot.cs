using UnityEngine;

// Scene khởi động (plan §3): nạp Settings đã lưu rồi chuyển sang MainMenu.
public class Boot : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private Settings settings;   // asset cấu hình dùng chung (kéo thả trong Editor)

    private void Start()
    {
        // Giai đoạn 7: nạp Settings đã lưu (PlayerPrefs) vào asset trước khi vào menu.
        if (settings != null)
        {
            SaveSystem.LoadSettings(settings);
            settings.Apply();   // Giai đoạn 8: áp cài đặt ngay khi khởi động
        }
        GameFlow.Load(mainMenuScene);
    }
}
