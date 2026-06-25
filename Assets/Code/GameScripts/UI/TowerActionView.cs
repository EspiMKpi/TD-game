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
        if (panel != null)
        {
            panel.SetActive(true);
            PositionNextTo(tower.transform.position);
        }
        Refresh();
    }

    // Đặt popup cạnh tháp (đổi toạ độ world -> điểm trên canvas), nghiêng về giữa màn
    // hình để không che tháp và luôn nằm trong khung hình.
    private void PositionNextTo(Vector3 worldPos)
    {
        var rt = panel.transform as RectTransform;
        if (rt == null) return;
        var canvas = rt.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRect = canvas.transform as RectTransform;

        Camera cam = Camera.main;
        Vector2 screen = cam != null ? (Vector2)cam.WorldToScreenPoint(worldPos) : (Vector2)worldPos;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCam, out Vector2 local))
            return;

        float dir = local.x > 0f ? -1f : 1f;                 // đẩy về phía giữa
        local.x += dir * (rt.rect.width * 0.5f + 40f);

        Vector2 half = canvasRect.rect.size * 0.5f;          // kẹp trong canvas
        Vector2 pHalf = rt.rect.size * 0.5f;
        local.x = Mathf.Clamp(local.x, -half.x + pHalf.x, half.x - pHalf.x);
        local.y = Mathf.Clamp(local.y, -half.y + pHalf.y, half.y - pHalf.y);

        rt.anchoredPosition = local;
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
