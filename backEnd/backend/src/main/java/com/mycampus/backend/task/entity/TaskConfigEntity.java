package com.mycampus.backend.task.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "task_config")
public class TaskConfigEntity {

    @Id
    @Column(name = "task_id")
    private Long taskId;

    @Column(name = "task_code", nullable = false, unique = true, length = 64)
    private String taskCode;

    @Column(name = "task_name", nullable = false, length = 128)
    private String taskName;

    @Column(name = "task_type", nullable = false, length = 32)
    private String taskType;

    @Column(name = "chapter_no", nullable = false)
    private Integer chapterNo;

    @Column(name = "step_no", nullable = false)
    private Integer stepNo;

    @Column(name = "category", length = 32)
    private String category;

    @Column(name = "trigger_type", length = 64)
    private String triggerType;

    @Column(name = "pre_task_code", length = 64)
    private String preTaskCode;

    @Column(name = "target_type", length = 64)
    private String targetType;

    @Column(name = "target_id")
    private Long targetId;

    @Column(name = "target_count", nullable = false)
    private Integer targetCount;

    @Column(name = "reward_exp", nullable = false)
    private Integer rewardExp;

    @Column(name = "reward_coin", nullable = false)
    private Integer rewardCoin;

    @Column(name = "reward_unlock_feature", length = 64)
    private String rewardUnlockFeature;

    @Column(name = "description", columnDefinition = "longtext")
    private String description;

    @Column(name = "elf_npc_name", length = 64)
    private String elfNpcName;

    @Column(name = "elf_avatar_key", length = 64)
    private String elfAvatarKey;

    @Column(name = "elf_start_prompt_json", columnDefinition = "longtext")
    private String elfStartPromptJson;

    @Column(name = "elf_progress_prompt_json", columnDefinition = "longtext")
    private String elfProgressPromptJson;

    @Column(name = "elf_complete_prompt_json", columnDefinition = "longtext")
    private String elfCompletePromptJson;

    @Column(name = "status", nullable = false)
    private Integer status;

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

    public String getTaskType() {
        return taskType;
    }

    public void setTaskType(String taskType) {
        this.taskType = taskType;
    }

    public Integer getChapterNo() {
        return chapterNo;
    }

    public void setChapterNo(Integer chapterNo) {
        this.chapterNo = chapterNo;
    }

    public Integer getStepNo() {
        return stepNo;
    }

    public void setStepNo(Integer stepNo) {
        this.stepNo = stepNo;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public String getTriggerType() {
        return triggerType;
    }

    public void setTriggerType(String triggerType) {
        this.triggerType = triggerType;
    }

    public String getPreTaskCode() {
        return preTaskCode;
    }

    public void setPreTaskCode(String preTaskCode) {
        this.preTaskCode = preTaskCode;
    }

    public String getTargetType() {
        return targetType;
    }

    public void setTargetType(String targetType) {
        this.targetType = targetType;
    }

    public Long getTargetId() {
        return targetId;
    }

    public void setTargetId(Long targetId) {
        this.targetId = targetId;
    }

    public Integer getTargetCount() {
        return targetCount;
    }

    public void setTargetCount(Integer targetCount) {
        this.targetCount = targetCount;
    }

    public Integer getRewardExp() {
        return rewardExp;
    }

    public void setRewardExp(Integer rewardExp) {
        this.rewardExp = rewardExp;
    }

    public Integer getRewardCoin() {
        return rewardCoin;
    }

    public void setRewardCoin(Integer rewardCoin) {
        this.rewardCoin = rewardCoin;
    }

    public String getRewardUnlockFeature() {
        return rewardUnlockFeature;
    }

    public void setRewardUnlockFeature(String rewardUnlockFeature) {
        this.rewardUnlockFeature = rewardUnlockFeature;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public String getElfNpcName() {
        return elfNpcName;
    }

    public void setElfNpcName(String elfNpcName) {
        this.elfNpcName = elfNpcName;
    }

    public String getElfAvatarKey() {
        return elfAvatarKey;
    }

    public void setElfAvatarKey(String elfAvatarKey) {
        this.elfAvatarKey = elfAvatarKey;
    }

    public String getElfStartPromptJson() {
        return elfStartPromptJson;
    }

    public void setElfStartPromptJson(String elfStartPromptJson) {
        this.elfStartPromptJson = elfStartPromptJson;
    }

    public String getElfProgressPromptJson() {
        return elfProgressPromptJson;
    }

    public void setElfProgressPromptJson(String elfProgressPromptJson) {
        this.elfProgressPromptJson = elfProgressPromptJson;
    }

    public String getElfCompletePromptJson() {
        return elfCompletePromptJson;
    }

    public void setElfCompletePromptJson(String elfCompletePromptJson) {
        this.elfCompletePromptJson = elfCompletePromptJson;
    }

    public Integer getStatus() {
        return status;
    }

    public void setStatus(Integer status) {
        this.status = status;
    }
}
