package com.mycampus.backend.auth.repository;

import com.mycampus.backend.auth.entity.EmailVerificationCode;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface EmailVerificationCodeRepository extends JpaRepository<EmailVerificationCode, Long> {
    Optional<EmailVerificationCode> findFirstByMailboxAndIsUsedOrderByCreatedAtDesc(
            String mailbox,
            Integer isUsed
    );

    Optional<EmailVerificationCode> findFirstByMailboxAndVerificationCodeAndIsUsedOrderByCreatedAtDesc(
            String mailbox,
            String verificationCode,
            Integer isUsed
    );

    void deleteByMailbox(String mailbox);
}
