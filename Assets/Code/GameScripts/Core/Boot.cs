using UnityEngine;

// Scene khởi động (plan §3): khởi tạo rồi chuyển sang MainMenu.
public class Boot : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Start()
    {
        // TODO Giai đoạn 7: nạp Settings/save trước khi vào menu.
        GameFlow.Load(mainMenuScene);
    }
}
