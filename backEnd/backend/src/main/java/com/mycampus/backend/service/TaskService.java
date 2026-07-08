package com.mycampus.backend.service;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.mycampus.backend.api.dto.TaskDtos;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.player.entity.PlayerStat;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.task.TaskStatus;
import com.mycampus.backend.task.entity.BuildingLocation;
import com.mycampus.backend.task.entity.PlayerCheckinRecord;
import com.mycampus.backend.task.entity.PlayerTask;
import com.mycampus.backend.task.entity.TaskConfigEntity;
import com.mycampus.backend.task.repository.BuildingLocationRepository;
import com.mycampus.backend.task.repository.PlayerCheckinRecordRepository;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.function.Consumer;
import java.util.function.Predicate;

@Service
public class TaskService {

    private record ChapterMeta(String typeString, String chapterId, String title, String description) {
    }

    private record SpeakerMeta(String speakerType, String speakerName, String avatarKey) {
    }

    private static final BigDecimal NPC_DIALOGUE_RADIUS_METERS = BigDecimal.valueOf(5);
    private static final BigDecimal ARRIVE_BUILDING_RADIUS_METERS = BigDecimal.valueOf(100);

    private static final Map<String, Long> TASK_TITLE_IDS = Map.of(
            "M_1_1", 2L,
            "M_1_3", 3L,
            "M_1_4", 4L,
            "M_2_3", 5L,
            "M_3_4", 6L
    );

    private static final Map<Integer, Long> CHAPTER_TITLE_IDS = Map.of(
            1, 7L,
            2, 8L,
            3, 9L
    );

    private static final Map<Integer, String> CHAPTER_REWARD_TITLE_NAMES = Map.of(
            1, "初入校园",
            2, "校园漫步者",
            3, "教学楼常客"
    );

    private static final Map<Integer, ChapterMeta> CHAPTER_META = Map.of(
            1, new ChapterMeta("主线", "CH01", "初入校园 · 报到日",
                    "在AI伙伴的陪伴下，完成报到手续，找到你的宿舍，并通过体检，正式开启校园生活。"),
            2, new ChapterMeta("主线", "CH02", "校园初识 · 漫游时光",
                    "在AI伙伴的陪伴下，漫步大学校园，探访标志性地标与特色景观，建立空间记忆与归属感。"),
            3, new ChapterMeta("主线", "CH03", "学习体验 · 第一堂课",
                    "在AI伙伴的陪伴下，熟悉教学楼区域各栋建筑外观与功能，完成第一堂课的室外签到与准备流程。"),
            11, new ChapterMeta("支线", "BR01", "校园打卡纪念",
                    "通过拍摄并保存3个不同地标的照片，帮助玩家熟悉校园环境，获得探索成就感。"),
            12, new ChapterMeta("支线", "BR02", "校园单车初体验",
                    "引导玩家寻找并解锁校园单车功能，提升移动效率与探索自由度。")
    );

    private static final Map<Long, SpeakerMeta> NPC_SPEAKERS = Map.of(
            1001L, new SpeakerMeta("NPC", "迎新志愿者", "npc_welcome_volunteer"),
            1002L, new SpeakerMeta("NPC", "迎新接待志愿者", "npc_reception_volunteer"),
            1003L, new SpeakerMeta("NPC", "宿管阿姨", "npc_dorm_manager"),
            1004L, new SpeakerMeta("NPC", "体检引导员", "npc_nurse")
    );

    private static final Map<String, String> PRIMARY_AVATAR_BY_TASK = Map.of(
            "M_1_1", "elf_default",
            "M_1_2", "npc_reception_volunteer",
            "M_1_3", "npc_dorm_manager",
            "M_1_4", "npc_nurse"
    );

    private static final Map<Long, String> TARGET_ANCHOR_KEYS = Map.ofEntries(
            Map.entry(1001L, "loc_welcome_volunteer"),
            Map.entry(1002L, "loc_reception_volunteer"),
            Map.entry(1003L, "loc_dorm_manager"),
            Map.entry(1004L, "loc_nurse"),
            Map.entry(2001L, "loc_reception"),
            Map.entry(2002L, "loc_dorm_bamboo3"),
            Map.entry(2003L, "loc_campus_hospital"),
            Map.entry(2004L, "loc_jin_lake"),
            Map.entry(2005L, "loc_botanical_garden"),
            Map.entry(2006L, "loc_library"),
            Map.entry(2007L, "loc_complex_building"),
            Map.entry(2008L, "loc_teaching_building_1"),
            Map.entry(2009L, "loc_experiment_building_1"),
            Map.entry(2010L, "loc_art_square"),
            Map.entry(2011L, "loc_clock_tower"),
            Map.entry(2012L, "loc_sunset_point"),
            Map.entry(4001L, "loc_bike_station")
    );

    private final PlayerService playerService;
    private final TaskConfigRepository taskConfigRepository;
    private final PlayerTaskRepository playerTaskRepository;
    private final PlayerCheckinRecordRepository playerCheckinRecordRepository;
    private final BuildingLocationRepository buildingLocationRepository;
    private final PlayerStatRepository playerStatRepository;
    private final ProgressionService progressionService;
    private final PlayerStateInitializer playerStateInitializer;
    private final ObjectMapper objectMapper;

    public TaskService(PlayerService playerService,
                       TaskConfigRepository taskConfigRepository,
                       PlayerTaskRepository playerTaskRepository,
                       PlayerCheckinRecordRepository playerCheckinRecordRepository,
                       BuildingLocationRepository buildingLocationRepository,
                       PlayerStatRepository playerStatRepository,
                       ProgressionService progressionService,
                       PlayerStateInitializer playerStateInitializer,
                       ObjectMapper objectMapper) {
        this.playerService = playerService;
        this.taskConfigRepository = taskConfigRepository;
        this.playerTaskRepository = playerTaskRepository;
        this.playerCheckinRecordRepository = playerCheckinRecordRepository;
        this.buildingLocationRepository = buildingLocationRepository;
        this.playerStatRepository = playerStatRepository;
        this.progressionService = progressionService;
        this.playerStateInitializer = playerStateInitializer;
        this.objectMapper = objectMapper;
    }

    @Transactional
    public Map<String, Object> getTasks(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("acceptedTasks", taskViews(context, task -> TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())));
        result.put("availableTasks", taskViews(context, task -> TaskStatus.AVAILABLE.equals(task.getTaskStatus())));
        result.put("completedTasks", taskViews(context, task -> TaskStatus.COMPLETED.equals(task.getTaskStatus())));
        result.put("claimedTasks", taskViews(context, task -> TaskStatus.CLAIMED.equals(task.getTaskStatus())));
        result.put("currentMainTask", currentMainTaskView(context));
        result.put("elfPrompt", currentMainElfPrompt(context));
        result.put("prompts", currentMainPrompts(context));
        result.put("chapters", chapterSummary(context));
        return result;
    }

    @Transactional
    public Map<String, Object> getTaskDetail(CurrentAccount principal, String taskCode) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        PlayerTask playerTask = context.findPlayerTaskByCode(taskCode);
        TaskConfigEntity config = context.findConfigByCode(taskCode);
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("task", taskView(config, playerTask));
        result.put("elfPrompt", buildElfPrompt(config, stageForTask(playerTask)));
        result.put("prompts", buildPrompts(config, stageForTask(playerTask)));
        return result;
    }

    @Transactional
    public Map<String, Object> accept(CurrentAccount principal, String taskCode) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        PlayerTask playerTask = context.findPlayerTaskByCode(taskCode);
        if (!TaskStatus.AVAILABLE.equals(playerTask.getTaskStatus())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该任务当前不可手动接取");
        }
        playerTask.setTaskStatus(TaskStatus.IN_PROGRESS);
        playerTask.setAcceptedAt(LocalDateTime.now());
        playerTask.touch();
        playerTaskRepository.save(playerTask);
        return Map.of("taskCode", taskCode, "accepted", true);
    }

    @Transactional
    public Map<String, Object> progress(CurrentAccount principal, String taskCode, TaskDtos.TaskProgressRequest request) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        PlayerTask playerTask = context.findPlayerTaskByCode(taskCode);
        TaskConfigEntity config = context.findConfigByCode(taskCode);
        TaskDtos.TaskEventRequest event = new TaskDtos.TaskEventRequest(
                request.eventType(),
                request.targetType(),
                request.targetId(),
                request.increment(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                true,
                request.extra()
        );
        return applyEventToTask(roleId, context, config, playerTask, event);
    }

    @Transactional
    public Map<String, Object> claim(CurrentAccount principal, String taskCode) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        PlayerTask playerTask = context.findPlayerTaskByCode(taskCode);
        TaskConfigEntity config = context.findConfigByCode(taskCode);
        if (!TaskStatus.COMPLETED.equals(playerTask.getTaskStatus())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "任务尚未完成");
        }

        Map<String, Object> rewardResult = new LinkedHashMap<>();
        if (config.getRewardExp() > 0) {
            rewardResult.put("exp", progressionService.grantExp(roleId, "TASK", taskCode, config.getRewardExp()));
        }
        if (config.getRewardCoin() > 0) {
            rewardResult.put("coin", progressionService.grantCoins(roleId, "TASK", taskCode, config.getRewardCoin()));
        }
        Long titleId = taskCode == null ? null : TASK_TITLE_IDS.get(taskCode);
        if (titleId != null) {
            rewardResult.put("title", progressionService.unlockTitle(roleId, titleId, "TASK", config.getTaskId()));
        }
        if ("bike".equalsIgnoreCase(config.getRewardUnlockFeature())) {
            rewardResult.put("feature", progressionService.unlockBike(roleId, "TASK", taskCode));
        }

        playerTask.setTaskStatus(TaskStatus.CLAIMED);
        playerTask.setRewardClaimedAt(LocalDateTime.now());
        playerTask.touch();
        playerTaskRepository.save(playerTask);

        refreshAndLoad(roleId);
        return Map.of(
                "taskCode", taskCode,
                "claimed", true,
                "rewards", rewardResult
        );
    }

    @Transactional
    public Map<String, Object> currentMainTask(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("task", currentMainTaskView(context));
        result.put("elfPrompt", currentMainElfPrompt(context));
        result.put("prompts", currentMainPrompts(context));
        return result;
    }

    @Transactional
    public Map<String, Object> chapters(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("chapters", chapterBookViews(refreshAndLoad(roleId)));
        return result;
    }

    @Transactional
    public Map<String, Object> processEvent(CurrentAccount principal, TaskDtos.TaskEventRequest request) {
        Long roleId = playerService.currentRole(principal).getId();
        TaskContext context = refreshAndLoad(roleId);
        List<String> triggeredTasks = autoTriggerTasksFromEvent(context, request);
        List<Map<String, Object>> progressed = new ArrayList<>();

        for (PlayerTask task : context.playerTasks()) {
            TaskConfigEntity config = context.configById().get(task.getTaskId());
            if (!TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())) {
                continue;
            }
            if (!matches(config, request)) {
                continue;
            }
            progressed.add(applyEventToTask(roleId, context, config, task, request));
        }

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("eventType", request.eventType());
        result.put("triggeredTasks", triggeredTasks);
        result.put("progressedTasks", progressed);
        TaskContext refreshed = refreshAndLoad(roleId);
        result.put("currentMainTask", currentMainTaskView(refreshed));
        result.put("elfPrompt", currentMainElfPrompt(refreshed));
        result.put("prompts", currentMainPrompts(refreshed));
        result.put("completedElfPrompt", completedElfPrompt(context, progressed));
        result.put("completedPrompts", completedPrompts(context, progressed));
        if ("BIKE_STATION_VISIT".equals(request.eventType())) {
            result.put("bikeUnlocked", progressionService.unlockBike(roleId, "TASK_EVENT", "BIKE_STATION_VISIT"));
        }
        result.put("behaviorTitleUnlocks", progressionService.refreshBehaviorTitles(roleId));
        return result;
    }

    private Map<String, Object> applyEventToTask(Long roleId,
                                                 TaskContext context,
                                                 TaskConfigEntity config,
                                                 PlayerTask task,
                                                 TaskDtos.TaskEventRequest request) {
        if (!TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "任务当前不在进行中");
        }

        int before = task.getProgressCurrent();
        int increment = Math.max(1, request.increment() == null ? 1 : request.increment());

        TaskDtos.ChapterCompletionDto chapterCompletion = null;

        if ("photo_checkin_distinct_location".equals(config.getTargetType())) {
            PhotoCheckinResult result = recordPhotoCheckin(roleId, config, request);
            if (!result.success()) {
                throw new AppException(HttpStatus.BAD_REQUEST,
                        "当前未命中有效地标，距离 %.2f 米，需进入 %.2f 米范围内。"
                                .formatted(result.distanceToTarget().doubleValue(), result.radius().doubleValue()));
            }
            increment = (int) playerCheckinRecordRepository.countDistinctSuccessfulPhotoCheckinsForTask(roleId, config.getTaskId()) - before;
            increment = Math.max(0, increment);
            updatePhotoStats(roleId, config.getTaskId());
        } else if ("bike_trial_distance".equals(config.getTargetType())) {
            increment = Math.max(0, request.increment() == null ? 0 : request.increment());
            if (increment > 0) {
                updateStat(roleId, stat -> stat.setBikeRideCount(stat.getBikeRideCount() + 1));
            }
        } else if ("ai_dialogue".equals(config.getTargetType())) {
            updateStat(roleId, stat -> stat.setElfAskCount(stat.getElfAskCount() + 1));
        }

        int after = Math.min(task.getProgressTarget(), before + increment);
        task.setProgressCurrent(after);
        if (after >= task.getProgressTarget()) {
            task.setTaskStatus(TaskStatus.COMPLETED);
            if (task.getCompletedAt() == null) {
                task.setCompletedAt(LocalDateTime.now());
            }
            chapterCompletion = handleTaskCompleted(roleId, context, config);
        }
        task.touch();
        playerTaskRepository.save(task);
        refreshAutoUnlocks(context.roleId());
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("taskCode", config.getTaskCode());
        result.put("progressBefore", before);
        result.put("progressAfter", after);
        result.put("completed", TaskStatus.COMPLETED.equals(task.getTaskStatus()));
        if (chapterCompletion != null) {
            result.put("chapterCompletion", chapterCompletion);
        }
        return result;
    }

    private TaskDtos.ChapterCompletionDto handleTaskCompleted(Long roleId, TaskContext context, TaskConfigEntity config) {
        syncCoreBuildingReachedCount(roleId, context, config);
        Integer chapterNo = config.getChapterNo();
        if (chapterNo == null) {
            return null;
        }
        if (chapterNo >= 10) {
            return null;
        }
        List<PlayerTask> chapterTasks = context.playerTasksByChapter(chapterNo);
        boolean chapterCompleted = chapterTasks.stream().allMatch(task ->
                TaskStatus.COMPLETED.equals(task.getTaskStatus()) || TaskStatus.CLAIMED.equals(task.getTaskStatus()));
        if (chapterCompleted) {
            Long titleId = CHAPTER_TITLE_IDS.get(chapterNo);
            if (titleId != null) {
                progressionService.unlockTitle(roleId, titleId, "CHAPTER", chapterNo.longValue());
            }
            ChapterMeta meta = chapterNo == null ? null : CHAPTER_META.get(chapterNo);
            return new TaskDtos.ChapterCompletionDto(
                    chapterNo,
                    meta == null ? null : meta.title(),
                    titleId,
                    CHAPTER_REWARD_TITLE_NAMES.get(chapterNo)
            );
        }
        return null;
    }

    private PhotoCheckinResult recordPhotoCheckin(Long roleId, TaskConfigEntity config, TaskDtos.TaskEventRequest request) {
        if (request.targetId() == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "拍照打卡事件缺少目标地标 ID");
        }
        BuildingLocation location = buildingLocationRepository.findById(request.targetId())
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "目标地标不存在"));

        BigDecimal currentPosX = defaultDecimal(request.currentPosX());
        BigDecimal currentPosY = defaultDecimal(request.currentPosY());
        BigDecimal currentPosZ = defaultDecimal(request.currentPosZ());
        BigDecimal distance = hasTargetAnchorPosition(request)
                ? calculateDistance(currentPosX, currentPosY, currentPosZ,
                request.targetPosX(), request.targetPosY(), request.targetPosZ())
                : calculateDistance(currentPosX, currentPosY, currentPosZ, location);
        BigDecimal radius = defaultDecimal(location.getCheckinRadius());
        boolean success = distance.compareTo(radius) <= 0;

        if (success && playerCheckinRecordRepository.existsByRoleIdAndTaskIdAndBuildingLocationIdAndTriggerTypeAndIsSuccess(
                roleId, config.getTaskId(), location.getBuildingLocationId(), "photo_checkin", 1)) {
            return new PhotoCheckinResult(true, distance, radius, true);
        }

        PlayerCheckinRecord record = new PlayerCheckinRecord();
        record.setRoleId(roleId);
        record.setTaskId(config.getTaskId());
        record.setBuildingLocationId(location.getBuildingLocationId());
        record.setCurrentPosX(currentPosX);
        record.setCurrentPosY(currentPosY);
        record.setCurrentPosZ(currentPosZ);
        record.setDistanceToTarget(distance.setScale(2, RoundingMode.HALF_UP));
        record.setIsSuccess(success ? 1 : 0);
        record.setTriggerType("photo_checkin");
        record.setCheckedAt(LocalDateTime.now());
        playerCheckinRecordRepository.save(record);

        return new PhotoCheckinResult(success, distance, radius, false);
    }

    private BigDecimal calculateDistance(BigDecimal currentPosX,
                                         BigDecimal currentPosY,
                                         BigDecimal currentPosZ,
                                         BuildingLocation location) {
        return calculateDistance(
                currentPosX,
                currentPosY,
                currentPosZ,
                defaultDecimal(location.getPosX()),
                defaultDecimal(location.getPosY()),
                defaultDecimal(location.getPosZ()));
    }

    private BigDecimal calculateDistance(BigDecimal currentPosX,
                                         BigDecimal currentPosY,
                                         BigDecimal currentPosZ,
                                         BigDecimal targetPosX,
                                         BigDecimal targetPosY,
                                         BigDecimal targetPosZ) {
        double dx = currentPosX.subtract(targetPosX).doubleValue();
        double dy = currentPosY.subtract(targetPosY).doubleValue();
        double dz = currentPosZ.subtract(targetPosZ).doubleValue();
        return BigDecimal.valueOf(Math.sqrt(dx * dx + dy * dy + dz * dz)).setScale(2, RoundingMode.HALF_UP);
    }

    private BigDecimal defaultDecimal(BigDecimal value) {
        return value == null ? BigDecimal.ZERO : value;
    }

    private boolean hasTargetAnchorPosition(TaskDtos.TaskEventRequest request) {
        return request.targetPosX() != null
                && request.targetPosY() != null
                && request.targetPosZ() != null;
    }

    private void updatePhotoStats(Long roleId, Long taskId) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElseThrow();
        int distinctCount = (int) playerCheckinRecordRepository.countDistinctSuccessfulPhotoCheckinsForTask(roleId, taskId);
        stat.setPhotoCount(Math.max(stat.getPhotoCount() + 1, distinctCount));
        stat.setDistinctPhotoLocationCount(distinctCount);
        stat.touch();
        playerStatRepository.save(stat);
    }

    private void syncCoreBuildingReachedCount(Long roleId, TaskContext context, TaskConfigEntity currentTaskConfig) {
        if (!isCoreBuildingTask(currentTaskConfig)) {
            return;
        }
        int reachedCount = (int) context.sortedConfigs().stream()
                .filter(this::isCoreBuildingTask)
                .map(TaskConfigEntity::getTaskId)
                .filter(taskId -> {
                    PlayerTask playerTask = context.playerTaskById().get(taskId);
                    return playerTask != null && (TaskStatus.COMPLETED.equals(playerTask.getTaskStatus())
                            || TaskStatus.CLAIMED.equals(playerTask.getTaskStatus())
                            || taskId.equals(currentTaskConfig.getTaskId()));
                })
                .count();
        updateStat(roleId, stat -> stat.setCoreBuildingReachedCount(reachedCount));
    }

    private boolean isCoreBuildingTask(TaskConfigEntity config) {
        return config != null
                && Integer.valueOf(1).equals(config.getStatus())
                && "MAIN".equals(config.getTaskType())
                && config.getChapterNo() != null
                && config.getChapterNo() >= 2
                && "arrive_building".equals(config.getTargetType());
    }

    private void updateStat(Long roleId, Consumer<PlayerStat> consumer) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElseThrow();
        consumer.accept(stat);
        stat.touch();
        playerStatRepository.save(stat);
    }

    private List<String> autoTriggerTasksFromEvent(TaskContext context, TaskDtos.TaskEventRequest request) {
        List<String> triggered = new ArrayList<>();
        if ("LANDMARK_VISIT".equals(request.eventType())) {
            PlayerTask branch = context.findOptionalPlayerTaskByCode("B_1_1").orElse(null);
            if (branch != null && TaskStatus.LOCKED.equals(branch.getTaskStatus()) && prerequisiteClaimed(context, "M_1_3")) {
                activateTask(branch, false);
                playerTaskRepository.save(branch);
                triggered.add("B_1_1");
            }
        }
        if ("BIKE_STATION_VISIT".equals(request.eventType())) {
            PlayerTask branch = context.findOptionalPlayerTaskByCode("B_2_1").orElse(null);
            if (branch != null && TaskStatus.LOCKED.equals(branch.getTaskStatus()) && prerequisiteClaimed(context, "M_1_1")) {
                activateTask(branch, false);
                playerTaskRepository.save(branch);
                triggered.add("B_2_1");
            }
        }
        return triggered;
    }

    private boolean matches(TaskConfigEntity config, TaskDtos.TaskEventRequest request) {
        return switch (config.getTargetType()) {
            case "ai_dialogue" -> "AI_DIALOGUE".equals(request.eventType());
            case "arrive_building" -> ("ARRIVE_BUILDING".equals(request.eventType()) || "LANDMARK_VISIT".equals(request.eventType()))
                    && Objects.equals(config.getTargetId(), request.targetId())
                    && isWithinBuildingRange(request.targetId(), request, ARRIVE_BUILDING_RADIUS_METERS);
            case "npc_dialogue" -> "NPC_DIALOGUE".equals(request.eventType())
                    && Objects.equals(config.getTargetId(), request.targetId())
                    && isWithinNpcRange(request, radiusMetersForNpcDialogue(config));
            case "photo_checkin_distinct_location" -> "PHOTO_CHECKIN".equals(request.eventType());
            case "bike_trial_distance" -> "BIKE_TRIAL_DISTANCE".equals(request.eventType());
            default -> false;
        };
    }

    private boolean isWithinBuildingRange(Long targetId, TaskDtos.TaskEventRequest request, BigDecimal radiusMeters) {
        if (targetId == null || request.currentPosX() == null || request.currentPosY() == null || request.currentPosZ() == null) {
            return false;
        }
        if (request.targetPosX() == null || request.targetPosY() == null || request.targetPosZ() == null) {
            return false;
        }
        BigDecimal distance = calculateDistance(
                request.currentPosX(), request.currentPosY(), request.currentPosZ(),
                request.targetPosX(), request.targetPosY(), request.targetPosZ());
        return distance.compareTo(radiusMeters) <= 0;
    }

    private boolean isWithinNpcRange(TaskDtos.TaskEventRequest request, BigDecimal radiusMeters) {
        if (request.currentPosX() == null || request.currentPosY() == null || request.currentPosZ() == null) {
            return false;
        }
        if (request.targetPosX() == null || request.targetPosY() == null || request.targetPosZ() == null) {
            return false;
        }
        BigDecimal distance = calculateDistance(
                request.currentPosX(), request.currentPosY(), request.currentPosZ(),
                request.targetPosX(), request.targetPosY(), request.targetPosZ());
        return distance.compareTo(radiusMeters) <= 0;
    }

    private BigDecimal radiusMetersForNpcDialogue(TaskConfigEntity config) {
        return NPC_DIALOGUE_RADIUS_METERS;
    }

    private String anchorKeyFor(TaskConfigEntity config) {
        if (config == null || config.getTargetId() == null) {
            return null;
        }
        return TARGET_ANCHOR_KEYS.get(config.getTargetId());
    }

    private TaskContext refreshAndLoad(Long roleId) {
        playerStateInitializer.ensureInitialized(roleId);
        refreshAutoUnlocks(roleId);
        return loadContext(roleId);
    }

    private void refreshAutoUnlocks(Long roleId) {
        TaskContext context = loadContext(roleId);
        boolean changed = false;
        for (TaskConfigEntity config : context.sortedConfigs()) {
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null || task.getTaskStatus() == null) {
                continue;
            }
            if (!TaskStatus.LOCKED.equals(task.getTaskStatus())) {
                continue;
            }
            if (!prerequisiteSatisfied(context, config)) {
                continue;
            }
            String trigger = config.getTriggerType();
            if ("auto_after_role_created".equals(trigger) && "M_1_1".equals(config.getTaskCode())) {
                activateTask(task, true);
                changed = true;
            } else if ("auto_after_task_completed".equals(trigger) || "auto_after_role_created".equals(trigger)) {
                changed = true;
                activateTask(task, false);
            } else if ("manual_after_chapter_completed".equals(trigger)) {
                activateTask(task, false);
                changed = true;
            }
        }
        if (changed) {
            playerTaskRepository.saveAll(context.playerTasks());
        }
    }

    private boolean prerequisiteSatisfied(TaskContext context, TaskConfigEntity config) {
        if (config.getPreTaskCode() == null || config.getPreTaskCode().isBlank()) {
            return true;
        }
        PlayerTask prerequisiteTask = context.findOptionalPlayerTaskByCode(config.getPreTaskCode()).orElse(null);
        if (prerequisiteTask == null) {
            return false;
        }
        if ("manual_after_chapter_completed".equals(config.getTriggerType())) {
            return TaskStatus.COMPLETED.equals(prerequisiteTask.getTaskStatus())
                    || TaskStatus.CLAIMED.equals(prerequisiteTask.getTaskStatus());
        }
        return TaskStatus.CLAIMED.equals(prerequisiteTask.getTaskStatus())
                || TaskStatus.COMPLETED.equals(prerequisiteTask.getTaskStatus());
    }

    private boolean prerequisiteClaimed(TaskContext context, String taskCode) {
        PlayerTask task = context.findOptionalPlayerTaskByCode(taskCode).orElse(null);
        return task != null && (TaskStatus.CLAIMED.equals(task.getTaskStatus()) || TaskStatus.COMPLETED.equals(task.getTaskStatus()));
    }

    private void activateTask(PlayerTask task, boolean autoAccepted) {
        task.setTaskStatus(autoAccepted ? TaskStatus.IN_PROGRESS : TaskStatus.AVAILABLE);
        if (task.getAcceptedAt() == null && autoAccepted) {
            task.setAcceptedAt(LocalDateTime.now());
        }
        task.touch();
    }

    private TaskContext loadContext(Long roleId) {
        List<TaskConfigEntity> configs = taskConfigRepository.findAllByOrderByChapterNoAscStepNoAsc();
        List<PlayerTask> tasks = playerTaskRepository.findByRoleId(roleId);
        return new TaskContext(roleId, configs, tasks);
    }

    private List<Map<String, Object>> taskViews(TaskContext context, Predicate<PlayerTask> predicate) {
        List<Map<String, Object>> result = new ArrayList<>();
        for (TaskConfigEntity config : context.sortedConfigs()) {
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null) {
                continue;
            }
            if (predicate.test(task)) {
                result.add(taskView(config, task));
            }
        }
        return result;
    }

    private TaskDtos.ElfPromptDto currentMainElfPrompt(TaskContext context) {
        for (TaskConfigEntity config : context.sortedConfigs()) {
            if (!"MAIN".equals(config.getTaskType())) {
                continue;
            }
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null || task.getTaskStatus() == null) {
                continue;
            }
            if (TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())) {
                return buildElfPrompt(config, stageForTask(task));
            }
        }
        return null;
    }

    private List<TaskDtos.PromptMessageDto> currentMainPrompts(TaskContext context) {
        for (TaskConfigEntity config : context.sortedConfigs()) {
            if (!"MAIN".equals(config.getTaskType())) {
                continue;
            }
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null || task.getTaskStatus() == null) {
                continue;
            }
            if (TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())) {
                return buildPrompts(config, stageForTask(task));
            }
        }
        return List.of();
    }

    private Map<String, Object> currentMainTaskView(TaskContext context) {
        for (TaskConfigEntity config : context.sortedConfigs()) {
            if (!"MAIN".equals(config.getTaskType())) {
                continue;
            }
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null || task.getTaskStatus() == null) {
                continue;
            }
            if (TaskStatus.IN_PROGRESS.equals(task.getTaskStatus())) {
                return taskView(config, task);
            }
        }
        return null;
    }

    private TaskDtos.ElfPromptDto completedElfPrompt(TaskContext context, List<Map<String, Object>> progressed) {
        for (Map<String, Object> item : progressed) {
            if (!Boolean.TRUE.equals(item.get("completed"))) {
                continue;
            }
            Object taskCodeValue = item.get("taskCode");
            if (!(taskCodeValue instanceof String taskCode)) {
                continue;
            }
            return buildElfPrompt(context.findConfigByCode(taskCode), "ON_TASK_COMPLETE");
        }
        return null;
    }

    private List<TaskDtos.PromptMessageDto> completedPrompts(TaskContext context, List<Map<String, Object>> progressed) {
        for (Map<String, Object> item : progressed) {
            if (!Boolean.TRUE.equals(item.get("completed"))) {
                continue;
            }
            Object taskCodeValue = item.get("taskCode");
            if (taskCodeValue instanceof String taskCode) {
                return buildPrompts(context.findConfigByCode(taskCode), "ON_TASK_COMPLETE");
            }
        }
        return List.of();
    }

    private List<Map<String, Object>> chapterSummary(TaskContext context) {
        Map<Integer, List<PlayerTask>> byChapter = new LinkedHashMap<>();
        for (TaskConfigEntity config : context.sortedConfigs()) {
            if (!"MAIN".equals(config.getTaskType())) {
                continue;
            }
            PlayerTask task = context.playerTaskById().get(config.getTaskId());
            if (task == null || task.getTaskStatus() == null) {
                continue;
            }
            byChapter.computeIfAbsent(config.getChapterNo(), ignored -> new ArrayList<>())
                    .add(task);
        }
        List<Map<String, Object>> summary = new ArrayList<>();
        for (Map.Entry<Integer, List<PlayerTask>> entry : byChapter.entrySet()) {
            long claimed = entry.getValue().stream().filter(task -> TaskStatus.CLAIMED.equals(task.getTaskStatus())).count();
            long completed = entry.getValue().stream().filter(task ->
                    TaskStatus.COMPLETED.equals(task.getTaskStatus()) || TaskStatus.CLAIMED.equals(task.getTaskStatus())).count();
            summary.add(Map.of(
                    "chapterNo", entry.getKey(),
                    "taskCount", entry.getValue().size(),
                    "completedCount", completed,
                    "claimedCount", claimed,
                    "chapterCompleted", completed == entry.getValue().size()
            ));
        }
        return summary;
    }

    private List<Map<String, Object>> chapterBookViews(TaskContext context) {
        Map<Integer, List<TaskConfigEntity>> configsByChapter = new LinkedHashMap<>();
        for (TaskConfigEntity config : context.sortedConfigs()) {
            configsByChapter.computeIfAbsent(config.getChapterNo(), ignored -> new ArrayList<>()).add(config);
        }

        List<Map<String, Object>> chapters = new ArrayList<>();
        for (Map.Entry<Integer, List<TaskConfigEntity>> entry : configsByChapter.entrySet()) {
            Integer chapterNo = entry.getKey();
            ChapterMeta meta = chapterNo == null ? null : CHAPTER_META.get(chapterNo);
            if (meta == null) {
                continue;
            }

            List<Map<String, Object>> goals = new ArrayList<>();
            int index = 1;
            for (TaskConfigEntity config : entry.getValue()) {
                PlayerTask task = context.playerTaskById().get(config.getTaskId());
                if (task == null) {
                    continue;
                }
                Map<String, Object> goal = new LinkedHashMap<>();
                goal.put("goalId", meta.chapterId() + "_G" + String.format("%02d", index++));
                goal.put("description", config.getTaskName());
                goal.put("taskCode", config.getTaskCode());
                goal.put("targetType", config.getTargetType());
                goal.put("targetId", config.getTargetId());
                goal.put("targetAnchorKey", anchorKeyFor(config));
                goal.put("status", task.getTaskStatus());
                goal.put("completed", TaskStatus.COMPLETED.equals(task.getTaskStatus()) || TaskStatus.CLAIMED.equals(task.getTaskStatus()));
                goal.put("current", TaskStatus.IN_PROGRESS.equals(task.getTaskStatus()));
                goals.add(goal);
            }

            Map<String, Object> chapter = new LinkedHashMap<>();
            chapter.put("typeString", meta.typeString());
            chapter.put("taskChapterID", meta.chapterId());
            chapter.put("taskChapterTitle", meta.title());
            chapter.put("taskChapterDescription", meta.description());
            chapter.put("goals", goals);
            chapters.add(chapter);
        }
        return chapters;
    }

    private Map<String, Object> taskView(TaskConfigEntity config, PlayerTask task) {
        Map<String, Object> view = new LinkedHashMap<>();
        view.put("taskId", config.getTaskId());
        view.put("taskCode", config.getTaskCode());
        view.put("taskName", config.getTaskName());
        view.put("taskType", config.getTaskType());
        view.put("chapterNo", config.getChapterNo());
        view.put("stepNo", config.getStepNo());
        view.put("category", config.getCategory());
        view.put("triggerType", config.getTriggerType());
        view.put("targetType", config.getTargetType());
        view.put("targetId", config.getTargetId());
        view.put("targetAnchorKey", anchorKeyFor(config));
        view.put("targetCount", config.getTargetCount());
        view.put("rewardExp", config.getRewardExp());
        view.put("rewardCoin", config.getRewardCoin());
        view.put("rewardUnlockFeature", config.getRewardUnlockFeature());
        view.put("description", config.getDescription());
        view.put("status", task.getTaskStatus());
        view.put("progressCurrent", task.getProgressCurrent());
        view.put("progressTarget", task.getProgressTarget());
        view.put("acceptedAt", task.getAcceptedAt());
        view.put("completedAt", task.getCompletedAt());
        view.put("rewardClaimedAt", task.getRewardClaimedAt());
        view.put("elfPrompt", buildElfPrompt(config, stageForTask(task)));
        view.put("prompts", buildPrompts(config, stageForTask(task)));
        return view;
    }

    private String stageForTask(PlayerTask task) {
        if (task == null) {
            return "ON_TASK_START";
        }
        if (TaskStatus.COMPLETED.equals(task.getTaskStatus()) || TaskStatus.CLAIMED.equals(task.getTaskStatus())) {
            return "ON_TASK_COMPLETE";
        }
        if (task.getProgressCurrent() != null && task.getProgressCurrent() > 0) {
            return "ON_TASK_PROGRESS";
        }
        return "ON_TASK_START";
    }

    private TaskDtos.ElfPromptDto buildElfPrompt(TaskConfigEntity config, String stage) {
        if (config == null) {
            return null;
        }

        String promptJson = switch (stage) {
            case "ON_TASK_COMPLETE" -> firstNonBlank(config.getElfCompletePromptJson(), config.getElfProgressPromptJson(), config.getElfStartPromptJson());
            case "ON_TASK_PROGRESS" -> firstNonBlank(config.getElfProgressPromptJson(), config.getElfStartPromptJson(), config.getElfCompletePromptJson());
            default -> firstNonBlank(config.getElfStartPromptJson(), config.getElfProgressPromptJson(), config.getElfCompletePromptJson());
        };

        List<String> contents = parsePromptContents(promptJson, config.getDescription());
        if (contents.isEmpty()) {
            return null;
        }

        return new TaskDtos.ElfPromptDto(
                firstNonBlank(config.getElfNpcName(), "AI小精灵"),
                firstNonBlank(config.getTaskCode() == null ? null : PRIMARY_AVATAR_BY_TASK.get(config.getTaskCode()), config.getElfAvatarKey(), "elf_default"),
                stage,
                titleForStage(config, stage),
                contents,
                true,
                config.getTaskCode()
        );
    }

    private List<TaskDtos.PromptMessageDto> buildPrompts(TaskConfigEntity config, String stage) {
        TaskDtos.ElfPromptDto primary = buildElfPrompt(config, stage);
        if (primary == null) {
            return List.of();
        }

        List<TaskDtos.PromptMessageDto> prompts = new ArrayList<>();
        prompts.add(new TaskDtos.PromptMessageDto(
                "AI",
                primary.npcName(),
                firstNonBlank(config.getElfAvatarKey(), "elf_default"),
                primary.stage(),
                primary.contents(),
                primary.autoPopup(),
                primary.taskCode()
        ));

        SpeakerMeta speaker = config.getTargetId() == null ? null : NPC_SPEAKERS.get(config.getTargetId());
        if (speaker != null && ("npc_dialogue".equals(config.getTargetType()) || "M_1_2".equals(config.getTaskCode()))) {
            List<String> contents = npcPromptContents(config, stage, speaker);
            if (!contents.isEmpty()) {
                prompts.add(new TaskDtos.PromptMessageDto(
                        speaker.speakerType(),
                        speaker.speakerName(),
                        speaker.avatarKey(),
                        stage,
                        contents,
                        true,
                        config.getTaskCode()
                ));
            }
        }

        return prompts;
    }

    private List<String> npcPromptContents(TaskConfigEntity config, String stage, SpeakerMeta speaker) {
        return switch (config.getTaskCode()) {
            case "M_1_1" -> List.of("欢迎新同学！看到你身边那位发光的小家伙了吗？那是你的AI伙伴，遇到问题随时都可以问它。");
            case "M_1_2" -> "ON_TASK_COMPLETE".equals(stage)
                    ? List.of("欢迎加入大数据与软件学院！这是你的新生资料包。", "你的宿舍在竹苑3号楼205室，接下来去宿舍安顿吧。")
                    : List.of("你好！是来报到的新同学吗？请确认一下你的录取信息。");
            case "M_1_3" -> "ON_TASK_COMPLETE".equals(stage)
                    ? List.of("门禁激活完成啦。", "晚上11点后进出记得登记，有事随时来找我。")
                    : List.of("新来的同学吧？你的房间是205室。", "来，在门禁机上刷一下校园卡，以后就能自由进出啦。");
            case "M_1_4" -> "ON_TASK_COMPLETE".equals(stage)
                    ? List.of("检查完成，各项指标都很健康。", "这是你的体检合格证明和新生健康指南，请收好。")
                    : List.of("同学你好，是来参加新生体检的吗？", "请出示你的校园卡，验证通过后就能进入体检通道。");
            default -> List.of("靠近 " + speaker.speakerName() + " 后即可触发对话与任务推进。");
        };
    }

    private String titleForStage(TaskConfigEntity config, String stage) {
        return switch (stage) {
            case "ON_TASK_COMPLETE" -> config.getTaskName() + " 已完成";
            case "ON_TASK_PROGRESS" -> config.getTaskName() + " 进行中";
            default -> config.getTaskName();
        };
    }

    private List<String> parsePromptContents(String promptJson, String fallback) {
        if (promptJson != null && !promptJson.isBlank()) {
            try {
                return objectMapper.readValue(promptJson, new TypeReference<List<String>>() {
                });
            } catch (Exception ignored) {
            }
        }
        if (fallback == null || fallback.isBlank()) {
            return List.of();
        }
        return List.of(fallback);
    }

    private String firstNonBlank(String... values) {
        for (String value : values) {
            if (value != null && !value.isBlank()) {
                return value;
            }
        }
        return null;
    }

    private record TaskContext(Long roleId,
                               List<TaskConfigEntity> sortedConfigs,
                               List<PlayerTask> playerTasks) {
        Map<Long, TaskConfigEntity> configById() {
            Map<Long, TaskConfigEntity> map = new HashMap<>();
            for (TaskConfigEntity config : sortedConfigs) {
                map.put(config.getTaskId(), config);
            }
            return map;
        }

        Map<Long, PlayerTask> playerTaskById() {
            Map<Long, PlayerTask> map = new HashMap<>();
            for (PlayerTask task : playerTasks) {
                map.put(task.getTaskId(), task);
            }
            return map;
        }

        TaskConfigEntity findConfigByCode(String taskCode) {
            return sortedConfigs.stream()
                    .filter(config -> config.getTaskCode().equals(taskCode))
                    .findFirst()
                    .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "任务不存在"));
        }

        PlayerTask findPlayerTaskByCode(String taskCode) {
            return findOptionalPlayerTaskByCode(taskCode)
                    .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "玩家任务不存在"));
        }

        Optional<PlayerTask> findOptionalPlayerTaskByCode(String taskCode) {
            Long taskId = sortedConfigs.stream()
                    .filter(config -> config.getTaskCode().equals(taskCode))
                    .map(TaskConfigEntity::getTaskId)
                    .findFirst()
                    .orElse(null);
            if (taskId == null) {
                return Optional.empty();
            }
            return playerTasks.stream().filter(task -> task.getTaskId().equals(taskId)).findFirst();
        }

        List<PlayerTask> playerTasksByChapter(Integer chapterNo) {
            Set<Long> taskIds = new LinkedHashSet<>();
            for (TaskConfigEntity config : sortedConfigs) {
                if (Objects.equals(config.getChapterNo(), chapterNo) && "MAIN".equals(config.getTaskType())) {
                    taskIds.add(config.getTaskId());
                }
            }
            List<PlayerTask> tasks = new ArrayList<>();
            for (PlayerTask task : playerTasks) {
                if (taskIds.contains(task.getTaskId())) {
                    tasks.add(task);
                }
            }
            return tasks;
        }
    }

    private record PhotoCheckinResult(boolean success,
                                      BigDecimal distanceToTarget,
                                      BigDecimal radius,
                                      boolean duplicateSuccess) {
    }
}
