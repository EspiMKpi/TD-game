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

    private void OnMouseDown()
    {
        if (tower != null) return;

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
