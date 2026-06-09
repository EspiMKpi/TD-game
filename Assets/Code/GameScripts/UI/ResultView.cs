using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Màn hình kết quả thắng/thua (UC11). Lắng nghe GameSession.onStatusChanged,
// hiện panel khi status != Playing và dừng game.
public class ResultView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;            // panel kết quả (ẩn lúc đầu)
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI starsText;   // tùy chọn: nếu null thì ghép sao vào scoreText
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [SerializeField] private string menuSceneName = "MainMenu";

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenu);

        if (GameSession.Instance != null)
            GameSession.Instance.onStatusChanged.AddListener(OnStatusChanged);
    }

    private void OnDestroy()
    {
        if (GameSession.Instance != null)
            GameSession.Instance.onStatusChanged.RemoveListener(OnStatusChanged);
    }

    private void OnStatusChanged()
    {
        var gs = GameSession.Instance;
        if (gs == null || gs.status == GameStatus.Playing) return;

        if (panel != null) panel.SetActive(true);
        if (titleText != null) titleText.text = gs.status == GameStatus.Won ? "CHIẾN THẮNG" : "THẤT BẠI";

        string scoreLine = "Điểm: " + gs.Score;
        if (starsText != null) starsText.text = "Sao: " + gs.Stars + "/3";
        else scoreLine += "   Sao: " + gs.Stars + "/3";
        if (scoreText != null) scoreText.text = scoreLine;

        Time.timeScale = 0f;   // dừng game khi hiện kết quả
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
