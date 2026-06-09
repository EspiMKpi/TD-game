using UnityEngine;

// Cấu hình người chơi (UC2) — ScriptableObject, lưu qua PlayerPrefs/JSON ở Giai đoạn 7.
[CreateAssetMenu(menuName = "Quantall/Settings")]
public class Settings : ScriptableObject
{
    public float musicVolume;
    public float sfxVolume;
    public string graphicsQuality;
    public string language;
    public string displayMode;

    // UC2 – Điều chỉnh cài đặt
    public bool saveSettings(float musicVolume, float sfxVolume,
                             string graphicsQuality, string language, string displayMode)
    {
        this.musicVolume = musicVolume;
        this.sfxVolume = sfxVolume;
        this.graphicsQuality = graphicsQuality;
        this.language = language;
        this.displayMode = displayMode;
        Apply();   // Giai đoạn 8: áp dụng ngay vào engine
        return true;
    }

    // Giai đoạn 8: áp cài đặt vào engine.
    // Ghi chú: tách riêng music/sfx cần AudioMixer (chưa có asset) — tạm dùng
    // AudioListener.volume làm master theo musicVolume. language chưa áp (cần localization).
    public void Apply()
    {
        AudioListener.volume = Mathf.Clamp01(musicVolume);

        if (!string.IsNullOrEmpty(graphicsQuality))
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
                if (names[i] == graphicsQuality) { QualitySettings.SetQualityLevel(i, true); break; }
        }

        switch (displayMode)
        {
            case "Fullscreen": Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case "Windowed":   Screen.fullScreenMode = FullScreenMode.Windowed; break;
            case "Borderless": Screen.fullScreenMode = FullScreenMode.MaximizedWindow; break;
        }
    }
}
