using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryDetailPageUI : PageUI
{
    public static StoryDetailPageUI Instance;

    void Awake()
    {
        Instance = this;
    }

    public TMP_Text theme, timeLeft;
    public Text themeDescription;
    public Transform rewardRoot;
    public RewardItemUI rewardItemPrefab;
    public Button startButton;
    public ActivityPanelUI parent;
    public StoryContentPageUI child;

    void OnEnable()
    {
        GetThemeInfoFromBackend();
    }

    // 从后端获取 → 全部交给 StoryAPIManager 存储
    void GetThemeInfoFromBackend()
    {
        StoryAPIManager.Instance.GetStoryThemeInfo(
            onSuccess: (res) =>
            {
                Refresh();
            },
            onError: (err) =>
            {
                Debug.LogError("获取故事主题失败：" + err);
            });
    }

    public void Refresh()
    {
        var api = StoryAPIManager.Instance;

        // 优先使用后端统一数据
        if (api.currentThemeInfo != null)
        {
            theme.text = $"本期主题：{api.currentThemeInfo.theme}";
            themeDescription.text = $"“{api.currentThemeInfo.opening}”";

            TimeSpan time = api.GetRemainingTime();
            timeLeft.text = $"{time.Days}天{time.Hours}小时{time.Minutes}分钟";
        }
        // 兜底本地配置
        else
        {
            var config = StoryEventSystem.GetCurrentConfig();
            theme.text = $"本期主题：{config.theme}";
            themeDescription.text = $"“{config.themeDescription}”";
            TimeSpan time = StoryEventSystem.GetTimeLeft();
            timeLeft.text = $"{time.Days}天{time.Hours}小时{time.Minutes}分钟";
        }

        // ====================== 以下完全不动 ======================
        foreach (Transform child in rewardRoot)
            Destroy(child.gameObject);
        rewardRoot.localScale = new Vector3(0.6f, 0.6f, 1f);
        foreach (var reward in StoryEventSystem.GetCurrentConfig().rewards)
        {
            var item = Instantiate(rewardItemPrefab, rewardRoot);
            item.Setup(reward);
        }

        TMP_Text startLabel = startButton.GetComponentInChildren<TMP_Text>();
        if (startLabel != null)
            startLabel.text = StoryEventSystem.storyEventState.hasFinished ? "继续查看" : "开始创作";

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            if (!StoryEventSystem.storyEventState.hasFinished)
            {
                child.ClearSegment();
                StoryEventSystem.StartNewSession();
            }
            parent.SwitchPage(ActivityPanelUI.ActivityPageState.StoryContent);
        });
    }
}