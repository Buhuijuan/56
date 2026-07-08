package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.TaskDtos;
import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.TaskService;
import jakarta.validation.Valid;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
public class TaskController {

    private final TaskService taskService;

    public TaskController(TaskService taskService) {
        this.taskService = taskService;
    }

    @GetMapping("/api/tasks")
    public ApiResponse<Map<String, Object>> getTasks(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(taskService.getTasks(principal));
    }

    @GetMapping("/api/tasks/{taskCode}")
    public ApiResponse<Map<String, Object>> getTask(@AuthenticationPrincipal CurrentAccount principal,
                                                    @PathVariable String taskCode) {
        return ApiResponse.ok(taskService.getTaskDetail(principal, taskCode));
    }

    @PostMapping("/api/tasks/{taskCode}/accept")
    public ApiResponse<Map<String, Object>> accept(@AuthenticationPrincipal CurrentAccount principal,
                                                   @PathVariable String taskCode) {
        return ApiResponse.ok(taskService.accept(principal, taskCode));
    }

    @PostMapping("/api/tasks/{taskCode}/progress")
    public ApiResponse<Map<String, Object>> progress(@AuthenticationPrincipal CurrentAccount principal,
                                                     @PathVariable String taskCode,
                                                     @Valid @RequestBody TaskDtos.TaskProgressRequest request) {
        return ApiResponse.ok(taskService.progress(principal, taskCode, request));
    }

    @PostMapping("/api/tasks/{taskCode}/claim")
    public ApiResponse<Map<String, Object>> claim(@AuthenticationPrincipal CurrentAccount principal,
                                                  @PathVariable String taskCode) {
        return ApiResponse.ok(taskService.claim(principal, taskCode));
    }

    @GetMapping("/api/tasks/current/main")
    public ApiResponse<Map<String, Object>> currentMainTask(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(taskService.currentMainTask(principal));
    }

    @GetMapping("/api/tasks/chapters")
    public ApiResponse<Map<String, Object>> chapters(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(taskService.chapters(principal));
    }

    @PostMapping("/api/tasks/events")
    public ApiResponse<Map<String, Object>> processEvent(@AuthenticationPrincipal CurrentAccount principal,
                                                         @Valid @RequestBody TaskDtos.TaskEventRequest request) {
        return ApiResponse.ok(taskService.processEvent(principal, request));
    }
}
