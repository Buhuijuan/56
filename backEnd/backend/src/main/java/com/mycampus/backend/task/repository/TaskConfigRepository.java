package com.mycampus.backend.task.repository;

import com.mycampus.backend.task.entity.TaskConfigEntity;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface TaskConfigRepository extends JpaRepository<TaskConfigEntity, Long> {
    Optional<TaskConfigEntity> findByTaskCode(String taskCode);
    List<TaskConfigEntity> findByStatusOrderByChapterNoAscStepNoAsc(Integer status);
    List<TaskConfigEntity> findAllByOrderByChapterNoAscStepNoAsc();
}
