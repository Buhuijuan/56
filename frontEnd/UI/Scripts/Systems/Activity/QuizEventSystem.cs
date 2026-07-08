using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class QuizEventSystem
{
    public static RoleData currentRole => AccountSystem.GetCurrentRole();
    public static PlayerQuizEventState quizEventState => currentRole?.quizEventState;
    public static QuizSessionState Session;

    // 鍚庣妯″紡涓撶敤锛氬瓨鍌ㄥ畬鏁寸殑棰樼洰璇︽儏
    private static List<BackendQuizQuestionDetail> _currentQuestions;
    private static QuizEventConfig _currentConfig;
    private static string _currentEventId;
    private static bool _backendCanClaimWeeklyReward;
    private static int _backendClaimableCoin;
    private static bool _backendWeeklyRewardClaimed;
    private const string AbandonLockKey = "quiz.abandon.lock";
    private static string _abandonLockRoleId;
    private static string _abandonLockEventId;
    private static DateTime _abandonLockDate = DateTime.MinValue;
    private static bool _abandonLockLoaded;

    private sealed class WeeklyRewardTier
    {
        public int minScore;
        public int maxScore;
        public int coinAmount;
    }

    private static readonly List<WeeklyRewardTier> WeeklyRewardTiers = new()
    {
        new WeeklyRewardTier { minScore = 0, maxScore = 200, coinAmount = 10 },
        new WeeklyRewardTier { minScore = 201, maxScore = 400, coinAmount = 30 },
        new WeeklyRewardTier { minScore = 401, maxScore = 550, coinAmount = 50 },
        new WeeklyRewardTier { minScore = 551, maxScore = 700, coinAmount = 100 }
    };

    private static bool UseBackendQuiz()
    {
        return BackendSettings.UseBackendMode && SessionStore.HasToken;
    }

    private static void EnsureQuizState()
    {
        if (currentRole == null)
            return;

        currentRole.quizEventState ??= new PlayerQuizEventState();
        currentRole.quizEventState.answerHistory ??= new List<string>();
    }

    private static void EnsureAbandonLockLoaded()
    {
        if (_abandonLockLoaded)
            return;

        _abandonLockLoaded = true;
        string raw = PlayerPrefs.GetString(AbandonLockKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return;

        string[] parts = raw.Split('|');
        if (parts.Length != 3)
        {
            // Legacy format without roleId can leak state across roles; drop it.
            PlayerPrefs.DeleteKey(AbandonLockKey);
            PlayerPrefs.Save();
            return;
        }

        _abandonLockRoleId = parts[0];
        _abandonLockEventId = parts[1];
        if (!DateTime.TryParse(parts[2], out _abandonLockDate))
            _abandonLockDate = DateTime.MinValue;
    }

    private static bool IsAbandonLockedToday(string eventId)
    {
        EnsureAbandonLockLoaded();
        string roleId = currentRole?.roleID;
        return !string.IsNullOrWhiteSpace(eventId) &&
               !string.IsNullOrWhiteSpace(roleId) &&
               string.Equals(_abandonLockRoleId, roleId, StringComparison.Ordinal) &&
               string.Equals(_abandonLockEventId, eventId, StringComparison.Ordinal) &&
               _abandonLockDate.Date == DateTime.Today;
    }

    private static void SaveAbandonLock(string eventId, DateTime date)
    {
        EnsureAbandonLockLoaded();
        _abandonLockRoleId = currentRole?.roleID;
        _abandonLockEventId = eventId;
        _abandonLockDate = date.Date;
        if (string.IsNullOrWhiteSpace(_abandonLockRoleId))
            return;

        PlayerPrefs.SetString(AbandonLockKey, $"{_abandonLockRoleId}|{_abandonLockEventId}|{_abandonLockDate:yyyy-MM-dd}");
        PlayerPrefs.Save();
    }

    private static void ClearAbandonLock(string eventId)
    {
        EnsureAbandonLockLoaded();
        string roleId = currentRole?.roleID;
        if (!string.IsNullOrWhiteSpace(eventId) &&
            !string.IsNullOrWhiteSpace(roleId) &&
            string.Equals(_abandonLockRoleId, roleId, StringComparison.Ordinal) &&
            string.Equals(_abandonLockEventId, eventId, StringComparison.Ordinal))
        {
            _abandonLockRoleId = null;
            _abandonLockEventId = null;
            _abandonLockDate = DateTime.MinValue;
            PlayerPrefs.DeleteKey(AbandonLockKey);
            PlayerPrefs.Save();
        }
    }

    public static void ApplyRemoteCurrent(BackendQuizEventConfigDto config, BackendQuizState state)
    {
        EnsureQuizState();
        if (quizEventState == null)
            return;

        _currentConfig = NormalizeRemoteConfig(config);
        if (_currentConfig != null)
        {
            // 浼樺厛浣跨敤 startTimeString 瑙ｆ瀽
            if (!string.IsNullOrEmpty(_currentConfig.startTimeString))
            {
                if (DateTime.TryParse(_currentConfig.startTimeString, out DateTime parsedStart))
                {
                    _currentConfig.startTime = parsedStart;
                    Debug.Log($"[QuizEventSystem] ApplyRemoteCurrent - Parsed startTime: {_currentConfig.startTime}");
                }
                else
                {
                    Debug.LogError($"[QuizEventSystem] ApplyRemoteCurrent - Failed to parse: {_currentConfig.startTimeString}");
                    _currentConfig.startTime = DateTime.Today;
                }
            }
            else
            {
                Debug.LogWarning("[QuizEventSystem] ApplyRemoteCurrent - startTimeString is null or empty, using today");
                _currentConfig.startTime = DateTime.Today;
            }

            // 纭繚 durationDays 鏈夋晥
            if (_currentConfig.durationDays <= 0)
            {
                Debug.LogWarning($"[QuizEventSystem] ApplyRemoteCurrent - Invalid durationDays: {_currentConfig.durationDays}, using 7");
                _currentConfig.durationDays = 7;
            }

            BindRewardMetadata(_currentConfig);
        }

        if (state != null)
        {
            quizEventState.eventId = state.eventId;
            quizEventState.weeklyScore = state.weeklyScore;
            quizEventState.hasPlayedToday = state.hasPlayedToday;
            quizEventState.lastPlayDate = ParseDate(state.lastPlayDate);
            _backendCanClaimWeeklyReward = state.canClaimWeeklyReward;
            _backendClaimableCoin = Mathf.Max(0, state.claimableCoin);
            _backendWeeklyRewardClaimed = state.weeklyRewardClaimed;
            _currentEventId = state.eventId;
            if (state.hasPlayedToday)
                ClearAbandonLock(state.eventId);
        }
    }

    public static void StartRemoteSession(BackendQuizEventConfigDto config, List<BackendQuizQuestionDetail> questions)
    {
        EnsureQuizState();
        _currentConfig = NormalizeRemoteConfig(config);
        _currentQuestions = questions;

        if (_currentConfig != null)
        {
            if (!DateTime.TryParse(_currentConfig.startTimeString, out _currentConfig.startTime))
                _currentConfig.startTime = DateTime.Today;
            BindRewardMetadata(_currentConfig);
        }

        // 杞崲涓烘湰鍦?Session 鏍煎紡
        List<QuizQuestion> converted = questions != null
            ? questions.Select(q => new QuizQuestion
            {
                questionId = q.questionId,
                questionText = q.questionText,
                options = q.options != null ? new List<string>(q.options) : new List<string>(),
                correctIndex = q.correctIndex,
                explanation = q.explanation ?? string.Empty
            }).ToList()
            : new List<QuizQuestion>();

        Session = new QuizSessionState
        {
            currentIndex = 0,
            questions = converted,
            userAnswers = Enumerable.Repeat(-1, converted.Count).ToList(),
            isFinished = false
        };
    }

    public static void ApplyRemoteSubmit(int score, int weeklyScore, int addedCoin, List<BackendQuizSubmitResult> results)
    {
        EnsureQuizState();
        if (quizEventState == null)
            return;

        quizEventState.weeklyScore = weeklyScore;
        quizEventState.lastPlayDate = DateTime.Today;
        quizEventState.hasPlayedToday = true;
        quizEventState.totalSessions += 1;
        ClearAbandonLock(_currentConfig != null ? _currentConfig.eventId : quizEventState.eventId);

        // 璁＄畻姝ｇ‘棰樻暟
        int correctCount = results?.Count(r => r.correct) ?? 0;
        quizEventState.totalCorrectAnswers += correctCount;

        quizEventState.answerHistory ??= new List<string>();
        quizEventState.answerHistory.Add(
            $"{DateTime.Today:yyyy-MM-dd} | 绛斿 {correctCount}/{results?.Count ?? 0} 棰?| +{score} 绉垎 | 鎬荤Н鍒?{quizEventState.weeklyScore}");

        if (Session != null)
            Session.isFinished = true;

        TitleEventReporter.ReportQuizCorrect(correctCount);
        // /quiz/submit 仅用于结算分数，不发放纪念币。
    }

    public static void ApplyRemoteWeeklyClaim(int addedCoin, int weeklyScore, bool claimed)
    {
        EnsureQuizState();
        if (quizEventState == null)
            return;

        quizEventState.weeklyScore = weeklyScore;
        _backendCanClaimWeeklyReward = false;
        _backendClaimableCoin = 0;
        _backendWeeklyRewardClaimed = claimed;

        if (addedCoin > 0 && currentRole != null)
        {
            currentRole.rewardState ??= new PlayerRewardState();
            currentRole.rewardState.AddReward(1, addedCoin);
        }
    }

    private static void BindRewardMetadata(QuizEventConfig config)
    {
        if (config?.finalRewards == null)
            return;

        foreach (RewardItem reward in config.finalRewards)
        {
            if (reward == null)
                continue;

            RewardItem baseItem = RewardItemSystem.GetRewardItem(reward.rewardId);
            if (baseItem == null)
                continue;

            reward.rewardName = baseItem.rewardName;
            reward.rewardSprite = baseItem.rewardSprite;
            reward.spritePath = baseItem.spritePath;
        }
    }

    private static QuizEventConfig NormalizeRemoteConfig(BackendQuizEventConfigDto source)
    {
        if (source == null)
            return null;

        QuizEventConfig config = new QuizEventConfig
        {
            eventId = FirstNonEmpty(source.eventId, source.activityId, source.quizId, source.id),
            theme = FirstNonEmpty(source.theme, source.title, source.topic),
            startTimeString = FirstNonEmpty(
                source.startTimeString,
                source.startTime,
                source.startAt,
                source.beginTime,
                source.beginAt,
                source.openTime),
            totalQuestions = FirstPositive(source.totalQuestions, source.questionCount, source.questionsCount, source.quizCount),
            durationDays = FirstPositive(source.durationDays, source.duration, source.durationDay, source.days),
            questionsFile = FirstNonEmpty(source.questionsFile, source.questionFile, source.questionBankFile, source.questionBank),
            finalRewards = source.finalRewards ?? source.rewards ?? source.rewardList ?? source.awards
        };

        if (config.durationDays <= 0 &&
            TryParseDateTime(config.startTimeString, out DateTime start) &&
            TryParseDateTime(FirstNonEmpty(source.endTime, source.endAt), out DateTime end) &&
            end > start)
        {
            config.durationDays = Mathf.Max(1, (int)Math.Ceiling((end - start).TotalDays));
        }

        return config;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        if (values == null)
            return null;

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static int FirstPositive(params int[] values)
    {
        if (values == null)
            return 0;

        foreach (int value in values)
        {
            if (value > 0)
                return value;
        }

        return 0;
    }

    private static bool TryParseDateTime(string value, out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = DateTime.MinValue;
            return false;
        }

        return DateTime.TryParse(value, out result);
    }

    // 鑾峰彇褰撳墠娲诲姩閰嶇疆锛堝悗绔ā寮忎紭鍏堬級
    public static QuizEventConfig GetCurrentConfig()
    {
        if (UseBackendQuiz())
            return _currentConfig;

        // 鏈湴妯″紡閫昏緫锛堜繚鐣欏吋瀹癸級
        return LoadLocalCurrentConfig();
    }

    private static List<QuizEventConfig> _allEvents;
    private static QuizEventConfig LoadLocalCurrentConfig()
    {
        if (_allEvents == null)
        {
            TextAsset json = Resources.Load<TextAsset>("Jsons/QuizEvents");
            if (json == null)
            {
                _allEvents = new List<QuizEventConfig>();
                return null;
            }
            _allEvents = JsonUtility.FromJson<QuizEventListWrapper>(json.text).quizEvents;
            foreach (QuizEventConfig quizEvent in _allEvents)
            {
                if (!DateTime.TryParse(quizEvent.startTimeString, out quizEvent.startTime))
                    quizEvent.startTime = DateTime.Today;
                BindRewardMetadata(quizEvent);
            }
        }

        DateTime today = DateTime.Today;
        QuizEventConfig config = _allEvents
            .FirstOrDefault(quizEvent => today >= quizEvent.startTime && today < quizEvent.startTime.AddDays(quizEvent.durationDays));
        config ??= _allEvents.OrderByDescending(quizEvent => quizEvent.startTime).First();

        if (quizEventState != null && quizEventState.eventId != config.eventId)
        {
            ResetWeeklyProgressLocal();
            quizEventState.eventId = config.eventId;
        }

        return config;
    }

    private static void ResetWeeklyProgressLocal()
    {
        if (quizEventState == null)
            return;

        quizEventState.weeklyScore = 0;
        quizEventState.totalSessions = 0;
        quizEventState.hasPlayedToday = false;
        quizEventState.lastPlayDate = DateTime.MinValue;
        quizEventState.rewardClaimedEventId = string.Empty;
        quizEventState.claimedWeeklyCoinAmount = 0;
        quizEventState.answerHistory = new List<string>();
        Session = null;
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static TimeSpan GetTimeLeft()
    {
        QuizEventConfig config = GetCurrentConfig();
        if (config == null)
        {
            Debug.LogWarning("[QuizEventSystem] GetTimeLeft: config is null");
            return TimeSpan.Zero;
        }

        Debug.Log($"[QuizEventSystem] GetTimeLeft - eventId: {config.eventId}, " +
                  $"durationDays: {config.durationDays}, startTimeString: {config.startTimeString}, " +
                  $"startTime: {config.startTime}");

        if (config.startTime == DateTime.MinValue && !string.IsNullOrEmpty(config.startTimeString))
        {
            if (DateTime.TryParse(config.startTimeString, out DateTime parsed))
            {
                config.startTime = parsed;
                Debug.Log($"[QuizEventSystem] Parsed startTime: {config.startTime}");
            }
            else
            {
                Debug.LogError($"[QuizEventSystem] Failed to parse startTimeString: {config.startTimeString}");
                config.startTime = DateTime.Today;
            }
        }

        int duration = config.durationDays > 0 ? config.durationDays : 7;
        DateTime end = config.startTime.AddDays(duration);
        TimeSpan time = end - DateTime.Now;

        Debug.Log($"[QuizEventSystem] Start: {config.startTime}, End: {end}, Time left: {time}");

        return time < TimeSpan.Zero ? TimeSpan.Zero : time;
    }

    public static bool HasPlayedToday()
    {
        EnsureQuizState();
        if (quizEventState == null)
            return false;

        if (UseBackendQuiz())
        {
            string eventId = _currentConfig != null ? _currentConfig.eventId : quizEventState.eventId;
            if (IsAbandonLockedToday(eventId))
                return true;
            return quizEventState.hasPlayedToday;
        }

        // 鏈湴妯″紡
        return quizEventState.lastPlayDate.Date == DateTime.Today;
    }

    public static bool IsCurrentEventFinalDay()
    {
        QuizEventConfig config = GetCurrentConfig();
        if (config == null)
            return false;

        DateTime start = config.startTime.Date;
        DateTime end = start.AddDays(Mathf.Max(1, config.durationDays) - 1);
        return DateTime.Today == end;
    }

    public static bool HasActiveSession()
    {
        return Session != null && !Session.isFinished;
    }

    public static int GetAnsweredCount()
    {
        return Session == null ? 0 : Session.userAnswers.Count(answer => answer >= 0);
    }

    public static bool HasUnansweredQuestions()
    {
        return Session == null || Session.userAnswers.Contains(-1);
    }

    // 鏈湴妯″紡锛氬紑濮嬫柊浼氳瘽
    public static void StartNewSession()
    {
        if (UseBackendQuiz())
            return;

        if (HasPlayedToday())
            return;

        // 鏈湴妯″紡鍔犺浇棰樼洰閫昏緫锛堜繚鐣欏吋瀹癸級
        LoadLocalQuestions();
    }

    private static List<QuizQuestion> _localQuestions;
    private static void LoadLocalQuestions()
    {
        QuizEventConfig config = GetCurrentConfig();
        if (config == null || string.IsNullOrWhiteSpace(config.questionsFile))
        {
            _localQuestions = new List<QuizQuestion>();
            return;
        }

        TextAsset json = Resources.Load<TextAsset>($"Jsons/QuizQuestions/{config.questionsFile}");
        if (json == null)
        {
            _localQuestions = new List<QuizQuestion>();
            return;
        }

        _localQuestions = JsonUtility.FromJson<QuizQuestionListWrapper>(json.text).questions;

        List<QuizQuestion> questions = _localQuestions
            .OrderBy(_ => Guid.NewGuid())
            .Take(config.totalQuestions)
            .ToList();

        Session = new QuizSessionState
        {
            currentIndex = 0,
            questions = questions,
            userAnswers = Enumerable.Repeat(-1, questions.Count).ToList(),
            isFinished = false
        };

        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static int SubmitSessionLocal()
    {
        if (UseBackendQuiz() || Session == null)
            return 0;

        int score = 0;
        int correctCount = 0;
        for (int i = 0; i < Session.questions.Count; i++)
        {
            if (i < Session.userAnswers.Count && Session.userAnswers[i] == Session.questions[i].correctIndex)
            {
                score += 10;
                correctCount++;
            }
        }

        quizEventState.weeklyScore += score;
        quizEventState.lastPlayDate = DateTime.Today;
        quizEventState.hasPlayedToday = true;
        quizEventState.eventId = GetCurrentConfig()?.eventId ?? string.Empty;
        quizEventState.answerHistory ??= new List<string>();
        quizEventState.totalSessions += 1;
        quizEventState.totalCorrectAnswers += correctCount;

        ActivityRewardSystem.GrantQuizSessionReward(correctCount);
        TitleEventReporter.ReportQuizCorrect(correctCount);
        quizEventState.answerHistory.Add(
            $"{DateTime.Today:yyyy-MM-dd} | 绛斿 {correctCount}/{Session.questions.Count} | +{score} 绉垎 | 鎬荤Н鍒?{quizEventState.weeklyScore}");

        Session.isFinished = true;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
        return score;
    }

    public static void AbandonSession()
    {
        Session = null;
        if (!UseBackendQuiz())
            LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static int GetCurrentWeeklyCoinReward()
    {
        int weeklyScore = Mathf.Clamp(quizEventState != null ? quizEventState.weeklyScore : 0, 0, 700);
        WeeklyRewardTier tier = WeeklyRewardTiers.LastOrDefault(item => weeklyScore >= item.minScore && weeklyScore <= item.maxScore);
        return tier != null ? tier.coinAmount : 0;
    }

    public static int GetClaimableWeeklyCoinReward()
    {
        if (UseBackendQuiz())
            return Mathf.Max(0, _backendClaimableCoin);

        QuizEventConfig config = GetCurrentConfig();
        if (config == null) return 0;

        int targetCoin = GetCurrentWeeklyCoinReward();
        int claimedCoin = quizEventState.rewardClaimedEventId == config.eventId ? quizEventState.claimedWeeklyCoinAmount : 0;
        return Mathf.Max(0, targetCoin - claimedCoin);
    }

    public static bool CanClaimWeeklyReward()
    {
        if (UseBackendQuiz())
            return _backendCanClaimWeeklyReward && !_backendWeeklyRewardClaimed && _backendClaimableCoin > 0;

        QuizEventConfig config = GetCurrentConfig();
        return config != null && IsCurrentEventFinalDay()
            && quizEventState.totalSessions > 0
            && GetClaimableWeeklyCoinReward() > 0;
    }

    public static void MarkPlayedTodayFromAbandon()
    {
        EnsureQuizState();
        if (quizEventState == null)
            return;

        Session = null;
        quizEventState.lastPlayDate = DateTime.Today;
        quizEventState.hasPlayedToday = true;
        string eventId = _currentConfig != null ? _currentConfig.eventId : quizEventState.eventId;
        if (string.IsNullOrWhiteSpace(eventId))
            eventId = _currentEventId;
        if (!string.IsNullOrWhiteSpace(eventId))
            SaveAbandonLock(eventId, DateTime.Today);
        if (!UseBackendQuiz())
            LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static List<RewardItem> ClaimWeeklyReward()
    {
        if (UseBackendQuiz())
            return new List<RewardItem>();

        QuizEventConfig config = GetCurrentConfig();
        if (config == null) return new List<RewardItem>();

        int claimableCoin = GetClaimableWeeklyCoinReward();
        if (claimableCoin <= 0)
            return new List<RewardItem>();

        quizEventState.rewardClaimedEventId = config.eventId;
        quizEventState.claimedWeeklyCoinAmount += claimableCoin;

        RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
        List<RewardItem> rewards = new()
        {
            new RewardItem
            {
                rewardId = 1,
                rewardName = "\u7eaa\u5ff5\u5e01",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = claimableCoin
            }
        };

        ActivityRewardSystem.GrantRewards("\u6821\u56ed\u95ee\u7b54\u5468\u5956\u52b1", 0, rewards);
        LocalProfileSaveSystem.SaveCurrentAccount();
        return rewards;
    }

    public static List<RewardItem> GetCurrentTierPreviewRewards()
    {
        QuizEventConfig config = GetCurrentConfig();
        if (config?.finalRewards != null && config.finalRewards.Count > 0)
            return config.finalRewards;

        RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
        return new List<RewardItem>
        {
            new RewardItem
            {
                rewardId = 1,
                rewardName = "\u7eaa\u5ff5\u5e01",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = GetCurrentWeeklyCoinReward()
            }
        };
    }

    public static string GetWeeklyRewardDescription()
    {
        if (UseBackendQuiz())
            return $"\u5f53\u524d\u603b\u79ef\u5206\uff1a{quizEventState?.weeklyScore ?? 0} \u5206\n\u672c\u5468\u53ef\u9886\u53d6\uff1a{Mathf.Max(0, _backendClaimableCoin)} \u7eaa\u5ff5\u5e01";

        int currentReward = GetCurrentWeeklyCoinReward();
        int claimableReward = GetClaimableWeeklyCoinReward();
        return $"\u5f53\u524d\u603b\u79ef\u5206\uff1a{quizEventState.weeklyScore} \u5206\n\u5f53\u524d\u5956\u52b1\u6863\u4f4d\uff1a{currentReward} \u7eaa\u5ff5\u5e01\n\u672c\u5468\u53ef\u9886\u53d6\uff1a{claimableReward} \u7eaa\u5ff5\u5e01";
    }

    public static List<string> GetAnswerHistory()
    {
        EnsureQuizState();
        quizEventState.answerHistory ??= new List<string>();
        return quizEventState.answerHistory;
    }

    private static DateTime ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;

        return DateTime.TryParse(value, out DateTime parsed) ? parsed : DateTime.MinValue;
    }

    [Serializable]
    public class QuizEventListWrapper
    {
        public List<QuizEventConfig> quizEvents;
    }

    [Serializable]
    public class QuizQuestionListWrapper
    {
        public List<QuizQuestion> questions;
    }
}

