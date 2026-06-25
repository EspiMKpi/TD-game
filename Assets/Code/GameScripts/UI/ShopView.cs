using UnityEngine;
using UnityEngine.UI;

// Chỉ báo tháp đang chọn trong Shop (UC5). Thuần trình bày (View):
// bật khung sáng cho nút đang chọn, ẩn các nút còn lại. Việc chọn tháp vẫn
// do onClick của nút -> BuildManager.SetSelectedTower (không đụng tới logic).
public class ShopView : MonoBehaviour
{
    [SerializeField] private Button[] towerButtons;        // các nút mua tháp theo thứ tự
    [SerializeField] private GameObject[] selectedFrames;  // khung sáng, song song với towerButtons
    [SerializeField] private int defaultIndex = 0;         // tháp chọn sẵn lúc đầu (khớp BuildManager)

    private void Start()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            int idx = i;
            if (towerButtons[i] != null) towerButtons[i].onClick.AddListener(() => Highlight(idx));
        }
        Highlight(defaultIndex);
    }

    // Bật khung của nút được chọn, tắt phần còn lại.
    public void Highlight(int index)
    {
        if (selectedFrames == null) return;
        for (int i = 0; i < selectedFrames.Length; i++)
            if (selectedFrames[i] != null) selectedFrames[i].SetActive(i == index);
    }
}
