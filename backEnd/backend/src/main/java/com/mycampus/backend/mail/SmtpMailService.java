package com.mycampus.backend.mail;

import com.mycampus.backend.common.AppException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.mail.MailException;
import org.springframework.mail.SimpleMailMessage;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.stereotype.Service;

@Service
public class SmtpMailService implements MailService {

    private static final Logger log = LoggerFactory.getLogger(SmtpMailService.class);

    private final JavaMailSender mailSender;
    private final boolean enabled;
    private final boolean mockDelivery;
    private final String host;
    private final String username;
    private final String password;
    private final String fromAddress;
    private final String fromName;

    public SmtpMailService(JavaMailSender mailSender,
                           @Value("${app.mail.enabled:true}") boolean enabled,
                           @Value("${app.mail.mock-delivery:false}") boolean mockDelivery,
                           @Value("${spring.mail.host:}") String host,
                           @Value("${spring.mail.username:}") String username,
                           @Value("${spring.mail.password:}") String password,
                           @Value("${app.mail.from-address:}") String fromAddress,
                           @Value("${app.mail.from-name:MyCampus}") String fromName) {
        this.mailSender = mailSender;
        this.enabled = enabled;
        this.mockDelivery = mockDelivery;
        this.host = host == null ? "" : host.trim();
        this.username = username == null ? "" : username.trim();
        this.password = password == null ? "" : password.trim();
        this.fromAddress = fromAddress == null ? "" : fromAddress.trim();
        this.fromName = fromName;
    }

    @Override
    public void sendVerificationCode(String mailbox, String verificationCode) {
        if (mockDelivery) {
            log.info("Mock mail delivery enabled; verification code generated for {}", mailbox);
            return;
        }

        if (!enabled) {
            throw new AppException(HttpStatus.SERVICE_UNAVAILABLE, "邮件服务未启用，请先配置 SMTP。");
        }

        validateMailConfiguration();

        SimpleMailMessage message = new SimpleMailMessage();
        message.setFrom(fromAddress);
        message.setTo(mailbox);
        message.setSubject(fromName + " verification code");
        message.setText("""
                Your MyCampus verification code is: %s

                This code will expire in 10 minutes.
                If you did not request this code, please ignore this email.
                """.formatted(verificationCode));

        try {
            mailSender.send(message);
        } catch (MailException ex) {
            log.error("Failed to send verification email to {}", mailbox, ex);
            throw new AppException(
                    HttpStatus.SERVICE_UNAVAILABLE,
                    "验证码邮件发送失败，请检查 SMTP 配置后重试。"
            );
        }
    }

    private void validateMailConfiguration() {
        if (host.isBlank()) {
            throw new AppException(HttpStatus.SERVICE_UNAVAILABLE, "SMTP 主机未配置。");
        }
        if (username.isBlank()) {
            throw new AppException(HttpStatus.SERVICE_UNAVAILABLE, "SMTP 发件账号未配置。");
        }
        if (password.isBlank()) {
            throw new AppException(HttpStatus.SERVICE_UNAVAILABLE, "SMTP 密码或授权码未配置。");
        }
        if (fromAddress.isBlank()) {
            throw new AppException(HttpStatus.SERVICE_UNAVAILABLE, "SMTP 发件地址未配置。");
        }
    }
}
