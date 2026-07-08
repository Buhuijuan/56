package com.mycampus.backend.activity.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.time.LocalDate;

@Entity
@Table(name = "player_quiz_state")
public class PlayerQuizState {

    @Id
    @jakarta.persistence.Column(name = "role_id")
    private Long roleId;

    @jakarta.persistence.Column(name = "event_id")
    private String eventId;

    @jakarta.persistence.Column(name = "weekly_score")
    private Integer weeklyScore;

    @jakarta.persistence.Column(name = "last_play_date")
    private LocalDate lastPlayDate;

    @jakarta.persistence.Column(name = "has_played_today")
    private Boolean hasPlayedToday;

    @jakarta.persistence.Column(name = "weekly_reward_claimed")
    private Boolean weeklyRewardClaimed;

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public String getEventId() {
        return eventId;
    }

    public void setEventId(String eventId) {
        this.eventId = eventId;
    }

    public Integer getWeeklyScore() {
        return weeklyScore;
    }

    public void setWeeklyScore(Integer weeklyScore) {
        this.weeklyScore = weeklyScore;
    }

    public LocalDate getLastPlayDate() {
        return lastPlayDate;
    }

    public void setLastPlayDate(LocalDate lastPlayDate) {
        this.lastPlayDate = lastPlayDate;
    }

    public Boolean getHasPlayedToday() {
        return hasPlayedToday;
    }

    public void setHasPlayedToday(Boolean hasPlayedToday) {
        this.hasPlayedToday = hasPlayedToday;
    }

    public Boolean getWeeklyRewardClaimed() {
        return weeklyRewardClaimed;
    }

    public void setWeeklyRewardClaimed(Boolean weeklyRewardClaimed) {
        this.weeklyRewardClaimed = weeklyRewardClaimed;
    }
}
