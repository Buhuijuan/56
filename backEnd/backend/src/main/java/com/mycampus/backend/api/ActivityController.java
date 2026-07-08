package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.ActivityDtos;
import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.ActivityService;
import jakarta.validation.Valid;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/activities")
public class ActivityController {

    private final ActivityService activityService;

    public ActivityController(ActivityService activityService) {
        this.activityService = activityService;
    }

    @GetMapping("/quiz/current")
    public ApiResponse<Map<String, Object>> currentQuiz(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.currentQuiz(principal));
    }

    @PostMapping("/quiz/start")
    public ApiResponse<Map<String, Object>> startQuiz(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.startQuiz(principal));
    }

    @PostMapping("/quiz/submit")
    public ApiResponse<Map<String, Object>> submitQuiz(@AuthenticationPrincipal CurrentAccount principal,
                                                       @Valid @RequestBody ActivityDtos.QuizSubmitRequest request) {
        return ApiResponse.ok(activityService.submitQuiz(principal, request));
    }

    @PostMapping("/quiz/claim-weekly-reward")
    public ApiResponse<Map<String, Object>> claimWeeklyQuizReward(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.claimWeeklyQuizReward(principal));
    }

    @GetMapping("/clockin/current")
    public ApiResponse<Map<String, Object>> currentClockIn(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.currentClockIn(principal));
    }

    @PostMapping("/clockin/{locationId}/check")
    public ApiResponse<Map<String, Object>> checkClockIn(@AuthenticationPrincipal CurrentAccount principal,
                                                         @PathVariable String locationId,
                                                         @RequestBody(required = false) ActivityDtos.ClockInCheckRequest request) {
        ActivityDtos.ClockInCheckRequest safeRequest = request == null
                ? new ActivityDtos.ClockInCheckRequest(null, null, null)
                : request;
        return ApiResponse.ok(activityService.checkClockIn(principal, locationId, safeRequest));
    }

    @GetMapping("/story/current")
    public ApiResponse<Map<String, Object>> currentStory(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.currentStory(principal));
    }

    @PostMapping("/story/start")
    public ApiResponse<Map<String, Object>> startStory(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.startStory(principal));
    }

    @PostMapping("/story/choice")
    public ApiResponse<Map<String, Object>> chooseStory(@AuthenticationPrincipal CurrentAccount principal,
                                                        @Valid @RequestBody ActivityDtos.StoryChoiceRequest request) {
        return ApiResponse.ok(activityService.chooseStory(principal, request));
    }

    @PostMapping("/story/save")
    public ApiResponse<Map<String, Object>> saveStory(@AuthenticationPrincipal CurrentAccount principal,
                                                      @RequestBody(required = false) ActivityDtos.StorySaveRequest request) {
        ActivityDtos.StorySaveRequest safeRequest = request == null ? new ActivityDtos.StorySaveRequest(null) : request;
        return ApiResponse.ok(activityService.saveStory(principal, safeRequest));
    }

    @PostMapping("/story/upload")
    public ApiResponse<Map<String, Object>> uploadStory(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(activityService.uploadStory(principal));
    }
}
