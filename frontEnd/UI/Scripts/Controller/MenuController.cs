using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public string currentServerName = "安心大学";

    private IEnumerator Start()
    {
        // 纯后端模式：必须有 token 才能尝试恢复会话
        if (!SessionStore.HasToken)
            yield break;

        // 如果已经有 Me 数据，说明 RestoreSession 在 LaunchController 已经跑过
        if (GameStateStore.Me != null)
            yield break;

        bool completed = false;

        yield return BackendFacade.RestoreSession((success, error) =>
        {
            completed = true;

            if (!success)
            {
                Debug.LogWarning($"Failed to restore session in menu: {error}");
                BackendRuntime.HandleUnauthorized();
            }
        });

        if (!completed)
            BackendRuntime.HandleUnauthorized();
    }

    public void SetServer(string name)
    {
        currentServerName = name;
    }

    public void OnClickEnterGame()
    {
        // 纯后端模式：角色信息全部来自 SessionStore + GameStateStore

        // 1. 没有角色 → 去创建角色
        if (!SessionStore.Current.hasCurrentRole || !GameStateStore.HasRoles)
        {
            GameSceneManager.Instance.SwitchScene("04_Character");
            return;
        }

        // 2. 当前角色信息不完整 → 去创建角色
        if (!GameStateStore.HasCurrentRole || GameStateStore.Home == null || GameStateStore.Home.role == null)
        {
            GameSceneManager.Instance.SwitchScene("04_Character");
            return;
        }

        // 3. 当前角色没有昵称 → 去创建角色
        if (string.IsNullOrEmpty(GameStateStore.Home.role.nickName))
        {
            GameSceneManager.Instance.SwitchScene("04_Character");
            return;
        }

        // 4. 一切正常 → 进入游戏
        StartCoroutine(EnterCampusRoutine());
    }

    private IEnumerator EnterCampusRoutine()
    {
        bool completed = false;

        yield return BackendFacade.RefreshHome(result =>
        {
            completed = true;

            if (!result.Success || result.Data == null || !result.Data.success)
            {
                UIManager.Instance.ShowRemind(
                    "加载失败",
                    "知道了",
                    string.IsNullOrEmpty(result.Message) ? "无法获取主页数据" : result.Message
                );
                return;
            }

            GameSceneManager.Instance.SwitchToGame("05_Game", "05_Campus");
        });

        if (!completed)
        {
            UIManager.Instance.ShowRemind("加载失败", "知道了", "未收到服务器响应");
        }
    }
}
