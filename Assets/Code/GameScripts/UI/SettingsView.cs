using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Cài đặt (UC2). Đọc giá trị hiện tại vào UI, lưu qua Settings.saveSettings.
public class SettingsView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Settings settings;

    [Header("Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown displayDropdown;

    [Header("Buttons / Panel")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject panel;

    private void Start()
    {
        if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        LoadIntoUI();
    }

    private void LoadIntoUI()
    {
        if (settings == null) return;
        if (musicSlider != null) musicSlider.value = settings.musicVolume;
        if (sfxSlider != null) sfxSlider.value = settings.sfxVolume;
        SelectOption(qualityDropdown, settings.graphicsQuality);
        SelectOption(languageDropdown, settings.language);
        SelectOption(displayDropdown, settings.displayMode);
    }

    private void OnSave()
    {
        if (settings == null) return;
        settings.saveSettings(
            musicSlider != null ? musicSlider.value : settings.musicVolume,
            sfxSlider != null ? sfxSlider.value : settings.sfxVolume,
            OptionText(qualityDropdown, settings.graphicsQuality),
            OptionText(languageDropdown, settings.language),
            OptionText(displayDropdown, settings.displayMode));
        SaveSystem.SaveSettings(settings);   // Giai đoạn 7: ghi tiến trình cài đặt xuống PlayerPrefs
    }

    private void OnBack()
    {
        if (panel != null) panel.SetActive(false);
    }

    private static string OptionText(TMP_Dropdown d, string fallback)
    {
        if (d == null || d.options.Count == 0) return fallback;
        return d.options[d.value].text;
    }

    private static void SelectOption(TMP_Dropdown d, string value)
    {
        if (d == null || string.IsNullOrEmpty(value)) return;
        for (int i = 0; i < d.options.Count; i++)
            if (d.options[i].text == value) { d.value = i; return; }
    }
}
