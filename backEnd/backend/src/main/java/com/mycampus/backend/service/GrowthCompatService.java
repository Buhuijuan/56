package com.mycampus.backend.service;

import com.mycampus.backend.api.dto.TaskDtos;
import com.mycampus.backend.api.dto.growthcompat.*;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.entity.Role;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.RoleRepository;
import com.mycampus.backend.progression.entity.PlayerLevelState;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.task.TaskStatus;
import com.mycampus.backend.task.entity.PlayerTask;
import com.mycampus.backend.task.entity.TaskConfigEntity;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.Comparator;
import java.util.List;

@Service
public class GrowthCompatService {

    private final PlayerService playerService;
    private final PlayerStateInitializer playerStateInitializer;
    private final RoleRepository roleRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerLevelStateRepository levelRepository;
    private final PlayerTaskRepository playerTaskRepository;
    private final TaskConfigRepository taskConfigRepository;
    private final TaskService taskService;
    private final ProgressionService progressionService;
    private final GameConfigService gameConfigService;

    public GrowthCompatService(PlayerService playerService,
                               PlayerStateInitializer playerStateInitializer,
                               RoleRepository roleRepository,
                               PlayerProfileRepository profileRepository,
                               PlayerLevelStateRepository levelRepository,
                               PlayerTaskRepository playerTaskRepository,
                               TaskConfigRepository taskConfigRepository,
                               TaskService taskService,
                               ProgressionService progressionService,
                               GameConfigService gameConfigService) {
        this.playerService = playerService;
        this.playerStateInitializer = playerStateInitializer;
        this.roleRepository = roleRepository;
        this.profileRepository = profileRepository;
        this.levelRepository = levelRepository;
        this.playerTaskRepository = playerTaskRepository;
        this.taskConfigRepository = taskConfigRepository;
        this.taskService = taskService;
        this.progressionService = progressionService;
        this.gameConfigService = gameConfigService;
    }

    @Transactional
    public void initProfile(CurrentAccount principal, InitProfileCompatRequest request) {
        Long roleId = validateRoleAccess(principal, request.getRoleId());
        if (profileRepository.findById(roleId).isEmpty()) {
            playerStateInitializer.initialize(roleId);
        }

        Role role = roleRepository.findById(roleId).orElseThrow();
        if (request.getNickname() != null && !request.getNickname().isBlank()) {
            role.setNickName(request.getNickname().trim());
            roleRepository.save(role);
        }

        PlayerProfile profile = profileRepository.findById(roleId).orElseThrow();
        if (request.getNickname() != null && !request.getNickname().isBlank()) {
            profile.setNickname(request.getNickname().trim());
        }
        profile.setLastLoginAt(LocalDateTime.now());
        profile.touch();
        profileRepository.save(profile);
    }

    @Transactional(readOnly = true)
    public GrowthProfileCompatDto getGrowthProfile(CurrentAccount principal, Long roleId) {
        Long resolvedRoleId = validateRoleAccess(principal, roleId);
        PlayerProfile profile = profileRepository.findById(resolvedRoleId)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "角色资料不存在"));
        PlayerLevelState levelState = levelRepository.findById(resolvedRoleId)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "等级状态不存在"));
        return toGrowthProfile(profile, levelState);
    }

    @Transactional(readOnly = true)
    public GrowthSnapshotCompatDto getGrowthSnapshot(CurrentAccount principal, Long roleId) {
        Long resolvedRoleId = validateRoleAccess(principal, roleId);

        GrowthSnapshotCompatDto snapshot = new GrowthSnapshotCompatDto();
        snapshot.setProfile(getGrowthProfile(principal, resolvedRoleId));
        snapshot.setTasks(playerTaskRepository.findByRoleId(resolvedRoleId).stream()
                .sorted(Comparator.comparing(PlayerTask::getTaskId))
                .map(this::toTaskState)
                .toList());
        return snapshot;
    }

    @Transactional
    public void acceptTask(CurrentAccount principal, AcceptTaskCompatRequest request) {
        validateRoleAccess(principal, request.getRoleId());
        taskService.accept(principal, request.getTaskCode());
    }

    @Transactional
    public void updateTaskProgress(CurrentAccount principal, UpdateTaskProgressCompatRequest request) {
        Long roleId = validateRoleAccess(principal, request.getRoleId());
        String taskCode = requireTaskCode(request.getTaskCode());
        int delta = request.getDelta() == null ? 0 : request.getDelta();
        if (delta <= 0) {
            throw new AppException(HttpStatus.BAD_REQUEST, "任务进度增量必须大于 0");
        }

        TaskConfigEntity config = taskConfigRepository.findByTaskCode(taskCode)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "任务不存在"));
        PlayerTask playerTask = playerTaskRepository.findByRoleIdAndTaskCode(roleId, taskCode)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "玩家任务不存在"));

        if (!TaskStatus.IN_PROGRESS.equals(playerTask.getTaskStatus())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "任务当前不在进行中");
        }

        int current = playerTask.getProgressCurrent() == null ? 0 : playerTask.getProgressCurrent();
        int target = playerTask.getProgressTarget() == null ? 1 : playerTask.getProgressTarget();
        int newProgress = Math.min(target, current + delta);

        playerTask.setProgressCurrent(newProgress);
        if (newProgress >= target) {
            playerTask.setTaskStatus(TaskStatus.COMPLETED);
            if (playerTask.getCompletedAt() == null) {
                playerTask.setCompletedAt(LocalDateTime.now());
            }
        }
        playerTask.touch();
        playerTaskRepository.save(playerTask);

        if (newProgress >= target) {
            taskService.getTasks(principal);
        }
    }

    @Transactional
    public TaskRewardCompatDto claimTaskReward(CurrentAccount principal, ClaimTaskRewardCompatRequest request) {
        Long roleId = validateRoleAccess(principal, request.getRoleId());
        String taskCode = requireTaskCode(request.getTaskCode());

        TaskConfigEntity config = taskConfigRepository.findByTaskCode(taskCode)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "任务不存在"));
        PlayerTask playerTask = playerTaskRepository.findByRoleIdAndTaskCode(roleId, taskCode)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "玩家任务不存在"));

        if (!TaskStatus.COMPLETED.equals(playerTask.getTaskStatus())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "任务当前不可领取");
        }

        PlayerProfile profile = profileRepository.findById(roleId)
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "角色资料不存在"));
        int oldLevel = profile.getLevel() == null ? 1 : profile.getLevel();
        int oldExp = profile.getExp() == null ? 0 : profile.getExp();

        if (config.getRewardExp() != null && config.getRewardExp() > 0) {
            progressionService.grantExp(roleId, "TASK", taskCode, config.getRewardExp());
        }
        if (config.getRewardCoin() != null && config.getRewardCoin() > 0) {
            progressionService.grantCoins(roleId, "TASK", taskCode, config.getRewardCoin());
        }
        if ("bike".equalsIgnoreCase(config.getRewardUnlockFeature())) {
            progressionService.unlockBike(roleId, "TASK", taskCode);
        }

        playerTask.setTaskStatus(TaskStatus.CLAIMED);
        playerTask.setRewardClaimedAt(LocalDateTime.now());
        playerTask.touch();
        playerTaskRepository.save(playerTask);

        taskService.getTasks(principal);

        PlayerProfile latestProfile = profileRepository.findById(roleId).orElseThrow();
        TaskRewardCompatDto result = new TaskRewardCompatDto();
        result.setTaskCode(taskCode);
        result.setTaskName(config.getTaskName());
        result.setRewardExp(config.getRewardExp());
        result.setRewardCoin(config.getRewardCoin());
        result.setUnlockFeature(config.getRewardUnlockFeature());
        result.setOldLevel(oldLevel);
        result.setNewLevel(latestProfile.getLevel());
        result.setOldExp(oldExp);
        result.setNewExp(latestProfile.getExp());
        return result;
    }

    private Long validateRoleAccess(CurrentAccount principal, Long roleId) {
        if (roleId == null || roleId <= 0) {
            throw new AppException(HttpStatus.BAD_REQUEST, "roleId 必须是正整数");
        }
        Long currentRoleId = playerService.currentRole(principal).getId();
        if (!currentRoleId.equals(roleId)) {
            throw new AppException(HttpStatus.FORBIDDEN, "roleId 与当前角色不匹配");
        }
        return currentRoleId;
    }

    private String requireTaskCode(String taskCode) {
        if (taskCode == null || taskCode.isBlank()) {
            throw new AppException(HttpStatus.BAD_REQUEST, "taskCode 不能为空");
        }
        return taskCode.trim();
    }

    private GrowthProfileCompatDto toGrowthProfile(PlayerProfile profile, PlayerLevelState levelState) {
        GrowthProfileCompatDto dto = new GrowthProfileCompatDto();
        dto.setRoleId(profile.getRoleId());
        dto.setNickname(profile.getNickname());
        dto.setLevel(profile.getLevel());
        dto.setExp(profile.getExp());
        dto.setCoin(profile.getCoin());
        dto.setBikeUnlocked(profile.getBikeUnlocked());

        var levels = gameConfigService.levels();
        var current = levels.stream()
                .filter(config -> config.level().equals(profile.getLevel()))
                .findFirst()
                .orElse(null);
        var next = levels.stream()
                .filter(config -> config.level().equals(profile.getLevel() + 1))
                .findFirst()
                .orElse(null);

        dto.setCurrentLevelTotalExp(current == null ? 0 : current.requiredExp());
        if (next == null) {
            dto.setMaxLevel(true);
            dto.setNextLevelNeedExp(0);
            dto.setNextLevelTotalExp(profile.getExp());
            dto.setRemainExpToNextLevel(0);
        } else {
            int remain = Math.max(0, next.requiredExp() - levelState.getExp());
            dto.setMaxLevel(false);
            dto.setNextLevelNeedExp(remain);
            dto.setNextLevelTotalExp(next.requiredExp());
            dto.setRemainExpToNextLevel(remain);
        }
        return dto;
    }

    private TaskStateCompatDto toTaskState(PlayerTask playerTask) {
        TaskConfigEntity config = taskConfigRepository.findById(playerTask.getTaskId())
                .orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "任务配置不存在"));
        TaskStateCompatDto dto = new TaskStateCompatDto();
        dto.setTaskId(playerTask.getTaskId());
        dto.setTaskCode(config.getTaskCode());
        dto.setTaskName(config.getTaskName());
        dto.setTaskStatus(playerTask.getTaskStatus());
        dto.setProgressCurrent(playerTask.getProgressCurrent());
        dto.setProgressTarget(playerTask.getProgressTarget());
        return dto;
    }
}
