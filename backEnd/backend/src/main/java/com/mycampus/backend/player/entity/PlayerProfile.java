package com.mycampus.backend.player.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.time.LocalDateTime;

@Entity
@Table(name = "player_profile")
public class PlayerProfile {

    @Id
    @Column(name = "role_id")
    private Long roleId;

    @Column(name = "nickname", length = 64)
    private String nickname;

    @Column(name = "level", nullable = false)
    private Integer level;

    @Column(name = "exp", nullable = false)
    private Integer exp;

    @Column(name = "coin", nullable = false)
    private Integer coin;

    @Column(name = "current_title_id")
    private Long currentTitleId;

    @Column(name = "bike_unlocked", nullable = false)
    private Integer bikeUnlocked;

    @Column(name = "first_login_at")
    private LocalDateTime firstLoginAt;

    @Column(name = "last_login_at")
    private LocalDateTime lastLoginAt;

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

    public Long getCurrentTitleId() {
        return currentTitleId;
    }

    public void setCurrentTitleId(Long currentTitleId) {
        this.currentTitleId = currentTitleId;
    }

    public Integer getBikeUnlocked() {
        return bikeUnlocked;
    }

    public void setBikeUnlocked(Integer bikeUnlocked) {
        this.bikeUnlocked = bikeUnlocked;
    }

    public LocalDateTime getFirstLoginAt() {
        return firstLoginAt;
    }

    public void setFirstLoginAt(LocalDateTime firstLoginAt) {
        this.firstLoginAt = firstLoginAt;
    }

    public LocalDateTime getLastLoginAt() {
        return lastLoginAt;
    }

    public void setLastLoginAt(LocalDateTime lastLoginAt) {
        this.lastLoginAt = lastLoginAt;
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
