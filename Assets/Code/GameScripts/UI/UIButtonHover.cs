using UnityEngine;
using UnityEngine.EventSystems;

// Gắn lên 1 nút: phóng to nhẹ khi rê chuột vào, thu lại khi rời ra.
// Thuần trình bày (View), built-in, dùng unscaledTime để chạy cả khi tạm dừng.
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float speed = 12f;

    private Vector3 baseScale;
    private float target = 1f;

    private void Awake() { baseScale = transform.localScale; }
    private void OnDisable() { target = 1f; transform.localScale = baseScale; }

    public void OnPointerEnter(PointerEventData _) { target = hoverScale; }
    public void OnPointerExit(PointerEventData _) { target = 1f; }

    private void Update()
    {
        float cur = transform.localScale.x / Mathf.Max(baseScale.x, 0.0001f);
        float next = Mathf.MoveTowards(cur, target, speed * Time.unscaledDeltaTime);
        transform.localScale = baseScale * next;
    }
}
