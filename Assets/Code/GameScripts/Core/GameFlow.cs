using UnityEngine.SceneManagement;

// Điều phối luồng scene Boot -> MainMenu -> Gameplay và giữ màn chơi đang chọn.
// Plan §3: màn chơi được chọn lưu vào biến tĩnh để scene Gameplay đọc khi load.
public static class GameFlow
{
    public static Level SelectedLevel;

    public static void Load(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
    }
}
