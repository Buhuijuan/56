using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanelUI : MonoBehaviour
{
    public List<Button> buttons;
    public List<string> regions;
    public Button closeButton;

    [Header("Big Map Marker")]
    public RectTransform mapAreaRect;
    public RectTransform bigMapPlayerMarker;
    public bool autoFindMarkerByName = true;
    public string markerObjectName = "BigMapPlayerMarker";

    [Header("World Bounds")]
    public bool autoFindBoundsByName = true;
    public string boundNWName = "MapBound_NW";
    public string boundNEName = "MapBound_NE";
    public string boundSWName = "MapBound_SW";
    public string boundSEName = "MapBound_SE";
    public Transform mapBoundNW;
    public Transform mapBoundNE;
    public Transform mapBoundSW;
    public Transform mapBoundSE;

    [Header("Mapping")]
    public bool invertX;
    public bool invertY;

    private void Awake()
    {
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                if (button == null) continue;
                Image img = button.GetComponent<Image>();
                if (img != null)
                    img.alphaHitTestMinimumThreshold = 0.1f;
            }
        }

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshPlayerMarker();
    }

    private void Start()
    {
        BindButtons();
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void BindButtons()
    {
        if (buttons == null || regions == null)
            return;

        for (int i = 0; i < buttons.Count && i < regions.Count; i++)
        {
            int index = i;
            if (buttons[index] != null)
                buttons[index].onClick.AddListener(() => OnRegionClick(regions[index]));
        }
    }

    private void OnRegionClick(string regionId)
    {
        Debug.Log($"[MapPanel] Click region: {regionId}");

        if (TeleportManager.Instance == null)
        {
            Debug.LogError("[MapPanel] TeleportManager missing in 00_Persistent.");
            UIManager.Instance?.ShowRemind("传送失败", "知道了", "系统未就绪，请稍后重试");
            return;
        }

        gameObject.SetActive(false);
        TeleportManager.Instance.TeleportToRegion(regionId);
    }

    public void RefreshPlayerMarker()
    {
        ResolveReferencesIfNeeded();

        if (mapAreaRect == null || bigMapPlayerMarker == null)
            return;

        if (!TryGetPlayerPosition(out Vector3 playerPos))
            return;

        if (!TryGetBounds(out float minX, out float maxX, out float minZ, out float maxZ))
            return;

        if (Mathf.Approximately(maxX, minX) || Mathf.Approximately(maxZ, minZ))
            return;

        float u = Mathf.InverseLerp(minX, maxX, playerPos.x);
        float v = Mathf.InverseLerp(minZ, maxZ, playerPos.z);
        if (invertX) u = 1f - u;
        if (invertY) v = 1f - v;
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);

        Rect rect = mapAreaRect.rect;
        Vector2 mapLocalPoint = new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, u),
            Mathf.Lerp(rect.yMin, rect.yMax, v));

        Vector3 worldPoint = mapAreaRect.TransformPoint(mapLocalPoint);
        RectTransform markerParent = bigMapPlayerMarker.parent as RectTransform;
        if (markerParent == null)
            return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(markerParent, screenPoint, null, out Vector2 markerLocal))
            bigMapPlayerMarker.anchoredPosition = markerLocal;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (mapAreaRect == null)
            mapAreaRect = GetComponent<RectTransform>();

        if (autoFindMarkerByName && bigMapPlayerMarker == null && !string.IsNullOrWhiteSpace(markerObjectName))
        {
            RectTransform[] rects = Object.FindObjectsOfType<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rt = rects[i];
                if (rt != null && rt.name == markerObjectName)
                {
                    bigMapPlayerMarker = rt;
                    break;
                }
            }
        }

        if (!autoFindBoundsByName)
            return;

        if (mapBoundNW == null) mapBoundNW = FindTransformByName(boundNWName);
        if (mapBoundNE == null) mapBoundNE = FindTransformByName(boundNEName);
        if (mapBoundSW == null) mapBoundSW = FindTransformByName(boundSWName);
        if (mapBoundSE == null) mapBoundSE = FindTransformByName(boundSEName);
    }

    private bool TryGetBounds(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        minX = maxX = minZ = maxZ = 0f;

        List<Transform> points = new List<Transform>();
        if (mapBoundNW != null) points.Add(mapBoundNW);
        if (mapBoundNE != null) points.Add(mapBoundNE);
        if (mapBoundSW != null) points.Add(mapBoundSW);
        if (mapBoundSE != null) points.Add(mapBoundSE);

        if (points.Count < 2)
            return false;

        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minZ = float.PositiveInfinity;
        maxZ = float.NegativeInfinity;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = points[i].position;
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z;
            if (p.z > maxZ) maxZ = p.z;
        }

        return true;
    }

    private static bool TryGetPlayerPosition(out Vector3 pos)
    {
        if (PlayerAgentMove.Instance != null)
        {
            pos = PlayerAgentMove.Instance.transform.position;
            return true;
        }

        PlayerAgentMove agent = Object.FindObjectOfType<PlayerAgentMove>(true);
        if (agent != null)
        {
            pos = agent.transform.position;
            return true;
        }

        PlayerMove move = Object.FindObjectOfType<PlayerMove>(true);
        if (move != null)
        {
            pos = move.transform.position;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    private static Transform FindTransformByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Transform[] all = Object.FindObjectsOfType<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t != null && t.name == name)
                return t;
        }

        return null;
    }
}
