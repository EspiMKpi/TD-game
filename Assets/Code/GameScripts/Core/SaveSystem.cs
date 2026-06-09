using UnityEngine;

// Lưu/đọc tiến trình qua PlayerPrefs (Giai đoạn 7). Đơn giản, ổn định cho đồ án.
public static class SaveSystem
{
    public static void SaveLevel(Level l)
    {
        if (l == null) return;
        string p = "lvl_" + l.levelId + "_";
        PlayerPrefs.SetInt(p + "unlocked", l.isUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(p + "score", l.bestScore);
        PlayerPrefs.SetInt(p + "stars", l.bestStars);
        PlayerPrefs.Save();
    }

    public static void LoadLevel(Level l)
    {
        if (l == null) return;
        string p = "lvl_" + l.levelId + "_";
        l.isUnlocked = PlayerPrefs.GetInt(p + "unlocked", l.isUnlocked ? 1 : 0) == 1;
        l.bestScore = PlayerPrefs.GetInt(p + "score", l.bestScore);
        l.bestStars = PlayerPrefs.GetInt(p + "stars", l.bestStars);
    }

    public static void SaveSettings(Settings s)
    {
        if (s == null) return;
        PlayerPrefs.SetFloat("set_music", s.musicVolume);
        PlayerPrefs.SetFloat("set_sfx", s.sfxVolume);
        PlayerPrefs.SetString("set_quality", s.graphicsQuality ?? "");
        PlayerPrefs.SetString("set_lang", s.language ?? "");
        PlayerPrefs.SetString("set_display", s.displayMode ?? "");
        PlayerPrefs.Save();
    }

    public static void LoadSettings(Settings s)
    {
        if (s == null) return;
        s.musicVolume = PlayerPrefs.GetFloat("set_music", s.musicVolume);
        s.sfxVolume = PlayerPrefs.GetFloat("set_sfx", s.sfxVolume);
        s.graphicsQuality = PlayerPrefs.GetString("set_quality", s.graphicsQuality);
        s.language = PlayerPrefs.GetString("set_lang", s.language);
        s.displayMode = PlayerPrefs.GetString("set_display", s.displayMode);
    }
}
