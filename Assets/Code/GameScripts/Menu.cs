using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;

    private bool isMenuOpen = true;
    public bool IsMenuOpen => isMenuOpen;

    private void Start()
    {
        // Đồng bộ animator với trạng thái ban đầu (mở) để hình ảnh khớp logic ngay từ đầu.
        if (anim != null) anim.SetBool("MenuOpen", isMenuOpen);
    }


    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
    }

    private void OnGUI()
    {
        // BUG-04: null-guard tránh NullReferenceException mỗi frame nếu thiếu tham chiếu.
        if (currencyUI != null && Level_Manager.main != null)
            currencyUI.text = Level_Manager.main.currency.ToString();
    }

}
