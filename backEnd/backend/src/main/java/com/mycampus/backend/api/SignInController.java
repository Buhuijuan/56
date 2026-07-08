package com.mycampus.backend.api;

import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.SignInService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/signin")
public class SignInController {

    private final SignInService signInService;

    public SignInController(SignInService signInService) {
        this.signInService = signInService;
    }

    @GetMapping
    public ApiResponse<Map<String, Object>> status(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(signInService.getStatus(principal));
    }

    @PostMapping("/daily")
    public ApiResponse<Map<String, Object>> daily(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(signInService.dailySign(principal));
    }

    @PostMapping("/online/{awardId}/claim")
    public ApiResponse<Map<String, Object>> claimOnline(@AuthenticationPrincipal CurrentAccount principal,
                                                        @PathVariable Integer awardId) {
        return ApiResponse.ok(signInService.claimOnline(principal, awardId));
    }

    @PostMapping("/total/{awardId}/claim")
    public ApiResponse<Map<String, Object>> claimTotal(@AuthenticationPrincipal CurrentAccount principal,
                                                       @PathVariable Integer awardId) {
        return ApiResponse.ok(signInService.claimTotal(principal, awardId));
    }
}
