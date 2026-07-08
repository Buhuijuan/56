using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    [System.Serializable]
    public class TeleportPoint
    {
        public string regionName;      // 区域名称（与按钮对应）
        public string displayName;     // 显示名称（如"教学楼区"）
        public Vector3 position;       // 传送位置
        public Quaternion rotation;    // 传送后的朝向
        public GameObject visualMarker; // 可选：场景中的标记物体（用于获取位置）
    }

    [Header("传送点配置")]
    public TeleportPoint[] teleportPoints;

    [Header("特效")]
    public GameObject teleportEffectPrefab;  // 传送特效预制体
    public float teleportDelay = 0.1f;       // 传送延迟

    private bool isCampusSceneLoaded = false;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[TeleportManager] 初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 监听场景加载
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "05_Campus")
        {
            isCampusSceneLoaded = true;
            Debug.Log("[TeleportManager] 校园场景已加载，传送功能可用");

            // 可选：延迟一帧确保玩家完全生成
            Invoke(nameof(InitializePlayerReferences), 0.1f);
        }
        else
        {
            isCampusSceneLoaded = false;
        }
    }

    private void InitializePlayerReferences()
    {
        // 预热玩家引用，确保后续传送快速执行
        var player = PlayerAgentMove.Instance;
        Debug.Log($"[TeleportManager] 玩家引用已初始化: {(player != null ? "成功" : "失败")}");
    }

    /// <summary>
    /// 传送玩家到指定区域（通过名称）
    /// </summary>
    public void TeleportToRegion(string regionName)
    {
        if (!isCampusSceneLoaded)
        {
            Debug.LogWarning($"[TeleportManager] 校园场景未加载，无法传送");
            return;
        }

        // 查找传送点
        TeleportPoint targetPoint = System.Array.Find(teleportPoints, p => p.regionName == regionName);
        if (targetPoint == null)
        {
            Debug.LogWarning($"[TeleportManager] 未找到区域: {regionName}");
            return;
        }

        // 获取玩家
        PlayerAgentMove playerMove = PlayerAgentMove.Instance;
        if (playerMove == null)
        {
            Debug.LogWarning("[TeleportManager] 未找到玩家实例");
            return;
        }

        // 执行传送
        StartCoroutine(ExecuteTeleport(playerMove, targetPoint));
    }

    /// <summary>
    /// 传送玩家到指定区域（通过索引）
    /// </summary>
    public void TeleportToRegion(int index)
    {
        if (index >= 0 && index < teleportPoints.Length)
        {
            TeleportToRegion(teleportPoints[index].regionName);
        }
        else
        {
            Debug.LogWarning($"[TeleportManager] 无效的索引: {index}");
        }
    }

    private System.Collections.IEnumerator ExecuteTeleport(PlayerAgentMove player, TeleportPoint point)
    {
        // 可选：播放传送开始特效
        PlayEffect(player.transform.position, true);

        // 可选：延迟传送，让特效有时间播放
        if (teleportDelay > 0)
        {
            yield return new WaitForSeconds(teleportDelay);
        }

        // 记录旧位置
        Vector3 oldPos = player.transform.position;

        // 执行传送
        player.transform.position = point.position;
        player.transform.rotation = point.rotation;

        // 重置 NavMeshAgent（如果有）
        if (player.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            agent.Warp(point.position);
            agent.velocity = Vector3.zero;
        }

        // 重置动画状态（避免动画卡死）
        if (player.animator != null)
        {
            player.animator.enabled = false;
            player.animator.enabled = true;
        }

        // 可选：播放传送完成特效
        PlayEffect(point.position, false);

        Debug.Log($"[TeleportManager] 玩家已传送到: {point.displayName} ({point.regionName}), 位置: {point.position}");

        // 可选：显示提示
        UIManager.Instance?.ShowRemind("传送完成", "知道了", $"已到达 {point.displayName}", null, null);
    }

    private void PlayEffect(Vector3 position, bool isStart)
    {
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    /// <summary>
    /// 获取传送点位置（供外部使用）
    /// </summary>
    public Vector3 GetRegionPosition(string regionName)
    {
        TeleportPoint point = System.Array.Find(teleportPoints, p => p.regionName == regionName);
        return point != null ? point.position : Vector3.zero;
    }

    /// <summary>
    /// 获取传送点信息列表
    /// </summary>
    public TeleportPoint[] GetAllTeleportPoints()
    {
        return teleportPoints;
    }

#if UNITY_EDITOR
    // 编辑器下可视化传送点位置
    private void OnDrawGizmosSelected()
    {
        if (teleportPoints == null) return;

        foreach (var point in teleportPoints)
        {
            if (point == null) continue;

            // 绘制球体标记位置
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point.position, 0.5f);

            // 绘制方向指示线
            Gizmos.color = Color.blue;
            Vector3 forward = point.rotation * Vector3.forward;
            Gizmos.DrawRay(point.position, forward * 2f);

            // 绘制文字标签
            UnityEditor.Handles.Label(point.position + Vector3.up * 1f, point.displayName);
        }
    }
#endif
}