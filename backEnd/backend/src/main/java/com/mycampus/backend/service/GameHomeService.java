package com.mycampus.backend.service;

import com.mycampus.backend.activity.repository.PlayerClockInStateRepository;
import com.mycampus.backend.activity.repository.PlayerQuizStateRepository;
import com.mycampus.backend.activity.repository.PlayerStoryStateRepository;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.player.repository.PlayerProfileRepository;
import com.mycampus.backend.progression.repository.PlayerLevelStateRepository;
import com.mycampus.backend.progression.repository.PlayerTitleStateRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.signin.repository.PlayerSignInStateRepository;
import com.mycampus.backend.task.repository.PlayerTaskRepository;
import com.mycampus.backend.task.repository.TaskConfigRepository;
import org.springframework.stereotype.Service;

import java.util.LinkedHashMap;
import java.util.Map;

@Service
public class GameHomeService {

    private final PlayerService playerService;
    private final PlayerProfileRepository profileRepository;
    private final PlayerLevelStateRepository levelRepository;
    private final PlayerTitleStateRepository titleRepository;
    private final PlayerSignInStateRepository signInRepository;
    private final PlayerTaskRepository taskRepository;
    private final TaskConfigRepository taskConfigRepository;
    private final PlayerQuizStateRepository quizRepository;
    private final PlayerClockInStateRepository clockInRepository;
    private final PlayerStoryStateRepository storyRepository;
    private final GameConfigService gameConfigService;
    private final PlayerStateInitializer playerStateInitializer;

    public GameHomeService(PlayerService playerService,
                           PlayerProfileRepository profileRepository,
                           PlayerLevelStateRepository levelRepository,
                           PlayerTitleStateRepository titleRepository,
                           PlayerSignInStateRepository signInRepository,
                           PlayerTaskRepository taskRepository,
                           TaskConfigRepository taskConfigRepository,
                           PlayerQuizStateRepository quizRepository,
                           PlayerClockInStateRepository clockInRepository,
                           PlayerStoryStateRepository storyRepository,
                           GameConfigService gameConfigService,
                           PlayerStateInitializer playerStateInitializer) {
        this.playerService = playerService;
        this.profileRepository = profileRepository;
        this.levelRepository = levelRepository;
        this.titleRepository = titleRepository;
        this.signInRepository = signInRepository;
        this.taskRepository = taskRepository;
        this.taskConfigRepository = taskConfigRepository;
        this.quizRepository = quizRepository;
        this.clockInRepository = clockInRepository;
        this.storyRepository = storyRepository;
        this.gameConfigService = gameConfigService;
        this.playerStateInitializer = playerStateInitializer;
    }

    public Map<String, Object> home(CurrentAccount principal) {
        var account = playerService.currentAccount(principal);
        var role = playerService.currentRole(principal);
        Long roleId = role.getId();
        playerStateInitializer.ensureInitialized(roleId);
        Map<String, Object> accountView = new LinkedHashMap<>();
        accountView.put("id", account.getId());
        accountView.put("accountCode", account.getAccountCode());
        accountView.put("mailbox", account.getMailbox());

        Map<String, Object> taskView = new LinkedHashMap<>();
        taskView.put("configs", taskConfigRepository.findAllByOrderByChapterNoAscStepNoAsc());
        taskView.put("state", taskRepository.findByRoleId(roleId));

        Map<String, Object> quizView = new LinkedHashMap<>();
        quizView.put("config", gameConfigService.currentQuizEvent());
        quizView.put("state", quizRepository.findById(roleId).orElseThrow());

        Map<String, Object> clockInView = new LinkedHashMap<>();
        clockInView.put("config", gameConfigService.currentClockInEvent());
        clockInView.put("state", clockInRepository.findById(roleId).orElseThrow());

        Map<String, Object> storyView = new LinkedHashMap<>();
        storyView.put("config", gameConfigService.currentStoryEvent());
        storyView.put("state", storyRepository.findById(roleId).orElseThrow());

        Map<String, Object> activitiesView = new LinkedHashMap<>();
        activitiesView.put("quiz", quizView);
        activitiesView.put("clockIn", clockInView);
        activitiesView.put("story", storyView);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("account", accountView);
        result.put("role", role);
        result.put("profile", profileRepository.findById(roleId).orElseThrow());
        result.put("level", levelRepository.findById(roleId).orElseThrow());
        result.put("title", titleRepository.findById(roleId).orElseThrow());
        result.put("signIn", signInRepository.findById(roleId).orElseThrow());
        result.put("task", taskView);
        result.put("activities", activitiesView);
        return result;
    }
}
