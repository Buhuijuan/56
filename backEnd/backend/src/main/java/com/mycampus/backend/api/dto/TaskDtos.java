package com.mycampus.backend.api.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;
import java.util.List;
import java.util.Map;

public final class TaskDtos {

    private TaskDtos() {
    }

    public record TaskProgressRequest(@NotBlank String eventType,
                                      String targetType,
                                      Long targetId,
                                      Integer increment,
                                      Map<String, Object> extra) {
    }

    public record TaskEventRequest(@NotBlank String eventType,
                                   String targetType,
                                   Long targetId,
                                   Integer increment,
                                   BigDecimal currentPosX,
                                   BigDecimal currentPosY,
                                   BigDecimal currentPosZ,
                                   BigDecimal targetPosX,
                                   BigDecimal targetPosY,
                                   BigDecimal targetPosZ,
                                   String targetAnchorKey,
                                   BigDecimal distanceToTarget,
                                   Boolean success,
                                   Map<String, Object> extra) {
    }

    public record AcceptTaskRequest(@NotNull Boolean accepted) {
    }

    public record ElfPromptDto(String npcName,
                               String avatarKey,
                               String stage,
                               String title,
                               List<String> contents,
                               Boolean autoPopup,
                               String taskCode) {
    }

    public record PromptMessageDto(String speakerType,
                                   String speakerName,
                                   String avatarKey,
                                   String stage,
                                   List<String> contents,
                                   Boolean autoPopup,
                                   String taskCode) {
    }

    public record ChapterCompletionDto(Integer chapterNo,
                                       String chapterTitle,
                                       Long chapterTitleId,
                                       String chapterTitleName) {
    }
}
