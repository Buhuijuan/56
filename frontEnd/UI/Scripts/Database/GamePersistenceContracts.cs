using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IGamePersistenceService
{
    Task SaveUserAsync(AccountData account, CancellationToken ct = default);
    Task SaveRoleAsync(long userId, long schoolId, RoleData role, int slotNo, CancellationToken ct = default);
    Task SavePlayerProfileAsync(long roleId, RoleData role, CancellationToken ct = default);
    Task SavePlayerTaskStateAsync(long roleId, PlayerTaskState state, CancellationToken ct = default);
    Task SavePlayerTitleStateAsync(long roleId, PlayerTitleState titleState, CancellationToken ct = default);
    Task SavePlayerGrowthStateAsync(long roleId, PlayerGrowthState growthState, CancellationToken ct = default);
    Task SavePlayerRewardStateAsync(long roleId, PlayerRewardState rewardState, CancellationToken ct = default);
    Task SavePlayerStatAsync(long roleId, PlayerTitleState titleState, PlayerQuizEventState quizState, PlayerClockInEventState clockInState, CancellationToken ct = default);
    Task SaveSignInAsync(long roleId, PlayerSignInState signInState, CancellationToken ct = default);
    Task SaveQuizStateAsync(long roleId, PlayerQuizEventState quizState, CancellationToken ct = default);
    Task SaveQuizProgressAsync(long roleId, PlayerQuizEventState quizState, IEnumerable<DbPlayerQuizAnswer> answers, CancellationToken ct = default);
    Task SaveClockInStateAsync(long roleId, PlayerClockInEventState clockInState, CancellationToken ct = default);
    Task SaveMorningCheckinAsync(long roleId, PlayerClockInEventState clockInState, DbPlayerMorningCheckin checkin, CancellationToken ct = default);
    Task SaveStoryStateAsync(long roleId, PlayerStoryEventState storyState, CancellationToken ct = default);
    Task SaveStoryRecordAsync(long roleId, StoryRecord storyRecord, CancellationToken ct = default);
    Task SaveStoryRecordsAsync(long roleId, IEnumerable<StoryRecord> storyRecords, CancellationToken ct = default);
    Task SavePhotoCheckinRecordAsync(DbPlayerCheckinRecord record, CancellationToken ct = default);

    Task<DbPlayerProfile> LoadPlayerProfileAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerTask>> LoadPlayerTasksAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerTitle>> LoadPlayerTitlesAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerGrowthState> LoadPlayerGrowthStateAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerRewardState> LoadPlayerRewardStateAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerRewardEntry>> LoadPlayerRewardEntriesAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerStat> LoadPlayerStatAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerSignIn>> LoadPlayerSignInsAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerQuizState> LoadQuizStateAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerQuizAnswer>> LoadQuizAnswersAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerClockInState> LoadClockInStateAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerMorningCheckin>> LoadMorningCheckinsAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerStoryState> LoadStoryStateAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbStoryRecord>> LoadStoryRecordsAsync(long roleId, CancellationToken ct = default);
}
