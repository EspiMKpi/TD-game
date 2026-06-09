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
            if (resourcesText != null) resourcesText.text = gs.currentResources.ToString();
            if (waveText != null) waveText.text = "Wave " + gs.CurrentWaveIndex;
            if (scoreText != null) scoreText.text = gs.Score.ToString();
        }

        if (baseHpSlider != null && Base.main != null)
        {
            baseHpSlider.maxValue = Base.main.MaxHP;
            baseHpSlider.value = Base.main.CurrentHP;
        }

        // Khóa nút gọi wave sớm tới khi đạt 80% (UC8).
        if (callNextWaveButton != null && EnemySpawner.main != null)
            callNextWaveButton.interactable = EnemySpawner.main.CanCallNextWaveEarly();
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
