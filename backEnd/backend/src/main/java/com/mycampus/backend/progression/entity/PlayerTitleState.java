package com.mycampus.backend.progression.entity;

import com.mycampus.backend.persistence.IntegerSetConverter;
import jakarta.persistence.*;

import java.util.LinkedHashSet;
import java.util.Set;

@Entity
@Table(name = "player_title_state")
public class PlayerTitleState {

    @Id
    private Long roleId;

    @Convert(converter = IntegerSetConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Set<Integer> unlockedTitleIds = new LinkedHashSet<>();

    @Column(nullable = false)
    private Integer equippedTitleId;

    public Long getRoleId() {
        return roleId;
    }

    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }

    public Set<Integer> getUnlockedTitleIds() {
        return unlockedTitleIds;
    }

    public Integer getEquippedTitleId() {
        return equippedTitleId;
    }

    public void setEquippedTitleId(Integer equippedTitleId) {
        this.equippedTitleId = equippedTitleId;
    }
}
