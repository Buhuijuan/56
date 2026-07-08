package com.mycampus.backend.task.repository;

import com.mycampus.backend.task.entity.PlayerCheckinRecord;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface PlayerCheckinRecordRepository extends JpaRepository<PlayerCheckinRecord, Long> {
    void deleteByRoleId(Long roleId);

    @Query("""
            select count(distinct r.buildingLocationId) from PlayerCheckinRecord r
            where r.roleId = :roleId and r.triggerType = 'photo_checkin' and r.isSuccess = 1
            """)
    long countDistinctSuccessfulPhotoCheckins(@Param("roleId") Long roleId);

    @Query("""
            select count(distinct r.buildingLocationId) from PlayerCheckinRecord r
            where r.roleId = :roleId and r.taskId = :taskId and r.triggerType = 'photo_checkin' and r.isSuccess = 1
            """)
    long countDistinctSuccessfulPhotoCheckinsForTask(@Param("roleId") Long roleId, @Param("taskId") Long taskId);

    boolean existsByRoleIdAndTaskIdAndBuildingLocationIdAndTriggerTypeAndIsSuccess(
            Long roleId,
            Long taskId,
            Long buildingLocationId,
            String triggerType,
            Integer isSuccess
    );
}
