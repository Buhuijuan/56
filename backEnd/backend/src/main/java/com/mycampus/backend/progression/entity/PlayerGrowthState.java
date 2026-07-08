package com.mycampus.backend.progression.entity;

import com.mycampus.backend.persistence.StringSetConverter;
import jakarta.persistence.Column;
import jakarta.persistence.Convert;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import java.util.LinkedHashSet;
import java.util.Set;

@Entity
@Table(name = "player_growth_state")
public class PlayerGrowthState {

    @Id
    private Long roleId;

    @Convert(converter = StringSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<String> stageCompleted = new LinkedHashSet<>();

    @Convert(converter = StringSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<String> rewardClaimed = new LinkedHashSet<>();

    @Convert(converter = StringSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<String> taskCompleted = new LinkedHashSet<>();

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Set<String> getStageCompleted() {
        return stageCompleted;
    }

    public Set<String> getRewardClaimed() {
        return rewardClaimed;
    }

    public Set<String> getTaskCompleted() {
        return taskCompleted;
    }
}
