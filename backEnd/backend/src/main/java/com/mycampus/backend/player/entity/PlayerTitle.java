package com.mycampus.backend.player.entity;

import jakarta.persistence.*;

import java.time.LocalDateTime;

@Entity
@Table(name = "player_title", uniqueConstraints = {
        @UniqueConstraint(name = "uk_player_title_role_title", columnNames = {"role_id", "title_id"})
})
public class PlayerTitle {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "player_title_id")
    private Long playerTitleId;

    @Column(name = "role_id", nullable = false)
    private Long roleId;

    @Column(name = "title_id", nullable = false)
    private Long titleId;

    @Column(name = "unlocked_at", nullable = false)
    private LocalDateTime unlockedAt;

    @Column(name = "is_equipped", nullable = false)
    private Integer isEquipped;

    @Column(name = "source_type", length = 32)
    private String sourceType;

    @Column(name = "source_ref_id")
    private Long sourceRefId;

    public Long getPlayerTitleId() {
        return playerTitleId;
    }

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Long getTitleId() {
        return titleId;
    }

    public void setTitleId(Long titleId) {
        this.titleId = titleId;
    }

    public LocalDateTime getUnlockedAt() {
        return unlockedAt;
    }

    public void setUnlockedAt(LocalDateTime unlockedAt) {
        this.unlockedAt = unlockedAt;
    }

    public Integer getIsEquipped() {
        return isEquipped;
    }

    public void setIsEquipped(Integer isEquipped) {
        this.isEquipped = isEquipped;
    }

    public String getSourceType() {
        return sourceType;
    }

    public void setSourceType(String sourceType) {
        this.sourceType = sourceType;
    }

    public Long getSourceRefId() {
        return sourceRefId;
    }

    public void setSourceRefId(Long sourceRefId) {
        this.sourceRefId = sourceRefId;
    }
}
