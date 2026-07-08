package com.mycampus.backend.progression.repository;

import com.mycampus.backend.progression.entity.PlayerLevelState;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerLevelStateRepository extends JpaRepository<PlayerLevelState, Long> {
}
