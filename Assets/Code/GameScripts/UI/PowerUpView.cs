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

    private int armedIndex = -1;   // slot đang chờ chọn vị trí; -1 = không

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
        if (armedIndex < 0) return;

        if (Input.GetMouseButtonDown(1)) { armedIndex = -1; return; }   // chuột phải: hủy

        if (Input.GetMouseButtonDown(0))
        {
            // Bỏ qua click trên UI (gồm cả chính cú bấm nút slot ở frame này).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var gs = GameSession.Instance;
            if (gs != null && gs.powerUps != null && armedIndex < gs.powerUps.Count)
            {
                Camera cam = worldCamera != null ? worldCamera : Camera.main;
                Vector3 wp = cam != null ? cam.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
                wp.z = 0f;
                gs.activatePowerUp(gs.powerUps[armedIndex], wp);
            }
            armedIndex = -1;
        }
    }
}
