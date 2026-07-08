package com.mycampus.backend.api.dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;

public final class PlayerDtos {

    private PlayerDtos() {
    }

    public record CreateRoleRequest(Long schoolId,
                                    String campusName,
                                    @NotBlank String nickName,
                                    @NotNull Integer characterId) {
    }

    public record SwitchRoleRequest(@NotNull Long roleId) {
    }

    public record UpdateMailboxRequest(@Email String mailbox) {
    }

    public record UpdatePasswordRequest(@NotBlank @Size(min = 8, max = 32) String newPassword) {
    }

    public record HeartbeatRequest(Integer elapsedSeconds) {
    }
}
