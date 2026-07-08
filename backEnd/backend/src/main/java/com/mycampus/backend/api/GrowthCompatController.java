package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.growthcompat.*;
import com.mycampus.backend.common.CompatResult;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.GrowthCompatService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

@RestController
public class GrowthCompatController {

    private final GrowthCompatService growthCompatService;

    public GrowthCompatController(GrowthCompatService growthCompatService) {
        this.growthCompatService = growthCompatService;
    }

    @PostMapping("/api/profile/init")
    public CompatResult<String> initProfile(@AuthenticationPrincipal CurrentAccount principal,
                                            @RequestBody InitProfileCompatRequest request) {
        growthCompatService.initProfile(principal, request);
        return CompatResult.success("profile initialized", null);
    }

    @GetMapping("/api/profile/growth/{roleId}")
    public CompatResult<GrowthProfileCompatDto> getGrowthProfile(@AuthenticationPrincipal CurrentAccount principal,
                                                                 @PathVariable Long roleId) {
        return CompatResult.success(growthCompatService.getGrowthProfile(principal, roleId));
    }

    @GetMapping("/api/profile/snapshot/{roleId}")
    public CompatResult<GrowthSnapshotCompatDto> getGrowthSnapshot(@AuthenticationPrincipal CurrentAccount principal,
                                                                   @PathVariable Long roleId) {
        return CompatResult.success(growthCompatService.getGrowthSnapshot(principal, roleId));
    }

    @PostMapping("/api/task/accept")
    public CompatResult<String> acceptTask(@AuthenticationPrincipal CurrentAccount principal,
                                           @RequestBody AcceptTaskCompatRequest request) {
        growthCompatService.acceptTask(principal, request);
        return CompatResult.success("task accepted", null);
    }

    @PostMapping("/api/task/progress")
    public CompatResult<String> updateTaskProgress(@AuthenticationPrincipal CurrentAccount principal,
                                                   @RequestBody UpdateTaskProgressCompatRequest request) {
        growthCompatService.updateTaskProgress(principal, request);
        return CompatResult.success("task progress updated", null);
    }

    @PostMapping("/api/task/claim")
    public CompatResult<TaskRewardCompatDto> claimTaskReward(@AuthenticationPrincipal CurrentAccount principal,
                                                             @RequestBody ClaimTaskRewardCompatRequest request) {
        return CompatResult.success(growthCompatService.claimTaskReward(principal, request));
    }
}
