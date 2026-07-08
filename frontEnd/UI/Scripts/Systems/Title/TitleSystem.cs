using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TitleSystem
{
    public static PlayerTitleState titleState => AccountSystem.GetCurrentRole()?.titleState;

    private const int DefaultTitleId = 1;

    private static List<TitleData> titles = new();

    static TitleSystem()
    {
        if (!BackendSettings.UseBackendMode)
            LoadTitleConfig();
    }

    public static void LoadTitleConfig()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            return;

        TextAsset json = Resources.Load<TextAsset>("Jsons/TitleConfig");
        if (json == null)
        {
            ApplyTitleConfig(Array.Empty<TitleData>());
            return;
        }

        TitleConfigListWrapper wrapper = JsonUtility.FromJson<TitleConfigListWrapper>(json.text);
        ApplyTitleConfig(wrapper?.titles ?? new List<TitleData>());
    }

    public static void SetTitleConfig(IEnumerable<TitleData> configTitles)
    {
        if (configTitles == null)
            return;

        ApplyTitleConfig(configTitles);
    }

    private static void ApplyTitleConfig(IEnumerable<TitleData> configTitles)
    {
        titles = configTitles
            .Where(title => title != null)
            .Select(CloneTitleData)
            .OrderBy(title => title.titleID)
            .ToList();

        foreach (TitleData title in titles)
        {
            if (!Enum.TryParse(title.typeString, out TitleType parsed))
                parsed = TitleType.None;

            title.type = parsed;
        }

        RefreshUnlockedTitles();
    }

    public static List<TitleData> GetTitleDatas()
    {
        if ((!BackendSettings.UseBackendMode || !SessionStore.HasToken) && (titles == null || titles.Count == 0))
            LoadTitleConfig();

        RefreshUnlockedTitles();
        return titles;
    }

    public static void RefreshUnlockedTitles()
    {
        PlayerTitleState state = titleState;
        if (state == null)
            return;

        state.unlockedTitles ??= new HashSet<int>();
        state.extraUnlockedTitles ??= new HashSet<int>();
        state.talkedNpcIds ??= new HashSet<string>();
        state.npcConversationCounts ??= new Dictionary<string, int>();
        state.savedPhotoIds ??= new HashSet<string>();

        HashSet<int> unlocked = BackendSettings.UseBackendMode && SessionStore.HasToken
            ? new HashSet<int>(state.backendUnlockedTitles ?? new HashSet<int>())
            : new HashSet<int>(state.unlockedTitles);
        unlocked.UnionWith(state.extraUnlockedTitles);
        unlocked.Add(DefaultTitleId);

        ApplyLevelTitleUnlockRules(unlocked);

        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken)
        {
            SyncGrowthTitles(unlocked);
            SyncTaskTitles(unlocked);
        }

        state.unlockedTitles = unlocked;

        if (!state.unlockedTitles.Contains(state.equippedTitleID))
            state.equippedTitleID = state.unlockedTitles.Contains(DefaultTitleId) ? DefaultTitleId : state.unlockedTitles.FirstOrDefault();
    }

    public static bool SetCurrentTitle(int titleId)
    {
        PlayerTitleState state = titleState;
        if (state == null)
            return false;

        RefreshUnlockedTitles();
        if (!IsTitleUnlocked(titleId))
            return false;

        state.equippedTitleID = titleId;
        LocalProfileSaveSystem.SaveCurrentAccount();
        return true;
    }

    public static TitleData GetCurrentTitle()
    {
        RefreshUnlockedTitles();
        if (titles == null || titles.Count == 0)
            return BuildFallbackDefaultTitle();

        int equippedTitleId = titleState?.equippedTitleID ?? DefaultTitleId;
        TitleData current = titles.Find(i => i.titleID == equippedTitleId);
        return current ?? titles.Find(i => i.titleID == DefaultTitleId) ?? titles.FirstOrDefault() ?? BuildFallbackDefaultTitle();
    }

    public static void UnLockTitle(int titleID)
    {
        PlayerTitleState state = titleState;
        if (state == null)
            return;

        state.extraUnlockedTitles ??= new HashSet<int>();
        state.unlockedTitles ??= new HashSet<int>();
        state.extraUnlockedTitles.Add(titleID);
        state.unlockedTitles.Add(titleID);
    }

    public static bool IsTitleUnlocked(int titleID)
    {
        PlayerTitleState state = titleState;
        if (state == null)
            return false;

        RefreshUnlockedTitles();
        return state.unlockedTitles.Contains(titleID);
    }

    public static bool IsTitleEquipped(int titleID)
    {
        PlayerTitleState state = titleState;
        return GetCurrentTitle() != null && state != null && state.equippedTitleID == titleID;
    }

    public static bool IsBackendUnlockedTitle(int titleId)
    {
        PlayerTitleState state = titleState;
        return state != null && state.backendUnlockedTitles != null && state.backendUnlockedTitles.Contains(titleId);
    }

    private static void ApplyLevelTitleUnlockRules(HashSet<int> unlocked)
    {
        foreach (LevelConfig config in LevelSystem.GetAllLevelConfigs())
        {
            if (config.titleID <= 0)
                continue;

            unlocked.Remove(config.titleID);

            if (LevelSystem.currentLevel >= config.level && LevelSystem.IsBoxOpened(config.level))
                unlocked.Add(config.titleID);
        }
    }

    private static void SyncGrowthTitles(HashSet<int> unlocked)
    {
        foreach (GrowthStageData stage in GrowthSystem.GetStageDatas())
        {
            if (stage.titleID > 0 && GrowthSystem.IsRewardClaimed(stage.stageID))
                unlocked.Add(stage.titleID);
        }
    }

    private static void SyncTaskTitles(HashSet<int> unlocked)
    {
        if (IsGoalCompleted("CH01_G01"))
            unlocked.Add(2);

        if (IsGoalCompleted("CH01_G03"))
            unlocked.Add(3);

        if (IsGoalCompleted("CH01_G04"))
            unlocked.Add(4);

        if (IsGoalCompleted("CH02_G03"))
            unlocked.Add(5);

        if (IsGoalCompleted("CH03_G04"))
            unlocked.Add(6);

        if (IsChapterCompleted("CH01"))
            unlocked.Add(7);

        if (IsChapterCompleted("CH02"))
            unlocked.Add(8);

        if (IsChapterCompleted("CH03"))
            unlocked.Add(9);
    }

    private static bool IsChapterCompleted(string chapterId)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        return role?.taskState?.completedTasks != null && role.taskState.completedTasks.Contains(chapterId);
    }

    private static bool IsGoalCompleted(string goalId)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        return role?.taskState?.completedGoals != null && role.taskState.completedGoals.Contains(goalId);
    }

    private static TitleData CloneTitleData(TitleData source)
    {
        if (!Enum.TryParse(source.typeString, out TitleType parsed))
            parsed = TitleType.None;

        return new TitleData
        {
            titleID = source.titleID,
            typeString = source.typeString,
            type = parsed,
            titleName = source.titleName
        };
    }

    private static TitleData BuildFallbackDefaultTitle()
    {
        return new TitleData
        {
            titleID = DefaultTitleId,
            typeString = TitleType.None.ToString(),
            type = TitleType.None,
            titleName = "Default Title"
        };
    }

    [Serializable]
    public class TitleConfigListWrapper
    {
        public List<TitleData> titles;
    }
}
