package com.mycampus.backend.player.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.time.LocalDateTime;

@Entity
@Table(name = "player_stat")
public class PlayerStat {

    @Id
    @Column(name = "role_id")
    private Long roleId;

    @Column(name = "npc_distinct_talk_count", nullable = false)
    private Integer npcDistinctTalkCount;

    @Column(name = "animal_interact_count", nullable = false)
    private Integer animalInteractCount;

    @Column(name = "elf_ask_count", nullable = false)
    private Integer elfAskCount;

    @Column(name = "photo_count", nullable = false)
    private Integer photoCount;

    @Column(name = "distinct_photo_location_count", nullable = false)
    private Integer distinctPhotoLocationCount;

    @Column(name = "bike_ride_count", nullable = false)
    private Integer bikeRideCount;

    @Column(name = "quiz_correct_count", nullable = false)
    private Integer quizCorrectCount;

    @Column(name = "morning_checkin_days", nullable = false)
    private Integer morningCheckinDays;

    @Column(name = "story_completed_count", nullable = false)
    private Integer storyCompletedCount;

    @Column(name = "title_unlocked_count", nullable = false)
    private Integer titleUnlockedCount;

    @Column(name = "login_days", nullable = false)
    private Integer loginDays;

    @Column(name = "core_building_reached_count", nullable = false)
    private Integer coreBuildingReachedCount;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    public void touch() {
        updatedAt = LocalDateTime.now();
    }

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Integer getNpcDistinctTalkCount() {
        return npcDistinctTalkCount;
    }

    public void setNpcDistinctTalkCount(Integer npcDistinctTalkCount) {
        this.npcDistinctTalkCount = npcDistinctTalkCount;
    }

    public Integer getAnimalInteractCount() {
        return animalInteractCount;
    }

    public void setAnimalInteractCount(Integer animalInteractCount) {
        this.animalInteractCount = animalInteractCount;
    }

    public Integer getElfAskCount() {
        return elfAskCount;
    }

    public void setElfAskCount(Integer elfAskCount) {
        this.elfAskCount = elfAskCount;
    }

    public Integer getPhotoCount() {
        return photoCount;
    }

    public void setPhotoCount(Integer photoCount) {
        this.photoCount = photoCount;
    }

    public Integer getDistinctPhotoLocationCount() {
        return distinctPhotoLocationCount;
    }

    public void setDistinctPhotoLocationCount(Integer distinctPhotoLocationCount) {
        this.distinctPhotoLocationCount = distinctPhotoLocationCount;
    }

    public Integer getBikeRideCount() {
        return bikeRideCount;
    }

    public void setBikeRideCount(Integer bikeRideCount) {
        this.bikeRideCount = bikeRideCount;
    }

    public Integer getQuizCorrectCount() {
        return quizCorrectCount;
    }

    public void setQuizCorrectCount(Integer quizCorrectCount) {
        this.quizCorrectCount = quizCorrectCount;
    }

    public Integer getMorningCheckinDays() {
        return morningCheckinDays;
    }

    public void setMorningCheckinDays(Integer morningCheckinDays) {
        this.morningCheckinDays = morningCheckinDays;
    }

    public Integer getStoryCompletedCount() {
        return storyCompletedCount;
    }

    public void setStoryCompletedCount(Integer storyCompletedCount) {
        this.storyCompletedCount = storyCompletedCount;
    }

    public Integer getTitleUnlockedCount() {
        return titleUnlockedCount;
    }

    public void setTitleUnlockedCount(Integer titleUnlockedCount) {
        this.titleUnlockedCount = titleUnlockedCount;
    }

    public Integer getLoginDays() {
        return loginDays;
    }

    public void setLoginDays(Integer loginDays) {
        this.loginDays = loginDays;
    }

    public Integer getCoreBuildingReachedCount() {
        return coreBuildingReachedCount;
    }

    public void setCoreBuildingReachedCount(Integer coreBuildingReachedCount) {
        this.coreBuildingReachedCount = coreBuildingReachedCount;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }
}
