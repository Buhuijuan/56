using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// ==========================
// 故事接龙 数据结构（完全对应你最新后端返回）
// ==========================
[Serializable]
public class StoryThemeInfoResponse
{
    public int code;
    public string theme;          // 故事主题
    public string opening;      // 开场文案
    public string remaining_time; // 剩余时间
}


[Serializable]
public class StoryStartResponse
{
    public int code;
    public string theme;
    public string content;
    public string[] options;
    public int round;
    public bool is_end;
}

[Serializable]
public class StoryNextResponse
{
    public int code;
    public string theme;
    public string content;
    public string[] options;
    public int round;
    public bool is_end;
}

[Serializable]
public class StoryBaseResponse
{
    public int code;
    public string msg;
}

[Serializable]
public class StoryForceUpdateResponse
{
    public int code;
    public string theme;
}

[Serializable]
public class StoryHealthResponse
{
    public string status;
}

// ==========================
// 故事专用 API 管理器（和APIManager同风格）
// ==========================
public class StoryAPIManager : MonoBehaviour
{
    public static StoryAPIManager Instance;

    // 【唯一配置】和你的APIManager使用同一个地址
    private string baseUrl = "http://47.109.31.215:8000";

    // ====================== 全局统一存储 ======================
    [HideInInspector] public StoryThemeInfoResponse currentThemeInfo;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====================== 全局统一获取时间 ======================
    public TimeSpan GetRemainingTime()
    {
        if (currentThemeInfo == null)
            return TimeSpan.Zero;

        return ParseBackendTime(currentThemeInfo.remaining_time);
    }

    // ====================== 全局统一解析时间 ======================
    private TimeSpan ParseBackendTime(string timeStr)
    {
        try
        {
            string[] daySplit = timeStr.Split(new[] { " days, ", " day, " }, StringSplitOptions.None);
            int days = int.Parse(daySplit[0]);
            string[] hms = daySplit[1].Split(':');
            int h = int.Parse(hms[0]);
            int m = int.Parse(hms[1]);
            int s = int.Parse(hms[2]);
            return new TimeSpan(days, h, m, s);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }


    // ================================================================================
    // ✅ 已修正：1. GET 获取故事主题信息 /api/story/theme/info
    // ================================================================================
    public void GetStoryThemeInfo(Action<StoryThemeInfoResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_GetStoryThemeInfo(onSuccess, onError));
    }

    IEnumerator Co_GetStoryThemeInfo(Action<StoryThemeInfoResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/theme/info";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryThemeInfoResponse>(request.downloadHandler.text);

                    // ✅ 先保存
                    currentThemeInfo = res;

                    // ✅ 再回调
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 2. POST 开始故事 /api/story/start
    // ================================================================================
    public void StartStory(string userId, string nickname, Action<StoryStartResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_StartStory(userId, nickname, onSuccess, onError));
    }

    IEnumerator Co_StartStory(string userId, string nickname, Action<StoryStartResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/start?user_id={userId}&nickname={UnityWebRequest.EscapeURL(nickname)}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryStartResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 3. POST 下一步（选择选项） /api/story/next
    // ================================================================================
    public void NextStory(string userId, string choice, Action<StoryNextResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_NextStory(userId, choice, onSuccess, onError));
    }

    IEnumerator Co_NextStory(string userId, string choice, Action<StoryNextResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/next?user_id={userId}&choice={UnityWebRequest.EscapeURL(choice)}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryNextResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 4. POST 自定义输入 /api/story/next (custom_choice)
    // ================================================================================
    public void NextStoryCustom(string userId, string customChoice, Action<StoryNextResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_NextStoryCustom(userId, customChoice, onSuccess, onError));
    }

    IEnumerator Co_NextStoryCustom(string userId, string customChoice, Action<StoryNextResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/next?user_id={userId}&custom_choice={UnityWebRequest.EscapeURL(customChoice)}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryNextResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 5. POST 重置故事 /api/story/reset
    // ================================================================================
    public void ResetStory(string userId, Action<StoryBaseResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_ResetStory(userId, onSuccess, onError));
    }

    IEnumerator Co_ResetStory(string userId, Action<StoryBaseResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/reset?user_id={userId}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryBaseResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 6. POST 强制更新主题 /api/story/force_update_theme
    // ================================================================================
    public void ForceUpdateTheme(Action<StoryForceUpdateResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_ForceUpdateTheme(onSuccess, onError));
    }

    IEnumerator Co_ForceUpdateTheme(Action<StoryForceUpdateResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/story/force_update_theme";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryForceUpdateResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    // ================================================================================
    // 7. GET 健康检查 /api/health
    // ================================================================================
    public void CheckHealth(Action<StoryHealthResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(Co_CheckHealth(onSuccess, onError));
    }

    IEnumerator Co_CheckHealth(Action<StoryHealthResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/health";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var res = JsonUtility.FromJson<StoryHealthResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误: " + request.error);
            }
        }
    }

    public string GetCurrentTheme()
    {
        if (currentThemeInfo == null) return "默认主题";
        return currentThemeInfo.theme;
    }


}