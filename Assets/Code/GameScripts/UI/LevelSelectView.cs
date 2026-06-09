using UnityEngine;
using UnityEngine.UI;

// Chọn màn chơi (UC3). Giữ danh sách Level; chọn 1 màn -> mở LevelDetailView.
// Danh sách nút có thể dựng sẵn trong Editor và gọi SelectLevel(index) qua onClick,
// hoặc sinh động từ 'levels' (làm khi có nhiều Level asset ở Giai đoạn 7).
public class LevelSelectView : MonoBehaviour
{
    [Header("Data / Refs")]
    [SerializeField] private Level[] levels;
    [SerializeField] private LevelDetailView detailView;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    // Gọi từ nút màn chơi (gán index trong Editor).
    public void SelectLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length) return;
        Level level = levels[index];
        GameFlow.SelectedLevel = level;
        if (detailView != null) detailView.Show(level);
    }

    private void OnBack()
    {
        if (panel != null) panel.SetActive(false);
    }
}
