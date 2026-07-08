using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskIntroductionCardSystem
{
    private static Dictionary<string, IntroductionCardConfig> configMap;
    private static readonly Dictionary<string, BackendElfPrompt> progressPromptMap = new()
    {
        ["M_2_1"] = CreateProgressPrompt("M_2_1", "沿着这条小路往前走就能看到湖了。", "缙湖是校园的灵魂景观，也是热门打卡地。"),
        ["M_2_2"] = CreateProgressPrompt("M_2_2", "从湖边往南走就能看到植物园入口。", "每个区域都有自己的特色。"),
        ["M_2_3"] = CreateProgressPrompt("M_2_3", "图书馆就在前面那栋高大的建筑里。", "以后这里会是你自习、查资料的重要地点。"),
        ["M_3_1"] = CreateProgressPrompt("M_3_1", "看，那栋白色外墙、有弧形楼梯的建筑就是综合楼。", "你的第一节课就在这里。"),
        ["M_3_2"] = CreateProgressPrompt("M_3_2", "第一教学楼主要是各专业的专业课教室。", "很多学院的学生都会来这里上课。"),
        ["M_3_3"] = CreateProgressPrompt("M_3_3", "第一实验楼是理工科同学常来的地方。", "实验室一般需要提前预约，记得遵守安全规定。"),
        ["M_3_4"] = CreateProgressPrompt("M_3_4", "艺术楼里经常有展览、音乐会和演出。", "路过时偶尔还能听到琴声或歌声哦。")
    };

    public static bool TryGetConfig(long targetId, out IntroductionCardConfig config)
    {
        config = null;

        if (!TaskTargetIds.TryGetAnchorLocationId(targetId, out string locationId))
            return false;

        EnsureLoaded();
        return !string.IsNullOrEmpty(locationId) &&
               configMap != null &&
               configMap.TryGetValue(locationId, out config) &&
               config != null;
    }

    public static bool ShouldUseIntroductionCard(BackendTaskEntry task)
    {
        if (task == null || task.targetType != "arrive_building")
            return false;

        return task.taskCode switch
        {
            "M_2_1" => true,
            "M_2_2" => true,
            "M_2_3" => true,
            "M_3_1" => true,
            "M_3_2" => true,
            "M_3_3" => true,
            "M_3_4" => true,
            _ => false
        };
    }

    public static bool TryGetProgressPrompt(string taskCode, out BackendElfPrompt prompt)
    {
        prompt = null;
        return !string.IsNullOrWhiteSpace(taskCode) && progressPromptMap.TryGetValue(taskCode, out prompt) && prompt != null;
    }

    private static void EnsureLoaded()
    {
        if (configMap != null)
            return;

        configMap = new Dictionary<string, IntroductionCardConfig>();
        TextAsset json = Resources.Load<TextAsset>("Jsons/IntroductionCardConfig");
        if (json == null || string.IsNullOrWhiteSpace(json.text))
            return;

        IntroductionCardConfigListWrapper wrapper = JsonUtility.FromJson<IntroductionCardConfigListWrapper>(json.text);
        if (wrapper?.cards == null)
            return;

        foreach (IntroductionCardConfig card in wrapper.cards.Where(card => card != null && !string.IsNullOrWhiteSpace(card.locationId)))
            configMap[card.locationId] = card;
    }

    private static BackendElfPrompt CreateProgressPrompt(string taskCode, params string[] contents)
    {
        return new BackendElfPrompt
        {
            taskCode = taskCode,
            stage = "progress",
            npcName = "AI小精灵",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = contents != null ? new List<string>(contents) : new List<string>()
        };
    }
}
