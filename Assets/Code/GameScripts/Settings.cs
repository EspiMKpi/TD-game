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
        // TODO Giai đoạn 7/8: ghi PlayerPrefs/JSON; áp dụng AudioMixer, Screen.SetResolution...
        return true;
    }
}
