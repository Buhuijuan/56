package com.mycampus.backend.api.dto.growthcompat;

public class GrowthProfileCompatDto {

    private Long roleId;
    private String nickname;
    private Integer level;
    private Integer exp;
    private Integer coin;
    private Integer bikeUnlocked;
    private Integer currentLevelTotalExp;
    private Integer nextLevelNeedExp;
    private Integer nextLevelTotalExp;
    private Integer remainExpToNextLevel;
    private Boolean maxLevel;

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public String getNickname() {
        return nickname;
    }

    public void setNickname(String nickname) {
        this.nickname = nickname;
    }

    public Integer getLevel() {
        return level;
    }

    public void setLevel(Integer level) {
        this.level = level;
    }

    public Integer getExp() {
        return exp;
    }

    public void setExp(Integer exp) {
        this.exp = exp;
    }

    public Integer getCoin() {
        return coin;
    }

    public void setCoin(Integer coin) {
        this.coin = coin;
    }

    public Integer getBikeUnlocked() {
        return bikeUnlocked;
    }

    public void setBikeUnlocked(Integer bikeUnlocked) {
        this.bikeUnlocked = bikeUnlocked;
    }

    public Integer getCurrentLevelTotalExp() {
        return currentLevelTotalExp;
    }

    public void setCurrentLevelTotalExp(Integer currentLevelTotalExp) {
        this.currentLevelTotalExp = currentLevelTotalExp;
    }

    public Integer getNextLevelNeedExp() {
        return nextLevelNeedExp;
    }

    public void setNextLevelNeedExp(Integer nextLevelNeedExp) {
        this.nextLevelNeedExp = nextLevelNeedExp;
    }

    public Integer getNextLevelTotalExp() {
        return nextLevelTotalExp;
    }

    public void setNextLevelTotalExp(Integer nextLevelTotalExp) {
        this.nextLevelTotalExp = nextLevelTotalExp;
    }

    public Integer getRemainExpToNextLevel() {
        return remainExpToNextLevel;
    }

    public void setRemainExpToNextLevel(Integer remainExpToNextLevel) {
        this.remainExpToNextLevel = remainExpToNextLevel;
    }

    public Boolean getMaxLevel() {
        return maxLevel;
    }

    public void setMaxLevel(Boolean maxLevel) {
        this.maxLevel = maxLevel;
    }
}
