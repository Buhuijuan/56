package com.mycampus.backend.player.repository;

import com.mycampus.backend.player.entity.PlayerProfile;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerProfileRepository extends JpaRepository<PlayerProfile, Long> {
}
