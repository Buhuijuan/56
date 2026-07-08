using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform bg;
    public RectTransform handle;

    [Header("Input")]
    public float sensitivity = 1f;
    [Range(0f, 0.2f)]
    public float deadZone = 0.05f;

    public Vector2 Direction { get; private set; }

    private float Radius
    {
        get
        {
            if (bg == null) return 100f;
            return Mathf.Min(bg.rect.width, bg.rect.height) * 0.5f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 不要在按下的一瞬间直接调用 OnDrag
        // 否则轻点一下 Handle 就会明显跳动
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (bg == null || handle == null) return;

        Camera cam = eventData.pressEventCamera ?? eventData.enterEventCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bg, eventData.position, cam, out Vector2 localPos))
            return;

        float r = Radius;

        // 转成 -1 ~ 1
        Vector2 normalized = localPos / r;

        // 限制在圆形范围内
        if (normalized.magnitude > 1f)
            normalized = normalized.normalized;

        // 死区
        if (normalized.magnitude < deadZone)
            normalized = Vector2.zero;

        handle.anchoredPosition = normalized * r;

        Direction = Vector2.ClampMagnitude(normalized * sensitivity, 1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        Direction = Vector2.zero;
    }
}