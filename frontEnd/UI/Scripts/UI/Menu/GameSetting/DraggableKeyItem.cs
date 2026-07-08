using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableKeyItem : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public string keyId;
    public RectTransform selectedRing;

    private RectTransform rect;
    private RectTransform parentRect;
    private Image img;
    private Vector2 defaultSize;
    private Vector2 defaultRingSize;
    private bool ringSizeCaptured;
    private float currentScale = KeyBindingSystem.DefaultScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = rect.parent.GetComponent<RectTransform>();
        img = GetComponent<Image>();
        defaultSize = rect.sizeDelta;
        if (selectedRing != null)
        {
            defaultRingSize = selectedRing.sizeDelta;
            ringSizeCaptured = true;
        }
        SetSelected(false);
    }

    public void InitPosition()
    {
        Vector2 norm = KeyBindingSystem.normalizedPositions[keyId];
        rect.anchoredPosition = NormalizedToLocal(norm, parentRect);
        ApplyScale(KeyBindingSystem.GetScale(keyId), false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (KeyBindingMenuUI.Instance != null)
            KeyBindingMenuUI.Instance.SelectItem(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta;
        ClampToParent();
        RecalculateAllConflicts();
    }

    public float GetCurrentScale()
    {
        return currentScale;
    }

    public void SetSelected(bool selected)
    {
        if (selectedRing != null)
            selectedRing.gameObject.SetActive(selected);
    }

    public void ApplyScale(float scale, bool notifyMenu = true)
    {
        currentScale = Mathf.Clamp(scale, KeyBindingSystem.MinScale, KeyBindingSystem.MaxScale);
        rect.sizeDelta = defaultSize * currentScale;
        if (selectedRing != null)
        {
            if (!ringSizeCaptured)
            {
                defaultRingSize = selectedRing.sizeDelta;
                ringSizeCaptured = true;
            }
            selectedRing.sizeDelta = defaultRingSize * currentScale;
        }
        ClampToParent();
        RecalculateAllConflicts();

        if (notifyMenu && KeyBindingMenuUI.Instance != null)
            KeyBindingMenuUI.Instance.NotifySelectedScaleChanged(this, currentScale);
    }

    private void ClampToParent()
    {
        Rect parent = parentRect.rect;
        Rect self = rect.rect;

        Vector2 pos = rect.anchoredPosition;

        float halfW = self.width / 2;
        float halfH = self.height / 2;

        pos.x = Mathf.Clamp(pos.x, parent.xMin + halfW, parent.xMax - halfW);
        pos.y = Mathf.Clamp(pos.y, parent.yMin + halfH, parent.yMax - halfH);

        rect.anchoredPosition = pos;
    }

    private static void RecalculateAllConflicts()
    {
        var items = FindObjectsOfType<DraggableKeyItem>();
        bool anyConflict = false;

        foreach (var item in items)
        {
            bool itemConflict = false;

            foreach (var other in items)
            {
                if (other == item) continue;
                if (RectOverlaps(item.rect, other.rect))
                {
                    itemConflict = true;
                    anyConflict = true;
                    break;
                }
            }

            if (item.img != null)
                item.img.color = itemConflict ? Color.red : Color.white;
        }
        KeyBindingMenuUI.NotifyConflictChanged(anyConflict);
    }

    private static bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect ra = GetScreenRect(a);
        Rect rb = GetScreenRect(b);
        return ra.Overlaps(rb);
    }

    private static Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        float xMin = corners[0].x;
        float xMax = corners[2].x;
        float yMin = corners[0].y;
        float yMax = corners[2].y;

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public static Vector2 LocalToNormalized(Vector2 local, RectTransform parent)
    {
        float nx = (local.x - parent.rect.xMin) / parent.rect.width;
        float ny = (local.y - parent.rect.yMin) / parent.rect.height;
        return new Vector2(nx, ny);
    }

    public static Vector2 NormalizedToLocal(Vector2 norm, RectTransform parent)
    {
        float x = parent.rect.xMin + norm.x * parent.rect.width;
        float y = parent.rect.yMin + norm.y * parent.rect.height;
        return new Vector2(x, y);
    }
}
