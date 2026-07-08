package com.mycampus.backend.task.entity;

import jakarta.persistence.*;

import java.math.BigDecimal;
import java.time.LocalDateTime;

@Entity
@Table(name = "player_checkin_record")
public class PlayerCheckinRecord {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "player_checkin_record_id")
    private Long playerCheckinRecordId;

    @Column(name = "role_id", nullable = false)
    private Long roleId;

    @Column(name = "task_id")
    private Long taskId;

    @Column(name = "building_location_id")
    private Long buildingLocationId;

    @Column(name = "current_pos_x", nullable = false, precision = 10, scale = 2)
    private BigDecimal currentPosX;

    @Column(name = "current_pos_y", nullable = false, precision = 10, scale = 2)
    private BigDecimal currentPosY;

    @Column(name = "current_pos_z", nullable = false, precision = 10, scale = 2)
    private BigDecimal currentPosZ;

    @Column(name = "distance_to_target", precision = 10, scale = 2)
    private BigDecimal distanceToTarget;

    @Column(name = "is_success", nullable = false)
    private Integer isSuccess;

    @Column(name = "trigger_type", nullable = false, length = 32)
    private String triggerType;

    @Column(name = "checked_at", nullable = false)
    private LocalDateTime checkedAt;

    public Long getPlayerCheckinRecordId() {
        return playerCheckinRecordId;
    }

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Long getTaskId() {
        return taskId;
    }

    public void setTaskId(Long taskId) {
        this.taskId = taskId;
    }

    public Long getBuildingLocationId() {
        return buildingLocationId;
    }

    public void setBuildingLocationId(Long buildingLocationId) {
        this.buildingLocationId = buildingLocationId;
    }

    public BigDecimal getCurrentPosX() {
        return currentPosX;
    }

    public void setCurrentPosX(BigDecimal currentPosX) {
        this.currentPosX = currentPosX;
    }

    public BigDecimal getCurrentPosY() {
        return currentPosY;
    }

    public void setCurrentPosY(BigDecimal currentPosY) {
        this.currentPosY = currentPosY;
    }

    public BigDecimal getCurrentPosZ() {
        return currentPosZ;
    }

    public void setCurrentPosZ(BigDecimal currentPosZ) {
        this.currentPosZ = currentPosZ;
    }

    public BigDecimal getDistanceToTarget() {
        return distanceToTarget;
    }

    public void setDistanceToTarget(BigDecimal distanceToTarget) {
        this.distanceToTarget = distanceToTarget;
    }

    public Integer getIsSuccess() {
        return isSuccess;
    }

    public void setIsSuccess(Integer isSuccess) {
        this.isSuccess = isSuccess;
    }

    public String getTriggerType() {
        return triggerType;
    }

    public void setTriggerType(String triggerType) {
        this.triggerType = triggerType;
    }

    public LocalDateTime getCheckedAt() {
        return checkedAt;
    }

    public void setCheckedAt(LocalDateTime checkedAt) {
        this.checkedAt = checkedAt;
    }
}
