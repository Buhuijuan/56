package com.mycampus.backend.task.entity;

import jakarta.persistence.*;

import java.time.LocalDateTime;

@Entity
@Table(name = "player_task", uniqueConstraints = {
        @UniqueConstraint(name = "uk_player_task_role_task", columnNames = {"role_id", "task_code"})
})
public class PlayerTask {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "player_task_id")
    private Long playerTaskId;

    @Column(name = "role_id", nullable = false)
    private Long roleId;

    @Column(name = "task_id", nullable = false)
    private Long taskId;

    @Column(name = "task_code", nullable = false, length = 50)
    private String taskCode;

    @Column(name = "task_status", nullable = false, length = 24)
    private String taskStatus;

    @Column(name = "progress_current", nullable = false)
    private Integer progressCurrent;

    @Column(name = "progress_target", nullable = false)
    private Integer progressTarget;

    @Column(name = "current_step", nullable = false)
    private Integer currentStep;

    @Column(name = "accepted_at")
    private LocalDateTime acceptedAt;

    @Column(name = "completed_at")
    private LocalDateTime completedAt;

    @Column(name = "reward_claimed_at")
    private LocalDateTime rewardClaimedAt;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    public void touch() {
        updatedAt = LocalDateTime.now();
        if (createdAt == null) {
            createdAt = updatedAt;
        }
    }

    public Long getPlayerTaskId() {
        return playerTaskId;
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

    public String getTaskCode() {
        return taskCode;
    }

    public void setTaskCode(String taskCode) {
        this.taskCode = taskCode;
    }

    public String getTaskStatus() {
        return taskStatus;
    }

    public void setTaskStatus(String taskStatus) {
        this.taskStatus = taskStatus;
    }

    public Integer getProgressCurrent() {
        return progressCurrent;
    }

    public void setProgressCurrent(Integer progressCurrent) {
        this.progressCurrent = progressCurrent;
    }

    public Integer getProgressTarget() {
        return progressTarget;
    }

    public void setProgressTarget(Integer progressTarget) {
        this.progressTarget = progressTarget;
    }

    public Integer getCurrentStep() {
        return currentStep;
    }

    public void setCurrentStep(Integer currentStep) {
        this.currentStep = currentStep;
    }

    public LocalDateTime getAcceptedAt() {
        return acceptedAt;
    }

    public void setAcceptedAt(LocalDateTime acceptedAt) {
        this.acceptedAt = acceptedAt;
    }

    public LocalDateTime getCompletedAt() {
        return completedAt;
    }

    public void setCompletedAt(LocalDateTime completedAt) {
        this.completedAt = completedAt;
    }

    public LocalDateTime getRewardClaimedAt() {
        return rewardClaimedAt;
    }

    public void setRewardClaimedAt(LocalDateTime rewardClaimedAt) {
        this.rewardClaimedAt = rewardClaimedAt;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }
}
