// Trạng thái ván đấu — thay cho thiết kế gốc 'status: boolean' (mục 2.3 kế hoạch).
// 3 trạng thái thay vì 2 để phân biệt rõ đang chơi / thắng / thua.
public enum GameStatus
{
    Playing,
    Won,
    Lost
}
