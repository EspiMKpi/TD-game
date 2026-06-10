using UnityEngine;
using UnityEngine.UI;
using TMPro;

// HUD trong màn chơi (Giai đoạn 6). Cập nhật bằng polling trong Update cho gọn,
// nối nút bấm tới GameSession / EnemySpawner. Kéo thả tham chiếu UI trong Editor.
public class GameplayView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI resourcesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Base HP")]
    [SerializeField] private Slider baseHpSlider;

    [Header("Buttons")]
    [SerializeField] private Button callNextWaveButton;   // UC8
    [SerializeField] private Button pauseButton;          // UC10

    [Header("Shop responsiveness")]
    [SerializeField] private Menu shopMenu;               // nút next-wave dịch tránh khi shop mở
    [SerializeField] private float nextWaveRestX = -20f;       // vị trí x khi shop đóng
    [SerializeField] private float nextWaveMenuOpenX = -320f;  // vị trí x khi shop mở (tránh panel)

    private void Start()
    {
        if (callNextWaveButton != null) callNextWaveButton.onClick.AddListener(OnCallNextWave);
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPause);
    }

    private void Update()
    {
        var gs = GameSession.Instance;
        if (gs != null)
        {
            if (resourcesText != null) resourcesText.text = "Tiền: " + gs.currentResources;
            if (waveText != null)
            {
                int total = EnemySpawner.main != null ? EnemySpawner.main.TotalWaves : 0;
                waveText.text = "Wave " + gs.CurrentWaveIndex + "/" + total;
            }
            if (scoreText != null) scoreText.text = "Điểm: " + gs.Score;
        }

        if (baseHpSlider != null && Base.main != null)
        {
            baseHpSlider.maxValue = Base.main.MaxHP;
            baseHpSlider.value = Base.main.CurrentHP;
        }

        if (callNextWaveButton != null)
        {
            // Khóa nút gọi wave sớm tới khi đạt 80% (UC8).
            if (EnemySpawner.main != null)
                callNextWaveButton.interactable = EnemySpawner.main.CanCallNextWaveEarly();

            // Luôn hiện nhưng dịch trái khi shop mở để không bị panel che (phản hồi theo menu).
            var rt = (RectTransform)callNextWaveButton.transform;
            float targetX = (shopMenu != null && shopMenu.IsMenuOpen) ? nextWaveMenuOpenX : nextWaveRestX;
            Vector2 ap = rt.anchoredPosition;
            ap.x = Mathf.Lerp(ap.x, targetX, Time.unscaledDeltaTime * 12f);
            rt.anchoredPosition = ap;
        }
    }

    private void OnCallNextWave()
    {
        if (EnemySpawner.main != null) EnemySpawner.main.CallNextWaveEarly();
    }

    private void OnPause()
    {
        if (GameSession.Instance != null) GameSession.Instance.pause();
    }
}
