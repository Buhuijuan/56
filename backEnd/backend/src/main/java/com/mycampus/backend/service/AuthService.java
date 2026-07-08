package com.mycampus.backend.service;

import com.mycampus.backend.api.dto.AuthDtos;
import com.mycampus.backend.auth.entity.Account;
import com.mycampus.backend.auth.entity.EmailVerificationCode;
import com.mycampus.backend.auth.repository.AccountRepository;
import com.mycampus.backend.auth.repository.EmailVerificationCodeRepository;
import com.mycampus.backend.common.AppException;
import com.mycampus.backend.mail.MailService;
import com.mycampus.backend.security.JwtService;
import org.springframework.http.HttpStatus;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

@Service
public class AuthService {

    private final AccountRepository accountRepository;
    private final EmailVerificationCodeRepository emailVerificationCodeRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtService jwtService;
    private final MailService mailService;

    public AuthService(AccountRepository accountRepository,
                       EmailVerificationCodeRepository emailVerificationCodeRepository,
                       PasswordEncoder passwordEncoder,
                       JwtService jwtService,
                       MailService mailService) {
        this.accountRepository = accountRepository;
        this.emailVerificationCodeRepository = emailVerificationCodeRepository;
        this.passwordEncoder = passwordEncoder;
        this.jwtService = jwtService;
        this.mailService = mailService;
    }

    @Transactional
    public Map<String, Object> sendCode(AuthDtos.SendCodeRequest request) {
        String mailbox = normalizeMailbox(request.mailbox());
        String code = String.valueOf(ThreadLocalRandom.current().nextInt(100000, 1000000));

        EmailVerificationCode record = new EmailVerificationCode();
        record.setMailbox(mailbox);
        record.setVerificationCode(code);
        record.setVerificationExpiresAt(LocalDateTime.now().plusMinutes(10));
        record.setPurpose("auth");
        emailVerificationCodeRepository.save(record);

        mailService.sendVerificationCode(mailbox, code);
        return Map.of("mailbox", mailbox, "expiresInSeconds", 600, "sent", true);
    }

    @Transactional
    public Map<String, Object> register(AuthDtos.RegisterRequest request) {
        String mailbox = normalizeMailbox(request.mailbox());
        Account account = accountRepository.findByMailbox(mailbox).orElseGet(Account::new);
        boolean isNew = account.getMailbox() == null;

        if (isNew) {
            account.setAccountCode(generateUniqueAccountCode());
            account.setMailbox(mailbox);
        } else if (account.getPasswordHash() != null && !account.getPasswordHash().isBlank()) {
            throw new AppException(HttpStatus.BAD_REQUEST, "该邮箱已注册");
        }

        EmailVerificationCode codeRecord = validateCode(mailbox, request.verificationCode());
        account.setPasswordHash(passwordEncoder.encode(request.password()));
        accountRepository.save(account);
        markCodeUsed(codeRecord);

        Map<String, Object> response = new LinkedHashMap<>();
        response.put("accountId", account.getId());
        response.put("accountCode", account.getAccountCode());
        response.put("mailbox", account.getMailbox());
        response.put("currentRoleId", account.getCurrentRoleId());
        response.put("token", jwtService.createToken(account.getId(), account.getMailbox()));
        return response;
    }

    public Map<String, Object> login(AuthDtos.LoginRequest request) {
        String mailbox = normalizeMailbox(request.mailbox());
        Account account = accountRepository.findByMailbox(mailbox)
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "账号不存在"));

        if (!passwordEncoder.matches(request.password(), account.getPasswordHash())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "密码错误");
        }

        Map<String, Object> response = new LinkedHashMap<>();
        response.put("accountId", account.getId());
        response.put("accountCode", account.getAccountCode());
        response.put("mailbox", account.getMailbox());
        response.put("currentRoleId", account.getCurrentRoleId());
        response.put("token", jwtService.createToken(account.getId(), account.getMailbox()));
        return response;
    }

    @Transactional
    public Map<String, Object> resetPassword(AuthDtos.ResetPasswordRequest request) {
        String mailbox = normalizeMailbox(request.mailbox());
        Account account = accountRepository.findByMailbox(mailbox)
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "账号不存在"));

        EmailVerificationCode codeRecord = validateCode(mailbox, request.verificationCode());
        account.setPasswordHash(passwordEncoder.encode(request.newPassword()));
        accountRepository.save(account);
        markCodeUsed(codeRecord);
        return Map.of("mailbox", account.getMailbox(), "reset", true);
    }

    private EmailVerificationCode validateCode(String mailbox, String code) {
        EmailVerificationCode record = emailVerificationCodeRepository
                .findFirstByMailboxAndVerificationCodeAndIsUsedOrderByCreatedAtDesc(mailbox, code, 0)
                .orElseThrow(() -> new AppException(HttpStatus.BAD_REQUEST, "验证码错误"));

        if (record.getVerificationExpiresAt().isBefore(LocalDateTime.now())) {
            throw new AppException(HttpStatus.BAD_REQUEST, "验证码已过期");
        }
        return record;
    }

    private void markCodeUsed(EmailVerificationCode record) {
        record.setIsUsed(1);
        emailVerificationCodeRepository.save(record);
    }

    private String normalizeMailbox(String mailbox) {
        return mailbox == null ? null : mailbox.trim().toLowerCase();
    }

    private String generateUniqueAccountCode() {
        for (int attempt = 0; attempt < 20; attempt++) {
            String candidate = String.valueOf(ThreadLocalRandom.current().nextInt(100000, 999999));
            if (accountRepository.findByAccountCode(candidate).isEmpty()) {
                return candidate;
            }
        }
        throw new AppException(HttpStatus.INTERNAL_SERVER_ERROR, "账号编号生成失败，请稍后重试");
    }
}
