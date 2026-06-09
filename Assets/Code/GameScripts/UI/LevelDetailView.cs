using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Chi tiết màn chơi (UC3): hiện thông tin + nút bắt đầu nạp scene Gameplay.
public class LevelDetailView : MonoBehaviour
{
    [Header("Panel / Texts")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI bestStarsText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "0 (1)";

    private Level current;

    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStart);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        // Panel được lưu inactive sẵn trong scene; KHÔNG tự tắt ở Start vì view nằm
        // trên chính panel này (Start chỉ chạy sau khi Show bật panel -> sẽ tự tắt nhầm).
    }

    public void Show(Level level)
    {
        current = level;
        GameFlow.SelectedLevel = level;
        if (panel != null) panel.SetActive(true);
        if (level == null) return;
        level.LoadProgress();   // đọc bestScore/bestStars đã lưu
        if (nameText != null) nameText.text = "Màn " + level.levelId;
        if (bestScoreText != null) bestScoreText.text = "Best: " + level.bestScore;
        if (bestStarsText != null) bestStarsText.text = "Sao: " + level.bestStars;
    }

    private void OnStart()
    {
        GameFlow.SelectedLevel = current;
        GameFlow.Load(gameplaySceneName);
    }

    private void OnBack()
    {
        if (panel != null) panel.SetActive(false);
    }
}
