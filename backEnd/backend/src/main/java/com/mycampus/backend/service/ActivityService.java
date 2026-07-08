package com.mycampus.backend.service;

import com.mycampus.backend.activity.entity.PlayerClockInState;
import com.mycampus.backend.activity.entity.PlayerQuizState;
import com.mycampus.backend.activity.entity.PlayerStoryState;
import com.mycampus.backend.activity.entity.StoryRecordEntity;
import com.mycampus.backend.activity.repository.PlayerClockInStateRepository;
import com.mycampus.backend.activity.repository.PlayerQuizStateRepository;
import com.mycampus.backend.activity.repository.PlayerStoryStateRepository;
import com.mycampus.backend.activity.repository.StoryRecordRepository;
import com.mycampus.backend.api.dto.ActivityDtos;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.game.config.GameConfigService;
import com.mycampus.backend.game.config.GameConfigs;
import com.mycampus.backend.player.entity.PlayerStat;
import com.mycampus.backend.player.repository.PlayerStatRepository;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.task.entity.BuildingLocation;
import com.mycampus.backend.task.repository.BuildingLocationRepository;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.Clock;
import java.time.LocalDate;
import java.time.LocalTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class ActivityService {

    private final PlayerService playerService;
    private final PlayerQuizStateRepository quizRepository;
    private final PlayerClockInStateRepository clockInRepository;
    private final PlayerStoryStateRepository storyRepository;
    private final StoryRecordRepository storyRecordRepository;
    private final GameConfigService gameConfigService;
    private final ProgressionService progressionService;
    private final PlayerStatRepository playerStatRepository;
    private final BuildingLocationRepository buildingLocationRepository;
    private final int storyMaxRounds;
    private final Clock clock;

    private final Map<Long, QuizSession> quizSessions = new ConcurrentHashMap<>();
    private final Map<Long, StorySession> storySessions = new ConcurrentHashMap<>();

    private static final Map<String, String> CLOCK_IN_BUILDING_CODES = Map.of(
            "loc_teaching_building_1", "teaching_building_1",
            "loc_library", "library",
            "loc_art_square", "art_building",
            "loc_jin_lake", "jinhu"
    );

    public ActivityService(PlayerService playerService,
                           PlayerQuizStateRepository quizRepository,
                           PlayerClockInStateRepository clockInRepository,
                           PlayerStoryStateRepository storyRepository,
                           StoryRecordRepository storyRecordRepository,
                           GameConfigService gameConfigService,
                           ProgressionService progressionService,
                           PlayerStatRepository playerStatRepository,
                           BuildingLocationRepository buildingLocationRepository,
                           Clock clock,
                           @Value("${app.game.story-max-rounds}") int storyMaxRounds) {
        this.playerService = playerService;
        this.quizRepository = quizRepository;
        this.clockInRepository = clockInRepository;
        this.storyRepository = storyRepository;
        this.storyRecordRepository = storyRecordRepository;
        this.gameConfigService = gameConfigService;
        this.progressionService = progressionService;
        this.playerStatRepository = playerStatRepository;
        this.buildingLocationRepository = buildingLocationRepository;
        this.clock = clock;
        this.storyMaxRounds = storyMaxRounds;
    }

    public Map<String, Object> currentQuiz(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerQuizState state = quizRepository.findById(roleId).orElseThrow();
        GameConfigs.QuizEventConfig event = gameConfigService.currentQuizEvent();
        normalizeQuizState(state, event);
        quizRepository.save(state);

        boolean canClaimWeeklyReward = canClaimWeeklyReward(state, event);
        int claimableCoin = canClaimWeeklyReward ? calculateWeeklyRewardCoin(state.getWeeklyScore()) : 0;

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("config", event);
        result.put("state", state);
        result.put("canClaimWeeklyReward", canClaimWeeklyReward);
        result.put("claimableCoin", claimableCoin);
        result.put("weeklyRewardClaimed", Boolean.TRUE.equals(state.getWeeklyRewardClaimed()));
        return result;
    }

    @Transactional
    public Map<String, Object> startQuiz(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerQuizState state = quizRepository.findById(roleId).orElseThrow();
        GameConfigs.QuizEventConfig event = gameConfigService.currentQuizEvent();
        normalizeQuizState(state, event);
        quizRepository.save(state);

        if (Boolean.TRUE.equals(state.getHasPlayedToday()) && LocalDate.now(clock).equals(state.getLastPlayDate())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "你今天已经参加过校园问答了。");
        }

        List<GameConfigs.QuizQuestionConfig> questions = new ArrayList<>(gameConfigService.quizQuestions(event.questionsFile()));
        Collections.shuffle(questions);
        List<GameConfigs.QuizQuestionConfig> selected = questions.stream().limit(event.totalQuestions()).toList();
        quizSessions.put(roleId, new QuizSession(selected));

        List<Map<String, Object>> questionViews = selected.stream().map(q -> {
            Map<String, Object> question = new LinkedHashMap<>();
            question.put("questionId", q.questionId());
            question.put("questionText", q.questionText());
            question.put("options", q.options());
            question.put("correctIndex", q.correctIndex());
            question.put("explanation", q.explanation());
            return question;
        }).toList();

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("event", event);
        result.put("questions", questionViews);
        return result;
    }

    @Transactional
    public Map<String, Object> submitQuiz(CurrentAccount principal, ActivityDtos.QuizSubmitRequest request) {
        Long roleId = playerService.currentRole(principal).getId();
        QuizSession session = quizSessions.get(roleId);
        if (session == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前没有进行中的问答场次。");
        }
        if (request.answers().size() != session.questions().size()) {
            throw new AppException(HttpStatus.BAD_REQUEST, "提交答案数量与当前题目数量不一致。");
        }

        int score = 0;
        List<Map<String, Object>> results = new ArrayList<>();
        for (int i = 0; i < request.answers().size(); i++) {
            GameConfigs.QuizQuestionConfig question = session.questions().get(i);
            Integer selectedIndex = request.answers().get(i);
            boolean correct = Objects.equals(selectedIndex, question.correctIndex());
            if (correct) {
                score += 10;
            }
            Map<String, Object> item = new LinkedHashMap<>();
            item.put("questionId", question.questionId());
            item.put("selectedIndex", selectedIndex);
            item.put("correctIndex", question.correctIndex());
            item.put("correct", correct);
            item.put("explanation", question.explanation());
            results.add(item);
        }

        PlayerQuizState state = quizRepository.findById(roleId).orElseThrow();
        GameConfigs.QuizEventConfig event = gameConfigService.currentQuizEvent();
        normalizeQuizState(state, event);
        state.setEventId(event.eventId());
        state.setWeeklyScore((state.getWeeklyScore() == null ? 0 : state.getWeeklyScore()) + score);
        state.setLastPlayDate(LocalDate.now(clock));
        state.setHasPlayedToday(true);
        quizRepository.save(state);
        quizSessions.remove(roleId);

        final int correctCount = score / 10;
        updateStat(roleId, stat -> stat.setQuizCorrectCount(stat.getQuizCorrectCount() + correctCount));
        Map<String, Object> exp = progressionService.grantExp(roleId, "QUIZ", state.getEventId(), Math.min(score, 100));
        List<Map<String, Object>> behaviorTitleUnlocks = progressionService.refreshBehaviorTitles(roleId);
        return Map.of(
                "score", score,
                "weeklyScore", state.getWeeklyScore(),
                "exp", exp,
                "addedCoin", 0,
                "results", results,
                "behaviorTitleUnlocks", behaviorTitleUnlocks
        );
    }

    @Transactional
    public Map<String, Object> claimWeeklyQuizReward(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerQuizState state = quizRepository.findById(roleId).orElseThrow();
        GameConfigs.QuizEventConfig event = gameConfigService.currentQuizEvent();
        normalizeQuizState(state, event);

        if (!canClaimWeeklyReward(state, event)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前还不满足领取问答周奖励的条件");
        }

        int addedCoin = calculateWeeklyRewardCoin(state.getWeeklyScore());
        String claimTier = weeklyRewardTier(state.getWeeklyScore());
        if (addedCoin > 0) {
            progressionService.grantCoins(roleId, "QUIZ_WEEKLY_REWARD", event.eventId(), addedCoin);
        }
        state.setWeeklyRewardClaimed(true);
        quizRepository.save(state);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("addedCoin", addedCoin);
        result.put("weeklyScore", state.getWeeklyScore());
        result.put("claimed", true);
        result.put("claimTier", claimTier);
        return result;
    }

    public Map<String, Object> currentClockIn(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerClockInState state = clockInRepository.findById(roleId).orElseThrow();
        normalizeClockInState(state);
        clockInRepository.save(state);

        List<String> checkedLocationIds = state.getCheckedIn().entrySet().stream()
                .filter(Map.Entry::getValue)
                .map(Map.Entry::getKey)
                .toList();

        Map<String, Object> stateView = new LinkedHashMap<>();
        stateView.put("roleId", state.getRoleId());
        stateView.put("eventId", state.getEventId());
        stateView.put("lastCheckInDate", state.getLastCheckInDate());
        stateView.put("checkedLocationIds", checkedLocationIds);

        Map<String, Object> result = new LinkedHashMap<>();
        result.put("config", gameConfigService.currentClockInEvent());
        result.put("state", stateView);
        return result;
    }

    @Transactional
    public Map<String, Object> checkClockIn(CurrentAccount principal, String locationId, ActivityDtos.ClockInCheckRequest request) {
        if (LocalTime.now(clock).isAfter(LocalTime.of(11, 0))) {
            throw new AppException(HttpStatus.BAD_REQUEST, "今天的晨间打卡时间已经结束。");
        }

        Long roleId = playerService.currentRole(principal).getId();
        PlayerClockInState state = clockInRepository.findById(roleId).orElseThrow();
        normalizeClockInState(state);
        long checkedBefore = state.getCheckedIn().values().stream().filter(Boolean::booleanValue).count();

        if (!state.getCheckedIn().containsKey(locationId)) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前打卡点不在今日活动范围内。");
        }
        if (Boolean.TRUE.equals(state.getCheckedIn().get(locationId))) {
            throw new AppException(HttpStatus.BAD_REQUEST, "这个地点今天已经完成打卡了。");
        }
        if (request.currentPosX() == null || request.currentPosY() == null || request.currentPosZ() == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "缺少玩家当前位置，无法完成打卡。");
        }

        BuildingLocation location = resolveClockInBuilding(locationId);
        double distance = calculateDistance(request, location);
        double radius = valueOf(location.getCheckinRadius());
        if (distance > radius) {
            throw new AppException(HttpStatus.BAD_REQUEST,
                    "你当前不在打卡范围内。当前距离 %.2f 米，需要进入 %.2f 米范围内。"
                            .formatted(distance, radius));
        }

        state.getCheckedIn().put(locationId, true);
        clockInRepository.save(state);
        long completed = state.getCheckedIn().values().stream().filter(Boolean::booleanValue).count();
        if (checkedBefore == 0 && completed > 0) {
            updateStat(roleId, stat -> stat.setMorningCheckinDays(stat.getMorningCheckinDays() + 1));
        }
        Map<String, Object> coinReward = progressionService.grantCoins(roleId, "CLOCK_IN", state.getEventId() + ":" + locationId, 10);
        List<Map<String, Object>> behaviorTitleUnlocks = progressionService.refreshBehaviorTitles(roleId);

        return Map.of(
                "locationId", locationId,
                "checkedCount", completed,
                "total", state.getCheckedIn().size(),
                "addedCoin", coinReward.get("addedCoin"),
                "distanceToTarget", distance,
                "behaviorTitleUnlocks", behaviorTitleUnlocks
        );
    }

    public Map<String, Object> currentStory(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        PlayerStoryState state = storyRepository.findById(roleId).orElseThrow();
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("config", gameConfigService.currentStoryEvent());
        result.put("state", state);
        result.put("myStories", storyRecordRepository.findByRoleIdOrderByCreatedAtDesc(roleId));
        result.put("uploadedStories", storyRecordRepository.findByUploadedTrueOrderByCreatedAtDesc());
        return result;
    }

    public Map<String, Object> startStory(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        GameConfigs.StoryEventConfig event = gameConfigService.currentStoryEvent();
        GameConfigs.StorySegment first = new GameConfigs.StorySegment(
                "你正站在一段校园故事的开端。当前主题：" + event.theme(),
                List.of("从食堂开始", "从图书馆开始", "从操场开始"),
                null
        );
        storySessions.put(roleId, new StorySession(event.eventId(), event.theme(), new ArrayList<>(List.of(first))));
        return Map.of("event", event, "segments", List.of(first), "round", 0);
    }

    public Map<String, Object> chooseStory(CurrentAccount principal, ActivityDtos.StoryChoiceRequest request) {
        Long roleId = playerService.currentRole(principal).getId();
        StorySession session = storySessions.get(roleId);
        if (session == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前没有进行中的故事接龙。");
        }

        int currentRound = session.segments().size();
        GameConfigs.StorySegment last = session.segments().get(session.segments().size() - 1);
        session.segments().set(session.segments().size() - 1,
                new GameConfigs.StorySegment(last.segmentText(), last.options(), request.choice()));

        if (currentRound >= storyMaxRounds) {
            return Map.of("finished", true, "segments", session.segments());
        }

        GameConfigs.StorySegment next = new GameConfigs.StorySegment(
                "你选择了“" + request.choice() + "”。新的校园故事片段正在第 " + currentRound + " 回合展开。",
                currentRound == storyMaxRounds - 1
                        ? List.of("撰写结局")
                        : List.of("继续探索", "寻找 NPC", "记录此刻心情"),
                null
        );
        session.segments().add(next);
        return Map.of("finished", session.segments().size() >= storyMaxRounds, "segments", session.segments(), "round", session.segments().size() - 1);
    }

    @Transactional
    public Map<String, Object> saveStory(CurrentAccount principal, ActivityDtos.StorySaveRequest request) {
        var account = playerService.currentAccount(principal);
        var role = playerService.currentRole(principal);
        StorySession session = storySessions.get(role.getId());
        if (session == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "当前没有可保存的故事接龙内容。");
        }

        PlayerStoryState state = storyRepository.findById(role.getId()).orElseThrow();
        String fullText = request.finalText() != null && !request.finalText().isBlank()
                ? request.finalText()
                : session.segments().stream().map(GameConfigs.StorySegment::segmentText).reduce("", (a, b) -> a + "\n" + b).trim();

        StoryRecordEntity record = new StoryRecordEntity();
        record.setAccountId(account.getId());
        record.setRoleId(role.getId());
        record.setEventId(session.eventId());
        record.setTheme(session.theme());
        record.setFullText(fullText);
        record.setUploaded(false);
        storyRecordRepository.save(record);

        state.setEventId(session.eventId());
        state.setHasFinished(true);
        state.setLastPlayDate(LocalDate.now(clock));
        int addedCoin = 0;
        if (!Boolean.TRUE.equals(state.getRewardClaimed())) {
            progressionService.grantExp(role.getId(), "STORY", session.eventId(), 80);
            addedCoin = sumRewardCoins(gameConfigService.currentStoryEvent().rewards());
            if (addedCoin > 0) {
                progressionService.grantCoins(role.getId(), "STORY", session.eventId(), addedCoin);
            }
            state.setRewardClaimed(true);
        }
        storyRepository.save(state);
        updateStat(role.getId(), stat -> stat.setStoryCompletedCount(stat.getStoryCompletedCount() + 1));
        List<Map<String, Object>> behaviorTitleUnlocks = progressionService.refreshBehaviorTitles(role.getId());

        return Map.of("saved", true, "storyId", record.getId(), "rewardClaimed", true, "addedCoin", addedCoin, "behaviorTitleUnlocks", behaviorTitleUnlocks);
    }

    @Transactional
    public Map<String, Object> uploadStory(CurrentAccount principal) {
        Long roleId = playerService.currentRole(principal).getId();
        StoryRecordEntity record = storyRecordRepository.findByRoleIdOrderByCreatedAtDesc(roleId)
                .stream()
                .findFirst()
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "当前没有可上传的故事内容。"));
        record.setUploaded(true);
        storyRecordRepository.save(record);
        return Map.of("uploaded", true, "storyId", record.getId());
    }

    private void normalizeQuizState(PlayerQuizState state, GameConfigs.QuizEventConfig event) {
        LocalDate today = LocalDate.now(clock);
        if (!Objects.equals(state.getEventId(), event.eventId())) {
            state.setEventId(event.eventId());
            state.setWeeklyScore(0);
            state.setLastPlayDate(null);
            state.setHasPlayedToday(false);
            state.setWeeklyRewardClaimed(false);
            return;
        }
        if (!today.equals(state.getLastPlayDate())) {
            state.setHasPlayedToday(false);
        }
        if (state.getWeeklyRewardClaimed() == null) {
            state.setWeeklyRewardClaimed(false);
        }
    }

    private boolean canClaimWeeklyReward(PlayerQuizState state, GameConfigs.QuizEventConfig event) {
        LocalDate today = LocalDate.now(clock);
        LocalDate lastDay = event.startTime().toLocalDate().plusDays(event.durationDays() - 1L);
        return today.equals(lastDay)
                && today.equals(state.getLastPlayDate())
                && Boolean.TRUE.equals(state.getHasPlayedToday())
                && !Boolean.TRUE.equals(state.getWeeklyRewardClaimed());
    }

    private int calculateWeeklyRewardCoin(Integer weeklyScore) {
        int score = weeklyScore == null ? 0 : weeklyScore;
        if (score <= 200) {
            return 10;
        }
        if (score <= 400) {
            return 30;
        }
        if (score <= 550) {
            return 50;
        }
        return 100;
    }

    private String weeklyRewardTier(Integer weeklyScore) {
        int score = weeklyScore == null ? 0 : weeklyScore;
        if (score <= 200) {
            return "0-200";
        }
        if (score <= 400) {
            return "201-400";
        }
        if (score <= 550) {
            return "401-550";
        }
        return "551-700";
    }

    private void normalizeClockInState(PlayerClockInState state) {
        GameConfigs.ClockInEventConfig config = gameConfigService.currentClockInEvent();
        LocalDate today = LocalDate.now(clock);
        if (state.getLastCheckInDate() == null || !state.getLastCheckInDate().equals(today) || !Objects.equals(state.getEventId(), config.eventId())) {
            state.setEventId(config.eventId());
            state.setLastCheckInDate(today);
            LinkedHashMap<String, Boolean> map = new LinkedHashMap<>();
            config.locations().forEach(location -> map.put(location.locationId(), false));
            state.setCheckedIn(map);
        }
    }

    private BuildingLocation resolveClockInBuilding(String locationId) {
        String buildingCode = locationId == null ? null : CLOCK_IN_BUILDING_CODES.get(locationId);
        if (buildingCode == null) {
            throw new AppException(HttpStatus.BAD_REQUEST, "后端尚未配置该打卡点对应的建筑映射。");
        }

        Optional<BuildingLocation> location = buildingLocationRepository.findByBuildingCode(buildingCode);
        return location.orElseThrow(() -> new AppException(HttpStatus.NOT_FOUND, "后端尚未配置该打卡点的坐标信息。"));
    }

    private double calculateDistance(ActivityDtos.ClockInCheckRequest request, BuildingLocation location) {
        double dx = request.currentPosX() - valueOf(location.getPosX());
        double dz = request.currentPosZ() - valueOf(location.getPosZ());
        return Math.sqrt(dx * dx + dz * dz);
    }

    private double valueOf(BigDecimal value) {
        return value == null ? 0d : value.doubleValue();
    }

    private void updateStat(Long roleId, java.util.function.Consumer<PlayerStat> consumer) {
        PlayerStat stat = playerStatRepository.findById(roleId).orElse(null);
        if (stat == null) {
            return;
        }
        consumer.accept(stat);
        stat.touch();
        playerStatRepository.save(stat);
    }

    private int sumRewardCoins(List<GameConfigs.RewardRef> rewards) {
        if (rewards == null) {
            return 0;
        }
        return rewards.stream()
                .filter(Objects::nonNull)
                .filter(reward -> reward.rewardId() != null && reward.amount() != null)
                .filter(reward -> reward.rewardId() == 1)
                .mapToInt(GameConfigs.RewardRef::amount)
                .sum();
    }

    private record QuizSession(List<GameConfigs.QuizQuestionConfig> questions) {
    }

    private record StorySession(String eventId, String theme, List<GameConfigs.StorySegment> segments) {
    }
}
