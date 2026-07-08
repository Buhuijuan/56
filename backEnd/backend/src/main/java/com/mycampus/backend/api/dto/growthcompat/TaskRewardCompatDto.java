package com.mycampus.backend.api.dto.growthcompat;

public class TaskRewardCompatDto {

    private String taskCode;
    private String taskName;
    private Integer rewardExp;
    private Integer rewardCoin;
    private String unlockFeature;
    private Integer oldLevel;
    private Integer newLevel;
    private Integer oldExp;
    private Integer newExp;

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

    public String getUnlockFeature() {
        return unlockFeature;
    }

    public void setUnlockFeature(String unlockFeature) {
        this.unlockFeature = unlockFeature;
    }

    public Integer getOldLevel() {
        return oldLevel;
    }

    public void setOldLevel(Integer oldLevel) {
        this.oldLevel = oldLevel;
    }

    public Integer getNewLevel() {
        return newLevel;
    }

    public void setNewLevel(Integer newLevel) {
        this.newLevel = newLevel;
    }

    public Integer getOldExp() {
        return oldExp;
    }

    public void setOldExp(Integer oldExp) {
        this.oldExp = oldExp;
    }

    public Integer getNewExp() {
        return newExp;
    }

    public void setNewExp(Integer newExp) {
        this.newExp = newExp;
    }
}
