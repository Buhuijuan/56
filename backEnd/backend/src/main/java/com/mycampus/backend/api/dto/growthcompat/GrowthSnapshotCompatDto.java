package com.mycampus.backend.api.dto.growthcompat;

import java.util.List;

public class GrowthSnapshotCompatDto {

    private GrowthProfileCompatDto profile;
    private List<TaskStateCompatDto> tasks;

    public GrowthProfileCompatDto getProfile() {
        return profile;
    }

    public void setProfile(GrowthProfileCompatDto profile) {
        this.profile = profile;
    }

    public List<TaskStateCompatDto> getTasks() {
        return tasks;
    }

    public void setTasks(List<TaskStateCompatDto> tasks) {
        this.tasks = tasks;
    }
}
