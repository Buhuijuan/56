using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasAutoBindCamera : MonoBehaviour
{
    void Awake()
    {
        var canvas = GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        var uiCam = GameObject.FindWithTag("UICamera");
        if (uiCam != null)
        {
            canvas.worldCamera = uiCam.GetComponent<Camera>();
        }
    }
}
