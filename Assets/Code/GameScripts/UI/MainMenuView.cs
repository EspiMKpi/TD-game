using UnityEngine;
using UnityEngine.UI;

// Menu chính (UC1). Điều hướng bằng cách bật/tắt các panel con trong scene MainMenu.
public class MainMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmExitPanel;

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(() => Toggle(levelSelectPanel));
        if (settingsButton != null) settingsButton.onClick.AddListener(() => Toggle(settingsPanel));
        if (exitButton != null) exitButton.onClick.AddListener(() => Toggle(confirmExitPanel));
    }

    // Mở 1 panel và ẩn các panel anh em còn lại (tránh chồng panel).
    private void Toggle(GameObject panel)
    {
        if (panel == null) return;
        if (levelSelectPanel != null && levelSelectPanel != panel) levelSelectPanel.SetActive(false);
        if (settingsPanel != null && settingsPanel != panel) settingsPanel.SetActive(false);
        if (confirmExitPanel != null && confirmExitPanel != panel) confirmExitPanel.SetActive(false);
        panel.SetActive(true);
    }
}
