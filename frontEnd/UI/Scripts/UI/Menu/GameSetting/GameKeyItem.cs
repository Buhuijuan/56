using UnityEngine;

public class GameKeyItem : MonoBehaviour
{
    public string keyId;

    private RectTransform rect;
    private RectTransform parentRect;
    private Vector2 defaultSize;
    private Joystick joystick;
    private Vector2 defaultHandleSize;
    private bool handleSizeCaptured;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        joystick = GetComponent<Joystick>();
        if (rect != null)
            defaultSize = rect.sizeDelta;
    }

    private void Start()
    {
        RefreshFromBinding();
    }

    public void RefreshFromBinding()
    {
        if (rect == null)
            return;

        parentRect = rect.parent.GetComponent<RectTransform>();

        if (!KeyBindingSystem.normalizedPositions.TryGetValue(keyId, out Vector2 norm))
        {
            Debug.LogError($"[GameKeyItem] keyId = {keyId}, but normalizedPositions does NOT contain this key. Current keys = {string.Join(", ", KeyBindingSystem.normalizedPositions.Keys)}");
            return;
        }

        rect.anchoredPosition = DraggableKeyItem.NormalizedToLocal(norm, parentRect);
        ApplyScale();
    }

    private void ApplyScale()
    {
        float scale = KeyBindingSystem.GetScale(keyId);
        rect.sizeDelta = defaultSize * scale;

        if (joystick == null || joystick.handle == null)
            return;

        if (!handleSizeCaptured)
        {
            defaultHandleSize = joystick.handle.sizeDelta;
            handleSizeCaptured = true;
        }

        joystick.handle.sizeDelta = defaultHandleSize * scale;
    }

    public static void RefreshAllInScene()
    {
        foreach (var item in FindObjectsOfType<GameKeyItem>(true))
        {
            if (item != null)
                item.RefreshFromBinding();
        }
    }
}
