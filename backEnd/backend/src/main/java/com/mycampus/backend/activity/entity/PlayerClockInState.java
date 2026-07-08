package com.mycampus.backend.activity.entity;

import com.mycampus.backend.persistence.StringBooleanMapConverter;
import jakarta.persistence.*;

import java.time.LocalDate;
import java.util.LinkedHashMap;
import java.util.Map;

@Entity
@Table(name = "player_clockin_state")
public class PlayerClockInState {

    @Id
    private Long roleId;

    private String eventId;

    private LocalDate lastCheckInDate;

    @Convert(converter = StringBooleanMapConverter.class)
    @Column(nullable = false, columnDefinition = "tinytext")
    private Map<String, Boolean> checkedIn = new LinkedHashMap<>();

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

    public LocalDate getLastCheckInDate() {
        return lastCheckInDate;
    }

    public void setLastCheckInDate(LocalDate lastCheckInDate) {
        this.lastCheckInDate = lastCheckInDate;
    }

    public Map<String, Boolean> getCheckedIn() {
        return checkedIn;
    }

    public void setCheckedIn(Map<String, Boolean> checkedIn) {
        this.checkedIn = checkedIn;
    }
}
