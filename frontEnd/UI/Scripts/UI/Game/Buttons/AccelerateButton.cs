using UnityEngine;
using UnityEngine.EventSystems;

public class AccelerateButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (PlayerAgentMove.Instance != null)
            PlayerAgentMove.Instance.isAccelerating = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (PlayerAgentMove.Instance != null)
            PlayerAgentMove.Instance.isAccelerating = false;
    }
}
