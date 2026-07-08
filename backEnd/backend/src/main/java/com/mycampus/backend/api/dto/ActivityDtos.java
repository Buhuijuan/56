package com.mycampus.backend.api.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotEmpty;

import java.util.List;

public final class ActivityDtos {

    private ActivityDtos() {
    }

    public record QuizSubmitRequest(@NotEmpty List<Integer> answers) {
    }

    public record StoryChoiceRequest(@NotBlank String choice) {
    }

    public record StorySaveRequest(String finalText) {
    }

    public record ClockInCheckRequest(Double currentPosX, Double currentPosY, Double currentPosZ) {
    }

    public record CampusQaRequest(String question, String player_id, String scene_name) {
    }
}
