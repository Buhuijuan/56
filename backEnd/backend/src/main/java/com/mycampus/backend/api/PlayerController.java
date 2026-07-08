package com.mycampus.backend.api;

import com.mycampus.backend.api.dto.PlayerDtos;
import com.mycampus.backend.common.ApiResponse;
import com.mycampus.backend.player.entity.Role;
import com.mycampus.backend.security.CurrentAccount;
import com.mycampus.backend.service.PlayerService;
import jakarta.validation.Valid;
import org.springframework.http.MediaType;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.Map;

@RestController
@RequestMapping("/api/player")
public class PlayerController {

    private final PlayerService playerService;

    public PlayerController(PlayerService playerService) {
        this.playerService = playerService;
    }

    @GetMapping("/me")
    public ApiResponse<Map<String, Object>> me(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(playerService.getMe(principal));
    }

    @PostMapping("/roles")
    public ApiResponse<Role> createRole(@AuthenticationPrincipal CurrentAccount principal,
                                        @Valid @RequestBody PlayerDtos.CreateRoleRequest request) {
        return ApiResponse.ok(playerService.createRole(principal, request));
    }

    @PutMapping("/current-role")
    public ApiResponse<Map<String, Object>> switchRole(@AuthenticationPrincipal CurrentAccount principal,
                                                       @Valid @RequestBody PlayerDtos.SwitchRoleRequest request) {
        return ApiResponse.ok(playerService.switchRole(principal, request));
    }

    @PutMapping("/profile/mailbox")
    public ApiResponse<Map<String, Object>> updateMailbox(@AuthenticationPrincipal CurrentAccount principal,
                                                          @Valid @RequestBody PlayerDtos.UpdateMailboxRequest request) {
        return ApiResponse.ok(playerService.updateMailbox(principal, request));
    }

    @PutMapping("/profile/password")
    public ApiResponse<Map<String, Object>> updatePassword(@AuthenticationPrincipal CurrentAccount principal,
                                                           @Valid @RequestBody PlayerDtos.UpdatePasswordRequest request) {
        return ApiResponse.ok(playerService.updatePassword(principal, request));
    }

    @PostMapping(value = "/current-role/avatar", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    public ApiResponse<Map<String, Object>> uploadCurrentRoleAvatar(@AuthenticationPrincipal CurrentAccount principal,
                                                                    @RequestPart("file") MultipartFile file) {
        return ApiResponse.ok(playerService.uploadCurrentRoleAvatar(principal, file), "头像上传成功");
    }

    @DeleteMapping("/roles/{roleId}")
    public ApiResponse<Map<String, Object>> deleteRole(@AuthenticationPrincipal CurrentAccount principal,
                                                       @PathVariable Long roleId) {
        return ApiResponse.ok(playerService.deleteRole(principal, roleId), "角色删除成功");
    }

    @DeleteMapping("/account")
    public ApiResponse<Map<String, Object>> deleteAccount(@AuthenticationPrincipal CurrentAccount principal) {
        return ApiResponse.ok(playerService.deleteAccount(principal), "账号删除成功");
    }

    @PostMapping("/heartbeat")
    public ApiResponse<Map<String, Object>> heartbeat(@AuthenticationPrincipal CurrentAccount principal,
                                                      @RequestBody(required = false) PlayerDtos.HeartbeatRequest request) {
        PlayerDtos.HeartbeatRequest safeRequest = request == null ? new PlayerDtos.HeartbeatRequest(30) : request;
        return ApiResponse.ok(playerService.heartbeat(principal, safeRequest));
    }
}
