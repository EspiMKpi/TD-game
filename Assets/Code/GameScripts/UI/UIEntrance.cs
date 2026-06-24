using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Hiệu ứng UI bằng built-in (coroutine tween, KHÔNG dùng DOTween).
// Thuần trình bày (View): không đụng tới logic game, singleton hay dữ liệu.
// Dùng unscaledTime để chạy được cả khi Time.timeScale = 0 (menu tạm dừng).

// Gắn lên 1 panel/phần tử UI: khi bật (OnEnable) sẽ mờ dần hiện ra + trượt + phóng nhẹ.
[RequireComponent(typeof(CanvasGroup))]
public class UIEntrance : MonoBehaviour
{
    [Header("Thời lượng / độ trễ")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float delay = 0f;

    [Header("Hiệu ứng")]
    [SerializeField] private bool fade = true;
    [SerializeField] private Vector2 slideFrom = new Vector2(0f, -40f); // px lệch lúc bắt đầu
    [SerializeField] private float scaleFrom = 0.94f;                   // tỉ lệ lúc bắt đầu

    private CanvasGroup group;
    private RectTransform rect;
    private Vector2 targetPos;
    private Vector3 targetScale;
    private bool captured;
    private Coroutine running;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        rect = transform as RectTransform;
        Capture();
    }

    // Ghi nhớ vị trí/tỉ lệ "đích" (giá trị thiết kế sẵn trong scene).
    private void Capture()
    {
        if (captured) return;
        if (rect != null) targetPos = rect.anchoredPosition;
        targetScale = transform.localScale;
        captured = true;
    }

    private void OnEnable()
    {
        Capture();
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        float t = 0f;
        if (delay > 0f)
        {
            float d = 0f;
            while (d < delay) { d += Time.unscaledDeltaTime; yield return null; }
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            float e = 1f - (1f - k) * (1f - k); // ease-out quad

            if (fade && group != null) group.alpha = e;
            if (rect != null) rect.anchoredPosition = Vector2.LerpUnclamped(targetPos + slideFrom, targetPos, e);
            transform.localScale = Vector3.LerpUnclamped(targetScale * scaleFrom, targetScale, e);
            yield return null;
        }

        if (fade && group != null) group.alpha = 1f;
        if (rect != null) rect.anchoredPosition = targetPos;
        transform.localScale = targetScale;
        running = null;
    }
}
