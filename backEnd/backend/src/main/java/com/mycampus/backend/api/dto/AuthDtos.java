package com.mycampus.backend.api.dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;

public final class AuthDtos {

    private AuthDtos() {
    }

    public record RegisterRequest(@NotBlank(message = "请输入邮箱。")
                                  @Email(message = "邮箱格式不正确。") String mailbox,
                                  @NotBlank(message = "请输入密码。")
                                  @Size(min = 8, max = 32, message = "密码长度必须在 8 到 32 位之间。") String password,
                                  @NotBlank(message = "请输入验证码。") String verificationCode) {
    }

    public record LoginRequest(@NotBlank(message = "请输入邮箱。")
                               @Email(message = "邮箱格式不正确。") String mailbox,
                               @NotBlank(message = "请输入密码。") String password) {
    }

    public record SendCodeRequest(@NotBlank(message = "请输入邮箱。")
                                  @Email(message = "邮箱格式不正确。") String mailbox) {
    }

    public record ResetPasswordRequest(@NotBlank(message = "请输入邮箱。")
                                       @Email(message = "邮箱格式不正确。") String mailbox,
                                       @NotBlank(message = "请输入验证码。") String verificationCode,
                                       @NotBlank(message = "请输入新密码。")
                                       @Size(min = 8, max = 32, message = "新密码长度必须在 8 到 32 位之间。") String newPassword) {
    }
}
