package com.mycampus.backend.service;

import com.mycampus.backend.common.AppException;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.game.config.GameConfigs;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.entity.PlayerStat;
import com.mycampus.backend.player.entity.PlayerTitle;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import com.mycampus.backend.progression.entity.PlayerGrowthState;
import com.mycampus.backend.progression.entity.PlayerLevelState;
import com.mycampus.backend.progression.entity.PlayerTitleState;
import com.mycampus.backend.progression.repository.PlayerGrowthStateRepository;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.signin.entity.PlayerSignInState;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import com.mycampus.backend.task.TaskStatus;
import com.mycampus.backend.task.entity.PlayerTask;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

@Service
public class ProgressionService {

    private record BehaviorTitleRule(Long titleId, String sourceType, java.util.function.Predicate<BehaviorTitleContext> predicate) {
    }

    private record BehaviorTitleContext(PlayerStat stat, PlayerTitleState titleState) {
    }

    private static final List<BehaviorTitleRule> BEHAVIOR_TITLE_RULES = List.of(
            new BehaviorTitleRule(25L, "SOCIAL", ctx -> valueOf(ctx.stat().getNpcDistinctTalkCount()) >= 10),
            new BehaviorTitleRule(30L, "FUNCTION", ctx -> valueOf(ctx.stat().getElfAskCount()) >= 30),
            new BehaviorTitleRule(31L, "FUNCTION", ctx -> valueOf(ctx.stat().getPhotoCount()) >= 30),
            new BehaviorTitleRule(32L, "FUNCTION", ctx -> valueOf(ctx.stat().getBikeRideCount()) >= 15),
            new BehaviorTitleRule(33L, "FUNCTION", ctx -> valueOf(ctx.stat().getQuizCorrectCount()) >= 50),
            new BehaviorTitleRule(34L, "FUNCTION", ctx -> valueOf(ctx.stat().getMorningCheckinDays()) >= 30),
            new BehaviorTitleRule(35L, "FUNCTION", ctx -> valueOf(ctx.stat().getStoryCompletedCount()) >= 10),
            new BehaviorTitleRule(36L, "COLLECT", ctx -> ctx.titleState().getUnlockedTitleIds().size() >= 10),
            new BehaviorTitleRule(37L, "COLLECT", ctx -> ctx.titleState().getUnlockedTitleIds().size() >= 20),
            new BehaviorTitleRule(38L, "COLLECT", ctx -> ctx.titleState().getUnlockedTitleIds().size() >= 30),
            new BehaviorTitleRule(39L, "COLLECT", ctx ->
                    ctx.titleState().getUnlockedTitleIds().contains(36)
                            && ctx.titleState().getUnlockedTitleIds().contains(37)
                            && ctx.titleState().getUnlockedTitleIds().contains(38))
    );

    private final PlayerService playerService;
    private final PlayerLevelStateRepository levelRepository;
    private final PlayerGrowthStateRepository growthRepository;
    private final PlayerTitleStateRepository titleRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerTitleRepository playerTitleRepository;
    private final PlayerStatRepository playerStatRepository;
    private final GameConfigService gameConfigService;
    private final PlayerTaskRepository playerTaskRepository;
    private final TaskConfigRepository taskConfigRepository;
    private final PlayerSignInStateRepository signInRepository;

    public ProgressionService(PlayerService playerService,
                              PlayerLevelStateRepository levelRepository,
                              PlayerGrowthStateRepository growthRepository,
                              PlayerTitleStateRepository titleRepository,
                              PlayerProfileRepository profileRepository,
                              PlayerTitleRepository playerTitleRepository,
                              PlayerStatRepository playerStatRepository,
                              GameConfigService gameConfigService,
                              PlayerTaskRepository playerTaskRepository,
                              TaskConfigRepository taskConfigRepository,
                              PlayerSignInStateRepository signInRepository) {
        this.playerService = playerService;
        this.levelRepository = levelRepository;
        this.growthRepository = growthRepository;
        this.titleRepository = titleRepository;
        this.profileRepository = profileRepository;
        this.playerTitleRepository = playerTitleRepository;
        this.playerStatRepository = playerStatRepository;
        this.gameConfigService = gameConfigService;
        this.playerTaskRepository = playerTaskRepository;
        this.taskConfigRepository = taskConfigRepository;
        this.signInRepository = signInRepository;
    }

    public Map<String, Object> growth(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerLevelState level = levelRepository.findById(roleId).orElseThrow();
        PlayerGrowthState growth = growthRepository.findById(roleId).orElseThrow();
        PlayerTitleState title = titleRepository.findById(roleId).orElseThrow();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        PlayerSignInState signIn = signInRepository.findById(roleId).orElseThrow();
        refreshGrowthState(roleId, growth, signIn);
        growthRepository.save(growth);

        Map<String, Object> response = new LinkedHashMap<>();
        response.put("levelState", level);
        response.put("growthState", growth);
        response.put("titleState", title);
        response.put("profile", profile);
        response.put("levelConfig", gameConfigService.levels());
        response.put("growthConfig", gameConfigService.growthStages());
        response.put("titleConfig", gameConfigService.titles());
        return response;
    }

    @Transactional
    public Map<String, Object> refreshGrowth(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerGrowthState growth = growthRepository.findById(roleId).orElseThrow();
        PlayerSignInState signIn = signInRepository.findById(roleId).orElseThrow();
        boolean updated = refreshGrowthState(roleId, growth, signIn);
        growthRepository.save(growth);
        return Map.of(
                "updated", updated,
                "growthState", growth
        );
    }

    @Transactional
    public Map<String, Object> claimGrowth(CurrentAccount principal, String stageId) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerGrowthState growth = growthRepository.findById(roleId).orElseThrow();
        PlayerSignInState signIn = signInRepository.findById(roleId).orElseThrow();
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();
        refreshGrowthState(roleId, growth, signIn);

        var stageConfig = gameConfigService.growthStages().stream()
                .filter(stage -> stage.stageID().equals(stageId))
                .findFirst()
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "成长阶段不存在"));

        if (!growth.getStageCompleted().contains(stageId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "成长阶段尚未完成");
        }
        if (growth.getRewardClaimed().contains(stageId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "成长奖励已领取");
        }

        growth.getRewardClaimed().add(stageId);
        growthRepository.save(growth);

        int addedCoin = 0;
        if (stageConfig.rewards() != null) {
            for (var reward : stageConfig.rewards()) {
                if (reward != null && reward.rewardId() != null && reward.rewardId() == 1 && reward.amount() != null) {
                    addedCoin += reward.amount();
                }
            }
        }
        if (addedCoin > 0) {
            grantCoins(roleId, "GROWTH_STAGE", stageId, addedCoin);
        }

        boolean titleUnlocked = false;
        if (stageConfig.titleID() != null && stageConfig.titleID() > 0) {
            titleUnlocked = unlockTitleInternal(roleId, titleState, stageConfig.titleID().longValue(), "GROWTH_STAGE", null);
        }
        List<Map<String, Object>> behaviorTitleUnlocks = refreshBehaviorTitlesInternal(roleId, titleState);
        titleRepository.save(titleState);

        return Map.of(
                "stageId", stageId,
                "claimed", true,
                "addedCoin", addedCoin,
                "titleUnlocked", titleUnlocked,
                "titleId", stageConfig.titleID() == null ? 0 : stageConfig.titleID(),
                "behaviorTitleUnlocks", behaviorTitleUnlocks
        );
    }

    @Transactional
    public Map<String, Object> claimLevelReward(CurrentAccount principal, Integer level) {
        Long roleId = playerService.currentRole(principal).getId();
        if (level == null || level <= 0) {
            throw new AppException(HttpStatus.BAD_REQUEST, "等级参数不合法");
        }

        PlayerLevelState levelState = levelRepository.findById(roleId).orElseThrow();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();

        var levelConfig = gameConfigService.levels().stream()
                .filter(config -> config.level() != null && config.level().equals(level))
                .findFirst()
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "等级奖励配置不存在"));

        int currentLevel = profile.getLevel() == null ? 1 : profile.getLevel();
        if (currentLevel < level) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前等级不足，无法领取该奖励");
        }
        if (levelState.getRewardClaimed().contains(level)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该等级奖励已领取");
        }

        levelState.getRewardClaimed().add(level);
        levelRepository.save(levelState);

        int addedCoin = sumRewardCoins(levelConfig.rewards());
        if (addedCoin > 0) {
            grantCoins(roleId, "LEVEL_REWARD", String.valueOf(level), addedCoin);
        }

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("level", level);
        result.put("claimed", true);
        result.put("addedCoin", addedCoin);
        result.put("rewardClaimed", new ArrayList<>(levelState.getRewardClaimed()));
        return result;
    }

    @Transactional
    public Map<String, Object> claimLevelTitle(CurrentAccount principal, Integer level) {
        Long roleId = playerService.currentRole(principal).getId();
        if (level == null || level <= 0) {
            throw new AppException(HttpStatus.BAD_REQUEST, "等级参数不合法");
        }

        PlayerLevelState levelState = levelRepository.findById(roleId).orElseThrow();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();

        var levelConfig = gameConfigService.levels().stream()
                .filter(config -> config.level() != null && config.level().equals(level))
                .findFirst()
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "等级称号配置不存在"));

        int currentLevel = profile.getLevel() == null ? 1 : profile.getLevel();
        if (currentLevel < level) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前等级不足，无法领取该称号");
        }
        if (levelState.getBoxOpened().contains(level)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该等级称号已领取");
        }
        if (levelConfig.titleID() == null || levelConfig.titleID() <= 0) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该等级未配置称号奖励");
        }

        levelState.getBoxOpened().add(level);
        levelRepository.save(levelState);

        boolean unlocked = unlockTitleInternal(roleId, titleState, levelConfig.titleID().longValue(), "LEVEL_TITLE", null);
        List<Map<String, Object>> behaviorTitleUnlocks = refreshBehaviorTitlesInternal(roleId, titleState);
        titleRepository.save(titleState);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("level", level);
        result.put("claimed", true);
        result.put("titleId", levelConfig.titleID());
        result.put("unlocked", unlocked);
        result.put("boxOpened", new ArrayList<>(levelState.getBoxOpened()));
        result.put("behaviorTitleUnlocks", behaviorTitleUnlocks);
        return result;
    }

    @Transactional
    public Map<String, Object> equipTitle(CurrentAccount principal, Integer titleId) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();
        if (!titleState.getUnlockedTitleIds().contains(titleId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "称号尚未解锁");
        }

        titleState.setEquippedTitleId(titleId);
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        profile.setCurrentTitleId(titleId.longValue());
        profile.touch();

        List<PlayerTitle> records = playerTitleRepository.findByRoleIdOrderByTitleIdAsc(roleId);
        for (PlayerTitle record : records) {
            record.setIsEquipped(record.getTitleId().equals(titleId.longValue()) ? 1 : 0);
        }

        titleRepository.save(titleState);
        profileRepository.save(profile);
        playerTitleRepository.saveAll(records);
        return Map.of("equippedTitleId", titleId);
    }

    @Transactional
    public Map<String, Object> grantExp(Long roleId, String sourceType, String sourceId, int exp) {
        PlayerLevelState levelState = levelRepository.findById(roleId).orElseThrow();
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        int beforeLevel = levelState.getLevel();

        levelState.setExp(levelState.getExp() + exp);
        var levelConfig = gameConfigService.levelForExp(levelState.getExp());
        levelState.setLevel(levelConfig.level());
        profile.setExp(levelState.getExp());
        profile.setLevel(levelState.getLevel());
        profile.touch();

        levelRepository.save(levelState);
        titleRepository.save(titleState);
        profileRepository.save(profile);
        List<Map<String, Object>> behaviorTitleUnlocks = refreshBehaviorTitlesInternal(roleId, titleState);
        return Map.of(
                "sourceType", sourceType,
                "sourceId", sourceId,
                "addedExp", exp,
                "levelBefore", beforeLevel,
                "levelAfter", levelState.getLevel(),
                "totalExp", levelState.getExp(),
                "behaviorTitleUnlocks", behaviorTitleUnlocks
        );
    }

    @Transactional
    public Map<String, Object> grantCoins(Long roleId, String sourceType, String sourceId, int coin) {
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        profile.setCoin(profile.getCoin() + coin);
        profile.touch();
        profileRepository.save(profile);
        return Map.of(
                "sourceType", sourceType,
                "sourceId", sourceId,
                "addedCoin", coin,
                "totalCoin", profile.getCoin()
        );
    }

    @Transactional
    public Map<String, Object> unlockTitle(Long roleId, Long titleId, String sourceType, Long sourceRefId) {
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();
        boolean unlocked = unlockTitleInternal(roleId, titleState, titleId, sourceType, sourceRefId);
        List<Map<String, Object>> behaviorTitleUnlocks = refreshBehaviorTitlesInternal(roleId, titleState);
        titleRepository.save(titleState);
        return Map.of("titleId", titleId, "unlocked", unlocked, "behaviorTitleUnlocks", behaviorTitleUnlocks);
    }

    @Transactional
    public Map<String, Object> unlockBike(Long roleId, String sourceType, String sourceId) {
        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        profile.setBikeUnlocked(1);
        profile.touch();
        profileRepository.save(profile);
        return Map.of(
                "sourceType", sourceType,
                "sourceId", sourceId,
                "bikeUnlocked", true
        );
    }

    @Transactional
    public List<Map<String, Object>> refreshBehaviorTitles(Long roleId) {
        PlayerTitleState titleState = titleRepository.findById(roleId).orElseThrow();
        List<Map<String, Object>> unlocked = refreshBehaviorTitlesInternal(roleId, titleState);
        titleRepository.save(titleState);
        return unlocked;
    }

    private boolean unlockTitleInternal(Long roleId, PlayerTitleState titleState, Long titleId, String sourceType, Long sourceRefId) {
        boolean added = titleState.getUnlockedTitleIds().add(titleId.intValue());
        PlayerTitle titleRecord = playerTitleRepository.findByRoleIdAndTitleId(roleId, titleId)
                .orElseGet(PlayerTitle::new);
        titleRecord.setRoleId(roleId);
        titleRecord.setTitleId(titleId);
        if (titleRecord.getUnlockedAt() == null) {
            titleRecord.setUnlockedAt(LocalDateTime.now());
        }
        titleRecord.setIsEquipped(titleRecord.getIsEquipped() == null ? 0 : titleRecord.getIsEquipped());
        titleRecord.setSourceType(sourceType);
        titleRecord.setSourceRefId(sourceRefId);
        playerTitleRepository.save(titleRecord);
        syncTitleUnlockedCount(roleId, titleState.getUnlockedTitleIds().size());
        return added;
    }

    private void syncTitleUnlockedCount(Long roleId, int count) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElse(null);
        if (stat == null) {
            return;
        }
        stat.setTitleUnlockedCount(count);
        stat.touch();
        playerStatRepository.save(stat);
    }

    private void syncGrowthStats(Long roleId, PlayerSignInState signIn, long coreBuildingReachedCount) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElse(null);
        if (stat == null) {
            return;
        }
        stat.setLoginDays(signIn != null && signIn.getTotalLoginDays() != null ? signIn.getTotalLoginDays() : 0);
        stat.setCoreBuildingReachedCount((int) coreBuildingReachedCount);
        stat.touch();
        playerStatRepository.save(stat);
    }

    private List<Map<String, Object>> refreshBehaviorTitlesInternal(Long roleId, PlayerTitleState titleState) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElse(null);
        if (stat == null) {
            return List.of();
        }

        List<Map<String, Object>> unlocked = new ArrayList<>();
        boolean changed;
        do {
            changed = false;
            BehaviorTitleContext context = new BehaviorTitleContext(stat, titleState);
            for (BehaviorTitleRule rule : BEHAVIOR_TITLE_RULES) {
                if (titleState.getUnlockedTitleIds().contains(rule.titleId().intValue())) {
                    continue;
                }
                if (!rule.predicate().test(context)) {
                    continue;
                }
                if (unlockTitleInternal(roleId, titleState, rule.titleId(), rule.sourceType(), null)) {
                    unlocked.add(titleView(rule.titleId()));
                    changed = true;
                }
            }
        } while (changed);

        return unlocked;
    }

    private Map<String, Object> titleView(Long titleId) {
        var config = gameConfigService.titles().stream()
                .filter(title -> title.titleID() != null && title.titleID().longValue() == titleId)
                .findFirst()
                .orElse(null);
        Map<String, Object> view = new LinkedHashMap<>();
        view.put("titleId", titleId);
        if (config != null) {
            view.put("titleName", config.titleName());
            view.put("typeString", config.typeString());
        }
        return view;
    }

    private static int valueOf(Integer value) {
        return value == null ? 0 : value;
    }

    private boolean refreshGrowthState(Long roleId, PlayerGrowthState growth, PlayerSignInState signIn) {
        boolean updated = false;
        List<PlayerTask> tasks = playerTaskRepository.findByRoleId(roleId);
        long coreBuildingReachedCount = countClaimedCoreGoals(tasks);
        syncGrowthStats(roleId, signIn, coreBuildingReachedCount);

        for (var stage : gameConfigService.growthStages()) {
            if (stage.tasks() == null) {
                continue;
            }

            for (var task : stage.tasks()) {
                if (task == null || task.taskId() == null) {
                    continue;
                }
                if (!growth.getTaskCompleted().contains(task.taskId())
                        && evaluateGrowthTask(task.taskId(), task.description(), tasks, signIn)) {
                    growth.getTaskCompleted().add(task.taskId());
                    updated = true;
                }
            }

            boolean allCompleted = stage.tasks().stream()
                    .allMatch(task -> task != null && task.taskId() != null && growth.getTaskCompleted().contains(task.taskId()));
            if (allCompleted && !growth.getStageCompleted().contains(stage.stageID())) {
                growth.getStageCompleted().add(stage.stageID());
                updated = true;
            }
        }

        return updated;
    }

    private boolean evaluateGrowthTask(String taskId, String description, List<PlayerTask> tasks, PlayerSignInState signIn) {
        if (taskId != null) {
            if (taskId.endsWith("_GT1")) {
                return signIn != null && isChapterClaimed(signIn.getRoleId(), 1);
            }
            if (taskId.endsWith("_GT2")) {
                return countClaimedCoreGoals(tasks) >= 3;
            }
            if (taskId.endsWith("_GT3")) {
                int requiredDays = extractInt(description, 3);
                return signIn != null && signIn.getTotalLoginDays() != null && signIn.getTotalLoginDays() >= requiredDays;
            }
        }

        String text = description == null ? "" : description;

        if (text.contains("报到日")) {
            return signIn != null && isChapterClaimed(signIn.getRoleId(), 1);
        }
        if (text.contains("核心建筑")) {
            return countClaimedCoreGoals(tasks) >= 3;
        }
        if (text.contains("累计登录天数")) {
            int requiredDays = extractInt(text, 3);
            return signIn != null && signIn.getTotalLoginDays() != null && signIn.getTotalLoginDays() >= requiredDays;
        }
        return false;
    }

    private boolean isChapterClaimed(Long roleId, int chapterNo) {
        List<PlayerTask> chapterTasks = playerTaskRepository.findByRoleIdAndChapterNo(roleId, chapterNo);
        return !chapterTasks.isEmpty() && chapterTasks.stream().allMatch(task -> TaskStatus.CLAIMED.equals(task.getTaskStatus()));
    }

    private long countClaimedCoreGoals(List<PlayerTask> tasks) {
        var coreGoalTaskIds = taskConfigRepository.findAllByOrderByChapterNoAscStepNoAsc().stream()
                .filter(task -> Integer.valueOf(1).equals(task.getStatus()))
                .filter(task -> "MAIN".equals(task.getTaskType()))
                .filter(task -> task.getChapterNo() != null && task.getChapterNo() >= 2)
                .filter(task -> "arrive_building".equals(task.getTargetType()))
                .map(task -> task.getTaskId())
                .toList();

        return tasks.stream()
                .filter(task -> TaskStatus.CLAIMED.equals(task.getTaskStatus()))
                .filter(task -> coreGoalTaskIds.contains(task.getTaskId()))
                .count();
    }

    private int extractInt(String text, int defaultValue) {
        StringBuilder digits = new StringBuilder();
        for (char c : text.toCharArray()) {
            if (Character.isDigit(c)) {
                digits.append(c);
            } else if (digits.length() > 0) {
                break;
            }
        }
        if (digits.length() == 0) {
            return defaultValue;
        }
        return Integer.parseInt(digits.toString());
    }

    private int sumRewardCoins(List<GameConfigs.RewardRef> rewards) {
        if (rewards == null) {
            return 0;
        }
        return rewards.stream()
                .filter(reward -> reward != null && reward.rewardId() != null && reward.amount() != null)
                .filter(reward -> reward.rewardId() == 1)
                .mapToInt(GameConfigs.RewardRef::amount)
                .sum();
    }
}
