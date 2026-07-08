package com.mycampus.backend.signin.entity;

import com.mycampus.backend.persistence.IntegerSetConverter;
import jakarta.persistence.*;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.LinkedHashSet;
import java.util.Set;

@Entity
@Table(name = "player_signin_state")
public class PlayerSignInState {

    @Id
    private Long roleId;

    @Column(nullable = false)
    private Integer todayOnlineSeconds;

    @Column(nullable = false)
    private Boolean dailySigned;

    private LocalDate lastSignInDate;

    @Column(nullable = false)
    private Integer continuousSignDays;

    @Column(nullable = false)
    private Integer currentWeekIndex;

    @Column(nullable = false)
    private Integer totalLoginDays;

    private LocalDateTime lastHeartbeatAt;

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> onlineRewardClaimed = new LinkedHashSet<>();

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> dailyRewardClaimed = new LinkedHashSet<>();

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> totalRewardClaimed = new LinkedHashSet<>();

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Integer getTodayOnlineSeconds() {
        return todayOnlineSeconds;
    }

    public void setTodayOnlineSeconds(Integer todayOnlineSeconds) {
        this.todayOnlineSeconds = todayOnlineSeconds;
    }

    public Boolean getDailySigned() {
        return dailySigned;
    }

    public void setDailySigned(Boolean dailySigned) {
        this.dailySigned = dailySigned;
    }

    public LocalDate getLastSignInDate() {
        return lastSignInDate;
    }

    public void setLastSignInDate(LocalDate lastSignInDate) {
        this.lastSignInDate = lastSignInDate;
    }

    public Integer getContinuousSignDays() {
        return continuousSignDays;
    }

    public void setContinuousSignDays(Integer continuousSignDays) {
        this.continuousSignDays = continuousSignDays;
    }

    public Integer getCurrentWeekIndex() {
        return currentWeekIndex;
    }

    public void setCurrentWeekIndex(Integer currentWeekIndex) {
        this.currentWeekIndex = currentWeekIndex;
    }

    public Integer getTotalLoginDays() {
        return totalLoginDays;
    }

    public void setTotalLoginDays(Integer totalLoginDays) {
        this.totalLoginDays = totalLoginDays;
    }

    public LocalDateTime getLastHeartbeatAt() {
        return lastHeartbeatAt;
    }

    public void setLastHeartbeatAt(LocalDateTime lastHeartbeatAt) {
        this.lastHeartbeatAt = lastHeartbeatAt;
    }

    public Set<Integer> getOnlineRewardClaimed() {
        return onlineRewardClaimed;
    }

    public Set<Integer> getDailyRewardClaimed() {
        return dailyRewardClaimed;
    }

    public Set<Integer> getTotalRewardClaimed() {
        return totalRewardClaimed;
    }
}
