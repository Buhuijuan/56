package com.mycampus.backend.auth.repository;

import com.mycampus.backend.auth.entity.Account;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface AccountRepository extends JpaRepository<Account, Long> {
    Optional<Account> findByMailbox(String mailbox);
    Optional<Account> findByAccountCode(String accountCode);
    boolean existsByMailbox(String mailbox);
}
