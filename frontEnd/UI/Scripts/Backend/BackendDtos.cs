using System;
using System.Collections.Generic;

[Serializable]
public class BackendApiErrorEnvelope
{
    public bool success;
    public string message;
}

[Serializable]
public class BackendApiResult<T>
{
    public bool Success;
    public int StatusCode;
    public string Message;
    public T Data;
}

[Serializable]
public class BackendAuthData
{
    public long accountId;
    public string accountCode;
    public string mailbox;
    public long currentRoleId;
    public string token;
}

[Serializable]
public class BackendAuthEnvelope
{
    public bool success;
    public BackendAuthData data;
    public string message;
}

[Serializable]
public class BackendSendCodeData
{
    public string mailbox;
    public int expiresInSeconds;
    public bool sent;
}

[Serializable]
public class BackendSendCodeEnvelope
{
    public bool success;
    public BackendSendCodeData data;
    public string message;
}

[Serializable]
public class BackendRoleData
{
    public long id;
    public long accountId;
    public string roleCode;
    public long schoolId;
    public string campusName;
    public int slotNo;
    public string nickName;
    public string avatarUrl;
    public int currentCharacterId;
    public List<int> unlockedCharacterIds;
}

[Serializable]
public class BackendPlayerMeData
{
    public long accountId;
    public string accountCode;
    public string mailbox;
    public long currentRoleId;
    public List<BackendRoleData> roles;
}

[Serializable]
public class BackendPlayerMeEnvelope
{
    public bool success;
    public BackendPlayerMeData data;
    public string message;
}

[Serializable]
public class BackendRoleEnvelope
{
    public bool success;
    public BackendRoleData data;
    public string message;
}

[Serializable]
public class BackendAccountSummary
{
    public long id;
    public string accountCode;
    public string mailbox;
}

[Serializable]
public class BackendLevelState
{
    public long roleId;
    public int level;
    public int exp;
    public List<int> rewardClaimed;
    public List<int> boxOpened;
}

[Serializable]
public class BackendTitleState
{
    public long roleId;
    public List<int> unlockedTitleIds;
    public int equippedTitleId;
}

[Serializable]
public class BackendTaskState
{
    public long roleId;
    public List<string> completedTasks;
    public List<string> completedGoals;
    public List<string> claimedTasks;
}

[Serializable]
public class BackendTaskEntry
{
    public long taskId;
    public string taskCode;
    public string taskName;
    public string taskType;
    public int chapterNo;
    public int stepNo;
    public string category;
    public string triggerType;
    public string targetType;
    public long targetId;
    public int targetCount;
    public int rewardExp;
    public int rewardCoin;
    public string rewardUnlockFeature;
    public string description;
    public string status;
    public int progressCurrent;
    public int progressTarget;
    public string acceptedAt;
    public string completedAt;
    public string rewardClaimedAt;
    public BackendElfPrompt elfPrompt;
}
[Serializable]
public class BackendElfPrompt
{
    public string npcName;
    public string avatarKey;
    public string stage;
    public string title;
    public List<string> contents;
    public bool autoPopup;
    public string taskCode;
}

[Serializable]
public class BackendTaskGoalEntry
{
    public string goalId;
    public string description;
    public bool completed;
}

[Serializable]
public class BackendTaskChapterEntry
{
    public string typeString;
    public string taskChapterID;
    public string taskChapterTitle;
    public string taskChapterDescription;
    public List<BackendTaskGoalEntry> goals;
}
[Serializable]
public class BackendTaskChaptersEnvelope
{
    public bool success;
    public BackendTaskChaptersData data;
    public string message;
}

[Serializable]
public class BackendTaskChaptersData
{
    public List<BackendTaskChapterEntry> chapters;
}

[Serializable]
public class BackendTaskChapterSummary
{
    public int chapterNo;
    public int taskCount;
    public int completedCount;
    public int claimedCount;
    public bool chapterCompleted;
}

[Serializable]
public class BackendTaskPageData
{
    public List<BackendTaskEntry> acceptedTasks;
    public List<BackendTaskEntry> availableTasks;
    public List<BackendTaskEntry> completedTasks;
    public List<BackendTaskEntry> claimedTasks;
    public BackendTaskEntry currentMainTask;
    public List<BackendTaskChapterSummary> chapters;
    public BackendElfPrompt elfPrompt;
}

[Serializable]
public class BackendTaskPageEnvelope
{
    public bool success;
    public BackendTaskPageData data;
    public string message;
}

[Serializable]
public class BackendTaskCurrentMainData
{
    public BackendTaskEntry task;
    public BackendElfPrompt elfPrompt;
}

[Serializable]
public class BackendTaskCurrentMainEnvelope
{
    public bool success;
    public BackendTaskCurrentMainData data;
    public string message;
}

[Serializable]
public class BackendTaskProgressResult
{
    public string taskCode;
    public int progressBefore;
    public int progressAfter;
    public bool completed;
}

[Serializable]
public class BackendTaskEventData
{
    public string eventType;
    public List<string> triggeredTasks;
    public List<BackendTaskProgressResult> progressedTasks;
    public BackendTaskEntry currentMainTask;
    public BackendElfPrompt elfPrompt;
    public BackendElfPrompt completedElfPrompt;
}

[Serializable]
public class BackendTaskEventEnvelope
{
    public bool success;
    public BackendTaskEventData data;
    public string message;

}

[Serializable]
public class BackendTaskOperationEnvelope
{
    public bool success;
    public BackendTaskOperationData data;
    public string message;
}

[Serializable]
public class BackendTaskOperationData
{
    public string taskCode;
    public bool accepted;
    public bool claimed;
    public BackendElfPrompt elfPrompt;
}

[Serializable]
public class BackendQuizState
{
    public long roleId;
    public string eventId;
    public int weeklyScore;
    public string lastPlayDate;
    public bool hasPlayedToday;
    public bool canClaimWeeklyReward;
    public int claimableCoin;
    public bool weeklyRewardClaimed;
}

[Serializable]
public class BackendQuizCurrentPageData
{
    public BackendQuizEventConfigDto config;
    public BackendQuizState state;
}

[Serializable]
public class BackendQuizCurrentEnvelope
{
    public bool success;
    public BackendQuizCurrentPageData data;
    public string message;
}

[Serializable]
public class BackendQuizQuestionData
{
    public string questionId;
    public string questionText;
    public List<string> options;
}

[Serializable]
public class BackendQuizStartData
{
    public BackendQuizEventConfigDto @event;
    public List<BackendQuizQuestionDetail> questions;
}

[Serializable]
public class BackendQuizEventConfigDto
{
    public string eventId;
    public string theme;
    public string startTimeString;
    public int totalQuestions;
    public int durationDays;
    public string questionsFile;
    public List<RewardItem> finalRewards;

    // Compatibility aliases for possible backend field names.
    public string id;
    public string quizId;
    public string activityId;
    public string title;
    public string topic;
    public string startTime;
    public string startAt;
    public string beginTime;
    public string beginAt;
    public string openTime;
    public string endTime;
    public string endAt;
    public int questionCount;
    public int questionsCount;
    public int quizCount;
    public int duration;
    public int durationDay;
    public int days;
    public string questionFile;
    public string questionBankFile;
    public string questionBank;
    public List<RewardItem> rewards;
    public List<RewardItem> rewardList;
    public List<RewardItem> awards;
}
[Serializable]
public class BackendQuizQuestionDetail
{
    public string questionId;
    public string questionText;
    public List<string> options;
    public int correctIndex;
    public string explanation;
}
[Serializable]
public class BackendQuizStartEnvelope
{
    public bool success;
    public BackendQuizStartData data;
    public string message;
}

[Serializable]
public class BackendQuizSubmitData
{
    public int score;
    public int weeklyScore;
    public int addedCoin;
    public List<BackendQuizSubmitResult> results;
}
[Serializable]
public class BackendQuizSubmitResult
{
    public string questionId;
    public int selectedIndex;
    public int correctIndex;
    public bool correct;
    public string explanation;
}

[Serializable]
public class BackendQuizSubmitEnvelope
{
    public bool success;
    public BackendQuizSubmitData data;
    public string message;
}

[Serializable]
public class BackendQuizWeeklyClaimData
{
    public int addedCoin;
    public int weeklyScore;
    public bool claimed;
    public int claimTier;
}

[Serializable]
public class BackendQuizWeeklyClaimEnvelope
{
    public bool success;
    public BackendQuizWeeklyClaimData data;
    public string message;
}

[Serializable]
public class BackendClockInState
{
    public long roleId;
    public string eventId;
    public string lastCheckInDate;
    public List<string> checkedLocationIds;
}

[Serializable]
public class BackendStoryState
{
    public long roleId;
    public string eventId;
    public bool hasFinished;
    public bool rewardClaimed;
    public string lastPlayDate;
}

[Serializable]
public class BackendActivityHomeEntry<TConfig, TState>
{
    public TConfig config;
    public TState state;
}

[Serializable]
public class BackendActivityHomeData
{
    public BackendActivityHomeEntry<QuizEventConfig, BackendQuizState> quiz;
    public BackendActivityHomeEntry<ClockInEventConfig, BackendClockInState> clockIn;
    public BackendActivityHomeEntry<StoryEventConfig, BackendStoryState> story;
}

[Serializable]
public class BackendGameHomeData
{
    public BackendAccountSummary account;
    public BackendRoleData role;
    public BackendPlayerProfile profile;
    public BackendLevelState level;
    public BackendTitleState title;
    public BackendSignInState signIn;
    public BackendTaskState task;
    public BackendActivityHomeData activities;
}

[Serializable]
public class BackendGameHomeEnvelope
{
    public bool success;
    public BackendGameHomeData data;
    public string message;
}

[Serializable]
public class BackendSignInState
{
    public long roleId;
    public int todayOnlineSeconds;
    public bool dailySigned;
    public string lastSignInDate;
    public int continuousSignDays;
    public int currentWeekIndex;
    public int totalLoginDays;
    public List<int> onlineRewardClaimed;
    public List<int> dailyRewardClaimed;
    public List<int> totalRewardClaimed;
}

[Serializable]
public class BackendSignInPageData
{
    public BackendSignInState state;
    public List<DailyAwardConfig> dailyAwards;
    public List<OnlineAwardConfig> onlineAwards;
    public List<TotalAwardConfig> totalAwards;
}

[Serializable]
public class BackendSignInEnvelope
{
    public bool success;
    public BackendSignInPageData data;
    public string message;
}

[Serializable]
public class BackendClockInPageData
{
    public ClockInEventConfig config;
    public BackendClockInState state;
}

[Serializable]
public class BackendClockInEnvelope
{
    public bool success;
    public BackendClockInPageData data;
    public string message;
}

[Serializable]
public class BackendClockInCheckData
{
    public string locationId;
    public long checkedCount;
    public int total;
    public int addedCoin;
    public double distanceToTarget;
}

[Serializable]
public class BackendClockInCheckEnvelope
{
    public bool success;
    public BackendClockInCheckData data;
    public string message;
}

[Serializable]
public class BackendGrowthState
{
    public long roleId;
    public List<string> stageCompleted;
    public List<string> rewardClaimed;
    public List<string> taskCompleted;
}

[Serializable]
public class BackendGrowthPageData
{
    public BackendLevelState levelState;
    public BackendGrowthState growthState;
    public BackendTitleState titleState;
    public BackendPlayerProfile profile;
    public List<LevelConfig> levelConfig;
    public List<GrowthStageData> growthConfig;
    public List<TitleData> titleConfig;
}

[Serializable]
public class BackendPlayerProfile
{
    public long roleId;
    public string nickname;
    public int level;
    public int exp;
    public int coin;
    public long currentTitleId;
    public int bikeUnlocked;
}

[Serializable]
public class BackendGrowthEnvelope
{
    public bool success;
    public BackendGrowthPageData data;
    public string message;
}

[Serializable]
public class BackendGrowthClaimData
{
    public string stageId;
    public bool claimed;
    public int addedCoin;
    public bool titleUnlocked;
    public int titleId;
}

[Serializable]
public class BackendGrowthClaimEnvelope
{
    public bool success;
    public BackendGrowthClaimData data;
    public string message;
}

[Serializable]
public class BackendSimpleStatusData
{
    public bool updated;
    public bool signed;
    public bool claimed;
    public int addedCoin;
    public int continuousSignDays;
    public int totalLoginDays;
    public int todayOnlineSeconds;
    public int todayOnlineMinutes;
}

[Serializable]
public class BackendSimpleStatusEnvelope
{
    public bool success;
    public BackendSimpleStatusData data;
    public string message;
}

[Serializable]
public class BackendEquipTitleData
{
    public int equippedTitleId;
}

[Serializable]
public class BackendEquipTitleEnvelope
{
    public bool success;
    public BackendEquipTitleData data;
    public string message;
}

[Serializable]
public class BackendLevelRewardClaimData
{
    public bool claimed;
    public int level;
    public int addedCoin;
}

[Serializable]
public class BackendLevelRewardClaimEnvelope
{
    public bool success;
    public BackendLevelRewardClaimData data;
    public string message;
}

[Serializable]
public class BackendLevelTitleClaimData
{
    public bool claimed;
    public int level;
    public int titleId;
}

[Serializable]
public class BackendLevelTitleClaimEnvelope
{
    public bool success;
    public BackendLevelTitleClaimData data;
    public string message;
}

[Serializable]
public class BackendAvatarUploadData
{
    public long roleId;
    public string avatarUrl;
}

[Serializable]
public class BackendAvatarUploadEnvelope
{
    public bool success;
    public BackendAvatarUploadData data;
    public string message;
}

[Serializable]
public class BackendSwitchRoleData
{
    public long currentRoleId;
    public string roleCode;
    public int slotNo;
}

[Serializable]
public class BackendSwitchRoleEnvelope
{
    public bool success;
    public BackendSwitchRoleData data;
    public string message;
}

[Serializable]
public class AuthLoginBody
{
    public string mailbox;
    public string password;
}

[Serializable]
public class AuthRegisterBody
{
    public string mailbox;
    public string password;
    public string verificationCode;
}

[Serializable]
public class AuthSendCodeBody
{
    public string mailbox;
}

[Serializable]
public class AuthResetPasswordBody
{
    public string mailbox;
    public string verificationCode;
    public string newPassword;
}

[Serializable]
public class PlayerCreateRoleBody
{
    public long schoolId;
    public string campusName;
    public string nickName;
    public int characterId;
}

[Serializable]
public class PlayerSwitchRoleBody
{
    public long roleId;
}

[Serializable]
public class PlayerChangePasswordBody
{
    public string newPassword;
}

[Serializable]
public class PlayerChangeMailboxBody
{
    public string mailbox;
}

[Serializable]
public class HeartbeatBody
{
    public int elapsedSeconds;
}

[Serializable]
public class TaskEventBody
{
    public string eventType;
    public string targetType;
    public long targetId;
    public int increment;
    public float currentPosX;
    public float currentPosY;
    public float currentPosZ;
    public float targetPosX;
    public float targetPosY;
    public float targetPosZ;
    public string targetAnchorKey;
}

[Serializable]
public class ClockInCheckBody
{
    public float currentPosX;
    public float currentPosY;
    public float currentPosZ;
}

[Serializable]
public class QuizSubmitBody
{
    public List<int> answers;
}
