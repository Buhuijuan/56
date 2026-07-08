package com.mycampus.backend.progression.repository;

import com.mycampus.backend.progression.entity.PlayerGrowthState;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerGrowthStateRepository extends JpaRepository<PlayerGrowthState, Long> {
}
