using UnityEngine;

public class MiniMapFollowSystem : MonoBehaviour
{
    [Header("References")]
    public Camera miniMapCamera;
    public Transform playerOverride;
    public RectTransform playerMarkerUI;
    public bool autoFindPlayerMarker = true;
    public string playerMarkerName = "MiniMapPlayerIcon";

    [Header("Follow")]
    public float fixedHeight = 120f;
    public bool followInLateUpdate = true;
    public float reacquireInterval = 0.5f;

    [Header("Rotation")]
    public bool useFixedRotation = true;
    public Vector3 fixedEulerAngles = new Vector3(90f, 0f, 0f);
    public bool rotateMapWithPlayer = false;
    public float mapYawOffset = 0f;
    public bool rotateMarkerWithPlayer = true;
    public float markerYawOffset = 0f;
    public bool invertMarkerYaw = true;

    private Transform resolvedPlayer;
    private float nextReacquireTime;
    private float nextMarkerReacquireTime;

    private void Awake()
    {
        if (miniMapCamera == null)
            miniMapCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!followInLateUpdate)
            FollowTick();
    }

    private void LateUpdate()
    {
        if (followInLateUpdate)
            FollowTick();
    }

    private void FollowTick()
    {
        if (miniMapCamera == null)
            return;

        ResolvePlayerIfNeeded();
        if (resolvedPlayer == null)
            return;

        ResolveMarkerIfNeeded();

        Vector3 p = resolvedPlayer.position;
        miniMapCamera.transform.position = new Vector3(p.x, fixedHeight, p.z);

        float playerYaw = resolvedPlayer.eulerAngles.y;

        if (rotateMapWithPlayer)
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(
                fixedEulerAngles.x,
                playerYaw + mapYawOffset,
                fixedEulerAngles.z);
        }
        else if (useFixedRotation)
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(fixedEulerAngles);
        }

        if (playerMarkerUI != null)
        {
            float markerZ = 0f;
            if (rotateMarkerWithPlayer && !rotateMapWithPlayer)
            {
                markerZ = invertMarkerYaw ? -playerYaw : playerYaw;
                markerZ += markerYawOffset;
            }
            else
            {
                markerZ = markerYawOffset;
            }

            playerMarkerUI.localRotation = Quaternion.Euler(0f, 0f, markerZ);
        }
    }

    private void ResolveMarkerIfNeeded()
    {
        if (!autoFindPlayerMarker || playerMarkerUI != null)
            return;

        if (Time.unscaledTime < nextMarkerReacquireTime)
            return;

        nextMarkerReacquireTime = Time.unscaledTime + Mathf.Max(0.1f, reacquireInterval);

        RectTransform[] rects = Object.FindObjectsOfType<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rt = rects[i];
            if (rt == null)
                continue;

            if (!string.IsNullOrWhiteSpace(playerMarkerName) && rt.name == playerMarkerName)
            {
                playerMarkerUI = rt;
                return;
            }
        }
    }

    private void ResolvePlayerIfNeeded()
    {
        if (playerOverride != null)
        {
            resolvedPlayer = playerOverride;
            return;
        }

        if (resolvedPlayer != null && resolvedPlayer.gameObject.activeInHierarchy)
            return;

        if (Time.unscaledTime < nextReacquireTime)
            return;

        nextReacquireTime = Time.unscaledTime + Mathf.Max(0.1f, reacquireInterval);

        if (PlayerAgentMove.Instance != null)
        {
            resolvedPlayer = PlayerAgentMove.Instance.transform;
            return;
        }

        PlayerAgentMove agent = Object.FindObjectOfType<PlayerAgentMove>(true);
        if (agent != null)
        {
            resolvedPlayer = agent.transform;
            return;
        }

        PlayerMove move = Object.FindObjectOfType<PlayerMove>(true);
        if (move != null)
            resolvedPlayer = move.transform;
    }
}
