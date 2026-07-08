package com.mycampus.backend.signin.repository;

import com.mycampus.backend.signin.entity.PlayerSignInState;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerSignInStateRepository extends JpaRepository<PlayerSignInState, Long> {
}
