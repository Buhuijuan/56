package com.mycampus.backend.api;

import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.ProgressionService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
public class ProgressionController {

    private final ProgressionService progressionService;

    public ProgressionController(ProgressionService progressionService) {
        this.progressionService = progressionService;
    }

    @GetMapping("/api/growth")
    public ApiResponse<Map<String, Object>> growth(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(progressionService.growth(principal));
    }

    @PostMapping("/api/growth/refresh")
    public ApiResponse<Map<String, Object>> refreshGrowth(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(progressionService.refreshGrowth(principal));
    }

    @PostMapping("/api/growth/{stageId}/claim")
    public ApiResponse<Map<String, Object>> claimGrowth(@AuthenticationPrincipal CurrentAccount principal,
                                                        @PathVariable String stageId) {
        return ApiResponse.ok(progressionService.claimGrowth(principal, stageId));
    }

    @PostMapping("/api/levels/{level}/reward/claim")
    public ApiResponse<Map<String, Object>> claimLevelReward(@AuthenticationPrincipal CurrentAccount principal,
                                                             @PathVariable Integer level) {
        return ApiResponse.ok(progressionService.claimLevelReward(principal, level));
    }

    @PostMapping("/api/levels/{level}/title/claim")
    public ApiResponse<Map<String, Object>> claimLevelTitle(@AuthenticationPrincipal CurrentAccount principal,
                                                            @PathVariable Integer level) {
        return ApiResponse.ok(progressionService.claimLevelTitle(principal, level));
    }

    @PostMapping("/api/titles/{titleId}/equip")
    public ApiResponse<Map<String, Object>> equip(@AuthenticationPrincipal CurrentAccount principal,
                                                  @PathVariable Integer titleId) {
        return ApiResponse.ok(progressionService.equipTitle(principal, titleId));
    }
}
