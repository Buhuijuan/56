package com.mycampus.backend.service;

import com.mycampus.backend.activity.entity.PlayerClockInState;
import com.mycampus.backend.activity.entity.PlayerQuizState;
import com.mycampus.backend.activity.entity.PlayerStoryState;
import com.mycampus.backend.activity.repository.PlayerClockInStateRepository;
import com.mycampus.backend.activity.repository.PlayerQuizStateRepository;
import com.mycampus.backend.activity.repository.PlayerStoryStateRepository;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.player.entity.PlayerProfile;
import com.mycampus.backend.player.entity.PlayerStat;
import com.mycampus.backend.player.entity.PlayerTitle;
import com.mycampus.backend.player.entity.Role;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.player.repository.PlayerTitleRepository;
import com.mycampus.backend.player.repository.RoleRepository;
import com.mycampus.backend.progression.entity.PlayerGrowthState;
import com.mycampus.backend.progression.entity.PlayerLevelState;
import com.mycampus.backend.progression.entity.PlayerTitleState;
import com.mycampus.backend.progression.repository.PlayerGrowthStateRepository;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.signin.entity.PlayerSignInState;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import com.mycampus.backend.task.TaskStatus;
import com.mycampus.backend.task.entity.PlayerTask;
import com.mycampus.backend.task.entity.TaskConfigEntity;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import org.springframework.stereotype.Component;

import java.util.LinkedHashMap;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Map;

@Component
public class PlayerStateInitializer {

    private final RoleRepository roleRepository;
    private final PlayerLevelStateRepository levelRepository;
    private final PlayerGrowthStateRepository growthRepository;
    private final PlayerTitleStateRepository titleRepository;
    private final PlayerProfileRepository profileRepository;
    private final PlayerStatRepository statRepository;
    private final PlayerTitleRepository playerTitleRepository;
    private final PlayerSignInStateRepository signInRepository;
    private final PlayerTaskRepository playerTaskRepository;
    private final TaskConfigRepository taskConfigRepository;
    private final PlayerQuizStateRepository quizRepository;
    private final PlayerClockInStateRepository clockInRepository;
    private final PlayerStoryStateRepository storyRepository;
    private final GameConfigService gameConfigService;

    public PlayerStateInitializer(RoleRepository roleRepository,
                                  PlayerLevelStateRepository levelRepository,
                                  PlayerGrowthStateRepository growthRepository,
                                  PlayerTitleStateRepository titleRepository,
                                  PlayerProfileRepository profileRepository,
                                  PlayerStatRepository statRepository,
                                  PlayerTitleRepository playerTitleRepository,
                                  PlayerSignInStateRepository signInRepository,
                                  PlayerTaskRepository playerTaskRepository,
                                  TaskConfigRepository taskConfigRepository,
                                  PlayerQuizStateRepository quizRepository,
                                  PlayerClockInStateRepository clockInRepository,
                                  PlayerStoryStateRepository storyRepository,
                                  GameConfigService gameConfigService) {
        this.roleRepository = roleRepository;
        this.levelRepository = levelRepository;
        this.growthRepository = growthRepository;
        this.titleRepository = titleRepository;
        this.profileRepository = profileRepository;
        this.statRepository = statRepository;
        this.playerTitleRepository = playerTitleRepository;
        this.signInRepository = signInRepository;
        this.playerTaskRepository = playerTaskRepository;
        this.taskConfigRepository = taskConfigRepository;
        this.quizRepository = quizRepository;
        this.clockInRepository = clockInRepository;
        this.storyRepository = storyRepository;
        this.gameConfigService = gameConfigService;
    }

    public void initialize(Long roleId) {
        Role role = roleRepository.findById(roleId).orElseThrow();

        ensureInitialized(roleId, role);
    }

    public void ensureInitialized(Long roleId) {
        Role role = roleRepository.findById(roleId).orElseThrow();
        ensureInitialized(roleId, role);
    }

    private void ensureInitialized(Long roleId, Role role) {
        if (!levelRepository.existsById(roleId)) {
            PlayerLevelState level = new PlayerLevelState();
            level.setRoleId(roleId);
            level.setLevel(1);
            level.setExp(0);
            levelRepository.save(level);
        }

        if (!growthRepository.existsById(roleId)) {
            PlayerGrowthState growth = new PlayerGrowthState();
            growth.setRoleId(roleId);
            growthRepository.save(growth);
        }

        if (!titleRepository.existsById(roleId)) {
            PlayerTitleState title = new PlayerTitleState();
            title.setRoleId(roleId);
            title.getUnlockedTitleIds().add(1);
            title.setEquippedTitleId(1);
            titleRepository.save(title);
        }

        if (!profileRepository.existsById(roleId)) {
            PlayerProfile profile = new PlayerProfile();
            profile.setRoleId(roleId);
            profile.setNickname(role.getNickName());
            profile.setLevel(1);
            profile.setExp(0);
            profile.setCoin(0);
            profile.setCurrentTitleId(1L);
            profile.setBikeUnlocked(0);
            profile.setFirstLoginAt(LocalDateTime.now());
            profile.setLastLoginAt(LocalDateTime.now());
            profile.touch();
            profileRepository.save(profile);
        }

        if (!statRepository.existsById(roleId)) {
            PlayerStat stat = new PlayerStat();
            stat.setRoleId(roleId);
            stat.setNpcDistinctTalkCount(0);
            stat.setAnimalInteractCount(0);
            stat.setElfAskCount(0);
            stat.setPhotoCount(0);
            stat.setDistinctPhotoLocationCount(0);
            stat.setBikeRideCount(0);
            stat.setQuizCorrectCount(0);
            stat.setMorningCheckinDays(0);
            stat.setStoryCompletedCount(0);
            stat.setTitleUnlockedCount(1);
            stat.setLoginDays(0);
            stat.setCoreBuildingReachedCount(0);
            stat.touch();
            statRepository.save(stat);
        }

        if (playerTitleRepository.findByRoleIdAndTitleId(roleId, 1L).isEmpty()) {
            PlayerTitle titleRecord = new PlayerTitle();
            titleRecord.setRoleId(roleId);
            titleRecord.setTitleId(1L);
            titleRecord.setUnlockedAt(LocalDateTime.now());
            titleRecord.setIsEquipped(1);
            titleRecord.setSourceType("SYSTEM");
            playerTitleRepository.save(titleRecord);
        }

        if (!signInRepository.existsById(roleId)) {
            PlayerSignInState signIn = new PlayerSignInState();
            signIn.setRoleId(roleId);
            signIn.setTodayOnlineSeconds(0);
            signIn.setDailySigned(false);
            signIn.setContinuousSignDays(0);
            signIn.setCurrentWeekIndex(0);
            signIn.setTotalLoginDays(0);
            signInRepository.save(signIn);
        }

        initializeTasks(roleId);

        if (!quizRepository.existsById(roleId)) {
            PlayerQuizState quizState = new PlayerQuizState();
            quizState.setRoleId(roleId);
            quizState.setEventId(gameConfigService.currentQuizEvent().eventId());
            quizState.setWeeklyScore(0);
            quizState.setHasPlayedToday(false);
            quizState.setWeeklyRewardClaimed(false);
            quizRepository.save(quizState);
        }

        if (!clockInRepository.existsById(roleId)) {
            PlayerClockInState clockInState = new PlayerClockInState();
            clockInState.setRoleId(roleId);
            clockInState.setEventId(gameConfigService.currentClockInEvent().eventId());
            clockInRepository.save(clockInState);
        }

        if (!storyRepository.existsById(roleId)) {
            PlayerStoryState storyState = new PlayerStoryState();
            storyState.setRoleId(roleId);
            storyState.setEventId(gameConfigService.currentStoryEvent().eventId());
            storyState.setHasFinished(false);
            storyState.setRewardClaimed(false);
            storyRepository.save(storyState);
        }
    }

    private void initializeTasks(Long roleId) {
        List<TaskConfigEntity> configs = taskConfigRepository.findAllByOrderByChapterNoAscStepNoAsc();
        List<PlayerTask> existingTasks = playerTaskRepository.findByRoleId(roleId);
        Map<String, PlayerTask> tasksByCode = new LinkedHashMap<>();
        Map<Long, PlayerTask> tasksByTaskId = new LinkedHashMap<>();
        for (PlayerTask existing : existingTasks) {
            if (existing.getTaskCode() != null && !existing.getTaskCode().isBlank()) {
                tasksByCode.putIfAbsent(existing.getTaskCode(), existing);
            }
            if (existing.getTaskId() != null) {
                tasksByTaskId.putIfAbsent(existing.getTaskId(), existing);
            }
        }

        for (TaskConfigEntity config : configs) {
            PlayerTask playerTask = tasksByCode.get(config.getTaskCode());
            if (playerTask == null) {
                playerTask = tasksByTaskId.get(config.getTaskId());
            }

            boolean changed = false;
            if (playerTask == null) {
                playerTask = new PlayerTask();
                playerTask.setRoleId(roleId);
                playerTask.setTaskId(config.getTaskId());
                playerTask.setTaskCode(config.getTaskCode());
                playerTask.setProgressCurrent(0);
                playerTask.setProgressTarget(config.getTargetCount());
                playerTask.setCurrentStep(config.getStepNo());
                playerTask.setTaskStatus(initialStatusFor(config));
                if (TaskStatus.IN_PROGRESS.equals(playerTask.getTaskStatus())) {
                    playerTask.setAcceptedAt(LocalDateTime.now());
                }
                changed = true;
            } else {
                if (playerTask.getRoleId() == null) {
                    playerTask.setRoleId(roleId);
                    changed = true;
                }
                if (playerTask.getTaskId() == null || !playerTask.getTaskId().equals(config.getTaskId())) {
                    playerTask.setTaskId(config.getTaskId());
                    changed = true;
                }
                if (playerTask.getTaskCode() == null || !playerTask.getTaskCode().equals(config.getTaskCode())) {
                    playerTask.setTaskCode(config.getTaskCode());
                    changed = true;
                }
                if (playerTask.getProgressCurrent() == null) {
                    playerTask.setProgressCurrent(0);
                    changed = true;
                }
                if (playerTask.getProgressTarget() == null) {
                    playerTask.setProgressTarget(config.getTargetCount());
                    changed = true;
                }
                if (playerTask.getCurrentStep() == null) {
                    playerTask.setCurrentStep(config.getStepNo());
                    changed = true;
                }
                if (playerTask.getTaskStatus() == null || playerTask.getTaskStatus().isBlank()) {
                    playerTask.setTaskStatus(initialStatusFor(config));
                    changed = true;
                }
                if (TaskStatus.IN_PROGRESS.equals(playerTask.getTaskStatus()) && playerTask.getAcceptedAt() == null) {
                    playerTask.setAcceptedAt(LocalDateTime.now());
                    changed = true;
                }
            }

            if (changed) {
                playerTask.touch();
                playerTask = playerTaskRepository.save(playerTask);
            }

            tasksByCode.put(config.getTaskCode(), playerTask);
            tasksByTaskId.put(config.getTaskId(), playerTask);
        }
    }

    private String initialStatusFor(TaskConfigEntity config) {
        if ("M_1_1".equals(config.getTaskCode())) {
            return TaskStatus.IN_PROGRESS;
        }
        return TaskStatus.LOCKED;
    }
}
