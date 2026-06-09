using UnityEngine;
using UnityEngine.UI;

// Hộp thoại xác nhận thoát (UC12).
public class ConfirmExitView : MonoBehaviour
{
    [Header("Panel / Buttons")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Refs")]
    [SerializeField] private Player player;

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnYes);
        if (noButton != null) noButton.onClick.AddListener(OnNo);
        // Panel lưu inactive sẵn trong scene; không tự tắt ở Start (view nằm trên chính panel).
    }

    private void OnYes()
    {
        if (player != null) player.exit();
        else Application.Quit();
    }

    private void OnNo()
    {
        if (panel != null) panel.SetActive(false);
    }
}
