using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// UI kỹ năng bổ trợ (UC9). Mỗi nút slot ứng với GameSession.powerUps[i].
// Bấm slot -> "lên đạn"; click tiếp trên map -> activatePowerUp(powerUp, worldPos).
// Chuột phải để hủy. Slot không có power-up tương ứng sẽ tự ẩn.
//
// Lưu ý dữ liệu: cần gán các asset PowerUp vào GameSession.powerUps (Inspector) thì
// các slot mới hiện. Logic kích hoạt/hồi chiêu/sát thương đã nằm trong GameSession.activatePowerUp.
public class PowerUpView : MonoBehaviour
{
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Text[] slotLabels;              // tùy chọn (UI.Text, khớp nút HUD legacy)
    [SerializeField] private Camera worldCamera;             // mặc định Camera.main

    [Header("Range indicator")]
    [SerializeField] private SpriteRenderer rangeIndicator;  // vòng tròn trong suốt (world-space)
    [SerializeField] private float baseAlpha = 0.5f;
    [SerializeField] private float flashDuration = 0.45f;    // nháy khi kích hoạt

    private int armedIndex = -1;   // slot đang chờ chọn vị trí; -1 = không
    private float flashTimer;
    private Vector3 flashPos;
    private float flashRadius;
    private Color flashTint;

    public static PowerUpView Instance { get; private set; }
    public bool IsArmed => armedIndex >= 0;   // đang chờ chọn vị trí dùng power-up

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        int n = slotButtons != null ? slotButtons.Length : 0;
        for (int i = 0; i < n; i++)
        {
            int idx = i;
            if (slotButtons[i] != null) slotButtons[i].onClick.AddListener(() => Arm(idx));
        }
        RefreshSlots();
        HideIndicator();
    }

    // Ẩn/hiện slot theo số power-up thực có; cập nhật nhãn tên.
    public void RefreshSlots()
    {
        if (slotButtons == null) return;
        var gs = GameSession.Instance;
        int count = (gs != null && gs.powerUps != null) ? gs.powerUps.Count : 0;
        for (int i = 0; i < slotButtons.Length; i++)
        {
            bool has = i < count && gs.powerUps[i] != null;
            if (slotButtons[i] != null) slotButtons[i].gameObject.SetActive(has);
            if (has && slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                slotLabels[i].text = gs.powerUps[i].powerUpName;
        }
    }

    private void Arm(int index)
    {
        armedIndex = index;
    }

    private void Update()
    {
        if (armedIndex >= 0)
        {
            ShowPreviewAtCursor();   // vòng tròn tầm bám con trỏ

            if (Input.GetMouseButtonDown(1)) { armedIndex = -1; HideIndicator(); return; }   // chuột phải: hủy

            if (Input.GetMouseButtonDown(0))
            {
                // Bỏ qua click trên UI (gồm cả chính cú bấm nút slot ở frame này).
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                var gs = GameSession.Instance;
                if (gs != null && gs.powerUps != null && armedIndex < gs.powerUps.Count)
                {
                    PowerUp pu = gs.powerUps[armedIndex];
                    Camera cam = worldCamera != null ? worldCamera : Camera.main;
                    Vector3 wp = cam != null ? cam.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
                    wp.z = 0f;
                    if (gs.activatePowerUp(pu, wp)) StartFlash(wp, pu);   // nháy vòng tròn tại điểm dùng
                }
                armedIndex = -1;
            }
        }
        else
        {
            UpdateFlash();   // mờ dần vòng nháy, hoặc ẩn
        }
    }

    private void ShowPreviewAtCursor()
    {
        var gs = GameSession.Instance;
        if (gs == null || gs.powerUps == null || armedIndex >= gs.powerUps.Count) return;
        PowerUp pu = gs.powerUps[armedIndex];
        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        if (cam == null) return;
        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition); wp.z = 0f;
        ShowIndicator(wp, pu.effectRadius, pu.tintColor, baseAlpha);
    }

    private void ShowIndicator(Vector3 pos, float radius, Color tint, float alpha)
    {
        if (rangeIndicator == null) return;
        rangeIndicator.transform.position = pos;
        float diameter = Mathf.Max(0.1f, radius * 2f);   // sprite đường kính 1 unit ở scale 1
        rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        Color c = tint; c.a = alpha;
        rangeIndicator.color = c;
        rangeIndicator.enabled = true;
    }

    private void HideIndicator()
    {
        flashTimer = 0f;
        if (rangeIndicator != null) rangeIndicator.enabled = false;
    }

    private void StartFlash(Vector3 pos, PowerUp pu)
    {
        flashPos = pos; flashRadius = pu.effectRadius; flashTint = pu.tintColor; flashTimer = flashDuration;
    }

    private void UpdateFlash()
    {
        if (flashTimer <= 0f)
        {
            if (rangeIndicator != null && rangeIndicator.enabled) rangeIndicator.enabled = false;
            return;
        }
        flashTimer -= Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(flashTimer / flashDuration);
        ShowIndicator(flashPos, flashRadius, flashTint, baseAlpha * t);
        if (flashTimer <= 0f) HideIndicator();
    }
}
