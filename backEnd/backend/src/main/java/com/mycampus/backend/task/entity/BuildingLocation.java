package com.mycampus.backend.task.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.math.BigDecimal;
import java.time.LocalDateTime;

@Entity
@Table(name = "building_location")
public class BuildingLocation {

    @Id
    @Column(name = "building_location_id")
    private Long buildingLocationId;

    @Column(name = "school_id", nullable = false)
    private Long schoolId;

    @Column(name = "building_code", nullable = false, unique = true, length = 64)
    private String buildingCode;

    @Column(name = "building_name", nullable = false, length = 128)
    private String buildingName;

    @Column(name = "posx", nullable = false, precision = 10, scale = 2)
    private BigDecimal posX;

    @Column(name = "posy", nullable = false, precision = 10, scale = 2)
    private BigDecimal posY;

    @Column(name = "posz", nullable = false, precision = 10, scale = 2)
    private BigDecimal posZ;

    @Column(name = "checkin_radius", nullable = false, precision = 10, scale = 2)
    private BigDecimal checkinRadius;

    @Column(name = "status", nullable = false)
    private Integer status;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    public Long getBuildingLocationId() {
        return buildingLocationId;
    }

    public void setBuildingLocationId(Long buildingLocationId) {
        this.buildingLocationId = buildingLocationId;
    }

    public Long getSchoolId() {
        return schoolId;
    }

    public void setSchoolId(Long schoolId) {
        this.schoolId = schoolId;
    }

    public String getBuildingCode() {
        return buildingCode;
    }

    public void setBuildingCode(String buildingCode) {
        this.buildingCode = buildingCode;
    }

    public String getBuildingName() {
        return buildingName;
    }

    public void setBuildingName(String buildingName) {
        this.buildingName = buildingName;
    }

    public BigDecimal getPosX() {
        return posX;
    }

    public void setPosX(BigDecimal posX) {
        this.posX = posX;
    }

    public BigDecimal getPosY() {
        return posY;
    }

    public void setPosY(BigDecimal posY) {
        this.posY = posY;
    }

    public BigDecimal getPosZ() {
        return posZ;
    }

    public void setPosZ(BigDecimal posZ) {
        this.posZ = posZ;
    }

    public BigDecimal getCheckinRadius() {
        return checkinRadius;
    }

    public void setCheckinRadius(BigDecimal checkinRadius) {
        this.checkinRadius = checkinRadius;
    }

    public Integer getStatus() {
        return status;
    }

    public void setStatus(Integer status) {
        this.status = status;
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
