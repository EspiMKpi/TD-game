using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;
    private GameObject tower;
    private Color startColor;

    private void Start()
    {
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        sr.color = startColor;
    }

    // UC7 — gọi sau khi bán tháp để ô trống trở lại.
    public void ClearTower()
    {
        tower = null;
    }

    private void OnMouseDown()
    {
        // UC9 — đang "lên đạn" power-up: dành cú click này cho power-up (PowerUpView),
        // không xây tháp / không mở bảng tháp. OnMouseDown chạy trước Update nên IsArmed còn true.
        if (PowerUpView.Instance != null && PowerUpView.Instance.IsArmed) return;

        // UC6/UC7 — ô đã có tháp: mở bảng nâng cấp/bán thay vì xây mới.
        if (tower != null)
        {
            TurretScript turret = tower.GetComponentInChildren<TurretScript>();
            if (turret != null && TowerActionView.Instance != null)
                TowerActionView.Instance.Show(turret, this);
            return;
        }

        Tower towerToBuild = BuildManager.main.GetSelectedTower();

        GameObject built;
        if (GameSession.Instance != null)
        {
            // UC5 — định tuyến qua GameSession (trung tâm điều phối).
            built = GameSession.Instance.buildTower(towerToBuild, transform.position);
        }
        else
        {
            // Fallback: scene chưa có GameSession — giữ luồng cũ qua Level_Manager.
            if (towerToBuild.cost > Level_Manager.main.currency)
            {
                Debug.Log("You can't afford this tower");
                return;
            }
            Level_Manager.main.SpendCurrency(towerToBuild.cost);
            built = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        }

        if (built != null) tower = built;
    }
}
