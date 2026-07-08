package com.mycampus.backend.task;

public final class TaskStatus {

    public static final String LOCKED = "LOCKED";
    public static final String AVAILABLE = "AVAILABLE";
    public static final String IN_PROGRESS = "IN_PROGRESS";
    public static final String COMPLETED = "COMPLETED";
    public static final String CLAIMED = "CLAIMED";

    private TaskStatus() {
    }
}
