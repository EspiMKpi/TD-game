using UnityEngine;

// Chọn map theo Level đã chọn (UC3). Bật map ứng với levelId, nạp đường đi của map đó
// vào Level_Manager và dời Base về cuối đường. Vẫn dùng chung 1 scene Gameplay.
// Chạy sớm (trước khi địch sinh) để Level_Manager có path đúng.
[DefaultExecutionOrder(-50)]
public class MapSelector : MonoBehaviour
{
    [SerializeField] private MapData[] maps;   // theo thứ tự màn: maps[0] = màn 1, ...
    [SerializeField] private Base theBase;

    private void Awake()
    {
        if (maps == null || maps.Length == 0) return;

        int id = GameFlow.SelectedLevel != null ? GameFlow.SelectedLevel.levelId : 1;
        int idx = Mathf.Clamp(id - 1, 0, maps.Length - 1);

        for (int i = 0; i < maps.Length; i++)
            if (maps[i] != null) maps[i].gameObject.SetActive(i == idx);

        var m = maps[idx];
        if (m == null) return;
        var lm = Object.FindObjectOfType<Level_Manager>();
        if (lm == null) return;

        if (m.path != null && m.path.Length > 0) lm.path = m.path;
        if (m.startPoint != null) lm.startPoint = m.startPoint;
        if (theBase != null && m.path != null && m.path.Length > 0)
            theBase.transform.position = m.path[m.path.Length - 1].position;
    }
}
