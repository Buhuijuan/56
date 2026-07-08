using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AIResponse
{
    public int code;
    public string msg;
    public string request_id;
    public AIData data;
}

[Serializable]
public class AIData
{
    public string answer;
    public string category;
    public string target_scene;

    public TargetPosition target_position;
}

[Serializable]
public class TargetPosition
{
    public float x;
    public float y;
    public float z;
}

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    // 【唯一配置】你的后端地址，只需改这里
    private string baseUrl = "http://47.109.31.215:8000";

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

    /// <summary>
    /// 通用AI问答请求（支持所有NPC/所有模块）
    /// </summary>
    /// <param name="question">用户问题</param>
    /// <param name="playerId">用户ID（多轮记忆必须）</param>
    /// <param name="category">NPC类型(必须）：echo / default/grass_cow/gardener/librarian/dorm_manager/volunteer/security/nurse</param>
    /// <param name="scene">场景（可空）</param>
    /// <param name="onSuccess">成功回调</param>
    /// <param name="onError">失败回调</param>
    public void RequestQA(
        string question,
        string playerId,
        string category,
        string scene,
        Action<AIResponse> onSuccess,
        Action<string> onError)
    {
        StartCoroutine(Co_RequestQA(question, playerId, category, scene, onSuccess, onError));
    }

    IEnumerator Co_RequestQA(
        string question,
        string playerId,
        string category,
        string scene,
        Action<AIResponse> onSuccess,
        Action<string> onError)
    {
        // 构建URL：所有参数外部传入，APIManager不写死
        string url = $"{baseUrl}/api/campus/qa" +
                     $"?question={UnityWebRequest.EscapeURL(question)}" +
                     $"&player_id={playerId}" +
                     $"&category={category}" +
                     $"&scene_name={scene}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    AIResponse res = JsonUtility.FromJson<AIResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(res);
                }
                catch (Exception e)
                {
                    onError?.Invoke("解析失败：" + e.Message);
                }
            }
            else
            {
                onError?.Invoke("网络错误：" + request.error);
            }
        }
    }
}