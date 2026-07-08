using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IMyCampusDatabaseContext
{
    IUserRepository Users { get; }
    IEmailVerificationCodeRepository EmailVerificationCodes { get; }
    ISchoolRepository Schools { get; }
    IRoleRepository Roles { get; }
    IPlayerProfileRepository PlayerProfiles { get; }
    ILevelConfigRepository LevelConfigs { get; }
    ITaskConfigRepository TaskConfigs { get; }
    ITitleConfigRepository TitleConfigs { get; }
    IPlayerTaskRepository PlayerTasks { get; }
    IPlayerTitleRepository PlayerTitles { get; }
    IPlayerStatRepository PlayerStats { get; }
    IPlayerGrowthStateRepository PlayerGrowthStates { get; }
    IPlayerRewardStateRepository PlayerRewardStates { get; }
    IPlayerRewardEntryRepository PlayerRewardEntries { get; }
    IPlayerSignInRepository PlayerSignIns { get; }
    IPlayerOnlineDailyRepository PlayerOnlineDailies { get; }
    IPlayerOnlineRewardClaimRepository PlayerOnlineRewardClaims { get; }
    IPlayerQuizStateRepository PlayerQuizStates { get; }
    IPlayerQuizAnswerRepository PlayerQuizAnswers { get; }
    IPlayerClockInStateRepository PlayerClockInStates { get; }
    IPlayerMorningCheckinRepository PlayerMorningCheckins { get; }
    IPlayerStoryStateRepository PlayerStoryStates { get; }
    IStoryRecordRepository StoryRecords { get; }
    IBuildingLocationRepository BuildingLocations { get; }
    IPlayerCheckinRecordRepository PlayerCheckinRecords { get; }
}

public interface IUserRepository
{
    Task<DbUser> GetByIdAsync(long userId, CancellationToken ct = default);
    Task<DbUser> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<long> InsertAsync(DbUser user, CancellationToken ct = default);
    Task UpdateAsync(DbUser user, CancellationToken ct = default);
}

public interface IEmailVerificationCodeRepository
{
    Task<DbEmailVerificationCode> GetLatestValidCodeAsync(string email, string purpose, CancellationToken ct = default);
    Task<long> InsertAsync(DbEmailVerificationCode entity, CancellationToken ct = default);
    Task MarkUsedAsync(long emailVerificationCodeId, DateTime usedAt, CancellationToken ct = default);
}

public interface ISchoolRepository
{
    Task<DbSchool> GetByIdAsync(long schoolId, CancellationToken ct = default);
    Task<DbSchool> GetByCodeAsync(string schoolCode, CancellationToken ct = default);
    Task<IReadOnlyList<DbSchool>> GetEnabledAsync(CancellationToken ct = default);
}

public interface IRoleRepository
{
    Task<DbRole> GetByIdAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbRole>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<long> InsertAsync(DbRole role, CancellationToken ct = default);
    Task UpdateAsync(DbRole role, CancellationToken ct = default);
}

public interface IPlayerProfileRepository
{
    Task<DbPlayerProfile> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerProfile profile, CancellationToken ct = default);
}

public interface ILevelConfigRepository
{
    Task<DbLevelConfig> GetByLevelAsync(int level, CancellationToken ct = default);
    Task<IReadOnlyList<DbLevelConfig>> GetAllAsync(CancellationToken ct = default);
}

public interface ITaskConfigRepository
{
    Task<DbTaskConfig> GetByIdAsync(long taskId, CancellationToken ct = default);
    Task<DbTaskConfig> GetByCodeAsync(string taskCode, CancellationToken ct = default);
    Task<IReadOnlyList<DbTaskConfig>> GetByChapterAsync(int chapterNo, CancellationToken ct = default);
}

public interface ITitleConfigRepository
{
    Task<DbTitleConfig> GetByIdAsync(long titleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbTitleConfig>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<IReadOnlyList<DbTitleConfig>> GetEnabledAsync(CancellationToken ct = default);
}

public interface IPlayerTaskRepository
{
    Task<DbPlayerTask> GetByRoleAndTaskAsync(long roleId, long taskId, CancellationToken ct = default);
    Task<IReadOnlyList<DbPlayerTask>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerTask entity, CancellationToken ct = default);
}

public interface IPlayerTitleRepository
{
    Task<IReadOnlyList<DbPlayerTitle>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerTitle entity, CancellationToken ct = default);
    Task EquipAsync(long roleId, long titleId, CancellationToken ct = default);
}

public interface IPlayerStatRepository
{
    Task<DbPlayerStat> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerStat entity, CancellationToken ct = default);
}

public interface IPlayerGrowthStateRepository
{
    Task<DbPlayerGrowthState> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerGrowthState entity, CancellationToken ct = default);
}

public interface IPlayerRewardStateRepository
{
    Task<DbPlayerRewardState> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerRewardState entity, CancellationToken ct = default);
}

public interface IPlayerRewardEntryRepository
{
    Task<IReadOnlyList<DbPlayerRewardEntry>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerRewardEntry entity, CancellationToken ct = default);
    Task UpsertBatchAsync(IEnumerable<DbPlayerRewardEntry> entities, CancellationToken ct = default);
}

public interface IPlayerSignInRepository
{
    Task<IReadOnlyList<DbPlayerSignIn>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerSignIn> GetByRoleAndDateAsync(long roleId, DateTime signDate, CancellationToken ct = default);
    Task<long> InsertAsync(DbPlayerSignIn entity, CancellationToken ct = default);
}

public interface IPlayerOnlineDailyRepository
{
    Task<DbPlayerOnlineDaily> GetByRoleAndDateAsync(long roleId, DateTime onlineDate, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerOnlineDaily entity, CancellationToken ct = default);
}

public interface IPlayerOnlineRewardClaimRepository
{
    Task<IReadOnlyList<DbPlayerOnlineRewardClaim>> GetByRoleAndDateAsync(long roleId, DateTime onlineDate, CancellationToken ct = default);
    Task<long> InsertAsync(DbPlayerOnlineRewardClaim entity, CancellationToken ct = default);
}

public interface IPlayerQuizStateRepository
{
    Task<DbPlayerQuizState> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerQuizState entity, CancellationToken ct = default);
}

public interface IPlayerQuizAnswerRepository
{
    Task<IReadOnlyList<DbPlayerQuizAnswer>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task InsertBatchAsync(IEnumerable<DbPlayerQuizAnswer> entities, CancellationToken ct = default);
}

public interface IPlayerClockInStateRepository
{
    Task<DbPlayerClockInState> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerClockInState entity, CancellationToken ct = default);
}

public interface IPlayerMorningCheckinRepository
{
    Task<IReadOnlyList<DbPlayerMorningCheckin>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task<DbPlayerMorningCheckin> GetByRoleAndDateAsync(long roleId, DateTime checkinDate, CancellationToken ct = default);
    Task<long> InsertAsync(DbPlayerMorningCheckin entity, CancellationToken ct = default);
}

public interface IPlayerStoryStateRepository
{
    Task<DbPlayerStoryState> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task UpsertAsync(DbPlayerStoryState entity, CancellationToken ct = default);
}

public interface IStoryRecordRepository
{
    Task<DbStoryRecord> GetByIdAsync(long storyRecordId, CancellationToken ct = default);
    Task<IReadOnlyList<DbStoryRecord>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<DbStoryRecord>> GetUploadedAsync(CancellationToken ct = default);
    Task<long> InsertAsync(DbStoryRecord entity, CancellationToken ct = default);
    Task UpdateAsync(DbStoryRecord entity, CancellationToken ct = default);
}

public interface IBuildingLocationRepository
{
    Task<DbBuildingLocation> GetByIdAsync(long buildingLocationId, CancellationToken ct = default);
    Task<DbBuildingLocation> GetByCodeAsync(string buildingCode, CancellationToken ct = default);
    Task<IReadOnlyList<DbBuildingLocation>> GetBySchoolIdAsync(long schoolId, CancellationToken ct = default);
}

public interface IPlayerCheckinRecordRepository
{
    Task<IReadOnlyList<DbPlayerCheckinRecord>> GetByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task<long> InsertAsync(DbPlayerCheckinRecord entity, CancellationToken ct = default);
}

[Serializable]
public class DbUser
{
    public long user_id;
    public string email;
    public string password_hash;
    public DateTime created_at;
}

[Serializable]
public class DbEmailVerificationCode
{
    public long email_verification_code_id;
    public string email;
    public string code;
    public DateTime expire_at;
    public int is_used;
    public DateTime created_at;
    public string purpose;
}

[Serializable]
public class DbSchool
{
    public long school_id;
    public string school_name;
    public string school_code;
    public int status;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbRole
{
    public long role_id;
    public long user_id;
    public long school_id;
    public int slot_no;
    public string role_name;
    public string avatar_url;
    public int status;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerProfile
{
    public long role_id;
    public string nickname;
    public int level;
    public int exp;
    public int coin;
    public long? current_title_id;
    public int bike_unlocked;
    public DateTime? first_login_at;
    public DateTime? last_login_at;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbLevelConfig
{
    public int level;
    public int need_exp;
    public int total_exp;
}

[Serializable]
public class DbTaskConfig
{
    public long task_id;
    public string task_code;
    public string task_name;
    public string task_type;
    public int chapter_no;
    public int step_no;
    public string category;
    public string trigger_type;
    public string pre_task_code;
    public string target_type;
    public long? target_id;
    public int target_count;
    public int reward_exp;
    public int reward_coin;
    public string reward_unlock_feature;
    public string description;
    public int status;
}

[Serializable]
public class DbTitleConfig
{
    public long title_id;
    public string title_name;
    public string title_category;
    public string unlock_type;
    public string condition_json;
    public int reward_exp;
    public int sort_no;
    public int status;
}

[Serializable]
public class DbPlayerTask
{
    public long player_task_id;
    public long role_id;
    public long task_id;
    public string task_status;
    public int progress_current;
    public int progress_target;
    public int current_step;
    public DateTime? accepted_at;
    public DateTime? completed_at;
    public DateTime? reward_claimed_at;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerTitle
{
    public long player_title_id;
    public long role_id;
    public long title_id;
    public DateTime unlocked_at;
    public int is_equipped;
    public string source_type;
    public long? source_ref_id;
}

[Serializable]
public class DbPlayerStat
{
    public long role_id;
    public int npc_distinct_talk_count;
    public int animal_interact_count;
    public int elf_ask_count;
    public int photo_count;
    public int distinct_photo_location_count;
    public int bike_ride_count;
    public int quiz_correct_count;
    public int morning_checkin_days;
    public int story_completed_count;
    public int title_unlocked_count;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerGrowthState
{
    public long role_id;
    public string stage_completed_json;
    public string reward_claimed_json;
    public string task_completed_json;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerRewardState
{
    public long role_id;
    public int total_activity_exp_gained;
    public int total_memorial_coin;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerRewardEntry
{
    public long player_reward_entry_id;
    public long role_id;
    public int reward_id;
    public int amount;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerSignIn
{
    public long player_sign_in_id;
    public long role_id;
    public DateTime sign_date;
    public int cycle_day_no;
    public int reward_coin;
    public int is_extra_reward;
    public DateTime signed_at;
}

[Serializable]
public class DbPlayerOnlineDaily
{
    public long player_online_daily_id;
    public long role_id;
    public DateTime online_date;
    public int total_online_seconds;
    public DateTime? last_heartbeat_at;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerOnlineRewardClaim
{
    public long player_online_reward_claim_id;
    public long role_id;
    public DateTime online_date;
    public int reward_minutes;
    public DateTime claimed_at;
}

[Serializable]
public class DbPlayerQuizState
{
    public long role_id;
    public string event_id;
    public int weekly_score;
    public int total_correct_answers;
    public int total_sessions;
    public DateTime? last_play_date;
    public int has_played_today;
    public string reward_claimed_event_id;
    public int claimed_weekly_coin_amount;
    public string answer_history_json;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerQuizAnswer
{
    public long player_quiz_answer_id;
    public long role_id;
    public long question_id;
    public string selected_answer;
    public int is_correct;
    public DateTime answered_at;
}

[Serializable]
public class DbPlayerClockInState
{
    public long role_id;
    public string event_id;
    public DateTime? last_checkin_date;
    public int total_checkin_count;
    public string checked_in_json;
    public string last_checked_location_id;
    public string history_records_json;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerMorningCheckin
{
    public long player_morning_checkin_id;
    public long role_id;
    public DateTime checkin_date;
    public long location_id;
    public DateTime checked_at;
}

[Serializable]
public class DbPlayerStoryState
{
    public long role_id;
    public string event_id;
    public int has_finished;
    public int reward_claimed;
    public DateTime? last_play_date;
    public string current_story_id;
    public string uploaded_story_ids_json;
    public DateTime updated_at;
}

[Serializable]
public class DbStoryRecord
{
    public long story_record_id;
    public long account_id;
    public long role_id;
    public string event_id;
    public string theme;
    public string full_text;
    public int uploaded;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbBuildingLocation
{
    public long building_location_id;
    public long school_id;
    public string building_code;
    public string building_name;
    public decimal pos_x;
    public decimal pos_y;
    public decimal pos_z;
    public decimal checkin_radius;
    public int status;
    public DateTime created_at;
    public DateTime updated_at;
}

[Serializable]
public class DbPlayerCheckinRecord
{
    public long player_checkin_record_id;
    public long role_id;
    public long? task_id;
    public long? building_location_id;
    public decimal current_pos_x;
    public decimal current_pos_y;
    public decimal current_pos_z;
    public decimal distance_to_target;
    public int is_success;
    public string trigger_type;
    public DateTime checked_at;
}
