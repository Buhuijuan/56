package com.mycampus.backend.player.entity;

import com.mycampus.backend.persistence.IntegerSetConverter;
import jakarta.persistence.*;

import java.time.LocalDateTime;
import java.util.LinkedHashSet;
import java.util.Set;

@Entity
@Table(name = "role", uniqueConstraints = {
        @UniqueConstraint(name = "uk_role_user_slot", columnNames = {"account_code", "slot_no"})
})
public class Role {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "role_id")
    private Long id;

    @Column(name = "account_code", length = 32)
    private String accountCode;

    @Column(name = "school_id", nullable = false)
    private Long schoolId;

    @Column(name = "slot_no", nullable = false)
    private Integer slotNo;

    @Column(name = "nickname", nullable = false, length = 100)
    private String nickName;

    @Column(name = "avatar_url", length = 255)
    private String avatarUrl;

    @Column(name = "status", nullable = false)
    private Integer status = 1;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    @Column(name = "role_code", length = 32)
    private String roleCode;

    @Column(name = "campus_name", length = 128)
    private String campusName;

    @Column(name = "current_character_id")
    private Integer currentCharacterId = 1;

    @Convert(converter = IntegerSetConverter.class)
    @Column(name = "unlocked_character_ids", columnDefinition = "tinytext")
    private Set<Integer> unlockedCharacterIds = new LinkedHashSet<>();

    @PrePersist
    public void prePersist() {
        LocalDateTime now = LocalDateTime.now();
        createdAt = now;
        updatedAt = now;
        if (status == null) {
            status = 1;
        }
    }

    @PreUpdate
    public void preUpdate() {
        updatedAt = LocalDateTime.now();
    }

    public Long getId() {
        return id;
    }

    public String getAccountCode() {
        return accountCode;
    }

    public void setAccountCode(String accountCode) {
        this.accountCode = accountCode;
    }

    public Long getSchoolId() {
        return schoolId;
    }

    public void setSchoolId(Long schoolId) {
        this.schoolId = schoolId;
    }

    public Integer getSlotNo() {
        return slotNo;
    }

    public void setSlotNo(Integer slotNo) {
        this.slotNo = slotNo;
    }

    public String getNickName() {
        return nickName;
    }

    public void setNickName(String nickName) {
        this.nickName = nickName;
    }

    public String getAvatarUrl() {
        return avatarUrl;
    }

    public void setAvatarUrl(String avatarUrl) {
        this.avatarUrl = avatarUrl;
    }

    public Integer getStatus() {
        return status;
    }

    public void setStatus(Integer status) {
        this.status = status;
    }

    public String getRoleCode() {
        return roleCode;
    }

    public void setRoleCode(String roleCode) {
        this.roleCode = roleCode;
    }

    public String getCampusName() {
        return campusName;
    }

    public void setCampusName(String campusName) {
        this.campusName = campusName;
    }

    public Integer getCurrentCharacterId() {
        return currentCharacterId;
    }

    public void setCurrentCharacterId(Integer currentCharacterId) {
        this.currentCharacterId = currentCharacterId;
    }

    public Set<Integer> getUnlockedCharacterIds() {
        return unlockedCharacterIds;
    }

    public void setUnlockedCharacterIds(Set<Integer> unlockedCharacterIds) {
        this.unlockedCharacterIds = unlockedCharacterIds == null ? new LinkedHashSet<>() : unlockedCharacterIds;
    }
}
