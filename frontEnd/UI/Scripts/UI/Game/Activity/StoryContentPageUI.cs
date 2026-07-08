using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryContentPageUI : PageUI
{
    public Transform contentRoot;
    public StorySegmentItemUI segmentPrefab;

    [Header("Loading UI")]
    public GameObject loadingRoot;
    public TMP_Text loadingText;

    private Coroutine _loadingAnimationCoroutine;
    private Coroutine _waitingForStoryCoroutine;

    void OnEnable()
    {
        // 显示Loading，开始等待
        ShowLoading(true);

        // 必须等待后端主题加载完成
        if (StoryAPIManager.Instance == null || StoryAPIManager.Instance.currentThemeInfo == null)
        {
            Debug.Log("等待后端主题中...");
            // 启动一个协程持续检查，直到主题加载完成
            StartCoroutine(WaitForThemeAndStart());
            return;
        }

        // 只有主题拿到了，才开始故事
        if (StoryEventSystem.session == null || StoryEventSystem.session.currentRound == 0)
        {
            StartStoryFromBackend();
        }
        else
        {
            // 已有故事会话，直接显示已有内容，隐藏Loading
            ShowLoading(false);
        }
    }

    void OnDisable()
    {
        // 页面关闭时停止所有动画协程
        if (_loadingAnimationCoroutine != null)
            StopCoroutine(_loadingAnimationCoroutine);
        if (_waitingForStoryCoroutine != null)
            StopCoroutine(_waitingForStoryCoroutine);
    }

    private IEnumerator WaitForThemeAndStart()
    {
        float timeout = 10f; // 10秒超时
        float elapsed = 0f;

        while ((StoryAPIManager.Instance == null || StoryAPIManager.Instance.currentThemeInfo == null) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (StoryAPIManager.Instance != null && StoryAPIManager.Instance.currentThemeInfo != null)
        {
            // 主题加载完成，开始故事
            if (StoryEventSystem.session == null || StoryEventSystem.session.currentRound == 0)
            {
                StartStoryFromBackend();
            }
            else
            {
                ShowLoading(false);
            }
        }
        else
        {
            // 超时，显示错误提示
            ShowLoading(false);
            Debug.LogError("加载故事主题超时");
            UIManager.Instance?.ShowRemind("加载失败", "知道了", "无法加载故事内容，请稍后重试。");
        }
    }

    // ================================================================================
    // 【核心】从后端开始故事（替代假AI）
    // ================================================================================
    void StartStoryFromBackend()
    {
        string userId = AccountSystem.GetCurrentAccount()?.accountID ?? "test123";
        string nickname = CharacterSystem.GetCurrentNickName();

        StoryAPIManager.Instance.StartStory(userId, nickname,
            onSuccess: (res) =>
            {
                // 后端返回成功，隐藏Loading
                ShowLoading(false);

                // 把后端返回的第一段故事，填入前端UI结构
                StorySegment firstSeg = new StorySegment();
                firstSeg.segmentText = res.content;
                firstSeg.options = new List<string>(res.options);

                StoryEventSystem.session.segments.Add(firstSeg);
                CreateSegmentUI(0, firstSeg, isLastRound: res.is_end);
            },
            onError: (err) =>
            {
                // 出错时隐藏Loading并显示错误
                ShowLoading(false);
                Debug.LogError("故事启动失败：" + err);
                UIManager.Instance?.ShowRemind("启动失败", "知道了", $"无法开始故事：{err}");
            });
    }

    // ================================================================================
    // 创建一段故事UI（完全保留你原来的UI逻辑）
    // ================================================================================
    void CreateSegmentUI(int segCount, StorySegment seg, bool isLastRound)
    {
        var item = Instantiate(segmentPrefab, contentRoot);
        item.Setup(seg, OnPlayerChoose, isLastRound);
    }

    // ================================================================================
    // 玩家选择后 → 调用后端获取下一段（完全保留你的锁定、动画、交互）
    // ================================================================================
    void OnPlayerChoose(string choice)
    {
        LockLastSegment(choice);
        string userId = AccountSystem.GetCurrentAccount()?.accountID ?? "test123";

        // 在选择后显示Loading，等待下一段故事生成
        ShowLoading(true);

        // 发送选择给后端，获取下一段
        StoryAPIManager.Instance.NextStory(userId, choice,
            onSuccess: (res) =>
            {
                // 后端返回成功，隐藏Loading
                ShowLoading(false);

                // 后端返回的新段落
                StorySegment nextSeg = new StorySegment();
                nextSeg.segmentText = res.content;
                nextSeg.options = new List<string>(res.options);

                // 加入前端会话
                StoryEventSystem.session.segments.Add(nextSeg);
                StoryEventSystem.session.currentRound = res.round;

                // 创建UI（后端告诉你是否结束，前端只负责显示）
                CreateSegmentUI(
                    StoryEventSystem.session.segments.Count,
                    nextSeg,
                    isLastRound: res.is_end
                );

                // 如果后端说结束，标记完成
                if (res.is_end)
                {
                    StoryEventSystem.session.isFinished = true;
                    StoryEventSystem.SaveStory();
                }
            },
            onError: (err) =>
            {
                // 出错时隐藏Loading并显示错误
                ShowLoading(false);
                Debug.LogError("获取下一段故事失败：" + err);
                UIManager.Instance?.ShowRemind("加载失败", "知道了", $"无法继续故事：{err}");
            });
    }

    // ================================================================================
    // 锁定最后一段（完全保留你原来的逻辑）
    // ================================================================================
    void LockLastSegment(string choice)
    {
        if (contentRoot.childCount == 0) return;
        var last = contentRoot.GetChild(contentRoot.childCount - 1).GetComponent<StorySegmentItemUI>();
        last.LockInteraction(choice);
    }

    // ================================================================================
    // 清空UI（保留）
    // ================================================================================
    public void ClearSegment()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);
    }

    // ================================================================================
    // Loading UI 控制
    // ================================================================================
    private void ShowLoading(bool show)
    {
        if (loadingRoot == null)
        {
            Debug.LogWarning("StoryContentPageUI: loadingRoot 未设置");
            return;
        }

        loadingRoot.SetActive(show);

        if (show)
        {
            // 启动文字动画和图标旋转
            if (_loadingAnimationCoroutine != null)
                StopCoroutine(_loadingAnimationCoroutine);
            _loadingAnimationCoroutine = StartCoroutine(LoadingAnimation());
        }
        else
        {
            // 停止动画
            if (_loadingAnimationCoroutine != null)
            {
                StopCoroutine(_loadingAnimationCoroutine);
                _loadingAnimationCoroutine = null;
            }

            // 重置文字为原始状态
            if (loadingText != null)
                loadingText.text = "小精灵努力创作中...";
        }
    }

    /// <summary>
    /// Loading 文字动画：三个点循环出现和消失
    /// </summary>
    private IEnumerator LoadingAnimation()
    {
        if (loadingText == null)
            yield break;

        string baseText = "小精灵努力创作中";
        int dotCount = 0;
        float animationSpeed = 0.5f; // 每0.5秒变化一次

        while (true)
        {
            // 更新点号数量：0 -> 1 -> 2 -> 3 -> 0 循环
            dotCount = (dotCount + 1) % 4;
            string dots = new string('.', dotCount);
            loadingText.text = $"{baseText}{dots}";

            yield return new WaitForSeconds(animationSpeed);
        }
    }
}