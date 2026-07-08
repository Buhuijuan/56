package com.mycampus.backend.player.repository;

import com.mycampus.backend.player.entity.PlayerStat;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerStatRepository extends JpaRepository<PlayerStat, Long> {
}
