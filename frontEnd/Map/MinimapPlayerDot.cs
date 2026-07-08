using UnityEngine;

public class MinimapPlayerDot : MonoBehaviour
{
    [Header("玩家")]
    public Transform player;

    [Header("小地图UI")]
    public RectTransform minimapView;
    public RectTransform dotRect;

    [Header("地图范围")]
    public Renderer mapRenderer;
    public bool invertX = false;
    public bool invertZ = false;

    private float minX, maxX, minZ, maxZ;

    void Start()
    {
        if (mapRenderer == null)
        {
            Debug.LogError("MinimapPlayerDot: mapRenderer 没有绑定");
            return;
        }

        Bounds b = mapRenderer.bounds;
        minX = b.min.x;
        maxX = b.max.x;
        minZ = b.min.z;
        maxZ = b.max.z;
    }

    void Update()
    {
        if (player == null || minimapView == null || dotRect == null || mapRenderer == null) return;

        Vector3 p = player.position;

        float nx = Mathf.InverseLerp(minX, maxX, p.x);
        float nz = Mathf.InverseLerp(minZ, maxZ, p.z);

        if (invertX) nx = 1f - nx;
        if (invertZ) nz = 1f - nz;

        nx = Mathf.Clamp01(nx);
        nz = Mathf.Clamp01(nz);

        float width = minimapView.rect.width;
        float height = minimapView.rect.height;

        float x = (nx - 0.5f) * width;
        float y = (nz - 0.5f) * height;

        dotRect.anchoredPosition = new Vector2(x, y);
    }
}