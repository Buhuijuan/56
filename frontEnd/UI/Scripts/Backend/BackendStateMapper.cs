using System;
using System.Collections.Generic;
using System.Linq;

public static class BackendStateMapper
{
    public static void ApplyPlayerMe(BackendPlayerMeData me)
    {
        if (me == null)
            return;

        Dictionary<string, RoleData> previousRoles = new();
        List<RoleData> existingRoles = AccountSystem.GetRoles();
        if (existingRoles != null)
        {
            foreach (RoleData role in existingRoles)
            {
                if (role != null && !string.IsNullOrWhiteSpace(role.roleID))
                    previousRoles[role.roleID] = role;
            }
        }

        List<RoleData> mappedRoles = me.roles != null ? me.roles.Select(ToRoleData).ToList() : new List<RoleData>();
        foreach (RoleData mapped in mappedRoles)
        {
            if (mapped == null || string.IsNullOrWhiteSpace(mapped.roleID))
                continue;

            if (!previousRoles.TryGetValue(mapped.roleID, out RoleData previous))
                continue;

            if (string.IsNullOrWhiteSpace(mapped.avatarUrl))
                mapped.avatarUrl = previous.avatarUrl;

            mapped.useCustomAvatar = previous.useCustomAvatar;
            mapped.customAvatarPath = previous.customAvatarPath;
        }

        AccountData account = new()
        {
            accountID = me.accountCode,
            mailbox = me.mailbox,
            password = string.Empty,
            roles = mappedRoles
        };

        AccountSystem.InitFromBackend(account, account.roles, me.currentRoleId);
    }

    public static void ApplyGameHome(BackendGameHomeData home)
    {
        if (home == null)
            return;

        RoleData previousRole = AccountSystem.GetCurrentRole();
        RoleData currentRole = ToRoleData(home.role);
        if (previousRole != null)
        {
            if (string.IsNullOrWhiteSpace(currentRole.avatarUrl))
                currentRole.avatarUrl = previousRole.avatarUrl;
            currentRole.useCustomAvatar = previousRole.useCustomAvatar;
            currentRole.customAvatarPath = previousRole.customAvatarPath;
            currentRole.rewardState = previousRole.rewardState ?? new PlayerRewardState();
        }
        currentRole.levelState = ToLevelState(home.level);
        currentRole.titleState = ToTitleState(home.title);
        currentRole.taskState = ToTaskState(home.task);
        currentRole.signInState = home.signIn != null ? ToFrontSignIn(home.signIn) : new PlayerSignInState();
        currentRole.rewardState ??= new PlayerRewardState();

        if (home.profile != null)
        {
            currentRole.nickName = string.IsNullOrWhiteSpace(home.profile.nickname) ? currentRole.nickName : home.profile.nickname;
            currentRole.rewardState.ApplyBackendMemorialCoin(home.profile.coin);
        }

        if (home.activities != null)
        {
            if (home.activities.quiz != null && home.activities.quiz.state != null)
                currentRole.quizEventState = ToQuizState(home.activities.quiz.state);
            if (home.activities.clockIn != null && home.activities.clockIn.state != null)
                currentRole.clockInEventState = ToClockInState(home.activities.clockIn.state);
            if (home.activities.story != null && home.activities.story.state != null)
                currentRole.storyEventState = ToStoryState(home.activities.story.state);
        }

        AccountData account = AccountSystem.GetCurrentAccount() ?? new AccountData();
        if (home.account != null)
        {
            account.accountID = home.account.accountCode;
            account.mailbox = home.account.mailbox;
        }

        AccountSystem.UpdateCurrentRole(currentRole);
        HeadAreaUI.RefreshAll();
    }

    public static PlayerSignInState ToFrontSignIn(BackendSignInState state)
    {
        PlayerSignInState result = new();
        if (state == null)
            return result;
        result.todayOnlineSeconds = state.todayOnlineSeconds;
        result.todayOnlineMinutes = state.todayOnlineSeconds / 60;
        result.dailySigned = state.dailySigned;
        result.lastSignInDate = ParseDateTime(state.lastSignInDate);
        result.continuousSignDays = state.continuousSignDays;
        result.currentWeekIndex = state.currentWeekIndex;
        result.totalLoginDays = state.totalLoginDays;
        result.onlineRewardClaimed = ToHashSet(state.onlineRewardClaimed);
        result.dailyRewardClaimed = ToHashSet(state.dailyRewardClaimed);
        result.totalRewardClaimed = ToHashSet(state.totalRewardClaimed);
        return result;
    }

    public static void ApplyGrowth(BackendGrowthPageData data)
    {
        if (data == null)
            return;

        if (data.titleConfig != null && data.titleConfig.Count > 0)
            TitleSystem.SetTitleConfig(data.titleConfig);
        if (data.growthConfig != null && data.growthConfig.Count > 0)
            GrowthSystem.SetGrowthConfig(data.growthConfig);

        RoleData currentRole = AccountSystem.GetCurrentRole();
        if (currentRole == null)
            return;

        currentRole.levelState = ToLevelState(data.levelState);
        currentRole.titleState = ToTitleState(data.titleState);
        currentRole.growthState = ToGrowthState(data.growthState);
        if (data.profile != null)
        {
            currentRole.nickName = string.IsNullOrWhiteSpace(data.profile.nickname) ? currentRole.nickName : data.profile.nickname;
            currentRole.rewardState ??= new PlayerRewardState();
            currentRole.rewardState.ApplyBackendMemorialCoin(data.profile.coin);
        }

        HeadAreaUI.RefreshAll();
    }

    private static RoleData ToRoleData(BackendRoleData role)
    {
        if (role == null)
            return null;
        RoleData result = new()
        {
            roleID = role.id.ToString(),
            campusName = role.campusName,
            nickName = role.nickName,
            avatarUrl = role.avatarUrl,
            characterState = new PlayerCharacterState
            {
                currentCharacterID = role.currentCharacterId,
                unlockedCharacters = ToHashSet(role.unlockedCharacterIds)
            }
        };
        return result;
    }

    private static PlayerLevelState ToLevelState(BackendLevelState state)
    {
        PlayerLevelState existing = AccountSystem.GetCurrentRole()?.levelState;
        PlayerLevelState result = existing != null
            ? new PlayerLevelState
            {
                level = existing.level,
                progress = existing.progress,
                exp = existing.exp,
                rewardClaimed = existing.rewardClaimed != null ? new HashSet<int>(existing.rewardClaimed) : new HashSet<int>(),
                boxOpened = existing.boxOpened != null ? new HashSet<int>(existing.boxOpened) : new HashSet<int>()
            }
            : new PlayerLevelState();

        if (state == null)
            return result;

        result.level = state.level;
        result.exp = state.exp;

        HashSet<int> backendRewardClaimed = ToHashSet(state.rewardClaimed);
        HashSet<int> backendBoxOpened = ToHashSet(state.boxOpened);

        result.rewardClaimed = backendRewardClaimed;
        result.boxOpened = backendBoxOpened;

        int currentExp = GetRequiredExp(state.level);
        int nextExp = GetRequiredExp(state.level + 1);
        if (nextExp <= currentExp)
            result.progress = 1f;
        else
            result.progress = UnityEngine.Mathf.Clamp01((state.exp - currentExp) / (float)(nextExp - currentExp));
        return result;
    }

    private static PlayerTitleState ToTitleState(BackendTitleState state)
    {
        PlayerTitleState existing = AccountSystem.GetCurrentRole()?.titleState;
        PlayerTitleState result = existing != null
            ? new PlayerTitleState
            {
                unlockedTitles = existing.unlockedTitles != null ? new HashSet<int>(existing.unlockedTitles) : new HashSet<int>(),
                backendUnlockedTitles = existing.backendUnlockedTitles != null ? new HashSet<int>(existing.backendUnlockedTitles) : new HashSet<int>(),
                extraUnlockedTitles = existing.extraUnlockedTitles != null ? new HashSet<int>(existing.extraUnlockedTitles) : new HashSet<int>(),
                equippedTitleID = existing.equippedTitleID,
                hasCreatedCharacter = existing.hasCreatedCharacter,
                hasEnteredCampusOnce = existing.hasEnteredCampusOnce,
                talkedNpcIds = existing.talkedNpcIds != null ? new HashSet<string>(existing.talkedNpcIds) : new HashSet<string>(),
                npcConversationCounts = existing.npcConversationCounts != null ? new Dictionary<string, int>(existing.npcConversationCounts) : new Dictionary<string, int>(),
                petInteractionCount = existing.petInteractionCount,
                elfAnswerCount = existing.elfAnswerCount,
                savedPhotoIds = existing.savedPhotoIds != null ? new HashSet<string>(existing.savedPhotoIds) : new HashSet<string>(),
                bikeRideCount = existing.bikeRideCount
            }
            : new PlayerTitleState();

        if (state == null)
            return result;

        HashSet<int> backendUnlockedTitles = ToHashSet(state.unlockedTitleIds);
        int previousEquippedTitleId = existing != null ? existing.equippedTitleID : 0;

        result.backendUnlockedTitles = backendUnlockedTitles;
        result.unlockedTitles = new HashSet<int>(backendUnlockedTitles);
        result.equippedTitleID = state.equippedTitleId;

        if (previousEquippedTitleId > 0 && !backendUnlockedTitles.Contains(previousEquippedTitleId))
            result.equippedTitleID = previousEquippedTitleId;

        return result;
    }

    private static PlayerTaskState ToTaskState(BackendTaskState state)
    {
        PlayerTaskState result = new();
        if (state == null)
            return result;
        result.completedTasks = ToHashSet(state.completedTasks);
        result.completedGoals = ToHashSet(state.completedGoals);
        result.claimedTasks = ToHashSet(state.claimedTasks);
        return result;
    }

    private static PlayerGrowthState ToGrowthState(BackendGrowthState state)
    {
        PlayerGrowthState result = new();
        if (state == null)
            return result;
        result.stageCompleted = ToHashSet(state.stageCompleted);
        result.rewardClaimed = ToHashSet(state.rewardClaimed);
        result.taskCompleted = ToHashSet(state.taskCompleted);
        return result;
    }

    private static PlayerQuizEventState ToQuizState(BackendQuizState state)
    {
        PlayerQuizEventState existing = AccountSystem.GetCurrentRole()?.quizEventState;
        PlayerQuizEventState result = existing != null
            ? new PlayerQuizEventState
            {
                eventId = existing.eventId,
                weeklyScore = existing.weeklyScore,
                totalCorrectAnswers = existing.totalCorrectAnswers,
                totalSessions = existing.totalSessions,
                lastPlayDate = existing.lastPlayDate,
                hasPlayedToday = existing.hasPlayedToday,
                rewardClaimedEventId = existing.rewardClaimedEventId,
                claimedWeeklyCoinAmount = existing.claimedWeeklyCoinAmount,
                answerHistory = existing.answerHistory != null ? new List<string>(existing.answerHistory) : null
            }
            : new PlayerQuizEventState();

        result.eventId = state.eventId;
        result.weeklyScore = state.weeklyScore;
        result.lastPlayDate = ParseDateTime(state.lastPlayDate);
        result.hasPlayedToday = state.hasPlayedToday;
        return result;
    }

    private static PlayerClockInEventState ToClockInState(BackendClockInState state)
    {
        PlayerClockInEventState existing = AccountSystem.GetCurrentRole()?.clockInEventState;
        PlayerClockInEventState result = existing != null
            ? new PlayerClockInEventState
            {
                lastCheckInDate = existing.lastCheckInDate,
                totalCheckInCount = existing.totalCheckInCount,
                checkedIn = existing.checkedIn != null ? new Dictionary<string, bool>(existing.checkedIn) : null,
                lastCheckedLocationId = existing.lastCheckedLocationId,
                historyRecords = existing.historyRecords != null ? new List<string>(existing.historyRecords) : null
            }
            : new PlayerClockInEventState();

        result.lastCheckInDate = ParseDateTime(state.lastCheckInDate);
        result.checkedIn = new Dictionary<string, bool>();
        if (state.checkedLocationIds != null)
        {
            foreach (string checkedLocationId in state.checkedLocationIds)
            {
                if (!string.IsNullOrWhiteSpace(checkedLocationId))
                    result.checkedIn[checkedLocationId] = true;
            }
        }
        return result;
    }

    private static PlayerStoryEventState ToStoryState(BackendStoryState state)
    {
        PlayerStoryEventState result = new();
        result.eventId = state.eventId;
        result.hasFinished = state.hasFinished;
        result.rewardClaimed = state.rewardClaimed;
        result.lastPlayDate = ParseDateTime(state.lastPlayDate);
        return result;
    }

    private static HashSet<int> ToHashSet(List<int> source)
    {
        return source != null ? new HashSet<int>(source) : new HashSet<int>();
    }

    private static HashSet<string> ToHashSet(List<string> source)
    {
        return source != null ? new HashSet<string>(source) : new HashSet<string>();
    }

    private static DateTime ParseDateTime(string value)
    {
        if (string.IsNullOrEmpty(value))
            return DateTime.MinValue;
        if (DateTime.TryParse(value, out DateTime parsed))
            return parsed;
        return DateTime.MinValue;
    }

    private static int GetRequiredExp(int level)
    {
        LevelConfig config = LevelSystem.GetLevelConfig(level);
        return config != null ? config.requiredExp : 0;
    }
}
