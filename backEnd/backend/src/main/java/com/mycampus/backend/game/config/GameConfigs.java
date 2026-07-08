package com.mycampus.backend.game.config;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.time.LocalDateTime;
import java.util.List;

public final class GameConfigs {

    private GameConfigs() {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record RewardRef(Integer rewardId, Integer amount) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record RewardItemConfig(Integer rewardId, String rewardName, String spritePath) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record LevelConfig(Integer level, Integer requiredExp, Integer titleID, List<RewardRef> rewards) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record TaskGoalConfig(String goalId, String description) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record TaskConfig(String typeString, String taskChapterID, String taskChapterTitle,
                             String taskChapterDescription, List<TaskGoalConfig> goals) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record DailyAwardConfig(Integer baseAwardID, Integer dayIndex, RewardRef baseReward,
                                   Integer extraAwardID, RewardRef extraReward) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record OnlineAwardConfig(Integer awardID, Integer requiredMinutes, List<RewardRef> rewards) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record TotalAwardConfig(Integer awardID, Integer requiredDays, Integer rewardCharacterID) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record TitleConfig(Integer titleID, String typeString, String titleName) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record GrowthTaskConfig(String taskId, String description) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record GrowthStageConfig(String stageID, String stageTitle, List<GrowthTaskConfig> tasks,
                                    List<RewardRef> rewards, Integer titleID) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record CharacterConfig(Integer characterID, String characterImagePath, String characterHeadPath) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record QuizQuestionConfig(String questionId, String questionText, List<String> options,
                                     Integer correctIndex, String explanation) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record QuizEventConfig(String eventId, String theme, String startTimeString, Integer durationDays,
                                  Integer totalQuestions, String questionsFile, List<RewardRef> finalRewards) {
        public LocalDateTime startTime() {
            return LocalDateTime.parse(startTimeString);
        }
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record Position(float x, float y, float z) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record ClockInLocationConfig(String locationId, String name, Position worldPosition) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record ClockInEventConfig(String eventId, String refreshTimeString, List<ClockInLocationConfig> locations) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record StoryEventConfig(String eventId, String theme, String themeDescription,
                                   String startTimeString, Integer durationDays, List<RewardRef> rewards) {
        public LocalDateTime startTime() {
            return LocalDateTime.parse(startTimeString + "T00:00:00");
        }
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record StorySegment(String segmentText, List<String> options, String userChoice) {
    }
}
