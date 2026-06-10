using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI resourcesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Base HP")]
    [SerializeField] private TextMeshProUGUI hpText;       // máu hiển thị bằng số
    [SerializeField] private Slider baseHpSlider;          // tuỳ chọn (giữ null-guard)

    [Header("Buttons")]
    [SerializeField] private Button callNextWaveButton;   // UC8
    [SerializeField] private Button pauseButton;          // UC10
    [SerializeField] private PauseView pauseView;         // cửa sổ tạm dừng (UC10)

    [Header("Shop responsiveness")]
    [SerializeField] private RectTransform shopPanel;     // RectTransform của Menu (vị trí slide thực)
    [SerializeField] private RectTransform shopToggle;    // nút Shop — đi theo panel, đồng tốc
    [SerializeField] private float shopHiddenX = 200f;    // anchoredPos.x của shop khi ẩn
    [SerializeField] private float shopOpenX = -150f;     // anchoredPos.x của shop khi mở
    [SerializeField] private float toggleRestX = -20f;    // x của toggle khi shop ẩn (góc phải)
    [SerializeField] private float toggleOpenX = -320f;   // x của toggle khi shop mở (tránh panel)

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

        if (hpText != null && Base.main != null)
            hpText.text = "Máu: " + Base.main.CurrentHP + "/" + Base.main.MaxHP;

        if (baseHpSlider != null && Base.main != null)
        {
            baseHpSlider.maxValue = Base.main.MaxHP;
            baseHpSlider.value = Base.main.CurrentHP;
        }

        // Khóa nút gọi wave sớm tới khi đạt 80% (UC8). Nút này nằm cố định góc trái (dưới Wave).
        if (callNextWaveButton != null && EnemySpawner.main != null)
            callNextWaveButton.interactable = EnemySpawner.main.CanCallNextWaveEarly();

        // Nút Shop luôn hiện, đi theo panel shop ĐỒNG TỐC: lái trực tiếp theo vị trí slide thực
        // của shop (không lerp riêng) nên ra/vào khớp đúng tốc độ animation của shop.
        if (shopPanel != null && shopToggle != null)
        {
            float t = Mathf.InverseLerp(shopHiddenX, shopOpenX, shopPanel.anchoredPosition.x);
            Vector2 ap = shopToggle.anchoredPosition;
            ap.x = Mathf.Lerp(toggleRestX, toggleOpenX, t);
            shopToggle.anchoredPosition = ap;
        }
    }

    private void OnCallNextWave()
    {
        if (EnemySpawner.main != null) EnemySpawner.main.CallNextWaveEarly();
    }

    private void OnPause()
    {
        // Mở cửa sổ tạm dừng (Tiếp tục/Chơi lại/Cài đặt/Menu chính). Fallback: chỉ dừng.
        if (pauseView != null) pauseView.Pause();
        else if (GameSession.Instance != null) GameSession.Instance.pause();
    }
}
