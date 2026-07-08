using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ClockInEventSystem
{
    public static RoleData currentRole => AccountSystem.GetCurrentRole();
    public static PlayerClockInEventState clockInEventState => currentRole.clockInEventState;

    private const float DefaultCheckRadius = 100f;
    private const int DailyLocationCount = 2;
    private const bool EnableTimeRestriction = false;

    private static readonly Dictionary<string, Vector3> resolvedPositions = new();
    private static readonly Dictionary<string, ClockInEventConfig> locationSourceConfigs = new();
    private static List<ClockInLocation> allLocations;
    private static ClockInEventConfig currentConfig;
    private static ClockInLocation todayLocation;
    private static int lastResolvedSceneHandle = int.MinValue;

    static ClockInEventSystem()
    {
        LoadLocationPool();
        SelectTodayLocations();
        InitializeTodayState();
    }

    private static void LoadLocationPool()
    {
        TextAsset[] allConfigs = Resources.LoadAll<TextAsset>("Jsons/ClockInEvents");
        allLocations = new List<ClockInLocation>();
        locationSourceConfigs.Clear();

        foreach (TextAsset asset in allConfigs)
        {
            ClockInEventConfig config = JsonUtility.FromJson<ClockInEventConfig>(asset.text);
            if (config == null || config.locations == null)
                continue;

            NormalizeConfig(config);

            foreach (ClockInLocation location in config.locations)
            {
                if (location == null || string.IsNullOrEmpty(location.locationId))
                    continue;

                if (allLocations.All(existing => existing.locationId != location.locationId))
                {
                    allLocations.Add(CloneLocation(location));
                    locationSourceConfigs[location.locationId] = config;
                }
            }
        }

        if (allLocations.Count == 0)
            Debug.LogWarning("ClockInEventSystem: no clock-in locations found.");
    }

    private static void NormalizeConfig(ClockInEventConfig config)
    {
        if (!TimeSpan.TryParse(config.refreshTimeString, out config.refreshTime))
            config.refreshTime = new TimeSpan(5, 0, 0);

        if (!TimeSpan.TryParse(config.openStartTimeString, out config.openStartTime))
            config.openStartTime = new TimeSpan(6, 0, 0);

        if (!TimeSpan.TryParse(config.openEndTimeString, out config.openEndTime))
            config.openEndTime = new TimeSpan(9, 0, 0);

        config.description ??= "前往今日随机校园地标，完成晨光打卡。";
        config.rewards = BuildDefaultRewards();
    }

    private static List<RewardItem> BuildDefaultRewards()
    {
        RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
        return new List<RewardItem>
        {
            new RewardItem
            {
                rewardId = 1,
                rewardName = "纪念币",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = 10
            }
        };
    }

    private static void SelectTodayLocations()
    {
        if (allLocations == null || allLocations.Count == 0)
        {
            currentConfig = null;
            todayLocation = null;
            return;
        }

        int seed = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;
        int firstIndex = Mathf.Abs(seed) % allLocations.Count;
        List<ClockInLocation> todayLocations = new();
        ClockInEventConfig sourceConfig = null;

        for (int offset = 0; offset < allLocations.Count && todayLocations.Count < Mathf.Min(DailyLocationCount, allLocations.Count); offset++)
        {
            int index = (firstIndex + offset) % allLocations.Count;
            ClockInLocation candidate = CloneLocation(allLocations[index]);

            if (!TryResolveLocationPosition(candidate, out Vector3 resolvedPosition))
            {
                Debug.LogWarning($"ClockInEventSystem: skip unresolved landmark '{candidate.locationId}' / '{candidate.name}'.");
                continue;
            }

            candidate.worldPosition = resolvedPosition;
            todayLocations.Add(candidate);

            if (sourceConfig == null && locationSourceConfigs.TryGetValue(candidate.locationId, out ClockInEventConfig config))
                sourceConfig = config;
        }

        if (todayLocations.Count == 0)
        {
            Debug.LogError("ClockInEventSystem: no real scene landmarks could be resolved. Clock-in locations are disabled until landmarks can be found in scene.");
            currentConfig = new ClockInEventConfig
            {
                eventId = $"clockin_{DateTime.Today:yyyy_MM_dd}",
                refreshTimeString = "05:00:00",
                refreshTime = new TimeSpan(5, 0, 0),
                openStartTimeString = "06:00:00",
                openEndTimeString = "09:00:00",
                openStartTime = new TimeSpan(6, 0, 0),
                openEndTime = new TimeSpan(9, 0, 0),
                description = "未找到可用的校园地标，请检查场景中的真实地标对象。",
                rewards = BuildDefaultRewards(),
                locations = new List<ClockInLocation>()
            };
            todayLocation = null;
            return;
        }

        todayLocation = todayLocations.FirstOrDefault();
        currentConfig = new ClockInEventConfig
        {
            eventId = $"clockin_{DateTime.Today:yyyy_MM_dd}",
            refreshTimeString = sourceConfig?.refreshTimeString ?? "05:00:00",
            refreshTime = sourceConfig?.refreshTime ?? new TimeSpan(5, 0, 0),
            openStartTimeString = "06:00:00",
            openEndTimeString = "09:00:00",
            openStartTime = new TimeSpan(6, 0, 0),
            openEndTime = new TimeSpan(9, 0, 0),
            description = sourceConfig?.description ?? "前往今日随机校园地标，完成晨光打卡。",
            rewards = BuildDefaultRewards(),
            locations = todayLocations
        };
    }

    private static ClockInLocation CloneLocation(ClockInLocation location)
    {
        return new ClockInLocation
        {
            locationId = location.locationId,
            name = location.name,
            worldPosition = location.worldPosition
        };
    }

    private static bool TryResolveLocationPosition(ClockInLocation location, out Vector3 position)
    {
        position = Vector3.zero;
        if (location == null || string.IsNullOrEmpty(location.locationId))
            return false;

        // The most reliable approach is to use explicit landmark anchors placed at
        // the real world positions in the scene. We intentionally do not fall back
        // to fuzzy object-name matching or JSON coordinates, because both can drift
        // and cause incorrect distance prompts.
        ClockInLandmarkAnchor[] anchors = UnityEngine.Object.FindObjectsOfType<ClockInLandmarkAnchor>(true);
        ClockInLandmarkAnchor matchedAnchor = anchors.FirstOrDefault(anchor => anchor != null && anchor.Matches(location.locationId));
        if (matchedAnchor != null)
        {
            position = matchedAnchor.transform.position;
            resolvedPositions[location.locationId] = position;
            return true;
        }

        return resolvedPositions.TryGetValue(location.locationId, out position);
    }

    private static void InitializeTodayState()
    {
        if (currentConfig == null || currentConfig.locations == null || currentConfig.locations.Count == 0)
            return;

        clockInEventState.checkedIn ??= new Dictionary<string, bool>();
        clockInEventState.historyRecords ??= new List<string>();

        if (clockInEventState.lastCheckInDate.Date != DateTime.Today)
        {
            clockInEventState.lastCheckInDate = DateTime.Today;
            clockInEventState.checkedIn = new Dictionary<string, bool>();
            LocalProfileSaveSystem.SaveCurrentAccount();
        }

        foreach (ClockInLocation location in currentConfig.locations)
        {
            if (!clockInEventState.checkedIn.ContainsKey(location.locationId))
                clockInEventState.checkedIn[location.locationId] = false;
        }
    }

    public static void CheckAndRefresh()
    {
        int activeSceneHandle = SceneManager.GetActiveScene().handle;
        if (activeSceneHandle != lastResolvedSceneHandle)
        {
            resolvedPositions.Clear();
            lastResolvedSceneHandle = activeSceneHandle;
        }

        SelectTodayLocations();
        InitializeTodayState();
    }

    public static ClockInEventConfig GetCurrentConfig()
    {
        CheckAndRefresh();
        return currentConfig;
    }

    public static ClockInLocation GetTodayLocation()
    {
        CheckAndRefresh();
        return todayLocation;
    }

    public static List<ClockInLocation> GetTodayLocations()
    {
        CheckAndRefresh();
        return currentConfig?.locations ?? new List<ClockInLocation>();
    }

    public static float GetCheckRadius()
    {
        return DefaultCheckRadius;
    }

    public static string GetOpenWindowLabel()
    {
        return "06:00 - 09:00";
    }

    public static List<RewardItem> GetTodayRewards()
    {
        CheckAndRefresh();
        return currentConfig?.rewards ?? new List<RewardItem>();
    }

    public static bool IsChecked(string locationId)
    {
        CheckAndRefresh();
        return clockInEventState.checkedIn.ContainsKey(locationId) && clockInEventState.checkedIn[locationId];
    }

    public static bool IsWithinOpenTime(out string message)
    {
        CheckAndRefresh();

        if (!EnableTimeRestriction)
        {
            message = string.Empty;
            return true;
        }

        TimeSpan now = DateTime.Now.TimeOfDay;
        TimeSpan start = new TimeSpan(6, 0, 0);
        TimeSpan end = new TimeSpan(9, 0, 0);
        if (now < start || now > end)
        {
            message = $"当前不在开放时段内，请在 {GetOpenWindowLabel()} 之间完成打卡。";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public static bool IsPlayerInRange(Vector3 playerPosition, string locationId, out float distance)
    {
        CheckAndRefresh();
        distance = float.MaxValue;

        ClockInLocation location = currentConfig?.locations?.FirstOrDefault(item => item.locationId == locationId);
        if (location == null)
            return false;

        Vector3 flatPlayer = new Vector3(playerPosition.x, 0f, playerPosition.z);
        Vector3 flatTarget = new Vector3(location.worldPosition.x, 0f, location.worldPosition.z);
        distance = Vector3.Distance(flatPlayer, flatTarget);
        return distance <= DefaultCheckRadius;
    }

    public static ClockInAttemptResult TryCheckIn(Vector3 playerPosition, string locationId)
    {
        CheckAndRefresh();

        ClockInLocation targetLocation = currentConfig?.locations?.FirstOrDefault(item => item.locationId == locationId);
        if (targetLocation == null || string.IsNullOrEmpty(locationId))
            return new ClockInAttemptResult(false, false, "今日打卡点不存在或未能定位到真实场景地点。");

        if (IsChecked(locationId))
            return new ClockInAttemptResult(false, true, $"{targetLocation.name} 今天已经打卡过了。");

        bool inRange = IsPlayerInRange(playerPosition, locationId, out float distance);
        if (!inRange)
            return new ClockInAttemptResult(false, false, $"不在打卡范围内，请靠近 {targetLocation.name} 后再尝试。当前距离约 {distance:F1} 米。");

        if (!IsWithinOpenTime(out string timeMessage))
            return new ClockInAttemptResult(false, false, $"{timeMessage}\n当前距离目标点约 {distance:F1} 米。");

        clockInEventState.checkedIn[locationId] = true;
        clockInEventState.totalCheckInCount++;
        clockInEventState.lastCheckedLocationId = locationId;
        clockInEventState.historyRecords ??= new List<string>();
        clockInEventState.historyRecords.Add($"{DateTime.Today:yyyy-MM-dd} | {targetLocation.name}");

        ActivityRewardSystem.GrantClockInReward(GetTodayRewards());
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();

        string successMessage =
            $"今日打卡点：{targetLocation.name}\n已完成打卡。\n累计打卡：{clockInEventState.totalCheckInCount} 天\n获得奖励：纪念币 x10";
        return new ClockInAttemptResult(true, false, successMessage, GetTodayRewards());
    }

    public static int GetCheckedCount()
    {
        CheckAndRefresh();
        return clockInEventState.checkedIn.Values.Count(item => item);
    }

    public static int GetLocationCount()
    {
        return GetTodayLocations().Count;
    }
}

public readonly struct ClockInAttemptResult
{
    public readonly bool success;
    public readonly bool alreadyChecked;
    public readonly string message;
    public readonly List<RewardItem> rewards;

    public ClockInAttemptResult(bool success, bool alreadyChecked, string message, List<RewardItem> rewards = null)
    {
        this.success = success;
        this.alreadyChecked = alreadyChecked;
        this.message = message;
        this.rewards = rewards;
    }
}
