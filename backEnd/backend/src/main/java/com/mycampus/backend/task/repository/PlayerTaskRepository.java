package com.mycampus.backend.task.repository;

import com.mycampus.backend.task.entity.PlayerTask;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.List;
import java.util.Optional;

public interface PlayerTaskRepository extends JpaRepository<PlayerTask, Long> {
    List<PlayerTask> findByRoleId(Long roleId);
    void deleteByRoleId(Long roleId);

    @Query("""
            select pt from PlayerTask pt
            join TaskConfigEntity tc on tc.taskId = pt.taskId
            where pt.roleId = :roleId and tc.taskCode = :taskCode
            """)
    Optional<PlayerTask> findByRoleIdAndTaskCode(@Param("roleId") Long roleId, @Param("taskCode") String taskCode);

    @Query("""
            select pt from PlayerTask pt
            join TaskConfigEntity tc on tc.taskId = pt.taskId
            where pt.roleId = :roleId and tc.chapterNo = :chapterNo
            order by tc.stepNo asc
            """)
    List<PlayerTask> findByRoleIdAndChapterNo(@Param("roleId") Long roleId, @Param("chapterNo") Integer chapterNo);
}
