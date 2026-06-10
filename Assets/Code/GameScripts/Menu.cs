using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;

    private bool isMenuOpen = true;


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
