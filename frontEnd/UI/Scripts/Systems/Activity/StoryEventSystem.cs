using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StoryEventSystem
{
    public static RoleData currentRole => AccountSystem.GetCurrentRole();
    public static PlayerStoryEventState storyEventState
    {
        get
        {
            var role = AccountSystem.GetCurrentRole();
            if (role == null)
            {
                return new PlayerStoryEventState();
            }
            return role.storyEventState;
        }
    }
    public static List<StoryRecord> myStories = new();
    public static List<StoryRecord> allUploadedStories = new();
    public static StorySessionState session;
    private static List<StoryEventConfig> allEvents;
    private static StoryEventConfig currentConfig;

    static StoryEventSystem()
    {
        LoadAllEvents();
        SelectCurrentEvent();
        BindRewards();
    }

    private static void LoadAllEvents()
    {
        TextAsset json = Resources.Load<TextAsset>("Jsons/StoryEvents");
        allEvents = JsonUtility.FromJson<StoryEventListWrapper>(json.text).storyEvents;

        foreach (StoryEventConfig storyEvent in allEvents)
        {
            if (!DateTime.TryParse(storyEvent.startTimeString, out storyEvent.startTime))
                storyEvent.startTime = DateTime.Today;
        }
    }

    private static void SelectCurrentEvent()
    {
        DateTime today = DateTime.Today;
        currentConfig = allEvents
            .FirstOrDefault(storyEvent => today >= storyEvent.startTime &&
                                          today < storyEvent.startTime.AddDays(storyEvent.durationDays));

        currentConfig ??= allEvents
            .OrderByDescending(storyEvent => storyEvent.startTime)
            .First();

        storyEventState.uploadedStoryIds ??= new HashSet<string>();
        if (storyEventState.eventId != currentConfig.eventId)
        {
            storyEventState.eventId = currentConfig.eventId;
            storyEventState.hasFinished = false;
            storyEventState.rewardClaimed = false;
            storyEventState.currentStoryId = string.Empty;
            session = null;
        }
    }

    private static void BindRewards()
    {
        if (currentConfig == null || currentConfig.rewards == null)
            return;

        foreach (RewardItem reward in currentConfig.rewards)
        {
            RewardItem baseItem = RewardItemSystem.GetRewardItem(reward.rewardId);
            if (baseItem == null)
                continue;

            reward.rewardName = baseItem.rewardName;
            reward.rewardSprite = baseItem.rewardSprite;
            reward.spritePath = baseItem.spritePath;
        }
    }

    public static StoryEventConfig GetCurrentConfig()
    {
        SelectCurrentEvent();
        return currentConfig;
    }

    public static TimeSpan GetTimeLeft()
    {
        // 优先使用后端真实时间
        if (StoryAPIManager.Instance != null)
        {
            var serverTime = StoryAPIManager.Instance.GetRemainingTime();
            if (serverTime > TimeSpan.Zero)
                return serverTime;
        }

        // 降级：本地时间
        DateTime end = currentConfig.startTime.AddDays(currentConfig.durationDays);
        TimeSpan time = end - DateTime.Now;
        return time < TimeSpan.Zero ? TimeSpan.Zero : time;
    }

    public static void StartNewSession()
    {
        SelectCurrentEvent();
        storyEventState.hasFinished = false;
        storyEventState.rewardClaimed = false;
        storyEventState.currentStoryId = string.Empty;

        session = new StorySessionState
        {
            eventId = currentConfig.eventId,
            theme = StoryAPIManager.Instance != null
            ? StoryAPIManager.Instance.GetCurrentTheme()
            : "加载中...",
            segments = new List<StorySegment>(),
            currentRound = 0,
            isFinished = false
        };
    }

    public static void SaveStory()
    {
        if (session == null || session.segments == null || session.segments.Count == 0)
            return;

        string storyId = string.IsNullOrEmpty(storyEventState.currentStoryId)
            ? Guid.NewGuid().ToString("N")
            : storyEventState.currentStoryId;

        System.Text.StringBuilder builder = new();
        foreach (StorySegment segment in session.segments)
        {
            builder.AppendLine(segment.segmentText);
            if (!string.IsNullOrWhiteSpace(segment.userChoice))
                builder.AppendLine($"\u73a9\u5bb6\u9009\u62e9\uff1a{segment.userChoice}");
            builder.AppendLine();
        }

        StoryRecord record = new()
        {
            storyId = storyId,
            authorId = AccountSystem.GetCurrentAccount().accountID,
            theme = session.theme,
            segments = new List<StorySegment>(session.segments),
            fullText = builder.ToString().Trim(),
            createdAt = DateTime.Now
        };

        int existingIndex = myStories.FindIndex(item => item.storyId == storyId);
        if (existingIndex >= 0)
            myStories[existingIndex] = record;
        else
            myStories.Add(record);

        storyEventState.currentStoryId = storyId;
        storyEventState.hasFinished = true;
        storyEventState.lastPlayDate = DateTime.Today;
        TitleEventReporter.ReportStoryFinished(storyId);
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static bool CanClaimReward()
    {
        return storyEventState.hasFinished && !storyEventState.rewardClaimed && GetCurrentStory() != null;
    }

    public static bool ClaimReward()
    {
        if (!CanClaimReward())
            return false;

        storyEventState.rewardClaimed = true;
        LocalProfileSaveSystem.SaveCurrentAccount();
        return true;
    }

    public static bool CanUploadCurrentStory()
    {
        StoryRecord currentStory = GetCurrentStory();
        return currentStory != null && !storyEventState.uploadedStoryIds.Contains(currentStory.storyId);
    }

    public static bool UploadStory()
    {
        StoryRecord currentStory = GetCurrentStory();
        if (currentStory == null || !CanUploadCurrentStory())
            return false;

        storyEventState.uploadedStoryIds.Add(currentStory.storyId);
        if (allUploadedStories.All(item => item.storyId != currentStory.storyId))
            allUploadedStories.Add(currentStory);

        LocalProfileSaveSystem.SaveCurrentAccount();
        return true;
    }

    public static StoryRecord GetCurrentStory()
    {
        if (string.IsNullOrEmpty(storyEventState.currentStoryId))
            return myStories.LastOrDefault();

        return myStories.FirstOrDefault(item => item.storyId == storyEventState.currentStoryId);
    }

    [Serializable]
    public class StoryEventListWrapper
    {
        public List<StoryEventConfig> storyEvents;
    }


}
