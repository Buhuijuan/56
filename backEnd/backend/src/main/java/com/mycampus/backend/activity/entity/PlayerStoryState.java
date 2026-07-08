package com.mycampus.backend.activity.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.time.LocalDate;

@Entity
@Table(name = "player_story_state")
public class PlayerStoryState {

    @Id
    @jakarta.persistence.Column(name = "role_id")
    private Long roleId;

    @jakarta.persistence.Column(name = "event_id")
    private String eventId;

    @jakarta.persistence.Column(name = "has_finished")
    private Boolean hasFinished;

    @jakarta.persistence.Column(name = "reward_claimed")
    private Boolean rewardClaimed;

    @jakarta.persistence.Column(name = "last_play_date")
    private LocalDate lastPlayDate;

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

    public Boolean getHasFinished() {
        return hasFinished;
    }

    public void setHasFinished(Boolean hasFinished) {
        this.hasFinished = hasFinished;
    }

    public Boolean getRewardClaimed() {
        return rewardClaimed;
    }

    public void setRewardClaimed(Boolean rewardClaimed) {
        this.rewardClaimed = rewardClaimed;
    }

    public LocalDate getLastPlayDate() {
        return lastPlayDate;
    }

    public void setLastPlayDate(LocalDate lastPlayDate) {
        this.lastPlayDate = lastPlayDate;
    }
}
