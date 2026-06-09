using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI nâng cấp / bán tháp đã đặt (UC6, UC7). Hiện khi click vào Plot đang có tháp.
// Định tuyến qua GameSession.upgradeTower / sellTower (logic + kiểm tra tiền đã có sẵn).
public class TowerActionView : MonoBehaviour
{
    public static TowerActionView Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;            // panel hành động (ẩn lúc đầu)
    [SerializeField] private TextMeshProUGUI infoText;    // cấp / chi phí nâng cấp / giá bán
    [SerializeField] private Button upgradeButton;        // UC6
    [SerializeField] private Button sellButton;           // UC7
    [SerializeField] private Button closeButton;

    private TurretScript current;
    private Plot currentPlot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgrade);
        if (sellButton != null) sellButton.onClick.AddListener(OnSell);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    // Gọi từ Plot khi click vào ô có tháp.
    public void Show(TurretScript tower, Plot plot)
    {
        if (tower == null) return;
        current = tower;
        currentPlot = plot;
        if (panel != null) panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        current = null;
        currentPlot = null;
        if (panel != null) panel.SetActive(false);
    }

    private void Refresh()
    {
        if (current == null) { Hide(); return; }
        if (infoText != null)
        {
            string up = current.IsMaxLevel ? "—" : current.UpgradeCost.ToString();
            infoText.text = "Cấp " + current.CurrentLevel + (current.IsMaxLevel ? " (tối đa)" : "")
                          + "\nNâng cấp: " + up + "\nBán: " + current.SellPrice;
        }
        if (upgradeButton != null) upgradeButton.interactable = !current.IsMaxLevel;
    }

    private void OnUpgrade()
    {
        if (current == null || GameSession.Instance == null) return;
        GameSession.Instance.upgradeTower(current);   // tự kiểm tra tiền + cấp tối đa
        Refresh();
    }

    private void OnSell()
    {
        if (current == null) return;
        if (GameSession.Instance != null) GameSession.Instance.sellTower(current);
        else Destroy(current.gameObject);
        if (currentPlot != null) currentPlot.ClearTower();
        Hide();
    }
}
