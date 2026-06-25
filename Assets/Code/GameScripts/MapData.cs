using UnityEngine;

// Dữ liệu map cục bộ cho 1 màn: điểm xuất phát + danh sách waypoint đường đi.
// MapSelector nạp các giá trị này vào Level_Manager khi chọn màn (giữ 1 scene Gameplay dùng chung).
public class MapData : MonoBehaviour
{
    public Transform startPoint;
    public Transform[] path;
}
