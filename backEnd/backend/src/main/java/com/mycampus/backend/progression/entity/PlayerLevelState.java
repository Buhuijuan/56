package com.mycampus.backend.progression.entity;

import com.mycampus.backend.persistence.IntegerSetConverter;
import jakarta.persistence.*;

import java.util.LinkedHashSet;
import java.util.Set;

@Entity
@Table(name = "player_level_state")
public class PlayerLevelState {

    @Id
    private Long roleId;

    @Column(nullable = false)
    private Integer level;

    @Column(nullable = false)
    private Integer exp;

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> rewardClaimed = new LinkedHashSet<>();

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> boxOpened = new LinkedHashSet<>();

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
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

    public Set<Integer> getRewardClaimed() {
        return rewardClaimed;
    }

    public Set<Integer> getBoxOpened() {
        return boxOpened;
    }
}
