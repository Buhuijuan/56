package com.mycampus.backend.activity.repository;

import com.mycampus.backend.activity.entity.PlayerClockInState;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerClockInStateRepository extends JpaRepository<PlayerClockInState, Long> {
}
