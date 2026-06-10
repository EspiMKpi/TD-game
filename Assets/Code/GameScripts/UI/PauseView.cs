using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Cửa sổ tạm dừng trong màn chơi (UC10): Tiếp tục / Chơi lại / Cài đặt / Menu chính.
// Khi mở -> Time.timeScale = 0 (UI vẫn bấm được vì không phụ thuộc timeScale).
public class PauseView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;       // bảng tạm dừng (ẩn lúc đầu)
    [SerializeField] private GameObject settingsPanel;    // bảng cài đặt trong game (ẩn)

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;         // Tiếp tục
    [SerializeField] private Button restartButton;        // Chơi lại
    [SerializeField] private Button settingsButton;       // Cài đặt
    [SerializeField] private Button menuButton;           // Menu chính

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Update()
    {
        // Esc: mở khi đang chơi / đóng khi đang ở bảng tạm dừng. (Update chạy cả khi timeScale=0.)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf) { settingsPanel.SetActive(false); return; }
            if (pausePanel != null && pausePanel.activeSelf) Resume();
            else Pause();
        }
    }

    // Mở cửa sổ tạm dừng + dừng game.
    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        if (GameSession.Instance != null) GameSession.Instance.pause();
    }

    public void Resume()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        if (GameSession.Instance != null) GameSession.Instance.resume();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);   // SettingsView.OnBack sẽ ẩn lại
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
