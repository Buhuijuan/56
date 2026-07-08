package com.mycampus.backend.game.config;

import com.fasterxml.jackson.databind.JsonNode;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.common.JsonUtils;
import com.mycampus.backend.game.config.GameConfigs.*;
import jakarta.annotation.PostConstruct;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.Resource;
import org.springframework.core.io.ResourceLoader;
import org.springframework.core.io.support.ResourcePatternResolver;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.util.StreamUtils;

import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.time.LocalDate;
import java.util.Comparator;
import java.util.List;
import java.util.Optional;

@Service
public class GameConfigService {

    private static final Logger log = LoggerFactory.getLogger(GameConfigService.class);

    private final Path dataRoot;
    private final String classpathRoot;
    private final String clockInDir;
    private final ResourceLoader resourceLoader;
    private final ResourcePatternResolver resourcePatternResolver;

    private List<RewardItemConfig> rewardItems;
    private List<LevelConfig> levels;
    private List<TaskConfig> tasks;
    private List<DailyAwardConfig> dailyAwards;
    private List<OnlineAwardConfig> onlineAwards;
    private List<TotalAwardConfig> totalAwards;
    private List<TitleConfig> titles;
    private List<GrowthStageConfig> growthStages;
    private List<CharacterConfig> characters;
    private List<QuizEventConfig> quizEvents;
    private List<StoryEventConfig> storyEvents;

    public GameConfigService(@Value("${app.game.data-root}") String dataRoot,
                             @Value("${app.game.classpath-root:game-config}") String classpathRoot,
                             @Value("${app.game.clockin-dir}") String clockInDir,
                             ResourceLoader resourceLoader,
                             ResourcePatternResolver resourcePatternResolver) {
        this.dataRoot = dataRoot == null || dataRoot.isBlank() ? null : Path.of(dataRoot);
        this.classpathRoot = classpathRoot;
        this.clockInDir = clockInDir;
        this.resourceLoader = resourceLoader;
        this.resourcePatternResolver = resourcePatternResolver;
    }

    @PostConstruct
    public void init() {
        rewardItems = readList("RewardItem.json", "rewards", RewardItemConfig.class);
        levels = readList("LevelConfig.json", "levels", LevelConfig.class);
        tasks = readList("TaskConfig.json", "tasks", TaskConfig.class);
        dailyAwards = readList("DailyAwardConfig.json", "dailyAwards", DailyAwardConfig.class);
        onlineAwards = readList("OnlineAwardConfig.json", "onlineAwards", OnlineAwardConfig.class);
        totalAwards = readList("TotalAwardConfig.json", "totalAwards", TotalAwardConfig.class);
        titles = readList("TitleConfig.json", "titles", TitleConfig.class);
        growthStages = readList("GrowthConfig.json", "stages", GrowthStageConfig.class);
        characters = readList("CharacterData.json", "characters", CharacterConfig.class);
        quizEvents = readList("QuizEvents.json", "quizEvents", QuizEventConfig.class);
        storyEvents = readList("StoryEvents.json", "storyEvents", StoryEventConfig.class);
    }

    private <T> List<T> readList(String relativePath, String nodeName, Class<T> itemType) {
        try {
            String content = readText(relativePath);
            JsonNode root = JsonUtils.mapper().readTree(content).get(nodeName);
            return JsonUtils.mapper().readerForListOf(itemType).readValue(root);
        } catch (IOException e) {
            throw new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "读取配置失败：" + relativePath);
        }
    }

    public List<RewardItemConfig> rewardItems() {
        return rewardItems;
    }

    public List<LevelConfig> levels() {
        return levels;
    }

    public LevelConfig levelForExp(int exp) {
        return levels.stream()
                .filter(level -> level.requiredExp() <= exp)
                .max(Comparator.comparing(LevelConfig::requiredExp))
                .orElse(levels.get(0));
    }

    public List<TaskConfig> tasks() {
        return tasks;
    }

    public Optional<TaskConfig> taskById(String taskId) {
        return tasks.stream().filter(task -> task.taskChapterID().equals(taskId)).findFirst();
    }

    public List<DailyAwardConfig> dailyAwards() {
        return dailyAwards;
    }

    public List<OnlineAwardConfig> onlineAwards() {
        return onlineAwards;
    }

    public List<TotalAwardConfig> totalAwards() {
        return totalAwards;
    }

    public Optional<TotalAwardConfig> totalAward(Integer awardId) {
        return totalAwards.stream().filter(item -> item.awardID().equals(awardId)).findFirst();
    }

    public List<TitleConfig> titles() {
        return titles;
    }

    public List<GrowthStageConfig> growthStages() {
        return growthStages;
    }

    public List<CharacterConfig> characters() {
        return characters;
    }

    public QuizEventConfig currentQuizEvent() {
        LocalDate today = LocalDate.now();
        return quizEvents.stream()
                .filter(event -> {
                    LocalDate start = event.startTime().toLocalDate();
                    LocalDate end = start.plusDays(event.durationDays());
                    return !today.isBefore(start) && today.isBefore(end);
                })
                .findFirst()
                .orElseGet(() -> quizEvents.stream()
                        .max(Comparator.comparing(QuizEventConfig::startTime))
                        .orElseThrow(() -> new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "当前未配置校园问答活动。")));
    }

    public List<QuizQuestionConfig> quizQuestions(String questionsFile) {
        return readList(Path.of("QuizQuestions", questionsFile + ".json").toString(), "questions", QuizQuestionConfig.class);
    }

    public StoryEventConfig currentStoryEvent() {
        LocalDate today = LocalDate.now();
        return storyEvents.stream()
                .filter(event -> {
                    LocalDate start = event.startTime().toLocalDate();
                    LocalDate end = start.plusDays(event.durationDays());
                    return !today.isBefore(start) && today.isBefore(end);
                })
                .findFirst()
                .orElseGet(() -> storyEvents.stream()
                        .max(Comparator.comparing(StoryEventConfig::startTime))
                        .orElseThrow(() -> new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "当前未配置故事接龙活动。")));
    }

    public ClockInEventConfig currentClockInEvent() {
        try {
            return JsonUtils.mapper().readValue(readClockInEventText(), ClockInEventConfig.class);
        } catch (Exception e) {
            log.warn("Failed to load current clock-in config, fallback to empty config", e);
            return new ClockInEventConfig("clockin_fallback", "05:00:00", List.of());
        }
    }

    private String readClockInEventText() throws IOException {
        String todayFile = clockInDir + "/ClockInEvent_" + LocalDate.now().toString().replace('-', '_') + ".json";
        try {
            return readText(todayFile);
        } catch (IOException ignored) {
            String latestFile = latestClockInRelativePath()
                    .orElseThrow(() -> new IOException("No clock-in config available"));
            return readText(latestFile);
        }
    }

    private Optional<String> latestClockInRelativePath() {
        if (dataRoot != null) {
            Path dir = dataRoot.resolve(clockInDir);
            if (Files.isDirectory(dir)) {
                try (var paths = Files.list(dir)) {
                    return paths
                            .filter(file -> {
                                String name = file.getFileName().toString();
                                return name.startsWith("ClockInEvent_") && name.endsWith(".json");
                            })
                            .max(Comparator.comparing(Path::getFileName))
                            .map(path -> clockInDir + "/" + path.getFileName());
                } catch (IOException e) {
                    log.warn("Failed to scan external clock-in config directory {}", dir, e);
                }
            }
        }

        try {
            Resource[] resources = resourcePatternResolver.getResources("classpath*:" + classpathRoot + "/" + clockInDir + "/ClockInEvent_*.json");
            return List.of(resources).stream()
                    .filter(Resource::exists)
                    .max(Comparator.comparing(resource -> resource.getFilename() == null ? "" : resource.getFilename()))
                    .map(resource -> clockInDir + "/" + resource.getFilename());
        } catch (IOException e) {
            log.warn("Failed to scan packaged clock-in configs", e);
            return Optional.empty();
        }
    }

    private String readText(String relativePath) throws IOException {
        if (dataRoot != null) {
            Path path = dataRoot.resolve(relativePath);
            if (Files.exists(path)) {
                return Files.readString(path);
            }
        }

        String normalized = relativePath.replace('\\', '/');
        Resource resource = resourceLoader.getResource("classpath:" + classpathRoot + "/" + normalized);
        if (!resource.exists()) {
            throw new IOException("Resource not found: " + normalized);
        }

        try (InputStream inputStream = resource.getInputStream()) {
            return StreamUtils.copyToString(inputStream, StandardCharsets.UTF_8);
        }
    }
}
