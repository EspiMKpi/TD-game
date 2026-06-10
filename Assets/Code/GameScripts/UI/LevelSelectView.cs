using UnityEngine;
using UnityEngine.UI;

// Chọn màn chơi (UC3). Giữ danh sách Level; chọn 1 màn -> mở LevelDetailView.
// Danh sách nút có thể dựng sẵn trong Editor và gọi SelectLevel(index) qua onClick,
// hoặc sinh động từ 'levels' (làm khi có nhiều Level asset ở Giai đoạn 7).
public class LevelSelectView : MonoBehaviour
{
    [Header("Data / Refs")]
    [SerializeField] private Level[] levels;
    [SerializeField] private Button[] levelButtons;   // song song với levels[] để khóa/mở
    [SerializeField] private LevelDetailView detailView;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        RefreshUnlocks();
    }

    private void OnEnable()
    {
        RefreshUnlocks();   // cập nhật mỗi khi mở lại (sau khi thắng màn -> mở khóa màn kế)
    }

    // Đọc tiến trình đã lưu, khóa nút của màn chưa mở (UC3).
    public void RefreshUnlocks()
    {
        if (levels == null) return;
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null) levels[i].LoadProgress();
            if (levelButtons != null && i < levelButtons.Length && levelButtons[i] != null)
                levelButtons[i].interactable = levels[i] != null && levels[i].isUnlocked;
        }
    }

    // Gọi từ nút màn chơi (gán index trong Editor).
    public void SelectLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length) return;
        Level level = levels[index];
        if (level == null || !level.isUnlocked) return;   // chặn màn đang khóa
        GameFlow.SelectedLevel = level;
        if (detailView != null) detailView.Show(level);
    }

    private void OnBack()
    {
        if (panel != null) panel.SetActive(false);
    }
}
