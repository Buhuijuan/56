package com.mycampus.backend.api.dto.growthcompat;

public class TaskStateCompatDto {

    private Long taskId;
    private String taskCode;
    private String taskName;
    private String taskStatus;
    private Integer progressCurrent;
    private Integer progressTarget;

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

    public String getTaskName() {
        return taskName;
    }

    public void setTaskName(String taskName) {
        this.taskName = taskName;
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
}
