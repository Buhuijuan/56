using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectArrowController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Button upButton;
    public Button downButton;

    [Range(0.01f, 0.3f)]
    public float step = 0.1f;

    private bool holdUp = false;
    private bool holdDown = false;

    void Start()
    {
        upButton.onClick.AddListener(() => ScrollUp());
        downButton.onClick.AddListener(() => ScrollDown());

        AddHoldEvent(upButton, () => holdUp = true, () => holdUp = false);
        AddHoldEvent(downButton, () => holdDown = true, () => holdDown = false);
    }

    void Update()
    {
        if (holdUp)
            ScrollUp(Time.deltaTime);

        if (holdDown)
            ScrollDown(Time.deltaTime);
    }

    void ScrollUp(float multiplier = 1f)
    {
        scrollRect.verticalNormalizedPosition =
            Mathf.Clamp01(scrollRect.verticalNormalizedPosition + step * multiplier);
    }

    void ScrollDown(float multiplier = 1f)
    {
        scrollRect.verticalNormalizedPosition =
            Mathf.Clamp01(scrollRect.verticalNormalizedPosition - step * multiplier);
    }

    void AddHoldEvent(Button btn, System.Action onDown, System.Action onUp)
    {
        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

        var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((_) => onDown());
        trigger.triggers.Add(entryDown);

        var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((_) => onUp());
        trigger.triggers.Add(entryUp);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) => onUp());
        trigger.triggers.Add(entryExit);
    }
}
