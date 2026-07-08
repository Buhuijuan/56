package com.mycampus.backend.activity.repository;

import com.mycampus.backend.activity.entity.StoryRecordEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface StoryRecordRepository extends JpaRepository<StoryRecordEntity, Long> {
    List<StoryRecordEntity> findByRoleIdOrderByCreatedAtDesc(Long roleId);
    List<StoryRecordEntity> findByUploadedTrueOrderByCreatedAtDesc();
    void deleteByRoleId(Long roleId);
}
