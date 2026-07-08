using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class NullGamePersistenceService : IGamePersistenceService
{
    public Task SaveUserAsync(AccountData account, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveRoleAsync(long userId, long schoolId, RoleData role, int slotNo, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerProfileAsync(long roleId, RoleData role, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerTaskStateAsync(long roleId, PlayerTaskState state, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerTitleStateAsync(long roleId, PlayerTitleState titleState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerGrowthStateAsync(long roleId, PlayerGrowthState growthState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerRewardStateAsync(long roleId, PlayerRewardState rewardState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePlayerStatAsync(long roleId, PlayerTitleState titleState, PlayerQuizEventState quizState, PlayerClockInEventState clockInState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveSignInAsync(long roleId, PlayerSignInState signInState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveQuizStateAsync(long roleId, PlayerQuizEventState quizState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveQuizProgressAsync(long roleId, PlayerQuizEventState quizState, IEnumerable<DbPlayerQuizAnswer> answers, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveClockInStateAsync(long roleId, PlayerClockInEventState clockInState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveMorningCheckinAsync(long roleId, PlayerClockInEventState clockInState, DbPlayerMorningCheckin checkin, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveStoryStateAsync(long roleId, PlayerStoryEventState storyState, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveStoryRecordAsync(long roleId, StoryRecord storyRecord, CancellationToken ct = default) => Task.CompletedTask;
    public Task SaveStoryRecordsAsync(long roleId, IEnumerable<StoryRecord> storyRecords, CancellationToken ct = default) => Task.CompletedTask;
    public Task SavePhotoCheckinRecordAsync(DbPlayerCheckinRecord record, CancellationToken ct = default) => Task.CompletedTask;

    public Task<DbPlayerProfile> LoadPlayerProfileAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerProfile>(null);
    public Task<IReadOnlyList<DbPlayerTask>> LoadPlayerTasksAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerTask>>(new List<DbPlayerTask>());
    public Task<IReadOnlyList<DbPlayerTitle>> LoadPlayerTitlesAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerTitle>>(new List<DbPlayerTitle>());
    public Task<DbPlayerGrowthState> LoadPlayerGrowthStateAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerGrowthState>(null);
    public Task<DbPlayerRewardState> LoadPlayerRewardStateAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerRewardState>(null);
    public Task<IReadOnlyList<DbPlayerRewardEntry>> LoadPlayerRewardEntriesAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerRewardEntry>>(new List<DbPlayerRewardEntry>());
    public Task<DbPlayerStat> LoadPlayerStatAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerStat>(null);
    public Task<IReadOnlyList<DbPlayerSignIn>> LoadPlayerSignInsAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerSignIn>>(new List<DbPlayerSignIn>());
    public Task<DbPlayerQuizState> LoadQuizStateAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerQuizState>(null);
    public Task<IReadOnlyList<DbPlayerQuizAnswer>> LoadQuizAnswersAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerQuizAnswer>>(new List<DbPlayerQuizAnswer>());
    public Task<DbPlayerClockInState> LoadClockInStateAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerClockInState>(null);
    public Task<IReadOnlyList<DbPlayerMorningCheckin>> LoadMorningCheckinsAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbPlayerMorningCheckin>>(new List<DbPlayerMorningCheckin>());
    public Task<DbPlayerStoryState> LoadStoryStateAsync(long roleId, CancellationToken ct = default) => Task.FromResult<DbPlayerStoryState>(null);
    public Task<IReadOnlyList<DbStoryRecord>> LoadStoryRecordsAsync(long roleId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DbStoryRecord>>(new List<DbStoryRecord>());
}
