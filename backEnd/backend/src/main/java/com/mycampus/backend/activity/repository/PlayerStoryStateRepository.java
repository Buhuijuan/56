package com.mycampus.backend.activity.repository;

import com.mycampus.backend.activity.entity.PlayerStoryState;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerStoryStateRepository extends JpaRepository<PlayerStoryState, Long> {
}
